using System;

namespace Bitbird.Core.WebApi.Net
{
    /// <summary>
    /// Maps a resource to a model class.
    /// Can be used to identify resources for a model class and vice-versa if a meta-data-structure is built.
    /// Not all resources with this attribute have their own controller.
    /// Some resources are only used during data-transfer (serialization and deserialization).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonApiResourceMappingAttribute : Attribute
    {
        /// <summary>
        /// The model type that should be associated with the resource class that is assigned this resource.
        /// </summary>
        public readonly Type Type;
        /// <summary>
        /// Whether this resource is only used for data-transfer (serialization and deserialization), or has its own controller to be managed with.
        /// </summary>
        public readonly bool DataTransferOnly;

        /// <summary>
        /// Maps a resource to a model class.
        /// Can be used to identify resources for a model class and vice-versa if a meta-data-structure is built.
        /// Not all resources with this attribute have their own controller.
        /// Some resources are only used during data-transfer (serialization and deserialization).
        /// </summary>
        /// <param name="type">The model type that should be associated with the resource class that is assigned this resource.</param>
        /// <param name="dataTransferOnly">Whether this resource is only used for data-transfer (serialization and deserialization), or has its own controller to be managed with.</param>
        public JsonApiResourceMappingAttribute(Type type, bool dataTransferOnly)
        {
            Type = type;
            DataTransferOnly = dataTransferOnly;
        }
    }
}