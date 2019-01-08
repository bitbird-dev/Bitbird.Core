using System;

namespace Bitbird.Core.Exceptions
{
    public class ExpressionCompilationException : Exception
    {
        public ExpressionCompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    public class ExpressionBuildException : Exception
    {
        public ExpressionBuildException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
