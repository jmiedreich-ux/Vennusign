type DisplayPageProps = {
  screenId: string;
};

export default function DisplayPage({ screenId }: DisplayPageProps) {
  return (
    <main>
      <h1>Vennu Display</h1>
      <p>Display foundation is ready.</p>
      <dl>
        <dt>Screen ID</dt>
        <dd data-testid="screen-id">{screenId}</dd>
      </dl>
    </main>
  );
}
