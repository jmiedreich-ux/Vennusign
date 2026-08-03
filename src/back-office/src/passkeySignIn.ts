import type { BackOfficeConfiguration } from "./config";

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

export async function signInWithPasskey(configuration: BackOfficeConfiguration, email: string) {
  if (!window.PublicKeyCredential || !navigator.credentials) throw new Error("This browser does not support passkeys. Use Google, Apple, or email sign-in instead.");
  const optionsResponse = await fetch(`${configuration.apiBaseUrl}/api/customer-auth/strong/passkeys/assertion/options`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email })
  });
  if (!optionsResponse.ok) throw new Error("No usable passkey was found. Check the email or use Google, Apple, or email recovery.");
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
  let credential: PublicKeyCredential | null;
  try { credential = await navigator.credentials.get({ publicKey }) as PublicKeyCredential | null; }
  catch (reason) {
    if (reason instanceof DOMException && reason.name === "NotAllowedError") throw new Error("Passkey sign-in was canceled or timed out. Try again or use another sign-in method.");
    throw new Error("The browser could not use this passkey. Check the secure site address or use account recovery.");
  }
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
  if (!complete.ok) throw new Error(complete.status === 401
    ? "This passkey request expired or could not be verified. Start again or use account recovery."
    : "Passkey sign-in could not be completed. Try again or use another sign-in method.");
}
