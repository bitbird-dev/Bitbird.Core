using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    public class ApiEnumInfo
    {
        [NotNull, UsedImplicitly] public string TypeAsCsType { get; }
        [NotNull, UsedImplicitly] public string UnderlyingTypeAsCsType { get; }
        [NotNull, UsedImplicitly] public ApiEnumValueInfo[] Values { get; }

        public ApiEnumInfo(
            [NotNull] string typeAsCsType,
            [NotNull] string underlyingTypeAsCsType,
            [NotNull] ApiEnumValueInfo[] values)
        {
            TypeAsCsType = typeAsCsType ?? throw new ArgumentNullException(nameof(typeAsCsType));
            UnderlyingTypeAsCsType = underlyingTypeAsCsType ?? throw new ArgumentNullException(nameof(underlyingTypeAsCsType));
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }
    }
}