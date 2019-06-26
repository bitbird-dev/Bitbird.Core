using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Bitbird.Core.Data;
using Bitbird.Core.Data.CliToolAnnotations;
using Bitbird.Core.Data.DbContext;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelReader
    {
        [NotNull] private readonly string modelPostfix;
        [NotNull, ItemNotNull] private readonly Type[] dataModelTypes;

        [UsedImplicitly]
        public DataModelReader([NotNull] string modelPostfix, [NotNull, ItemNotNull] params Type[] dataModelTypes)
        {
            if (dataModelTypes == null)
                throw new ArgumentNullException(nameof(dataModelTypes));
            if (dataModelTypes.Length == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(dataModelTypes));
            if (dataModelTypes.Any(x => x == null))
                throw new ArgumentException("Value cannot be contain null.", nameof(dataModelTypes));

            this.modelPostfix = modelPostfix ?? throw new ArgumentNullException(nameof(modelPostfix));
            this.dataModelTypes = dataModelTypes;
        }

        [NotNull, UsedImplicitly]
        public DataModelsInfo ReadDataModelInfo()
        {
            var dataModelInfos = dataModelTypes
                .Select(ExtractDataModelInfo)
                .ToArray();

            return new DataModelsInfo(dataModelInfos);
        }

        [NotNull]
        private DataModelInfo ExtractDataModelInfo([NotNull] Type dataModelType)
        {
            if (!dataModelType.Name.EndsWith(modelPostfix))
                throw new Exception($"Class {dataModelType.FullName} does not have the postfix {modelPostfix}.");

            var tableAttribute = dataModelType.GetCustomAttribute<TableAttribute>()
                ?? throw new Exception($"Class {dataModelType.FullName} as no {nameof(TableAttribute)} defined.");
            if (tableAttribute.Name == null)
                throw new Exception($"Class {dataModelType.FullName} as a {nameof(TableAttribute)} with {nameof(tableAttribute.Name)} = null.");

            var isIIsDeletedFlagEntity = typeof(IIsDeletedFlagEntity).IsAssignableFrom(dataModelType);
            var isIIsLockedFlagEntity = typeof(IIsLockedFlagEntity).IsAssignableFrom(dataModelType);
            var isIIsActiveFlagEntity = typeof(IIsActiveFlagEntity).IsAssignableFrom(dataModelType);
            var isIOptimisticLockable = typeof(IOptimisticLockable).IsAssignableFrom(dataModelType);
            var idType = dataModelType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IId<>))?.GetGenericArguments()[0];
            var idSetterType = dataModelType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIdSetter<>))?.GetGenericArguments()[0];

            var baseType = dataModelType;
            var isTemporalTableBase = false;
            while (baseType != null)
            {
                if (baseType.Name.Equals("TemporalTableDbModelBase"))
                {
                    isTemporalTableBase = true;
                    break;
                }

                baseType = baseType.BaseType;
            }

            return new DataModelInfo(
                dataModelType,
                dataModelType.Name,
                dataModelType.Name.Substring(0, dataModelType.Name.Length - modelPostfix.Length),
                tableAttribute.Name,
                dataModelType.GetProperties()
                    .Select(ExtractDataModelPropertyInfo)
                    .Where(x => x != null)
                    .ToArray(),
                isIIsDeletedFlagEntity,
                isIIsLockedFlagEntity,
                isIIsActiveFlagEntity,
                isIOptimisticLockable,
                isTemporalTableBase,
                idType,
                idSetterType);
        }

        [CanBeNull]
        private DataModelPropertyInfo ExtractDataModelPropertyInfo([NotNull] PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetCustomAttribute<IgnoreInApiModelAttribute>() != null)
                return null;

            var isNavigationalProperty = propertyInfo.GetGetMethod().IsVirtual && !propertyInfo.GetGetMethod().IsFinal;
            if (isNavigationalProperty)
                return null;

            var type = propertyInfo.PropertyType;
            var isSqlNullable = (!type.IsValueType || Nullable.GetUnderlyingType(type) != null) && type.GetCustomAttribute<RequiredAttribute>() == null;

            var isKey = propertyInfo.GetCustomAttribute<KeyAttribute>() != null;
            var dbGeneratedAttribute = propertyInfo.GetCustomAttribute<DatabaseGeneratedAttribute>();
            var isDbGenerated = dbGeneratedAttribute != null;
            var isIdentity = dbGeneratedAttribute?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
            var isComputed = dbGeneratedAttribute?.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed;

            var foreignKeyNavigationalPropertyInfo = propertyInfo.DeclaringType?.GetProperties().FirstOrDefault(p =>
                p.GetCustomAttribute<ForeignKeyAttribute>()?.Name?.Equals(propertyInfo.Name) ?? false);
            var foreignKeyDataModelClass = foreignKeyNavigationalPropertyInfo?.PropertyType;
            var foreignKeyDataModelClassName = foreignKeyDataModelClass?.Name;
            var foreignKeyDataModelModelName = foreignKeyDataModelClassName?.Substring(0, foreignKeyDataModelClassName.Length - modelPostfix.Length);

            var stringLengthAttribute = propertyInfo.GetCustomAttribute<StringLengthAttribute>();
            var stringInfo = type != typeof(string)
                ? null
                : new DataModelPropertyStringInfo(stringLengthAttribute?.MaximumLength);

            var persistInApiModel = !(typeof(IOptimisticLockable).IsAssignableFrom(propertyInfo.DeclaringType) &&
                propertyInfo.Name.Equals(nameof(IOptimisticLockable.SysStartTime)));

            var nullableGenericUnderlyingType = Nullable.GetUnderlyingType(type);
            var isNullableGeneric = nullableGenericUnderlyingType != null;
            var canTypeBeAssignedNull = type.IsClass || isNullableGeneric;

            var isNotMapped = propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null;

            return new DataModelPropertyInfo(
                propertyInfo.Name,
                type,
                isSqlNullable,
                isNullableGeneric,
                nullableGenericUnderlyingType,
                canTypeBeAssignedNull,
                isKey,
                isDbGenerated,
                isIdentity,
                isComputed,
                isNotMapped,
                persistInApiModel,
                foreignKeyDataModelClass,
                foreignKeyDataModelClassName,
                foreignKeyDataModelModelName,
                stringInfo);
        }
    }
}
