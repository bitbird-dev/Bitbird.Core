using Bitbird.Core.JsonApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Tests.Models
{
    public class ModelWithSelfReference : JsonApiBaseModel
    {
        public ModelWithSelfReference Name { get; set; }
    }
}
