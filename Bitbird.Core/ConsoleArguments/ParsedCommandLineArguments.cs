using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.CommandLineArguments
{
    public class ParsedCommandLineArguments
    {
        private readonly Dictionary<string, string> parameters;
        private readonly HashSet<string> switches;
        private readonly Dictionary<Type, CommandLinePropertyInfo[]> extractedTypes = new Dictionary<Type, CommandLinePropertyInfo[]>();
        private readonly Dictionary<Type, CommandLinePropertyInfo[]> missingRequired = new Dictionary<Type, CommandLinePropertyInfo[]>();
        private bool wasHelpSupported = false;

        public ParsedCommandLineArguments(Dictionary<string, string> parameters, HashSet<string> switches)
        {
            this.parameters = parameters;
            this.switches = switches;
        }

        public ParsedCommandLineArguments Extract<T>(out T model) where T : new()
        {
            var properties = typeof(T)
                .GetProperties()
                .Where(p => p.CanWrite)
                .Select(p => new CommandLinePropertyInfo(p.Name.ToUpper(), p, p.GetCustomAttribute<CommandLineArgAttribute>()))
                .Where(p => p.Attribute != null)
                .ToArray();

            extractedTypes.Add(typeof(T), properties);

            var required = properties
                .Where(p => p.Attribute.IsRequired)
                .ToDictionary(p => p.Key);

            var propertyDict = properties
                .ToDictionary(p => p.Key);

            model = new T();
            foreach (var @switch in switches)
            {
                if (!propertyDict.TryGetValue(@switch, out var property))
                    continue;

                if (property.Property.PropertyType == typeof(bool))
                    property.Property.SetValue(model, true);
                if (property.Property.PropertyType == typeof(bool?))
                    property.Property.SetValue(model, (bool?)true);
                else
                    continue;

                required.Remove(@switch);
            }
            foreach (var parameter in parameters)
            {
                if (!propertyDict.TryGetValue(parameter.Key, out var property))
                    continue;

                var value = Convert.ChangeType(parameter.Value, property.Property.PropertyType);
                property.Property.SetValue(model, value);

                required.Remove(parameter.Key);
            }

            if (required.Count != 0)
                missingRequired.Add(typeof(T), required.Values.ToArray());

            return this;
        }

        public ParsedCommandLineArguments PrintInfoIfHelpSwitchWasFound(string customMessage = null, Action<string> writeToOutput = null)
        {
            wasHelpSupported = true;

            if (!switches.Contains("help".ToUpper()))
                return this;

            var sbCommand = new StringBuilder();
            var sbDescription = new StringBuilder();

            sbCommand.Append(Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));

            foreach (var extractedType in extractedTypes)
            {
                sbDescription.AppendLine($"  Parameter group {extractedType.Key.Name} defines:");
                foreach (var property in extractedType.Value)
                {
                    if (property.Property.PropertyType == typeof(bool) || property.Property.PropertyType == typeof(bool?))
                    {
                        sbCommand.Append($" {(!property.Attribute.IsRequired? "[" : string.Empty)}-{property.Property.Name} [<{property.Property.PropertyType.Name}>]{(!property.Attribute.IsRequired ? "]" : string.Empty)}");
                        sbDescription.AppendLine($"    Parameter/Switch -{property.Property.Name}. Type: {property.Property.PropertyType.Name}. {(!property.Attribute.IsRequired ? "Optional" : "Required")}.");
                    }
                    else
                    {
                        sbCommand.Append($" {(!property.Attribute.IsRequired? "[" : string.Empty)}-{property.Property.Name} <{property.Property.PropertyType.Name}>{(!property.Attribute.IsRequired ? "]" : string.Empty)}");
                        sbDescription.AppendLine($"    Parameter -{property.Property.Name}. Type: {property.Property.PropertyType.Name}. {(!property.Attribute.IsRequired ? "Optional" : "Required")}.");
                    }
                }
                sbDescription.AppendLine();
            }

            var info = $"{customMessage ?? string.Empty}{(customMessage == null ? string.Empty : Environment.NewLine)}Usage: {sbCommand}{Environment.NewLine}{Environment.NewLine}Parameters/Switches:{Environment.NewLine}{sbDescription}{new string('-',20)}";

            writeToOutput = writeToOutput ?? Console.WriteLine;
            writeToOutput(info);

            return this;
        }

        public bool IsSuccess(string customErrorMessage = null, Action<string> writeToOutput = null)
        {
            if (missingRequired.Count == 0)
                return true;

            var sb = new StringBuilder();

            foreach (var type in missingRequired)
            {
                sb.AppendLine($"  From group {type.Key.Name}:");
                foreach (var property in type.Value)
                    sb.AppendLine($"    -{property.Property.Name}");
            }

            var info = $"{customErrorMessage ?? string.Empty}{(customErrorMessage == null ? string.Empty : Environment.NewLine)}The following required parameters were not provided:{Environment.NewLine}{sb}{Environment.NewLine}{(wasHelpSupported ? "For more information use the -Help switch." : string.Empty)}";

            writeToOutput = writeToOutput ?? Console.WriteLine;
            writeToOutput(info);

            return false;
        }
    }
}