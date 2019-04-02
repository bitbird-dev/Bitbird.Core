using System.Linq.Expressions;

namespace Bitbird.Core.Query
{
    public class ParameterExistsVisitor : ExpressionVisitor
    {
        private bool exists;

        public bool Check(Expression expression)
        {
            exists = false;
            Visit(expression);
            return exists;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            exists = true;
            return base.VisitParameter(node);
        }
    }
}