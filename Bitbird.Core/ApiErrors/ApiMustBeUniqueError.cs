using System;
using System.Linq.Expressions;
using Bitbird.Core.ApiErrors;

namespace Bitbird.Core
{
    public class ApiMustBeUniqueError<TEntity, TMember> : ApiError
    {
        public readonly Expression<Func<TEntity, TMember>> AttributeExpression;

        public ApiMustBeUniqueError(Expression<Func<TEntity, TMember>> attributeExpression, string detailMessage)
            : base(ApiErrorType.MustBeUnique, ApiErrorMessages.ApiErrorType_MustBeUnique_Title, detailMessage)
        {
            AttributeExpression = attributeExpression;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(TEntity)}: {typeof(TEntity).Name}, {nameof(TMember)}: {typeof(TMember).Name}, {nameof(AttributeExpression)}: {AttributeExpression}";
        }
    }
}