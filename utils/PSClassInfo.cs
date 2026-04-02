    /// Contains a PS Class information.
    public sealed class PSClassInfo
        /// Initializes a new instance of the PSClassInfo class.
        /// <param name="name">Name of the PS Class.</param>
        internal PSClassInfo(string name)
        /// Name of the class.
        /// Collection of members of the class.
        public ReadOnlyCollection<PSClassMemberInfo> Members { get; private set; }
        /// Updates members of the class.
        /// <param name="members">Updated members.</param>
        public void UpdateMembers(IList<PSClassMemberInfo> members)
            if (members != null)
                this.Members = new ReadOnlyCollection<PSClassMemberInfo>(members);
        /// Module in which the class is implemented in.
    /// Contains a class field information.
    public sealed class PSClassMemberInfo
        /// Initializes a new instance of the PSClassMemberInfo class.
        internal PSClassMemberInfo(string name, string memberType, string defaultValue)
            ArgumentException.ThrowIfNullOrEmpty(name);
            this.TypeName = memberType;
            this.DefaultValue = defaultValue;
        /// Gets or sets name of the member.
        /// Gets or sets type of the member.
        /// Default value of the Field.
        public string DefaultValue { get; }
