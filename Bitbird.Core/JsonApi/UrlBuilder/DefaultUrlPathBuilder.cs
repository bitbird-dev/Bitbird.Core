using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Json.JsonApi.UrlBuilder
{
    public class DefaultUrlPathBuilder : IUrlPathBuilder
    {
        private readonly string _prefix;

        #region Constructor

        public DefaultUrlPathBuilder() : this(string.Empty)
        {

        }

        public DefaultUrlPathBuilder(string prefix)
        {
            _prefix = prefix;
        }

        #endregion

        public virtual string BuildCanonicalPath(Type resources)
        {
            return '/'.TrimJoin(_prefix, resources.GetJsonApiClassName())
                .EnsureStartsWith("/")
                .EnsureEndsWith("/");
        }

        public virtual string BuildCanonicalPath(IJsonApiDataModel resource)
        {
            return '/'.TrimJoin(_prefix, resource.GetJsonApiClassName(), resource.GetIdAsString())
                .EnsureStartsWith("/")
                .EnsureEndsWith("/");
        }

        public virtual string BuildRelationshipPath(IJsonApiDataModel resource, PropertyInfo relatedProperty)
        {
            return '/'.TrimJoin(_prefix, resource.GetJsonApiClassName(), resource.GetIdAsString(), relatedProperty.Name)
                .EnsureStartsWith("/")
                .EnsureEndsWith("/");
        }
    }
}
