using System;
using System.Linq;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    public class ApiNodeInfo
    {
        [NotNull, UsedImplicitly] public string NodeTypeName { get; }
        [NotNull, UsedImplicitly] public string NodeName { get; }
        [UsedImplicitly] public bool IsCrud { get; }
        [UsedImplicitly] public bool IsRead { get; }
        [CanBeNull, UsedImplicitly] public string ModelTypeAsCsType { get; }
        [NotNull, ItemNotNull, UsedImplicitly] public ApiNodeMethodInfo[] AdditionalMethods { get; }

        public ApiNodeInfo([NotNull] string nodeTypeName,
            [NotNull] string nodeName,
            bool isCrud,
            bool isRead,
            [CanBeNull] string modelTypeAsCsType, 
            [NotNull, ItemNotNull] ApiNodeMethodInfo[] additionalMethods)
        {
            NodeTypeName = nodeTypeName ?? throw new ArgumentNullException(nameof(nodeTypeName));
            NodeName = nodeName ?? throw new ArgumentNullException(nameof(nodeName));
            IsCrud = isCrud;
            IsRead = isRead;
            ModelTypeAsCsType = modelTypeAsCsType;
            AdditionalMethods = additionalMethods ?? throw new ArgumentNullException(nameof(additionalMethods));
            if (additionalMethods.Any(p => p == null)) throw new ArgumentNullException(nameof(additionalMethods));
        }
    }
}