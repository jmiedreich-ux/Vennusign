import type { VenueAdminConfiguration } from "./config";

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

export async function signInWithPasskey(configuration: VenueAdminConfiguration, email: string) {
  if (!window.PublicKeyCredential || !navigator.credentials) throw new Error("Passkeys are not supported by this browser.");
  const optionsResponse = await fetch(`${configuration.apiBaseUrl}/api/customer-auth/strong/passkeys/assertion/options`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email })
  });
  if (!optionsResponse.ok) throw new Error("No passkey is available for that account.");
  const challenge = await optionsResponse.json() as {
    challengeId: string;
    options: Omit<PublicKeyCredentialRequestOptions, "challenge" | "allowCredentials"> & {
      challenge: string;
      allowCredentials?: Array<{ id: string; type: PublicKeyCredentialType; transports?: AuthenticatorTransport[] }>;
    };
  };
  const publicKey: PublicKeyCredentialRequestOptions = {
    ...challenge.options,
    challenge: decode(challenge.options.challenge),
    allowCredentials: challenge.options.allowCredentials?.map(item => ({ ...item, id: decode(item.id) }))
  };
  const credential = await navigator.credentials.get({ publicKey }) as PublicKeyCredential | null;
  if (!credential) throw new Error("Passkey sign-in was canceled.");
  const response = credential.response as AuthenticatorAssertionResponse;
  const complete = await fetch(`${configuration.apiBaseUrl}/api/customer-auth/strong/passkeys/assertion/complete`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      challengeId: challenge.challengeId,
      response: {
        id: credential.id,
        rawId: encode(credential.rawId),
        type: credential.type,
        response: {
          authenticatorData: encode(response.authenticatorData),
          clientDataJSON: encode(response.clientDataJSON),
          signature: encode(response.signature),
          userHandle: encode(response.userHandle)
        },
        clientExtensionResults: credential.getClientExtensionResults()
      }
    })
  });
  if (!complete.ok) throw new Error("Passkey sign-in could not be completed.");
}
