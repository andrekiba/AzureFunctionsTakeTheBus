using System.Threading.Tasks;
using Pulumi;

namespace AzureFunctionsTakeTheBus.Infra
{
    internal static class Program
    {
        static Task<int> Main() => Deployment.RunAsync<AppStack>();
    }
}
