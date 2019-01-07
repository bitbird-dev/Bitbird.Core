using System.Reflection;

namespace Bitbird.Core.CommandLineArguments
{
    internal class CommandLinePropertyInfo
    {
        public readonly string Key;
        public readonly PropertyInfo Property;
        public readonly CommandLineArgAttribute Attribute;

        public CommandLinePropertyInfo(string key, PropertyInfo property, CommandLineArgAttribute attribute)
        {
            Key = key;
            Property = property;
            Attribute = attribute;
        }
    }
}