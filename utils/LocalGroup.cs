    /// Describes a Local Group.
    /// Objects of this type are provided to and returned from group-related Cmdlets.
    public class LocalGroup : LocalPrincipal
        /// A short description of the Group.
        #region Construction
        /// Initializes a new LocalGroup object.
        public LocalGroup()
            ObjectClass = Strings.ObjectClassGroup;
        /// Initializes a new LocalUser object with the specified name.
        /// <param name="name">Name of the new LocalGroup.</param>
        public LocalGroup(string name)
        /// Construct a new LocalGroup object that is a copy of another.
        /// <param name="other"></param>
        private LocalGroup(LocalGroup other)
          : this(other.Name)
            Description = other.Description;
        #endregion Construction
        /// Provides a string representation of the LocalGroup object.
        /// A string containing the Group Name.
            return Name ?? SID.ToString();
        /// Create a copy of a LocalGroup object.
        /// A new LocalGroup object with the same property values as this one.
        public LocalGroup Clone()
            return new LocalGroup(this);
