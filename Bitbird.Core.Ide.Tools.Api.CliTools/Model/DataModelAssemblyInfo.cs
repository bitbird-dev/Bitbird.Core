using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelAssemblyInfo
    {
        [NotNull, ItemNotNull, UsedImplicitly]
        public DataModelInfo[] DataModelInfos { get; }


        [JsonConstructor]
        public DataModelAssemblyInfo(
            [NotNull, ItemNotNull] DataModelInfo[] dataModelInfos)
        {
            DataModelInfos = dataModelInfos ?? throw new ArgumentNullException(nameof(dataModelInfos));
            if (dataModelInfos.Any(p => p == null)) throw new ArgumentNullException(nameof(dataModelInfos));
        }
    }
}