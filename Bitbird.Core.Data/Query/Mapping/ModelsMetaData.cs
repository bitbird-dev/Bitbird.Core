using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Query.Mapping
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
        internal readonly Dictionary<Type, ModelMetaData> ByModelType = new Dictionary<Type, ModelMetaData>();


        /// <summary>
        /// Baking field for the singleton instance.
        /// </summary>
        [NotNull]
        private readonly static ModelsMetaData instance = new ModelsMetaData();

        /// <summary>
        /// The singleton instance.
        /// </summary>
        [NotNull]
        public static ModelsMetaData Instance => instance;


        /// <summary>
        /// Adds types to the singleton <see cref="Instance"/>.
        /// </summary>
        public void RegisterModelsFromAssembly([ItemNotNull] [NotNull] params Assembly[] assemblies)
        {
            if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));
            if (assemblies.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(assemblies));

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

                lock (this)
                {
                    foreach (var dbMapping in dbMappings)
                    {
                        if (ByModelType.ContainsKey(dbMapping.Key))
                            throw new Exception($"The type {dbMapping.Key.FullName} was already added.");

                        ByModelType.Add(dbMapping.Key, dbMapping.Value);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"The data model is not annotated correctly. An error occurred when creating a meta-data structure. (Details: {e})");
            }
        }
    }
}