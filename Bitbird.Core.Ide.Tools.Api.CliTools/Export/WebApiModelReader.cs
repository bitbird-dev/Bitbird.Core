using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bitbird.Core.Api.CliToolAnnotations;
using Bitbird.Core.Api.Core;
using Bitbird.Core.Types;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class WebApiModelReader
    {
        [NotNull] private readonly string controllerPostfix;
        [NotNull, ItemNotNull] private readonly Type[] controllerTypes;

        [UsedImplicitly]
        public WebApiModelReader(
            [NotNull] string controllerPostfix,
            [NotNull] Assembly webApiModelTypeAssembly)
        {
            this.controllerPostfix = controllerPostfix ?? throw new ArgumentNullException(nameof(controllerPostfix));

            controllerTypes = (webApiModelTypeAssembly ?? throw new ArgumentNullException(nameof(webApiModelTypeAssembly)))
                .GetTypes()
                .Where(t => t.IsDerivedFromGeneric(typeof(ServiceNodeBase<>)) && !t.IsAbstract)
                .ToArray();
        }

        [NotNull, UsedImplicitly]
        public WebApiModelAssemblyInfo ExtractWebApiModelInfo()
        {
            var webApiControllerInfos = controllerTypes
                .Select(ExtractControllerInfo)
                .ToArray();

            return new WebApiModelAssemblyInfo(
                webApiControllerInfos);
        }

        [NotNull]
        private WebApiControllerInfo ExtractControllerInfo(
            [NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!type.IsClass)
                throw new Exception($"{type.FullName} is not a class.");
            if (!type.Name.EndsWith(controllerPostfix))
                throw new Exception($"{type.FullName} does not end with {controllerPostfix}.");

            var classIgnores = new HashSet<string>(
                type.GetCustomAttributes<IgnorePropertyInResourceAttribute>()
                    .Select(a => a.PropertyName));

            var properties = type
                .GetProperties()
                .Where(p => !classIgnores.Contains(p.Name) && 
                            p.GetCustomAttribute<IgnoreInResourceAttribute>() == null && 
                            p.CanRead && 
                            !p.GetGetMethod().IsStatic)
                .OrderBy(p => p.Name)
                .ToArray();

            var relations = properties
                .Select(p => new
                {
                    Property = p,
                    Attribute = p.GetCustomAttribute<ModelRelationAttribute>()
                })
                .Where(p => p.Attribute != null)
                .GroupBy(p => p.Attribute.RelationName)
                .Select(p =>
                {
                    Type propertyType;
                    if (p.Count() == 2)
                    {
                        propertyType = p.Single(x => !x.Attribute.IsId).Property.PropertyType;
                        if (propertyType.IsArray)
                            propertyType = propertyType.GetElementType();

                        return new ApiModelRelationInfo(
                            p.Single(x => !x.Attribute.IsId).Property.Name,
                            p.Single(x => x.Attribute.IsId).Property.Name,
                            p.First().Attribute.RelationType,
                            propertyType.ToCsType(t =>
                            {
                                if (foundTypes.Add(t))
                                    nextTodoTypes.Add(t);
                            }));
                    }

                    var idProp = p.Single().Attribute.IsId
                        ? p.Single().Property
                        : type.GetProperties().Single(x => x.Name == p.Single().Attribute.RelatedPropertyName);

                    var navProp = !p.Single().Attribute.IsId
                        ? p.Single().Property
                        : type.GetProperties().Single(x => x.Name == p.Single().Attribute.RelatedPropertyName);

                    propertyType = navProp.PropertyType;
                    if (propertyType.IsArray)
                        propertyType = propertyType.GetElementType();

                    return new ApiModelRelationInfo(
                        navProp.Name,
                        idProp.Name,
                        p.First().Attribute.RelationType,
                        propertyType.ToCsType(t =>
                        {
                            if (foundTypes.Add(t))
                                nextTodoTypes.Add(t);
                        }));
                })
                .OrderBy(x => x.Type)
                .ThenBy(x => x.Name)
                .ToArray();

            var relationPropertyNameMap = new HashSet<string>(
                relations.SelectMany(r => new[]
                {
                    r.Name,
                    r.IdName
                }));

            var attributes = properties
                .Where(p => !relationPropertyNameMap.Contains(p.Name))
                .Select(x => ExtractModelAttributeInfo(x, foundTypes, nextTodoTypes))
                .ToArray();

            var idAttribute = attributes.SingleOrDefault(p => p.Name == "Id");

            attributes = attributes
                .Where(p => p != idAttribute)
                .OrderBy(p => p.Name)
                .ToArray();
            
            return new ApiModelInfo(
                type.Name,
                type.Name.Substring(0, type.Name.Length - modelPostfix.Length),
                type.Name.Substring(0, type.Name.Length - modelPostfix.Length).ToKebabCase(),
                idAttribute,
                attributes,
                relations);
        }
}
