using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bitbird.Core.Benchmarks
{
    public class ActiveMeasuring : IDisposable
    {
        private readonly List<BenchmarkResult> benchmarks;
        private readonly string name;
        private readonly long start;

        public ActiveMeasuring(List<BenchmarkResult> benchmarks, string name)
        {
            this.benchmarks = benchmarks;
            this.name = name;
            if (this.benchmarks != null)
                start = Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            benchmarks?.Add(new BenchmarkResult(name, (Stopwatch.GetTimestamp() - start) / (Stopwatch.Frequency / 1000)));
        }
    }
}