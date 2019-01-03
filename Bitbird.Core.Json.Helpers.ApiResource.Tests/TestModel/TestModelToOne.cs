using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Json.Helpers.ApiResource.Tests.TestModel
{
    internal class TestModelToOne : TestModelBase
    {
        public Guid ToOneId { get; set; }
        public TestModelBase ToOne { get; set; }
    }

    internal class TestModelToOneApiResource : TestModelApiResource
    {
        public TestModelToOneApiResource() : base()
        {
            BelongsTo<TestModelApiResource>(nameof(TestModelToOne.ToOne), nameof(TestModelToOne.ToOneId));
        }
    }
}
