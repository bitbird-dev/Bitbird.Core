using Bitbird.Core.Utils;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    public class DasherizedNamingStrategy : NamingStrategy
    {
        protected override string ResolvePropertyName(string name)
        {
            return StringUtils.ToSnakeCase(name);
        }
    }
}
