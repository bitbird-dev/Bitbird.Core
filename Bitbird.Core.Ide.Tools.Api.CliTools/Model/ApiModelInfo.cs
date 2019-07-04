using System;
using System.Linq;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    public class ApiModelInfo
    {
        [NotNull, UsedImplicitly] public string TypeNameAsCsType { get; set; }
        [NotNull, UsedImplicitly] public string ModelName { get; set; }
        [NotNull, UsedImplicitly] public string ModelNameAsKebabCase { get; set; }
        [CanBeNull, UsedImplicitly] public ApiModelAttributeInfo IdAttribute { get; }
        [NotNull, ItemNotNull, UsedImplicitly] public ApiModelAttributeInfo[] Attributes { get; }
        [NotNull, ItemNotNull, UsedImplicitly] public ApiModelRelationInfo[] Relations { get; }

        public ApiModelInfo(
            [NotNull, UsedImplicitly] string typeNameAsCsType,
            [NotNull, UsedImplicitly] string modelName,
            [NotNull, UsedImplicitly] string modelNameAsKebabCase,
            [CanBeNull] ApiModelAttributeInfo idAttribute,
            [NotNull, ItemNotNull] ApiModelAttributeInfo[] attributes, 
            [NotNull, ItemNotNull] ApiModelRelationInfo[] relations)
        {
            TypeNameAsCsType = typeNameAsCsType ?? throw new ArgumentNullException(nameof(typeNameAsCsType));
            ModelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
            ModelNameAsKebabCase = modelNameAsKebabCase ?? throw new ArgumentNullException(nameof(modelNameAsKebabCase));
            IdAttribute = idAttribute ?? throw new ArgumentNullException(nameof(idAttribute));
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            Relations = relations ?? throw new ArgumentNullException(nameof(relations));
            if (attributes.Any(p => p == null)) throw new ArgumentNullException(nameof(attributes));
            if (relations.Any(p => p == null)) throw new ArgumentNullException(nameof(relations));
        }
    }
}