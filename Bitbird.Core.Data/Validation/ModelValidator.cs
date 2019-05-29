using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    internal class ModelValidator<TEntity, T> : IModelValidator, IModelValidator<TEntity, T>
    {
        [NotNull] private readonly Action<T, ValidatorBase, Expression<Func<TEntity, T>>> validateAction;
        
        public ModelValidator()
        {
            var type = typeof(T);
            var properties = type.GetProperties();

            var modelParameter = Expression.Parameter(typeof(T), "x");
            var validatorParameter = Expression.Parameter(typeof(ValidatorBase), "validator");
            var entityExpressionParameter = Expression.Parameter(typeof(Expression<Func<TEntity, T>>), "entityExpression");
            var bodyExpressions = new List<Expression>();
            var hasValidationErrorsVariableExpression = Expression.Variable(typeof(bool), "hasValidationErrors");

            foreach (var property in properties)
            {
                CreateCheckExpressions(property, bodyExpressions, modelParameter, validatorParameter, entityExpressionParameter, hasValidationErrorsVariableExpression);
            }

            validateAction = Expression
                .Lambda<Action<T, ValidatorBase, Expression<Func<TEntity, T>>>>(
                    Expression.Block(
                        new []
                        {
                            hasValidationErrorsVariableExpression
                        }, 
                        bodyExpressions), 
                    modelParameter, 
                    validatorParameter, 
                    entityExpressionParameter)
                .Compile();
        }

        public void Validate(T model, ValidatorBase validator, Expression<Func<TEntity,T>> entityExpression)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            validateAction.Invoke(model, validator, entityExpression);
        }
        public void Validate(T model, ValidatorBase validator)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (validator == null) throw new ArgumentNullException(nameof(validator));

            if (!typeof(T).IsAssignableFrom(typeof(TEntity)))
                throw new Exception($"Cannot convert {typeof(TEntity).FullName} to {typeof(T).FullName}. Use the overload of the {nameof(Validate)} method that defines a conversion expression.");

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var lambda = typeof(T) == typeof(TEntity) ? Expression.Lambda<Func<TEntity, T>>(parameter, parameter) : Expression.Lambda<Func<TEntity, T>>(Expression.Convert(parameter, typeof(T)), parameter);

            validateAction.Invoke(model, validator, lambda);
        }

        public void Validate(object model, ValidatorBase validator, LambdaExpression entityExpression)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            if (!(model is T typedModel))
                throw new ArgumentException($"The passed model is not of type {typeof(T).FullName} but of type {model.GetType().FullName}", nameof(model));

            validateAction.Invoke(typedModel, validator, Expression.Lambda<Func<TEntity, T>>(entityExpression.Body, entityExpression.Parameters));
        }
        public void Validate(object model, ValidatorBase validator)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            if (!(model is T typedModel))
                throw new ArgumentException($"The passed model is not of type {typeof(T).FullName} but of type {model.GetType().FullName}", nameof(model));

            if (!typeof(T).IsAssignableFrom(typeof(TEntity)))
                throw new Exception($"Cannot convert {typeof(TEntity).FullName} to {typeof(T).FullName}. Use the overload of the {nameof(Validate)} method that defines a conversion expression.");

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var lambda = typeof(T) == typeof(TEntity) ? Expression.Lambda<Func<TEntity, T>>(parameter, parameter) : Expression.Lambda<Func<TEntity, T>>(Expression.Convert(parameter, typeof(T)), parameter);

            validateAction.Invoke(typedModel, validator, lambda);
        }

        private void CreateCheckExpressions(
            [NotNull] PropertyInfo property,
            [NotNull, ItemNotNull] ICollection<Expression> bodyExpressions,
            [NotNull] Expression modelParameter,
            [NotNull] Expression validatorParameter,
            [NotNull] Expression entityExpressionParameter,
            [NotNull] Expression hasValidationErrorsVariableExpression)
        {
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TEntity), property.PropertyType);

            var entityExpressionParameterVariable = Expression.Variable(typeof(ParameterExpression), "entityExpressionParameter");
            var entityExpressionBodyVariable = Expression.Variable(typeof(Expression), "entityExpressionBody");
            var propertyExpressionParameterVariable = Expression.Variable(typeof(ParameterExpression), "propertyExpressionParameter");
            var propertyExpressionBodyVariable = Expression.Variable(typeof(Expression), "propertyExpressionBody");
            var propertyExpressionVariable = Expression.Variable(typeof(Expression<>).MakeGenericType(delegateType), "propertyExpression");
            var propertyExpressionObjectVariable = Expression.Variable(typeof(Expression<Func<TEntity, object>>), "propertyExpressionObject");

            var propertyBlockExpressions = new List<Expression>();
            propertyBlockExpressions.Add(Expression.Assign(entityExpressionParameterVariable,
                    Expression.Property(
                        Expression.Property(
                            entityExpressionParameter, 
                            nameof(Expression<Func<TEntity, T>>.Parameters)),
                            typeof(ReadOnlyCollection<ParameterExpression>)
                                .GetDefaultMembers()
                                .OfType<PropertyInfo>()
                                .Single(p => p.GetIndexParameters().Length == 1 &&
                                            p.GetIndexParameters()[0].ParameterType == typeof(int)),
                        Expression.Constant(0, typeof(int)))));
            propertyBlockExpressions.Add(Expression.Assign(propertyExpressionParameterVariable, entityExpressionParameterVariable));
            propertyBlockExpressions.Add(Expression.Assign(entityExpressionBodyVariable,
                    Expression.Property(entityExpressionParameter, nameof(LambdaExpression.Body))));
            propertyBlockExpressions.Add(Expression.Assign(propertyExpressionBodyVariable,
                        Expression.Call(
                            typeof(Expression)
                                .GetMethods()
                                .Single(m => m.Name.Equals(nameof(Expression.Property)) &&
                                             m.GetParameters().Length == 2 &&
                                             m.GetParameters()[0].ParameterType == typeof(Expression) &&
                                             m.GetParameters()[1].ParameterType == typeof(PropertyInfo)),
                            entityExpressionBodyVariable,
                            Expression.Constant(property, typeof(PropertyInfo)))));
            propertyBlockExpressions.Add(Expression.Assign(propertyExpressionVariable,
                    Expression.Call(
                        typeof(Expression)
                            .GetMethods()
                            .Single(m => m.Name.Equals(nameof(Expression.Lambda)) &&
                                         m.IsGenericMethodDefinition &&
                                         m.GetParameters().Length == 2 &&
                                         m.GetParameters()[0].ParameterType == typeof(Expression) &&
                                         m.GetParameters()[1].ParameterType == typeof(ParameterExpression[]))
                            .MakeGenericMethod(delegateType),
                        propertyExpressionBodyVariable,
                        Expression.NewArrayInit(
                            typeof(ParameterExpression),
                            entityExpressionParameterVariable))));
            propertyBlockExpressions.Add(Expression.Assign(propertyExpressionObjectVariable,
                    Expression.Call(typeof(Expression)
                            .GetMethods()
                            .Single(m => m.Name.Equals(nameof(Expression.Lambda)) &&
                                         m.IsGenericMethodDefinition &&
                                         m.GetParameters().Length == 2 &&
                                         m.GetParameters()[0].ParameterType == typeof(Expression) &&
                                         m.GetParameters()[1].ParameterType == typeof(ParameterExpression[]))
                            .MakeGenericMethod(typeof(Func<TEntity, object>)),
                        Expression.Call(
                            typeof(Expression)
                                .GetMethods()
                                .Single(m => m.Name.Equals(nameof(Expression.Convert)) &&
                                             m.GetParameters().Length == 2 &&
                                             m.GetParameters()[0].ParameterType == typeof(Expression) &&
                                             m.GetParameters()[1].ParameterType == typeof(Type)),
                            propertyExpressionBodyVariable,
                            Expression.Constant(typeof(object), typeof(Type))),
                        Expression.NewArrayInit(
                            typeof(ParameterExpression),
                            entityExpressionParameterVariable))));
            propertyBlockExpressions.Add(Expression.Assign(hasValidationErrorsVariableExpression, Expression.Constant(false)));


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

            var nullableUnderlying = Nullable.GetUnderlyingType(property.PropertyType);
            var valueType = nullableUnderlying ?? property.PropertyType;
            if (valueType.IsEnum)
            {
                propertyBlockExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckEnumValueIsDefined),
                            new[]
                            {
                                typeof(TEntity),
                                valueType
                            },
                            Expression.Property(modelParameter, property),
                            propertyExpressionObjectVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckNotNullAttribute), out attribute) &&
                attribute is ValidatorCheckNotNullAttribute)
            {
                propertyBlockExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckNotNull),
                            new[]
                            {
                                typeof(TEntity)
                            },
                            Expression.Property(modelParameter, property),
                            propertyExpressionObjectVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckNotNullOrEmptyAttribute), out attribute) &&
                attribute is ValidatorCheckNotNullOrEmptyAttribute)
            {
                propertyBlockExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckNotNullOrEmpty),
                            new[]
                            {
                                typeof(TEntity)
                            },
                            Expression.Property(modelParameter, property),
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckNotEmptyAttribute), out attribute) &&
                attribute is ValidatorCheckNotEmptyAttribute)
            {
                propertyBlockExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckNotEmpty),
                            new[]
                            {
                                typeof(TEntity)
                            },
                            Expression.Property(modelParameter, property),
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckTrimmedAttribute), out attribute) &&
                attribute is ValidatorCheckTrimmedAttribute)
            {
                propertyBlockExpressions.Add(
                    Expression.Assign(hasValidationErrorsVariableExpression,
                        Expression.Call(
                            validatorParameter,
                            nameof(ValidatorBase.CheckTrimmed),
                            new[]
                            {
                                typeof(TEntity)
                            },
                            Expression.Property(modelParameter, property),
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckGreaterThanAttribute), out attribute) &&
                attribute is ValidatorCheckGreaterThanAttribute validatorCheckGreaterThanAttribute)
            {
                propertyBlockExpressions.Add(
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
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckGreaterThanEqualAttribute), out attribute) &&
                attribute is ValidatorCheckGreaterThanEqualAttribute validatorCheckGreaterThanEqualAttribute)
            {
                propertyBlockExpressions.Add(
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
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckLessThanAttribute), out attribute) &&
                attribute is ValidatorCheckLessThanAttribute validatorCheckLessThanAttribute)
            {
                propertyBlockExpressions.Add(
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
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckLessThanEqualAttribute), out attribute) &&
                attribute is ValidatorCheckLessThanEqualAttribute validatorCheckLessThanEqualAttribute)
            {
                propertyBlockExpressions.Add(
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
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckBetweenInclusiveAttribute), out attribute) &&
                attribute is ValidatorCheckBetweenInclusiveAttribute validatorCheckBetweenInclusiveAttribute)
            {
                propertyBlockExpressions.Add(
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
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckBetweenExclusiveAttribute), out attribute) &&
                attribute is ValidatorCheckBetweenExclusiveAttribute validatorCheckBetweenExclusiveAttribute)
            {
                propertyBlockExpressions.Add(
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
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckMaxStringLengthAttribute), out attribute) &&
                attribute is ValidatorCheckMaxStringLengthAttribute validatorCheckMaxStringLengthAttribute)
            {
                propertyBlockExpressions.Add(
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
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckItemNotNullAttribute), out attribute) &&
                attribute is ValidatorCheckItemNotNullAttribute)
            {
                propertyBlockExpressions.Add(
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
                            propertyExpressionVariable)));
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckDistinctAttribute), out attribute) &&
                attribute is ValidatorCheckDistinctAttribute validatorCheckDistinctAttribute)
            {
                if (validatorCheckDistinctAttribute.DistinctSelectEqualityMemberProviderType == null)
                {
                    propertyBlockExpressions.Add(
                        Expression.Assign(hasValidationErrorsVariableExpression,
                            Expression.Call(
                                validatorParameter,
                                nameof(ValidatorBase.CheckDistinct),
                                new[]
                                {
                                    typeof(TEntity),
                                    property.PropertyType.GetElementType()
                                },
                                Expression.Property(modelParameter, property),
                                propertyExpressionVariable)));
                }
                else
                {
                    var interfaceTye = validatorCheckDistinctAttribute.DistinctSelectEqualityMemberProviderType;
                    while (interfaceTye.IsGenericType && interfaceTye.GetGenericTypeDefinition() == typeof(IDistinctSelectEqualityMemberProvider<,>))
                    {
                        interfaceTye = interfaceTye.BaseType ?? throw new Exception($"{validatorCheckDistinctAttribute.DistinctSelectEqualityMemberProviderType.FullName} is not of type {typeof(IDistinctSelectEqualityMemberProvider<,>).FullName}.");
                    }

                    var equalityMemberType = interfaceTye.GetGenericArguments()[1];

                    propertyBlockExpressions.Add(
                        Expression.Assign(hasValidationErrorsVariableExpression,
                            Expression.Call(
                                validatorParameter,
                                nameof(ValidatorBase.CheckDistinct),
                                new[]
                                {
                                    typeof(TEntity),
                                    property.PropertyType.GetElementType(),
                                    equalityMemberType
                                },
                                Expression.Property(modelParameter, property),
                                Expression.Constant(Activator.CreateInstance(validatorCheckDistinctAttribute.DistinctSelectEqualityMemberProviderType), validatorCheckDistinctAttribute.DistinctSelectEqualityMemberProviderType),
                                propertyExpressionVariable)));
                }
            }

            if (attributes.TryGetValue(typeof(ValidatorCheckRecursiveAttribute), out attribute) &&
                attribute is ValidatorCheckRecursiveAttribute)
            {
                if (property.PropertyType.IsArray)
                {
                    var elementType = property.PropertyType.GetElementType();

                    var validator = ModelValidators.GetValidator(typeof(TEntity), elementType);

                    var arrayVariableExpression = Expression.Variable(property.PropertyType, "array");
                    var idxVariableExpression = Expression.Variable(typeof(int), "idx");
                    var itemVariableExpression = Expression.Variable(elementType, "item");

                    var loopBreakLabel = Expression.Label();


                    // array = <model>.<property>;
                    var assignArrayVariable = Expression.Assign(arrayVariableExpression, Expression.Property(modelParameter, property));

                    // array != null
                    var arrayVariableIsNotNull = Expression.NotEqual(arrayVariableExpression, Expression.Constant(null, property.PropertyType));

                    // idx = 0;
                    var setIdxVariable0 = Expression.Assign(idxVariableExpression, Expression.Constant(0, typeof(int)));

                    // array.Length
                    var arrayLength = Expression.Property(arrayVariableExpression, nameof(Array.Length));

                    // idx < array.Length
                    var idxLessThanArrayLength = Expression.LessThan(idxVariableExpression, arrayLength);

                    // item = array[idx];
                    var assignItemVariable = Expression.Assign(itemVariableExpression, Expression.ArrayIndex(arrayVariableExpression, idxVariableExpression));

                    // item != null
                    var itemVariableIsNull = Expression.NotEqual(itemVariableExpression, Expression.Constant(null, elementType));

                    // <validator instance>
                    var modelValidatorConstant = Expression.Constant(validator, typeof(IModelValidator));

                    // MethodInfo of IModelValidator.Validate(object, ValidatorBase, LambdaExpression)
                    var methodInfoValidatorValidateMethod = typeof(IModelValidator)
                                                      .GetMethods()
                                                      .SingleOrDefault(m =>
                                                          m.Name.Equals(nameof(IModelValidator.Validate)) &&
                                                          m.GetParameters().Length == 3) ??
                                                  throw new Exception(
                                                      $"Could not find method {nameof(IModelValidator)}.{nameof(IModelValidator.Validate)}");

                    // MethodInfo of Expression.Lambda<Func<TEntity, <elementtype>>>(Expression, ParameterExpression[])
                    var methodInfoExpressionLambdaEntityTypeToElementType = typeof(Expression)
                        .GetMethods()
                        .Single(m => m.Name.Equals(nameof(Expression.Lambda)) &&
                                     m.IsGenericMethodDefinition &&
                                     m.GetParameters().Length == 2 &&
                                     m.GetParameters()[0].ParameterType == typeof(Expression) &&
                                     m.GetParameters()[1].ParameterType == typeof(ParameterExpression[]))
                        .MakeGenericMethod(typeof(Func<,>).MakeGenericType(typeof(TEntity), elementType));

                    // MethodInfo of Expression.ArrayIndex(Expression, Expression)
                    var methodInfoExpressionArrayIndex = typeof(Expression)
                        .GetMethods()
                        .Single(m =>
                            m.Name.Equals(nameof(Expression.ArrayIndex)) &&
                            m.GetParameters().Length == 2 &&
                            m.GetParameters()[0].ParameterType == typeof(Expression) &&
                            m.GetParameters()[1].ParameterType == typeof(Expression));

                    // MethodInfo of Expression.Constant(object, Type)
                    var methodInfoExpressionConstant = typeof(Expression)
                        .GetMethods()
                        .Single(m =>
                            m.Name.Equals(nameof(Expression.Constant)) &&
                            m.GetParameters().Length == 2 &&
                            m.GetParameters()[0].ParameterType == typeof(object) &&
                            m.GetParameters()[1].ParameterType == typeof(Type));

                    // Expression.Constant((object)idx, typeof(int))
                    var idxVariableAsConstant = Expression.Call(
                            methodInfoExpressionConstant,
                            Expression.Convert(idxVariableExpression, typeof(object)),
                            Expression.Constant(typeof(int), typeof(Type)));

                    // Expression.ArrayIndex(<propertyExpression>.Body, Expression.Constant((object)idx, typeof(int)))
                    var arrayItemExpression = Expression.Call(methodInfoExpressionArrayIndex,
                            propertyExpressionBodyVariable,
                            idxVariableAsConstant);

                    // Expression.Lambda<Func<TEntity, <elementType>>>(<arrayItemExpression>, new ParameterExpression[] { <propertyExpression>.Parameters[0] })
                    var makeIndexedPropertyExpression = Expression.Call( 
                            methodInfoExpressionLambdaEntityTypeToElementType,
                            arrayItemExpression,
                            Expression.NewArrayInit(
                                typeof(ParameterExpression),
                                propertyExpressionParameterVariable));


                    // <modelValidatorConstant>.Validate(item, validator, <makeIndexedPropertyExpression>);
                    var callModelValidator = Expression.Call(
                        modelValidatorConstant,
                        methodInfoValidatorValidateMethod,
                        itemVariableExpression,
                        validatorParameter,
                        makeIndexedPropertyExpression
                    );

                    // <elementType> item;
                    // item = array[idx];
                    // if (item != null)
                    // {
                    //    <callModelValidator>
                    // }
                    // ++ idx;
                    var visitItemBlock = Expression.Block(
                        new[]
                        {
                            itemVariableExpression
                        },
                        assignItemVariable,
                        Expression.IfThen(itemVariableIsNull, callModelValidator),
                        Expression.PreIncrementAssign(idxVariableExpression)
                    );

                    // while(true)
                    // {
                    //    if (idx < array.length)
                    //    {
                    //       <visitItemBlock>
                    //    }
                    //    break;
                    // }
                    var loopOverItems = Expression.Loop(
                        Expression.IfThenElse(
                            idxLessThanArrayLength,
                            visitItemBlock,
                            Expression.Break(loopBreakLabel)),
                        loopBreakLabel);

                    // <elementType>[] array;
                    // array = <model>.<property>;
                    // if (array != null)
                    // {
                    //    int idx;
                    //    idx = 0;
                    //    <loopOverItems>
                    // }
                    var propertyBlock = Expression.Block(
                        new[]
                        {
                            arrayVariableExpression
                        },
                        assignArrayVariable,
                        Expression.IfThen(
                            arrayVariableIsNotNull,
                            Expression.Block(
                                new[]
                                {
                                    idxVariableExpression
                                },
                                setIdxVariable0,
                                loopOverItems)));

                    propertyBlockExpressions.Add(propertyBlock);
                }
                else
                {
                    var validator = ModelValidators.GetValidator(typeof(TEntity), property.PropertyType);

                    propertyBlockExpressions.Add(
                        Expression.IfThen(
                            Expression.AndAlso(
                                Expression.Equal(hasValidationErrorsVariableExpression, Expression.Constant(false)),
                                Expression.NotEqual(Expression.Property(modelParameter, property), Expression.Constant(null, property.PropertyType))),
                            Expression.Call(
                                Expression.Constant(validator, typeof(IModelValidator)),
                                typeof(IModelValidator)
                                    .GetMethods()
                                    .SingleOrDefault(m => m.Name.Equals(nameof(IModelValidator.Validate)) && 
                                                m.GetParameters().Length == 3) ?? throw new Exception($"Could not find method {nameof(IModelValidator)}.{nameof(IModelValidator.Validate)}"),
                                Expression.Property(modelParameter, property),
                                validatorParameter,
                                propertyExpressionVariable)));
                }
            }

            bodyExpressions.Add(
                Expression.Block(
                    new []
                    {
                        entityExpressionParameterVariable,
                        entityExpressionBodyVariable,
                        propertyExpressionParameterVariable,
                        propertyExpressionBodyVariable,
                        propertyExpressionVariable,
                        propertyExpressionObjectVariable
                    },
                    propertyBlockExpressions.ToArray()
                ));
        }
    }
}
