using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools.NetCore
{
    [UsedImplicitly]
    public static class Program
    {
        [UsedImplicitly]
        public static Task Main([NotNull] string[] args)
        {
            return CliTools.Program.Main(args);
        }
    }
}
