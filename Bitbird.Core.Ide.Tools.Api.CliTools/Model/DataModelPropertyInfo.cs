using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class DataModelPropertyInfo
    {
        [UsedImplicitly, NotNull] public string Name { get; }
        [UsedImplicitly, NotNull] public string TypeAsCsType { get; }
        [UsedImplicitly] public bool IsSqlNullable { get; }
        [UsedImplicitly] public bool IsNullableGeneric { get; }
        [UsedImplicitly, CanBeNull] public string NullableGenericUnderlyingTypeAsCsType { get; }
        [UsedImplicitly] public bool CanTypeBeAssignedNull { get; }
        [UsedImplicitly] public bool IsEnum { get; }
        [UsedImplicitly] public bool IsKey { get; }
        [UsedImplicitly] public bool IsDbGenerated { get; }
        [UsedImplicitly] public bool IsIdentity { get; }
        [UsedImplicitly] public bool IsComputed { get; }
        [UsedImplicitly] public bool IsNotMapped { get; }
        [UsedImplicitly] public bool PersistInApiModel { get; }
        [UsedImplicitly, CanBeNull] public string ForeignKeyDataModelClassAsCsType { get; }
        [UsedImplicitly, CanBeNull] public string ForeignKeyDataModelClassName { get; }
        [UsedImplicitly, CanBeNull] public string ForeignKeyDataModelModelName { get; }
        [UsedImplicitly, CanBeNull] public DataModelPropertyStringInfo StringInfo { get; }
        [UsedImplicitly, NotNull] public Attribute[] Attributes { get; }

        [UsedImplicitly] public bool IsForeignKey => ForeignKeyDataModelClassAsCsType != null;
        [UsedImplicitly] public bool IsString => StringInfo != null;

        [NotNull, JsonIgnore]
        public string ResharperCanBeNullAttributeCode => CanTypeBeAssignedNull ? "[CanBeNull] " : string.Empty;
        [NotNull, JsonIgnore]
        public string NameAsCamelCase => Name.ToCamelCase();
        [CanBeNull, JsonIgnore]
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

        [JsonConstructor]
        public DataModelPropertyInfo(
            [NotNull] string name, 
            [NotNull] string typeAsCsType, 
            bool isSqlNullable,
            bool isNullableGeneric,
            [CanBeNull] string nullableGenericUnderlyingTypeAsCsType,
            bool isEnum,
            bool canTypeBeAssignedNull,
            bool isKey, 
            bool isDbGenerated, 
            bool isIdentity, 
            bool isComputed,
            bool isNotMapped,
            bool persistInApiModel,
            [CanBeNull] string foreignKeyDataModelClassAsCsType,
            [CanBeNull] string foreignKeyDataModelClassName,
            [CanBeNull] string foreignKeyDataModelModelName,
            [CanBeNull] DataModelPropertyStringInfo stringInfo,
            [NotNull, ItemNotNull] Attribute[] attributes)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            TypeAsCsType = typeAsCsType ?? throw new ArgumentNullException(nameof(typeAsCsType));
            IsSqlNullable = isSqlNullable;
            IsNullableGeneric = isNullableGeneric;
            NullableGenericUnderlyingTypeAsCsType = nullableGenericUnderlyingTypeAsCsType;
            CanTypeBeAssignedNull = canTypeBeAssignedNull;
            IsEnum = isEnum;
            IsKey = isKey;
            IsDbGenerated = isDbGenerated;
            IsIdentity = isIdentity;
            IsComputed = isComputed;
            IsNotMapped = isNotMapped;
            PersistInApiModel = persistInApiModel;
            ForeignKeyDataModelClassAsCsType = foreignKeyDataModelClassAsCsType;
            ForeignKeyDataModelClassName = foreignKeyDataModelClassName;
            ForeignKeyDataModelModelName = foreignKeyDataModelModelName;
            StringInfo = stringInfo;
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
        }

        public bool TryGetAttribute<T>(out T attribute) where T : Attribute
        {
            attribute = Attributes.OfType<T>().FirstOrDefault();
            return attribute != null;
        }

        public bool TryGetAttribute(string typeName, out dynamic attribute)
        {
            var attr = Attributes.FirstOrDefault(a => a.GetType().Name.Equals(typeName));
            attribute = attr;
            return attr != null;
        }
    }
}