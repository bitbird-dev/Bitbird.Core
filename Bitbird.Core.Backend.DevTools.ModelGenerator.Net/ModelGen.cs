using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Bitbird.Core.Data.Query;
using Bitbird.Core.Json.Helpers.ApiResource;
using Bitbird.Core.Types;
using Bitbird.Core.WebApi.Controllers;
using Bitbird.Core.WebApi.JsonApi;
using Bitbird.Core.WebApi.Resources;
using Microsoft.AspNet.SignalR;

namespace Bitbird.Core.Backend.DevTools.ModelGenerator.Net
{
    /// <summary>
    /// Generates models.
    /// </summary>
    public class ModelGen
    {
        private readonly ConsoleArguments arguments;
        private readonly Type[] modelResourceAssemblyTypes;
        private readonly Func<Type, bool> modelResourceTypePredicate;
        private readonly Type[] signalRHubAssemblyTypes;
        private readonly Func<Type, bool> signalRHubTypePredicate;
        private readonly Type[] controllerAssemblyTypes;
        private readonly Func<Type, bool> controllerTypePredicate;
        private readonly ResourceManager[] translationResourceManagers;
        private readonly Dictionary<string, bool> customPredicates;
        private readonly string[] languages;
        private readonly long interfaceVersion;
        private readonly DocumentationCollection documentation;
        private readonly TemplateCollection templates;

        /// <summary>
        /// Constructs a <see cref="ModelGenerator"/>.
        /// For more info see the class documentation of <see cref="ModelGenerator"/>.
        /// </summary>
        public ModelGen(ConsoleArguments arguments,
            Type[] modelResourceAssemblyTypes,
            Func<Type, bool> modelResourceTypePredicate,
            Type[] signalRHubAssemblyTypes,
            Func<Type, bool> signalRHubTypePredicate,
            Type[] controllerAssemblyTypes,
            Func<Type, bool> controllerTypePredicate,
            ResourceManager[] translationResourceManagers,
            Dictionary<string, bool> customPredicates,
            string[] languages,
            long interfaceVersion, 
            ResourceManager resourceManager)
        {
            this.arguments = arguments;
            this.modelResourceAssemblyTypes = modelResourceAssemblyTypes;
            this.modelResourceTypePredicate = modelResourceTypePredicate;
            this.signalRHubAssemblyTypes = signalRHubAssemblyTypes;
            this.signalRHubTypePredicate = signalRHubTypePredicate;
            this.controllerAssemblyTypes = controllerAssemblyTypes;
            this.controllerTypePredicate = controllerTypePredicate;
            this.translationResourceManagers = translationResourceManagers;
            this.customPredicates = customPredicates;
            this.languages = languages;
            this.interfaceVersion = interfaceVersion;
            documentation = new DocumentationCollection(arguments.DocDirectory);
            templates = new TemplateCollection(arguments.TargetFormat, resourceManager);
        }

        public void ClearDirAndFile()
        {
            if (Directory.Exists(arguments.TargetDirectory))
            {
                foreach (var file in Directory.GetFiles(arguments.TargetDirectory, "*", SearchOption.AllDirectories))
                    File.Delete(file);

                foreach (var dir in Directory.GetDirectories(arguments.TargetDirectory, "*", SearchOption.TopDirectoryOnly))
                    Directory.Delete(dir, true);
            }

            if (arguments.TargetFile != null && File.Exists(arguments.TargetFile))
                File.Delete(arguments.TargetFile);
        }

        /// <summary>
        /// Finds all models, generates code files and writes them to the output directory.
        /// </summary>
        /// <returns>Nothing.</returns>
        public async Task WriteAllFiles()
        {
            var models = FindAllModels();
            var controllers = FindAllControllers();
            var hubs = FindAllHubs();
            var foundClassTypes = new HashSet<Type>();

            var generatedFiles = models.Select(model => GenerateModelFile(model, foundClassTypes)).ToArray();
            generatedFiles = generatedFiles.Concat(controllers.Select(c => GenerateProxyFile(c, models, foundClassTypes))).ToArray();
            generatedFiles = generatedFiles.Concat(new[] { GenerateProxiesFile(controllers) }).ToArray();

            hubs.SelectMany(hub =>
                hub.ServerMethods.SelectMany(m => m.ParameterTypes.Concat(new[] {m.ReturnType}))
                    .Concat(hub.ClientMethods.SelectMany(m => m.ParameterTypes.Concat(new[] {m.ReturnType}))))
                .ToList()
                .ForEach(t =>
                {
                    var value = t;
                    if (typeof(Task).IsAssignableFrom(value))
                        value = value.GetGenericArguments().FirstOrDefault() ?? typeof(void);

                    if (typeof(IQueryResult).IsAssignableFrom(value))
                        value = value.GetGenericArguments()[0].MakeArrayType();

                    if ((typeof(IEnumerable).IsAssignableFrom(value) || value.IsArray) && value != typeof(string))
                        value = value.IsArray
                            ? value.GetElementType()
                            : value.GetGenericArguments()[0];

                    if (value == typeof(void))
                        return;

                    if (value?.Namespace?.StartsWith("System") ?? false)
                        return;
                    
                    foundClassTypes.Add(value);
                });

            var foundPlainModels = foundClassTypes.Where(m => !m.IsEnum).ToHashSet();
            var foundEnums = foundClassTypes.Where(m => m.IsEnum).ToHashSet();
            generatedFiles = generatedFiles.Concat(foundPlainModels.ToArray().SelectMany(type => GeneratePlainModelFile(type, foundPlainModels, foundEnums))).ToArray();

            generatedFiles = generatedFiles.Concat(foundEnums.Select(GenerateEnumFile)).ToArray();
            generatedFiles = generatedFiles.Concat(new[] { GenerateEnumsFile(foundEnums) }).ToArray();
            generatedFiles = generatedFiles.Concat(GenerateEnumsTranslationFiles(foundEnums.ToArray())).ToArray();
            generatedFiles = generatedFiles.Concat(new[] { GenerateRouteFile(models) }).ToArray();
            generatedFiles = generatedFiles.Concat(new[] { GenerateInterfaceVersion() }).ToArray();

            foreach (var generated in generatedFiles)
            {
                if (string.IsNullOrWhiteSpace(generated.Filename))
                    continue;

                var path = Path.Combine(arguments.TargetDirectory, generated.Filename);

                Directory.CreateDirectory(Path.GetDirectoryName(path) 
                                          ?? throw new InvalidOperationException());

                using (var file = new StreamWriter(path, false, Encoding.UTF8))
                {
                    await file.WriteAsync(generated.Content);
                    await file.FlushAsync();
                    file.Close();
                }
            }

            if (arguments.TargetFile != null)
                ZipFile.CreateFromDirectory(arguments.TargetDirectory, arguments.TargetFile);
        }

        private ModelClassMetaData[] FindAllModels()
        {
            return modelResourceAssemblyTypes
                .SelectMany(x => Assembly.GetAssembly(x).GetTypes())
                .Where(modelResourceTypePredicate)
                .Where(type => typeof(JsonApiResource).IsAssignableFrom(type))
                .Select(type =>
                {
                    var attr = type.GetCustomAttribute<JsonApiResourceMappingAttribute>();
                    return new ModelClassMetaData(type,
                        (JsonApiResource) Activator.CreateInstance(type),
                        attr?.Type,
                        attr?.IsDefaultDeserializer ?? false,
                        attr?.IsForDataTransferOnly ?? true
                    );
                })
                .Where(x => x.Model != null)
                .ToArray();
        }

        private HubMetaData[] FindAllHubs()
        {
            return signalRHubAssemblyTypes
                .SelectMany(t => Assembly.GetAssembly(t).GetTypes())
                .Where(signalRHubTypePredicate)
                .Select(t => new
                {
                    Type = t,
                    IsHub = typeof(Hub).IsAssignableFrom(t)
                })
                .Where(t => t.IsHub)
                .OrderBy(t => t.Type.Name)
                .Select(t =>
                {
                    var serverMethods = t.Type.GetMethods()
                        .Where(m => !m.IsVirtual && m.IsPublic && !m.IsStatic && m.DeclaringType == t.Type)
                        .Select(m => new HubServerMethodMetaData(m.Name, 
                            m.ReturnType,
                            m.GetParameters().Select(p => p.ParameterType).ToArray())).ToArray();

                    Type clientInterfaceType = null;
                    var type = t.Type;
                    while (type != null && type != typeof(object))
                    {
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Hub<>))
                        {
                            clientInterfaceType = type.GetGenericArguments()[0];
                            break;
                        }

                        type = type.BaseType;
                    }

                    var clientMethods = new HubClientMethodMetaData[0];
                    if (clientInterfaceType != null)
                    {
                        clientMethods = clientInterfaceType.GetMethods()
                            .Where(m => m.IsPublic && !m.IsStatic && m.DeclaringType == clientInterfaceType)
                            .Select(m => new HubClientMethodMetaData(m.Name,
                                m.ReturnType,
                                m.GetParameters().Select(p => p.ParameterType).ToArray())).ToArray();
                    }

                    return new HubMetaData(t.Type.Name, serverMethods, clientMethods);
                })
                .ToArray();
        }

        private Dictionary<ControllerMetaData, ActionMetaData[]> FindAllControllers()
        {
            string GetHttpCommand(MethodInfo actionMethod)
            {
                if (actionMethod.GetCustomAttribute<HttpGetAttribute>() != null)
                    return "GET";
                if (actionMethod.GetCustomAttribute<HttpPostAttribute>() != null)
                    return "POST";
                if (actionMethod.GetCustomAttribute<HttpPutAttribute>() != null)
                    return "PUT";
                if (actionMethod.GetCustomAttribute<HttpPatchAttribute>() != null)
                    return "PATCH";
                if (actionMethod.GetCustomAttribute<HttpDeleteAttribute>() != null)
                    return "DELETE";
                if (actionMethod.GetCustomAttribute<HttpOptionsAttribute>() != null)
                    return "OPTIONS";
                return "GET";
            }

            return controllerAssemblyTypes
                .SelectMany(t => Assembly.GetAssembly(t).GetTypes())
                .Where(controllerTypePredicate)
                .Select(t => new
                {
                    Type = t,
                    IsController = typeof(IControllerBase).IsAssignableFrom(t)
                })
                .Where(t => t.IsController && !t.Type.IsAbstract)
                .OrderBy(t => t.Type.Name)
                .Select(t =>
                {
                    var mD = new ControllerMetaData(t.Type,
                        t.Type.Name.EndsWith("Controller")
                            ? t.Type.Name.Substring(0, t.Type.Name.Length - "Controller".Length)
                            : t.Type.Name,
                        t.Type.GetCustomAttribute<ObsoleteAttribute>() != null,
                        t.Type.Namespace?.Split('.').Last(),
                        t.Type.GetCustomAttribute<RoutePrefixAttribute>()?.Prefix,
                        (IControllerBase) Activator.CreateInstance(t.Type));

                    mD.ReadInstance?.SetApiResourceModels();

                    return mD;
                })
                .Where(c => c.RoutePrefix != null)
                .ToDictionary(c => c, 
                    c => c.ControllerType.GetMethods()
                        .Select(m => new
                        {
                            Method = m,
                            Route = m.GetCustomAttribute<RouteAttribute>()
                        })
                        .Where(m => m.Route != null)
                        .SelectMany(m =>
                        {
                            var jsonApiAttr = m.Method.GetCustomAttribute<JsonApiAttribute>();
                            var name = m.Method.Name;
                            var httpMethod = GetHttpCommand(m.Method);
                            var relativeRoute = m.Route.Template;
                            var supportsQueryInfo = jsonApiAttr?.SupportsQuery ?? false;
                            var returnType = m.Method.ReturnType;
                            var bodyParameter = m.Method.GetParameters()
                                .Where(p => p.GetCustomAttribute<FromBodyAttribute>() != null)
                                .ToDictionary(p => p.Name, p =>
                                {
                                    if (typeof(IContentInfo).IsAssignableFrom(p.ParameterType))
                                        return p.ParameterType.GetGenericArguments()[0];
                                    return p.ParameterType;
                                })
                                .Cast<KeyValuePair<string, Type>?>()
                                .SingleOrDefault();
                            var routeParameter = m.Method.GetParameters()
                                .Where(p => p.GetCustomAttribute<FromBodyAttribute>() == null &&
                                            p.GetCustomAttribute<FromUriAttribute>() == null)
                                .ToDictionary(p => p.Name, p =>
                                {
                                    if (typeof(IContentInfo).IsAssignableFrom(p.ParameterType))
                                        return p.ParameterType.GetGenericArguments()[0];
                                    return p.ParameterType;
                                });

                            if (c.ReadInstance != null)
                            {
                                if (m.Method.Name == "GetRelationAsync")
                                    return new ActionMetaData[0];
                            }
                            if (c.CrudInstance != null)
                            {
                                if (m.Method.Name == "CreateRelationAsync")
                                    return new ActionMetaData[0];
                                if (m.Method.Name == "UpdateRelationAsync")
                                    return new ActionMetaData[0];
                                if (m.Method.Name == "DeleteRelationAsync")
                                    return new ActionMetaData[0];

                                if (m.Method.Name == "CreateAsync" && !c.CrudInstance.CanCreate)
                                    return new ActionMetaData[0];
                                if (m.Method.Name == "DeleteAsync" && !c.CrudInstance.CanDelete)
                                    return new ActionMetaData[0];
                                if (m.Method.Name == "UpdateAsync" && !c.CrudInstance.CanUpdate)
                                    return new ActionMetaData[0];
                            }

                            return new [] { new ActionMetaData(name,
                                    httpMethod,
                                    relativeRoute,
                                    supportsQueryInfo,
                                    returnType,
                                    bodyParameter,
                                    routeParameter,
                                    jsonApiAttr) };
                        }).ToArray());
        }
        private IEnumerable<GeneratedFile> GeneratePlainModelFile(Type modelType, HashSet<Type> foundPlainModels, HashSet<Type> foundEnums)
        {
            var files = new List<GeneratedFile>();
            // attributes
            var attributes = modelType.GetProperties()
                .Where(p => p.CanRead && p.CanWrite && !p.GetGetMethod().IsStatic)
                .Select(p =>
                {
                    var propertyDoc = documentation.ForProperty(p.DeclaringType, p.Name);

                    var type = p.PropertyType;
                    var isArray = type.IsArray;
                    if (isArray)
                        type = type.GetElementType();
                    var isCopyOrString = type.IsValueType || type == typeof(string);

                    var attribute = templates.Get(TemplateType.PlainModelClassAttribute);
                    attribute = ResolvePredicate(attribute, "isArray", isArray);
                    attribute = ResolvePredicate(attribute, "isCopyOrString", isCopyOrString);
                    attribute = ReplaceToken(attribute, "propertyName", p.Name);
                    attribute = ReplaceToken(attribute, "propertyType", p.PropertyType, t =>
                    {
                        if (t.IsEnum)
                            foundEnums.Add(t);
                        else if (foundPlainModels.Add(t))
                            files.AddRange(GeneratePlainModelFile(t, foundPlainModels, foundEnums));
                    });
                    attribute = ReplaceToken(attribute, "doc-summary", propertyDoc?.Get(DocumentationType.Summary, documentation));
                    return ExtractSections(attribute);
                }).ToArray();
            var attributesSection = JoinSections(x => string.Empty, attributes);

            // class
            var classDoc = documentation.ForClass(modelType);

            var model = templates.Get(TemplateType.PlainModelClass);
            model = ReplaceSections(model, "attributes", attributesSection);
            model = ReplaceToken(model, "doc-summary", classDoc?.Get(DocumentationType.Summary, documentation));
            model = ReplaceToken(model, "className", modelType.Name);
            model = ResolveCustomPredicates(model);

            // filename
            var filename = templates.Get(TemplateType.PlainModelClassFilename);
            filename = ReplaceSections(filename, "attributes", attributesSection);
            filename = ReplaceToken(filename, "className", modelType.Name);
            filename = ResolveCustomPredicates(filename);

            return files.Concat(new [] { new GeneratedFile(filename, model) });
        }

        private GeneratedFile GenerateModelFile(ModelClassMetaData resource, HashSet<Type> foundClassTypes)
        {
            var inverseProperties = resource.Resource
                .GetCustomAttributes<JsonApiResourceInverseRelationAttribute>()
                .ToDictionary(attr => attr.LocalAttribute,
                    attr => new
                    {
                        Attribute = attr,
                        RelatedResourceInstance = (JsonApiResource)Activator.CreateInstance(attr.RelatedResource)
                    });
            // check inverse properties
            foreach (var inverseProperty in inverseProperties)
            {
                var found = inverseProperty.Value.RelatedResourceInstance.Relationships.Where(r =>
                    r.PropertyName.Equals(inverseProperty.Value.Attribute.RelatedAttribute)).ToArray();

                if (found.Length != 1)
                    throw new Exception($"An inverse property was defined on {resource.Resource.Name} for {inverseProperty.Value.Attribute.LocalAttribute}, but the related resource ({inverseProperty.Value.Attribute.RelatedResource.Name}) has no attribute for {inverseProperty.Value.Attribute.RelatedAttribute}");
            }

            var alwaysIncludedIdProperties = resource.Resource
                .GetCustomAttributes<JsonApiResourceAlwaysIncludedRelationAttribute>()
                .Select(a => a.IdRelationPropertyName)
                .ToHashSet();


            // attributes
            var attributes = resource.Instance.Attributes.Select(a =>
            {
                var property = resource.Model.GetProperty(a.PropertyName) ?? throw new Exception($"Property {a.PropertyName} not found!");
                var propertyDoc = documentation.ForProperty(resource.Model, property.Name);

                var attribute = templates.Get(TemplateType.ModelClassAttribute);
                attribute = ReplaceToken(attribute, "propertyName", property.Name);
                attribute = ReplaceToken(attribute, "propertyType", property.PropertyType, t => foundClassTypes.Add(t));
                attribute = ReplaceToken(attribute, "doc-summary", propertyDoc?.Get(DocumentationType.Summary, documentation));
                return ExtractSections(attribute);
            }).ToArray();
            var attributesSection = JoinSections(x => string.Empty, attributes);

            // relations
            var relations = resource.Instance.Relationships.Select(a =>
            {
                var relationIdPropertyDoc = documentation.ForProperty(resource.Model, a.IdPropertyName);
                var inverse = inverseProperties.TryGetValue(a.PropertyName, out var inverseTemp) ? inverseTemp : null;

                var inverseName = inverse?.RelatedResourceInstance.Relationships
                    .Single(r => r.PropertyName.Equals(inverse.Attribute.RelatedAttribute)).PropertyName;

                var relation = templates.Get(a.Kind == RelationshipKind.BelongsTo  ? TemplateType.ModelClassRelationToOne : TemplateType.ModelClassRelationToMany);

                relation = ResolvePredicate(relation, "isAlwaysIncluded", alwaysIncludedIdProperties.Contains(a.IdPropertyName));

                relation = ReplaceToken(relation, "inverseProperty", inverseName ?? "null");
                relation = ReplaceToken(relation, "inversePropertyQuoted", inverseName == null ? "null" : $"'{inverseName}'");
                relation = ReplaceToken(relation, "propertyName", a.PropertyName);
                relation = ReplaceToken(relation, "relationPath", a.UrlPath.FromKebabCase());
                relation = ReplaceToken(relation, "relationType", a.RelatedResource.ResourceType.FromKebabCase());
                relation = ReplaceToken(relation, "doc-summary", relationIdPropertyDoc?.Get(DocumentationType.Summary, documentation));
                return ExtractSections(relation);
            }).ToArray();
            var relationsSection = JoinSections(x => string.Empty, relations);
            
            // id
            var idProperty = resource.Model.GetProperty(resource.Instance.IdProperty);
            string id = null;
            if (idProperty != null)
            {
                var idPropertyDoc = documentation.ForProperty(resource.Model, idProperty.Name);

                id = templates.Get(TemplateType.ModelClassId);
                id = ReplaceToken(id, "propertyName", idProperty.Name);
                id = ReplaceToken(id, "propertyType", idProperty.PropertyType, t => foundClassTypes.Add(t));
                id = ReplaceToken(id, "doc-summary", idPropertyDoc?.Get(DocumentationType.Summary, documentation));
            }
            var idSection = ExtractSections(id);

            // class
            var classDoc = documentation.ForClass(resource.Model);

            var model = templates.Get(TemplateType.ModelClass);
            model = ReplaceSections(model, "id", idSection);
            model = ReplaceSections(model, "attributes", attributesSection);
            model = ReplaceSections(model, "relations", relationsSection);
            model = ReplaceToken(model, "doc-summary", classDoc?.Get(DocumentationType.Summary, documentation));
            model = ReplaceToken(model, "className", resource.Instance.ResourceType.FromKebabCase());
            model = ReplaceToken(model, "route", resource.Instance.UrlPath.Trim('/', '\\'));
            model = ResolveCustomPredicates(model);

            // filename
            var filename = templates.Get(TemplateType.ModelClassFilename);
            filename = ReplaceSections(filename, "id", idSection);
            filename = ReplaceSections(filename, "attributes", attributesSection);
            filename = ReplaceSections(filename, "relations", relationsSection);
            filename = ReplaceToken(filename, "doc-summary", classDoc?.Get(DocumentationType.Summary, documentation));
            filename = ReplaceToken(filename, "className", resource.Instance.ResourceType.FromKebabCase());
            filename = ReplaceToken(filename, "route", resource.Instance.UrlPath.Trim('/', '\\'));
            filename = ResolveCustomPredicates(filename);

            return new GeneratedFile(filename, model);
        }

        private GeneratedFile GenerateProxiesFile(
            Dictionary<ControllerMetaData, ActionMetaData[]> controllers)
        {
            // controllers
            var controllerSections = controllers.Select(c =>
            {
                var section = templates.Get(TemplateType.ProxiesClassProxy);
                section = ReplaceToken(section, "controllerName", c.Key.FriendlyName);
                return ExtractSections(section);
            }).ToArray();
            var controllerSection = JoinSections(x => string.Empty, controllerSections);

            // class
            var model = templates.Get(TemplateType.ProxiesClass);
            model = ReplaceSections(model, "controllers", controllerSection);
            model = ResolveCustomPredicates(model);

            // filename
            var filename = templates.Get(TemplateType.ProxiesClassFilename);
            filename = ResolveCustomPredicates(filename);

            return new GeneratedFile(filename, model);
        }

        private GeneratedFile GenerateProxyFile(
            KeyValuePair<ControllerMetaData, ActionMetaData[]> controller,
            ModelClassMetaData[] models,
            HashSet<Type> foundPlainModels)
        {
            // controllers
            var actionsSections = controller.Value.Select(a =>
            {
                var section = templates.Get(TemplateType.ProxyClassAction);
                section = ReplaceToken(section, "actionName", a.Name);

                var jsonApiResource = a.JsonApiAttribute?.ReturnResourceGetter?.Invoke(controller.Key.Instance);
                var model = models.SingleOrDefault(m => m.Model == a.BodyParameter?.Value &&
                                                        m.IsDefaultDeserializer);
                if (model != null)
                    section = ReplaceToken(section, "actionParameter", model.Instance.ResourceType.FromKebabCase());
                else if (a.BodyParameter.HasValue)
                {
                    foundPlainModels.Add(a.BodyParameter.Value.Value);
                    section = ReplaceToken(section, "actionParameter", a.BodyParameter.Value.Value.Name);
                }

                section = ReplaceToken(section, "httpMethod", a.HttpMethod);
                section = ReplaceToken(section, "route", RemoveTypesFromRoute($"{controller.Key.RoutePrefix}{(string.IsNullOrWhiteSpace(a.RelativeRoute) ? string.Empty : "/")}{a.RelativeRoute}"));
                
                section = ReplaceToken(section, "doc-summary", documentation.ForMethod(controller.Key.ControllerType, a.Name)?.Get(DocumentationType.Summary, documentation));

                var value = a.ReturnType;
                if (typeof(Task).IsAssignableFrom(value))
                    value = value.GetGenericArguments().FirstOrDefault() ?? typeof(void);

                var queryResult = false;
                if (typeof(IQueryResult).IsAssignableFrom(value))
                {
                    value = value.GetGenericArguments()[0].MakeArrayType();
                    queryResult = true;
                }

                var isArray = true;
                if (typeof(IEnumerable).IsAssignableFrom(value) || value.IsArray)
                {
                    value = value.IsArray 
                        ? value.GetElementType() 
                        : value.GetGenericArguments()[0];
                }
                else
                {
                    isArray = false;
                }


                var returnsBinary = a.JsonApiAttribute?.ReturnsBinary ?? false;
                var returnsJsonApi = !returnsBinary && (jsonApiResource != null);
                var returnsNothing = !returnsBinary && !returnsJsonApi && (value == typeof(void));
                var returnsJson = !returnsBinary && !returnsJsonApi && !returnsNothing;
                var returnsNothingOrBinary = returnsNothing || returnsBinary;

                var returnModel = models.SingleOrDefault(m => m.Model == value &&
                                                              m.Resource == jsonApiResource?.GetType());

                section = ResolvePredicate(section, "isArray", isArray);

                section = ResolvePredicate(section, "supportsQueryInfo", returnsJsonApi || (a.JsonApiAttribute?.SupportsQuery ?? false));

                section = ResolvePredicate(section, "getsBinary", a.JsonApiAttribute?.HasBinaryParameters ?? false);
                section = ResolvePredicate(section, "getsJsonApi", !(a.JsonApiAttribute?.HasBinaryParameters ?? false) && a.BodyParameter.HasValue && (model != null));
                section = ResolvePredicate(section, "getsJson", !(a.JsonApiAttribute?.HasBinaryParameters ?? false) && a.BodyParameter.HasValue && (model == null));

                section = ResolvePredicate(section, "hasCount", queryResult);
                section = ResolvePredicate(section, "returnsNothingOrBinary", returnsNothingOrBinary);
                section = ResolvePredicate(section, "returnsNothing", returnsNothing);
                section = ResolvePredicate(section, "returnsJson", returnsJson);
                section = ResolvePredicate(section, "returnsJsonApi", returnsJsonApi);
                section = ResolvePredicate(section, "returnsBinary", returnsBinary);

                if (returnsJsonApi)
                {
                    section = ReplaceToken(section, "returnModelResource", $"{jsonApiResource?.ResourceType.FromKebabCase() ?? string.Empty}Resource");
                    section = ReplaceToken(section, "returnModelElement", returnModel?.Instance.ResourceType.FromKebabCase());
                    section = ReplaceToken(section, "returnModel", !isArray 
                        ? returnModel?.Instance.ResourceType.FromKebabCase()
                        : $"IEnumerable<{returnModel?.Instance.ResourceType.FromKebabCase()}>");
                }
                else if (returnsJson)
                {
                    section = ReplaceToken(section, "returnModelElement", value, t => foundPlainModels.Add(t));
                }





                // parameters
                var parametersSections = a.RouteParameter.Select(p =>
                {
                    var parameterSection = templates.Get(TemplateType.ProxyClassActionParameter);
                    parameterSection = ReplaceToken(parameterSection, "parameterName", p.Key);
                    parameterSection = ReplaceToken(parameterSection, "parameterType", p.Value, null);
                    return ExtractSections(parameterSection);
                }).ToArray();
                var parametersSection = JoinSections(x => string.Empty, parametersSections);

                section = ReplaceSections(section, "parameters", parametersSection);
                return ExtractSections(section);
            }).ToArray();
            var actionsSection = JoinSections(x => string.Empty, actionsSections);

            // class
            var proxy = templates.Get(TemplateType.ProxyClass);
            proxy = ReplaceSections(proxy, "actions", actionsSection);
            proxy = ReplaceToken(proxy, "controllerName", controller.Key.FriendlyName);
            proxy = ResolveCustomPredicates(proxy);

            // filename
            var filename = templates.Get(TemplateType.ProxyClassFilename);
            filename = ReplaceToken(filename, "controllerName", controller.Key.FriendlyName);
            filename = ResolveCustomPredicates(filename);

            return new GeneratedFile(filename, proxy);
        }

        private GeneratedFile GenerateEnumsFile(IEnumerable<Type> enumTypes)
        {
            var enums = enumTypes.Select(enumType =>
            {
                // values
                var values = Enum.GetValues(enumType).Cast<object>().Select(v =>
                {
                    var name = Enum.GetName(enumType, v);
                    var valueDoc = documentation.ForField(enumType, name);

                    var value = templates.Get(TemplateType.EnumsEnumClassValue);
                    value = ReplaceToken(value, "valueName", name);
                    value = ReplaceToken(value, "valueValue", Convert.ChangeType(v, Enum.GetUnderlyingType(enumType)).ToString());
                    value = ReplaceToken(value, "doc-summary", valueDoc?.Get(DocumentationType.Summary, documentation));
                    return value;
                }).ToArray();
                var valuesSection = string.Join(string.Empty, values);

                // class
                var classDoc = documentation.ForClass(enumType);

                var model = templates.Get(TemplateType.EnumsEnumClass);
                model = ReplaceToken(model, "values", valuesSection);
                model = ReplaceToken(model, "doc-summary", classDoc?.Get(DocumentationType.Summary, documentation));
                model = ReplaceToken(model, "baseType", Enum.GetUnderlyingType(enumType), null);
                model = ReplaceToken(model, "className", enumType.Name);

                return model;
            });
            var enumsSection = string.Join(string.Empty, enums);


            // file
            var file = templates.Get(TemplateType.EnumsClass);
            file = ReplaceToken(file, "enums", enumsSection);
            file = ResolveCustomPredicates(file);


            // filename
            var filename = templates.Get(TemplateType.EnumsClassFilename);
            filename = ReplaceToken(filename, "enums", enumsSection);
            filename = ResolveCustomPredicates(filename);

            return new GeneratedFile(filename, file);
        }

        private GeneratedFile GenerateEnumFile(Type enumType)
        {
            // values
            var values = Enum.GetValues(enumType).Cast<object>().Select(v =>
            {
                var name = Enum.GetName(enumType, v);
                var valueDoc = documentation.ForField(enumType, name);

                var value = templates.Get(TemplateType.EnumClassValue);
                value = ReplaceToken(value, "valueName", name);
                value = ReplaceToken(value, "valueValue", Convert.ChangeType(v, Enum.GetUnderlyingType(enumType)).ToString());
                value = ReplaceToken(value, "doc-summary", valueDoc?.Get(DocumentationType.Summary, documentation));
                return value;
            }).ToArray();
            var valuesSection = string.Join(string.Empty, values);

            // class
            var classDoc = documentation.ForClass(enumType);

            var model = templates.Get(TemplateType.EnumClass);
            model = ReplaceToken(model, "values", valuesSection);
            model = ReplaceToken(model, "doc-summary", classDoc?.Get(DocumentationType.Summary, documentation));
            model = ReplaceToken(model, "baseType", Enum.GetUnderlyingType(enumType), null);
            model = ReplaceToken(model, "className", enumType.Name);
            model = ResolveCustomPredicates(model);

            // filename
            var filename = templates.Get(TemplateType.EnumClassFilename);
            filename = ReplaceToken(filename, "values", valuesSection);
            filename = ReplaceToken(filename, "doc-summary", classDoc?.Get(DocumentationType.Summary, documentation));
            filename = ReplaceToken(filename, "baseType", Enum.GetUnderlyingType(enumType), null);
            filename = ReplaceToken(filename, "className", enumType.Name);
            filename = ResolveCustomPredicates(filename);

            return new GeneratedFile(filename, model);
        }

        private GeneratedFile[] GenerateEnumsTranslationFiles(Type[] enumTypes)
        {
            var originalCultureInfo = Thread.CurrentThread.CurrentUICulture;

            try
            {
                if (!arguments.Silent) Console.WriteLine("creating translation files for enums..");
                return languages.SelectMany(language =>
                {
                    var cultureInfo = new CultureInfo(language ?? "en-US");
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                    if (!arguments.Silent) Console.WriteLine($"  creating translation files for {cultureInfo.DisplayName} ({cultureInfo.Name})..");
                    // values
                    var enumsSections = enumTypes.Select(enumType =>
                    {
                        if (!arguments.Silent) Console.WriteLine($"    creating translation enum {enumType.FullName}..");
                        var valuesSections = Enum.GetValues(enumType).Cast<object>().Select(v =>
                        {
                            var name = Enum.GetName(enumType, v);
                            if (!arguments.Silent) Console.Write($"      creating translation value {name}: ");

                            var translation = translationResourceManagers
                                                  .Select(r => r.GetString($"{enumType.FullName?.Replace(".", "_")}_{name}",
                                                      cultureInfo))
                                                  .FirstOrDefault(x => x != null)
                                              ?? throw new Exception(
                                                  $"Could not find {language ?? "default"} translation for {enumType.FullName}.{name}.");
                            if (!arguments.Silent) Console.WriteLine($"'{translation}'..");

                            var valueSection = templates.Get(TemplateType.EnumsTranslationLanguageValue);
                            valueSection = ResolvePredicate(valueSection, "defaultLanguage", language == null);
                            valueSection = ReplaceToken(valueSection, "className", enumType.Name);
                            valueSection = ReplaceToken(valueSection, "valueName", name);
                            valueSection = ReplaceToken(valueSection, "translation", translation);
                            return ExtractSections(valueSection);
                        }).ToArray();
                        var valuesSection = JoinSections(x => string.Empty, valuesSections);

                        var enumSection = templates.Get(TemplateType.EnumsTranslationLanguageEnum);
                        enumSection = ReplaceToken(enumSection, "className", enumType.Name);
                        enumSection = ResolvePredicate(enumSection, "defaultLanguage", language == null);
                        enumSection = ReplaceSections(enumSection, "values", valuesSection);
                        return ExtractSections(enumSection);
                    }).ToArray();
                    var enumsSection = JoinSections(x => string.Empty, enumsSections);

                    var modelSection = templates.Get(TemplateType.EnumsTranslationLanguage);
                    modelSection = ResolvePredicate(modelSection, "defaultLanguage", language == null);
                    modelSection = ReplaceSections(modelSection, "enums", enumsSection);
                    var modelSections = ExtractSections(modelSection);


                    var filenameSection = templates.Get(TemplateType.EnumsTranslationLanguageFilename);
                    filenameSection = ResolvePredicate(filenameSection, "defaultLanguage", language == null);
                    filenameSection = ReplaceToken(filenameSection, "language", language);
                    var filenameSections = ExtractSections(filenameSection);

                    return filenameSections.Select(kvp =>
                    {
                        // filename
                        var fileContent = modelSections[kvp.Key];
                        fileContent = ResolveCustomPredicates(fileContent);
                        return new GeneratedFile(kvp.Value, fileContent);
                    });
                }).ToArray();
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = originalCultureInfo;
            }
        }

        private GeneratedFile GenerateInterfaceVersion()
        {
            // interface version
            var interfaceVersionFile = templates.Get(TemplateType.InterfaceVersion);
            interfaceVersionFile = ReplaceToken(interfaceVersionFile, "version", interfaceVersion.ToString());
            interfaceVersionFile = ResolveCustomPredicates(interfaceVersionFile);

            // filename
            var filename = templates.Get(TemplateType.InterfaceVersionFilename);
            filename = ResolveCustomPredicates(filename);

            return new GeneratedFile(filename, interfaceVersionFile);
        }

        private GeneratedFile GenerateRouteFile(ModelClassMetaData[] resources)
        {
            // routes
            var routes = resources.Select(resource =>
            {
                var route = templates.Get(TemplateType.RoutesClassRoute);
                route = ReplaceToken(route, "entity", resource.Instance.ResourceType.FromKebabCase());
                route = ReplaceToken(route, "route", resource.Instance.UrlPath.Trim('/', '\\'));
                return route;
            }).ToArray();
            var routesSection = string.Join(string.Empty, routes);

            // file
            var routeFile = templates.Get(TemplateType.RoutesClass);
            routeFile = ReplaceToken(routeFile, "routes", routesSection);
            routeFile = ResolveCustomPredicates(routeFile);

            // filename
            var filename = templates.Get(TemplateType.RoutesClassFilename);
            filename = ReplaceToken(filename, "routes", routesSection);
            filename = ResolveCustomPredicates(filename);

            return new GeneratedFile(filename, routeFile);
        }

        private string ResolveCustomPredicates(string content)
        {
            if (customPredicates == null)
                return content;

            foreach (var customPredicate in customPredicates)
                content = ResolvePredicate(content, customPredicate.Key, customPredicate.Value);

            return content;
        }

        private static Dictionary<string, string> ExtractSections(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new Dictionary<string, string>();

            var regexStart = new Regex(@"(\r\n|\r|\n)?[#]START[(](?<section>[a-zA-Z]+)[)](\r\n|\r|\n)?");
            var regexEnd = new Regex(@"(\r\n|\r|\n)?[#]END[(](?<section>[a-zA-Z]+)[)](\r\n|\r|\n)?");

            var starts = regexStart.Matches(content).Cast<Match>().ToDictionary(m => m.Groups["section"].Value, m => m.Index + m.Length);
            var ends = regexEnd.Matches(content).Cast<Match>().ToDictionary(m => m.Groups["section"].Value, m => m.Index);

            return starts.ToDictionary(kvp => kvp.Key, kvp => content.Substring(kvp.Value, ends.TryGetValue(kvp.Key, out var end) ? (end - kvp.Value) : throw  new Exception($"Could not find the end-section to {kvp.Key} in {content}")));
        }
        private static Dictionary<string, string> JoinSections(Func<string,string> separator, Dictionary<string,string>[] sections)
        {
            return !sections.Any() 
                ? new Dictionary<string, string>() 
                : sections.Aggregate((a, b) => a.ToDictionary(x => x.Key, x =>
                {
                    if (!b.TryGetValue(x.Key, out var bValue))
                        throw new Exception($"Section {x.Key} not found in ({string.Join(",", b.Keys)}).");

                    if (!b.TryGetValue(x.Key + "JOIN", out var bSeparator))
                        bSeparator = separator(x.Key);
                    return string.Join(bSeparator, x.Value, bValue);
                }));
        }
        private static string ReplaceSections(string content, string token, Dictionary<string, string> sections)
        {
            foreach (var section in sections)
                content = ReplaceToken(content, $"{token}[{section.Key}]", section.Value);

            var regex = new Regex("[{]" + Regex.Escape(token) + "[[][a-zA-Z]+[]]([:](?<format>[-/_.*a-zA-Z0-9() ]+))?[}]");
            while (true)
            {
                var match = regex.Match(content);

                if (!match.Success)
                    break;

                content = content.Replace(match.Value, string.Empty);
            }

            return content;
        }
        private static string ReplaceToken(string content, string token, Type type, Action<Type> foundType)
        {
            return ReplaceTokenRaw(content, token, type, (v, format) =>
            {
                if (format == null)
                    return v.FullName;

                if (format.ToLower().Trim().Equals("jstype"))
                    return v.ToJsType(foundType);
                if (format.ToLower().Trim().Equals("cstype"))
                    return v.ToCsType(foundType);

                throw new Exception($"Format '{format}' not supported.");
            });
        }
        private static string ReplaceToken(string content, string token, string value)
        {
            return ReplaceTokenRaw(content, token, value ?? string.Empty, (v, format) =>
            {
                if (format == null)
                    return v;

                if (format.ToLower().Trim().Equals("camelcase"))
                    return v.ToCamelCase();
                if (format.ToLower().Trim().Equals("kebabcase"))
                    return v.ToKebabCase();
                if (format.StartsWith("pad(") && format.EndsWith(")"))
                    return string.Join(Environment.NewLine,
                        v.Replace("\r", string.Empty)
                            .Split('\n')
                            .Select(line =>
                                $"{format.Substring("pad(".Length, format.Length - "pad(".Length - ")".Length)}{line}"));

                throw new Exception($"Format '{format}' not supported.");
            });
        }
        private static string ReplaceTokenRaw<T>(string content, string token, T value, Func<T,string,string> formatter)
        {
            var regex = new Regex("[{]" + Regex.Escape(token) + "([:](?<format>[-/_.*a-zA-Z0-9() ]+))?[}]");
            content = content ?? string.Empty;

            while (true)
            {
                var match = regex.Match(content);

                if (!match.Success)
                    break;

                var formatGroup = match.Groups["format"];
                content = content.Replace(match.Value, formatter(value, formatGroup.Success ? formatGroup.Value : null));
            }

            return content;
        }
        private static string ResolvePredicate(string content, string token, bool value)
        {
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(token))
                return content;

            var regexStart = new Regex($@"(\r\n|\r|\n)?[#]IF[(](?<negate>!)?{Regex.Escape(token)}[)][#](\r\n|\r|\n)?");
            var regexEnd = new Regex($@"(\r\n|\r|\n)?[#]FI[(](?<negate>!)?{Regex.Escape(token)}[)][#]");

            while (true)
            {
                var start = regexStart.Match(content);
                if (!start.Success)
                    break;

                var end = regexEnd.Match(content, start.Index + start.Length);
                if (!end.Success)
                    throw new Exception($"Could not find if-end-tag for {start.Value}");

                var body = string.Empty;
                if (value == !start.Groups["negate"].Success)
                    body = content.Substring(start.Index + start.Length, end.Index - (start.Index + start.Length));

                content = content
                    .Remove(start.Index, (end.Index + end.Length) - start.Index)
                    .Insert(start.Index, body);
            }

            return content;
        }

        private static string RemoveTypesFromRoute(string route)
        {
            var regex = new Regex(@"[{](?<name>[a-zA-Z0-9._]+)[:][a-zA-Z0-9._]+[}]");
            return regex.Replace(route, "{${name}}");
        }
    }
}
