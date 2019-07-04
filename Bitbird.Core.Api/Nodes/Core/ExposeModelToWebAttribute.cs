using System;
using JetBrains.Annotations;

namespace Bitbird.Core.Api.Nodes.Core
{
    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
    public class ExposeModelToWebAttribute : Attribute
    {
    }
}