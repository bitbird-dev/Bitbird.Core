using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi.Attributes
{
    public class JsonApiControllerRouteAttribute : Attribute
    {
        public string ControllerRoute { get; set; }
        public JsonApiControllerRouteAttribute(string controllerRoute)
        {
            ControllerRoute = controllerRoute;
        }
    }
}
