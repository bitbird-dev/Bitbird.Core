using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Json.Helpers.ApiResource.Tests.TestModel
{
    internal class TestModelToN : TestModelBase
    {
        public IEnumerable<Guid> ChildrenIds { get; set; }
        public IEnumerable<TestModelToOne> Children { get; set; }
    }

    internal class TestModelToNApiResource : TestModelApiResource
    {
        public TestModelToNApiResource() : base()
        {
            HasMany<TestModelToOneApiResource>(nameof(TestModelToN.Children), nameof(TestModelToN.ChildrenIds));
        }
    }
}
