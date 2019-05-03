using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bitbird.Core.Data;

namespace Bitbird.Core.Api.Core
{
    public class Permissions<TSession, TRightId>
    {
        private readonly Dictionary<TRightId, RightPermissions<TSession>> rightPermissions = new Dictionary<TRightId, RightPermissions<TSession>>();

        public void AddPermission<T>(TRightId rightId, AccessType access, Func<T, TSession, bool> conditionFunc = null, Func<TSession, Expression<Func<T, bool>>> conditionCreateExpression = null)
        {
            if (!rightPermissions.TryGetValue(rightId, out var rp))
                rightPermissions.Add(rightId, rp = new RightPermissions<TSession>());

            rp.AddPermission(access, conditionFunc, conditionCreateExpression);
        }

        private RightPermissions<TSession> ForRight(TRightId userRoleRight)
        {
            return rightPermissions.TryGetValue(userRoleRight, out var p) ? p : null;
        }
        public RightPermissions<TSession> ForRights(IEnumerable<TRightId> userRoleRights)
        {
            var all = userRoleRights
                .Select(ForRight)
                .Where(r => r != null)
                .ToArray();

            return Merge(all);
        }
        private static RightPermissions<TSession> Merge(RightPermissions<TSession>[] all)
        {
            if (all.Length == 0)
                return null;
            if (all.Length == 1)
                return all[0];

            var p = new RightPermissions<TSession>();
            foreach (var rp in all)
                p.AddPermissions(rp);
            return p;
        }
    }
}