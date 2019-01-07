using System;
using System.Linq.Expressions;

namespace Bitbird.Core
{
    public class ApiAttributeError : ApiError
    {
        public readonly string AttributeName;

        public ApiAttributeError(string attributeName, string detailMessage, ApiErrorType? apiErrorType = null)
            : base(apiErrorType ?? ApiErrorType.InvalidAttribute, "Attribute Error", detailMessage)
        {
            AttributeName = attributeName;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(AttributeName)}: {AttributeName}";
        }
    }

    public class ApiAttributeError<TEntity, TMember> : ApiAttributeError
    {
        public readonly Expression<Func<TEntity, TMember>> AttributeExpression;

        public ApiAttributeError(Expression<Func<TEntity, TMember>> attributeExpression, string detailMessage)
            : base((attributeExpression.Body as MemberExpression)?.Member.Name ?? "ERROR", detailMessage)
        {
            AttributeExpression = attributeExpression;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(TEntity)}: {typeof(TEntity).Name}, {nameof(TMember)}: {typeof(TMember).Name}, {nameof(AttributeExpression)}: {AttributeExpression}";
        }
    }
}