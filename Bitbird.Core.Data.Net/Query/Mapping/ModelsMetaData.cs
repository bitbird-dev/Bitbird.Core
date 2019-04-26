using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Net.Query.Mapping
{
    /// <summary>
    /// Is a singleton that loads meta data of the mapping of api models to db models at the startup.
    /// </summary>
    public class ModelsMetaData
    {
        /// <summary>
        /// For each found api model type that defines at least one property that has a <see cref="DbMappingExpressionAttribute"/>, this dictionary stores an entry detailed information.
        /// For more information, see <see cref="ModelMetaData"/>.
        /// Does not contain null entries.
        /// Is not null.
        /// </summary>
        [NotNull]
        internal readonly Dictionary<Type, ModelMetaData> ByModelType;

        /// <summary>
        /// Constructs a <see cref="ModelsMetaData"/> object.
        /// </summary>
        /// <param name="byModelType">See <see cref="ByModelType"/>. Cannot be null.</param>
        private ModelsMetaData([NotNull] Dictionary<Type, ModelMetaData> byModelType)
        {
            if (byModelType == null)
                throw new ArgumentNullException(nameof(byModelType), $@"{nameof(ModelsMetaData)}: The passed dictionary was null.");
            if (byModelType.Keys.Any(k => k == null))
                throw new ArgumentNullException(nameof(byModelType), $@"{nameof(ModelsMetaData)}: The passed dictionary contains null keys.");
            if (byModelType.Values.Any(v => v == null))
                throw new ArgumentNullException(nameof(byModelType), $@"{nameof(ModelsMetaData)}: The passed dictionary contains null values.");

            ByModelType = byModelType;
        }

        /// <summary>
        /// The singleton instance.
        /// Must be initialized manually.
        /// </summary>
        [NotNull]
        internal static ModelsMetaData Instance;

        /// <summary>
        /// Static constructor.
        /// Initializes the singleton <see cref="Instance"/>.
        /// </summary>
        public static void Init(params Assembly[] assemblies)
        {
            try
            {
                var dbMappings = assemblies
                    .SelectMany(a => a.ExportedTypes)
                    .Select(type => new
                    {
                        Type = type,
                        Properties = type
                            .GetProperties()
                            .SelectMany(p => p.GetCustomAttributes<DbMappingExpressionAttribute>()
                                .Select(att => new
                                {
                                    Property = p,
                                    DbMappingExpressionAttribute = att
                                }))
                            .Select(att =>
                            {
                                var property = type.GetProperty(att.DbMappingExpressionAttribute.NameDbMappingProperty)
                                               ?? throw new Exception($"Could not find static property {type.Name}.{att.DbMappingExpressionAttribute.NameDbMappingProperty}.");
                                var value = property.GetValue(null) 
                                            ?? throw new Exception($"Could not read value from property {type.Name}.{att.DbMappingExpressionAttribute.NameDbMappingProperty}.");
                                if (!(value is LambdaExpression lambdaExpression))
                                    throw new Exception($"Read value from property {type.Name}.{att.DbMappingExpressionAttribute.NameDbMappingProperty} is not of type LambdaExpression.");

                                return new
                                {
                                    att.DbMappingExpressionAttribute.DbModelType, 
                                    PropertyName = att.Property.Name,
                                    att.Property.PropertyType, 
                                    LambdaExpression = lambdaExpression,
                                    att.DbMappingExpressionAttribute.IsDefaultSort
                                };
                            })
                            .ToArray()
                    })
                    .Where(type => type.Properties.Any())
                    .ToDictionary(type => type.Type,
                        type => new ModelMetaData(type.Type, 
                            type.Properties
                                .GroupBy(p => p.DbModelType)
                                .ToDictionary(p => p.Key,
                                    p => new ModelMetaDataToDbMapping(p.Key, p.GroupBy(p1 => p1.PropertyName.ToUpperInvariant())
                                        .ToDictionary(p1 => p1.Key,
                                            p1 => p1.Select(p2 => new ModelMetaDataPropertyToDbMapping(p2.PropertyName, p2.PropertyType, p2.LambdaExpression, p2.IsDefaultSort)).Single())))));

                Instance = new ModelsMetaData(dbMappings);
            }
            catch (Exception e)
            {
                throw new Exception($"The data model is not annotated correctly. An error occurred when creating a meta-data structure. (Details: {e})");
            }
        }
    }
}