using System;
using Bitbird.Core.Api.CliToolAnnotations;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    public class ApiModelRelationInfo
    {
        [NotNull, UsedImplicitly] public string Name { get; }
        [NotNull, UsedImplicitly] public string IdName { get; }
        [UsedImplicitly] public ModelRelationType Type { get; }
        [NotNull, UsedImplicitly] public string ModelTypeAsCsType { get; }

        public ApiModelRelationInfo(
            [NotNull] string name, 
            [NotNull] string idName, 
            ModelRelationType type, 
            [NotNull] string modelTypeAsCsType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IdName = idName ?? throw new ArgumentNullException(nameof(idName));
            Type = type;
            ModelTypeAsCsType = modelTypeAsCsType ?? throw new ArgumentNullException(nameof(modelTypeAsCsType));
        }
    }
}