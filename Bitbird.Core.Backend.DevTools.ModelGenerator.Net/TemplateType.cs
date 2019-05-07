namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    /// <summary>
    /// A type of a template.
    /// </summary>
    public enum TemplateType
    {
        /// <summary>
        /// A template for the interface version filename.
        /// </summary>
        InterfaceVersionFilename,

        /// <summary>
        /// The supported server interface version.
        /// Results in a file
        /// </summary>
        InterfaceVersion,

        /// <summary>
        /// A template for the enums class filename.
        /// </summary>
        EnumsClassFilename,

        /// <summary>
        /// A value in an enum in a combined enums file.
        /// Embedded in <see cref="EnumsEnumClass"/>.
        /// </summary>
        EnumsEnumClassValue,

        /// <summary>
        /// An enum in a combined enums file.
        /// Embedded in <see cref="EnumsClass"/>.
        /// </summary>
        EnumsEnumClass,

        /// <summary>
        /// All enums.
        /// Results in a file.
        /// </summary>
        EnumsClass,

        /// <summary>
        /// A template for the enum class filename.
        /// </summary>
        EnumClassFilename,

        /// <summary>
        /// A value in an enum.
        /// Embedded in <see cref="EnumClass"/>.
        /// </summary>
        EnumClassValue,

        /// <summary>
        /// A translation for a language.
        /// Results in a file.
        /// </summary>
        EnumsTranslationLanguage,

        /// <summary>
        /// An enum translation for a language.
        /// Results in a file.
        /// </summary>
        EnumsTranslationLanguageEnum,

        /// <summary>
        /// A template for the enum translation filename for a language.
        /// </summary>
        EnumsTranslationLanguageFilename,

        /// <summary>
        /// A value in an enum translation for a language.
        /// Embedded in <see cref="EnumClass"/>.
        /// </summary>
        EnumsTranslationLanguageValue,

        /// <summary>
        /// An enum.
        /// Results in a file.
        /// </summary>
        EnumClass,

        /// <summary>
        /// A template for the proxies class filename.
        /// </summary>
        ProxiesClassFilename,

        /// <summary>
        /// A proxy in a combined proxies file.
        /// Embedded in <see cref="ProxiesClass"/>.
        /// </summary>
        ProxiesClassProxy,

        /// <summary>
        /// All proxies.
        /// Results in a file.
        /// </summary>
        ProxiesClass,

        /// <summary>
        /// A template for the proxy class filename.
        /// </summary>
        ProxyClassFilename,

        /// <summary>
        /// An action in a proxy class.
        /// Embedded in <see cref="ProxyClass"/>.
        /// </summary>
        ProxyClassAction,

        /// <summary>
        /// A parameter of an action in a proxy class.
        /// Embedded in <see cref="ProxyClass"/>.
        /// </summary>
        ProxyClassActionParameter,

        /// <summary>
        /// A proxy.
        /// Results in a file.
        /// </summary>
        ProxyClass,

        /// <summary>
        /// An attribute for a plain-model.
        /// Embedded in <see cref="PlainModelClass"/>.
        /// </summary>
        PlainModelClassAttribute,

        /// <summary>
        /// A plain-model.
        /// Results in a file.
        /// </summary>
        PlainModelClass,

        /// <summary>
        /// A template for the plain-model class filename.
        /// </summary>
        PlainModelClassFilename,

        /// <summary>
        /// An attribute for a model.
        /// Embedded in <see cref="ModelClass"/>.
        /// </summary>
        ModelClassAttribute,

        /// <summary>
        /// The id-attribute for a model.
        /// Embedded in <see cref="ModelClass"/>.
        /// </summary>
        ModelClassId,

        /// <summary>
        /// A to-many relation for a model.
        /// Embedded in <see cref="ModelClass"/>.
        /// </summary>
        ModelClassRelationToMany,

        /// <summary>
        /// A to-one/belongs-to relation for a model.
        /// Embedded in <see cref="ModelClass"/>.
        /// </summary>
        ModelClassRelationToOne,

        /// <summary>
        /// A model.
        /// Results in a file.
        /// </summary>
        ModelClass,

        /// <summary>
        /// A template for the model class filename.
        /// </summary>
        ModelClassFilename,

        /// <summary>
        /// A route for a model.
        /// Embedded in <see cref="RoutesClass"/>.
        /// </summary>
        RoutesClassRoute,

        /// <summary>
        /// All routes.
        /// Results in a file.
        /// </summary>
        RoutesClass,

        /// <summary>
        /// A template for the route class filename.
        /// </summary>
        RoutesClassFilename
    }
}