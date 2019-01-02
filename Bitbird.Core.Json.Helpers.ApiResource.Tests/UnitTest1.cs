using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Bitbird.Core.Json.JsonApi;
using Bitbird.Core.Json.Helpers.ApiResource.Extensions;
using Newtonsoft.Json;
using System.Linq;
using Bitbird.Core.Json.Helpers.Base.Converters;

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
            BtbrdCoreIdConverters.AddConverter(new BtbrdCoreIdConverter<long?>(toString => toString.ToString(), stringVal => long.TryParse(stringVal, out var tempVal) ? tempVal : (long?)null));
            BtbrdCoreIdConverters.AddConverter(new BtbrdCoreIdConverter<long>(toString => toString.ToString(), stringVal => long.TryParse(stringVal, out var tempVal) ? tempVal : long.MinValue));
        }

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

        [TestMethod]
        public void Test_NestedIncludes()
        {
            var data = new Model3
            {
                Id = 1,
                NestedReference = new Model3
                {
                    Id = 11,
                    ModelReference = new Model2
                    {
                        Id = 111,
                        Name = "test name"
                    },
                    ModelReferenceId = 111
                },
                NestedReferenceId = 11
            };
            var doc = JsonApiDocumentExtensions.CreateDocumentFromApiResource<Model3Resource>(data);
            doc.IncludeRelation<Model3Resource>(data, nameof(data.NestedReference));
            doc.IncludeRelation<Model3Resource>(data.NestedReference, nameof(data.NestedReference.ModelReference));
            var jsonString = JsonConvert.SerializeObject(doc, Formatting.Indented);
            var deserialized = JsonConvert.DeserializeObject<JsonApiDocument>(jsonString);
            var resultData = deserialized.ToObject<Model3, Model3Resource>();
            resultData.NestedReference = deserialized.Included.GetResource(resultData.NestedReferenceId, typeof(Model3))?.ToObject<Model3, Model3Resource>();
            resultData.NestedReference.ModelReference = deserialized.Included.GetResource(resultData.NestedReference.ModelReferenceId, typeof(Model2))?.ToObject<Model2, Model2Resource>();
            Assert.AreEqual(resultData.NestedReference.ModelReference.Name, data.NestedReference.ModelReference.Name);
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
            doc.IncludeRelation<Model1Resource>(data, nameof(data.Model2));
            doc.IncludeRelation<Model1Resource>(data, nameof(data.MoreModel2));

            var jsonString = JsonConvert.SerializeObject(doc, Formatting.Indented);
            var deserialized = JsonConvert.DeserializeObject<JsonApiDocument>(jsonString);
            Func<string, bool> foundAttributes;
            var resultData = deserialized.ToObject<Model1, Model1Resource>(out foundAttributes);

            resultData.Model2 = deserialized.Included.GetResource(resultData.Model2Id, typeof(Model2))?.ToObject<Model2, Model2Resource>();
            resultData.MoreModel2 = resultData.MoreModel2Id?.Select(x => deserialized.Included.GetResource(x, typeof(Model2))?.ToObject<Model2, Model2Resource>());
            Assert.IsTrue(foundAttributes(nameof(data.HomeAttribute)));
        }

        //[TestMethod]
        //public void DeserializeRelationsTest()
        //{
        //    var data = GetAResource();
        //    var apiResource = new ResourceWithToNRelations();
        //    var doc = new JsonApiDocument();
        //    doc.FromApiResource(data, apiResource);
        //    doc.IncludeRelation(data, apiResource, nameof(data.ToOneRelation));
        //    doc.IncludeRelation(data, apiResource, nameof(data.ToManyRelation));
        //    var jsonString = JsonConvert.SerializeObject(doc, Formatting.Indented);
        //    var deserialized = JsonConvert.DeserializeObject<JsonApiDocument>(jsonString);

        //ClassWithoutRelations o = deserialized.ToObject<ClassWithoutRelations>(typeof(ResourceWithoutRelations), out Func<string, bool> foundAttributes);

        //if (foundAttributes(nameof(ClassWithoutRelations.AttributeThree)))
        //{
        //    bla
        //}


        //IEnumerable<ClassWithoutRelations> os = deserialized.ToObjectCollection<ClassWithoutRelations>(typeof(ResourceWithoutRelations), out Func<int, string, bool> foundAttributes);

        //if (foundAttributes(0 /* = idx */, nameof(ClassWithoutRelations.AttributeThree) /* = property name */))
        //{
        //    bla
        //}

        //}

        //class FloClass
        //{
        //    public int? Id { get; set; }
        //    public FloClass Flo { get; set; }
        //    public FloClass Flo1 { get; set; }
        //}

        //[TestMethod]
        //public void flo()
        //{
        //    var data = new FloClass()
        //    {
        //        Id = 12124314,
        //        FloId = 85683645,
        //        Flo = new FloClass
        //        {
        //            Id = 85683645,
        //            FloId = 85683645,
        //            Flo = new FloClass
        //            {
        //                Id = 85683645
        //            },
        //        },
        //        Flo1Id = 85683645,
        //        Flo1 = new FloClass
        //        {
        //            Id = 85683645,
        //            FloId = 85683645,
        //            Flo = new FloClass
        //            {
        //                Id = 85683645
        //            },
        //            Flo1Id = 85683645,
        //            Flo1 = new FloClass
        //            {
        //                Id = 85683645
        //            }
        //        }
        //    };
        //    var doc = new JsonApiDocument<FloClass, FloClassResource>(data, includes);
        //    var json = JsonConvert.SerializeObject(doc, Formatting.Indented);
        //    var deserialized = JsonConvert.DeserializeObject<JsonApiDocument<FloClass>>(json);
        //    //var obj = deserialized.ToObject<FloClass>();
        //}

        //[TestMethod]
        //public void TestMethod1()
        //{
        //    var data = GetAResource();
        //    var apiResource = new ResourceWithToNRelations();
        //    var doc = new JsonApiDocument();
        //    doc.FromApiResource(data, apiResource);
        //    doc.IncludeRelation(data, apiResource, nameof(data.ToOneRelation));
        //    doc.IncludeRelation(data, apiResource, nameof(data.ToManyRelation));
        //    var jsonString = JsonConvert.SerializeObject(doc, Formatting.Indented);
        //    var deserialized = JsonConvert.DeserializeObject<JsonApiDocument>(jsonString);

        //    ClassWithoutRelations o = deserialized.ToObject<ClassWithoutRelations>(typeof(ResourceWithoutRelations), out Func<string, bool> foundAttributes);

        //    if (foundAttributes(nameof(ClassWithoutRelations.AttributeThree)))
        //    {
        //        bla
        //    }


        //    IEnumerable<ClassWithoutRelations> os = deserialized.ToObjectCollection<ClassWithoutRelations>(typeof(ResourceWithoutRelations), out Func<int, string, bool> foundAttributes);

        //    if (foundAttributes(0 /* = idx */, nameof(ClassWithoutRelations.AttributeThree) /* = property name */))
        //    {
        //        bla
        //    }

        //}

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
