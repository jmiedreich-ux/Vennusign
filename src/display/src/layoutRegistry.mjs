export const fallbackLayoutKey = 'default';

export function normalizeLayoutKey(value) {
  if (typeof value !== 'string') {
    return fallbackLayoutKey;
  }

  const normalized = value.trim().toLowerCase().replaceAll('-', '_').replaceAll(' ', '_');
  return normalized || fallbackLayoutKey;
}

export function createLayoutRegistry(registrations, fallbackKey = fallbackLayoutKey) {
  const normalizedFallbackKey = normalizeLayoutKey(fallbackKey);
  const layouts = new Map();

  for (const registration of registrations) {
    const key = normalizeLayoutKey(registration.key);

    if (layouts.has(key)) {
      throw new Error(`Display layout '${key}' is registered more than once.`);
    }

    layouts.set(key, Object.freeze({ ...registration, key }));
  }

  if (!layouts.has(normalizedFallbackKey)) {
    throw new Error(`Fallback display layout '${normalizedFallbackKey}' is not registered.`);
  }

  return Object.freeze({
    keys: Object.freeze([...layouts.keys()]),
    resolve(requestedKey) {
      const normalizedRequestedKey = normalizeLayoutKey(requestedKey);
      const matched = layouts.get(normalizedRequestedKey);

      if (matched) {
        return Object.freeze({
          requestedKey: normalizedRequestedKey,
          key: matched.key,
          isFallback: false,
          registration: matched
        });
      }

      const fallback = layouts.get(normalizedFallbackKey);
      return Object.freeze({
        requestedKey: normalizedRequestedKey,
        key: fallback.key,
        isFallback: true,
        registration: fallback
      });
    }
  });
}
