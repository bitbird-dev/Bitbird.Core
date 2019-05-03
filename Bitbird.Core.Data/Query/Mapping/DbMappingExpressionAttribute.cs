using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Query.Mapping
{
    /// <summary>
    /// For an api model, for a property this attribute specifies a mapping from the property to an expression of the underlying model.
    /// 
    /// This can be used to expose api models to the user/client, allowing them to use sorting/filtering expressions based on the api model,
    /// then translating these expressions to expressions of the db model that can be used to build SQL statements.
    /// </summary>
    public class DbMappingExpressionAttribute : Attribute
    {
        /// <summary>
        /// The name of the static property in the class that hosts the property this attribute is assigned to,
        /// that returns a <c>Expression&lt;Func%ltTModel,TResult&gt;&gt;</c> where <c>TModel</c> is the type stored in <see cref="DbModelType"/>.
        /// <c>TResult</c> should be a type that is associated with the type that is returned by the property that this attribute is assigned to.
        /// For primitive types, this means, it should return the same type.
        /// For relations, this means, that it should return a type (e.g. a db model type) that can be mapped to the property type (which will most likely be an api model type).
        ///
        /// The expression must translate to SQL.
        ///
        /// Neither this property, nor the property this attribute is pointing to can return null.
        /// </summary>
        [NotNull]
        public string NameDbMappingProperty { get; }

        /// <summary>
        /// The model type that will be mapped to.
        /// Will most likely be a db model.
        /// Must not be null.
        /// </summary>
        [NotNull]
        public Type DbModelType { get; }

        /// <summary>
        /// Whether the property is used to sort result sets of the class that hosts the property this attribute is assigned to, if no sorting is specified.
        /// For one db model, only one property in the class can be assigned an attribute with default sorting set to true.
        /// </summary>
        public bool IsDefaultSort { get; }

        /// <summary>
        /// Constructs a <see cref="DbMappingExpressionAttribute"/> object.
        /// For detailed information, see the class documentation of <see cref="DbMappingExpressionAttribute"/>, and the corresponding member documentation.
        /// </summary>
        /// <param name="dbModelType">See <see cref="DbModelType"/>.</param>
        /// <param name="nameDbMappingName">See <see cref="NameDbMappingProperty"/>.</param>
        /// <param name="isDefaultSort">See <see cref="IsDefaultSort"/>.</param>
        public DbMappingExpressionAttribute([NotNull] Type dbModelType, [NotNull] string nameDbMappingName, bool isDefaultSort = false)
        {
            DbModelType = dbModelType ?? throw new ArgumentNullException(nameof(dbModelType), $@"{nameof(DbMappingExpressionAttribute)}: The passed db model type is null.");
            NameDbMappingProperty = nameDbMappingName ?? throw new ArgumentNullException(nameof(dbModelType), $@"{nameof(DbMappingExpressionAttribute)}: The passed name of the db-mapping-property is null.");
            IsDefaultSort = isDefaultSort;
        }
    }
}