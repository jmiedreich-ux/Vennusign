import type { BackOfficeConfiguration } from "./config";

export type PasskeySummary = { id: string; displayName: string; createdUtc: string; lastUsedUtc?: string };

function decode(value: string): ArrayBuffer {
  const normalized = value.replace(/-/g, "+").replace(/_/g, "/");
  const binary = atob(normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "="));
  return Uint8Array.from(binary, character => character.charCodeAt(0)).buffer;
}

function encode(value: ArrayBuffer | null): string | null {
  if (!value) return null;
  const binary = String.fromCharCode(...new Uint8Array(value));
  return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

async function request<T>(configuration: BackOfficeConfiguration, path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/customer-auth/strong${path}`, { ...init, credentials: "include" });
  if (!response.ok) {
    const detail = await response.text();
    if (response.status === 428) throw new Error("Recent authentication is required. Sign out and sign in again before changing passkeys.");
    throw new Error(detail || "Vennusign could not update passkey security.");
  }
  return response.status === 204 ? undefined as T : response.json() as Promise<T>;
}

export const listPasskeys = (configuration: BackOfficeConfiguration) => request<PasskeySummary[]>(configuration, "/passkeys");

export const renamePasskey = (configuration: BackOfficeConfiguration, id: string, displayName: string) =>
  request<void>(configuration, `/passkeys/${encodeURIComponent(id)}`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ displayName }) });

export const removePasskey = (configuration: BackOfficeConfiguration, id: string) =>
  request<void>(configuration, `/passkeys/${encodeURIComponent(id)}`, { method: "DELETE" });

export async function registerPasskey(configuration: BackOfficeConfiguration, displayName: string) {
  if (!window.PublicKeyCredential || !navigator.credentials) throw new Error("This browser does not support passkeys. Use a current browser or keep email sign-in available.");
  const challenge = await request<{ challengeId: string; options: any }>(configuration, "/passkeys/registration/options", { method: "POST" });
  const options: PublicKeyCredentialCreationOptions = {
    ...challenge.options,
    challenge: decode(challenge.options.challenge),
    user: { ...challenge.options.user, id: decode(challenge.options.user.id) },
    excludeCredentials: challenge.options.excludeCredentials?.map((item: any) => ({ ...item, id: decode(item.id) }))
  };
  let credential: PublicKeyCredential | null;
  try { credential = await navigator.credentials.create({ publicKey: options }) as PublicKeyCredential | null; }
  catch (reason) {
    if (reason instanceof DOMException && reason.name === "NotAllowedError") throw new Error("Passkey setup was canceled or timed out. Try again when your authenticator is ready.");
    if (reason instanceof DOMException && reason.name === "InvalidStateError") throw new Error("That authenticator is already registered. Use another passkey or rename the existing one.");
    throw new Error("The browser could not create this passkey. Check browser support and the secure site address.");
  }
  if (!credential) throw new Error("Passkey setup was canceled.");
  const response = credential.response as AuthenticatorAttestationResponse;
  await request<void>(configuration, "/passkeys/registration/complete", {
    method: "POST", headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ challengeId: challenge.challengeId, displayName, response: {
      id: credential.id, rawId: encode(credential.rawId), type: credential.type,
      response: { attestationObject: encode(response.attestationObject), clientDataJSON: encode(response.clientDataJSON), transports: response.getTransports?.() ?? [] },
      clientExtensionResults: credential.getClientExtensionResults()
    } })
  });
}
