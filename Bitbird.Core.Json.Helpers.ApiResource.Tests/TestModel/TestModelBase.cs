using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Json.Helpers.ApiResource.Tests.TestModel
{
    internal class TestModelBase
    {
        public int? MyIdProperty { get; set; }
    }

    internal class TestModelApiResource : JsonApiResource
    {
        public TestModelApiResource()
        {
            OfType("singleData", "/singledata");
            WithId(nameof(TestModelBase.MyIdProperty));
        }
    }
}
