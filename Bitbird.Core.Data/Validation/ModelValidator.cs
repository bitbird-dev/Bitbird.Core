using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    internal class ModelValidator<TEntity, T> : IModelValidator, IModelValidator<T>
    {
        [NotNull] private readonly Expression<Func<TEntity, T>> entityExpression;
        [NotNull] private readonly Action<T, ValidatorBase> validateAction;
        
        public ModelValidator([NotNull] Expression<Func<TEntity, T>> entityExpression)
        {
            this.entityExpression = entityExpression ?? throw new ArgumentNullException(nameof(entityExpression));

            var type = typeof(T);
            var properties = type.GetProperties();

            var modelParameter = Expression.Parameter(typeof(T), "x");
            var validatorParameter = Expression.Parameter(typeof(ValidatorBase), "validator");
            var bodyExpressions = new List<Expression>();
            var hasValidationErrorsVariableExpression = Expression.Variable(typeof(bool), "hasValidationErrors");

            foreach (var property in properties)
            {
                CreateCheckExpressions(property, bodyExpressions, modelParameter, validatorParameter, hasValidationErrorsVariableExpression);
            }

            validateAction = Expression
                .Lambda<Action<T, ValidatorBase>>(Expression.Block(new [] { hasValidationErrorsVariableExpression }, bodyExpressions), modelParameter, validatorParameter)
                .Compile();
        }

        public void Validate(
            T model,
            ValidatorBase validator)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            validateAction.Invoke(model, validator);
        }
        public void Validate(object model, ValidatorBase validator)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            if (!(model is T typedModel))
                throw new ArgumentException($"The passed model is not of type {typeof(T).FullName} but of type {model.GetType().FullName}", nameof(model));

            validateAction.Invoke(typedModel, validator);
        }

        private void CreateCheckExpressions(
            [NotNull] PropertyInfo property,
            [NotNull, ItemNotNull] ICollection<Expression> bodyExpressions,
            [NotNull] Expression modelParameter,
            [NotNull] Expression validatorParameter,
            [NotNull] Expression hasValidationErrorsVariableExpression)
        {
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TEntity), property.PropertyType);
            var propertyExpressionParameter = entityExpression.Body;
            var propertyExpression = typeof(Expression)
                .GetMethods()
                .Single(m => m.Name.Equals(nameof(Expression.Lambda)) && 
                             m.IsGenericMethodDefinition && 
                             m.GetParameters().Length == 2 && 
                             m.GetParameters()[0].ParameterType == typeof(Expression) &&
                             m.GetParameters()[1].ParameterType == typeof(ParameterExpression[]))
                .MakeGenericMethod(delegateType)
                .Invoke(
                    null, 
                    new object[]
                    {
                        Expression.Property(propertyExpressionParameter, property),
                        entityExpression.Parameters.ToArray()
                    });
            var propertyExpressionWithObjectReturn = Expression.Lambda<Func<TEntity, object>>(
                Expression.Convert(Expression.Property(propertyExpressionParameter, property), typeof(object)),
                entityExpression.Parameters.ToArray());
            var propertyExpressionConstant = Expression.Constant(propertyExpression, propertyExpression.GetType());
            var propertyExpressionWithObjectReturnConstant = Expression.Constant(propertyExpressionWithObjectReturn, propertyExpressionWithObjectReturn.GetType());


            bodyExpressions.Add(Expression.Assign(hasValidationErrorsVariableExpression, Expression.Constant(false)));


            var attributes = property.GetCustomAttributes(true)
                .ToDictionary(a => a.GetType(), a => a);

            if (attributes.TryGetValue(typeof(CopyValidatorFromAttribute), out var attribute) &&
                attribute is CopyValidatorFromAttribute copyValidatorFromAttribute)
            {
                foreach (var kvp in copyValidatorFromAttribute.CopiedAttributes)
                    attributes.Add(kvp.Key, kvp.Value);
            }

            foreach (var attr in attributes.ToArray())
            {
                if (AttributeTranslations.TryTranslateAttribute(attr.Value, out var newAttr))
                    attributes.Add(newAttr.GetType(), newAttr);
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckNotNullAttribute), out attribute) &&
                attribute is ValidatorCheckNotNullAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckNotNull),
                            new[]
                            {
                                typeof(TEntity)
                            },
                            Expression.Property(modelParameter, property),
                            propertyExpressionWithObjectReturnConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckNotNullOrEmptyAttribute), out attribute) &&
                attribute is ValidatorCheckNotNullOrEmptyAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckNotNullOrEmpty),
                            new[]
                            {
                                typeof(TEntity)
                            },
                            Expression.Property(modelParameter, property),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckNotEmptyAttribute), out attribute) &&
                attribute is ValidatorCheckNotEmptyAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckNotEmpty),
                            new[]
                            {
                                typeof(TEntity)
                            },
                            Expression.Property(modelParameter, property),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckTrimmedAttribute), out attribute) &&
                attribute is ValidatorCheckTrimmedAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckTrimmed),
                            new[]
                            {
                                typeof(TEntity)
                            },
                            Expression.Property(modelParameter, property),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckGreaterThanAttribute), out attribute) &&
                attribute is ValidatorCheckGreaterThanAttribute validatorCheckGreaterThanAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckGreaterThan),
                            new[]
                            {
                                typeof(TEntity),
                                property.PropertyType
                            },
                            Expression.Constant(Convert.ChangeType(validatorCheckGreaterThanAttribute.Limit, property.PropertyType), property.PropertyType),
                            Expression.Property(modelParameter, property),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckGreaterThanEqualAttribute), out attribute) &&
                attribute is ValidatorCheckGreaterThanEqualAttribute validatorCheckGreaterThanEqualAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckGreaterThanEqual),
                            new[]
                            {
                                typeof(TEntity),
                                property.PropertyType
                            },
                            Expression.Constant(Convert.ChangeType(validatorCheckGreaterThanEqualAttribute.Limit, property.PropertyType), property.PropertyType),
                            Expression.Property(modelParameter, property),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckLessThanAttribute), out attribute) &&
                attribute is ValidatorCheckLessThanAttribute validatorCheckLessThanAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckLessThan),
                            new[]
                            {
                                typeof(TEntity),
                                property.PropertyType
                            },
                            Expression.Constant(Convert.ChangeType(validatorCheckLessThanAttribute.Limit, property.PropertyType), property.PropertyType),
                            Expression.Property(modelParameter, property),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckLessThanEqualAttribute), out attribute) &&
                attribute is ValidatorCheckLessThanEqualAttribute validatorCheckLessThanEqualAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckLessThanEqual),
                            new[]
                            {
                                typeof(TEntity),
                                property.PropertyType
                            },
                            Expression.Constant(Convert.ChangeType(validatorCheckLessThanEqualAttribute.Limit, property.PropertyType), property.PropertyType),
                            Expression.Property(modelParameter, property),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckBetweenInclusiveAttribute), out attribute) &&
                attribute is ValidatorCheckBetweenInclusiveAttribute validatorCheckBetweenInclusiveAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckBetweenInclusive),
                            new[]
                            {
                                typeof(TEntity),
                                property.PropertyType
                            },
                            Expression.Constant(Convert.ChangeType(validatorCheckBetweenInclusiveAttribute.LowerLimit, property.PropertyType), property.PropertyType),
                            Expression.Constant(Convert.ChangeType(validatorCheckBetweenInclusiveAttribute.UpperLimit, property.PropertyType), property.PropertyType),
                            Expression.Property(modelParameter, property),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckBetweenExclusiveAttribute), out attribute) &&
                attribute is ValidatorCheckBetweenExclusiveAttribute validatorCheckBetweenExclusiveAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckBetweenExclusive),
                            new[]
                            {
                                typeof(TEntity),
                                property.PropertyType
                            },
                            Expression.Constant(Convert.ChangeType(validatorCheckBetweenExclusiveAttribute.LowerLimit, property.PropertyType), property.PropertyType),
                            Expression.Constant(Convert.ChangeType(validatorCheckBetweenExclusiveAttribute.UpperLimit, property.PropertyType), property.PropertyType),
                            Expression.Property(modelParameter, property),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckMaxStringLengthAttribute), out attribute) &&
                attribute is ValidatorCheckMaxStringLengthAttribute validatorCheckMaxStringLengthAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckMaxStringLength),
                            new[]
                            {
                                typeof(TEntity)
                            },
                            Expression.Property(modelParameter, property),
                            Expression.Constant(validatorCheckMaxStringLengthAttribute.MaxLength, typeof(int)),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckItemNotNullAttribute), out attribute) &&
                attribute is ValidatorCheckItemNotNullAttribute)
            {
                bodyExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckItemNotNull),
                            new[]
                            {
                                typeof(TEntity),
                                property.PropertyType.GetElementType()
                            },
                            Expression.Property(modelParameter, property),
                            propertyExpressionConstant)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckRecursiveAttribute), out attribute) &&
                attribute is ValidatorCheckRecursiveAttribute)
            {
                var validator = Activator.CreateInstance(
                    typeof(ModelValidator<,>).MakeGenericType(typeof(TEntity), property.PropertyType),
                    propertyExpression);
                bodyExpressions.Add(
                    Expression.IfThen(
                        Expression.AndAlso(
                            Expression.Equal(hasValidationErrorsVariableExpression, Expression.Constant(false)),
                            Expression.NotEqual(Expression.Property(modelParameter, property), Expression.Constant(null, property.PropertyType))), 
                        Expression.Call(
                            Expression.Constant(validator, typeof(IModelValidator)),
                            typeof(IModelValidator).GetMethod(nameof(IModelValidator.Validate)) ?? throw new Exception($"Could not find method {nameof(IModelValidator)}.{nameof(IModelValidator.Validate)}"),
                            Expression.Property(modelParameter, property), 
                            validatorParameter)));
            }
        }
    }
}
