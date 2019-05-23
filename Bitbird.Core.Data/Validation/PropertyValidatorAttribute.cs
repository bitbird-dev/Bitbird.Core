using System;

namespace Bitbird.Core.Data.Validation
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public abstract class PropertyValidatorAttribute : Attribute
    {
    }
}