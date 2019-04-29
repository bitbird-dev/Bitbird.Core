using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bitbird.Core.Data.Net;
using Bitbird.Core.Expressions;

namespace Bitbird.Core.Api.Net.Core
{
    public class TypePermissions<TSession>
    {
        private readonly Dictionary<AccessType, List<PermissionCondition<TSession>>> accessTypeConditions = new Dictionary<AccessType, List<PermissionCondition<TSession>>>();
        public void AddPermission(AccessType combinedAccessType, PermissionCondition<TSession> condition)
        {
            foreach (var accessType in SplitAccessTypes(combinedAccessType))
            {
                if (!accessTypeConditions.TryGetValue(accessType, out var atc))
                    accessTypeConditions.Add(accessType, atc = new List<PermissionCondition<TSession>>());

                atc.Add(condition);
            }
        }

        public void AddPermissions(TypePermissions<TSession> tp)
        {
            foreach (var atc in tp.accessTypeConditions)
            {
                if (accessTypeConditions.TryGetValue(atc.Key, out var currentAtc))
                {
                    if (currentAtc.Count == 1 && currentAtc.First() == null)
                        continue;

                    if (atc.Value.Count == 1 && atc.Value.First() == null)
                        currentAtc.Clear();

                    currentAtc.AddRange(atc.Value);
                }
                else
                    accessTypeConditions.Add(atc.Key, new List<PermissionCondition<TSession>>(atc.Value));
            }
        }

        public bool HasAccess(AccessType accessType, object record, TSession session)
        {
            if (accessTypeConditions.TryGetValue(accessType, out var atc))
            {
                return atc.Any(c => c == null || c.Func(record, session));
            }

            return false;
        }

        public Expression<Func<T, bool>> GetHasAccessExpression<T>(AccessType accessType, TSession session)
        {
            if (!accessTypeConditions.TryGetValue(accessType, out var atc) || !atc.Any())
                return t => false;

            if (atc.Any(c => c == null))
                return null;

            return atc.Select(c => c.CreateExpression(session)).ComposeWithAnd<Func<T,bool>>();
        }

        public override string ToString()
        {
            return accessTypeConditions.Select(t => $"    {{ {t.Key}: {(t.Value.Any(v => v == null) ? "unconditional" : "conditional")}({t.Value.Count}) }}").Aggregate((a, b) => $"{a},\n{b}");
        }

        private static IEnumerable<AccessType> SplitAccessTypes(AccessType combinedAccessType)
        {
            return Enum.GetValues(typeof(AccessType)).Cast<AccessType>().Where(t => (combinedAccessType & t) == t).ToArray();
        }
        public TypePermissions<TSession> Clone()
        {
            var copy = new TypePermissions<TSession>();
            foreach (var atc in accessTypeConditions)
                copy.accessTypeConditions.Add(atc.Key, new List<PermissionCondition<TSession>>(atc.Value));
            return copy;
        }
    }
}