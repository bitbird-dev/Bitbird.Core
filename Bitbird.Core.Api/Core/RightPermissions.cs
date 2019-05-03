using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bitbird.Core.Data;

namespace Bitbird.Core.Api.Core
{
    public class RightPermissions<TSession>
    {
        private readonly Dictionary<Type, TypePermissions<TSession>> typePermissions = new Dictionary<Type, TypePermissions<TSession>>();

        public void AddPermission<T>(AccessType access, Func<T, TSession, bool> conditionFunc, Func<TSession, Expression<Func<T,bool>>> conditionCreateExpression)
        {
            if (!typePermissions.TryGetValue(typeof(T), out var rp))
                typePermissions.Add(typeof(T), rp = new TypePermissions<TSession>());

            var untypedConditionFunc = conditionFunc != null ? 
                new Func<object, TSession, bool>((record, session) => conditionFunc((T) record, session)) : 
                null;

            rp.AddPermission(access, untypedConditionFunc == null || conditionCreateExpression == null ? null : new PermissionCondition<TSession>(untypedConditionFunc, conditionCreateExpression));
        }
        public void AddPermissions(RightPermissions<TSession> p)
        {
            foreach (var tp in p.typePermissions)
            {
                if (typePermissions.TryGetValue(tp.Key, out var currentTp))
                    currentTp.AddPermissions(tp.Value);
                else
                    typePermissions.Add(tp.Key, tp.Value.Clone());
            }
        }

        public bool HasAccess(Type type, AccessType accessType, object record, TSession session)
        {
            return typePermissions.TryGetValue(type, out var tp) && 
                   tp.HasAccess(accessType, record, session);
        }

        public Expression<Func<T, bool>> GetHasAccessExpression<T>(AccessType accessType, TSession session)
        {
            if (!typePermissions.TryGetValue(typeof(T), out var tp))
                return t => false;

            return tp.GetHasAccessExpression<T>(accessType, session);
        }

        public override string ToString()
        {
            return typePermissions.Select(t => $"  {{ {t.Key.Name}:\n{t.Value}\n  }}").Aggregate((a, b) => $"{a};\n{b}");
        }
    }
}