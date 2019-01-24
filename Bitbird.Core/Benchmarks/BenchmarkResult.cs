namespace Bitbird.Core.Benchmarks
{
    public class BenchmarkResult
    {
        public readonly string Name;
        public readonly long Duration;

        public BenchmarkResult(string name, long duration)
        {
            Name = name;
            Duration = duration;
        }
    }
}