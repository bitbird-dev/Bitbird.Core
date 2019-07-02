using System.Collections.Generic;

namespace Bitbird.Core.Web.JsonApi
{
    public class JsonApiMetaData
    {
        public IEnumerable<string> Benchmarks { get; set; }
        public long? PageCount { get; set; }
        public long? RecordCount { get; set; }
    }
}