using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Json.Helpers.Base.Converters;
using Bitbird.Core.Json.Helpers.Base.Extensions;
using Bitbird.Core.Json.Helpers.ApiResource.Extensions;

namespace Bitbird.Core.Json.Helpers.ApiResource.Tests
{
    [TestClass]
    public class UnitTest1
    {
        
        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            BtbrdCoreIdConverters.AddConverter(new BtbrdCoreIdConverter<long?>(toString => toString.ToString(), stringVal => long.TryParse(stringVal, out var tempVal) ? tempVal : (long?)null));
            BtbrdCoreIdConverters.AddConverter(new BtbrdCoreIdConverter<long>(toString => toString.ToString(), stringVal => long.TryParse(stringVal, out var tempVal) ? tempVal : long.MinValue));
        }

        #region Test Models

        public class Model1Resource : JsonApiResource
        {
            public Model1Resource()
            {
                WithId(nameof(Model1.Id));
                Attribute(nameof(Model1.HomeAttribute));
                BelongsTo<Model2Resource>(nameof(Model1.Model2), nameof(Model1.Model2Id));
                HasMany<Model2Resource>(nameof(Model1.MoreModel2), nameof(Model1.MoreModel2Id));
            }
        }
        public class Model1
        {
            public long Id { get; set; }
            public string HomeAttribute { get; set; }
            /// <summary>
            /// Deserialize: Model2 is always null. Model2Id is null or a value, based on the data in the json-string
            /// Serialize: Model2Id is no attribute, but identifies the "BelongsTo"-relation-id.
            /// if it is null, the relation is not set. if it has a value, the relation shoudl be provided (id and type). 
            /// ONLY if the settings tell the system to "include" the relation Model2, the property Model2 is read and serialized (as included).
            /// </summary>
            public Model2 Model2 { get; set; }
            public long? Model2Id { get; set; }
            public IEnumerable<Model2> MoreModel2 { get; set; }
            public IEnumerable<long?> MoreModel2Id { get; set; }
        }
        public class Model2Resource : JsonApiResource
        {
            public Model2Resource()
            {
                WithId(nameof(Model2.Id));
                Attribute(nameof(Model2.Name));
            }
        }
        public class Model2
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        public class Model3Resource : JsonApiResource
        {
            public Model3Resource()
            {
                WithId(nameof(Model3.Id));
                BelongsTo<Model2Resource>(nameof(Model3.ModelReference), nameof(Model3.ModelReferenceId));
                BelongsTo<Model3Resource>(nameof(Model3.NestedReference), nameof(Model3.NestedReferenceId));
            }
        }
        public class Model3
        {
            public long Id { get; set; }
            public long? ModelReferenceId { get; set; }
            public Model2 ModelReference { get; set; }
            public long? NestedReferenceId { get; set; }

            public Model3 NestedReference { get; set; }
        }

        #endregion


        [TestMethod] public void AddCollectionsToDocument()
        {
            var data = new List<Model1>
            {
                new Model1
                {
                    HomeAttribute = "whatever"
                },
                new Model1
                {
                    HomeAttribute = "whatever2"
                }
            };
            var doc = JsonApiCollectionDocumentExtensions.CreateDocumentFromApiResource<Model1Resource>(data);
            var jsonString = JsonConvert.SerializeObject(doc, Formatting.Indented);
            var deserialized = JsonConvert.DeserializeObject<JsonApiCollectionDocument>(jsonString);
            var res2 = deserialized.ToObjectCollection(Activator.CreateInstance<Model1Resource>(), typeof(Model1));
        }

        [TestMethod] public void floTest()
        {
            var data = new Model1
            {
                Id = 1232385789,
                HomeAttribute = "myhome",
                Model2Id = 55555555,
                Model2 = new Model2
                {
                    Id = 55555555,
                    Name = "myName"
                },
                MoreModel2Id = new List<long?> {1,2,3},
                MoreModel2 = new List<Model2>
                {
                    new Model2
                    {
                        Id = 1,
                        Name = "arr"
                    },
                    new Model2
                    {
                        Id = 2,
                        Name = "arrr"
                    },
                    new Model2
                    {
                        Id = 3,
                        Name = "arrr"
                    },
                }
            };

            var doc = JsonApiDocumentExtensions.CreateDocumentFromApiResource<Model1Resource>(data);
            doc.IncludeRelation<Model1Resource>(data, $"{nameof(Model1.MoreModel2)},{nameof(Model1.Model2)}");

            var jsonString = JsonConvert.SerializeObject(doc, Formatting.Indented);
            var deserialized = JsonConvert.DeserializeObject<JsonApiDocument>(jsonString);
            Func<string, bool> foundAttributes;
            var resultData = deserialized.ToObject<Model1, Model1Resource>(out foundAttributes);

            resultData.Model2 = deserialized.Included.GetResource(resultData.Model2Id, typeof(Model2))?.ToObject<Model2, Model2Resource>();
            resultData.MoreModel2 = resultData.MoreModel2Id?.Select(x => deserialized.Included.GetResource(x, typeof(Model2))?.ToObject<Model2, Model2Resource>());
            Assert.IsTrue(foundAttributes(nameof(data.HomeAttribute)));
        }
    }
}
