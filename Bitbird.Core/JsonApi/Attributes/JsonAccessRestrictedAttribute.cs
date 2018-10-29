using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi.Attributes
{
    /// <summary>
    /// Use in combination with the JsonApiBaseModel.IsPropertyAccessible method
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)] 
    public class JsonAccessRestrictedAttribute : Attribute
    {
    }
}
