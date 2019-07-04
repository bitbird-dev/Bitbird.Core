using Bitbird.Core.CommandLineArguments;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    internal class ConsoleArguments
    {
        [UsedImplicitly, CommandLineArg]
        public bool DataExport { get; set; }

        [UsedImplicitly, CommandLineArg]
        public bool ApiExport { get; set; }
    }
}