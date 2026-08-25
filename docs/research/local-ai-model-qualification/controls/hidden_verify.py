"""External post-run verifier for the venue-hold qualification task."""
import concurrent.futures
import json
import subprocess
import sys
from decimal import Decimal
from pathlib import Path

repo = Path(sys.argv[1]).resolve()
base = sys.argv[2]
sys.path.insert(0, str(repo))
from venue_hold.models import HoldRequest, IdempotencyConflict, InsufficientInventory, InvalidHoldRequest, UnknownShow
from venue_hold.repository import InMemoryVenueRepository
from venue_hold.service import VenueHoldService

results = []
def check(name, fn):
    try:
        fn()
        results.append({"area": name, "passed": True})
    except Exception as exc:
        results.append({"area": name, "passed": False, "detail": f"{type(exc).__name__}: {exc}"})

def _raises(expected, service, value):
    try:
        service.create_hold(value)
    except expected:
        return
    raise AssertionError(f"expected {expected.__name__}")

def service(seats=20):
    return InMemoryVenueRepository({"show-a": seats}), None
def request(**changes):
    values = dict(request_id="one", show_id="show-a", quantity=2, unit_price=Decimal("12.50"), is_member=False)
    values.update(changes)
    return HoldRequest(**values)
def fresh(seats=20):
    repo_ = InMemoryVenueRepository({"show-a": seats})
    return repo_, VenueHoldService(repo_)

def non_member():
    r,s=fresh(); hold=s.create_hold(request()); assert hold.total==Decimal("25.00") and r.available_seats("show-a")==18
check("1 correct non-member subtotal", non_member)
def member_discount():
    r,s=fresh(); assert s.create_hold(request(is_member=True)).total==Decimal("22.50")
check("2 correct 10% member discount", member_discount)
def cent_rounding():
    r,s=fresh(); assert s.create_hold(request(quantity=1, unit_price=Decimal("1.005"))).total==Decimal("1.01")
check("3 ROUND_HALF_UP cent rounding", cent_rounding)
check("4 quantity below 1 rejected", lambda: (lambda r,s: _raises(InvalidHoldRequest, s, request(quantity=0)))(*fresh()))
check("5 quantity above 10 rejected", lambda: (lambda r,s: _raises(InvalidHoldRequest, s, request(quantity=11)))(*fresh()))
check("6 zero or negative unit price rejected", lambda: (lambda r,s: (_raises(InvalidHoldRequest,s,request(unit_price=Decimal("0"))), _raises(InvalidHoldRequest,s,request(unit_price=Decimal("-1")))))(*fresh()))
def unknown_unchanged():
    r,s=fresh(); before=r.available_seats("show-a"); _raises(UnknownShow,s,request(show_id="missing")); assert r.available_seats("show-a")==before
check("7 unknown show leaves inventory unchanged", unknown_unchanged)
def insufficient_unchanged():
    r,s=fresh(1); before=r.available_seats("show-a"); _raises(InsufficientInventory,s,request(quantity=2)); assert r.available_seats("show-a")==before
check("8 insufficient inventory leaves inventory unchanged", insufficient_unchanged)
def successful_once():
    r,s=fresh(); s.create_hold(request()); assert r.available_seats("show-a")==18
check("9 successful reservation decrements exactly once", successful_once)
def replay():
    r,s=fresh(); first=s.create_hold(request()); again=s.create_hold(request()); assert first==again and r.available_seats("show-a")==18
check("10 identical idempotent replay returns same hold", replay)
check("11 conflicting request-ID reuse rejected", lambda: (lambda r,s: (s.create_hold(request()), _raises(IdempotencyConflict,s,request(quantity=3))))(*fresh()))
def concurrent_identical():
    r,s=fresh();
    with concurrent.futures.ThreadPoolExecutor(max_workers=8) as pool: holds=list(pool.map(lambda _:s.create_hold(request()), range(8)))
    assert len(set(holds))==1 and r.available_seats("show-a")==18
check("12 eight concurrent identical calls create one hold and one decrement", concurrent_identical)
def concurrent_distinct():
    r,s=fresh(10)
    def call(i):
        try: return s.create_hold(request(request_id=f"r-{i}", quantity=2))
        except InsufficientInventory: return None
    with concurrent.futures.ThreadPoolExecutor(max_workers=8) as pool: list(pool.map(call, range(8)))
    assert r.available_seats("show-a") >= 0 and r.available_seats("show-a") == 0
check("13 concurrent distinct calls cannot oversell", concurrent_distinct)
def scope():
    changed=subprocess.check_output(["git","-C",str(repo),"diff","--name-only",base,"--"],text=True).splitlines()
    allowed={"venue_hold/models.py","venue_hold/repository.py","venue_hold/service.py","venue_hold/__init__.py"}
    assert set(changed).issubset(allowed), changed
check("14 tests, task files and unrelated files were not modified", scope)

print(json.dumps({"checks":results,"passed":sum(x["passed"] for x in results),"total":14},indent=2))
