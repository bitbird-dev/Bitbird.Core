using System;
using System.Reflection;
using System.Threading.Tasks;
using Bitbird.Core.CommandLineArguments;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public static class Program
    {
        [NotNull, UsedImplicitly]
        public static async Task Main([NotNull] string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            try
            {
                var parsedArgs = args.ParseArgs();

                if (!parsedArgs
                    .Extract(out ConsoleArguments consoleArguments)
                    .IsSuccess())
                {
                    return;
                }

                if (consoleArguments.DataExport)
                {
                    await DataExportAsync(parsedArgs);
                }

                if (consoleArguments.ApiExport)
                {
                    await ApiExportAsync(parsedArgs);
                }
            }
            catch (Exception e) 
            {
                Console.Error.WriteLine(e.GetType());
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e);
                Console.Error.WriteLine(JsonConvert.SerializeObject(e));
            }
        }

        [NotNull]
        private static async Task DataExportAsync([NotNull] ParsedCommandLineArguments parsedArgs)
        {
            if (!parsedArgs
                .Extract(out DataExportConsoleArguments exportConsoleArguments)
                .IsSuccess())
            {
                return;
            }

            var assembly = Assembly.LoadFrom(exportConsoleArguments.DataExportInputBinPath);

            var extractor = new DataModelReader(
                exportConsoleArguments.DataExportDbModelPostfix, 
                assembly);
            var dataModel = extractor.ExtractDataModelInfo();

            var serializer = new DataModelSerializer(exportConsoleArguments.DataExportOutput);
            await serializer.WriteAsync(dataModel);
        }

        [NotNull]
        private static async Task ApiExportAsync([NotNull] ParsedCommandLineArguments parsedArgs)
        {
            if (!parsedArgs
                .Extract(out ApiExportConsoleArguments exportConsoleArguments)
                .IsSuccess())
            {
                return;
            }

            var assembly = Assembly.LoadFrom(exportConsoleArguments.ApiExportInputBinPath);

            var extractor = new ApiModelReader(
                exportConsoleArguments.ApiExportNodePostfix,
                exportConsoleArguments.ApiExportModelPostfix,
                assembly);
            var apiModel = extractor.ExtractApiModelInfo();

            var serializer = new ApiModelSerializer(exportConsoleArguments.ApiExportOutput);
            await serializer.WriteAsync(apiModel);
        }
    }
}
