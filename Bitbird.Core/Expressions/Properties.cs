using Bitbird.Core.Exceptions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Bitbird.Core.Expressions
{
    public static class PropertiesHelper
    {
        public static Expression GetDottedPropertyGetterExpressionBody(Expression instance, string dottedProperty, Type instanceType, Type castReturnValueType, out Type returnType)
        {
            try
            {
                var path = dottedProperty.Split('.');

                var body = instance;
                var bodyType = instanceType;

                foreach (var node in path)
                {
                    var property = bodyType.GetProperties().Single(p => p.Name.ToUpper().Equals(node.ToUpper()));

                    body = Expression.Property(body, property);
                    bodyType = property.PropertyType;
                }

                if (castReturnValueType != null)
                {
                    body = Expression.Convert(body, castReturnValueType);
                    bodyType = castReturnValueType;
                }

                returnType = bodyType;
                return body;
            }
            catch (Exception e)
            {
                throw new ExpressionBuildException($"Could not compile property '{dottedProperty}'.", e);
            }
        }
        public static LambdaExpression GetDottedPropertyGetterExpression(string dottedProperty, Type instanceType, Type castReturnValueType = null)
        {
            try
            {
                var param = Expression.Parameter(instanceType, "x");
                var body = GetDottedPropertyGetterExpressionBody(param, dottedProperty, instanceType, castReturnValueType, out _);

                return Expression.Lambda(body, param);
            }
            catch (Exception e)
            {
                throw new ExpressionBuildException($"Could not compile property '{dottedProperty}'.", e);
            }
        }
        public static Expression<Func<T, TResult>> GetDottedPropertyGetterExpression<T, TResult>(string dottedProperty)
        {
            try
            {
                var lambda = GetDottedPropertyGetterExpression(dottedProperty, typeof(T), typeof(TResult));
                return Expression.Lambda<Func<T, TResult>>(lambda.Body, lambda.Parameters);
            }
            catch (Exception e)
            {
                throw new ExpressionBuildException($"Could not compile property '{dottedProperty}'.", e);
            }
        }
        public static Func<object, object> GetDottedPropertyGetter(string dottedProperty, Type instanceType, Type castReturnValueType)
        {
            try
            {
                var lambda = GetDottedPropertyGetterExpression(dottedProperty, instanceType, castReturnValueType);
                return Expression.Lambda<Func<object, object>>(lambda.Body, lambda.Parameters).Compile();
            }
            catch (Exception e)
            {
                throw new ExpressionCompilationException($"Could not compile property '{dottedProperty}'.", e);
            }
        }
        public static Func<T, TResult> GetDottedPropertyGetter<T, TResult>(string dottedProperty)
        {
            try
            {
                return GetDottedPropertyGetterExpression<T, TResult>(dottedProperty).Compile();
            }
            catch(Exception e)
            {
                throw new ExpressionCompilationException($"Could not compile property '{dottedProperty}'.", e);
            }
        }
    }
}
