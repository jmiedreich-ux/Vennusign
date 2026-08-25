from decimal import Decimal

from .models import Hold, HoldRequest, InsufficientInventory, InvalidHoldRequest
from .repository import InMemoryVenueRepository


class VenueHoldService:
    def __init__(self, repository: InMemoryVenueRepository):
        self._repository = repository

    def create_hold(self, request: HoldRequest) -> Hold:
        if request.quantity < 1:
            raise InvalidHoldRequest("quantity must be positive")
        if request.unit_price <= 0:
            raise InvalidHoldRequest("unit price must be positive")
        if self._repository.available_seats(request.show_id) < request.quantity:
            raise InsufficientInventory("not enough seats")
        total = request.unit_price * request.quantity
        if request.is_member:
            total -= total * Decimal("0.10")
        hold = Hold(request.request_id, request.show_id, request.quantity, request.unit_price, request.is_member, total)
        self._repository.decrement(request.show_id, request.quantity)
        self._repository.save_hold(hold)
        return hold
