using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Bitbird.Core.Query;

namespace Bitbird.Core.Expressions
{
    public static partial class ExpressionHelper
    {
        public static bool TryConvertToConstant(Expression expression, out object constant)
        {
            if (new ParameterExistsVisitor().Check(expression))
            {
                constant = null;
                return false;
            }

            try
            {
                constant = Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object))).Compile().Invoke();
                return true;
            }
            catch (Exception e)
            {
                throw new Exception($"{nameof(TryConvertToConstant)}: Could not compile expression to constant value. Expression: '{expression}'. Details: {e.Message}", e);
            }
        }
    }
}
