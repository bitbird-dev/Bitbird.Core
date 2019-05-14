using System;
using Bitbird.Core.Json.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitbird.Core.Tests
{
    public class TestOuter
    {
        public TestInner TestInnerInstance { get; set; }
    }

    public class TestInner
    {
        public TestInnerInner[] TestInnerInnerInstances { get; set; }
    }

    public class TestInnerInner
    {
        public TestInnerInnerInner TestInnerInnerInnerInstance { get; set; }
    }

    public class TestInnerInnerInner
    {
        public string Mek { get; set; }
    }

    [TestClass]
    public class ApiAttributeErrorTests
    {
        [TestMethod]
        public void ApiAttributeErrorExpression()
        {
            var idx = 27;
            var expression = new ApiAttributeError<TestOuter>(x =>
                x.TestInnerInstance.TestInnerInnerInstances[idx].TestInnerInnerInnerInstance.Mek, "");

            Assert.AreEqual(expression.AttributeName, $".TestInnerInstance.TestInnerInnerInstances[{idx}].TestInnerInnerInnerInstance.Mek");

            var jsonApiExpression = expression.AttributeName
                .Replace('[', '/')
                .Replace(']', '/')
                .Replace('.', '/')
                .Replace("//", "/")
                .FromCamelCaseToJsonCamelCase();

            Console.WriteLine(jsonApiExpression);
        }
    }
}
