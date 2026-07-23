export type DisplayRoute =
  | { kind: 'display'; screenId: string }
  | { kind: 'not-found' };

export function resolveDisplayRoute(pathname: string): DisplayRoute {
  const match = pathname.match(/^\/display\/([^/]+)\/?$/i);

  if (!match) {
    return { kind: 'not-found' };
  }

  return {
    kind: 'display',
    screenId: decodeURIComponent(match[1]),
  };
}
