using System;

namespace Bitbird.Core.CommandLineArguments
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CommandLineArgAttribute : Attribute
    {
        public readonly bool IsRequired;

        public CommandLineArgAttribute(bool isRequired = false)
        {
            IsRequired = isRequired;
        }
    }
}