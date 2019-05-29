using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    public interface IModelValidator<in T>
    {
        void Validate(
            [NotNull] T model,
            [NotNull] ValidatorBase validator);
    }
    public interface IModelValidator
    {
        void Validate(
            [NotNull] object model,
            [NotNull] ValidatorBase validator);
    }
}