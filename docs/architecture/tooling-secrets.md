# Tooling and QA secrets

Credentials used by development and QA tooling live in the Azure Key Vault **`kv-vennusign-dev`** (resource group `rg-basic-website`, RBAC authorization). This is separate from the product's own configuration platform described in `configuration-platform.md`, which encrypts application settings in the database and uses Key Vault only to wrap data keys.

Nothing here is stored in the repository, and nothing here should be pasted into issues, pull requests, or chat.

## Why this exists

These values previously existed only in a single developer's home directory, which meant QA tooling worked on exactly one machine and not in CI, and nothing survived that machine being lost. The vault is the durable copy.

## Access

Reading requires an Azure identity with the **Key Vault Secrets User** role on the vault; writing requires **Key Vault Secrets Officer**.

```
az keyvault secret show --vault-name kv-vennusign-dev --name <name> --query value -o tsv
```

## Contents

| Secret | Purpose |
| --- | --- |
| `zoho-mail-client-id` | Zoho Mail API self-client, for disposable QA mailboxes |
| `zoho-mail-client-secret` | as above |
| `zoho-mail-refresh-token` | as above; does not expire, access tokens are minted per run |
| `zoho-mail-dc` | Zoho data centre host (`accounts.zoho.com`) |
| `zoho-mail-zoid` | Zoho organization id, required in the mailbox delete path |
| `qa-murphy-entra-email` | dedicated QA customer account for signed-in UI testing |
| `qa-murphy-entra-password` | as above |
| `sql-dev-server` | dev SQL host, for verifying what actually reached the database |
| `sql-dev-database` | dev database name |
| `sql-dev-username` | dev SQL login |
| `sql-dev-password` | as above |
| `entra-ciam-tenant-id` | customer identity (CIAM) tenant; not secret, but needed for every auth investigation |
| `entra-ciam-client-id` | app registration the API authenticates customers through |
| `entra-ciam-domain` | CIAM sign-in domain |

## Consumers

`tests/ui/lib/zohoMailbox.mjs` reads the `zoho-mail-*` secrets when no local `~/.config/vennusign-zoho.json` is present. The local file takes precedence when it exists, because a developer machine already has it and it avoids a round trip per secret. The fallback shells out to the `az` CLI, so it needs `az` on `PATH` (or `AZ_CLI_PATH`) — true on a Linux CI runner, but not under Windows Node on a WSL machine where `az` is installed only inside WSL.

## Known gaps

- The Zoho client secret, the QA account password, and the dev SQL password were all transmitted in plain text while this tooling was being set up, so they should be treated as exposed and rotated. Storing them here does not undo that.
- No rotation process or expiry monitoring exists. The Zoho refresh token in particular does not expire, so nothing will surface its age.
- CI cannot read the vault yet: no workload identity has been granted Key Vault Secrets User, so `deploy-dev.yml` and any hosted Murphy run still have no access.
