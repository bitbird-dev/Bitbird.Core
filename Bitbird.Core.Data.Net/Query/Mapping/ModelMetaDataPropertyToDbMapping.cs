using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Net.Query.Mapping
{
    /// <summary>
    /// Is stored for each property in a db model type that a given api model type defines a mapping to.
    /// Can be accessed using the <see cref="ModelsMetaData"/> singleton.
    /// </summary>
    internal class ModelMetaDataPropertyToDbMapping
    {
        /// <summary>
        /// The property name in the api model that can be mapped.
        /// Is not null.
        /// </summary>
        [NotNull]
        internal readonly string PropertyName;
        /// <summary>
        /// The type for the property int the api model.
        /// Is not null.
        /// </summary>
        [NotNull]
        internal readonly Type PropertyType;
        /// <summary>
        /// The expression that defines the mapping.
        /// For detailed information, see <see cref="DbMappingExpressionAttribute.NameDbMappingProperty"/>.
        /// Is not null.
        /// </summary>
        [NotNull]
        internal readonly LambdaExpression Expression;
        /// <summary>
        /// Whether this is the default sorting expression or not (see <see cref="DbMappingExpressionAttribute.IsDefaultSort"/>).
        /// </summary>
        internal readonly bool IsDefaultSort;

        /// <summary>
        /// Constructs a <see cref="ModelMetaDataPropertyToDbMapping"/>.
        /// </summary>
        /// <param name="propertyName">See <see cref="PropertyName"/>.</param>
        /// <param name="propertyType">See <see cref="PropertyType"/>.</param>
        /// <param name="expression">See <see cref="Expression"/>.</param>
        /// <param name="isDefaultSort">See <see cref="IsDefaultSort"/>.</param>
        internal ModelMetaDataPropertyToDbMapping([NotNull] string propertyName, [NotNull] Type propertyType, [NotNull] LambdaExpression expression, bool isDefaultSort)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName), $@"{nameof(ModelMetaDataToDbMapping)}: The passed property name was null.");
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType), $@"{nameof(ModelMetaDataToDbMapping)}: The passed property type was null.");
            Expression = expression ?? throw new ArgumentNullException(nameof(expression), $@"{nameof(ModelMetaDataToDbMapping)}: The passed expression was null.");
            IsDefaultSort = isDefaultSort;
        }
    }
}