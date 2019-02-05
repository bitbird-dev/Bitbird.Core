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

        public class ModelOneResource : JsonApiResource
        {
            public ModelOneResource()
            {
                WithId(nameof(ModelOne.Id));
                Attribute(nameof(ModelOne.HomeAttribute));
                BelongsTo<Model2Resource>(nameof(ModelOne.Model2), nameof(ModelOne.Model2Id));
                HasMany<Model2Resource>(nameof(ModelOne.MoreModel2), nameof(ModelOne.MoreModel2Id));
            }
        }
        public class ModelOne
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
            var data = new List<ModelOne>
            {
                new ModelOne
                {
                    HomeAttribute = "whatever"
                },
                new ModelOne
                {
                    HomeAttribute = "whatever2"
                }
            };
            var doc = JsonApiCollectionDocumentExtensions.CreateDocumentFromApiResource<ModelOneResource>(data);
            var jsonString = JsonConvert.SerializeObject(doc, Formatting.Indented);
            var deserialized = JsonConvert.DeserializeObject<JsonApiCollectionDocument>(jsonString);
            var res2 = deserialized.ToObject(Activator.CreateInstance<ModelOneResource>(), typeof(ModelOne));
        }

        [TestMethod] public void floTest()
        {
            var data = new ModelOne
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

            var doc = JsonApiDocumentExtensions.CreateDocumentFromApiResource<ModelOneResource>(data);
            doc.IncludeRelation<ModelOneResource>(data, $"{nameof(ModelOne.MoreModel2)},{nameof(ModelOne.Model2)}");

            var jsonString = JsonConvert.SerializeObject(doc, Formatting.Indented);
            var deserialized = JsonConvert.DeserializeObject<JsonApiDocument>(jsonString);
            Func<string, bool> foundAttributes;
            var resultData = deserialized.ToObject<ModelOne, ModelOneResource>(out foundAttributes);

            resultData.Model2 = deserialized.Included.GetResource(resultData.Model2Id, typeof(Model2))?.ToObject<Model2, Model2Resource>();
            resultData.MoreModel2 = resultData.MoreModel2Id?.Select(x => deserialized.Included.GetResource(x, typeof(Model2))?.ToObject<Model2, Model2Resource>());
            Assert.IsTrue(foundAttributes(nameof(data.HomeAttribute)));
        }
    }
}
