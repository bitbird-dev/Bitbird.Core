using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class ApiModelSerializer : BaseModelSerializer<ApiModelAssemblyInfo>
    {
        [UsedImplicitly]
        public ApiModelSerializer([NotNull] string path)
            : base(path)
        {
        }
    }
}
