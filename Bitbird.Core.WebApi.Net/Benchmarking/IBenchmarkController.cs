using Bitbird.Core.Benchmarks;

namespace Bitbird.Core.WebApi.Benchmarking
{
    public interface IBenchmarkController
    {
        BenchmarkCollection Benchmarks { get; set; }
    }
}