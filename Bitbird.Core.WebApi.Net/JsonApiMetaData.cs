using System.Collections.Generic;

namespace Bitbird.Core.WebApi.Net
{
    public class JsonApiMetaData
    {
        public IEnumerable<string> Benchmarks { get; set; }
        public long? PageCount { get; set; }
        public long? RecordCount { get; set; }
    }
}