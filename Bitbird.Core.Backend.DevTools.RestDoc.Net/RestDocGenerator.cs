using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Bitbird.Core.Data.Query;
using Bitbird.Core.Query;
using Bitbird.Core.WebApi.Controllers;
using Bitbird.Core.WebApi.JsonApi;
using Bitbird.Core.WebApi.Models;

namespace Bitbird.Core.Backend.DevTools.RestDoc.Net
{
    public static class RestDocGenerator
    {
        public static void Export(params Type[] assemblyTypes)
        {
            var controllers = assemblyTypes.Select(Assembly.GetAssembly)
                .SelectMany(a => a.GetTypes())
                .Select(t => new
                {
                    Type = t,
                    IsController = typeof(IControllerBase).IsAssignableFrom(t)
                })
                .Where(t => t.IsController)
                .ToArray();

            var tocs = new Dictionary<string, StringBuilder>();

            string outputPath;
            foreach (var controller in controllers.OrderBy(c => c.Type.GetCustomAttribute<ObsoleteAttribute>() != null).ThenBy(c => c.Type.Name))
            {
                var friendlyName = controller.Type.Name;
                if (friendlyName.EndsWith("Controller"))
                    friendlyName = friendlyName.Substring(0, friendlyName.Length - "Controller".Length);

                var isObsolete = controller.Type.GetCustomAttribute<ObsoleteAttribute>() != null;

                var category = controller.Type.Namespace?.Split('.').Last() ?? "General";

                if (!tocs.TryGetValue(category, out var toc))
                    tocs[category] = toc = new StringBuilder();

                var sb = new StringBuilder();
                sb.AppendLine($"# {(isObsolete ? "\\[OBSOLETE\\] " : string.Empty)}{friendlyName} Controller");
                sb.AppendLine($"Category: {category}");
                sb.AppendLine();
                sb.AppendLine($"For detailed information about the implementation of this controller, have a look at the [Api Reference](xref:{controller.Type.Namespace}.{controller.Type.Name}).");
                sb.AppendLine();
                sb.AppendLine("For detailed information about controllers in general, see the [Rest Controller](../index.md) documentation or have a look at the [Interface](../../index.md) documentation.");
                sb.AppendLine();
                sb.AppendLine("## Supported Actions");
                sb.AppendLine();
                sb.AppendLine("|Cmd|Route|Supports Query?|Body|Returns|");
                sb.AppendLine("| --- | --- |:---:| --- | --- |");

                var controllerRoute = controller.Type.GetCustomAttribute<RoutePrefixAttribute>();
                if (controllerRoute == null)
                    continue;

                var methodInfos = controller.Type.GetMethods().Select(method =>
                {
                    var actionRoute = method.GetCustomAttribute<RouteAttribute>();
                    if (actionRoute == null)
                        return null;
                    if (IsSubclassOfRawGeneric(typeof(IReadControllerBase), method.DeclaringType))
                        return null;

                    var cmd = "GET";
                    if (method.GetCustomAttribute<HttpGetAttribute>() != null)
                        cmd = "GET";
                    if (method.GetCustomAttribute<HttpPostAttribute>() != null)
                        cmd = "POST";
                    if (method.GetCustomAttribute<HttpPutAttribute>() != null)
                        cmd = "PUT";
                    if (method.GetCustomAttribute<HttpPatchAttribute>() != null)
                        cmd = "PATCH";
                    if (method.GetCustomAttribute<HttpDeleteAttribute>() != null)
                        cmd = "DELETE";
                    if (method.GetCustomAttribute<HttpOptionsAttribute>() != null)
                        cmd = "OPTIONS";

                    var route =
                        $"{controllerRoute.Prefix}{(string.IsNullOrWhiteSpace(actionRoute.Template) ? string.Empty : "/")}{actionRoute.Template}";

                    var queryParams = false;
                    string param = null;
                    var returns = FormatReturnType(method.ReturnType);

                    foreach (var parameter in method.GetParameters())
                    {
                        if (parameter.ParameterType == typeof(QueryInfo))
                        {
                            queryParams = true;
                            continue;
                        }

                        var fromBody = parameter.GetCustomAttribute<FromBodyAttribute>();
                        if (fromBody == null)
                            continue;

                        param = FormatParameterType(parameter.ParameterType);
                    }

                    return new ControllerMethodInfo(route, queryParams, param, returns, cmd);
                });

                var controllerInstance = Activator.CreateInstance(controller.Type);
                var readControllerInstance = controllerInstance as IReadControllerBase;

                methodInfos = methodInfos.Concat(controller.Type.GetMethods().SelectMany(method =>
                {
                    var actionRoute = method.GetCustomAttribute<RouteAttribute>();
                    if (actionRoute == null)
                        return new ControllerMethodInfo[0];
                    if (!IsSubclassOfRawGeneric(typeof(IReadControllerBase), method.DeclaringType))
                        return new ControllerMethodInfo[0];

                    var cmd = "GET";
                    if (method.GetCustomAttribute<HttpGetAttribute>() != null)
                        cmd = "GET";
                    if (method.GetCustomAttribute<HttpPostAttribute>() != null)
                        cmd = "POST";
                    if (method.GetCustomAttribute<HttpPutAttribute>() != null)
                        cmd = "PUT";
                    if (method.GetCustomAttribute<HttpPatchAttribute>() != null)
                        cmd = "PATCH";
                    if (method.GetCustomAttribute<HttpDeleteAttribute>() != null)
                        cmd = "DELETE";
                    if (method.GetCustomAttribute<HttpOptionsAttribute>() != null)
                        cmd = "OPTIONS";

                    var route =
                        $"{controllerRoute.Prefix}{(string.IsNullOrWhiteSpace(actionRoute.Template) ? string.Empty : "/")}{actionRoute.Template}";


                    string param = null;
                    string returns;
                    if (method.Name == "UpdateRelationAsync" && readControllerInstance is ICrudControllerBase crudControllerBaseU)
                    {
                        var relations = CrudControllerResourceMetaData.Instance.AllForModel(readControllerInstance.ModelType);
                        return relations.Where(r => crudControllerBaseU.CanUpdateRelation(r.Key)).Select(r =>
                        {
                            returns = FormatReturnType(r.Value.IsToMany ? typeof(IdModel[]) : typeof(IdModel));
                            param = FormatReturnType(r.Value.IsToMany ? typeof(IdModel[]) : typeof(IdModel));

                            return new ControllerMethodInfo(route.Replace("{relationName}", r.Key), false, param, returns, cmd);
                        });
                    }
                    if (method.Name == "DeleteRelationAsync" && readControllerInstance is ICrudControllerBase crudControllerBaseD)
                    {
                        var relations = CrudControllerResourceMetaData.Instance.AllForModel(readControllerInstance.ModelType);
                        return relations.Where(r => crudControllerBaseD.CanDeleteRelation(r.Key)).Where(r => r.Value.IsToMany).Select(r =>
                        {
                            returns = FormatReturnType(typeof(IdModel[]));
                            param = FormatReturnType(typeof(IdModel[]));

                            return new ControllerMethodInfo(route.Replace("{relationName}", r.Key), false, param, returns, cmd);
                        });
                    }
                    if (method.Name == "CreateRelationAsync" && readControllerInstance is ICrudControllerBase crudControllerBaseC)
                    {
                        var relations = CrudControllerResourceMetaData.Instance.AllForModel(readControllerInstance.ModelType);
                        return relations.Where(r => crudControllerBaseC.CanCreateRelation(r.Key)).Where(r => r.Value.IsToMany).Select(r =>
                        {
                            returns = FormatReturnType(typeof(IdModel[]));
                            param = FormatReturnType(typeof(IdModel[]));

                            return new ControllerMethodInfo(route.Replace("{relationName}", r.Key), false, param, returns, cmd);
                        });
                    }
                    if (method.Name == "GetRelationAsync" && readControllerInstance != null)
                    {
                        var relations = CrudControllerResourceMetaData.Instance.AllForModel(readControllerInstance.ModelType);
                        return relations.Select(r =>
                        {
                            returns = FormatReturnType(r.Value.IsToMany ? typeof(IdModel[]) : typeof(IdModel));
                            param = string.Empty;

                            return new ControllerMethodInfo(route.Replace("{relationName}", r.Key), false, param, returns, cmd);
                        });
                    }

                    if (method.Name == "CreateAsync" && readControllerInstance is ICrudControllerBase crudControllerBase1 && !crudControllerBase1.CanCreate)
                    {
                        return new ControllerMethodInfo[0];
                    }
                    if (method.Name == "DeleteAsync" && readControllerInstance is ICrudControllerBase crudControllerBase2 && !crudControllerBase2.CanDelete)
                    {
                        return new ControllerMethodInfo[0];
                    }
                    if (method.Name == "UpdateAsync" && readControllerInstance is ICrudControllerBase crudControllerBase3 && !crudControllerBase3.CanUpdate)
                    {
                        return new ControllerMethodInfo[0];
                    }

                    var queryParams = false;
                    returns = FormatReturnType(method.ReturnType);

                    foreach (var parameter in method.GetParameters())
                    {
                        if (parameter.ParameterType == typeof(QueryInfo))
                        {
                            queryParams = true;
                            continue;
                        }

                        var fromBody = parameter.GetCustomAttribute<FromBodyAttribute>();
                        if (fromBody == null)
                            continue;

                        param = FormatParameterType(parameter.ParameterType);
                    }

                    return new[] { new ControllerMethodInfo(route, queryParams, param, returns, cmd) };
                }));

                methodInfos = methodInfos
                    .Where(x => x != null)
                    .OrderBy(x => x.Route)
                    .ThenBy(x => x.Cmd)
                    .ToArray();


                foreach (var methodInfo in methodInfos)
                {
                    sb.AppendLine($"|{methodInfo.Cmd}|{methodInfo.Route}|{methodInfo.QueryParams}|{methodInfo.Parameter}|{methodInfo.Returns}|");
                }
                Debug.WriteLine(sb);

                outputPath = $"..\\..\\..\\BackRohr.Web.Api\\Doc\\Interface\\RestController\\{category}\\{friendlyName}.md";
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new Exception($"Could not find parent directory for '{outputPath}'"));
                File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);

                toc.AppendLine($"- name: {(isObsolete ? "(OBSOLETE) " : string.Empty)}{friendlyName}");
                toc.AppendLine($"  href: {friendlyName}.md");
            }

            var overAllToc = new StringBuilder();

            foreach (var toc in tocs)
            {
                if (string.IsNullOrWhiteSpace(toc.Value.ToString()))
                    continue;

                outputPath = $"..\\..\\..\\BackRohr.Web.Api\\Doc\\Interface\\RestController\\{toc.Key}\\toc.yml";
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new Exception($"Could not find parent directory for '{outputPath}'"));
                File.WriteAllText(outputPath, toc.Value.ToString(), Encoding.UTF8);

                overAllToc.AppendLine($"- name: {toc.Key}");
                overAllToc.AppendLine($"  href: {toc.Key}/toc.yml");
            }

            outputPath = $"..\\..\\..\\BackRohr.Web.Api\\Doc\\Interface\\RestController\\toc.yml";
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new Exception($"Could not find parent directory for '{outputPath}'"));
            File.WriteAllText(outputPath, overAllToc.ToString(), Encoding.UTF8);
        }

        private static string FormatParameterType(Type t)
        {
            var result = string.Empty;

            if (IsSubclassOfRawGeneric(typeof(ContentInfo<>), t))
            {
                t = t.GetGenericArguments()[0];
                result = string.Join(" ", new[] { result, "_partial_" }.Where(s => !string.IsNullOrWhiteSpace(s)));
            }

            var link = t?.Name;
            if (t != null)
            {
                if (t.IsArray)
                {
                    t = t.GetElementType() ?? throw new Exception($"Could not find element type for array type {t.Name}");

                    link = $"[{t.Name}[]](xref:{t.Namespace}.{t.Name})";
                }
                else
                {
                    link = $"[{t.Name}](xref:{t.Namespace}.{t.Name})";
                }
            }

            result = string.Join(" ", new[] { result, link }.Where(s => !string.IsNullOrWhiteSpace(s)));

            return result;
        }
        private static string FormatReturnType(Type t)
        {
            var result = string.Empty;

            do
            {
                if (IsSubclassOfRawGeneric(typeof(Task<>), t))
                {
                    t = t.GetGenericArguments()[0];
                    continue;
                }
                if (IsSubclassOfRawGeneric(typeof(QueryResult<>), t))
                {
                    t = t.GetGenericArguments()[0].MakeArrayType();
                    continue;
                }
                if (IsSubclassOfRawGeneric(typeof(JsonApiOverridePrimaryType<>), t))
                {
                    t = t.GetGenericArguments()[0];
                    continue;
                }
                if (t == typeof(Task))
                {
                    t = null;
                    break;
                }
                if (t == typeof(HttpResponseMessage))
                {
                    result = string.Join(" ", new[] { result, "binary content" }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    t = null;
                }

                break;
            } while (true);

            var link = t?.Name;
            if (t != null)
            {
                if (t.IsArray)
                {
                    t = t.GetElementType() ?? throw new Exception($"Could not find element type for array type {t.Name}");

                    link = $"[{t.Name}[]](xref:{t.Namespace}.{t.Name})";
                }
                else
                {
                    link = $"[{t.Name}](xref:{t.Namespace}.{t.Name})";
                }
            }

            result = string.Join(" ", new[] { result, link }.Where(s => !string.IsNullOrWhiteSpace(s)));

            return result;
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
