using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    public static class ModelValidators
    {
        [NotNull]
        private static readonly Dictionary<Type, IModelValidator> validators = new Dictionary<Type, IModelValidator>();
        
        [NotNull]
        public static IModelValidator<T> GetValidator<T>()
        {
            lock (validators)
            {
                if (validators.TryGetValue(typeof(T), out var validator))
                    return (IModelValidator<T>)validator;
            }

            var createdValidator = new ModelValidator<T,T>(x => x);

            lock (validators)
            {
                if (validators.TryGetValue(typeof(T), out var validator))
                    return (IModelValidator<T>)validator;

                validators.Add(typeof(T), createdValidator);
                return createdValidator;
            }
        }
    }
}