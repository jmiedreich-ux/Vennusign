from dataclasses import dataclass
from decimal import Decimal


class VenueHoldError(Exception):
    """Base domain error for ticket holds."""


class InvalidHoldRequest(VenueHoldError):
    """The hold request has invalid quantity or price."""


class UnknownShow(VenueHoldError):
    """The requested show does not exist."""


class InsufficientInventory(VenueHoldError):
    """The requested show does not have enough available seats."""


class IdempotencyConflict(VenueHoldError):
    """A request id has already been used for a different hold request."""


@dataclass(frozen=True)
class HoldRequest:
    request_id: str
    show_id: str
    quantity: int
    unit_price: Decimal
    is_member: bool


@dataclass(frozen=True)
class Hold:
    request_id: str
    show_id: str
    quantity: int
    unit_price: Decimal
    is_member: bool
    total: Decimal
