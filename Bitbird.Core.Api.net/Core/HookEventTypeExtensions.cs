using System;
using Bitbird.Core.Api.Net.EntityChanges;
using Bitbird.Core.Data.Net.DbContext.Hooks;

namespace Bitbird.Core.Api.Net.Core
{
    public static class HookEventTypeExtensions
    {
        public static EntityChangeType ToEntityChangeType(this HookEventType type)
        {
            switch (type)
            {
                case HookEventType.Insert:
                    return EntityChangeType.Created;
                case HookEventType.Delete:
                    return EntityChangeType.Deleted;
                case HookEventType.Update:
                    return EntityChangeType.Updated;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}