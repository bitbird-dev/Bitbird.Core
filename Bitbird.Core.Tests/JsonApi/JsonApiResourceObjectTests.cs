using Bitbird.Core.JsonApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Tests.JsonApi
{
    [TestClass]
    public class JsonApiResourceObjectTests
    {
        class ClassA : JsonApiBaseModel
        {
            public string AName { get; set; }

            public ClassB BReference { get; set; }
        }

        class ClassB : JsonApiBaseModel
        {
            public string BName { get; set; }
            public ClassC CReference { get; set; }
        }

        class ClassC : JsonApiBaseModel
        {
            public string CName { get; set; }
        }

        [TestMethod]
        public void Test()
        {

        }
    }
}
