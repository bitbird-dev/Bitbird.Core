using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bitbird.Core.CommandLineArguments;
using Bitbird.Core.Data.CliToolAnnotations;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    internal static class Program
    {
        [UsedImplicitly]
        private static async Task Main([NotNull] string[] args)
        {
            try
            {
                var parsedArgs = args.ParseArgs();

                if (!parsedArgs
                    .Extract(out ConsoleArguments consoleArguments)
                    .IsSuccess())
                {
                    return;
                }

                if (consoleArguments.Export)
                {
                    await ExportAsync(parsedArgs);
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

        private static async Task ExportAsync([NotNull] ParsedCommandLineArguments parsedArgs)
        {
            if (!parsedArgs
                .Extract(out ExportConsoleArguments exportConsoleArguments)
                .IsSuccess())
            {
                return;
            }

            var assembly = Assembly.LoadFile(exportConsoleArguments.ExportInputBinPath);
            var types = assembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<EntityAttribute>() != null)
                .ToArray();

            var extractor = new DataModelReader(exportConsoleArguments.ExportDbModelPostfix, types);
            var dataModel = extractor.ExtractDataModelInfo();

            var serializer = new DataModelSerializer(exportConsoleArguments.ExportOutput);
            await serializer.WriteAsync(dataModel);
        }
    }
}
