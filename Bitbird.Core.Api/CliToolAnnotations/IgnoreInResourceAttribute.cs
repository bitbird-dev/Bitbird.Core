using System;

namespace Bitbird.Core.Api.CliToolAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreInResourceAttribute : Attribute
    {
    }
}
