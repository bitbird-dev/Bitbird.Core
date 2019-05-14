using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using Bitbird.Core.Expressions;

namespace Bitbird.Core.Query
{
    public abstract class QueryFilter
    {
        public readonly string PropertyName;

        protected QueryFilter(string propertyName)
        {
            PropertyName = propertyName;
        }

        public override string ToString()
        {
            return $"{nameof(PropertyName)}: {PropertyName}";
        }

        public abstract string ValueExpression { get; }

        public static QueryFilter Exact(string property, string value)
            => new QueryExactFilter(property, value);
        public static QueryFilter GreaterThan(string property, string lower)
            => new QueryGtFilter(property, lower);
        public static QueryFilter GreaterThanEqual(string property, string lower)
            => new QueryGteFilter(property, lower);
        public static QueryFilter LessThan(string property, string upper)
            => new QueryLtFilter(property, upper);
        public static QueryFilter LessThanEqual(string property, string upper)
            => new QueryLteFilter(property, upper);
        public static QueryFilter Range(string property, string lower, string upper)
            => new QueryRangeFilter(property, lower, upper);
        public static QueryFilter In(string property, string[] values)
            => new QueryInFilter(property, values);
        public static QueryFilter FreeText(string property, string pattern)
            => new QueryFreeTextFilter(property, pattern);


        public static QueryFilter[] Parse<T>(Expression<Func<T, bool>> exp)
        {
            return Parse<T>(exp.Body, exp.Parameters[0]);
        }
        private static QueryFilter[] Parse<T>(Expression exp, ParameterExpression parameter)
        {
            switch (exp)
            {
                case BinaryExpression binaryExpression:
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.AndAlso:
                            return Parse<T>(binaryExpression.Left, parameter)
                                .Concat(Parse<T>(binaryExpression.Right, parameter))
                                .ToArray();
                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.Equal:
                            return new[] { ParseComparison<T>(binaryExpression, parameter) };
                        default:
                            throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} with {nameof(binaryExpression.NodeType)}={binaryExpression.NodeType} are not supported yet. Found (sub-)expression: {exp}");
                    }
                case MethodCallExpression methodCallExpression:
                    if (methodCallExpression.Method.IsGenericMethod &&
                        methodCallExpression.Method.GetGenericMethodDefinition().Name == nameof(Enumerable.Contains) &&
                        methodCallExpression.Method.GetGenericMethodDefinition().GetParameters().Length == 2 &&
                        methodCallExpression.Method.GetGenericMethodDefinition().DeclaringType == typeof(Enumerable))
                    {
                        if (!(ExpressionHelper.TryConvertToConstant(methodCallExpression.Arguments[0], out var constant)))
                            throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} that call {methodCallExpression.Method.Name} must be called on an expression that compiles to a constant value. Found (sub-)expression: {exp}");
                        if (!(TryConvertToMember(methodCallExpression.Arguments[1], out var memberExpression)))
                            throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} that call {methodCallExpression.Method.Name} must be called with a member expression as argument. Found (sub-)expression: {exp}");

                        return new[] { In(QueryInfo.EncodeMemberExpression(memberExpression, parameter), EncodeConstantCollection(constant)) };
                    }
                    if (methodCallExpression.Method.Name == nameof(string.StartsWith) &&
                        methodCallExpression.Method.GetParameters().Length == 1 &&
                        methodCallExpression.Method.DeclaringType == typeof(string))
                    {
                        if (!(ExpressionHelper.TryConvertToConstant(methodCallExpression.Arguments[0], out var constant)))
                            throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} that call {methodCallExpression.Method.Name} must be called with an expression that compiles to a constant value as argument. Found (sub-)expression: {exp}");
                        if (!(TryConvertToMember(methodCallExpression.Object, out var memberExpression)))
                            throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} that call {methodCallExpression.Method.Name} must be called on a member expression. Found (sub-)expression: {exp}");

                        return new[] { FreeText(QueryInfo.EncodeMemberExpression(memberExpression, parameter), EncodeConstant(constant, constant.GetType()) + "*") };
                    }
                    if (methodCallExpression.Method.Name == nameof(string.EndsWith) &&
                        methodCallExpression.Method.GetParameters().Length == 1 &&
                        methodCallExpression.Method.DeclaringType == typeof(string))
                    {
                        if (!(ExpressionHelper.TryConvertToConstant(methodCallExpression.Arguments[0], out var constant)))
                            throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} that call {methodCallExpression.Method.Name} must be called with an expression that compiles to a constant value as argument. Found (sub-)expression: {exp}");
                        if (!(TryConvertToMember(methodCallExpression.Object, out var memberExpression)))
                            throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} that call {methodCallExpression.Method.Name} must be called on a member expression. Found (sub-)expression: {exp}");

                        return new[] { FreeText(QueryInfo.EncodeMemberExpression(memberExpression, parameter), "*" + EncodeConstant(constant, constant.GetType())) };
                    }
                    if (methodCallExpression.Method.Name == nameof(string.Contains) &&
                        methodCallExpression.Method.GetParameters().Length == 1 &&
                        methodCallExpression.Method.DeclaringType == typeof(string))
                    {
                        if (!(ExpressionHelper.TryConvertToConstant(methodCallExpression.Arguments[0], out var constant)))
                            throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} that call {methodCallExpression.Method.Name} must be called with an expression that compiles to a constant value as argument. Found (sub-)expression: {exp}");
                        if (!(TryConvertToMember(methodCallExpression.Object, out var memberExpression)))
                            throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} that call {methodCallExpression.Method.Name} must be called on a member expression. Found (sub-)expression: {exp}");

                        return new[] { FreeText(QueryInfo.EncodeMemberExpression(memberExpression, parameter), "*" + EncodeConstant(constant, constant.GetType()) + "*") };
                    }
                    throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} are only partially supported. The found call is not supported. Found (sub-)expression: {exp}");

                default:
                    throw new Exception($"{nameof(Parse)}: Expressions of type {exp.GetType()} are not supported yet. Found (sub-)expression: {exp}");
            }
        }

        private static bool TryConvertToMember(Expression expression, out MemberExpression memberExpression)
        {
            if (expression is MemberExpression foundMemberExpression)
            {
                memberExpression = foundMemberExpression;
                return true;
            }

            if (expression is UnaryExpression unaryExpression && 
                unaryExpression.NodeType == ExpressionType.Convert &&
                unaryExpression.Operand.Type.IsEnum)
            {
                return TryConvertToMember(unaryExpression.Operand, out memberExpression);
            }

            memberExpression = null;
            return false;
        }

        private static QueryFilter ParseComparison<T>(BinaryExpression binaryExpression, ParameterExpression parameter)
        {
            var leftIsConstant = ExpressionHelper.TryConvertToConstant(binaryExpression.Left, out var leftConstant);
            var rightIsConstant = ExpressionHelper.TryConvertToConstant(binaryExpression.Right, out var rightConstant);
            var leftIsProperty = TryConvertToMember(binaryExpression.Left, out var leftMember);
            var rightIsProperty = TryConvertToMember(binaryExpression.Right, out var rightMember);

            if (!((leftIsConstant ^ rightIsConstant) || (leftIsProperty || rightIsProperty)))
                throw new Exception($"{nameof(Parse)}: Expressions of type {binaryExpression.GetType()} with {nameof(binaryExpression.NodeType)}={binaryExpression.NodeType} must contain an expression that compiles to a constant value and a member expression. Found (sub-)expression: '{binaryExpression}'.");

            var constantValue = leftIsConstant ? leftConstant : rightConstant;
            var memberExpression = leftIsConstant ? rightMember : leftMember;

            var property = QueryInfo.EncodeMemberExpression(memberExpression, parameter);
            var constant = EncodeConstant(constantValue, constantValue.GetType());

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.GreaterThan:
                    return GreaterThan(property, constant);
                case ExpressionType.GreaterThanOrEqual:
                    return GreaterThanEqual(property, constant);
                case ExpressionType.LessThan:
                    return LessThan(property, constant);
                case ExpressionType.LessThanOrEqual:
                    return LessThanEqual(property, constant);
                case ExpressionType.Equal:
                    return Exact(property, constant);
                default:
                    throw new Exception($"{nameof(Parse)}: Expressions of type {binaryExpression.GetType()} with {nameof(binaryExpression.NodeType)}={binaryExpression.NodeType} are not supported yet. Found (sub-)expression: {binaryExpression}");
            }
        }

        private static string EncodeConstant(object value, Type t)
        {
            if (t == typeof(DateTime))
                return Convert.ToDateTime(value).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
            if (t.IsEnum)
                return Convert.ChangeType(value, Enum.GetUnderlyingType(t)).ToString();

            return value.ToString();
        }

        private static string[] EncodeConstantCollection(object value)
        {
            if (!(value is IEnumerable enumerable))
                throw new Exception($"{nameof(Parse)}/{nameof(EncodeConstantCollection)}: constant must be of type {nameof(IEnumerable)}. Found type is {value?.GetType()}.");

            return enumerable.Cast<object>().Select(i => EncodeConstant(i, i.GetType())).ToArray();
        }
    }
}