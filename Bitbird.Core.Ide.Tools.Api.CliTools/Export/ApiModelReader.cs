using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bitbird.Core.Api;
using Bitbird.Core.Api.CliToolAnnotations;
using Bitbird.Core.Api.Core;
using Bitbird.Core.Api.Nodes.Core;
using Bitbird.Core.Types;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    [UsedImplicitly]
    public sealed class ApiModelReader
    {
        [NotNull] private readonly string nodePostfix;
        [NotNull] private readonly string modelPostfix;
        [NotNull, ItemNotNull] private readonly Type[] nodeTypes;
        [NotNull, ItemNotNull] private readonly Type[] modelTypes;

        [UsedImplicitly]
        public ApiModelReader(
            [NotNull] string nodePostfix,
            [NotNull] string modelPostfix,
            [NotNull] Assembly apiModelTypeAssembly)
        {
            this.nodePostfix = nodePostfix ?? throw new ArgumentNullException(nameof(nodePostfix));
            this.modelPostfix = modelPostfix ?? throw new ArgumentNullException(nameof(modelPostfix));

            nodeTypes = (apiModelTypeAssembly ?? throw new ArgumentNullException(nameof(apiModelTypeAssembly)))
                .GetTypes()
                .Where(t => t.IsDerivedFromGeneric(typeof(ServiceNodeBase<>)) && !t.IsAbstract)
                .ToArray();

            modelTypes = apiModelTypeAssembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<ExposeModelToWebAttribute>() != null)
                .ToArray();
        }

        [NotNull, UsedImplicitly]
        public ApiModelAssemblyInfo ExtractApiModelInfo()
        {
            var foundTypes = new HashSet<Type>();

            var apiNodeInfos = nodeTypes
                .Select(x => ExtractNodeInfo(x, foundTypes))
                .ToArray();

            var nextTodoTypes = new HashSet<Type>(
                foundTypes
                    .Union(modelTypes));

            var apiModelInfos = new List<ApiModelInfo>();
            while (nextTodoTypes.Any())
            {
                var currentTodoTypes = nextTodoTypes.ToArray();
                nextTodoTypes.Clear();

                apiModelInfos.AddRange(currentTodoTypes
                    .Where(t => !t.IsEnum)
                    .ToArray()
                    .Select(x => ExtractModelInfo(x, foundTypes, nextTodoTypes)));
            }

            var apiEnumInfos = foundTypes
                .Where(t => t.IsEnum)
                .Select(ExtractEnumInfo)
                .ToArray();

            return new ApiModelAssemblyInfo(
                apiNodeInfos,
                apiModelInfos.ToArray(),
                apiEnumInfos);
        }

        [NotNull]
        private ApiModelInfo ExtractModelInfo(
            [NotNull] Type type, 
            [NotNull, ItemNotNull] HashSet<Type> foundTypes, 
            [NotNull, ItemNotNull] HashSet<Type> nextTodoTypes)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (foundTypes == null) throw new ArgumentNullException(nameof(foundTypes));

            if (!type.IsClass)
                throw new Exception($"{type.FullName} is not a class.");
            if (!type.Name.EndsWith(modelPostfix))
                throw new Exception($"{type.FullName} does not end with {modelPostfix}.");

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
                    if (p.Count() == 2)
                        return new ApiModelRelationInfo(
                            p.Single(x => !x.Attribute.IsId).Property.Name,
                            p.Single(x => x.Attribute.IsId).Property.Name,
                            p.First().Attribute.RelationType,
                            p.Single(x => !x.Attribute.IsId).Property.PropertyType.ToCsType());

                    var idProp = p.Single().Attribute.IsId
                        ? p.Single().Property
                        : type.GetProperties().Single(x => x.Name == p.Single().Attribute.RelatedPropertyName);

                    var navProp = !p.Single().Attribute.IsId
                        ? p.Single().Property
                        : type.GetProperties().Single(x => x.Name == p.Single().Attribute.RelatedPropertyName);

                    return new ApiModelRelationInfo(
                        navProp.Name,
                        idProp.Name,
                        p.First().Attribute.RelationType,
                        navProp.PropertyType.ToCsType(t =>
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
                idAttribute,
                attributes,
                relations);
        }

        [NotNull]
        private ApiModelAttributeInfo ExtractModelAttributeInfo(
            [NotNull] PropertyInfo property,
            [NotNull, ItemNotNull] HashSet<Type> foundTypes, 
            [NotNull, ItemNotNull] HashSet<Type> nextTodoTypes)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (foundTypes == null) throw new ArgumentNullException(nameof(foundTypes));
            if (nextTodoTypes == null) throw new ArgumentNullException(nameof(nextTodoTypes));

            return new ApiModelAttributeInfo(
                property.Name,
                property.PropertyType.ToCsType(t =>
                {
                    if (foundTypes.Add(t))
                        nextTodoTypes.Add(t);
                }));
        }

        [NotNull]
        private ApiEnumInfo ExtractEnumInfo([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!type.IsEnum)
                throw new Exception($"{type.FullName} is not an enum.");

            var values = Enum.GetValues(type)
                .OfType<object>()
                .Select(v => new ApiEnumValueInfo(
                    v.ToString(),
                    Enum.GetName(type, v) ?? throw new InvalidOperationException($"Enum value name for value {v} in enum {type.FullName} not found.")))
                .ToArray();

            return new ApiEnumInfo(
                type.ToCsType(),
                Enum.GetUnderlyingType(type).ToCsType(),
                values);
        }

        [NotNull]
        private ApiNodeInfo ExtractNodeInfo(
            [NotNull] Type nodeType, 
            [NotNull, ItemNotNull] HashSet<Type> foundTypes)
        {
            if (nodeType == null) throw new ArgumentNullException(nameof(nodeType));
            if (foundTypes == null) throw new ArgumentNullException(nameof(foundTypes));

            if (!nodeType.Name.EndsWith(nodePostfix))
                throw new Exception($"Class {nodeType.FullName} does not have the postfix {nodePostfix}.");

            var isCrud = nodeType.ImplementsGeneric(typeof(IServiceCrudNode<,,>), out var crudGenericArguments);
            var modelType = isCrud ? crudGenericArguments[1] : null;
            var isRead = nodeType.ImplementsGeneric(typeof(IServiceReadNode<,,>), out var readGenericArguments);
            modelType = modelType ?? (isRead ? readGenericArguments[1] : null);

            var additionalMethods = nodeType.GetMethods()
                .Where(m => m.GetCustomAttribute<ExposeMethodToWebAttribute>() != null)
                .Select(x => ExtractNodeMethodInfo(x, foundTypes))
                .ToArray();
            
            return new ApiNodeInfo(
                nodeType.Name,
                nodeType.Name.Substring(0, nodeType.Name.Length - nodePostfix.Length),
                isCrud,
                isRead,
                modelType?.ToCsType(t => foundTypes.Add(t)),
                additionalMethods);
        }

        [NotNull]
        private ApiNodeMethodInfo ExtractNodeMethodInfo(
            [NotNull] MethodInfo method, 
            [NotNull, ItemNotNull] HashSet<Type> foundTypes)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (foundTypes == null) throw new ArgumentNullException(nameof(foundTypes));

            var isAsync = (method.ReturnType.IsDerivedFromGeneric(typeof(Task<>), out var taskArguments) ||
                method.ReturnType.IsDerivedFrom(typeof(Task))) ;
            var returnType = taskArguments.FirstOrDefault() ?? method.ReturnType;
            if (returnType == typeof(void))
                returnType = null;

            var methodName = method.Name;
            if (methodName.EndsWith("Async"))
                methodName = methodName.Substring(0, methodName.Length - "Async".Length);
            
            var parametersDict = method.GetParameters()
                .Where(p => !p.ParameterType.Implements(typeof(IApiSession)))
                .Select(p => new
                {
                    IsRouteParameter = p.ParameterType.IsPrimitive || p.ParameterType == typeof(string),
                    Parameter = p
                })
                .GroupBy(x => x.IsRouteParameter)
                .ToDictionary(x => x.Key, x => x.Select(p => p.Parameter).ToArray());
            var routeParameters = parametersDict.TryGetValue(true, out var val) ? val : new ParameterInfo[0];
            var bodyParameters = parametersDict.TryGetValue(false, out val) ? val : new ParameterInfo[0];

            if (routeParameters.Length > 1)
                throw new Exception($"The method {method.DeclaringType?.FullName}.{method.Name} has more than one route parameter.");
            if (bodyParameters.Length > 1)
                throw new Exception($"The method {method.DeclaringType?.FullName}.{method.Name} has more than one body parameter.");

            return new ApiNodeMethodInfo(
                method.Name,
                methodName, 
                methodName.ToKebabCase(),
                isAsync,
                returnType?.ToCsType(t => foundTypes.Add(t)),
                routeParameters.Select(x => ExtractNodeMethodRouteParameter(x, foundTypes)).SingleOrDefault(),
                bodyParameters.Select(x => ExtractNodeMethodBodyParameterInfo(x, foundTypes)).SingleOrDefault());
        }

        [NotNull]
        private ApiNodeMethodRouteParameterInfo ExtractNodeMethodBodyParameterInfo(
            [NotNull] ParameterInfo parameter, 
            [NotNull, ItemNotNull] HashSet<Type> foundTypes)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));
            if (foundTypes == null) throw new ArgumentNullException(nameof(foundTypes));
            
            return new ApiNodeMethodRouteParameterInfo(
                parameter.Position,
                parameter.Name,
                parameter.ParameterType.ToCsType(t => foundTypes.Add(t)));
        }

        [NotNull]
        private ApiNodeMethodBodyParameterInfo ExtractNodeMethodRouteParameter(
            [NotNull] ParameterInfo parameter, 
            [NotNull, ItemNotNull] HashSet<Type> foundTypes)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));
            if (foundTypes == null) throw new ArgumentNullException(nameof(foundTypes));

            return new ApiNodeMethodBodyParameterInfo(
                parameter.Position, 
                parameter.Name,
                parameter.ParameterType.ToCsType(t => foundTypes.Add(t)));
        }
    }
}
