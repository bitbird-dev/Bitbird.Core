using System;
using System.Collections.Generic;
using Bitbird.Core.JsonApi;
using Bitbird.Core.Json.Helpers.ApiResource.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Bitbird.Core.Json.Helpers.ApiResource.Tests
{
    [TestClass]
    public class UnitTest1
    {
        class ResourceWithoutRelations : JsonApiResource
        {
            public ResourceWithoutRelations()
            {
                Attribute(nameof(ClassWithoutRelations.AttributeOne));
                Attribute(nameof(ClassWithoutRelations.AttributeTwo));
                Attribute(nameof(ClassWithoutRelations.AttributeThree));
            }
        }

        class ClassWithoutRelations
        {
            public string AttributeOne { get; set; }
            public string AttributeTwo { get; set; }
            public string AttributeThree { get; set; }
            public Guid Id { get; set; }
        }

        class ResourceWithToNRelations : JsonApiResource
        {
            public ResourceWithToNRelations()
            {
                Attribute(nameof(ClassWithToNRelations.AttributeOne));

                BelongsTo<ResourceWithoutRelations>(nameof(ClassWithToNRelations.ToOneRelation));
                HasMany<ResourceWithoutRelations>(nameof(ClassWithToNRelations.ToManyRelation));
            }
        }

        class ClassWithToNRelations
        {
            public int? AttributeOne { get; set; }

            public ClassWithoutRelations ToOneRelation { get; set; }
            public IEnumerable<ClassWithoutRelations> ToManyRelation { get; set; }
            public Guid Id { get; set; }
        }

        

        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
        }

        [TestMethod]
        public void TestMethod1()
        {
            var data = GetAResource();
            var apiResource = new ResourceWithToNRelations();
            var doc = new JsonApiDocument();
            doc.FromApiResource(data, apiResource);
            doc.IncludeRelation(data, apiResource, nameof(data.ToOneRelation));
            doc.IncludeRelation(data, apiResource, nameof(data.ToManyRelation));
            var jsonString = JsonConvert.SerializeObject(doc, Formatting.Indented);
        }

        private ClassWithToNRelations GetAResource()
        {
            return new ClassWithToNRelations
            {
                Id = Guid.NewGuid(),
                AttributeOne = 12345,
                ToOneRelation = new ClassWithoutRelations
                {
                    Id = Guid.NewGuid(),
                    AttributeOne = "muh",
                    AttributeTwo = "mäh",
                    AttributeThree = "meh"
                },
                ToManyRelation = new List<ClassWithoutRelations>{
                    new ClassWithoutRelations
                    {
                        Id = Guid.NewGuid(),
                        AttributeOne = "arr",
                        AttributeTwo = "arrr",
                        AttributeThree = "arrrr"
                    },
                    new ClassWithoutRelations
                    {
                        Id = Guid.NewGuid(),
                        AttributeOne = "brr",
                        AttributeTwo = "brrr",
                        AttributeThree = "brrrr"
                    }
                }
            };
        }

    }
}
