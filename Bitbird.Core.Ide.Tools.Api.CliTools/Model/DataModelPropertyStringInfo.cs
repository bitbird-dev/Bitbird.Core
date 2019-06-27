using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelPropertyStringInfo
    {
        [CanBeNull, UsedImplicitly] private int? MaximumLength { get; }

        [JsonConstructor]
        public DataModelPropertyStringInfo(
            [CanBeNull] int? maximumLength)
        {
            MaximumLength = maximumLength;
        }
    }
}