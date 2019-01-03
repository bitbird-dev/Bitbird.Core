using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Json.Helpers.ApiResource.Tests.TestModel
{
    internal static class TestModelRepository
    {
        public static TestModelCompound GetIncludeDeeplyNestedResourceTestData()
        {
            var nestedChild0 = new TestModelToOne { MyIdProperty = Guid.NewGuid(), ToOne = new TestModelBase { MyIdProperty = Guid.NewGuid() } };
            nestedChild0.ToOneId = nestedChild0.ToOne.MyIdProperty;
            var nestedChild1 = new TestModelToOne { MyIdProperty = Guid.NewGuid(), ToOne = new TestModelBase { MyIdProperty = Guid.NewGuid() } };
            nestedChild1.ToOneId = nestedChild1.ToOne.MyIdProperty;
            var data = new TestModelToN
            {
                MyIdProperty = Guid.NewGuid(),
                Children = new List<TestModelToOne>()
                {
                    nestedChild0,
                    nestedChild1
                }
            };
            data.ChildrenIds = data.Children.Select(c => c.MyIdProperty);
            return new TestModelCompound
            {
                BigData = data,
                BigDataId = data.MyIdProperty,
                SmallData = nestedChild0,
                SmallDataId = nestedChild0.MyIdProperty
            };
        }
    }
}
