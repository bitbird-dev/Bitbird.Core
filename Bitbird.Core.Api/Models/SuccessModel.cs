using JetBrains.Annotations;

namespace Bitbird.Core.Api.Models
{
    public class SuccessModel : IIdSetter<long>
    {
        [UsedImplicitly]
        public long Id { get; set; }

        [UsedImplicitly]
        public bool WasSuccess { get; set; }
    }
}