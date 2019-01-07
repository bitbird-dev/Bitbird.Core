using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bitbird.Core.CommandLineArguments
{
    public static class ParseExtensions
    {
        private static readonly Regex RegexArgsName = new Regex("^[-/](?<Name>.*)$", RegexOptions.Compiled);

        public static ParsedCommandLineArguments ParseArgs(this IReadOnlyList<string> args)
        {
            var grouped = args
                .Select((arg, idx) => new
                {
                    Arg = arg,
                    Idx = idx,
                    Match = RegexArgsName.Match(arg)
                })
                .Where(x => x.Match.Success)
                .Select(x => new
                {
                    x.Idx,
                    Value = x.Match.Groups["Name"].Value.ToUpper()
                })
                .GroupBy(x => x.Value)
                .ToArray();

            var duplicates = grouped
                .Where(group => group.Count() > 1)
                .ToArray();

            if (duplicates.Any())
                throw new Exception($"The following command line parameters were found more than once: {string.Join(",", duplicates.Select(d => d.Key))}");

            var temp = grouped
                .ToDictionary(x => x.Single().Idx, x => x.Single());

            var switches = new HashSet<string>(temp
                .Where(x => temp.ContainsKey(x.Value.Idx + 1) || (x.Value.Idx + 1) == args.Count)
                .Select(x => x.Value.Value));
            
            var parameters = temp
                .Where(x => !temp.ContainsKey(x.Value.Idx + 1) && (x.Value.Idx+1) < args.Count)
                .ToDictionary(x => x.Value.Value, x => args[x.Value.Idx+1]);

            return new ParsedCommandLineArguments(parameters, switches);
        }
    }
}
