export type DisplayRoute =
  | { kind: 'display'; screenId: string }
  | { kind: 'pair' }
  | { kind: 'provision' }
  | { kind: 'not-found' };

export function resolveDisplayRoute(pathname: string): DisplayRoute {
  if (/^\/pair\/?$/i.test(pathname)) {
    return { kind: 'pair' };
  }

  if (/^\/provision\/?$/i.test(pathname)) {
    return { kind: 'provision' };
  }

  const match = pathname.match(/^\/display\/([^/]+)\/?$/i);

  if (!match) {
    return { kind: 'not-found' };
  }

  return {
    kind: 'display',
    screenId: decodeURIComponent(match[1]),
  };
}
