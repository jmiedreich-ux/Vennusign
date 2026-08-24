// #769: deliberately screen-only. A display that also joined `venue:{id}` (the
// hub's JoinVenue) would receive every draft edit in the builder - section and
// item add/rename/delete/reorder/move - none of which may reach a screen before
// a publish (decisions 1 and 2). See ContentService.NotifyAsync for the audit of
// what venue-scoped notifies actually carry and why none of them belong here.
async function joinScreen(connection, screenId) {
  await connection.invoke('JoinScreen', screenId);
}

export async function startDisplayConnection(connection, screenId, onStateChanged) {
  onStateChanged('connecting');

  connection.onreconnecting(() => onStateChanged('reconnecting'));
  connection.onreconnected(async () => {
    try {
      await joinScreen(connection, screenId);
      onStateChanged('connected');
    } catch {
      onStateChanged('degraded');
    }
  });
  connection.onclose(() => onStateChanged('degraded'));

  try {
    await connection.start();
    await joinScreen(connection, screenId);
    onStateChanged('connected');
  } catch {
    onStateChanged('degraded');
  }

  return {
    stop: () => connection.stop()
  };
}
