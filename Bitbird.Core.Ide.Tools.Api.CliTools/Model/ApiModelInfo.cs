using System;
using System.Linq;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    public class ApiModelInfo
    {
        [CanBeNull, UsedImplicitly] public ApiModelAttributeInfo IdAttribute { get; }
        [NotNull, ItemNotNull, UsedImplicitly] public ApiModelAttributeInfo[] Attributes { get; }
        [NotNull, ItemNotNull, UsedImplicitly] public ApiModelRelationInfo[] Relations { get; }

        public ApiModelInfo(
            [CanBeNull] ApiModelAttributeInfo idAttribute,
            [NotNull, ItemNotNull] ApiModelAttributeInfo[] attributes, 
            [NotNull, ItemNotNull] ApiModelRelationInfo[] relations)
        {
            IdAttribute = idAttribute ?? throw new ArgumentNullException(nameof(idAttribute));
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            Relations = relations ?? throw new ArgumentNullException(nameof(relations));
            if (attributes.Any(p => p == null)) throw new ArgumentNullException(nameof(attributes));
            if (relations.Any(p => p == null)) throw new ArgumentNullException(nameof(relations));
        }
    }
}