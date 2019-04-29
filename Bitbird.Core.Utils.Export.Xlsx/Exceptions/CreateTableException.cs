using System;
using System.Runtime.Serialization;

namespace Bitbird.Core.Export.Xlsx
{
    public class CreateTableException : Exception
    {
        public string ParameterPath { get; private set; }

        public CreateTableException()
        {
        }

        public CreateTableException(string message) : base(message)
        {
        }

        public CreateTableException(string parameterPath, string detailedMessage, Exception innerException) : base(detailedMessage, innerException)
        {
            ParameterPath = detailedMessage;
        }

        public CreateTableException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CreateTableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
