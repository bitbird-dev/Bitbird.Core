using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools.Net
{
    [UsedImplicitly]
    public sealed class DataModelPropertyInfo
    {
        [NotNull, UsedImplicitly] public readonly string Name;
        [NotNull, UsedImplicitly] public readonly Type Type;
        [UsedImplicitly] public readonly bool IsNullable;
        [UsedImplicitly] public readonly bool IsKey;
        [UsedImplicitly] public readonly bool IsDbGenerated;
        [UsedImplicitly] public readonly bool IsIdentity;
        [UsedImplicitly] public readonly bool IsComputed;
        [CanBeNull, UsedImplicitly] public readonly Type ForeignKeyDataModelClass;
        [CanBeNull, UsedImplicitly] public readonly DataModelPropertyStringInfo StringInfo;

        [UsedImplicitly] public bool IsForeignKey => ForeignKeyDataModelClass != null;
        [UsedImplicitly] public bool IsString => StringInfo != null;

        internal DataModelPropertyInfo(
            [NotNull] string name, 
            [NotNull] Type type, 
            bool isNullable, 
            bool isKey, 
            bool isDbGenerated, 
            bool isIdentity, 
            bool isComputed,
            [CanBeNull] Type foreignKeyDataModelClass, 
            [CanBeNull] DataModelPropertyStringInfo stringInfo)
        {
            Name = name;
            Type = type;
            IsNullable = isNullable;
            IsKey = isKey;
            IsDbGenerated = isDbGenerated;
            IsIdentity = isIdentity;
            IsComputed = isComputed;
            ForeignKeyDataModelClass = foreignKeyDataModelClass;
            StringInfo = stringInfo;
        }
    }
}