using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    public static class ModelValidators
    {
        [NotNull]
        private static readonly Dictionary<ModelValidatorKey, IModelValidator> validators = new Dictionary<ModelValidatorKey, IModelValidator>();

        [NotNull]
        public static IModelValidator<T, T> GetValidator<T>() => GetValidator<T, T>();

        [NotNull]
        public static IModelValidator<TEntity, T> GetValidator<TEntity, T>()
        {
            var key = new ModelValidatorKey(typeof(TEntity), typeof(T));
            lock (validators)
            {
                if (validators.TryGetValue(key, out var validator))
                    return (IModelValidator<TEntity, T>)validator;
            }

            var createdValidator = new ModelValidator<TEntity, T>();

            lock (validators)
            {
                if (validators.TryGetValue(key, out var validator))
                    return (IModelValidator<TEntity, T>)validator;

                validators.Add(key, createdValidator);
                return createdValidator;
            }
        }

        [NotNull]
        public static IModelValidator GetValidator([NotNull]Type tEntity, [NotNull]Type t)
        {
            var key = new ModelValidatorKey(tEntity, t);
            lock (validators)
            {
                if (validators.TryGetValue(key, out var validator))
                    return validator;
            }

            var createdValidator = (IModelValidator)Activator.CreateInstance(typeof(ModelValidator<,>).MakeGenericType(tEntity, t));

            lock (validators)
            {
                if (validators.TryGetValue(key, out var validator))
                    return validator;

                validators.Add(key, createdValidator);
                return createdValidator;
            }
        }

        [NotNull]
        public static IModelValidator GetValidator([NotNull] Type t) => GetValidator(t, t);
    }
}