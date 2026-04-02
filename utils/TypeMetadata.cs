    /// This class represents the compiled metadata for a parameter set.
    public sealed class ParameterSetMetadata
        private bool _isMandatory;
        private int _position;
        private bool _valueFromPipeline;
        private bool _valueFromPipelineByPropertyName;
        private bool _valueFromRemainingArguments;
        /// <param name="psMD"></param>
        internal ParameterSetMetadata(ParameterSetSpecificMetadata psMD)
            Dbg.Assert(psMD != null, "ParameterSetSpecificMetadata cannot be null");
            Initialize(psMD);
        /// A copy constructor that creates a deep copy of the <paramref name="other"/> ParameterSetMetadata object.
        internal ParameterSetMetadata(ParameterSetMetadata other)
            _helpMessage = other._helpMessage;
            _helpMessageBaseName = other._helpMessageBaseName;
            _helpMessageResourceId = other._helpMessageResourceId;
            _isMandatory = other._isMandatory;
            _position = other._position;
            _valueFromPipeline = other._valueFromPipeline;
            _valueFromPipelineByPropertyName = other._valueFromPipelineByPropertyName;
            _valueFromRemainingArguments = other._valueFromRemainingArguments;
                return _isMandatory;
                _isMandatory = value;
        public int Position
                return _position;
                _position = value;
                return _valueFromPipeline;
                _valueFromPipeline = value;
        /// Specifies that this parameter can take values from a property from the incoming
                return _valueFromPipelineByPropertyName;
                _valueFromPipelineByPropertyName = value;
        /// Specifies if this parameter takes all the remaining unbound
        /// arguments that were specified.
        public bool ValueFromRemainingArguments
                return _valueFromRemainingArguments;
                _valueFromRemainingArguments = value;
                return _helpMessage;
                return _helpMessageBaseName;
                return _helpMessageResourceId;
        #region Private / Internal Methods & Properties
        internal void Initialize(ParameterSetSpecificMetadata psMD)
            _isMandatory = psMD.IsMandatory;
            _position = psMD.Position;
            _valueFromPipeline = psMD.ValueFromPipeline;
            _valueFromPipelineByPropertyName = psMD.ValueFromPipelineByPropertyName;
            _valueFromRemainingArguments = psMD.ValueFromRemainingArguments;
            _helpMessage = psMD.HelpMessage;
            _helpMessageBaseName = psMD.HelpMessageBaseName;
            _helpMessageResourceId = psMD.HelpMessageResourceId;
        /// Compares this instance with the supplied <paramref name="second"/>.
        /// An object to compare this instance with
        /// true if the metadata is same. false otherwise.
        internal bool Equals(ParameterSetMetadata second)
            if ((_isMandatory != second._isMandatory) ||
                (_position != second._position) ||
                (_valueFromPipeline != second._valueFromPipeline) ||
                (_valueFromPipelineByPropertyName != second._valueFromPipelineByPropertyName) ||
                (_valueFromRemainingArguments != second._valueFromRemainingArguments) ||
                (_helpMessage != second._helpMessage) ||
                (_helpMessageBaseName != second._helpMessageBaseName) ||
                (_helpMessageResourceId != second._helpMessageResourceId))
        #region Efficient serialization + rehydration logic
        internal enum ParameterFlags : uint
            Mandatory = 0x01,
            ValueFromPipeline = 0x02,
            ValueFromPipelineByPropertyName = 0x04,
            ValueFromRemainingArguments = 0x08,
        internal ParameterFlags Flags
                ParameterFlags flags = 0;
                if (IsMandatory) { flags |= ParameterFlags.Mandatory; }
                if (ValueFromPipeline) { flags |= ParameterFlags.ValueFromPipeline; }
                if (ValueFromPipelineByPropertyName) { flags |= ParameterFlags.ValueFromPipelineByPropertyName; }
                if (ValueFromRemainingArguments) { flags |= ParameterFlags.ValueFromRemainingArguments; }
                return flags;
                this.IsMandatory = ((value & ParameterFlags.Mandatory) == ParameterFlags.Mandatory);
                this.ValueFromPipeline = ((value & ParameterFlags.ValueFromPipeline) == ParameterFlags.ValueFromPipeline);
                this.ValueFromPipelineByPropertyName = ((value & ParameterFlags.ValueFromPipelineByPropertyName) == ParameterFlags.ValueFromPipelineByPropertyName);
                this.ValueFromRemainingArguments = ((value & ParameterFlags.ValueFromRemainingArguments) == ParameterFlags.ValueFromRemainingArguments);
        /// Constructor used by rehydration.
        internal ParameterSetMetadata(
            ParameterFlags flags,
            this.Position = position;
            this.Flags = flags;
            this.HelpMessage = helpMessage;
        #region Proxy Parameter Generation
        private const string MandatoryFormat = @"{0}Mandatory=$true";
        private const string PositionFormat = @"{0}Position={1}";
        private const string ValueFromPipelineFormat = @"{0}ValueFromPipeline=$true";
        private const string ValueFromPipelineByPropertyNameFormat = @"{0}ValueFromPipelineByPropertyName=$true";
        private const string ValueFromRemainingArgumentsFormat = @"{0}ValueFromRemainingArguments=$true";
        private const string HelpMessageFormat = @"{0}HelpMessage='{1}'";
        internal string GetProxyParameterData()
            Text.StringBuilder result = new System.Text.StringBuilder();
            string prefix = string.Empty;
            if (_isMandatory)
                result.AppendFormat(CultureInfo.InvariantCulture, MandatoryFormat, prefix);
                prefix = ", ";
            if (_position != Int32.MinValue)
                result.AppendFormat(CultureInfo.InvariantCulture, PositionFormat, prefix, _position);
            if (_valueFromPipeline)
                result.AppendFormat(CultureInfo.InvariantCulture, ValueFromPipelineFormat, prefix);
            if (_valueFromPipelineByPropertyName)
                result.AppendFormat(CultureInfo.InvariantCulture, ValueFromPipelineByPropertyNameFormat, prefix);
            if (_valueFromRemainingArguments)
                result.AppendFormat(CultureInfo.InvariantCulture, ValueFromRemainingArgumentsFormat, prefix);
            if (!string.IsNullOrEmpty(_helpMessage))
                    HelpMessageFormat,
                    CodeGeneration.EscapeSingleQuotedStringContent(_helpMessage));
    /// This class represents the compiled metadata for a parameter.
    public sealed class ParameterMetadata
        private bool _isDynamic;
        private Dictionary<string, ParameterSetMetadata> _parameterSets;
        private Collection<string> _aliases;
        /// Constructs a ParameterMetadata instance.
        /// Name of the parameter.
        /// name is null.
        public ParameterMetadata(string name)
            : this(name, null)
        /// Type of the parameter.
        public ParameterMetadata(string name, Type parameterType)
            _aliases = new Collection<string>();
            _parameterSets = new Dictionary<string, ParameterSetMetadata>();
        /// A copy constructor that creates a deep copy of the <paramref name="other"/> ParameterMetadata object.
        public ParameterMetadata(ParameterMetadata other)
            _isDynamic = other._isDynamic;
            _name = other._name;
            _parameterType = other._parameterType;
            _aliases = new Collection<string>(new List<string>(other._aliases.Count));
            foreach (string alias in other._aliases)
                _aliases.Add(alias);
            if (other._attributes == null)
                _attributes = new Collection<Attribute>(new List<Attribute>(other._attributes.Count));
                foreach (Attribute attribute in other._attributes)
            _parameterSets = null;
            if (other._parameterSets == null)
                _parameterSets = new Dictionary<string, ParameterSetMetadata>(other._parameterSets.Count);
                foreach (KeyValuePair<string, ParameterSetMetadata> entry in other._parameterSets)
                    _parameterSets.Add(entry.Key, new ParameterSetMetadata(entry.Value));
        /// An internal constructor which constructs a ParameterMetadata object
        /// from compiled command parameter metadata. ParameterMetadata
        /// is a proxy written on top of CompiledCommandParameter.
        /// <param name="cmdParameterMD">
        /// Internal CompiledCommandParameter metadata
        internal ParameterMetadata(CompiledCommandParameter cmdParameterMD)
            Dbg.Assert(cmdParameterMD != null,
                "CompiledCommandParameter cannot be null");
            Initialize(cmdParameterMD);
        internal ParameterMetadata(
            Collection<string> aliases,
            bool isDynamic,
            Dictionary<string, ParameterSetMetadata> parameterSets,
            Type parameterType)
            _aliases = aliases;
            _isDynamic = isDynamic;
            _parameterSets = parameterSets;
        #region Public Methods/Properties
                    throw PSTraceSource.NewArgumentNullException("Name");
        /// Gets the Type information of the Parameter.
        /// Gets the ParameterSets metadata that this parameter belongs to.
        public Dictionary<string, ParameterSetMetadata> ParameterSets
        /// Specifies if the parameter is Dynamic.
        public bool IsDynamic
            get { return _isDynamic; }
            set { _isDynamic = value; }
        /// Specifies the alias names for this parameter.
        public Collection<string> Aliases
                return _aliases;
        /// A collection of the attributes found on the member.
        /// Specifies if the parameter is a SwitchParameter.
        public bool SwitchParameter
                if (_parameterType != null)
                    return _parameterType.Equals(typeof(SwitchParameter));
        /// Gets a dictionary of parameter metadata for the supplied <paramref name="type"/>.
        /// CLR Type for which the parameter metadata is constructed.
        /// A Dictionary of ParameterMetadata keyed by parameter name.
        /// null if no parameter metadata is found.
        /// type is null.
        public static Dictionary<string, ParameterMetadata> GetParameterMetadata(Type type)
            CommandMetadata cmdMetaData = new CommandMetadata(type);
            Dictionary<string, ParameterMetadata> result = cmdMetaData.Parameters;
            // early GC.
            cmdMetaData = null;
        #region Internal Methods/Properties
        /// <param name="compiledParameterMD"></param>
        internal void Initialize(CompiledCommandParameter compiledParameterMD)
            _name = compiledParameterMD.Name;
            _parameterType = compiledParameterMD.Type;
            _isDynamic = compiledParameterMD.IsDynamic;
            // Create parameter set metadata
            _parameterSets = new Dictionary<string, ParameterSetMetadata>(StringComparer.OrdinalIgnoreCase);
            foreach (string key in compiledParameterMD.ParameterSetData.Keys)
                ParameterSetSpecificMetadata pMD = compiledParameterMD.ParameterSetData[key];
                _parameterSets.Add(key, new ParameterSetMetadata(pMD));
            // Create aliases for this parameter
            foreach (string alias in compiledParameterMD.Aliases)
            // Create attributes for this parameter
            foreach (var attrib in compiledParameterMD.CompiledAttributes)
                _attributes.Add(attrib);
        /// <param name="cmdParameterMetadata"></param>
        internal static Dictionary<string, ParameterMetadata> GetParameterMetadata(MergedCommandParameterMetadata
            cmdParameterMetadata)
            Dbg.Assert(cmdParameterMetadata != null, "cmdParameterMetadata cannot be null");
            foreach (var keyValuePair in cmdParameterMetadata.BindableParameters)
                var key = keyValuePair.Key;
                var mergedCompiledPMD = keyValuePair.Value;
                ParameterMetadata parameterMetaData = new ParameterMetadata(mergedCompiledPMD.Parameter);
                result.Add(key, parameterMetaData);
        internal bool IsMatchingType(PSTypeName psTypeName)
            Type dotNetType = psTypeName.Type;
            if (dotNetType != null)
                // ConstrainedLanguage note - This conversion is analyzed, but actually invoked via regular conversion.
                bool parameterAcceptsObjects =
                    ((int)(LanguagePrimitives.FigureConversion(typeof(object), this.ParameterType).Rank)) >=
                    (int)(ConversionRank.AssignableS2A);
                if (dotNetType.Equals(typeof(object)))
                    return parameterAcceptsObjects;
                if (parameterAcceptsObjects)
                    return (psTypeName.Type != null) && (psTypeName.Type.Equals(typeof(object)));
                var conversionData = LanguagePrimitives.FigureConversion(dotNetType, this.ParameterType);
                    if ((int)(conversionData.Rank) >= (int)(ConversionRank.NumericImplicitS2A))
            var wildcardPattern = WildcardPattern.Get(
                "*" + (psTypeName.Name ?? string.Empty),
                WildcardOptions.IgnoreCase | WildcardOptions.CultureInvariant);
            if (wildcardPattern.IsMatch(this.ParameterType.FullName))
            if (this.ParameterType.IsArray && wildcardPattern.IsMatch((this.ParameterType.GetElementType().FullName)))
            if (this.Attributes != null)
                PSTypeNameAttribute typeNameAttribute = this.Attributes.OfType<PSTypeNameAttribute>().FirstOrDefault();
                if (typeNameAttribute != null && wildcardPattern.IsMatch(typeNameAttribute.PSTypeName))
        #region Proxy Parameter generation
        // The formats are prefixed with {0} to enable easy formatting.
        private const string ParameterNameFormat = @"{0}${{{1}}}";
        private const string ParameterTypeFormat = @"{0}[{1}]";
        private const string ParameterSetNameFormat = "ParameterSetName='{0}'";
        private const string AliasesFormat = @"{0}[Alias({1})]";
        private const string ValidateLengthFormat = @"{0}[ValidateLength({1}, {2})]";
        private const string ValidateRangeRangeKindFormat = @"{0}[ValidateRange([System.Management.Automation.ValidateRangeKind]::{1})]";
        private const string ValidateRangeEnumFormat = @"{0}[ValidateRange([{3}]::{1}, [{3}]::{2})]";
        private const string ValidateRangeFloatFormat = @"{0}[ValidateRange({1:R}, {2:R})]";
        private const string ValidateRangeFormat = @"{0}[ValidateRange({1}, {2})]";
        private const string ValidatePatternFormat = "{0}[ValidatePattern('{1}')]";
        private const string ValidateScriptFormat = @"{0}[ValidateScript({{ {1} }})]";
        private const string ValidateCountFormat = @"{0}[ValidateCount({1}, {2})]";
        private const string ValidateSetFormat = @"{0}[ValidateSet({1})]";
        private const string ValidateNotNullFormat = @"{0}[ValidateNotNull()]";
        private const string ValidateNotNullOrEmptyFormat = @"{0}[ValidateNotNullOrEmpty()]";
        private const string ValidateNotNullOrWhiteSpaceFormat = @"{0}[ValidateNotNullOrWhiteSpace()]";
        private const string AllowNullFormat = @"{0}[AllowNull()]";
        private const string AllowEmptyStringFormat = @"{0}[AllowEmptyString()]";
        private const string AllowEmptyCollectionFormat = @"{0}[AllowEmptyCollection()]";
        private const string PSTypeNameFormat = @"{0}[PSTypeName('{1}')]";
        private const string ObsoleteFormat = @"{0}[Obsolete({1})]";
        private const string CredentialAttributeFormat = @"{0}[System.Management.Automation.CredentialAttribute()]";
        /// <param name="prefix">
        /// prefix that is added to every new-line. Used for tabbing content.
        /// <param name="paramNameOverride">
        /// The paramNameOverride is used as the parameter name if it is not null or empty.
        /// <param name="isProxyForCmdlet">
        /// The parameter is for a cmdlet and requires a Parameter attribute.
        internal string GetProxyParameterData(string prefix, string paramNameOverride, bool isProxyForCmdlet)
            if (_parameterSets != null && isProxyForCmdlet)
                foreach (var pair in _parameterSets)
                    string parameterSetName = pair.Key;
                    ParameterSetMetadata parameterSet = pair.Value;
                    string paramSetData = parameterSet.GetProxyParameterData();
                    if (!string.IsNullOrEmpty(paramSetData) || !parameterSetName.Equals(ParameterAttribute.AllParameterSets))
                        result.Append(prefix);
                        result.Append("[Parameter(");
                                ParameterSetNameFormat,
                                CodeGeneration.EscapeSingleQuotedStringContent(parameterSetName));
                        if (!string.IsNullOrEmpty(paramSetData))
                            result.Append(separator);
                            result.Append(paramSetData);
                        result.Append(")]");
            if ((_aliases != null) && (_aliases.Count > 0))
                Text.StringBuilder aliasesData = new System.Text.StringBuilder();
                string comma = string.Empty; // comma is not need for the first element
                foreach (string alias in _aliases)
                    aliasesData.AppendFormat(
                        "{0}'{1}'",
                        comma,
                        CodeGeneration.EscapeSingleQuotedStringContent(alias));
                    comma = ",";
                result.AppendFormat(CultureInfo.InvariantCulture, AliasesFormat, prefix, aliasesData.ToString());
            if ((_attributes != null) && (_attributes.Count > 0))
                foreach (Attribute attrib in _attributes)
                    string attribData = GetProxyAttributeData(attrib, prefix);
                    if (!string.IsNullOrEmpty(attribData))
                        result.Append(attribData);
            if (SwitchParameter)
                result.AppendFormat(CultureInfo.InvariantCulture, ParameterTypeFormat, prefix, "switch");
            else if (_parameterType != null)
                result.AppendFormat(CultureInfo.InvariantCulture, ParameterTypeFormat, prefix, ToStringCodeMethods.Type(_parameterType));
            /* 1. CredentialAttribute needs to go after the type
             * 2. To avoid risk, I don't want to move other attributes to go here / after the type */
            CredentialAttribute credentialAttrib = _attributes.OfType<CredentialAttribute>().FirstOrDefault();
            if (credentialAttrib != null)
                string attribData = string.Format(CultureInfo.InvariantCulture, CredentialAttributeFormat, prefix);
                ParameterNameFormat,
                CodeGeneration.EscapeVariableName(string.IsNullOrEmpty(paramNameOverride) ? _name : paramNameOverride));
        /// Generates proxy data for attributes like ValidateLength, ValidateRange etc.
        /// <param name="attrib">
        /// Attribute to process.
        /// Prefix string to add.
        /// Attribute's proxy string.
        private static string GetProxyAttributeData(Attribute attrib, string prefix)
            ValidateLengthAttribute validLengthAttrib = attrib as ValidateLengthAttribute;
            if (validLengthAttrib != null)
                    ValidateLengthFormat, prefix,
                    validLengthAttrib.MinLength,
                    validLengthAttrib.MaxLength);
            ValidateRangeAttribute validRangeAttrib = attrib as ValidateRangeAttribute;
            if (validRangeAttrib != null)
                if (validRangeAttrib.RangeKind.HasValue)
                        ValidateRangeRangeKindFormat,
                        validRangeAttrib.RangeKind.ToString());
                    Type rangeType = validRangeAttrib.MinRange.GetType();
                    string format;
                    if (rangeType == typeof(float) || rangeType == typeof(double))
                        format = ValidateRangeFloatFormat;
                    else if (rangeType.IsEnum)
                        format = ValidateRangeEnumFormat;
                        format = ValidateRangeFormat;
                        validRangeAttrib.MinRange,
                        validRangeAttrib.MaxRange,
                        rangeType.FullName);
            AllowNullAttribute allowNullAttrib = attrib as AllowNullAttribute;
            if (allowNullAttrib != null)
                result = string.Format(CultureInfo.InvariantCulture,
                    AllowNullFormat, prefix);
            AllowEmptyStringAttribute allowEmptyStringAttrib = attrib as AllowEmptyStringAttribute;
            if (allowEmptyStringAttrib != null)
                    AllowEmptyStringFormat, prefix);
            AllowEmptyCollectionAttribute allowEmptyColAttrib = attrib as AllowEmptyCollectionAttribute;
            if (allowEmptyColAttrib != null)
                    AllowEmptyCollectionFormat, prefix);
            ValidatePatternAttribute patternAttrib = attrib as ValidatePatternAttribute;
            if (patternAttrib != null)
                /* TODO: Validate Pattern dont support Options in ScriptCmdletText.
                StringBuilder regexOps = new System.Text.StringBuilder();
                string or = string.Empty;
                string[] regexOptionEnumValues = Enum.GetNames<System.Text.RegularExpressions.RegexOptions>();
                foreach (string regexOption in regexOptionEnumValues)
                    System.Text.RegularExpressions.RegexOptions option = (System.Text.RegularExpressions.RegexOptions) Enum.Parse(
                        typeof(System.Text.RegularExpressions.RegexOptions),
                        regexOption, true);
                    if ((option & patternAttrib.Options) == option)
                        tracer.WriteLine("Regex option {0} found", regexOption);
                        regexOps.AppendFormat(CultureInfo.InvariantCulture,
                            "{0}[System.Text.RegularExpressions.RegexOptions]::{1}", or,
                            option.ToString()
                        or = "|";
                    ValidatePatternFormat, prefix,
                    CodeGeneration.EscapeSingleQuotedStringContent(patternAttrib.RegexPattern)
                    /*,regexOps.ToString()*/);
            ValidateCountAttribute countAttrib = attrib as ValidateCountAttribute;
            if (countAttrib != null)
                    ValidateCountFormat, prefix, countAttrib.MinLength, countAttrib.MaxLength);
            ValidateNotNullAttribute notNullAttrib = attrib as ValidateNotNullAttribute;
            if (notNullAttrib != null)
                    ValidateNotNullFormat, prefix);
            ValidateNotNullOrEmptyAttribute notNullEmptyAttrib = attrib as ValidateNotNullOrEmptyAttribute;
            if (notNullEmptyAttrib != null)
                    ValidateNotNullOrEmptyFormat, prefix);
            ValidateNotNullOrWhiteSpaceAttribute notNullWhiteSpaceAttrib = attrib as ValidateNotNullOrWhiteSpaceAttribute;
            if (notNullWhiteSpaceAttrib != null)
                    ValidateNotNullOrWhiteSpaceFormat, prefix);
            ValidateSetAttribute setAttrib = attrib as ValidateSetAttribute;
            if (setAttrib != null)
                Text.StringBuilder values = new System.Text.StringBuilder();
                string comma = string.Empty;
                foreach (string validValue in setAttrib.ValidValues)
                    values.AppendFormat(
                        CodeGeneration.EscapeSingleQuotedStringContent(validValue));
                    ValidateSetFormat, prefix, values.ToString()/*, setAttrib.IgnoreCase*/);
            ValidateScriptAttribute scriptAttrib = attrib as ValidateScriptAttribute;
            if (scriptAttrib != null)
                // Talked with others and I think it is okay to use *unescaped* value from sb.ToString()
                // 1. implicit remoting is not bringing validation scripts across
                // 2. other places in code also assume that contents of a script block can be parsed
                //    without escaping
                    ValidateScriptFormat, prefix, scriptAttrib.ScriptBlock.ToString());
            PSTypeNameAttribute psTypeNameAttrib = attrib as PSTypeNameAttribute;
            if (psTypeNameAttrib != null)
                    PSTypeNameFormat,
                    CodeGeneration.EscapeSingleQuotedStringContent(psTypeNameAttrib.PSTypeName));
            ObsoleteAttribute obsoleteAttrib = attrib as ObsoleteAttribute;
            if (obsoleteAttrib != null)
                string parameters = string.Empty;
                if (obsoleteAttrib.IsError)
                    string message = "'" + CodeGeneration.EscapeSingleQuotedStringContent(obsoleteAttrib.Message) + "'";
                    parameters = message + ", $true";
                else if (obsoleteAttrib.Message != null)
                    parameters = "'" + CodeGeneration.EscapeSingleQuotedStringContent(obsoleteAttrib.Message) + "'";
                    ObsoleteFormat,
    /// The metadata associated with a bindable type.
    internal class InternalParameterMetadata
        /// Gets or constructs an instance of the InternalParameterMetadata for the specified runtime-defined parameters.
        /// <param name="runtimeDefinedParameters">
        /// The runtime-defined parameter collection that describes the parameters and their metadata.
        /// <param name="checkNames">
        /// Check for reserved parameter names.
        /// An instance of the TypeMetadata for the specified runtime-defined parameters. The metadata
        /// is always constructed on demand and never cached.
        /// If <paramref name="runtimeDefinedParameters"/> is null.
        internal static InternalParameterMetadata Get(RuntimeDefinedParameterDictionary runtimeDefinedParameters,
                                                      bool processingDynamicParameters,
                                                      bool checkNames)
            if (runtimeDefinedParameters == null)
                throw PSTraceSource.NewArgumentNullException("runtimeDefinedParameter");
            return new InternalParameterMetadata(runtimeDefinedParameters, processingDynamicParameters, checkNames);
        /// Gets or constructs an instance of the InternalParameterMetadata for the specified type.
        /// The type to get the metadata for.
        /// An instance of the TypeMetadata for the specified type. The metadata may get
        /// constructed on-demand or may be retrieved from the cache.
        /// If <paramref name="type"/> is null.
        internal static InternalParameterMetadata Get(Type type, ExecutionContext context, bool processingDynamicParameters)
            InternalParameterMetadata result;
            if (context == null || !s_parameterMetadataCache.TryGetValue(type.AssemblyQualifiedName, out result))
                result = new InternalParameterMetadata(type, processingDynamicParameters);
                    s_parameterMetadataCache.TryAdd(type.AssemblyQualifiedName, result);
        /// Constructs an instance of the InternalParameterMetadata using the metadata in the
        /// runtime-defined parameter collection.
        /// The collection of runtime-defined parameters that declare the parameters and their
        /// metadata.
        /// Check if the parameter name has been reserved.
        internal InternalParameterMetadata(RuntimeDefinedParameterDictionary runtimeDefinedParameters, bool processingDynamicParameters, bool checkNames)
                throw PSTraceSource.NewArgumentNullException(nameof(runtimeDefinedParameters));
            ConstructCompiledParametersUsingRuntimeDefinedParameters(runtimeDefinedParameters, processingDynamicParameters, checkNames);
        /// Constructs an instance of the InternalParameterMetadata using the reflection information retrieved
        /// The type information for the bindable object
        internal InternalParameterMetadata(Type type, bool processingDynamicParameters)
            TypeName = type.Name;
            ConstructCompiledParametersUsingReflection(processingDynamicParameters);
        /// Gets the type name of the bindable type.
        internal string TypeName { get; } = string.Empty;
        /// The dictionary keys are the names of the parameters (or aliases) and
        internal Dictionary<string, CompiledCommandParameter> BindableParameters { get; }
            = new Dictionary<string, CompiledCommandParameter>(StringComparer.OrdinalIgnoreCase);
        /// the alias name and the value is the CompiledCommandParameter metadata.
        internal Dictionary<string, CompiledCommandParameter> AliasedParameters { get; }
        /// The type information for the class that implements the bindable object.
        /// This member is null in all cases except when constructed with using reflection
        /// against the Type.
        /// The flags used when reflecting against the object to create the metadata.
        internal static readonly BindingFlags metaDataBindingFlags = (BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        /// Fills in the data for an instance of this class using the specified runtime-defined parameters.
        /// A description of the parameters and their metadata.
        private void ConstructCompiledParametersUsingRuntimeDefinedParameters(
            RuntimeDefinedParameterDictionary runtimeDefinedParameters,
                runtimeDefinedParameters != null,
                "This method should only be called when constructed with a valid runtime-defined parameter collection");
            foreach (RuntimeDefinedParameter parameterDefinition in runtimeDefinedParameters.Values)
                // Create the compiled parameter and add it to the bindable parameters collection
                    // When processing dynamic parameters, parameter definitions come from the user,
                    // Invalid data could be passed in, or the parameter could be actually disabled.
                    if (parameterDefinition == null || parameterDefinition.IsDisabled()) { continue; }
                CompiledCommandParameter parameter = new CompiledCommandParameter(parameterDefinition, processingDynamicParameters);
                AddParameter(parameter, checkNames);
        /// Compiles the parameter using reflection against the CLR type.
        private void ConstructCompiledParametersUsingReflection(bool processingDynamicParameters)
                _type != null,
            // Get the property and field info
            PropertyInfo[] properties = _type.GetProperties(metaDataBindingFlags);
            FieldInfo[] fields = _type.GetFields(metaDataBindingFlags);
                // Check whether the property is a parameter
                if (!IsMemberAParameter(property))
                AddParameter(property, processingDynamicParameters);
                // Check whether the field is a parameter
                if (!IsMemberAParameter(field))
                AddParameter(field, processingDynamicParameters);
        private static void CheckForReservedParameter(string name)
            if (name.Equals("SelectProperty", StringComparison.OrdinalIgnoreCase)
                name.Equals("SelectObject", StringComparison.OrdinalIgnoreCase))
                throw new MetadataException(
                            "ReservedParameterName",
                            DiscoveryExceptions.ReservedParameterName,
        // This call verifies that the parameter is unique or
        // can be deemed unique. If not, an exception is thrown.
        // If it is unique (or deemed unique), then it is added
        // to the bindableParameters collection
        private void AddParameter(MemberInfo member, bool processingDynamicParameters)
            bool error = false;
            bool useExisting = false;
            CheckForReservedParameter(member.Name);
                CompiledCommandParameter existingParameter;
                if (!BindableParameters.TryGetValue(member.Name, out existingParameter))
                Type existingParamDeclaringType = existingParameter.DeclaringType;
                if (existingParamDeclaringType == null)
                if (existingParamDeclaringType.IsSubclassOf(member.DeclaringType))
                    useExisting = true;
                if (member.DeclaringType.IsSubclassOf(existingParamDeclaringType))
                    // Need to swap out the new member for the parameter definition
                    // that is already defined.
                    RemoveParameter(existingParameter);
                // A duplicate parameter was found and could not be deemed unique
                // through inheritance.
                    "DuplicateParameterDefinition",
                    ParameterBinderStrings.DuplicateParameterDefinition,
            if (!useExisting)
                CompiledCommandParameter parameter = new CompiledCommandParameter(member, processingDynamicParameters);
                AddParameter(parameter, true);
        private void AddParameter(CompiledCommandParameter parameter, bool checkNames)
            if (checkNames)
                CheckForReservedParameter(parameter.Name);
            BindableParameters.Add(parameter.Name, parameter);
            // Now add entries in the parameter aliases collection for any aliases.
                if (AliasedParameters.ContainsKey(alias))
                            "AliasDeclaredMultipleTimes",
                            DiscoveryExceptions.AliasDeclaredMultipleTimes,
                            alias);
                AliasedParameters.Add(alias, parameter);
        private void RemoveParameter(CompiledCommandParameter parameter)
            BindableParameters.Remove(parameter.Name);
                AliasedParameters.Remove(alias);
        /// Determines if the specified member represents a parameter based on its attributes.
        /// The member to check to see if it is a parameter.
        /// True if at least one ParameterAttribute is declared on the member, or false otherwise.
        /// If GetCustomAttributes fails on <paramref name="member"/>.
        private static bool IsMemberAParameter(MemberInfo member)
                var expAttribute = member.GetCustomAttributes<ExperimentalAttribute>(inherit: false).FirstOrDefault();
                if (expAttribute != null && expAttribute.ToHide) { return false; }
                var hasAnyVisibleParamAttributes = false;
                var paramAttributes = member.GetCustomAttributes<ParameterAttribute>(inherit: false);
                foreach (var paramAttribute in paramAttributes)
                    if (!paramAttribute.ToHide)
                        hasAnyVisibleParamAttributes = true;
                return hasAnyVisibleParamAttributes;
                    "GetCustomAttributesMetadataException",
                    metadataException,
                    Metadata.MetadataMemberInitialization,
                    metadataException.Message);
                    "GetCustomAttributesArgumentException",
                    argumentException.Message);
        #region Metadata cache
        /// The cache of the type metadata. The key for the cache is the Type.FullName.
        /// Note, this is a case-sensitive dictionary because Type names are case sensitive.
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, InternalParameterMetadata> s_parameterMetadataCache =
            new System.Collections.Concurrent.ConcurrentDictionary<string, InternalParameterMetadata>(StringComparer.Ordinal);
        #endregion Metadata cache
