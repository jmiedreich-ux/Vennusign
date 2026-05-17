Where are we in the project? 
	1. We are in the early stages of development, focused on completing the Phase 02 backend foundation before Phase 03 feature/tier work.
	2. We are currently in Phase 02. Reference `Vennu_Overview_v5.md` and `Vennu_Roadmap_v5.md` for the detailed plan.


Current Structure / Decisions:
	1. `Vennu.Api` = API and startup wiring.
	2. `Vennu.Data` = Vennu-specific data layer, DbUp migrations, models, repository interfaces, repository implementations.
	3. `Vennu.DataAccess` = provider layer only. This project should stay generic so the provider can be swapped later.
	4. `src/display` = Vite + React + TypeScript display SPA. It should stay in the repo, but should NOT be added to the Visual Studio solution as a Website project.
	5. The current workspace targets `.NET 9`.

Current Phase 02 Progress:
	1. Created initial project structure under `src`.
	2. Wired `Program.cs` to run `DatabaseMigrator` before app startup.
	3. Added initial SQL scripts:
		- `001_create_venues.sql`
		- `002_create_screens.sql`
		- `003_create_screen_pairing_codes.sql`
	4. Added initial models:
		- `Venue`
		- `Screen`
		- `ScreenPairingCode`
	5. Added initial repository interfaces and implementations for the above entities.
	6. Moved Vennu-specific repository code out of `Vennu.DataAccess` and into `Vennu.Data`.
	7. Modernized `Vennu.DataAccess` into a generic RepoDb-based provider with async-friendly contracts.

Known Issue / Cleanup:
	1. The current solution build can fail if `display` is included as a Visual Studio Website project.
	2. `display` should be removed from the solution if it was added as a Website project.
	3. Run the display app separately with npm from `src/display`.

Azure SQL Local Development Notes:
	1. Local development should use an Azure SQL development database, never production.
	2. Use separate databases for shared dev, automated/integration testing, and production.
	3. Keep the real connection string in user secrets or environment variables.
	4. Keep schema changes in DbUp scripts only; do not make manual schema edits in shared environments.
	5. Expect cloud latency locally and validate connection resiliency/timeouts as part of development.
	6. Prefer Microsoft Entra auth where practical; otherwise use a dedicated SQL login stored as a secret.

What is next?
	1. Build the first API endpoints for Phase 02:
		- `POST /api/venues`
		- `POST /api/screens`
		- `POST /api/screens/pairing-code`
		- `GET /api/screens/pairing/{code}/status`
		- `POST /api/screens/pairing/{code}/claim`
	2. Add request/response DTOs and validation.
	3. Add screen key generation logic using format `sc-{6 chars}`.
	4. Add heartbeat endpoint and status update flow using `POST /api/display/{screenId}/heartbeat`.
	5. Add SignalR hub scaffolding after the core screen endpoints are in place.
	6. Validate the first end-to-end Phase 02 slice with two browser tabs before moving to Phase 03.


Coding Rules:	
	1. Follow the coding standards and guidelines established for the project.
	2. Write clean, maintainable, and well-documented code.
	3. Use meaningful variable and function names to enhance readability.
	4. Implement error handling and input validation where necessary.
	5. Ensure that the code is modular and reusable to promote scalability.
	6. Conduct regular code reviews to maintain code quality and consistency across the team.
	7. Keep constructor definitions on one line, and avoid using multiple lines for constructors.
