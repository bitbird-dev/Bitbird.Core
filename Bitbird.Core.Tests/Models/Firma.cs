using Bitbird.Core.JsonApi;
using Bitbird.Core.JsonApi.Attributes;
using Bitbird.Core.Tests.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Tests.Models
{
    public class Firma : JsonApiBaseModel
    {
        [JsonAccessRestricted]
        public string FirmenName { get; set; }

        
        public Fahrer Fahrer { get; set; }
        
        public IEnumerable<Fahrzeug> Fahrzeuge { get; set; }

        public override bool IsPropertyAccessible(PropertyInfo propertyInfo)
        {
            if(propertyInfo.Name == nameof(FirmenName))
            {
                return JsonAccessBroker.Instance.UserGroup == TestAccessGroup.FULL_ACCESS;
            }
            return true;
        }
    }
}
