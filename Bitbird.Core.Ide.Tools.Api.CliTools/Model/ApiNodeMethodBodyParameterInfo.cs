﻿using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    public class ApiNodeMethodBodyParameterInfo
    {
        [UsedImplicitly] public int Position { get; }
        [NotNull, UsedImplicitly] public string Name { get; }
        [NotNull, UsedImplicitly] public string Type { get; }

        public ApiNodeMethodBodyParameterInfo(
            int position, 
            [NotNull] string name, 
            [NotNull] string type)
        {
            Position = position;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}