# Back Office Organization and Venue Context

Back Office treats organization and venue selection as authenticated server state, not as browser authorization. The browser may remember the last accepted venue ID for continuity, but the customer-session authentication handler re-resolves the venue, organization, active memberships, manage-content capability, and effective features on every request.

`GET /api/back-office/session` is the context bootstrap and switch-validation boundary. Its response separates the signed-in account from the active organization and venue and returns only contexts the account can currently manage. A requested `X-Vennusign-Venue-Id` is accepted only when those checks pass. The client saves the returned venue ID after success; a rejected or stale saved ID is removed and the server-selected onboarding venue is retried.

Legacy access tokens remain bound to their configured venue and receive a single non-switchable context. Context changes remount venue-data screens and refresh billing presentation so state from the previous tenant is not reused. Changes to membership or venue ownership take effect on the next request; revoked access fails closed.
