using System;

namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    public class HubClientMethodMetaData
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly string name;
        public readonly Type ReturnType;
        public readonly Type[] ParameterTypes;

        public HubClientMethodMetaData(string name, Type returnType, Type[] parameterTypes)
        {
            this.name = name;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
        }
    }
}