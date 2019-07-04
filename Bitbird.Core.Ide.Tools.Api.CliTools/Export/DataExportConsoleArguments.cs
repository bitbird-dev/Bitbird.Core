using Bitbird.Core.CommandLineArguments;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    internal class DataExportConsoleArguments
    {
        [UsedImplicitly, NotNull, CommandLineArg(true)]
        public string DataExportInputBinPath { get; set; } = "Data.dll";

        [UsedImplicitly, NotNull, CommandLineArg]
        public string DataExportOutput { get; set; } = "dataModel.json";

        [UsedImplicitly, NotNull, CommandLineArg]
        public string DataExportDbModelPostfix { get; set; } = "DbModel";
    }
}