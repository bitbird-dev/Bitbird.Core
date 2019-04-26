using System;
using System.Linq.Expressions;

namespace Bitbird.Core.Data.Net
{
    /// <summary>
    /// Used to resolve permissions during data requests.
    /// This interface defines methods to handle the access of data (types, records, properties).
    /// If an expression is returned, the expression must translate to SQL.
    ///
    /// Members of this interface are not supposed to be called directly, but through <see cref="PermissionResolverHelper"/>.
    /// For details about this, see the documentation of <see cref="PermissionResolverHelper"/>.
    ///
    /// This interface should be implemented by the api layer to define permissions on a low-level.
    /// Other, more high-level permission concepts take place in the api layer.
    ///
    /// In general, if a variable of this type is null, a system execution is assumed, and therefore all permissions should be granted.
    /// </summary>
    public interface IPermissionResolver
    {
        /// <summary>
        /// Don't access this method directly, but rather use the <see cref="PermissionResolverHelper"/> class.
        /// 
        /// Checks whether the passed type may be accessed or not.
        /// </summary>
        /// <param name="accessType">Defines the type of access that is requested. See <see cref="AccessType"/>.</param>
        /// <param name="type">The entity type that an operation is requested on.</param>
        /// <returns>True if access is granted.</returns>
        [Obsolete("Don't access this method directly, but rather use the PermissionResolverHelper class.")]
        bool CanAccessType(AccessType accessType, Type type);

        /// <summary>
        /// Don't access this method directly, but rather use the <see cref="PermissionResolverHelper"/> class.
        /// 
        /// Checks whether the passed record may be accessed or not.
        /// The record has to have the correct entity type or the test will fail.
        /// </summary>
        /// <param name="accessType">Defines the type of access that is requested. See <see cref="AccessType"/>.</param>
        /// <param name="record">A record that an operation is requested on.</param>
        /// <returns>True if access is granted.</returns>
        [Obsolete("Don't access this method directly, but rather use the PermissionResolverHelper class.")]
        bool CanAccessRecord(AccessType accessType, object record);

        /// <summary>
        /// Don't access this method directly, but rather use the <see cref="PermissionResolverHelper"/> class.
        /// 
        /// Returns an expression that checks whether the passed record may be accessed or not.
        /// This expression must be translatable to SQL.
        /// So this expression can be used for access restriction directly in database queries.
        /// </summary>
        /// <typeparam name="T">The type for the record that the expression should work on. Must be a valid entity type or the expression will return false.</typeparam>
        /// <param name="accessType">Defines the type of access that is requested. See <see cref="AccessType"/>.</param>
        /// <returns>True if access is granted.</returns>
        [Obsolete("Don't access this method directly, but rather use the PermissionResolverHelper class.")]
        Expression<Func<T,bool>> GetCanAccessRecordExpression<T>(AccessType accessType);
    }
}