using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class WebApiModelSerializer : BaseModelSerializer<WebApiModelAssemblyInfo>
    {
        [UsedImplicitly]
        public WebApiModelSerializer([NotNull] string path)
            : base(path)
        {
        }
    }
}
