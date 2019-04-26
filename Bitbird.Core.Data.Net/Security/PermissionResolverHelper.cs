using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Net
{
    /// <summary>
    /// A helper class that helps working with the <see cref="IPermissionResolver"/> interface.
    ///
    /// For more information about the permission system, see <see cref="IPermissionResolver"/>.
    /// 
    /// Usually the <see cref="IPermissionResolver"/> interface supports these methods as well, but the fact that having a
    /// null reference for an <see cref="IPermissionResolver"/> usually means that the system is working (and therefore all permissions are granted),
    /// can lead to less readable code (since you always have to use null propagation).
    ///
    /// This class manages this constraint for you and allows you to pass null for a <see cref="IPermissionResolver"/>).
    /// </summary>
    public static class PermissionResolverHelper
    {
        // Disable "obsolete"-warnings, since the IPermissionResolver methods are flagged as obsolete (which they are not, but they should not be used directly).
#pragma warning disable 618

        /// <summary>
        /// Checks whether the passed type may be accessed or not.
        /// 
        /// For more information, see the class documentation of <see cref="PermissionResolverHelper"/>.
        /// </summary>
        /// <param name="permissionResolver">A <see cref="IPermissionResolver"/> object that defines permissions. Can be null (for system privileges, i.e. always grant).</param>
        /// <param name="accessType">Defines the type of access that is requested. See <see cref="AccessType"/>.</param>
        /// <param name="type">The entity type that an operation is requested on.</param>
        /// <returns>True if access is granted.</returns>
        public static bool CanAccessType([CanBeNull] IPermissionResolver permissionResolver, AccessType accessType, [NotNull] Type type)
        {
            return permissionResolver?.CanAccessType(accessType, type) ?? true;
        }
        /// <summary>
        /// Checks whether the passed record may be accessed or not.
        /// The record has to have the correct entity type or the test will fail.
        /// 
        /// For more information, see the class documentation of <see cref="PermissionResolverHelper"/>.
        /// </summary>
        /// <param name="permissionResolver">A <see cref="IPermissionResolver"/> object that defines permissions. Can be null (for system privileges, i.e. always grant).</param>
        /// <param name="accessType">Defines the type of access that is requested. See <see cref="AccessType"/>.</param>
        /// <param name="record">A record that an operation is requested on.</param>
        /// <returns>True if access is granted.</returns>
        public static bool CanAccessRecord([CanBeNull] IPermissionResolver permissionResolver, AccessType accessType, [NotNull] object record)
        {
            return permissionResolver?.CanAccessRecord(accessType, record) ?? true;
        }
        /// <summary>
        /// Returns an expression that checks whether the passed record may be accessed or not.
        /// This expression must be translatable to SQL.
        /// So this expression can be used for access restriction directly in database queries.
        /// 
        /// For more information, see the class documentation of <see cref="PermissionResolverHelper"/>.
        /// </summary>
        /// <typeparam name="T">The type for the record that the expression should work on. Must be a valid entity type or the expression will return false.</typeparam>
        /// <param name="permissionResolver">A <see cref="IPermissionResolver"/> object that defines permissions. Can be null (for system privileges, i.e. always grant).</param>
        /// <param name="accessType">Defines the type of access that is requested. See <see cref="AccessType"/>.</param>
        /// <returns>True if access is granted.</returns>
        public static Expression<Func<T, bool>> GetCanAccessRecordExpression<T>([CanBeNull] IPermissionResolver permissionResolver, AccessType accessType)
        {
            return permissionResolver?.GetCanAccessRecordExpression<T>(accessType);
        }
#pragma warning restore 618
    }
}