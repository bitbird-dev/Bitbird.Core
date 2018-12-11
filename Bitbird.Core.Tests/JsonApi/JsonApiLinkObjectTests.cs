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
    public class JsonApiLinkObjectTests
    {
        private string testUrl = @"http://test.url";
        private string meta = @"muh";

        [TestMethod]
        public void TestLinkObjectSerialization()
        {
            JsonApiLinksObject l = new JsonApiLinksObject {
                Self = new JsonApiLink { Href = testUrl },
                First = new JsonApiLink { Href = testUrl },
                Last = new JsonApiLink { Href = testUrl },
                Next = new JsonApiLink { Href = testUrl },
                Prev = new JsonApiLink { Href = testUrl },
                Related = new JsonApiLink { Href = testUrl }
            };
            ;
            JObject reference = new JObject(
                new JProperty("self", testUrl),
                new JProperty("related", testUrl),
                new JProperty("prev", testUrl),
                new JProperty("next", testUrl),
                new JProperty("first", testUrl),
                new JProperty("last", testUrl)
                );
            var result = JsonConvert.SerializeObject(l, Formatting.Indented);
            var referenceResult = JsonConvert.SerializeObject(reference, Formatting.Indented);

            Assert.IsTrue(string.Equals(result, referenceResult));
        }

        [TestMethod]
        public void TestLinkObjectSerializationWithMeta()
        {
            JsonApiLinksObject l = new JsonApiLinksObject
            {
                Self = new JsonApiLink { Href = testUrl, Meta = meta },
                First = new JsonApiLink { Href = testUrl, Meta = meta },
                Last = new JsonApiLink { Href = testUrl, Meta = meta },
                Next = new JsonApiLink { Href = testUrl, Meta = meta },
                Prev = new JsonApiLink { Href = testUrl, Meta = meta },
                Related = new JsonApiLink { Href = testUrl, Meta = meta }
            };
            ;
            JObject reference = new JObject(
                new JProperty("self", JObject.FromObject(new JsonApiLink { Href = testUrl, Meta = meta })),
                new JProperty("related", JObject.FromObject(new JsonApiLink { Href = testUrl, Meta = meta })),
                new JProperty("prev", JObject.FromObject(new JsonApiLink { Href = testUrl, Meta = meta })),
                new JProperty("next", JObject.FromObject(new JsonApiLink { Href = testUrl, Meta = meta })),
                new JProperty("first", JObject.FromObject(new JsonApiLink { Href = testUrl, Meta = meta })),
                new JProperty("last", JObject.FromObject(new JsonApiLink { Href = testUrl, Meta = meta }))
                );
            var result = JsonConvert.SerializeObject(l, Formatting.Indented);
            var referenceResult = JsonConvert.SerializeObject(reference, Formatting.Indented);

            Assert.IsTrue(string.Equals(result, referenceResult));
        }
    }
}
