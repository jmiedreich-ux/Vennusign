from decimal import ROUND_HALF_UP, Decimal

import threading

from .models import (
    Hold, HoldRequest, IdempotencyConflict, InsufficientInventory, UnknownShow, InvalidHoldRequest
)
from .repository import InMemoryVenueRepository


class VenueHoldService:
    def __init__(self, repository: InMemoryVenueRepository):
        self._repository = repository
        self._lock = threading.Lock()

    def create_hold(self, request: HoldRequest) -> Hold:
        if request.quantity < 1:
            raise InvalidHoldRequest("quantity must be positive")
        if request.unit_price <= 0:
            raise InvalidHoldRequest("unit price must be positive")

        with self._lock:
            existing_hold = self._repository.get_hold(request.request_id)
            if existing_hold is not None:
                if existing_hold.show_id != request.show_id or \
                   existing_hold.quantity != request.quantity or \
                   existing_hold.unit_price != request.unit_price or \
                   existing_hold.is_member != request.is_member:
                    raise IdempotencyConflict("request id already used for different hold")
                return existing_hold

            if request.show_id not in self._repository._inventory:
                raise UnknownShow("show does not exist")

            available = self._repository.available_seats(request.show_id)
            if available < request.quantity:
                raise InsufficientInventory("not enough seats")

            total = (request.unit_price * request.quantity) * (Decimal("0.90") if request.is_member else Decimal("1"))
            hold = Hold(
                request.request_id, request.show_id, request.quantity, request.unit_price, 
                request.is_member, total.quantize(Decimal("0.01"), rounding=ROUND_HALF_UP)
            )
            self._repository.decrement_and_save_hold(request.show_id, request.quantity, hold)
            return hold
