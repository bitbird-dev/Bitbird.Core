using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Ide.Tools.Api.CliTools
{
    public class ApiNodeMethodInfo
    {
        [NotNull, UsedImplicitly] public string MethodName { get; }
        [NotNull, UsedImplicitly] public string MethodText { get; }
        [NotNull, UsedImplicitly] public string MethodTextAsKebabCase { get; }
        [UsedImplicitly] public bool IsAsync { get; }
        [CanBeNull, UsedImplicitly] public string ReturnTypeAsCsType { get; }
        [CanBeNull, UsedImplicitly] public ApiNodeMethodBodyParameterInfo BodyParameter { get; }
        [CanBeNull, UsedImplicitly] public ApiNodeMethodRouteParameterInfo RouteParameter { get; }

        public ApiNodeMethodInfo(
            [NotNull] string methodName,
            [NotNull] string methodText,
            [NotNull] string methodTextAsKebabCase, 
            bool isAsync,
            [CanBeNull] string returnTypeAsCsType, 
            [CanBeNull] ApiNodeMethodBodyParameterInfo bodyParameter,
            [CanBeNull] ApiNodeMethodRouteParameterInfo routeParameter)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            MethodText = methodText ?? throw new ArgumentNullException(nameof(methodText));
            MethodTextAsKebabCase = methodTextAsKebabCase ?? throw new ArgumentNullException(nameof(methodTextAsKebabCase));
            IsAsync = isAsync;
            ReturnTypeAsCsType = returnTypeAsCsType;
            BodyParameter = bodyParameter;
            RouteParameter = routeParameter;
        }
    }
}