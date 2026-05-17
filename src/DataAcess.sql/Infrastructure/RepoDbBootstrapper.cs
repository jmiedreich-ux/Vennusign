using RepoDb;
using System.Threading;

namespace Vennu.DataAccess.Infrastructure;

internal static class RepoDbBootstrapper
{
    private static int initialized;

    public static void Initialize()
    {
        if (Interlocked.Exchange(ref initialized, 1) == 1)
        {
            return;
        }

#pragma warning disable CS0618
        SqlServerBootstrap.Initialize();
#pragma warning restore CS0618
    }
}
