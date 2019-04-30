using System;
using Bitbird.Core.Json.Helpers.ApiResource;

namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    public class ModelClassMetaData
    {
        public readonly Type Resource;
        public readonly JsonApiResource Instance;
        public readonly Type Model;
        public readonly bool IsDefaultDeserializer;
        public readonly bool IsForDataTransferOnly;

        public ModelClassMetaData(Type resource, JsonApiResource instance, Type model, bool isDefaultDeserializer, bool isForDataTransferOnly)
        {
            Resource = resource;
            Instance = instance;
            Model = model;
            IsDefaultDeserializer = isDefaultDeserializer;
            IsForDataTransferOnly = isForDataTransferOnly;
        }
    }
}