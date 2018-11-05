using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Tests.DataAccess
{

    public enum TestAccessGroup
    {
        LIMITED_ACCES,
        FULL_ACCESS
    }

    public class TestAccessBroker
    {
        private static TestAccessBroker _instance;

        public static TestAccessBroker Instance
        {
            get => _instance ?? (_instance = new TestAccessBroker());
        }

        public TestAccessGroup UserGroup { get; set; } = TestAccessGroup.FULL_ACCESS;

        private TestAccessBroker()
        {
        }
    }
}
