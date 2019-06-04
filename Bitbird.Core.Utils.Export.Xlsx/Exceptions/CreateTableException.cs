using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Utils.Export.Xlsx.Exceptions
{
    public class CreateTableException : Exception
    {
        [NotNull] public readonly string ParameterPath;

        public CreateTableException([NotNull] string parameterPath, [NotNull] string detailedMessage, [CanBeNull] Exception innerException) : base(detailedMessage, innerException)
        {
            if (string.IsNullOrWhiteSpace(parameterPath))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parameterPath));
            if (string.IsNullOrWhiteSpace(detailedMessage))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(detailedMessage));

            ParameterPath = detailedMessage;
        }
    }
}
