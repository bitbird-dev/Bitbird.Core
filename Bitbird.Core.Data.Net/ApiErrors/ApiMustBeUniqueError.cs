using System;
using System.Linq.Expressions;

namespace Bitbird.Core.Data.Net
{
    public class ApiMustBeUniqueError<TEntity, TMember> : ApiError
    {
        public readonly Expression<Func<TEntity, TMember>> AttributeExpression;

        public ApiMustBeUniqueError(Expression<Func<TEntity, TMember>> attributeExpression, string detailMessage)
            : base(ApiErrorType.MustBeUnique, "Must be unique", detailMessage)
        {
            AttributeExpression = attributeExpression;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(TEntity)}: {typeof(TEntity).Name}, {nameof(TMember)}: {typeof(TMember).Name}, {nameof(AttributeExpression)}: {AttributeExpression}";
        }
    }
}