using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitbird.Core.Data.Query.Mapping
{
    /// <summary>
    /// Is stored for each db model type that a given api model type defines a mapping to.
    /// Can be accessed using the <see cref="ModelsMetaData"/> singleton.
    /// </summary>
    internal class ModelMetaDataToDbMapping
    {
        /// <summary>
        /// The db model type.
        /// Cannot be null.
        /// </summary>
        internal readonly Type DbModelType;
        /// <summary>
        /// For each property in a db model type that a given api model type defines a mapping to, this dictionary stores detailed information.
        /// For more information see <see cref="ModelMetaDataPropertyToDbMapping"/>.
        /// Is not null.
        /// Does not contain null entries.
        /// Contains exactly one entry where <see cref="ModelMetaDataPropertyToDbMapping.IsDefaultSort"/> is set to true.
        /// </summary>
        internal readonly Dictionary<string, ModelMetaDataPropertyToDbMapping> ByProperty;
        /// <summary>
        /// Contains the entry of <see cref="ByProperty"/> where <see cref="ModelMetaDataPropertyToDbMapping.IsDefaultSort"/> is set to true.
        /// Is not null.
        /// </summary>
        internal readonly ModelMetaDataPropertyToDbMapping DefaultSortProperty;

        /// <summary>
        /// Constructs a <see cref="ModelMetaDataToDbMapping"/> object.
        /// </summary>
        /// <param name="dbModelType">See <see cref="DbModelType"/>.</param>
        /// <param name="byProperty">See <see cref="ByProperty"/>. Must contain exactly one entry with <see cref="ModelMetaDataPropertyToDbMapping.IsDefaultSort"/> set to true.</param>
        internal ModelMetaDataToDbMapping(Type dbModelType, Dictionary<string, ModelMetaDataPropertyToDbMapping> byProperty)
        {
            if (byProperty == null)
                throw new ArgumentNullException(nameof(byProperty), $@"{nameof(ModelMetaDataToDbMapping)}: The passed dictionary was null.");
            if (byProperty.Keys.Any(k => k == null))
                throw new ArgumentNullException(nameof(byProperty), $@"{nameof(ModelMetaDataToDbMapping)}: The passed dictionary contains null keys.");
            if (byProperty.Values.Any(v => v == null))
                throw new ArgumentNullException(nameof(byProperty), $@"{nameof(ModelMetaDataToDbMapping)}: The passed dictionary contains null values.");

            if (byProperty.Any(m => m.Value.Expression.Parameters.Single().Type != dbModelType))
                throw new ArgumentNullException(nameof(byProperty), 
                    $@"{nameof(ModelMetaDataToDbMapping)}: A mapping to the db model {dbModelType.Name} is defined that where the mapping expression does not take a {dbModelType.Name} instance as parameter, but rather a {byProperty.First(m => m.Value.Expression.Parameters.Single().Type != dbModelType).Value.Expression.Parameters.Single().Type.Name}.");

            DbModelType = dbModelType ?? throw new ArgumentNullException(nameof(dbModelType), $@"{nameof(ModelMetaDataToDbMapping)}: The passed model type was null.");
            ByProperty = byProperty;
            try
            {
                DefaultSortProperty = byProperty.Values.Single(p => p.IsDefaultSort);
            }
            catch (Exception e)
            {
                throw new Exception($"{nameof(ModelMetaDataToDbMapping)}: Each model type has to have exactly one default sort property (defined by {nameof(DbMappingExpressionAttribute)}). (Details: DbType={dbModelType.Name}, found default sorting columns={string.Join(",",byProperty.Values.Select(v => v.PropertyName))}), Error:{e}.");
            }
        }
    }
}