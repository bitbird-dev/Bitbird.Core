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

    public class JsonAccessBroker
    {
        private static JsonAccessBroker _instance;

        public static JsonAccessBroker Instance
        {
            get => _instance ?? (_instance = new JsonAccessBroker());
        }

        public TestAccessGroup UserGroup { get; set; } = TestAccessGroup.FULL_ACCESS;

        private JsonAccessBroker()
        {
        }
    }
}
