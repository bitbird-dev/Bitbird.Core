﻿using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelSerializer : BaseModelSerializer<DataModelAssemblyInfo>
    {
        [UsedImplicitly]
        public DataModelSerializer([NotNull] string path)
            : base(path)
        {
        }
    }
}
