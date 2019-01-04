using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

//https://blogs.msdn.microsoft.com/meek/2008/05/02/linq-to-entities-combining-predicates/

namespace Bitbird.Core.Expressions
{
    public static class ExpressionHelper
    {
        public static LambdaExpression ComposeLambdas(LambdaExpression[] expressions, Func<Expression, Expression, Expression> merge)
        {
            if (expressions.Length == 0)
                return null;

            var body = expressions[0].Body;

            for (var i = 1; i < expressions.Length; i++)
            {
                var map = expressions[0].Parameters.Select((f, idx) => new { f, s = expressions[i].Parameters[idx] }).ToDictionary(p => p.s, p => p.f);
                var iBody = ParameterRebinder.ReplaceParameters(map, expressions[i].Body);
                body = merge(body, iBody);
            }

            return Expression.Lambda(body, expressions[0].Parameters);
        }
        public static Expression<T> ComposeFromLambda<T>(LambdaExpression[] expressions, Func<Expression, Expression, Expression> merge)
        {
            if (expressions.Length == 0)
                return null;

            var body = expressions[0].Body;

            for (var i = 1; i < expressions.Length; i++)
            {
                var map = expressions[0].Parameters.Select((f, idx) => new { f, s = expressions[i].Parameters[idx] }).ToDictionary(p => p.s, p => p.f);
                var iBody = ParameterRebinder.ReplaceParameters(map, expressions[i].Body);
                body = merge(body, iBody);
            }

            return Expression.Lambda<T>(body, expressions[0].Parameters);
        }
        public static Expression<T> Compose<T>(this IEnumerable<Expression<T>> expressions, Func<Expression, Expression, Expression> merge)
        {
            var expressionsEnumerated = expressions.ToArray();
            if (expressionsEnumerated.Length == 0)
                return null;

            var body = expressionsEnumerated[0].Body;

            for (var i = 1; i < expressionsEnumerated.Length; i++)
            {
                var map = expressionsEnumerated[0].Parameters.Select((f, idx) => new { f, s = expressionsEnumerated[i].Parameters[idx] }).ToDictionary(p => p.s, p => p.f);
                var iBody = ParameterRebinder.ReplaceParameters(map, expressionsEnumerated[i].Body);
                body = merge(body, iBody);
            }

            return Expression.Lambda<T>(body, expressionsEnumerated[0].Parameters);
        }

        public static Expression<Func<T, bool>> ComposeWithAnd<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
        {
            return Compose(expressions.ToArray(), Expression.And);
        }
        public static LambdaExpression ComposeWithAnd(this IEnumerable<LambdaExpression> expressions)
        {
            return ComposeLambdas(expressions.ToArray(), Expression.And);
        }
        public static Expression<T> ComposeWithAnd<T>(this IEnumerable<LambdaExpression> expressions)
        {
            return ComposeFromLambda<T>(expressions.ToArray(), Expression.And);
        }
        public static Expression<Func<T, bool>> ComposeWithOr<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
        {
            return Compose(expressions.ToArray(), Expression.Or);
        }
        public static LambdaExpression ComposeWithOr(this IEnumerable<LambdaExpression> expressions)
        {
            return ComposeLambdas(expressions.ToArray(), Expression.Or);
        }
        public static Expression<T> ComposeWithOr<T>(this IEnumerable<LambdaExpression> expressions)
        {
            return ComposeFromLambda<T>(expressions.ToArray(), Expression.Or);
        }
    }
}
