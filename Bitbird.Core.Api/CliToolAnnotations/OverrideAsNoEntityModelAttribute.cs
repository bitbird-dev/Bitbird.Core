using System;

namespace Bitbird.Core.Api.CliToolAnnotations
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class OverrideAsNoEntityModelAttribute : Attribute
    {
    }
}