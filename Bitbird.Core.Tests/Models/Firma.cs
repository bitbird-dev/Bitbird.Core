using Bitbird.Core.JsonApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitbird.Core.Tests.Models
{
    public class Firma : JsonApiBaseModel
    {
        public string FirmenName { get; set; }
        public Fahrer Fahrer { get; set; }

        public IEnumerable<Fahrzeug> Fahrzeuge { get; set; }
    }
}
