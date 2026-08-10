namespace Vennu.Data.IntegrationTests.Fixtures;

/// <summary>
/// Runs <see cref="ModelInvariants"/> after every test in the class.
///
/// xUnit builds a new instance of a test class per test, so <c>DisposeAsync</c> fires
/// once per test with nothing for the author to call. That is the whole point: a check
/// somebody has to remember is a check that goes missing on the day it would have
/// mattered.
///
/// A test does not have to know this is happening, and should not have to. If it
/// leaves the model in a shape the product says is impossible, it fails - whatever it
/// was written to prove.
/// </summary>
[Trait("Category", "Integration")]
public abstract class InvariantCheckedTests(DatabaseFixture fixture) : IAsyncLifetime
{
    public virtual Task InitializeAsync() => fixture.ResetTablesAsync();

    public async Task DisposeAsync()
    {
        await OnDisposingAsync().ConfigureAwait(false);
        await ModelInvariants.AssertAllAsync(fixture, GetType().Name).ConfigureAwait(false);
    }

    /// <summary>Cleanup a derived class needs before the invariants are checked.</summary>
    protected virtual Task OnDisposingAsync() => Task.CompletedTask;
}
