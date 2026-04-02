    public class ShowCommandParameterType
        /// Initializes a new instance of the <see cref="ShowCommandParameterType"/> class
        /// with the specified <see cref="Type"/>.
        public ShowCommandParameterType(Type other)
            this.FullName = other.FullName;
            if (other.IsEnum)
                this.EnumValues = new ArrayList(Enum.GetValues(other));
            if (other.IsArray)
                this.ElementType = new ShowCommandParameterType(other.GetElementType());
            object[] attributes = other.GetCustomAttributes(typeof(FlagsAttribute), true);
            this.HasFlagAttribute = attributes.Length != 0;
            this.ImplementsDictionary = typeof(IDictionary).IsAssignableFrom(other);
        public ShowCommandParameterType(PSObject other)
            this.IsEnum = (bool)(other.Members["IsEnum"].Value);
            this.FullName = other.Members["FullName"].Value as string;
            this.IsArray = (bool)(other.Members["IsArray"].Value);
            this.HasFlagAttribute = (bool)(other.Members["HasFlagAttribute"].Value);
            this.ImplementsDictionary = (bool)(other.Members["ImplementsDictionary"].Value);
            if (this.IsArray)
                this.ElementType = new ShowCommandParameterType(other.Members["ElementType"].Value as PSObject);
            if (this.IsEnum)
                this.EnumValues = (other.Members["EnumValues"].Value as PSObject).BaseObject as ArrayList;
        /// The full name of the outermost type.
        public string FullName { get; }
        /// Whether or not this type is an enum.
        public bool IsEnum { get; }
        /// Whether or not this type is an dictionary.
        public bool ImplementsDictionary { get; }
        /// Whether or not this enum has a flag attribute.
        public bool HasFlagAttribute { get; }
        /// Whether or not this type is an array type.
        public bool IsArray { get; }
        /// Gets the inner type, if this corresponds to an array type.
        public ShowCommandParameterType ElementType { get; }
        /// Whether or not this type is a string.
        public bool IsString
                return string.Equals(this.FullName, "System.String", StringComparison.OrdinalIgnoreCase);
        /// Whether or not this type is an script block.
        public bool IsScriptBlock
                return string.Equals(this.FullName, "System.Management.Automation.ScriptBlock", StringComparison.OrdinalIgnoreCase);
        /// Whether or not this type is a bool.
        public bool IsBoolean
        /// Whether or not this type is a switch parameter.
        public bool IsSwitch
                return string.Equals(this.FullName, "System.Management.Automation.SwitchParameter", StringComparison.OrdinalIgnoreCase);
        /// If this is an enum value, return the list of potential values.
        public ArrayList EnumValues { get; }
