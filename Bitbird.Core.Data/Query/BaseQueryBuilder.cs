using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bitbird.Core;
using Bitbird.Core.Benchmarks;
using Bitbird.Core.Data;
using Bitbird.Core.Data.Query;
using Bitbird.Core.Data.Query.Mapping;
using Bitbird.Core.Expressions;
using Bitbird.Core.Extensions;
using Bitbird.Core.Query;

namespace Bitbird.Core.Data.Query
{
    /// <summary>
    /// Provides various methods to build queries based on other queries.
    /// </summary>
    public static class QueryBuilder
    {
        private static readonly MethodInfo OrderByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == nameof(Queryable.OrderBy) && method.GetParameters().Length == 2);
        private static readonly MethodInfo OrderByDescMethod = typeof(Queryable).GetMethods().Single(method => method.Name == nameof(Queryable.OrderByDescending) && method.GetParameters().Length == 2);
        private static readonly MethodInfo ThenByMethod = typeof(Queryable).GetMethods().Single(method => method.Name == nameof(Queryable.ThenBy) && method.GetParameters().Length == 2);
        private static readonly MethodInfo ThenByDescMethod = typeof(Queryable).GetMethods().Single(method => method.Name == nameof(Queryable.ThenByDescending) && method.GetParameters().Length == 2);
        private static readonly MethodInfo ContainsMethod = typeof(Enumerable).GetMethods().Single(method => method.Name == nameof(Enumerable.Contains) && method.GetParameters().Length == 2);
        private static readonly MethodInfo CastMethod = typeof(Enumerable).GetMethods().Single(method => method.Name == nameof(Enumerable.Cast) && method.GetParameters().Length == 1);

        private static MethodInfo likeMethod;
        public static MethodInfo LikeMethod
        {
            get => likeMethod ?? throw new Exception($"{nameof(QueryBuilder)}.{nameof(LikeMethod)} was not initialized before it was used. This initialization should be done by the framework-specific (e.g. .NET Core or .NET Framework) implementation.");
            set => likeMethod = value;
        }
        public static Expression LikeMethodInstance { get; set; }

        /// <summary>
        /// Invokes the passed generic <c>method</c>, using the record type as generic parameter, and the return type of <c>expression</c> as return type, on the passed <c>query</c> with the passed <c>expression</c> as parameter.
        /// The passed method must return an <see cref="IOrderedQueryable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The record type.</typeparam>
        /// <param name="method">The generic method to invoke.</param>
        /// <param name="query">The query to invoke the method on.</param>
        /// <param name="expression">The expression to pass to the method.</param>
        /// <returns>The result query.</returns>
        private static IOrderedQueryable<TSource> DynamicInvokeForAttributeExpression<TSource>(MethodInfo method, IQueryable<TSource> query, LambdaExpression expression)
        {
            var genericMethod = method.MakeGenericMethod(typeof(TSource), expression.ReturnType);
            var ret = genericMethod.Invoke(null, new object[] { query, expression });
            return (IOrderedQueryable<TSource>)ret;
        }
        /// <summary>
        /// Applies OrderBy on the given <c>query</c> with the passed <c>expression</c> as sorting expression.
        /// </summary>
        /// <typeparam name="TSource">The record type.</typeparam>
        /// <param name="query">The query to work on. Must not be null.</param>
        /// <param name="expression">The sorting expression. Must not be null. Must take the record type as parameter.</param>
        /// <returns>The sorted query.</returns>
        private static IOrderedQueryable<TSource> DynamicOrderByDesc<TSource>(IQueryable<TSource> query, LambdaExpression expression)
            => DynamicInvokeForAttributeExpression(OrderByDescMethod, query, expression);

        /// <summary>
        /// Return a lambda expression, taking <c>tDbModel</c> as parameter, and returning the property defined by <c>dottedProperty</c> mapped from <c>tModel</c> to <c>tDbModel</c>.
        /// </summary>
        /// <param name="tModel">The model defining the dotted property and a mapping to <c>tDbModel</c>.</param>
        /// <param name="tDbModel">The model on which to operate.</param>
        /// <param name="dottedProperty">The property defined on <c>tModel</c>.</param>
        /// <returns>A lambda expression for the resulting property access.</returns>
        public static LambdaExpression GetLambdaFromDottedProperty(Type tModel, Type tDbModel, string dottedProperty)
        {
            var parameter = Expression.Parameter(tDbModel, "x");
            var lambdaData = GetLambdaBodyFromDottedProperty(parameter, tModel, tDbModel, dottedProperty);
            return Expression.Lambda(lambdaData.Expression, parameter);
        }
        private static (Expression Expression, Type ModelType, Type DbModelType, Type UnderlyingDbModelType, Expression UnderlyingExpression) GetLambdaBodyFromDottedProperty(ParameterExpression parameter, Type tModel, Type tDbModel, string dottedProperty)
        {
            var path = dottedProperty.Split('.');

            Expression expression = parameter;
            var tCurrentModel = tModel;
            var tCurrentDbModel = tDbModel;

            foreach (var node in path)
            {
                if (!ModelsMetaData.Instance.ByModelType.TryGetValue(tCurrentModel, out var modelMetaData))
                    throw new Exception($"QueryBuilder: Could add sorting to query for data model {tModel.Name}. No database mapping was found for any property this type. Path: {dottedProperty}. Node: {node}. CurrentModelName: {tCurrentModel.Name}. CurrentDbModelName: {tCurrentDbModel.Name}.");

                if (!modelMetaData.ByDbModelType.TryGetValue(tCurrentDbModel, out var dbModelMetaData))
                    throw new Exception($"QueryBuilder: Could add sorting to query for {tModel.Name}. No database mapping was found for the database model {tDbModel.Name}. Path: {dottedProperty}. Node: {node}. CurrentModelName: {tCurrentModel.Name}. CurrentDbModelName: {tCurrentDbModel.Name}.");

                if (!dbModelMetaData.ByProperty.TryGetValue(node.ToUpperInvariant(), out var propertyMappingMetaData))
                    throw new Exception($"QueryBuilder: Could add sorting to query for {tModel.Name}. No database mapping was found from the database model {tDbModel.Name} to the property {node}. Path: {dottedProperty}. Node: {node}. CurrentModelName: {tCurrentModel.Name}. CurrentDbModelName: {tCurrentDbModel.Name}");
                
                expression = ParameterRebinder.ReplaceParameters(new Dictionary<ParameterExpression, Expression> { { propertyMappingMetaData.Expression.Parameters[0], expression } }, propertyMappingMetaData.Expression.Body);
                tCurrentModel = propertyMappingMetaData.PropertyType;
                tCurrentDbModel = propertyMappingMetaData.Expression.ReturnType;
            }

            if (typeof(IId<long>).IsAssignableFrom(tCurrentModel))
            {
                if (!ModelsMetaData.Instance.ByModelType.TryGetValue(tCurrentModel, out var modelMetaData))
                    throw new Exception($"QueryBuilder: Could add sorting to query for data model {tCurrentModel.Name}. No database mapping was found for any property this type.");

                if (!modelMetaData.ByDbModelType.TryGetValue(tCurrentDbModel, out var dbModelMetaData))
                    throw new Exception($"QueryBuilder: Could add sorting to query for {tCurrentModel.Name}. No database mapping was found for the database model {tCurrentDbModel.Name}.");
                
                expression = ParameterRebinder.ReplaceParameters(new Dictionary<ParameterExpression, Expression> { { dbModelMetaData.DefaultSortProperty.Expression.Parameters[0], expression } }, dbModelMetaData.DefaultSortProperty.Expression.Body);
                tCurrentModel = dbModelMetaData.DefaultSortProperty.PropertyType;
                tCurrentDbModel = dbModelMetaData.DefaultSortProperty.Expression.ReturnType;
            }

            var underlyingType = Nullable.GetUnderlyingType(tCurrentDbModel);
            var underlyingExpression = expression;
            if (underlyingType != null)
                underlyingExpression = Expression.Property(expression, tCurrentDbModel, "Value");
            else
                underlyingType = tCurrentDbModel;

            return (expression, tCurrentModel, tCurrentDbModel, underlyingType, underlyingExpression);
        }

        /// <summary>
        /// Adds sorting to the query based on property names and sorting directions.
        /// Sorting is done based on the passed sort properties.
        /// </summary>
        /// <typeparam name="TModel">The record type.</typeparam>
        /// <param name="query">The query on which to operate. Must not be null.</param>
        /// <param name="sortProperties">Sorting information. Can be null (if null, no sorting will be applied). Must not contain null entries.</param>
        /// <returns>The sorted query.</returns>
        internal static IOrderedQueryable<TModel> BuildSortedQuery<TModel>(IQueryable<TModel> query, QuerySortProperty[] sortProperties)
        {
            if (sortProperties == null || sortProperties.Length == 0)
                return query.OrderBy(t => true);

            IOrderedQueryable<TModel> orderedQuery = null;
            foreach (var prop in sortProperties)
            {
                var sortExpression = PropertiesHelper.GetDottedPropertyGetterExpression(prop.PropertyName, typeof(TModel));

                var method = orderedQuery == null
                    ? (prop.Ascending ? OrderByMethod : OrderByDescMethod)
                    : (prop.Ascending ? ThenByMethod : ThenByDescMethod);

                orderedQuery = DynamicInvokeForAttributeExpression(method, orderedQuery ?? query, sortExpression);
            }

            return orderedQuery;
        }

        /// <summary>
        /// Adds sorting to the db model query based on property names of the api model the and sorting directions.
        /// Sorting is done based on the passed sort properties.
        /// Mappings from the api model to the db model must be defined <see cref="DbMappingExpressionAttribute"/>.
        /// </summary>
        /// <typeparam name="TModel">The api model type.</typeparam>
        /// <typeparam name="TDbModel">The db model type.</typeparam>
        /// <param name="query">The query on which to operate. Must not be null.</param>
        /// <param name="sortProperties">Sorting information. Can be null (if null, no sorting will be applied). Must not contain null entries.</param>
        /// <returns>The sorted query.</returns>
        internal static IOrderedQueryable<TDbModel> BuildSortedQuery<TDbModel, TModel>(IQueryable<TDbModel> query, QuerySortProperty[] sortProperties)
        {
            if (sortProperties == null || sortProperties.Length == 0)
            {
                if (!ModelsMetaData.Instance.ByModelType.TryGetValue(typeof(TModel), out var modelMetaData))
                {
                    Debug.WriteLine($"QUERY BUILDING ERROR: Could add sorting to query for data model {typeof(TModel).Name}. No database mapping was found for any property this type.");
                    return query.OrderBy(x => true);
                }
                if (!modelMetaData.ByDbModelType.TryGetValue(typeof(TDbModel), out var dbModelMetaData))
                {
                    Debug.WriteLine($"QUERY BUILDING ERROR: Could add sorting to query for {typeof(TModel).Name}. No database mapping was found for the database model {typeof(TDbModel).Name}.");
                    return query.OrderBy(x => true);
                }

                return DynamicOrderByDesc(query, dbModelMetaData.DefaultSortProperty.Expression);
            }

            IOrderedQueryable<TDbModel> orderedQuery = null;
            foreach (var prop in sortProperties)
            {
                var sortExpression = GetLambdaFromDottedProperty(typeof(TModel), typeof(TDbModel), prop.PropertyName);

                var method = orderedQuery == null
                    ? (prop.Ascending ? OrderByMethod : OrderByDescMethod)
                    : (prop.Ascending ? ThenByMethod : ThenByDescMethod);

                orderedQuery = DynamicInvokeForAttributeExpression(method, orderedQuery ?? query, sortExpression);
            }

            return orderedQuery;
        }
        internal static IQueryable<TModel> BuildFilteredQuery<TModel>(IQueryable<TModel> query, QueryFilter[] filters)
        {
            if (filters == null || filters.Length == 0)
                return query;

            var parameter = Expression.Parameter(typeof(TModel), "x");
            Expression filterExpression = null;
            foreach (var filter in filters)
            {
                var lambdaBody = PropertiesHelper.GetDottedPropertyGetterExpressionBody(parameter, filter.PropertyName, typeof(TModel), null, out var lambdaReturnType);

                var lambdaUnderlyingReturnType = Nullable.GetUnderlyingType(lambdaReturnType);
                var lambdaReturnTypeIsNullable = lambdaUnderlyingReturnType != null;
                var lambdaUnderlyingBody = lambdaReturnTypeIsNullable ? Expression.Property(lambdaBody, nameof(Nullable<int>.Value)) : lambdaBody;
                lambdaUnderlyingReturnType = lambdaUnderlyingReturnType ?? lambdaReturnType;

                Expression currentFilterExpression;
                switch (filter)
                {
                    case QueryExactFilter exact:
                        var typedValue = exact.Value.ParseAs(lambdaUnderlyingReturnType);

                        if (!lambdaReturnTypeIsNullable)
                            currentFilterExpression = Expression.Equal(lambdaUnderlyingBody, Expression.Constant(typedValue, lambdaUnderlyingReturnType));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaBody, Expression.Constant(null)),
                                                                         Expression.Equal(lambdaUnderlyingBody, Expression.Constant(typedValue, lambdaUnderlyingReturnType)));
                        break;
                    case QueryGtFilter gt:
                        var gtTypedLower = gt.Lower.ParseAs(lambdaUnderlyingReturnType);

                        if (!lambdaReturnTypeIsNullable)
                            currentFilterExpression = Expression.GreaterThan(lambdaUnderlyingBody, Expression.Constant(gtTypedLower, lambdaUnderlyingReturnType));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaBody, Expression.Constant(null)),
                                Expression.GreaterThan(lambdaUnderlyingBody, Expression.Constant(gtTypedLower, lambdaUnderlyingReturnType)));
                        break;
                    case QueryGteFilter gte:
                        var gteTypedLower = gte.Lower.ParseAs(lambdaUnderlyingReturnType);

                        if (!lambdaReturnTypeIsNullable)
                            currentFilterExpression = Expression.GreaterThanOrEqual(lambdaUnderlyingBody, Expression.Constant(gteTypedLower, lambdaUnderlyingReturnType));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaBody, Expression.Constant(null)),
                                Expression.GreaterThanOrEqual(lambdaUnderlyingBody, Expression.Constant(gteTypedLower, lambdaUnderlyingReturnType)));
                        break;
                    case QueryLtFilter lt:
                        var ltTypedUpper = lt.Upper.ParseAs(lambdaUnderlyingReturnType);

                        if (!lambdaReturnTypeIsNullable)
                            currentFilterExpression = Expression.LessThan(lambdaUnderlyingBody, Expression.Constant(ltTypedUpper, lambdaUnderlyingReturnType));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaBody, Expression.Constant(null)),
                                Expression.LessThan(lambdaUnderlyingBody, Expression.Constant(ltTypedUpper, lambdaUnderlyingReturnType)));
                        break;
                    case QueryLteFilter lte:
                        var lteTypedUpper = lte.Upper.ParseAs(lambdaUnderlyingReturnType);

                        if (!lambdaReturnTypeIsNullable)
                            currentFilterExpression = Expression.LessThanOrEqual(lambdaUnderlyingBody, Expression.Constant(lteTypedUpper, lambdaUnderlyingReturnType));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaBody, Expression.Constant(null)),
                                Expression.LessThanOrEqual(lambdaUnderlyingBody, Expression.Constant(lteTypedUpper, lambdaUnderlyingReturnType)));
                        break;
                    case QueryRangeFilter range:
                        var typedLower = range.Lower.ParseAs(lambdaUnderlyingReturnType);
                        var typedUpper = range.Upper.ParseAs(lambdaUnderlyingReturnType);

                        if (!lambdaReturnTypeIsNullable)
                            currentFilterExpression = Expression.AndAlso(Expression.GreaterThanOrEqual(lambdaUnderlyingBody, Expression.Constant(typedLower, lambdaUnderlyingReturnType)),
                                                                         Expression.LessThanOrEqual(lambdaUnderlyingBody, Expression.Constant(typedUpper, lambdaUnderlyingReturnType)));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaBody, Expression.Constant(null)),
                                                                         Expression.AndAlso(Expression.GreaterThanOrEqual(lambdaUnderlyingBody, Expression.Constant(typedLower, lambdaUnderlyingReturnType)),
                                                                                            Expression.LessThanOrEqual(lambdaUnderlyingBody, Expression.Constant(typedUpper, lambdaUnderlyingReturnType))));
                        break;
                    case QueryInFilter @in:
                        var typedCollection = (object)@in.Values.Select(v => v.ParseAs(lambdaUnderlyingReturnType));
                        typedCollection = CastMethod.MakeGenericMethod(lambdaUnderlyingReturnType).Invoke(null, new[] { typedCollection });
                        currentFilterExpression = Expression.Call(ContainsMethod.MakeGenericMethod(lambdaUnderlyingReturnType), Expression.Constant(typedCollection), lambdaUnderlyingBody);
                        break;
                    case QueryFreeTextFilter freeText:
                        if (lambdaUnderlyingReturnType != typeof(string))
                            throw new NotSupportedException("Freetext filters are only allowed on string fields.");
                        if (query.IsInMemory())
                            currentFilterExpression = Expression.Call(null, typeof(Regex).GetMethod(nameof(Regex.IsMatch),new []{typeof(string), typeof(string)}) ?? throw new Exception("Could not find Regex.IsMatch(string,string)"), lambdaUnderlyingBody, Expression.Constant(freeText.Pattern.WildCardPatternToRegexPattern()));
                        else
                            currentFilterExpression = Expression.Call(LikeMethodInstance, LikeMethod, lambdaUnderlyingBody, Expression.Constant(freeText.Pattern.Replace("*", "%")));
                        break;
                    default:
                        throw new NotSupportedException($"QueryBuilder.BuildFilteredQuery: A {nameof(QueryFilter)} of type {filter.GetType().Name} was found, but is currently not supported");
                }

                filterExpression = filterExpression == null
                    ? currentFilterExpression
                    : Expression.AndAlso(filterExpression, currentFilterExpression);
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            var lambda = Expression.Lambda<Func<TModel, bool>>(filterExpression, parameter);
            return query.Where(lambda);
        }
        internal static IQueryable<TDbModel> BuildFilteredQuery<TDbModel, TModel>(IQueryable<TDbModel> query, QueryFilter[] filters)
        {
            if (filters == null || filters.Length == 0)
                return query;

            var parameter = Expression.Parameter(typeof(TDbModel), "x");
            Expression filterExpression = null;
            foreach (var filter in filters)
            {
                var lambdaData = GetLambdaBodyFromDottedProperty(parameter, typeof(TModel), typeof(TDbModel), filter.PropertyName);
                Expression currentFilterExpression;
                switch (filter)
                {
                    case QueryExactFilter exact:
                        var typedValue = exact.Value.ParseAs(lambdaData.UnderlyingDbModelType);

                        if (lambdaData.UnderlyingDbModelType == lambdaData.DbModelType)
                            currentFilterExpression = Expression.Equal(lambdaData.UnderlyingExpression, Expression.Constant(typedValue, lambdaData.UnderlyingDbModelType));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaData.Expression, Expression.Constant(null)),
                                                                         Expression.Equal(lambdaData.UnderlyingExpression, Expression.Constant(typedValue, lambdaData.UnderlyingDbModelType)));
                        break;
                    case QueryGtFilter gt:
                        var gtTypedUpper = gt.Lower.ParseAs(lambdaData.UnderlyingDbModelType);

                        if (lambdaData.UnderlyingDbModelType == lambdaData.DbModelType)
                            currentFilterExpression = Expression.GreaterThan(lambdaData.UnderlyingExpression, Expression.Constant(gtTypedUpper, lambdaData.UnderlyingDbModelType));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaData.Expression, Expression.Constant(null)),
                                Expression.GreaterThan(lambdaData.UnderlyingExpression, Expression.Constant(gtTypedUpper, lambdaData.UnderlyingDbModelType)));
                        break;
                    case QueryGteFilter gte:
                        var gteTypedUpper = gte.Lower.ParseAs(lambdaData.UnderlyingDbModelType);

                        if (lambdaData.UnderlyingDbModelType == lambdaData.DbModelType)
                            currentFilterExpression = Expression.GreaterThanOrEqual(lambdaData.UnderlyingExpression, Expression.Constant(gteTypedUpper, lambdaData.UnderlyingDbModelType));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaData.Expression, Expression.Constant(null)),
                                Expression.GreaterThanOrEqual(lambdaData.UnderlyingExpression, Expression.Constant(gteTypedUpper, lambdaData.UnderlyingDbModelType)));
                        break;
                    case QueryLtFilter lt:
                        var ltTypedUpper = lt.Upper.ParseAs(lambdaData.UnderlyingDbModelType);

                        if (lambdaData.UnderlyingDbModelType == lambdaData.DbModelType)
                            currentFilterExpression = Expression.LessThan(lambdaData.UnderlyingExpression, Expression.Constant(ltTypedUpper, lambdaData.UnderlyingDbModelType));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaData.Expression, Expression.Constant(null)),
                                Expression.LessThan(lambdaData.UnderlyingExpression, Expression.Constant(ltTypedUpper, lambdaData.UnderlyingDbModelType)));
                        break;
                    case QueryLteFilter lte:
                        var lteTypedUpper = lte.Upper.ParseAs(lambdaData.UnderlyingDbModelType);

                        if (lambdaData.UnderlyingDbModelType == lambdaData.DbModelType)
                            currentFilterExpression = Expression.LessThanOrEqual(lambdaData.UnderlyingExpression, Expression.Constant(lteTypedUpper, lambdaData.UnderlyingDbModelType));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaData.Expression, Expression.Constant(null)),
                                Expression.LessThanOrEqual(lambdaData.UnderlyingExpression, Expression.Constant(lteTypedUpper, lambdaData.UnderlyingDbModelType)));
                        break;
                    case QueryRangeFilter range:
                        var typedLower = range.Lower.ParseAs(lambdaData.UnderlyingDbModelType);
                        var typedUpper = range.Upper.ParseAs(lambdaData.UnderlyingDbModelType);

                        if (lambdaData.UnderlyingDbModelType == lambdaData.DbModelType)
                            currentFilterExpression = Expression.AndAlso(Expression.GreaterThanOrEqual(lambdaData.UnderlyingExpression, Expression.Constant(typedLower, lambdaData.UnderlyingDbModelType)),
                                                                         Expression.LessThanOrEqual(lambdaData.UnderlyingExpression, Expression.Constant(typedUpper, lambdaData.UnderlyingDbModelType)));
                        else
                            currentFilterExpression = Expression.AndAlso(Expression.NotEqual(lambdaData.Expression, Expression.Constant(null)),
                                                                         Expression.AndAlso(Expression.GreaterThanOrEqual(lambdaData.UnderlyingExpression, Expression.Constant(typedLower, lambdaData.UnderlyingDbModelType)),
                                                                                            Expression.LessThanOrEqual(lambdaData.UnderlyingExpression, Expression.Constant(typedUpper, lambdaData.UnderlyingDbModelType))));
                        break;
                    case QueryInFilter @in:
                        var typedCollection = (object)@in.Values.Select(v => v.ParseAs(lambdaData.UnderlyingDbModelType));
                        typedCollection = CastMethod.MakeGenericMethod(lambdaData.UnderlyingDbModelType).Invoke(null, new []{ typedCollection });
                        currentFilterExpression = Expression.Call(ContainsMethod.MakeGenericMethod(lambdaData.UnderlyingDbModelType), Expression.Constant(typedCollection), lambdaData.UnderlyingExpression);
                        break;
                    case QueryFreeTextFilter freeText:
                        if (lambdaData.UnderlyingDbModelType != typeof(string))
                            throw new NotSupportedException("Freetext filters are only allowed on string fields.");
                        currentFilterExpression = Expression.Call(LikeMethodInstance, LikeMethod, lambdaData.UnderlyingExpression, Expression.Constant(freeText.Pattern.Replace("*","%")));
                        break;
                    default:
                        throw new NotSupportedException($"QueryBuilder.BuildFilteredQuery: A {nameof(QueryFilter)} of type {filter.GetType().Name} was found, but is currently not supported");
                }

                filterExpression = filterExpression == null 
                    ? currentFilterExpression 
                    : Expression.AndAlso(filterExpression, currentFilterExpression);
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            var lambda = Expression.Lambda<Func<TDbModel, bool>>(filterExpression, parameter);
            return query.Where(lambda);
        }
        /// <summary>
        /// Adds paging to the query.
        /// Only models on the current page are returned.
        /// </summary>
        /// <typeparam name="TModel">The record type.</typeparam>
        /// <param name="query">The query on which to operate. Must not be null.</param>
        /// <param name="paging">Paging information. Can be null (if null, no paging will be applied).</param>
        /// <returns>The paged query.</returns>
        public static IQueryable<TModel> BuildPagedQuery<TModel>(this IOrderedQueryable<TModel> query, QueryPaging paging)
        {
            if (paging == null)
                return query;

            return query
                .Skip(paging.PageSize * paging.Page)
                .Take(paging.PageSize);
        }

        /// <summary>
        /// Adds permission checks to the query.
        /// Only models that pass the permission check are returned.
        /// Checks for the permission <see cref="AccessType.Read"/>.
        /// </summary>
        /// <typeparam name="TModel">The record type.</typeparam>
        /// <param name="query">The query on which to operate. Must not be null.</param>
        /// <param name="permissionResolver">The permission resolver to use. Can be null.</param>
        /// <returns>The secured query.</returns>
        public static IQueryable<TModel> BuildSecuredQuery<TModel>(this IQueryable<TModel> query, IPermissionResolver permissionResolver)
            where TModel : class
        {
            var expression = PermissionResolverHelper.GetCanAccessRecordExpression<TModel>(permissionResolver, AccessType.Read);
            if (expression != null)
                query = query.Where(expression);

            return query;
        }

        /// <summary>
        /// Applies permission checks, filtering, sorting and paging to the passed <c>query</c>, based on <c>queryInfo</c>.
        /// </summary>
        /// <param name="query">The query to work on. Must not be null.</param>
        /// <param name="queryInfo">The query info, defining sorting, paging and filtering. Can be null.</param>
        /// <param name="permissionResolver">The permission resolver to use for permission checks. Can be null.</param>
        /// <typeparam name="TDbModel">The record type.</typeparam>
        /// <typeparam name="TModel">The api model type, on which filtering and sorting are defined.</typeparam>
        /// <returns>A built query.</returns>
        public static BuiltQuery<TDbModel> BuildDbQuery<TDbModel, TModel>(this IQueryable<TDbModel> query, QueryInfo queryInfo, IPermissionResolver permissionResolver)
            where TDbModel : class
        {
            var dataQuery = BuildSecuredQuery(query, permissionResolver);
            var pagedQuery = dataQuery;

            if (queryInfo != null)
            {
                dataQuery = BuildFilteredQuery<TDbModel, TModel>(dataQuery, queryInfo.Filters);
                pagedQuery = BuildPagedQuery(BuildSortedQuery<TDbModel, TModel>(dataQuery, queryInfo.SortProperties), queryInfo.Paging);
            }

            return new BuiltQuery<TDbModel>(dataQuery, pagedQuery, queryInfo?.Paging?.PageSize ?? 0);
        }

        /// <summary>
        /// Applies permission checks, filtering, sorting and paging to the passed <c>query</c>, based on <c>queryInfo</c>.
        /// </summary>
        /// <param name="query">The query to work on. Must not be null.</param>
        /// <param name="queryInfo">The query info, defining sorting, paging and filtering. Can be null.</param>
        /// <typeparam name="TDbModel">The record type.</typeparam>
        /// <typeparam name="TModel">The api model type, on which filtering and sorting are defined.</typeparam>
        /// <returns>A built query.</returns>
        public static BuiltQuery<TDbModel> BuildUnsecuredDbQuery<TDbModel, TModel>(this IQueryable<TDbModel> query, QueryInfo queryInfo)
            where TDbModel : class
        {
            var dataQuery = query;
            var pagedQuery = dataQuery;

            if (queryInfo != null)
            {
                dataQuery = BuildFilteredQuery<TDbModel, TModel>(dataQuery, queryInfo.Filters);
                pagedQuery = BuildPagedQuery(BuildSortedQuery<TDbModel, TModel>(dataQuery, queryInfo.SortProperties), queryInfo.Paging);
            }

            return new BuiltQuery<TDbModel>(dataQuery, pagedQuery, queryInfo?.Paging?.PageSize ?? 0);
        }

        /// <summary>
        /// Applies permission checks, filtering, sorting and paging to the passed <c>query</c>, based on <c>queryInfo</c>.
        /// </summary>
        /// <param name="query">The query to work on. Must not be null.</param>
        /// <param name="queryInfo">The query info, defining sorting, paging and filtering. Can be null.</param>
        /// <param name="permissionResolver">The permission resolver to use for permission checks. Can be null.</param>
        /// <typeparam name="TModel">The record type.</typeparam>
        /// <returns>A built query.</returns>
        public static BuiltQuery<TModel> BuildDbQuery<TModel>(this IQueryable<TModel> query, QueryInfo queryInfo, IPermissionResolver permissionResolver)
            where TModel : class
        {
            var dataQuery = BuildSecuredQuery(query, permissionResolver);
            var pagedQuery = dataQuery;

            if (queryInfo != null)
            {
                dataQuery = BuildFilteredQuery(dataQuery, queryInfo.Filters);
                pagedQuery = BuildPagedQuery(BuildSortedQuery(dataQuery, queryInfo.SortProperties), queryInfo.Paging);
            }

            return new BuiltQuery<TModel>(dataQuery, pagedQuery, queryInfo?.Paging?.PageSize ?? 0);
        }

        /// <summary>
        /// Execute the query.
        /// Will fail if called on in-memory-queries.
        /// </summary>
        /// <typeparam name="TModel">The record type.</typeparam>
        /// <param name="query">The query to execute. Must not be null.</param>
        /// <param name="benchmarks">A benchmark section. Can be null.</param>
        /// <returns>A query result.</returns>
        public static async Task<QueryResult<TModel>> ExecuteAsync<TModel>(this BuiltQuery<TModel> query, BenchmarkSection benchmarks, Func<IQueryable<TModel>,Task<TModel[]>> toArrayFunc, Func<IQueryable<TModel>, Task<long>> countFunc)
            where TModel : class
        {
            TModel[] data;
            using (benchmarks.CreateBenchmark("QueryData"))
            {
                data = await toArrayFunc(query.PagedQuery);
            }

            long count;
            using (benchmarks.CreateBenchmark("QueryCount"))
            {
                count = await countFunc(query.Query);
            }

            var pageCount = query.PageSize == 0 ? 0 : (count / query.PageSize) + (((count % query.PageSize) == 0) ? 0 : 1);

            return new QueryResult<TModel>(data, count, pageCount);
        }
        /// <summary>
        /// Execute the query.
        /// </summary>
        /// <typeparam name="TModel">The record type.</typeparam>
        /// <param name="query">The query to execute. Must not be null.</param>
        /// <param name="benchmarks">A benchmark section. Can be null.</param>
        /// <returns>A query result.</returns>
        public static QueryResult<TModel> Execute<TModel>(this BuiltQuery<TModel> query, BenchmarkSection benchmarks = null)
            where TModel : class
        {
            TModel[] data;
            using (benchmarks.CreateBenchmark("QueryData"))
            {
                data = query.PagedQuery.ToArray();
            }

            long count;
            using (benchmarks.CreateBenchmark("QueryCount"))
            {
                count = query.Query.LongCount();
            }

            var pageCount = query.PageSize == 0 ? 0 : (count / query.PageSize) + (((count % query.PageSize) == 0) ? 0 : 1);

            return new QueryResult<TModel>(data, count, pageCount);
        }
    }
}