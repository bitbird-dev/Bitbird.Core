using Bitbird.Core.JsonApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Tests.JsonApi
{
    [TestClass]
    public class JsonApiRelationshipObjectBaseTests
    {
        [TestMethod]
        public void ToOneRelationship()
        {
            JsonApiRelationshipObjectBase relationship = new JsonApiToOneRelationshipObject
            {
                Data = new JsonApiResourceIdentifierObject(Guid.NewGuid().ToString(), "whatever"),
                Links = new JsonApiRelationshipLinksObject
                {
                    Self = new JsonApiLink(@"http://test.pro/self"),
                    Related = new JsonApiLink(@"http://test.pro/related")
                }
            };
            string serializedObject = JsonConvert.SerializeObject(relationship, Formatting.Indented);
            var deserializedObject = JsonConvert.DeserializeObject<JsonApiRelationshipObjectBase>(serializedObject);
            Assert.IsTrue((deserializedObject as JsonApiToOneRelationshipObject).Data.Id == (relationship as JsonApiToOneRelationshipObject).Data.Id);
        }

        [TestMethod]
        public void ToManyRelationship()
        {
            JsonApiRelationshipObjectBase relationship = new JsonApiToManyRelationshipObject
            {
                Data = new List<JsonApiResourceIdentifierObject>
                {
                    new JsonApiResourceIdentifierObject(Guid.NewGuid().ToString(), "whatever"),
                    new JsonApiResourceIdentifierObject(Guid.NewGuid().ToString(), "whatever")
                },
                Links = new JsonApiRelationshipLinksObject
                {
                    Self = new JsonApiLink(@"http://test.pro/self"),
                    Related = new JsonApiLink(@"http://test.pro/related")
                }
            };
            string serializedObject = JsonConvert.SerializeObject(relationship, Formatting.Indented);
            var deserializedObject = JsonConvert.DeserializeObject<JsonApiRelationshipObjectBase>(serializedObject);
            Assert.IsTrue((deserializedObject as JsonApiToManyRelationshipObject).Data[0].Id == (relationship as JsonApiToManyRelationshipObject).Data[0].Id);
            Assert.IsTrue((deserializedObject as JsonApiToManyRelationshipObject).Data[1].Id == (relationship as JsonApiToManyRelationshipObject).Data[1].Id);
        }

        [TestMethod]
        public void ToOneRelationship_LinkOnly()
        {
            JsonApiRelationshipObjectBase relationship = new JsonApiToOneRelationshipObject
            {
                Links = new JsonApiRelationshipLinksObject
                {
                    Self = new JsonApiLink(@"http://test.pro/self"),
                    Related = new JsonApiLink(@"http://test.pro/related")
                }
            };
            string json = JsonConvert.SerializeObject(relationship, Formatting.Indented);
        }

        [TestMethod]
        public void ToManyRelationship_LinkOnly()
        {
            JsonApiRelationshipObjectBase relationship = new JsonApiToManyRelationshipObject
            {
                Links = new JsonApiRelationshipLinksObject
                {
                    Self = new JsonApiLink(@"http://test.pro/self"),
                    Related = new JsonApiLink(@"http://test.pro/related")
                }
            };
            string json = JsonConvert.SerializeObject(relationship, Formatting.Indented);
        }
    }
}
