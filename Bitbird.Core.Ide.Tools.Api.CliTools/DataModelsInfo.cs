using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelsInfo
    {
        [NotNull, ItemNotNull, UsedImplicitly]
        public readonly DataModelInfo[] DataModelInfos;

        internal DataModelsInfo(
            [NotNull, ItemNotNull] DataModelInfo[] dataModelInfos)
        {
            DataModelInfos = dataModelInfos;
        }
    }
}