using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelPropertyStringInfo
    {
        [CanBeNull, UsedImplicitly] private readonly int? MaximumLength;

        internal DataModelPropertyStringInfo(
            [CanBeNull] int? maximumLength)
        {
            MaximumLength = maximumLength;
        }
    }
}