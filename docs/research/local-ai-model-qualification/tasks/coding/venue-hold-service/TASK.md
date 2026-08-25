# Venue hold workflow

Implement the ticket-hold workflow in this repository. Work only in production code; do not modify `TASK.md` or anything in `tests/`.

The public API is `VenueHoldService.create_hold(HoldRequest)`. A request has `request_id`, `show_id`, `quantity`, positive `Decimal` `unit_price`, and membership status. Public APIs and domain errors must remain clearly typed.

Required behavior:

1. Quantity is 1 through 10 inclusive and unit price is a positive `Decimal`.
2. An unknown show raises `UnknownShow` without changing inventory. Insufficient seats raise `InsufficientInventory` without a partial change.
3. A successful hold decreases available seats exactly once.
4. Replaying an identical request ID and payload returns the original hold without another decrement. Reusing that ID with a different show, quantity, price, or membership raises `IdempotencyConflict`.
5. Members receive a 10% subtotal discount. All money uses `Decimal`, rounded to two places with `ROUND_HALF_UP`.
6. Concurrent identical calls create one hold and one decrement. Concurrent distinct calls cannot oversell.
7. Run `python -m unittest -v` when finished.

You may add production files when justified. The completed implementation must modify production behavior across at least two of `models.py`, `repository.py`, and `service.py`.
