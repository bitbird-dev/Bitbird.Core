using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    public class ApiEnumValueInfo
    {
        [NotNull, UsedImplicitly] public string Value { get; }
        [NotNull, UsedImplicitly] public string Name { get; }

        public ApiEnumValueInfo([NotNull] string value, [NotNull] string name)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}