using System;
using Bitbird.Core.WebApi.Controllers;

namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    public class ControllerMetaData : IEquatable<ControllerMetaData>
    {
        public readonly Type ControllerType;
        public readonly string FriendlyName;
        public readonly bool IsObsolete;
        public readonly string Category;
        public readonly string RoutePrefix;
        public readonly IControllerBase Instance;
        public readonly IReadControllerBase ReadInstance;
        public readonly ICrudControllerBase CrudInstance;

        public ControllerMetaData(Type controllerType, string friendlyName, bool isObsolete, string category, string routePrefix, IControllerBase instance)
        {
            ControllerType = controllerType;
            FriendlyName = friendlyName;
            IsObsolete = isObsolete;
            Category = category;
            RoutePrefix = routePrefix;
            Instance = instance;
            ReadInstance = instance as IReadControllerBase;
            CrudInstance = instance as ICrudControllerBase;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ControllerMetaData) obj);
        }
        /// <inheritdoc />
        public bool Equals(ControllerMetaData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ControllerType == other.ControllerType;
        }
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (ControllerType != null ? ControllerType.GetHashCode() : 0);
        }
    }
}