using System.Collections.Generic;

namespace Bitbird.Core.Benchmarks
{
    public class BenchmarkCollection : BenchmarkSection
    {
        public BenchmarkCollection(bool enableLogging = true) : base(enableLogging ? new List<BenchmarkResult>() : null, null)
        {
        }
    }
}
