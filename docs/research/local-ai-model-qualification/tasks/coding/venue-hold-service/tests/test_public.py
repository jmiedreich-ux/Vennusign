import unittest
from decimal import Decimal

from venue_hold.models import HoldRequest, InsufficientInventory, InvalidHoldRequest
from venue_hold.repository import InMemoryVenueRepository
from venue_hold.service import VenueHoldService


class VenueHoldPublicTests(unittest.TestCase):
    def setUp(self):
        self.repository = InMemoryVenueRepository({"show-a": 12})
        self.service = VenueHoldService(self.repository)

    def request(self, **overrides):
        values = {"request_id": "r-1", "show_id": "show-a", "quantity": 2, "unit_price": Decimal("12.50"), "is_member": False}
        values.update(overrides)
        return HoldRequest(**values)

    def test_creates_non_member_hold(self):
        hold = self.service.create_hold(self.request())
        self.assertEqual(hold.total, Decimal("25.00"))
        self.assertEqual(self.repository.available_seats("show-a"), 10)

    def test_member_receives_discount(self):
        hold = self.service.create_hold(self.request(is_member=True))
        self.assertEqual(hold.total, Decimal("22.50"))

    def test_invalid_quantity_is_rejected(self):
        with self.assertRaises(InvalidHoldRequest):
            self.service.create_hold(self.request(quantity=0))

    def test_insufficient_inventory_is_rejected(self):
        with self.assertRaises(InsufficientInventory):
            self.service.create_hold(self.request(quantity=20))


if __name__ == "__main__":
    unittest.main()
