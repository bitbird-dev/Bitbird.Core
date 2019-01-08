using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Expressions
{
    public static class MethodInfoHelper
    {
        public static MethodInfo FromExpression<T, TReturn>(Expression<Func<T,TReturn>> expression)
        {
            return FromExpression(expression.Body);
        }
        public static MethodInfo FromExpression(Expression expression)
        {
            if (expression is LambdaExpression le)
                return FromExpression(le);
            if (expression is MethodCallExpression mce)
                return mce.Method;

            throw new Exception($"Expression {expression} is no {nameof(MethodCallExpression)}.");
        }

        /*private static readonly MethodInfo OrderByMethod = FromExpression<IQueryable>()*/
    }
}
