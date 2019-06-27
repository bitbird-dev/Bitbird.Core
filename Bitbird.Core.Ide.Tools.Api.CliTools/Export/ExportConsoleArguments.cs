using Bitbird.Core.CommandLineArguments;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    internal class ExportConsoleArguments
    {
        [UsedImplicitly, NotNull, CommandLineArg(true)]
        public string ExportInputBinPath { get; set; } = "Data.dll";

        [UsedImplicitly, NotNull, CommandLineArg(true)]
        public string ExportOutput { get; set; } = "dataModel.json";

        [UsedImplicitly, NotNull, CommandLineArg]
        public string ExportDbModelPostfix { get; set; } = "DbModel";
    }
}