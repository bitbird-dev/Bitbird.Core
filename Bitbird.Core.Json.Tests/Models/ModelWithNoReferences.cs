﻿using Bitbird.Core.Json.JsonApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Json.Tests.Models
{
    public class ModelWithNoReferences : JsonApiBaseModel
    {
        public string Name { get; set; }

        public int Number { get; set; }
    }
}
