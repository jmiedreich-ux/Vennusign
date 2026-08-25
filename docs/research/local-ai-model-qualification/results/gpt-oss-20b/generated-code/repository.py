from .models import Hold


class InMemoryVenueRepository:
    """In-memory store used by the public API and tests."""

    def __init__(self, inventory: dict[str, int]):
        self._inventory = dict(inventory)
        self._holds: dict[str, Hold] = {}

    def available_seats(self, show_id: str) -> int:
        return self._inventory[show_id]

    def get_hold(self, request_id: str) -> Hold | None:
        return self._holds.get(request_id)

    def save_hold(self, hold: Hold) -> None:
        self._holds[hold.request_id] = hold

    def decrement(self, show_id: str, quantity: int) -> None:
        self._inventory[show_id] -= quantity
