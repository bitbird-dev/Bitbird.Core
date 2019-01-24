using Bitbird.Core.Benchmarks;

namespace Bitbird.Core.WebApi.Net
{
    public interface IBenchmarkController
    {
        BenchmarkCollection Benchmarks { get; set; }
    }
}