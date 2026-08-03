# Google Customer Signup

## Local prerequisites

1. Trust the ASP.NET Core development certificate with `dotnet dev-certs https --trust`.
2. Build the API and Vennu Development Control once before using their `--no-build` launch commands.
3. Open `https://localhost:5174` once and accept the Vite development-certificate warning.
4. Use Vennu Development Control to run API at `https://localhost:7138` and Venue Admin at `https://localhost:5174`.

## Google Cloud Console

Create an OAuth 2.0 Client ID with application type **Web application**.

- Authorized JavaScript origin: `https://localhost:5174`
- Authorized redirect URI: `https://localhost:7138/signin-customer-google`

Production uses the deployed Venue Admin origin and the API callback origin instead. Never commit the client secret.

## Development configuration

In Super Admin ? Configuration, select `Development` and configure:

- `CustomerAuthentication:FrontendOrigin` = `https://localhost:5174`
- `CustomerAuthentication:Google:Enabled` = `true`
- `CustomerAuthentication:Google:ClientId` = the Google web client ID
- `CustomerAuthentication:Google:ClientSecret` = the Google web client secret

Save all values and restart API because these settings are read during authentication registration. The secret remains write-only and encrypted by the configuration provider.

## Flow

Venue Admin sends a local path such as `/onboarding` to the API. The API accepts only a bounded local path, stores the configured trusted frontend origin in protected authentication state, and redirects Google back to `/signin-customer-google` on the HTTPS API origin. After verified-email identity resolution and persisted session issuance, the callback returns to the trusted Venue Admin origin plus that local path.

## Troubleshooting

- `503`: Google is disabled or its client ID/secret is not configured; restart API after saving.
- `redirect_uri_mismatch`: the Google Console redirect URI must exactly match `https://localhost:7138/signin-customer-google`.
- Certificate warning or failed callback: trust the ASP.NET certificate and accept the Venue Admin development certificate before starting signup.
- Correlation/nonce error: ensure the flow starts and finishes on HTTPS and clear stale localhost cookies before retrying.
- Successful Google callback followed by no session: verify Venue Admin is `https://localhost:5174`, API is `https://localhost:7138`, and the browser is allowing the secure localhost cookies.
