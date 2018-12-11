using Bitbird.Core.JsonApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Tests.JsonApi
{
    [TestClass]
    public class JsonApiLinkTests
    {
        private string testUrl = @"http://test.url";
        private string meta = @"muh";

        [TestMethod]
        public void TestLinkSerializationHrefOnly()
        {
            JsonApiLink l_h = new JsonApiLink() { Href = testUrl };
            JObject reference = new JObject(new JProperty("href", testUrl));

            var result = JsonConvert.SerializeObject(l_h);
            var referenceResult = JsonConvert.SerializeObject(reference);

            Assert.IsTrue(string.Equals(result, referenceResult));
        }

        [TestMethod]
        public void TestLinkSerializationMetaOnly()
        {
            JsonApiLink l_h = new JsonApiLink() { Meta = meta };
            JObject reference = new JObject(new JProperty("href", null),new JProperty("meta", meta));

            var result = JsonConvert.SerializeObject(l_h);
            var referenceResult = JsonConvert.SerializeObject(reference);

            Assert.IsTrue(string.Equals(result, referenceResult));
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
    }
}
