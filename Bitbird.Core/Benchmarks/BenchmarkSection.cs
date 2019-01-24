using System.Collections.Generic;

namespace Bitbird.Core.Benchmarks
{
    public class BenchmarkSection
    {
        public readonly List<BenchmarkResult> Benchmarks;
        internal readonly string Prefix;

        public BenchmarkSection(List<BenchmarkResult> benchmarks, string prefix)
        {
            Benchmarks = benchmarks;
            Prefix = prefix;
        }
    }

    public static class BenchmarkSectionExtension
    {
        public static BenchmarkSection CreateSection(this BenchmarkSection section, string name)
        {
            if (section == null)
                return null;

            var fullName = name;
            if (section.Prefix != null)
                fullName = $"{section.Prefix}.{fullName}";

            return new BenchmarkSection(section.Benchmarks, fullName);
        }
        public static ActiveMeasuring CreateBenchmark(this BenchmarkSection section, string name)
        {
            var fullName = name;
            if (section?.Prefix != null)
                fullName = $"{section.Prefix ?? string.Empty}.{fullName}";
            
            return new ActiveMeasuring(section?.Benchmarks, fullName);
        }
    }
}