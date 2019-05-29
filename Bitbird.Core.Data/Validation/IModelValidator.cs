using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    public interface IModelValidator<TEntity, T>
    {
        void Validate(
            [NotNull] T model,
            [NotNull] ValidatorBase validator,
            [NotNull] Expression<Func<TEntity, T>> entityExpression);
        void Validate(
            [NotNull] T model,
            [NotNull] ValidatorBase validator);
    }

    public interface IModelValidator
    {
        void Validate(
            [NotNull] object model,
            [NotNull] ValidatorBase validator,
            [NotNull] LambdaExpression entityExpression);
        void Validate(
            [NotNull] object model,
            [NotNull] ValidatorBase validator);
    }
}