using Bitbird.Core.CommandLineArguments;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    internal class ApiExportConsoleArguments
    {
        [UsedImplicitly, NotNull, CommandLineArg(true)]
        public string ApiExportInputBinPath { get; set; } = "Api.dll";

        [UsedImplicitly, NotNull, CommandLineArg]
        public string ApiExportOutput { get; set; } = "apiModel.json";

        [UsedImplicitly, NotNull, CommandLineArg]
        public string ApiExportNodePostfix { get; set; } = "Node";
    }
}