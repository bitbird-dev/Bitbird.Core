using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    public interface IBitbirdCoreJsonApiContractResolver : IContractResolver
    {
        string ResolveRelationshipName(string propertyName);
    }
}
