using Bitbird.Core.JsonApi;
using Bitbird.Core.Extensions;
using Bitbird.Core.JsonApi.Dictionaries;
using Bitbird.Core.Tests.Models;
using Bitbird.Core.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bitbird.Core.JsonApi.Converters;
using Newtonsoft.Json.Linq;

namespace Bitbird.Core.Tests.JsonApi
{
    

    [TestClass]
    public class JsonApiResourceObjectTests
    {
        [ClassInitialize]
        public static void SetupTests(TestContext testContext)
        {
            ApiTests.SetupTests(testContext);
        }

        [TestMethod]
        public void JsonApiResourceObject_ManuallyAddRelations()
        {
            var data = new ModelWithReferences
            {
                Id = Guid.NewGuid().ToString(),
                SingleReference = new ModelWithNoReferences { Id = Guid.NewGuid().ToString() },
                CollectionReference = new List<ModelWithNoReferences>
                {
                    new ModelWithNoReferences { Id = Guid.NewGuid().ToString()},
                    new ModelWithNoReferences { Id = Guid.NewGuid().ToString()}
                }
            };
            var resourceObject = new JsonApiResourceObject(data, false);
            resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>();
            resourceObject.Relationships.Add("single-reference", new JsonApiToOneRelationshipObject{ Data =  new JsonApiResourceIdentifierObject(data.SingleReference.Id, data.SingleReference.GetType().GetJsonApiClassName()) });
            resourceObject.Relationships.Add("collection-reference", new JsonApiToManyRelationshipObject { Data = data.CollectionReference.Select(i => new JsonApiResourceIdentifierObject(i.Id, i.GetType().GetJsonApiClassName())).ToList() });
            
            //resourceObject.Attributes = (data, false, x => x.SingleReference);
            var json = JsonConvert.SerializeObject(resourceObject, Formatting.Indented);
            var deserialized = JsonConvert.DeserializeObject<JsonApiResourceObject>(json);
            var result = deserialized.ToObject<ModelWithReferences>();
            Assert.IsTrue(result.SingleReference.Id == data.SingleReference.Id);
            Assert.IsTrue(result.CollectionReference.Count() == data.CollectionReference.Count());
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
