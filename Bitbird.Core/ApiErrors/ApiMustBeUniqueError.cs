using System;
using System.Linq.Expressions;
using Bitbird.Core.ApiErrors;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiMustBeUniqueError<TEntity, TMember> : ApiError
    {
        [NotNull, UsedImplicitly]
        public readonly Expression<Func<TEntity, TMember>> AttributeExpression;

        public ApiMustBeUniqueError([NotNull] Expression<Func<TEntity, TMember>> attributeExpression, [NotNull] string detailMessage)
            : base(ApiErrorType.MustBeUnique, ApiErrorMessages.ApiErrorType_MustBeUnique_Title, detailMessage)
        {
            // ReSharper disable once JoinNullCheckWithUsage
            if (attributeExpression == null)
                throw new ArgumentNullException(nameof(attributeExpression));
            if (string.IsNullOrWhiteSpace(detailMessage))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(detailMessage));

            AttributeExpression = attributeExpression;
        }

        public override string ToString()
        {
            return $"{base.ToString()}; {nameof(TEntity)}: {typeof(TEntity).Name}; {nameof(TMember)}: {typeof(TMember).Name}; {nameof(AttributeExpression)}: {AttributeExpression}";
        }
    }
}