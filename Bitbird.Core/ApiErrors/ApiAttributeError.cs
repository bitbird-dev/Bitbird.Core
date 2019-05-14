using System;
using System.Linq.Expressions;
using Bitbird.Core.Expressions;

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

    public class ApiAttributeError<TEntity> : ApiAttributeError
    {
        public readonly Expression<Func<TEntity, object>> AttributeExpression;

        public ApiAttributeError(Expression<Func<TEntity, object>> attributeExpression, string detailMessage)
            : base(ConvertExpressionToName(attributeExpression), detailMessage)
        {
            AttributeExpression = attributeExpression;
        }

        private static string ConvertExpressionToName(Expression expression)
        {
            if (ExpressionHelper.TryConvertToConstant(expression, out var constant))
                return constant.ToString();

            switch (expression)
            {
                case ParameterExpression parameterExpression:
                    return string.Empty;

                case LambdaExpression lambdaExpression:
                    return ConvertExpressionToName(lambdaExpression.Body);

                case MemberExpression memberExpression:
                    return $"{ConvertExpressionToName(memberExpression.Expression)}.{memberExpression.Member.Name}";

                case BinaryExpression binaryExpression:
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.ArrayIndex:
                            return $"{ConvertExpressionToName(binaryExpression.Left)}[{ConvertExpressionToName(binaryExpression.Right)}]";
                    }

                    break;
            }

            return "ERROR";
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(TEntity)}: {typeof(TEntity).Name}, {nameof(AttributeExpression)}: {AttributeExpression}";
        }
    }
}