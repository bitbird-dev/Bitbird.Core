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
        private IGetQueryByEntity queryByEntity = null;
        
        public bool HasErrors => Errors.Any();

        public void ThrowIfHasErrors()
        {
            if (HasErrors)
                throw new ApiErrorException(Errors.ToArray());
        }

        [NotNull]
        protected IGetQueryByEntity QueryByEntity => queryByEntity ?? throw new Exception($"An instance of {nameof(IGetQueryByEntity)} must be registered before this method can be used.");

        public void RegisterQueryByEntity([NotNull] IGetQueryByEntity queryByEntity)
        {
            this.queryByEntity = queryByEntity;
        }

        [NotNull]
        protected abstract Task<bool> AnyAsync<T>([NotNull] IQueryable<T> query);

        private bool ExecuteCheck([NotNull] Func<bool> checkAction)
        {
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

        [NotNull]
        internal Expression<Func<TEntity, object>> CastExpression<TEntity, TResult>(
            [NotNull] Expression<Func<TEntity, TResult>> expression)
        {
            return Expression.Lambda<Func<TEntity, object>>(Expression.Convert(expression.Body, typeof(object)), expression.Parameters);
        }

        public void AddError([NotNull] ApiError error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            Errors.Add(error);
        }

        [ContractAnnotation("value:null => false; value:notnull => true")]
        public bool CheckNotNull<TEntity>(
            [CanBeNull] object value,
            [NotNull] Expression<Func<TEntity, object>> attributeExpression) =>
            ExecuteCheck(() =>
            {
                if (value == null)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(attributeExpression, ValidationMessages.NotNull));
                    return false;
                }

                return true;
            });
        [ContractAnnotation("value:null => false; value:notnull => true")]
        public bool CheckNotNull<TEntity>(
            [NotNull] TEntity model,
            [CanBeNull] object value,
            [NotNull] Expression<Func<TEntity, object>> attributeExpression) => CheckNotNull(value, attributeExpression);

        [ContractAnnotation("value:null => false")]
        public bool CheckNotNullOrEmpty<TEntity>(
            [CanBeNull] string value,
            [NotNull] Expression<Func<TEntity, string>> attributeExpression) =>
            ExecuteCheck(() =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), ValidationMessages.NotNullOrEmpty));
                    return false;
                }

                return true;
            });
        [ContractAnnotation("value:null => false")]
        public bool CheckNotNullOrEmpty<TEntity>(
            [NotNull] TEntity model,
            [CanBeNull] string value,
            [NotNull] Expression<Func<TEntity, string>> attributeExpression) => CheckNotNullOrEmpty(value, attributeExpression);

        [ContractAnnotation("value:null => true")]
        public bool CheckNotEmpty<TEntity>(
            [CanBeNull] string value,
            [NotNull] Expression<Func<TEntity, string>> attributeExpression) =>
            ExecuteCheck(() =>
            {
                if (value != null && value.Trim().Length == 0)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), ValidationMessages.NotEmpty));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("value:null => true")]
        public bool CheckMaxStringLength<TEntity>(
            [CanBeNull] string value,
            int maxLength,
            [NotNull] Expression<Func<TEntity, string>> attributeExpression) =>
            ExecuteCheck(() =>
            {
                if (value != null && value.Length > maxLength)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), string.Format(ValidationMessages.StringMaxLength, maxLength)));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("value:null => true")]
        public bool CheckTrimmed<TEntity>(
            [CanBeNull] string value,
            [NotNull] Expression<Func<TEntity, string>> attributeExpression) =>
            ExecuteCheck(() =>
            {
                if (value != null && !value.Trim().Equals(value) )
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), ValidationMessages.Trimmed));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("value:null => true")]
        public bool CheckGreaterThan<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue limit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue> =>
            ExecuteCheck(() =>
            {
                if (value != null && value.CompareTo(limit) <= 0)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), 
                        string.Format(ValidationMessages.GreaterThan, limit, value)));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("value:null => true")]
        public bool CheckGreaterThanEqual<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue limit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue> =>
            ExecuteCheck(() =>
            {
                if (value != null && value.CompareTo(limit) < 0)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                        string.Format(ValidationMessages.GreaterThanEqual, limit, value)));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("value:null => true")]
        public bool CheckLessThan<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue limit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue> =>
            ExecuteCheck(() =>
            {
                if (value != null && value.CompareTo(limit) >= 0)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                        string.Format(ValidationMessages.LessThan, limit, value)));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("value:null => true")]
        public bool CheckLessThanEqual<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue limit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue> =>
            ExecuteCheck(() =>
            {
                if (value != null && value.CompareTo(limit) > 0)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                        string.Format(ValidationMessages.LessThanEqual, limit, value)));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("value:null => true")]
        public bool CheckBetweenInclusive<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue lowerLimit,
            [NotNull] TValue upperLimit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue> =>
            ExecuteCheck(() =>
            {
                if (value != null && value.CompareTo(lowerLimit) >= 0 && value.CompareTo(upperLimit) <= 0)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                        string.Format(ValidationMessages.BetweenInclusive, lowerLimit, upperLimit, value)));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("value:null => true")]
        public bool CheckBetweenExclusive<TEntity, TValue>(
            [CanBeNull] TValue value,
            [NotNull] TValue lowerLimit,
            [NotNull] TValue upperLimit,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TValue : IComparable<TValue> =>
            ExecuteCheck(() =>
            {
                if (value != null && value.CompareTo(lowerLimit) >= 0 && value.CompareTo(upperLimit) <= 0)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression),
                        string.Format(ValidationMessages.BetweenExclusive, lowerLimit, upperLimit, value)));
                    return false;
                }

                return true;
            });

        [NotNull]
        public Task<bool> CheckUniqueAsync<TEntity, TDbEntity, TValue>(
            [CanBeNull] TValue value, 
            [CanBeNull] Expression<Func<TDbEntity, bool>> filterExpression,
            [NotNull] Expression<Func<TDbEntity, TValue>> selectValueExpression,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TDbEntity : class =>
            ExecuteCheckAsync(async () =>
            {
                var query = QueryByEntity.GetNonTrackingQuery<TDbEntity>();

                if (filterExpression != null)
                    query = query.Where(filterExpression);

                var valueQuery = query.Select(selectValueExpression);
                var parameterExpression = Expression.Parameter(typeof(TValue), "x");
                valueQuery = valueQuery.Where(Expression.Lambda<Func<TValue, bool>>(Expression.Equal(parameterExpression, Expression.Constant(value, typeof(TValue))), parameterExpression));

                var queryResult = await AnyAsync(valueQuery);

                if (queryResult)
                {
                    Errors.Add(new ApiMustBeUniqueError<TEntity, TValue>(attributeExpression, string.Format(ValidationMessages.Unique_AlreadyExists, value)));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("value:null => true"), NotNull]
        public Task<bool> CheckUniqueIfNotNullAsync<TEntity, TDbEntity, TValue>(
            [CanBeNull] TValue value,
            [CanBeNull] Expression<Func<TDbEntity, bool>> filterExpression,
            [NotNull] Expression<Func<TDbEntity, TValue>> selectValueExpression,
            [NotNull] Expression<Func<TEntity, TValue>> attributeExpression)
            where TDbEntity : class =>
            ExecuteCheckAsync(async () =>
            {
                if (value == null)
                    return true;

                var query = QueryByEntity.GetNonTrackingQuery<TDbEntity>();

                if (filterExpression != null)
                    query = query.Where(filterExpression);

                var valueQuery = query.Select(selectValueExpression);
                var parameterExpression = Expression.Parameter(typeof(TValue), "x");
                valueQuery = valueQuery.Where(Expression.Lambda<Func<TValue, bool>>(Expression.Equal(parameterExpression, Expression.Constant(value, typeof(TValue))), parameterExpression));

                var queryResult = await AnyAsync(valueQuery);

                if (queryResult)
                {
                    Errors.Add(new ApiMustBeUniqueError<TEntity, TValue>(attributeExpression, string.Format(ValidationMessages.Unique_AlreadyExists, value)));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("value:null => true")]
        public bool CheckItemNotNull<TEntity, TElementValue>(TElementValue[] values, Expression<Func<TEntity, TElementValue[]>> collectionAttributeExpression) =>
            ExecuteCheck(() =>
            {
                if (values == null)
                    return true;

                bool failed = false;
                int idx = 0;

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
        [ContractAnnotation("value:null => true")]
        public bool CheckItemNotNull<TEntity, TElementValue>([NotNull] TEntity model, TElementValue[] values, Expression<Func<TEntity, TElementValue[]>> collectionAttributeExpression) =>
            CheckItemNotNull(values, collectionAttributeExpression);

        [ContractAnnotation("value:null => true")]
        public bool CheckEnumValueIsDefined<TEntity, TValue>(TValue value, Expression<Func<TEntity, TValue>> attributeExpression) =>
            ExecuteCheck(() =>
            {
                var underlying = Nullable.GetUnderlyingType(typeof(TValue));
                if (underlying == null)
                { 
                    if (!Enum.IsDefined(typeof(TValue), value))
                    {
                        Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), string.Format(ValidationMessages.EnumValueIsDefined, value)));
                        return false;
                    }

                    return true;
                }

                if (value == null)
                    return true;

                if (!Enum.IsDefined(underlying, Convert.ChangeType(value, underlying)))
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), string.Format(ValidationMessages.EnumValueIsDefined, value)));
                    return false;
                }
                

                return true;
            });
        [ContractAnnotation("value:null => true")]
        public bool CheckEnumValueIsDefined<TEntity, TValue>([NotNull] TEntity model, TValue value, Expression<Func<TEntity, TValue>> attributeExpression) =>
            CheckEnumValueIsDefined(value, attributeExpression);


        [ContractAnnotation("id:null => true")]
        public bool CheckRelationExists<TEntity>(long? id, HashSet<long> existingIds, Expression<Func<TEntity, long?>> attributeExpression)
            => ExecuteCheck(() =>
            {
                if (id == null)
                    return true;

                if (!existingIds.Contains(id.Value))
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), string.Format(ValidationMessages.RelatedEntryExists, id.Value)));
                    return false;
                }

                return true;
            });

        public bool CheckRelationExists<TEntity>(long id, HashSet<long> existingIds, Expression<Func<TEntity, long?>> attributeExpression)
            => ExecuteCheck(() =>
            {
                if (!existingIds.Contains(id))
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), string.Format(ValidationMessages.RelatedEntryExists, id)));
                    return false;
                }

                return true;
            });

        [ContractAnnotation("id:null => true")]
        public Task<bool> CheckRelationExistsAsync<TEntity, TRelationDbEntity>(long? id, Expression<Func<TEntity, long?>> attributeExpression) 
            where TRelationDbEntity : class, IId<long> =>
            ExecuteCheckAsync(async () =>
            {
                if (id == null)
                    return true;

                var idValue = id.Value;

                var query = QueryByEntity
                    .GetNonTrackingQuery<TRelationDbEntity>()
                    .Where(x => x.Id == idValue);

                var queryResult = await AnyAsync(query);

                if (!queryResult)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), string.Format(ValidationMessages.RelatedEntryExists, id.Value)));
                    return false;
                }

                return true;
            });

        public Task<bool> CheckRelationExistsAsync<TEntity, TRelationDbEntity>(long id, Expression<Func<TEntity, long>> attributeExpression)
            where TRelationDbEntity : class, IId<long> =>
            ExecuteCheckAsync(async () =>
            {
                var query = QueryByEntity
                    .GetNonTrackingQuery<TRelationDbEntity>()
                    .Where(x => x.Id == id);

                var queryResult = await AnyAsync(query);

                if (!queryResult)
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), string.Format(ValidationMessages.RelatedEntryExists, id)));
                    return false;
                }

                return true;
            });

        public bool CheckDistinct<TEntity, TMember>(TMember[] collection, Expression<Func<TEntity, TMember[]>> attributeExpression)
            => ExecuteCheck(() =>
            {
                if (collection == null)
                    return true;

                var collectionWithoutNull = collection.Where(x => x != null).ToArray();

                if (collectionWithoutNull.Length != collectionWithoutNull.Distinct().Count())
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), ValidationMessages.Distinct));
                    return false;
                }

                return true;
            });

        public bool CheckDistinct<TEntity, TMember, TEqualityMember>(TMember[] collection, IDistinctSelectEqualityMemberProvider<TMember, TEqualityMember> equalityProvider, Expression<Func<TEntity, TMember[]>> attributeExpression)
            => ExecuteCheck(() =>
            {
                if (collection == null)
                    return true;

                var collectionWithoutNull = collection.Where(x => x != null).Select(x => equalityProvider.GetEqualityMember(x)).ToArray();

                if (collectionWithoutNull.Length != collectionWithoutNull.Distinct().Count())
                {
                    Errors.Add(new ApiAttributeError<TEntity>(CastExpression(attributeExpression), ValidationMessages.Distinct));
                    return false;
                }

                return true;
            });
    }
}