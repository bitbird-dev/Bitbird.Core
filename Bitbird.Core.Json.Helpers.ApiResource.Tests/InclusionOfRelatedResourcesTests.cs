using Bitbird.Core.Json.Helpers.ApiResource.Extensions;
using Bitbird.Core.Json.Helpers.ApiResource.Tests.TestModel;
using Bitbird.Core.Json.Helpers.Base.Converters;
using Bitbird.Core.Json.JsonApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Json.Helpers.ApiResource.Tests
{
    [TestClass]
    public class InclusionOfRelatedResourcesTests
    {
        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            BtbrdCoreIdConverters.AddConverter(new BtbrdCoreIdConverter<Guid>(g => g.ToString(), gs => Guid.Parse(gs??string.Empty)));
        }
        
        [TestMethod]
        public void IncludeDeeplyNestedResourceTest()
        {
            // Setup
            var data = TestModelRepository.GetIncludeDeeplyNestedResourceTestData();
            var jsonDocument = JsonApiDocumentExtensions.CreateDocumentFromApiResource<TestModelCompoundApiResource>(data);

            // add includes
            jsonDocument.IncludeRelation<TestModelCompoundApiResource>(data,"bigData.children.toOne"); // nicht generisch

            // Serialize
            var jsonString = JsonConvert.SerializeObject(jsonDocument, Formatting.Indented);
            
            // Deserialize
            var deserializedDocument = JsonConvert.DeserializeObject<JsonApiDocument>(jsonString);

            // Retrieve Data
            var retrievedData = deserializedDocument.ToObject<TestModelCompound, TestModelCompoundApiResource>();
            retrievedData.BigData = deserializedDocument.GetIncludedResource<TestModelToN, TestModelToNApiResource>(retrievedData.BigDataId);
            retrievedData.BigData.Children = retrievedData.BigData.ChildrenIds.Select(item => deserializedDocument.GetIncludedResource<TestModelToOne,TestModelToOneApiResource>(item));
            retrievedData.SmallData = deserializedDocument.GetIncludedResource<TestModelToOne, TestModelToOneApiResource>(retrievedData.SmallDataId);
            
            // Compare
            Assert.AreEqual(data.MyIdProperty, retrievedData.MyIdProperty);
        }


    }
}
