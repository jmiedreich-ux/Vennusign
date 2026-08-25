public sealed record ProviderIdentity(string Provider, string Subject, string? Email, bool EmailVerified, string AccessToken);

public interface IAccountRepository
{
    Task<Account?> FindByProviderAsync(string provider, string subject, CancellationToken cancellationToken);
    Task<Account?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task UpdateAsync(Account account, CancellationToken cancellationToken);
    Task InsertProviderBindingAsync(Guid accountId, string provider, string subject, CancellationToken cancellationToken);
}

public sealed class Account
{
    public Guid Id { get; init; }
    public string Provider { get; set; } = "";
    public string ProviderSubject { get; set; } = "";
    public string Email { get; set; } = "";
}
