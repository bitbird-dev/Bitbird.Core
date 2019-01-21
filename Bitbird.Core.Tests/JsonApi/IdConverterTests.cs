using System;
using Bitbird.Core.Json.Helpers.JsonDataModel;
using Bitbird.Core.Json.JsonApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Bitbird.Core.Json.Tests.JsonApi
{
    [TestClass]
    public class IdConverterTests
    {
        [ClassInitialize]
        public static void SetupTests(TestContext testContext)
        {
            ApiTests.SetupTests(testContext);
        }

        class arr : IJsonApiDataModel<int?>
        {
            public string MyProperty { get; set; }
            public int? Id { get ; set ; }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var modelIdNull = new arr();
            var modelIdNotNull = new arr { Id = 112314, MyProperty = "muh" };
            var docIdNull = new JsonApiCollectionDocument<arr>(modelIdNull);
            var docIdNotNull = new JsonApiCollectionDocument<arr>(modelIdNotNull);

            var jsonIdNull = JsonConvert.SerializeObject(docIdNull);
            var jsonIdNotNull = JsonConvert.SerializeObject(docIdNotNull);

            var resultIdNull = JsonConvert.DeserializeObject<arr>(jsonIdNull);
            var resultIdNotNull = JsonConvert.DeserializeObject<arr>(jsonIdNotNull);
        }
    }
}
