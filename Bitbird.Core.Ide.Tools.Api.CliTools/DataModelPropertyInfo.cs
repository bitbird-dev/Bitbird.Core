using System;
using Bitbird.Core.Types;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelPropertyInfo
    {
        [UsedImplicitly, NotNull] public readonly string Name;
        [UsedImplicitly, NotNull] public readonly Type Type;
        [UsedImplicitly] public readonly bool IsSqlNullable;
        [UsedImplicitly] public readonly bool IsNullableGeneric;
        [UsedImplicitly, CanBeNull] public readonly Type NullableGenericUnderlyingType;
        [UsedImplicitly] public readonly bool CanTypeBeAssignedNull;
        [UsedImplicitly] public readonly bool IsKey;
        [UsedImplicitly] public readonly bool IsDbGenerated;
        [UsedImplicitly] public readonly bool IsIdentity;
        [UsedImplicitly] public readonly bool IsComputed;
        [UsedImplicitly] public readonly bool IsNotMapped;
        [UsedImplicitly] public readonly bool PersistInApiModel;
        [UsedImplicitly, CanBeNull] public readonly Type ForeignKeyDataModelClass;
        [UsedImplicitly, CanBeNull] public readonly string ForeignKeyDataModelClassName;
        [UsedImplicitly, CanBeNull] public readonly string ForeignKeyDataModelModelName;
        [UsedImplicitly, CanBeNull] public readonly DataModelPropertyStringInfo StringInfo;

        [UsedImplicitly] public bool IsForeignKey => ForeignKeyDataModelClass != null;
        [UsedImplicitly] public bool IsString => StringInfo != null;

        public string ResharperCanBeNullAttributeCode => CanTypeBeAssignedNull ? "[CanBeNull] " : string.Empty;
        public string TypeAsCsType => Type.ToCsType();
        public string NameAsCamelCase => Name.ToCamelCase();
        public string ForeignKeyDataModelClassAsCsType => ForeignKeyDataModelClass?.ToCsType();

        public string ForeignKeyNavigationalPropertyName 
        {
            get
            {
                if (!IsForeignKey)
                    return null;

                const string postfix = "Id";

                if (!Name.EndsWith(postfix))
                    throw new Exception($"{nameof(ForeignKeyNavigationalPropertyName)} was accessed for a property that does not end with 'Id': {Name}");

                return Name.Substring(0, Name.Length - postfix.Length);
            }
        }

        internal DataModelPropertyInfo(
            [NotNull] string name, 
            [NotNull] Type type, 
            bool isSqlNullable,
            bool isNullableGeneric,
            [CanBeNull] Type nullableGenericUnderlyingType,
            bool canTypeBeAssignedNull,
            bool isKey, 
            bool isDbGenerated, 
            bool isIdentity, 
            bool isComputed,
            bool isNotMapped,
            bool persistInApiModel,
            [CanBeNull] Type foreignKeyDataModelClass,
            [CanBeNull] string foreignKeyDataModelClassName,
            [CanBeNull] string foreignKeyDataModelModelName,
            [CanBeNull] DataModelPropertyStringInfo stringInfo)
        {
            Name = name;
            Type = type;
            IsSqlNullable = isSqlNullable;
            IsNullableGeneric = isNullableGeneric;
            NullableGenericUnderlyingType = nullableGenericUnderlyingType;
            CanTypeBeAssignedNull = canTypeBeAssignedNull;
            IsKey = isKey;
            IsDbGenerated = isDbGenerated;
            IsIdentity = isIdentity;
            IsComputed = isComputed;
            IsNotMapped = isNotMapped;
            PersistInApiModel = persistInApiModel;
            ForeignKeyDataModelClass = foreignKeyDataModelClass;
            ForeignKeyDataModelClassName = foreignKeyDataModelClassName;
            ForeignKeyDataModelModelName = foreignKeyDataModelModelName;
            StringInfo = stringInfo;
        }
    }
}