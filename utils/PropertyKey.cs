    /// Defines a unique key for a Shell Property.
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal readonly struct PropertyKey : IEquatable<PropertyKey>
        /// A unique GUID for the property.
        public Guid FormatId { get; }
        /// Property identifier (PID)
        public int PropertyId { get; }
        #region Public Construction
        /// PropertyKey Constructor.
        /// <param name="formatId">A unique GUID for the property.</param>
        /// <param name="propertyId">Property identifier (PID).</param>
        internal PropertyKey(Guid formatId, int propertyId)
            this.FormatId = formatId;
            this.PropertyId = propertyId;
        #region IEquatable<PropertyKey> Members
        /// Returns whether this object is equal to another. This is vital for performance of value types.
        /// <param name="other">The object to compare against.</param>
        /// <returns>Equality result.</returns>
        public bool Equals(PropertyKey other)
            return other.Equals((object)this);
        #region equality and hashing
        /// Returns the hash code of the object. This is vital for performance of value types.
            return FormatId.GetHashCode() ^ PropertyId;
        /// <param name="obj">The object to compare against.</param>
        public override bool Equals(object obj)
            if (obj is not PropertyKey)
            PropertyKey other = (PropertyKey)obj;
            return other.FormatId.Equals(FormatId) && (other.PropertyId == PropertyId);
        /// Implements the == (equality) operator.
        /// <param name="propKey1">First property key to compare.</param>
        /// <param name="propKey2">Second property key to compare.</param>
        /// <returns>True if object a equals object b. false otherwise.</returns>
        public static bool operator ==(PropertyKey propKey1, PropertyKey propKey2)
            return propKey1.Equals(propKey2);
        /// Implements the != (inequality) operator.
        /// <returns>True if object a does not equal object b. false otherwise.</returns>
        public static bool operator !=(PropertyKey propKey1, PropertyKey propKey2)
            return !propKey1.Equals(propKey2);
        /// Override ToString() to provide a user friendly string representation.
        /// <returns>String representing the property key.</returns>
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "PropertyKeyFormatString",
                FormatId.ToString("B"), PropertyId);
