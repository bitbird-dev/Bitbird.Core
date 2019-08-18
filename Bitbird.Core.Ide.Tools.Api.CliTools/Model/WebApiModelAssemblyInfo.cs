using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class WebApiModelAssemblyInfo
    {
        [NotNull, ItemNotNull, UsedImplicitly]
        public WebApiControllerInfo[] WebApiControllerInfos { get; }
        
        [JsonConstructor]
        public WebApiModelAssemblyInfo(
            [NotNull, ItemNotNull] WebApiControllerInfo[] webApiControllerInfos)
        {
            WebApiControllerInfos = webApiControllerInfos ?? throw new ArgumentNullException(nameof(webApiControllerInfos));
            if (webApiControllerInfos.Any(p => p == null)) throw new ArgumentNullException(nameof(webApiControllerInfos));
        }
    }

    public sealed class WebApiControllerInfo
    {
    }
}