using System;

namespace Bitbird.Core.WebApi.JsonApi
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
        public readonly bool IsForDataTransferOnly;
        /// <summary>
        /// Whether this resource is used for deserialization of the passed model, if no other resource was specified.
        /// </summary>
        public readonly bool IsDefaultDeserializer;

        /// <summary>
        /// Maps a resource to a model class.
        /// Can be used to identify resources for a model class and vice-versa if a meta-data-structure is built.
        /// Not all resources with this attribute have their own controller.
        /// Some resources are only used during data-transfer (serialization and deserialization).
        /// </summary>
        /// <param name="type">The model type that should be associated with the resource class that is assigned this resource.</param>
        /// <param name="isForDataTransferOnly">Whether this resource is only used for data-transfer (serialization and deserialization), or has its own controller to be managed with.</param>
        /// <param name="isDefaultDeserializer">Whether this resource is used for deserialization of the passed model, if no other resource was specified.</param>
        public JsonApiResourceMappingAttribute(Type type, bool isForDataTransferOnly, bool isDefaultDeserializer = true)
        {
            Type = type;
            IsForDataTransferOnly = isForDataTransferOnly;
            IsDefaultDeserializer = isDefaultDeserializer;
        }
    }
}