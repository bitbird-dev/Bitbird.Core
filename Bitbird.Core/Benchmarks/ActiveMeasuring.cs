using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Bitbird.Core.Benchmarks
{
    public class ActiveMeasuring : IDisposable
    {
        private readonly List<BenchmarkResult> benchmarks;
        private readonly string name;
        private readonly Stopwatch sw;

        public ActiveMeasuring(List<BenchmarkResult> benchmarks, string name)
        {
            this.benchmarks = benchmarks;
            this.name = name;

            if (benchmarks == null)
                return;

            sw = new Stopwatch();
            sw.Start();
        }

        public void Dispose()
        {
            if (benchmarks == null)
                return;

            sw.Stop();
            benchmarks.Add(new BenchmarkResult(name, sw.ElapsedMilliseconds));
        }
    }
}