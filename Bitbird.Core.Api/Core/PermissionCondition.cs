using System;
using System.Linq.Expressions;

namespace Bitbird.Core.Api.Core
{
    public class PermissionCondition<TSession>
    {
        public readonly Func<object, TSession, bool> Func;
        public readonly Func<TSession, LambdaExpression> CreateExpression;
        
        public PermissionCondition(Func<object, TSession, bool> func, Func<TSession, LambdaExpression> expression)
        {
            Func = func;
            CreateExpression = expression;
        }
    }
}