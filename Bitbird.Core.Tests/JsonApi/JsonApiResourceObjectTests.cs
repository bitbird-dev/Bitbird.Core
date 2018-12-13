using Bitbird.Core.JsonApi;
using Bitbird.Core.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Tests.JsonApi
{
    [TestClass]
    public class JsonApiResourceObjectTests
    {
        [TestMethod]
        public void JsonApiResourceObject_ManuallyAddAttributesAndRelations()
        {
            var data = new ModelWithReferences { Id = Guid.NewGuid().ToString(), SingleReference = new ModelWithNoReferences { Id = Guid.NewGuid().ToString()} };
            var resourceObject = new JsonApiResourceObject();
            resourceObject.SetAttributes(data, false, x => x.SingleReference);
            var json = JsonConvert.SerializeObject(resourceObject, Formatting.Indented);
            var deserialized = JsonConvert.DeserializeObject<JsonApiResourceObject>(json);
            var result = deserialized.ToObject<ModelWithReferences>();
            Assert.IsTrue(result.SingleReference.Id == data.SingleReference.Id);

            resourceObject = new JsonApiResourceObject(data);
            json = JsonConvert.SerializeObject(resourceObject, Formatting.Indented);
            deserialized = JsonConvert.DeserializeObject<JsonApiResourceObject>(json);
            result = deserialized.ToObject<ModelWithReferences>();
            Assert.IsTrue(result.Id == data.Id);
            Assert.IsTrue(result.SingleReference.Id == data.SingleReference.Id);
        }

        [TestMethod]
        public void JsonApiResourceObject_AutoAddAttributesAndRelations()
        {
            var data = new ModelWithReferences { Id = Guid.NewGuid().ToString(), SingleReference = new ModelWithNoReferences { Id = Guid.NewGuid().ToString() } };
            var resourceObject = new JsonApiResourceObject(data);
            var json = JsonConvert.SerializeObject(resourceObject, Formatting.Indented);
            var deserialized = JsonConvert.DeserializeObject<JsonApiResourceObject>(json);
            var result = deserialized.ToObject<ModelWithReferences>();
            Assert.IsTrue(result.Id == data.Id);
            Assert.IsTrue(result.SingleReference.Id == data.SingleReference.Id);
        }

        [TestMethod]
        public void JsonApiResourceObject_AutoAddAttributes()
        {
            var data = new ModelWithReferences { Id = Guid.NewGuid().ToString(), SingleReference = new ModelWithNoReferences { Id = Guid.NewGuid().ToString() } };
            var resourceObject = new JsonApiResourceObject(data, null, false);
            var json = JsonConvert.SerializeObject(resourceObject, Formatting.Indented);
            var deserialized = JsonConvert.DeserializeObject<JsonApiResourceObject>(json);
            var result = deserialized.ToObject<ModelWithReferences>();
            Assert.IsTrue(result.Id == data.Id);
            Assert.IsNull(result.SingleReference);
        }
    }
}
