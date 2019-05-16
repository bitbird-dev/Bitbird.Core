using System;

namespace Bitbird.Core.Data.CliToolAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreInApiModelAttribute : Attribute
    {
    }
}
