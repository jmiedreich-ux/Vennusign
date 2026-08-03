namespace Vennu.DevControl.Tests;

public sealed class RootAdminKeyBatchTests
{
    [Fact]
    public void RootBatchCopiesExistingKeyThroughPowerShellHelper()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "get-super-admin-access-key.cmd")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        var batch = File.ReadAllText(Path.Combine(directory.FullName, "get-super-admin-access-key.cmd"));
        Assert.Contains("scripts\\set-super-admin-key.ps1", batch, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("-ReuseExisting", batch, StringComparison.Ordinal);
        Assert.DoesNotContain("echo %", batch, StringComparison.OrdinalIgnoreCase);
    }
}
