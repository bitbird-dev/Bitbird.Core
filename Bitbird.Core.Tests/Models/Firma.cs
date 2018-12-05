using Bitbird.Core.JsonApi;
using Bitbird.Core.JsonApi.Attributes;
using Bitbird.Core.Tests.DataAccess;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Tests.Models
{
    [JsonApiClass("firmacustom")]
    public class Firma : JsonApiBaseModel
    {
        [JsonAccessRestricted]
        public string FirmenName { get; set; }
        
        public Fahrer Fahrer { get; set; }
        
        public IEnumerable<Fahrzeug> FahrZeuge { get; set; }

        public override bool IsPropertyAccessible(PropertyInfo propertyInfo)
        {

            if(propertyInfo.Name == nameof(FirmenName))
            {
                return TestAccessBroker.Instance.UserGroup == TestAccessGroup.FULL_ACCESS;
            }
            return true;
        }
    }
    

    //public class JsonAccessRightRestricted : JsonAccessRestrictedAttribute
    //{
    //    private TestAccessGroup myVar;

    //    public TestAccessGroup MyProperty
    //    {
    //        get { return myVar; }
    //        set { myVar = value; }
    //    }

    //    public JsonAccessRightRestricted(TestAccessGroup level)
    //    {
    //        myVar = level;
    //    }
    //}
}
