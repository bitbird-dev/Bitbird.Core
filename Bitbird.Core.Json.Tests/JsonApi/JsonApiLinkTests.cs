using Bitbird.Core.Json.JsonApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Json.Tests.JsonApi
{
    [TestClass]
    public class JsonApiLinkTests
    {
        private string testUrl = @"http://test.url";
        private string meta = @"muh";

        [ClassInitialize]
        public static void SetupTests(TestContext testContext)
        {
            ApiTests.SetupTests(testContext);
        }

        [TestMethod]
        public void TestLinkSerializationHrefOnly()
        {
            JsonApiLink l_h = new JsonApiLink() { Href = testUrl };
            JValue reference = new JValue(testUrl);

            var result = JsonConvert.SerializeObject(l_h);
            var referenceResult = JsonConvert.SerializeObject(reference);

            Assert.IsTrue(string.Equals(result, referenceResult));
        }

        [TestMethod]
        public void TestLinkSerializationMetaOnly()
        {
            JsonApiLink l_h = new JsonApiLink() { Meta = meta };
            
            Assert.ThrowsException<JsonException>(()=> JsonConvert.SerializeObject(l_h));
        }

        [TestMethod]
        public void TestLinkSerialization()
        {
            JsonApiLink l_h = new JsonApiLink() {Href = testUrl, Meta = meta };
            JObject reference = new JObject(new JProperty("href", testUrl), new JProperty("meta", meta));

            var result = JsonConvert.SerializeObject(l_h);
            var referenceResult = JsonConvert.SerializeObject(reference);

            Assert.IsTrue(string.Equals(result, referenceResult));
        }

        [TestMethod]
        public void TestLinkDeserialization()
        {
            var json = "{ \"href\":\""+ testUrl + "\",\"meta\":\"" + meta + "\"}";
            var result = JsonConvert.DeserializeObject<JsonApiLink>(json);
            Assert.IsTrue(result.Href.Equals(testUrl));
            Assert.IsTrue(result.Meta.ToString().Equals(meta.ToString()));
        }

        [TestMethod]
        public void TestLinkDeserializationHrefOnly()
        {
            var json = "{ \"href\":\"" + testUrl + "\"}";
            var result = JsonConvert.DeserializeObject<JsonApiLink>(json);
            Assert.IsTrue(result.Href.Equals(testUrl));
            Assert.IsTrue(result.Meta == null);
        }

        [TestMethod]
        public void TestLinkDeserializationMetaOnly()
        {
            var json = "{ \"meta\":\"" + meta + "\"}";
            Assert.ThrowsException<JsonException>(() => JsonConvert.DeserializeObject<JsonApiLink>(json));
        }
    }
}
