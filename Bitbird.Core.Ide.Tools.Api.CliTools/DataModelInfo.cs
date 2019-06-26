using System;
using System.Collections.Generic;
using System.Linq;
using Bitbird.Core.Types;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelInfo
    {
        [NotNull, UsedImplicitly] public readonly Type ModelType;
        [NotNull, UsedImplicitly] public readonly string Name;
        [NotNull, UsedImplicitly] public readonly string TableName;
        [NotNull, UsedImplicitly] public readonly string ModelName;
        [NotNull, ItemNotNull, UsedImplicitly] public readonly DataModelPropertyInfo[] Properties;
        [UsedImplicitly] public readonly bool IsIIsDeletedFlagEntity;
        [UsedImplicitly] public readonly bool IsIIsLockedFlagEntity;
        [UsedImplicitly] public readonly bool IsIIsActiveFlagEntity;
        [UsedImplicitly] public readonly bool IsIOptimisticLockable;
        [UsedImplicitly] public readonly bool IsTemporalTableBase;
        [CanBeNull, UsedImplicitly] public readonly Type IdType;
        [CanBeNull, UsedImplicitly] public readonly Type IdSetterType;

        [UsedImplicitly] public bool IsIId => IdType != null;
        [UsedImplicitly] public bool IsIIdSetter => IdSetterType != null;

        public string IdTypeAsCsType => IdType?.ToCsType();
        public string IdSetterTypeAsCsType => IdSetterType?.ToCsType();


        [NotNull]
        public IEnumerable<DataModelPropertyInfo> PropertiesToPersistInApiModel => Properties.Where(x => x.PersistInApiModel);
        [NotNull]
        public IEnumerable<DataModelPropertyInfo> PropertiesToPersistInApiModelThatAreMapped => PropertiesToPersistInApiModel.Where(x => !x.IsNotMapped);

        public bool HasNoPersistedOptimisticLockingTokenInBaseClass => IsIOptimisticLockable && !IsTemporalTableBase;

        internal DataModelInfo([NotNull] Type modelType,
            [NotNull] string name,
            [NotNull] string tableName,
            [NotNull] string modelName,
            [NotNull] [ItemNotNull] DataModelPropertyInfo[] properties,
            bool isIIsDeletedFlagEntity,
            bool isIIsLockedFlagEntity,
            bool isIIsActiveFlagEntity,
            bool isIOptimisticLockable,
            bool isTemporalTableBase,
            [CanBeNull] Type idType,
            [CanBeNull] Type idSetterType)
        {
            ModelType = modelType;
            Name = name;
            TableName = tableName;
            ModelName = modelName;
            Properties = properties;
            IsIIsDeletedFlagEntity = isIIsDeletedFlagEntity;
            IsIIsLockedFlagEntity = isIIsLockedFlagEntity;
            IsIIsActiveFlagEntity = isIIsActiveFlagEntity;
            IsIOptimisticLockable = isIOptimisticLockable;
            IdType = idType;
            IdSetterType = idSetterType;
        }
    }
}