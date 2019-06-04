using System;
using System.Linq.Expressions;
using Bitbird.Core.ApiErrors;
using Bitbird.Core.Expressions;
using JetBrains.Annotations;

namespace Bitbird.Core
{
    public class ApiAttributeError : ApiError
    {
        [UsedImplicitly, NotNull]
        public readonly string AttributeName;

        public ApiAttributeError([NotNull] string attributeName, [NotNull] string detailMessage, [CanBeNull] ApiErrorType? apiErrorType = null)
            : base(apiErrorType ?? ApiErrorType.InvalidAttribute, ApiErrorMessages.ApiAttributeError_Title, detailMessage)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(attributeName));
            if (string.IsNullOrWhiteSpace(detailMessage))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(detailMessage));

            AttributeName = attributeName;
        }

        public override string ToString()
        {
            return $"{base.ToString()}; {nameof(AttributeName)}: {AttributeName}";
        }
    }

    public class ApiAttributeError<TEntity> : ApiAttributeError
    {
        [UsedImplicitly, NotNull]
        public readonly Expression<Func<TEntity, object>> AttributeExpression;

        public ApiAttributeError([NotNull] Expression<Func<TEntity, object>> attributeExpression, [NotNull] string detailMessage)
            : base(ConvertExpressionToName(attributeExpression), detailMessage)
        {
            // ReSharper disable once JoinNullCheckWithUsage
            if (attributeExpression == null)
                throw new ArgumentNullException(nameof(attributeExpression));
            if (string.IsNullOrEmpty(detailMessage))
                throw new ArgumentException("Value cannot be null or empty.", nameof(detailMessage));

            AttributeExpression = attributeExpression;
        }

        [NotNull]
        private static string ConvertExpressionToName([NotNull] Expression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            if (ExpressionHelper.TryConvertToConstant(expression, out var constant))
                return constant.ToString();

            switch (expression)
            {
                case ParameterExpression _:
                    return string.Empty;

                case LambdaExpression lambdaExpression:
                    // ReSharper disable once TailRecursiveCall
                    return ConvertExpressionToName(lambdaExpression.Body);

                case MemberExpression memberExpression:
                    return $"{ConvertExpressionToName(memberExpression.Expression)}.{memberExpression.Member.Name}";

                case BinaryExpression binaryExpression:
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.ArrayIndex:
                            return $"{ConvertExpressionToName(binaryExpression.Left)}[{ConvertExpressionToName(binaryExpression.Right)}]";
                    }

                    break;

                case UnaryExpression unaryExpression:
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (unaryExpression.NodeType)
                    {
                        case ExpressionType.Convert:
                            // ReSharper disable once TailRecursiveCall
                            return ConvertExpressionToName(unaryExpression.Operand);
                    }
                    break;
            }

            return "ERROR";
        }

        public override string ToString()
        {
            return $"{base.ToString()}; {nameof(TEntity)}: {typeof(TEntity).Name}; {nameof(AttributeExpression)}: {AttributeExpression}";
        }
    }
}