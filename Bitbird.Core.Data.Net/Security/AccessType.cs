using System;

namespace Bitbird.Core.Data.Net
{
    /// <summary>
    /// Defines the type of access that is requested for a type/record/property.
    /// Used when resolving permissions.
    /// Can be used as flags (some members define a flag, e.g. <see cref="Read"/>, others combine previously defined members, e.g. <see cref="Crud"/>).
    /// </summary>
    [Flags]
    public enum AccessType : byte
    {
        /// <summary>
        /// Defined for types, records, properties.
        /// </summary>
        Read = 1,
        /// <summary>
        /// Defined for types, records, properties.
        /// </summary>
        Update = 2,
        /// <summary>
        /// Defined for types.
        /// </summary>
        Create = 4,
        /// <summary>
        /// Defined for types, records.
        /// </summary>
        Delete = 8,

        /// <summary>
        /// Defines a combination of
        /// - <see cref="Read"/>
        /// - <see cref="Update"/>
        /// - <see cref="Create"/>
        /// - <see cref="Delete"/>
        /// </summary>
        Crud = Read | Update | Create | Delete
    }
}