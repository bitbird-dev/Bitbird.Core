using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Validation
{
    public abstract class ValidatorBase
    {
        [NotNull, ItemNotNull]
        private readonly List<ApiError> Errors = new List<ApiError>();
        [CanBeNull]
        private IGetQueryByEntity queryByEntity;
        
        [UsedImplicitly]
        public bool HasErrors => Errors.Any();

        [UsedImplicitly]
        public void ThrowIfHasErrors()
        {
            if (HasErrors)
                throw new ApiErrorException(Errors.ToArray());
        }

        [NotNull]
        private IGetQueryByEntity QueryByEntity => queryByEntity ?? throw new Exception($"An instance of {nameof(IGetQueryByEntity)} must be registered before this method can be used.");

        // ReSharper disable once ParameterHidesMember
        public void RegisterQueryByEntity([NotNull] IGetQueryByEntity queryByEntity)
        {
            this.queryByEntity = queryByEntity ?? throw new ArgumentNullException(nameof(queryByEntity));
        }

        [NotNull]
        protected abstract Task<bool> AnyAsync<T>([NotNull] IQueryable<T> query);

        private bool ExecuteCheck([NotNull] Func<bool> checkAction)
        {
            if (checkAction == null) throw new ArgumentNullException(nameof(checkAction));

            try
            {
                return checkAction();
            }
            catch (Exception exception)
            {
                Errors.Add(new ApiCannotProcessFurtherError(exception.Message));
                return false;
            }
        }

        private async Task<bool> ExecuteCheckAsync([NotNull] Func<Task<bool>> checkAsyncAction)
        {
            if (checkAsyncAction == null) throw new ArgumentNullException(nameof(checkAsyncAction));

            try
            {
                return await checkAsyncAction();
            }
            catch (Exception exception)
            {
                Errors.Add(new ApiCannotProcessFurtherError(exception.Message));
                return false;
            }
        }

        [NotNull, UsedImplicitly]
        internal Expression<Func<TEntity, object>> CastExpression<TEntity, TResult>(
            [NotNull] Expression<Func<TEntity, TResult>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            return Expression.Lambda<Func<TEntity, object>>(Expression.Convert(expression.Body, typeof(object)), expression.Parameters);
        }

        [UsedImplicitly]
        public void AddError([NotNull] ApiError error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));

            Errors.Add(error);
        }
        [UsedImplicitly]
        public void AddErrors([NotNull, ItemNotNull] params ApiError[] errors)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));
            if (errors.Any(x => x == null)) throw new ArgumentNullException(nameof(errors));

            Errors.AddRange(errors);
        }

        [ContractAnnotation("value:null => true; value:notnull => false")]
        [UsedImplicitly]
        public bool CheckNull<TEntity>(
            [CanBeNull] object value,
            [NotNull] Expression<Func<TEntity, object>> attributeExpression)
        {
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(attributeExpression, ValidationMessages.Null));
                return false;
            });
        }

        [ContractAnnotation("value:null => true; value:notnull => false")]
        [UsedImplicitly]
        public bool CheckNull<TEntity>(
            [CanBeNull] TEntity model,
            [CanBeNull] object value,
            [NotNull] Expression<Func<TEntity, object>> attributeExpression) => CheckNull(value, attributeExpression);

        [ContractAnnotation("value:null => false; value:notnull => true")]
        [UsedImplicitly]
        public bool CheckNotNull<TEntity>(
            [CanBeNull] object value,
            [NotNull] Expression<Func<TEntity, object>> attributeExpression)
        {
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value != null)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(attributeExpression, ValidationMessages.NotNull));
                return false;
            });
        }

        [ContractAnnotation("value:null => false; value:notnull => true")]
        [UsedImplicitly]
        public bool CheckNotNull<TEntity>(
            [CanBeNull] TEntity model,
            [CanBeNull] object value,
            [NotNull] Expression<Func<TEntity, object>> attributeExpression) => CheckNotNull(value, attributeExpression);

        [ContractAnnotation("value:null => false")]
        [UsedImplicitly]
        public bool CheckNotNullOrEmpty<TEntity>(
            [CanBeNull] string value,
            [NotNull] Expression<Func<TEntity, string>> attributeExpression)
        {
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    ValidationMessages.NotNullOrEmpty));
                return false;
            });
        }

        [ContractAnnotation("value:null => false")]
        [UsedImplicitly]
        public bool CheckNotNullOrEmpty<TEntity>(
            [CanBeNull] TEntity model,
            [CanBeNull] string value,
            [NotNull] Expression<Func<TEntity, string>> attributeExpression) => CheckNotNullOrEmpty(value, attributeExpression);

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckNotEmpty<TEntity>(
            [CanBeNull] string value,
            [NotNull] Expression<Func<TEntity, string>> attributeExpression)
        {
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null || value.Trim().Length != 0)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    ValidationMessages.NotEmpty));
                return false;
            });
        }

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckMaxStringLength<TEntity>(
            [CanBeNull] string value,
            int maxLength,
            [NotNull] Expression<Func<TEntity, string>> attributeExpression)
        {
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null || value.Length <= maxLength)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    string.Format(ValidationMessages.StringMaxLength, maxLength)));
                return false;
            });
        }

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckTrimmed<TEntity>(
            [CanBeNull] string value,
            [NotNull] Expression<Func<TEntity, string>> attributeExpression)
        {
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null || value.Trim().Equals(value))
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    ValidationMessages.Trimmed));
                return false;
            });
        }

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckGreaterThan<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue limit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue>
        {
            if (limit == null) throw new ArgumentNullException(nameof(limit));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null || value.CompareTo(limit) > 0)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    string.Format(ValidationMessages.GreaterThan, limit, value)));
                return false;
            });
        }

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckGreaterThanEqual<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue limit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue>
        {
            if (limit == null) throw new ArgumentNullException(nameof(limit));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null || value.CompareTo(limit) >= 0)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    string.Format(ValidationMessages.GreaterThanEqual, limit, value)));
                return false;
            });
        }

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckLessThan<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue limit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue>
        {
            if (limit == null) throw new ArgumentNullException(nameof(limit));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null || value.CompareTo(limit) < 0)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    string.Format(ValidationMessages.LessThan, limit, value)));
                return false;
            });
        }

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckLessThanEqual<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue limit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue>
        {
            if (limit == null) throw new ArgumentNullException(nameof(limit));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null || value.CompareTo(limit) <= 0)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    string.Format(ValidationMessages.LessThanEqual, limit, value)));
                return false;
            });
        }

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckBetweenInclusive<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue lowerLimit,
            [NotNull] TValue upperLimit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue>
        {
            if (lowerLimit == null) throw new ArgumentNullException(nameof(lowerLimit));
            if (upperLimit == null) throw new ArgumentNullException(nameof(upperLimit));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null || value.CompareTo(lowerLimit) < 0 || value.CompareTo(upperLimit) > 0)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    string.Format(ValidationMessages.BetweenInclusive, lowerLimit, upperLimit, value)));
                return false;
            });
        }

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckBetweenExclusive<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue lowerLimit,
            [NotNull] TValue upperLimit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue>
        {
            if (lowerLimit == null) throw new ArgumentNullException(nameof(lowerLimit));
            if (upperLimit == null) throw new ArgumentNullException(nameof(upperLimit));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null || value.CompareTo(lowerLimit) < 0 || value.CompareTo(upperLimit) > 0)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    string.Format(ValidationMessages.BetweenExclusive, lowerLimit, upperLimit, value)));
                return false;
            });
        }

        [NotNull]
        [UsedImplicitly]
        public Task<bool> CheckUniqueAsync<TEntity, TDbEntity, TValue>(
            [CanBeNull] TValue value, 
            [CanBeNull] Expression<Func<TDbEntity, bool>> filterExpression,
            [NotNull] Expression<Func<TDbEntity, TValue>> selectValueExpression,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TDbEntity : class
        {
            if (selectValueExpression == null) throw new ArgumentNullException(nameof(selectValueExpression));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheckAsync(async () =>
            {
                var query = QueryByEntity.GetNonTrackingQuery<TDbEntity>();

                if (filterExpression != null)
                    query = query.Where(filterExpression);

                var valueQuery = query.Select(selectValueExpression);
                var parameterExpression = Expression.Parameter(typeof(TValue), "x");
                valueQuery = valueQuery.Where(Expression.Lambda<Func<TValue, bool>>(
                    Expression.Equal(parameterExpression, Expression.Constant(value, typeof(TValue))),
                    parameterExpression));

                var queryResult = await AnyAsync(valueQuery);

                if (!queryResult)
                    return true;

                Errors.Add(new ApiMustBeUniqueError<TEntity, TValue>(attributeExpression,
                    string.Format(ValidationMessages.Unique_AlreadyExists, value)));
                return false;
            });
        }

        [ContractAnnotation("value:null => true"), NotNull]
        [UsedImplicitly]
        public Task<bool> CheckUniqueIfNotNullAsync<TEntity, TDbEntity, TValue>(
            [CanBeNull] TValue value,
            [CanBeNull] Expression<Func<TDbEntity, bool>> filterExpression,
            [NotNull] Expression<Func<TDbEntity, TValue>> selectValueExpression,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TDbEntity : class
        {
            if (selectValueExpression == null) throw new ArgumentNullException(nameof(selectValueExpression));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheckAsync(async () =>
            {
                if (value == null)
                    return true;

                var query = QueryByEntity.GetNonTrackingQuery<TDbEntity>();

                if (filterExpression != null)
                    query = query.Where(filterExpression);

                var valueQuery = query.Select(selectValueExpression);
                var parameterExpression = Expression.Parameter(typeof(TValue), "x");
                valueQuery = valueQuery.Where(Expression.Lambda<Func<TValue, bool>>(
                    Expression.Equal(parameterExpression, Expression.Constant(value, typeof(TValue))),
                    parameterExpression));

                var queryResult = await AnyAsync(valueQuery);

                if (!queryResult)
                    return true;

                Errors.Add(new ApiMustBeUniqueError<TEntity, TValue>(attributeExpression, string.Format(ValidationMessages.Unique_AlreadyExists, value)));
                return false;
            });
        }

        [ContractAnnotation("values:null => true")]
        [UsedImplicitly]
        public bool CheckItemNotNull<TEntity, TElementValue>(
            [CanBeNull, ItemCanBeNull] TElementValue[] values, 
            [NotNull] Expression<Func<TEntity, TElementValue[]>> collectionAttributeExpression)
        {
            if (collectionAttributeExpression == null) throw new ArgumentNullException(nameof(collectionAttributeExpression));

            return ExecuteCheck(() =>
            {
                if (values == null)
                    return true;

                var failed = false;
                var idx = 0;

                foreach (var value in values)
                {
                    if (value == null)
                    {
                        var elementExpression = Expression.Lambda<Func<TEntity, object>>(
                            Expression.ArrayIndex(
                                collectionAttributeExpression.Body,
                                Expression.Constant(idx, typeof(int))),
                            collectionAttributeExpression.Parameters);

                        Errors.Add(new ApiAttributeError<TEntity>(elementExpression, ValidationMessages.NotNull));
                        failed = true;
                    }

                    idx++;
                }

                return !failed;
            });
        }

        [ContractAnnotation("values:null => true")]
        [UsedImplicitly]
        public bool CheckItemNotNull<TEntity, TElementValue>(
            [CanBeNull] TEntity model, 
            [CanBeNull, ItemCanBeNull] TElementValue[] values, 
            [NotNull] Expression<Func<TEntity, TElementValue[]>> collectionAttributeExpression) =>
            CheckItemNotNull(values, collectionAttributeExpression);

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckEnumValueIsDefined<TEntity, TValue>(
            [CanBeNull] TValue value, 
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
        {
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (value == null)
                    return true;

                var underlying = Nullable.GetUnderlyingType(typeof(TValue));
                if (underlying == null)
                {
                    if (Enum.IsDefined(typeof(TValue), value))
                        return true;
                }
                else
                {
                    if (Enum.IsDefined(underlying, Convert.ChangeType(value, underlying)))
                        return true;
                }

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), string.Format(ValidationMessages.EnumValueIsDefined, value)));
                return false;
            });
        }

        [ContractAnnotation("value:null => true")]
        [UsedImplicitly]
        public bool CheckEnumValueIsDefined<TEntity, TValue>(
            [CanBeNull] TEntity model,
            [CanBeNull] TValue value, 
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression) =>
            CheckEnumValueIsDefined(value, attributeExpression);


        [ContractAnnotation("id:null => true")]
        [UsedImplicitly]
        public bool CheckRelationExists<TEntity>(
            [CanBeNull] long? id, 
            [NotNull] HashSet<long> existingIds, 
            [NotNull] Expression<Func<TEntity, long?>> attributeExpression)
        {
            if (existingIds == null) throw new ArgumentNullException(nameof(existingIds));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (id == null)
                    return true;

                if (existingIds.Contains(id.Value))
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), string.Format(ValidationMessages.RelatedEntryExists, id.Value)));
                return false;
            });
        }

        [UsedImplicitly]
        public bool CheckRelationExists<TEntity>(
            long id, 
            [NotNull] HashSet<long> existingIds, 
            [NotNull] Expression<Func<TEntity, long?>> attributeExpression)
        {
            if (existingIds == null) throw new ArgumentNullException(nameof(existingIds));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (existingIds.Contains(id))
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    string.Format(ValidationMessages.RelatedEntryExists, id)));
                return false;
            });
        }

        [ContractAnnotation("id:null => true")]
        [UsedImplicitly]
        public Task<bool> CheckRelationExistsAsync<TEntity, TRelationDbEntity>(
            [CanBeNull] long? id, 
            [NotNull] Expression<Func<TEntity, long?>> attributeExpression) 
            where TRelationDbEntity : class, IId<long>
        {
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheckAsync(async () =>
            {
                if (id == null)
                    return true;

                var idValue = id.Value;

                var query = QueryByEntity
                    .GetNonTrackingQuery<TRelationDbEntity>()
                    .Where(x => x.Id == idValue);

                var queryResult = await AnyAsync(query);

                if (queryResult)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),                     string.Format(ValidationMessages.RelatedEntryExists, id.Value)));
                return false;
            });
        }

        [UsedImplicitly]
        public Task<bool> CheckRelationExistsAsync<TEntity, TRelationDbEntity>(
            long id, 
            [NotNull] Expression<Func<TEntity, long>> attributeExpression)
            where TRelationDbEntity : class, IId<long>
        {
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheckAsync(async () =>
            {
                var query = QueryByEntity
                    .GetNonTrackingQuery<TRelationDbEntity>()
                    .Where(x => x.Id == id);

                var queryResult = await AnyAsync(query);

                if (queryResult)
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    string.Format(ValidationMessages.RelatedEntryExists, id)));
                return false;
            });
        }

        [UsedImplicitly]
        public bool CheckDistinct<TEntity, TMember>(
            [CanBeNull, ItemCanBeNull] TMember[] collection, 
            [NotNull] Expression<Func<TEntity, TMember[]>> attributeExpression)
        {
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (collection == null)
                    return true;

                var collectionWithoutNull = collection
                    .Where(x => x != null)
                    .ToArray();

                if (collectionWithoutNull.Length == collectionWithoutNull.Distinct().Count())
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), ValidationMessages.Distinct));
                return false;
            });
        }

        [UsedImplicitly]
        public bool CheckDistinct<TEntity, TMember, TEqualityMember>(
            [ItemCanBeNull, CanBeNull] TMember[] collection, 
            [NotNull] IDistinctSelectEqualityMemberProvider<TMember, TEqualityMember> equalityProvider, 
            [NotNull] Expression<Func<TEntity, TMember[]>> attributeExpression)
        {
            if (equalityProvider == null) throw new ArgumentNullException(nameof(equalityProvider));
            if (attributeExpression == null) throw new ArgumentNullException(nameof(attributeExpression));

            return ExecuteCheck(() =>
            {
                if (collection == null)
                    return true;

                var collectionWithoutNull = collection
                    .Where(x => x != null)
                    .Select(equalityProvider.GetEqualityMember)
                    .ToArray();

                if (collectionWithoutNull.Length == collectionWithoutNull.Distinct().Count())
                    return true;

                Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                    ValidationMessages.Distinct));
                return false;
            });
        }
    }
}