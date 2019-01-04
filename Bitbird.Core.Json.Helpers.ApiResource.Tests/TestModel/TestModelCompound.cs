using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Json.Helpers.ApiResource.Tests.TestModel
{
    internal class TestModelCompound : TestModelBase
    {
        public int? BigDataId { get; set; }
        public TestModelToN BigData { get; set; }
        public int? SmallDataId { get; set; }
        public TestModelToOne SmallData { get; set; }
    }

    internal class TestModelCompoundApiResource : TestModelApiResource
    {
        public TestModelCompoundApiResource() : base()
        {
            OfType("compoundData", "/compound");
            BelongsTo<TestModelToNApiResource>(nameof(TestModelCompound.BigData), nameof(TestModelCompound.BigDataId));
            BelongsTo<TestModelToOneApiResource>(nameof(TestModelCompound.SmallData), nameof(TestModelCompound.SmallDataId));
        }
    }
}
