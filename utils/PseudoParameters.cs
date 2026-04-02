    /// Represents a parameter declaration that can be constructed at runtime.
    /// Instances of <see cref="RuntimeDefinedParameterDictionary"/>
    /// should be returned to cmdlet implementations of
    /// <see cref="IDynamicParameters.GetDynamicParameters"/>.
    /// It is permitted to subclass <see cref="RuntimeDefinedParameter"/>
    /// <seealso cref="IDynamicParameters"/>
    /// <seealso cref="IDynamicParameters.GetDynamicParameters"/>
    public class RuntimeDefinedParameter
        /// Constructs a runtime-defined parameter instance.
        public RuntimeDefinedParameter()
        /// Constructs a new instance of a runtime-defined parameter using the specified parameters.
        /// The name of the parameter. This cannot be null or empty.
        /// <param name="parameterType">
        /// The type of the parameter value. Arguments will be coerced to this type before binding.
        /// This parameter cannot be null.
        /// <param name="attributes">
        /// Any parameter attributes that should be on the parameter. This can be any of the
        /// parameter attributes including but not limited to Validate*Attribute, ExpandWildcardAttribute, etc.
        /// If <paramref name="parameterType"/> is null.
        public RuntimeDefinedParameter(string name, Type parameterType, Collection<Attribute> attributes)
                throw PSTraceSource.NewArgumentNullException(nameof(parameterType));
            _parameterType = parameterType;
            if (attributes != null)
                Attributes = attributes;
        /// Gets or sets the name of the parameter.
        /// If <paramref name="value"/> is null or empty on set.
        private string _name = string.Empty;
        /// Gets or sets the type of the parameter.
        /// Arguments will be coerced to this type before being bound.
        /// If <paramref name="value"/> is null.
        public Type ParameterType
                return _parameterType;
                _parameterType = value;
        private Type _parameterType;
        /// Gets or sets the value of the parameter.
        /// If the value is set prior to parameter binding, the value will be
        /// reset before each pipeline object is processed.
                this.IsSet = true;
        /// Gets or sets whether this parameter value has been set.
        public bool IsSet { get; set; }
        /// Gets or sets the attribute collection that describes the parameter.
        /// This can be any attribute that can be applied to a normal parameter.
        public Collection<Attribute> Attributes { get; } = new Collection<Attribute>();
        /// Check if the parameter is disabled due to the associated experimental feature.
        internal bool IsDisabled()
            bool hasParameterAttribute = false;
            bool hasEnabledParamAttribute = false;
            bool hasSeenExpAttribute = false;
            foreach (Attribute attr in Attributes)
                if (!hasSeenExpAttribute && attr is ExperimentalAttribute expAttribute)
                    if (expAttribute.ToHide)
                    hasSeenExpAttribute = true;
                else if (attr is ParameterAttribute paramAttribute)
                    hasParameterAttribute = true;
                    if (paramAttribute.ToHide)
                    hasEnabledParamAttribute = true;
            // If one or more parameter attributes are declared but none is enabled,
            // then we consider the parameter is disabled.
            return hasParameterAttribute && !hasEnabledParamAttribute;
    /// Represents a collection of runtime-defined parameters that are keyed based on the name
    /// of the parameter.
    /// It is permitted to subclass <see cref="RuntimeDefinedParameterDictionary"/>
    public class RuntimeDefinedParameterDictionary : Dictionary<string, RuntimeDefinedParameter>
        /// Constructs a new instance of a runtime-defined parameter dictionary.
        public RuntimeDefinedParameterDictionary()
        /// Gets or sets the help file that documents these parameters.
            get { return _helpFile; }
            set { _helpFile = string.IsNullOrEmpty(value) ? string.Empty : value; }
        /// Gets or sets private data associated with the runtime-defined parameters.
        public object Data { get; set; }
        internal static readonly RuntimeDefinedParameter[] EmptyParameterArray = Array.Empty<RuntimeDefinedParameter>();
