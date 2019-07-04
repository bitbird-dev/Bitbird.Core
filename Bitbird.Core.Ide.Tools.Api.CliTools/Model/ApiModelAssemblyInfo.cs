using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class ApiModelAssemblyInfo
    {
        [NotNull, ItemNotNull, UsedImplicitly]
        public ApiNodeInfo[] ApiNodeInfos { get; }

        [NotNull, ItemNotNull, UsedImplicitly]
        public ApiModelInfo[] ApiModelInfos { get; }

        [NotNull, ItemNotNull, UsedImplicitly]
        public ApiEnumInfo[] ApiEnumInfos { get; }


        [JsonConstructor]
        public ApiModelAssemblyInfo(
            [NotNull, ItemNotNull] ApiNodeInfo[] apiNodeInfos,
            [NotNull, ItemNotNull] ApiModelInfo[] apiModelInfos,
            [NotNull, ItemNotNull] ApiEnumInfo[] apiEnumInfos)
        {
            ApiNodeInfos = apiNodeInfos ?? throw new ArgumentNullException(nameof(apiNodeInfos));
            if (apiNodeInfos.Any(p => p == null)) throw new ArgumentNullException(nameof(apiNodeInfos));

            ApiModelInfos = apiModelInfos ?? throw new ArgumentNullException(nameof(apiModelInfos));
            if (apiModelInfos.Any(p => p == null)) throw new ArgumentNullException(nameof(apiModelInfos));

            ApiEnumInfos = apiEnumInfos ?? throw new ArgumentNullException(nameof(apiEnumInfos));
            if (apiEnumInfos.Any(p => p == null)) throw new ArgumentNullException(nameof(apiEnumInfos));
        }
    }
}