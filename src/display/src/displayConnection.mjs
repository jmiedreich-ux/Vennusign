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
