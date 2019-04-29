using Bitbird.Core.Benchmarks;

namespace Bitbird.Core.WebApi.Net.Benchmarking
{
    public interface IBenchmarkController
    {
        BenchmarkCollection Benchmarks { get; set; }
    }
}