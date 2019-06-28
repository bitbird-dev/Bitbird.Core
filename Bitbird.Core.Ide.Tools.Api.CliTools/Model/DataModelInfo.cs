using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelInfo
    {
        [NotNull, UsedImplicitly] public string Name { get; }
        [NotNull, UsedImplicitly] public string TableName { get; }
        [NotNull, UsedImplicitly] public string ModelName { get; }
        [NotNull, ItemNotNull, UsedImplicitly] public DataModelPropertyInfo[] Properties { get; }
        [UsedImplicitly] public bool IsIIsDeletedFlagEntity { get; }
        [UsedImplicitly] public bool IsIIsLockedFlagEntity { get; }
        [UsedImplicitly] public bool IsIIsActiveFlagEntity { get; }
        [UsedImplicitly] public bool IsIOptimisticLockable { get; }
        [UsedImplicitly] public bool IsTemporalTableBase { get; }
        [CanBeNull, UsedImplicitly] public string IdTypeAsCsType { get; }
        [CanBeNull, UsedImplicitly] public string IdSetterTypeAsCsType { get; }
        [CanBeNull, UsedImplicitly] public string ImportKeyAsCsType { get; }
        [CanBeNull, UsedImplicitly] public string ImportKeyUnderlyingTypeAsCsType { get; }

        [UsedImplicitly, JsonIgnore]
        public bool IsIId => IdTypeAsCsType != null;
        [UsedImplicitly, JsonIgnore]
        public bool IsIIdSetter => IdSetterTypeAsCsType != null;
        [UsedImplicitly, JsonIgnore]
        public bool HasImportKey => ImportKeyAsCsType != null;
        [UsedImplicitly, JsonIgnore]
        public bool IsImportKeyNullable => ImportKeyAsCsType != null && !ImportKeyAsCsType.Equals(ImportKeyUnderlyingTypeAsCsType);
        [NotNull, JsonIgnore]
        public IEnumerable<DataModelPropertyInfo> PropertiesToPersistInApiModel => Properties.Where(x => x.PersistInApiModel);
        [NotNull, JsonIgnore]
        public IEnumerable<DataModelPropertyInfo> PropertiesToPersistInApiModelThatAreMapped => PropertiesToPersistInApiModel.Where(x => !x.IsNotMapped);
        [JsonIgnore]
        public bool HasNoPersistedOptimisticLockingTokenInBaseClass => IsIOptimisticLockable && !IsTemporalTableBase;

        [JsonConstructor]
        public DataModelInfo([NotNull] string name,
            [NotNull] string tableName,
            [NotNull] string modelName,
            [NotNull] [ItemNotNull] DataModelPropertyInfo[] properties,
            bool isIIsDeletedFlagEntity,
            bool isIIsLockedFlagEntity,
            bool isIIsActiveFlagEntity,
            bool isIOptimisticLockable,
            bool isTemporalTableBase,
            [CanBeNull] string idTypeAsCsType,
            [CanBeNull] string idSetterTypeAsCsType, 
            [CanBeNull] string importKeyAsCsType,
            [CanBeNull] string importKeyUnderlyingTypeAsCsType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            ModelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
            if (properties.Any(p => p == null)) throw new ArgumentNullException(nameof(properties));

            IsIIsDeletedFlagEntity = isIIsDeletedFlagEntity;
            IsIIsLockedFlagEntity = isIIsLockedFlagEntity;
            IsIIsActiveFlagEntity = isIIsActiveFlagEntity;
            IsIOptimisticLockable = isIOptimisticLockable;
            IsTemporalTableBase = isTemporalTableBase;
            IdTypeAsCsType = idTypeAsCsType;
            IdSetterTypeAsCsType = idSetterTypeAsCsType;
            ImportKeyAsCsType = importKeyAsCsType;
            ImportKeyUnderlyingTypeAsCsType = importKeyUnderlyingTypeAsCsType;
        }
    }
}