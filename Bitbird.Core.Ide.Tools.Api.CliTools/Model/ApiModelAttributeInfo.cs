using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    public class ApiModelAttributeInfo
    {
        [NotNull, UsedImplicitly] public string Name { get; }
        [NotNull, UsedImplicitly] public string TypeAsCsType { get; }

        public ApiModelAttributeInfo(
            [NotNull] string name,
            [NotNull] string typeAsCsType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TypeAsCsType = typeAsCsType ?? throw new ArgumentNullException(nameof(typeAsCsType));
        }
    }
}