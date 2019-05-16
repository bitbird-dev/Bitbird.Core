using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools.Net
{
    [UsedImplicitly]
    public sealed class DataModelInfo
    {
        [NotNull, UsedImplicitly] public readonly Type ModelType;
        [NotNull, UsedImplicitly] public readonly string Name;
        [NotNull, UsedImplicitly] public readonly string TableName;
        [NotNull, ItemNotNull, UsedImplicitly] public readonly DataModelPropertyInfo[] Properties;
        [UsedImplicitly] public readonly bool IsIIsDeletedFlagEntity;
        [UsedImplicitly] public readonly bool IsIIsLockedFlagEntity;
        [UsedImplicitly] public readonly bool IsIIsActiveFlagEntity;
        [UsedImplicitly] public readonly bool IsIOptimisticLockable;
        [CanBeNull, UsedImplicitly] public readonly Type IdType;
        [CanBeNull, UsedImplicitly] public readonly Type IdSetterType;

        [UsedImplicitly] public bool IsIId => IdType != null;
        [UsedImplicitly] public bool IsIIdSetter => IdSetterType != null;

        internal DataModelInfo([NotNull] Type modelType,
            [NotNull] string name,
            [NotNull] string tableName,
            [NotNull] [ItemNotNull] DataModelPropertyInfo[] properties,
            bool isIIsDeletedFlagEntity,
            bool isIIsLockedFlagEntity,
            bool isIIsActiveFlagEntity,
            bool isIOptimisticLockable,
            [CanBeNull] Type idType,
            [CanBeNull] Type idSetterType)
        {
            ModelType = modelType;
            Name = name;
            TableName = tableName;
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