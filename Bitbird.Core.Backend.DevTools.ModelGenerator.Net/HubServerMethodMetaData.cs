using System;

namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    public class HubServerMethodMetaData
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly string name;
        public readonly Type ReturnType;
        public readonly Type[] ParameterTypes;

        public HubServerMethodMetaData(string name, Type returnType, Type[] parameterTypes)
        {
            this.name = name;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
        }
    }
}