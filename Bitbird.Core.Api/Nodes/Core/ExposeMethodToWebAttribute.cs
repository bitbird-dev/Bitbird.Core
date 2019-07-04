using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Nodes.Core
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Method)]
    public class ExposeMethodToWebAttribute : Attribute
    {
    }
}
