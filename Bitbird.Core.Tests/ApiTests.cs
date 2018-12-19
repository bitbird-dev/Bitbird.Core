using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Bitbird.Core.JsonApi;
using Bitbird.Core.JsonApi.Dictionaries;
using Bitbird.Core.Tests.Models;
using Bitbird.Core.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Bitbird.Core.JsonApi.Converters;

namespace Bitbird.Core.Tests
{
    internal class ClassA : JsonApiBaseModel
    {
        public string AName { get; set; }

        public ClassB BReference { get; set; }
    }

    internal class ClassB : JsonApiBaseModel
    {
        public string BName { get; set; }
        public ClassC CReference { get; set; }
    }

    internal class ClassC : JsonApiBaseModel
    {
        public string CName { get; set; }
    }


    [TestClass]
    public class ApiTests
    {
        [ClassInitialize]
        public static void SetupTests(TestContext testContext)
        {
            BtbrdCoreIdConverters.AddConverter(new BtbrdCoreIdConverter<string>(toString => toString, toId => toId));
        }


        #region TypeUtils

        [TestMethod]
        public void IsNonStringEnumerable_Test()
        {
            var targets = new List<object> { "testString", new List<string> { "testString", "testString" }, new Dictionary<string, string>() };
            Assert.IsFalse(typeof(string).IsNonStringEnumerable());
            Assert.IsFalse(typeof(String).IsNonStringEnumerable());
            Assert.IsTrue(typeof(List<object>).IsNonStringEnumerable());
            Assert.IsTrue(typeof(Dictionary<string, object>).IsNonStringEnumerable());
        }

        #endregion

        [TestMethod]
        public void JsonApiDocument_TestResourceKeyCollision()
        {
            ResourceKey k1 = new ResourceKey("123", "classA");
            ResourceKey k2 = new ResourceKey("123", "classB");
            ResourceKey k3 = new ResourceKey("123", "classB");

            Assert.IsFalse(k1.Equals(k2));
            Assert.IsTrue(k2.Equals(k3));
            Assert.IsFalse(k1.GetHashCode() == k2.GetHashCode());
            Assert.IsTrue(k2.GetHashCode() == k3.GetHashCode());
        }
        

        [TestMethod]
        public void JsonApiDocument_TestNestedIncludes()
        {
            // A has a reference to B
            // B has a reference to C
            // B AND C ARE INCLUDED
            var data = new ClassA
            {
                Id = Guid.NewGuid().ToString(),
                AName = "A",
                BReference = new ClassB
                {
                    Id = Guid.NewGuid().ToString(),
                    BName = "B",
                    CReference = new ClassC
                    {
                        Id = Guid.NewGuid().ToString(),
                        CName = "C"
                    }
                }
            };

            var doc = new JsonApiDocument<ClassA>(data);
            doc.Included.AddResource(new JsonApiResourceObject(data.BReference));
            doc.Included.AddResource(new JsonApiResourceObject(data.BReference.CReference));
            string jsonString = JsonConvert.SerializeObject(doc, Formatting.Indented);
            var deserializedDocument = JsonConvert.DeserializeObject<JsonApiDocument<ClassA>>(jsonString);

            var aResource = deserializedDocument.Data.FirstOrDefault();
            Assert.IsNotNull(aResource);

            var aObject = aResource.ToObject<ClassA>();
            Assert.IsNotNull(aObject);
            aObject.BReference = deserializedDocument.Included.GetResource(aObject.BReference.Id, aObject.BReference.GetJsonApiClassName()).ToObject<ClassB>();
            Assert.IsTrue(aObject.BReference.BName == data.BReference.BName);
            aObject.BReference.CReference = deserializedDocument.Included.GetResource(aObject.BReference.CReference.Id, aObject.BReference.CReference.GetJsonApiClassName()).ToObject<ClassC>();
            Assert.IsTrue(aObject.BReference.CReference.CName == data.BReference.CReference.CName);
        }

        [TestMethod]
        public void JsonApiDocument_TestPrimitveArray()
        {
            var list = new List<string> { "data1", "data2", "data3" };
            var data = new Fahrer { Id = "123", Name = "hansi", Keys = list };
            var testApiDocument = new JsonApiDocument<Fahrer>(data);
            string jsonString = JsonConvert.SerializeObject(testApiDocument);

            var result = JsonConvert.DeserializeObject<JsonApiDocument<Fahrer>>(jsonString);
            Assert.IsNotNull(result);
            var deserialzedModel = result.ToObject()?.FirstOrDefault();
            Assert.IsNotNull(deserialzedModel);
            Assert.AreEqual(list.Count, deserialzedModel.Keys.Count());
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(list[i], deserialzedModel.Keys.ElementAt(i));
            }
        }

        [TestMethod]
        public void JsonApiDocument_DataArray()
        {
            var data = GetSomeData();
            var testApiDocument = new JsonApiDocument<Firma>(data);
            string jsonString = JsonConvert.SerializeObject(testApiDocument, Formatting.Indented);

            var result = JsonConvert.DeserializeObject<JsonApiDocument<Firma>>(jsonString);
            Assert.IsNotNull(result);
            var deserialzedModel = result.ToObject();
            Assert.IsNotNull(deserialzedModel);
            Assert.AreEqual(deserialzedModel.Count(), data.Count());
        }

        [TestMethod]
        public void JsonApiDocument_TestSelfLinks()
        {
            // create test data
            var model = GetSomeData();

            // setup Query url
            Uri queryUri = (new UriBuilder { Host = "localhost", Path = "api/firma"}).Uri;

            // create JsonDocument
            var jsonDocument = new JsonApiDocument<Firma>(model, new List<PropertyInfo> { typeof(Firma).GetProperty(nameof(Firma.Fahrer)) }, queryUri);
            jsonDocument.Links.Related = new JsonApiLink("test", 1);
            var jsonString = JsonConvert.SerializeObject(jsonDocument, Formatting.Indented);
            var result = JsonConvert.DeserializeObject<JsonApiDocument<Firma>>(jsonString);
            Assert.IsNotNull(result);
            var deserialzedModel = result.ToObject();
            Assert.IsNotNull(deserialzedModel);
        }

        [TestMethod]
        public void JsonApiDocument_TestRelationshipIdentifiers()
        {
            var model = GetSomeData().FirstOrDefault();
            var testApiDocument = new JsonApiDocument<Firma>(model);
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            string jsonString = JsonConvert.SerializeObject(testApiDocument, settings);

            System.Diagnostics.Debug.WriteLine(jsonString);

            var result = JsonConvert.DeserializeObject<JsonApiDocument<Firma>>(jsonString, settings);

            var deserialzedModel = result.ToObject()?.FirstOrDefault();

            Assert.IsNotNull(deserialzedModel);

            Assert.AreEqual(deserialzedModel.Fahrer.Id, model.Fahrer.Id);

            Assert.AreEqual(deserialzedModel.FahrZeuge.Count(), model.FahrZeuge.Count());
        }

        [TestMethod]
        public void JsonApiDocument_TestInclude()
        {
            var model = GetSomeData().FirstOrDefault();
            var testApiDocument = new JsonApiDocument<Firma>(model, new List<PropertyInfo> { model.GetType().GetProperty(nameof(model.Fahrer)) });
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            string jsonString = JsonConvert.SerializeObject(testApiDocument, settings);

            System.Diagnostics.Debug.WriteLine(jsonString);

            var result = JsonConvert.DeserializeObject<JsonApiDocument<Firma>>(jsonString, settings);

            var deserializedModel = result.ToObject()?.FirstOrDefault();

            Assert.IsNotNull(deserializedModel);

            Assert.AreEqual(deserializedModel.Fahrer.Name, model.Fahrer.Name);

            Assert.AreEqual(deserializedModel.Fahrer.Keys.Count(), model.Fahrer.Keys.Count());
        }

        [TestMethod]
        public void JsonApiDocument_TestIncludeToMany()
        {
            //var model = GetSomeData().FirstOrDefault();
            var model = new Firma
            {
                Id = Guid.NewGuid().ToString(),
                FirmenName = "bitbird",
                FahrZeuge = new List<Fahrzeug> { new Fahrzeug { Id = Guid.NewGuid().ToString(), Kilometerstand = 4000 }, new Fahrzeug { Id = Guid.NewGuid().ToString(), Kilometerstand = 10000 } },
                Fahrer = new Fahrer { Id = Guid.NewGuid().ToString(), Name = "Christian", Keys = new List<string> { "key0", "key1", "key2" } }
            };
            var testApiDocument = new JsonApiDocument<Firma>(model, new List<PropertyInfo>
            {
                model.GetType().GetProperty(nameof(model.Fahrer)),model.GetType().GetProperty(nameof(model.FahrZeuge))
            });
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            string jsonString = JsonConvert.SerializeObject(testApiDocument, settings);

            System.Diagnostics.Debug.WriteLine(jsonString);

            var result = JsonConvert.DeserializeObject<JsonApiDocument<Firma>>(jsonString, settings);

            var deserialzedModel = result.ToObject()?.FirstOrDefault();

            Assert.IsNotNull(deserialzedModel);

            Assert.AreEqual(deserialzedModel.FahrZeuge.Count(), model.FahrZeuge.Count());

            Assert.AreEqual(deserialzedModel.FahrZeuge.First().Id, model.FahrZeuge.First().Id);
        }

        #region Data

        private IEnumerable<Firma> GetSomeData()
        {
            return new List<Firma>{
                new Firma
                {
                    Id = "0",
                    FirmenName = "bitbird",
                    FahrZeuge = new List<Fahrzeug> { new Fahrzeug { Id = "0", Kilometerstand = 4000 }, new Fahrzeug { Id = "1", Kilometerstand = 10000 } },
                    Fahrer = new Fahrer { Id = "0", Name = "Christian", Keys = new List<string> { "key0", "key1", "key2" } }
                },
                new Firma
                {
                    Id = "1",
                    FirmenName = "Rohrmax",
                    FahrZeuge = new List<Fahrzeug> { new Fahrzeug { Id = "2", Kilometerstand = 400 }, new Fahrzeug { Id = "3", Kilometerstand = 1000 } },
                    Fahrer = new Fahrer { Id = "1", Name = "Alex", Keys = new List<string> { "key3", "key4", "key5" } }
                },
            };
        }

        #endregion
    }
}
