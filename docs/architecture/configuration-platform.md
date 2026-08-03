# Environment-Scoped Configuration Platform

## Bootstrap boundary

The database provider is enabled only when `VENU_CONFIGURATION_ENVIRONMENT` is present. Supported values are `Development`, `Test`, `Staging`, and `Production`.

Bootstrap values remain outside the database:

- `VENU_CONFIGURATION_ENVIRONMENT`
- `VENU_CONFIGURATION_CONNECTION_STRING` or `ConnectionStrings__VennuDatabase`
- `VENU_CONFIGURATION_KEY_PROVIDER` (`Environment` or `AzureKeyVault`)
- `VENU_CONFIGURATION_LOCAL_KEY` for local development
- `VENU_CONFIGURATION_KEY_ID` for an Azure Key Vault RSA key
- emergency environment-variable and command-line overrides

Environment variables and command-line values override database values. Database values override appsettings. If the provider cannot perform its initial load, startup fails. After startup, transient reload failures retain the last successful in-memory snapshot.

## Local development

Generate a local AES-256 key once and store it as a user environment variable:

```powershell
$key = New-Object byte[] 32
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try { $rng.GetBytes($key) } finally { $rng.Dispose() }
[Environment]::SetEnvironmentVariable("VENU_CONFIGURATION_LOCAL_KEY", [Convert]::ToBase64String($key), "User")
[Environment]::SetEnvironmentVariable("VENU_CONFIGURATION_KEY_PROVIDER", "Environment", "User")
[Environment]::SetEnvironmentVariable("VENU_CONFIGURATION_ENVIRONMENT", "Development", "User")
```

Supply the database connection through the existing `ConnectionStrings__VennuDatabase` bootstrap variable or `VENU_CONFIGURATION_CONNECTION_STRING`. Restart the API after changing user environment variables.

For first-time setup, provide `SuperAdmin__ApiKey` as a user environment variable. Use that bootstrap access to store the encrypted `SuperAdmin:ApiKey` value in the Development environment, restart the API, and then remove the bootstrap override if desired.

## Hosted environments

Use `VENU_CONFIGURATION_KEY_PROVIDER=AzureKeyVault`, set `VENU_CONFIGURATION_KEY_ID` to a versioned or versionless RSA key URI, and grant the workload identity wrap/unwrap permissions. Each secret uses a random AES-256 data key; only the Key Vault-wrapped data key and AES-GCM ciphertext are stored.

## Management

Super Admin `Configuration` supports environment/application filtering, typed values, write-only secret replacement/clear, restart notices, secret-safe export, dry-run import, conflict review, and atomic apply. Secrets are never returned or included in standard exports.

The page also reports provider load health, last successful load, sanitized failure type, secret replacement age, and 90-day rotation reminders. Revision history exposes metadata and fingerprints only; secret payloads remain protected and are never returned. Rollback copies the selected stored payload into a new immutable revision and audit record rather than rewriting history.

## Recovery

- Initial provider load failure stops startup so an instance cannot silently run with missing security configuration.
- A transient reload failure retains the last successful in-memory snapshot and marks provider health unhealthy.
- Correct the database/Key Vault condition, then wait for the reload interval or restart the application.
- Use an environment-variable override for emergency recovery; remove it after the database value is corrected.
- A stale edit, import preview, or rollback returns conflict and must be reloaded before retrying.
- Rotate any credential that was previously committed or otherwise exposed; database encryption does not retroactively protect repository history.

## Migrated settings

Registered API definitions cover customer Google/Apple/email authentication, Stripe revenue/webhook/checkout/portal settings, Square OAuth/webhooks, Toast catalog/inventory/polling/webhooks, Clover OAuth/catalog/webhooks, and the Super Admin key. Existing options consumers require no alternate code path because the provider participates in normal .NET configuration binding.
