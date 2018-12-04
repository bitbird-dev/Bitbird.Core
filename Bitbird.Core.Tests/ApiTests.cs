using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Bitbird.Core.JsonApi;
using Bitbird.Core.Tests.DataAccess;
using Bitbird.Core.Tests.Models;
using Bitbird.Core.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Bitbird.Core.Tests
{
    [TestClass]
    public class ApiTests
    {

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
        public void JsonApiDocument_TestPrimitveArray()
        {
            var list = new List<string> { "data1", "data2", "data3" };
            var data = new Fahrer { Id = "123", Name = "hansi", Keys = list };
            var testApiDocument = new JsonApiDocument<Fahrer>(data);
            string jsonString = JsonConvert.SerializeObject(testApiDocument);

            var result = JsonConvert.DeserializeObject<JsonApiDocument<Fahrer>>(jsonString);
            Assert.IsNotNull(result);
            var deserialzedModel = result.ExtractData()?.FirstOrDefault();
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
            var deserialzedModel = result.ExtractData();
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
            var deserialzedModel = result.ExtractData();
            Assert.IsNotNull(deserialzedModel);
        }

        [TestMethod]
        public void JsonApiDocument_TestRelationshipIdentifiers()
        {
            var model = GetSomeData().FirstOrDefault();
            var testApiDocument = new JsonApiDocument<Firma>(model);
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new BitbirdCorePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            string jsonString = JsonConvert.SerializeObject(testApiDocument, settings);

            System.Diagnostics.Debug.WriteLine(jsonString);

            var result = JsonConvert.DeserializeObject<JsonApiDocument<Firma>>(jsonString, settings);

            var deserialzedModel = result.ExtractData()?.FirstOrDefault();

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
                ContractResolver = new BitbirdCorePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            string jsonString = JsonConvert.SerializeObject(testApiDocument, settings);

            System.Diagnostics.Debug.WriteLine(jsonString);

            var result = JsonConvert.DeserializeObject<JsonApiDocument<Firma>>(jsonString, settings);

            var deserializedModel = result.ExtractData()?.FirstOrDefault();

            Assert.IsNotNull(deserializedModel);

            Assert.AreEqual(deserializedModel.Fahrer.Name, model.Fahrer.Name);

            Assert.AreEqual(deserializedModel.Fahrer.Keys.Count(), model.Fahrer.Keys.Count());
        }

        [TestMethod]
        public void JsonApiDocument_TestIncludeToMany()
        {
            var model = GetSomeData().FirstOrDefault();
            var testApiDocument = new JsonApiDocument<Firma>(model, new List<PropertyInfo>
            {
                //model.GetType().GetProperty(nameof(model.FahrZeuge)),
                model.GetType().GetProperty(nameof(model.Fahrer))
            });
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new BitbirdCorePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            string jsonString = JsonConvert.SerializeObject(testApiDocument, settings);

            System.Diagnostics.Debug.WriteLine(jsonString);

            var result = JsonConvert.DeserializeObject<JsonApiDocument<Firma>>(jsonString, settings);

            var deserialzedModel = result.ExtractData()?.FirstOrDefault();

            Assert.IsNotNull(deserialzedModel);

            Assert.AreEqual(deserialzedModel.FahrZeuge.Count(), model.FahrZeuge.Count());

            Assert.AreEqual(deserialzedModel.FahrZeuge.First().Id, model.FahrZeuge.First().Id);
        }

        [TestMethod]
        public void TestAccessRestriction()
        {
            TestAccessBroker.Instance.UserGroup = TestAccessGroup.FULL_ACCESS;
            var model = GetSomeData().FirstOrDefault();
            var testApiDocument = new JsonApiDocument<Firma>(model);
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = new BitbirdCorePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
            string jsonString = JsonConvert.SerializeObject(testApiDocument, settings);
            var result = JsonConvert.DeserializeObject<JsonApiDocument<Firma>>(jsonString, settings);
            var deserialzedModel1 = result.ExtractData()?.FirstOrDefault();

            Assert.IsNotNull(deserialzedModel1.FirmenName);

            TestAccessBroker.Instance.UserGroup = TestAccessGroup.LIMITED_ACCES;
            var testApiDocument2 = new JsonApiDocument<Firma>(model);
            string jsonString2 = JsonConvert.SerializeObject(testApiDocument2, settings);
            var result2 = JsonConvert.DeserializeObject<JsonApiDocument<Firma>>(jsonString2, settings);
            var deserialzedModel2 = result2.ExtractData()?.FirstOrDefault();

            Assert.IsNull(deserialzedModel2.FirmenName);
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
