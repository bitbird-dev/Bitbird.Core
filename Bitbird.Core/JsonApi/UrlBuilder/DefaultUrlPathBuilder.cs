using Bitbird.Core.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.JsonApi.UrlBuilder
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
            return '/'.TrimJoin(_prefix, JsonApiBaseModel.GetJsonApiClassName(resources))
                .EnsureStartsWith("/")
                .EnsureEndsWith("/");
        }

        public virtual string BuildCanonicalPath(JsonApiBaseModel resource)
        {
            return '/'.TrimJoin(_prefix, resource.GetJsonApiClassName(), resource.Id)
                .EnsureStartsWith("/")
                .EnsureEndsWith("/");
        }

        public virtual string BuildRelationshipPath(JsonApiBaseModel resource, PropertyInfo relatedProperty)
        {
            return '/'.TrimJoin(_prefix, resource.GetJsonApiClassName(), resource.Id, relatedProperty.Name)
                .EnsureStartsWith("/")
                .EnsureEndsWith("/");
        }
    }
}
