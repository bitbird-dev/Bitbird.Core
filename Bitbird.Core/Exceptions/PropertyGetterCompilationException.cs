using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Bitbird.Core.Exceptions
{
    class PropertyGetterCompilationException : Exception
    {
        public PropertyGetterCompilationException()
        {
        }

        public PropertyGetterCompilationException(string message) : base(message)
        {
        }

        public PropertyGetterCompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PropertyGetterCompilationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
