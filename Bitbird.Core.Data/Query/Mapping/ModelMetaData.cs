using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Query.Mapping
{
    /// <summary>
    /// Is stored for each found api model type that defines at least one property that has a <see cref="DbMappingExpressionAttribute"/>.
    /// Can be accessed using the <see cref="ModelsMetaData"/> singleton.
    /// </summary>
    internal class ModelMetaData
    {
        /// <summary>
        /// The api model type.
        /// Is not null.
        /// </summary>
        [NotNull]
        public readonly Type ModelType;
        /// <summary>
        /// For each db model type that the given api model type defines a mapping to, this dictionary stores detailed information.
        /// For more information see <see cref="ModelMetaDataToDbMapping"/>.
        /// Does not contain null entries.
        /// Is not null.
        /// </summary>
        [NotNull]
        internal readonly Dictionary<Type, ModelMetaDataToDbMapping> ByDbModelType;

        /// <summary>
        /// Constructs a <see cref="ModelMetaData"/> object.
        /// </summary>
        /// <param name="modelType">See <see cref="ModelType"/>.</param>
        /// <param name="byDbModelType">See <see cref="ByDbModelType"/>.</param>
        internal ModelMetaData([NotNull] Type modelType, [NotNull] Dictionary<Type, ModelMetaDataToDbMapping> byDbModelType)
        {
            if (byDbModelType == null)
                throw new ArgumentNullException(nameof(byDbModelType), $@"{nameof(ModelMetaData)}: The passed dictionary was null.");
            if (byDbModelType.Keys.Any(k => k == null))
                throw new ArgumentNullException(nameof(byDbModelType), $@"{nameof(ModelMetaData)}: The passed dictionary contains null keys.");
            if (byDbModelType.Values.Any(v => v == null))
                throw new ArgumentNullException(nameof(byDbModelType), $@"{nameof(ModelMetaData)}: The passed dictionary contains null values.");

            ModelType = modelType ?? throw new ArgumentNullException(nameof(modelType), $@"{nameof(ModelMetaData)}: The passed model type was null.");
            ByDbModelType = byDbModelType;
        }
    }
}