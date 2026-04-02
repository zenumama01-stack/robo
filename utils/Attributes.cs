    /// Serves as the base class for Metadata attributes.
    /// PSSnapins may not create custom attributes derived directly from <see cref="CmdletMetadataAttribute"/>,
    /// since it has no public constructor. Only the public subclasses <see cref="ValidateArgumentsAttribute"/>
    /// and <see cref="ArgumentTransformationAttribute"/> are available.
    /// <seealso cref="CmdletMetadataAttribute"/>
    /// <seealso cref="ValidateArgumentsAttribute"/>
    /// <seealso cref="ArgumentTransformationAttribute"/>
    [AttributeUsage(AttributeTargets.All)]
    public abstract class CmdletMetadataAttribute : Attribute
        internal CmdletMetadataAttribute()
    /// Serves as the base class for Metadata attributes that serve as guidance to the parser and parameter binder.
    /// PSSnapins may not create custom attributes derived from <see cref="ParsingBaseAttribute"/>, since it
    /// has no public constructor. Only the sealed public subclasses <see cref="ParameterAttribute"/> and
    /// <see cref="AliasAttribute"/> are available.
    /// <seealso cref="ParsingBaseAttribute"/>
    /// <seealso cref="ParameterAttribute"/>
    /// <seealso cref="AliasAttribute"/>
    public abstract class ParsingBaseAttribute : CmdletMetadataAttribute
        /// Constructor with no parameters.
        internal ParsingBaseAttribute()
    #region Base Metadata Classes
    /// Serves as the base class for Validate attributes that validate parameter arguments.
    /// Argument validation attributes can be attached to <see cref="Cmdlet"/> and
    /// <see cref="Provider.CmdletProvider"/> parameters to ensure that the Cmdlet or CmdletProvider will
    /// not be invoked with invalid values of the parameter. Existing validation attributes include
    /// <see cref="ValidateCountAttribute"/>,
    /// <see cref="ValidateNotNullAttribute"/>,
    /// <see cref="ValidateNotNullOrEmptyAttribute"/>,
    /// <see cref="ValidateArgumentsAttribute"/>,
    /// <see cref="ValidateLengthAttribute"/>,
    /// <see cref="ValidateRangeAttribute"/>,
    /// <see cref="ValidatePatternAttribute"/>, and
    /// <see cref="ValidateSetAttribute"/>.
    /// PSSnapins wishing to create custom argument validation attributes should derive from
    /// <see cref="ValidateArgumentsAttribute"/> and override the
    /// <see cref="ValidateArgumentsAttribute.Validate"/> abstract method, after which they can apply the
    /// attribute to their parameters.
    /// <see cref="ValidateArgumentsAttribute"/> validates the argument as a whole. If the argument value may
    /// be an enumerable, you can derive from <see cref="ValidateEnumeratedArgumentsAttribute"/>
    /// which will take care of unrolling the enumerable and validate each element individually.
    /// It is also recommended to override <see cref="object.ToString"/> to return a readable string
    /// similar to the attribute declaration, for example "[ValidateRangeAttribute(5,10)]".
    /// If this attribute is applied to a string parameter, the string command argument will be validated.
    /// If this attribute is applied to a string[] parameter, the string[] command argument will be validated.
    /// <seealso cref="ValidateEnumeratedArgumentsAttribute"/>
    /// <seealso cref="ValidateCountAttribute"/>
    /// <seealso cref="ValidateNotNullAttribute"/>
    /// <seealso cref="ValidateNotNullOrEmptyAttribute"/>
    /// <seealso cref="ValidateLengthAttribute"/>
    /// <seealso cref="ValidateRangeAttribute"/>
    /// <seealso cref="ValidatePatternAttribute"/>
    /// <seealso cref="ValidateSetAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class ValidateArgumentsAttribute : CmdletMetadataAttribute
        /// Verify that the value of <paramref name="arguments"/> is valid.
        /// <param name="arguments">Argument value to validate.</param>
        /// The engine APIs for the context under which the prerequisite is being evaluated.
        /// <exception cref="ValidationMetadataException">Should be thrown for any validation failure.</exception>
        protected abstract void Validate(object arguments, EngineIntrinsics engineIntrinsics);
        /// Method that the command processor calls for data validate processing.
        /// <param name="o">Object to validate.</param>
        /// <returns>True if the validation succeeded.</returns>
        /// <exception cref="ValidationMetadataException">
        /// Whenever any exception occurs during data validation.
        /// Additionally, all the system exceptions are wrapped in ValidationMetadataException.
        /// <exception cref="ArgumentException">For invalid arguments.</exception>
        internal void InternalValidate(object o, EngineIntrinsics engineIntrinsics) => Validate(o, engineIntrinsics);
        /// Initializes a new instance of a class derived from <see cref="ValidateArgumentsAttribute"/>.
        protected ValidateArgumentsAttribute()
    /// A variant of <see cref="ValidateArgumentsAttribute"/> which unrolls enumeration values and validates
    /// each element individually.
    /// <see cref="ValidateEnumeratedArgumentsAttribute"/> is like <see cref="ValidateArgumentsAttribute"/>,
    /// except that if the argument value is enumerable, <see cref="ValidateEnumeratedArgumentsAttribute"/>
    /// will unroll the enumeration and validate each item individually.
    /// Existing enumerated validation attributes include
    /// PSSnapins wishing to create custom enumerated argument validation attributes should derive from
    /// <seealso cref="ValidateEnumeratedArgumentsAttribute"/> and override the
    /// <seealso cref="ValidateEnumeratedArgumentsAttribute.ValidateElement"/>
    /// abstract method, after which they can apply the attribute to their parameters.
    /// If this attribute is applied to a string[] parameter, each string command argument will be validated.
    public abstract class ValidateEnumeratedArgumentsAttribute : ValidateArgumentsAttribute
        /// Initializes a new instance of a class derived from <see cref="ValidateEnumeratedArgumentsAttribute"/>.
        protected ValidateEnumeratedArgumentsAttribute() : base()
        /// Abstract method to be overridden by subclasses, implementing the validation of each parameter argument.
        /// Validate that the value of <paramref name="element"/> is valid, and throw
        /// <see cref="ValidationMetadataException"/> if it is invalid.
        /// <param name="element">One of the parameter arguments.</param>
        protected abstract void ValidateElement(object element);
        /// Calls ValidateElement in each element in the enumeration argument value.
        /// <param name="arguments">Object to validate.</param>
        /// PSSnapins should override <see cref="ValidateElement"/> instead.
        protected sealed override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
            if (LanguagePrimitives.IsNull(arguments))
                throw new ValidationMetadataException(
                    "ArgumentIsEmpty",
                    Metadata.ValidateNotNullOrEmptyCollectionFailure);
            var enumerator = _getEnumeratorSite.Target.Invoke(_getEnumeratorSite, arguments);
            if (enumerator == null)
                ValidateElement(arguments);
            // arguments is IEnumerator
                ValidateElement(enumerator.Current);
        private readonly CallSite<Func<CallSite, object, IEnumerator>> _getEnumeratorSite =
            CallSite<Func<CallSite, object, IEnumerator>>.Create(PSEnumerableBinder.Get());
    #endregion Base Metadata Classes
    #region Misc Attributes
    /// To specify RunAs behavior for the class
    /// /// </summary>
    public enum DSCResourceRunAsCredential
        /// <summary>Default is same as optional.</summary>
        /// PsDscRunAsCredential can not be used for this DSC Resource.
        NotSupported,
        /// PsDscRunAsCredential is mandatory for resource.
        Mandatory,
        /// PsDscRunAsCredential can or can not be specified.
        Optional = Default,
    /// Indicates the class defines a DSC resource.
    [AttributeUsage(AttributeTargets.Class)]
    public class DscResourceAttribute : CmdletMetadataAttribute
        /// To specify RunAs Behavior for the resource.
        public DSCResourceRunAsCredential RunAsCredential { get; set; }
    /// When specified on a property or field of a DSC Resource, the property
    /// can or must be specified in a configuration, unless it is marked
    /// <see cref="DscPropertyAttribute.NotConfigurable"/>, in which case it is
    /// returned by the Get() method of the resource.
    public class DscPropertyAttribute : CmdletMetadataAttribute
        /// Indicates the property is a required key property for a DSC resource.
        public bool Key { get; set; }
        /// Indicates the property is a required property for a DSC resource.
        public bool Mandatory { get; set; }
        /// Indicates the property is not a parameter to the DSC resource, but the
        /// property will contain a value after the Get() method of the resource is called.
        public bool NotConfigurable { get; set; }
    /// Indication the configuration is for local configuration manager, also known as meta configuration.
    public class DscLocalConfigurationManagerAttribute : CmdletMetadataAttribute
    /// Contains information about a cmdlet's metadata.
    public abstract class CmdletCommonMetadataAttribute : CmdletMetadataAttribute
        /// Gets or sets the cmdlet default parameter set.
        public string DefaultParameterSetName { get; set; }
        /// Gets or sets a Boolean value that indicates the Cmdlet supports ShouldProcess. By default
        /// the value is false, meaning the cmdlet doesn't support ShouldProcess.
        public bool SupportsShouldProcess { get; set; } = false;
        /// Gets or sets a Boolean value that indicates the Cmdlet supports Paging. By default
        /// the value is false, meaning the cmdlet doesn't support Paging.
        public bool SupportsPaging { get; set; } = false;
        /// Gets or sets a Boolean value that indicates the Cmdlet supports Transactions. By default
        /// the value is false, meaning the cmdlet doesn't support Transactions.
        public bool SupportsTransactions
                return _supportsTransactions;
                _supportsTransactions = value;
                // Disable 'SupportsTransactions' in CoreCLR
                // No transaction supported on CSS due to the lack of System.Transactions namespace
                _supportsTransactions = false;
        private bool _supportsTransactions = false;
        private ConfirmImpact _confirmImpact = ConfirmImpact.Medium;
        /// Gets or sets a ConfirmImpact value that indicates the "destructiveness" of the operation
        /// and when it should be confirmed. This should only be used when SupportsShouldProcess is
        /// specified.
            get => SupportsShouldProcess ? _confirmImpact : ConfirmImpact.None;
            set => _confirmImpact = value;
        /// Gets or sets a HelpUri value that indicates the location of online help. This is used by
        /// Get-Help to retrieve help content when -Online is specified.
        public string HelpUri { get; set; } = string.Empty;
        /// Gets or sets the RemotingBehavior value that declares how this cmdlet should interact
        /// with ambient remoting.
        public RemotingCapability RemotingCapability { get; set; } = RemotingCapability.PowerShell;
    /// Identifies a class as a cmdlet and specifies the verb and noun identifying this cmdlet.
    public sealed class CmdletAttribute : CmdletCommonMetadataAttribute
        /// Gets the cmdlet noun.
        public string NounName { get; }
        /// Gets the cmdlet verb.
        public string VerbName { get; }
        /// Initializes a new instance of the CmdletAttribute class.
        /// <param name="verbName">Verb for the command.</param>
        /// <param name="nounName">Noun for the command.</param>
        public CmdletAttribute(string verbName, string nounName)
            // NounName,VerbName have to be Non-Null strings
            if (string.IsNullOrEmpty(nounName))
                throw PSTraceSource.NewArgumentException(nameof(nounName));
            if (string.IsNullOrEmpty(verbName))
                throw PSTraceSource.NewArgumentException(nameof(verbName));
            NounName = nounName;
            VerbName = verbName;
    /// Identifies PowerShell script code as behaving like a cmdlet and hence uses cmdlet parameter binding
    /// instead of script parameter binding.
    public class CmdletBindingAttribute : CmdletCommonMetadataAttribute
        /// When true, the script will auto-generate appropriate parameter metadata to support positional
        /// parameters if the script hasn't already specified multiple parameter sets or specified positions
        /// explicitly via the <see cref="ParameterAttribute"/>.
        public bool PositionalBinding { get; set; } = true;
    /// OutputTypeAttribute is used to specify the type of objects output by a cmdlet or script.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    public sealed class OutputTypeAttribute : CmdletMetadataAttribute
        /// Construct the attribute from a <see>System.Type</see>
        internal OutputTypeAttribute(Type type)
            Type = new[] { new PSTypeName(type) };
        /// Construct the attribute from a type name.
        internal OutputTypeAttribute(string typeName)
            Type = new[] { new PSTypeName(typeName) };
        /// Construct the attribute from an array of <see>System.Type</see>
        /// <param name="type">The types output by the cmdlet.</param>
        public OutputTypeAttribute(params Type[] type)
            if (type?.Length > 0)
                Type = new PSTypeName[type.Length];
                for (int i = 0; i < type.Length; i++)
                    Type[i] = new PSTypeName(type[i]);
                Type = Array.Empty<PSTypeName>();
        /// Construct the attribute from an array of names of types.
        public OutputTypeAttribute(params string[] type)
        /// The types specified by the attribute.
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public PSTypeName[] Type { get; }
        /// Attributes implemented by a provider can use:
        ///     [OutputType(ProviderCmdlet='cmdlet', typeof(...))]
        /// To specify the provider specific objects returned for a given cmdlet.
        public string ProviderCmdlet { get; set; }
        /// The list of parameter sets this OutputType specifies.
        public string[] ParameterSetName
            get => _parameterSetName ??= new[] { ParameterAttribute.AllParameterSets };
            set => _parameterSetName = value;
        private string[] _parameterSetName;
    /// This attribute is used on a dynamic assembly to mark it as one that is used to implement
    /// a set of classes defined in a PowerShell script.
    [AttributeUsage(AttributeTargets.Assembly)]
    public class DynamicClassImplementationAssemblyAttribute : Attribute
        /// The (possibly null) path to the file defining this class.
        public string ScriptFile { get; set; }
    #endregion Misc Attributes
    #region Parsing guidelines Attributes
    /// Declares an alternative name for a parameter, cmdlet, or function.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AliasAttribute : ParsingBaseAttribute
        internal string[] aliasNames;
        /// Gets the alias names passed to the constructor.
        public IList<string> AliasNames { get => this.aliasNames; }
        /// Initializes a new instance of the AliasAttribute class.
        /// <param name="aliasNames">The name for this alias.</param>
        public AliasAttribute(params string[] aliasNames)
            if (aliasNames == null)
                throw PSTraceSource.NewArgumentNullException(nameof(aliasNames));
            this.aliasNames = aliasNames;
    /// Identifies parameters to Cmdlets.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class ParameterAttribute : ParsingBaseAttribute
        /// ParameterSetName referring to all ParameterSets.
        public const string AllParameterSets = "__AllParameterSets";
        /// Initializes a new instance of the ParameterAttribute class.
        public ParameterAttribute()
        /// Initializes a new instance that is associated with an experimental feature.
        public ParameterAttribute(string experimentName, ExperimentAction experimentAction)
            ExperimentalAttribute.ValidateArguments(experimentName, experimentAction);
            ExperimentName = experimentName;
            ExperimentAction = experimentAction;
        private string _parameterSetName = ParameterAttribute.AllParameterSets;
        private string _helpMessage;
        private string _helpMessageBaseName;
        private string _helpMessageResourceId;
        #region Experimental Feature Related Properties
        /// Gets the name of the experimental feature this attribute is associated with.
        public string ExperimentName { get; }
        /// Gets the action for the engine to take when the experimental feature is enabled.
        public ExperimentAction ExperimentAction { get; }
        internal bool ToHide => EffectiveAction == ExperimentAction.Hide;
        internal bool ToShow => EffectiveAction == ExperimentAction.Show;
        /// Gets the effective action to take at run time.
        private ExperimentAction EffectiveAction
                if (_effectiveAction == ExperimentAction.None)
                    _effectiveAction = ExperimentalFeature.GetActionToTake(ExperimentName, ExperimentAction);
                return _effectiveAction;
        private ExperimentAction _effectiveAction = default(ExperimentAction);
        /// Gets or sets the parameter position.
        /// If not set, the parameter is named.
        public int Position { get; set; } = int.MinValue;
        /// Gets or sets the name of the parameter set this parameter belongs to.
        /// When it is not specified, <see cref="ParameterAttribute.AllParameterSets"/> is assumed.
            get => _parameterSetName;
            set => _parameterSetName = string.IsNullOrEmpty(value) ? ParameterAttribute.AllParameterSets : value;
        /// Gets or sets a flag specifying if this parameter is Mandatory.
        /// When it is not specified, false is assumed and the parameter is considered optional.
        public bool Mandatory { get; set; } = false;
        /// Gets or sets a flag that specifies that this parameter can take values from the incoming pipeline
        /// When it is not specified, false is assumed.
        public bool ValueFromPipeline { get; set; }
        /// Gets or sets a flag that specifies that this parameter can take values from a property in the
        /// incoming pipeline object with the same name as the parameter or an alias of the parameter.
        public bool ValueFromPipelineByPropertyName { get; set; }
        /// Gets or sets a flag that specifies that the remaining command line parameters should be
        /// associated with this parameter in the form of an array.
        public bool ValueFromRemainingArguments { get; set; } = false;
        /// Gets or sets a short description for this parameter, suitable for presentation as a tool tip.
        /// <exception cref="ArgumentException">For a null or empty value when setting.</exception>
        public string HelpMessage
            get => _helpMessage;
                    throw PSTraceSource.NewArgumentException(nameof(HelpMessage));
                _helpMessage = value;
        /// Gets or sets the base name of the resource for a help message.
        /// When this field is specified, HelpMessageResourceId must also be specified.
        public string HelpMessageBaseName
            get => _helpMessageBaseName;
                    throw PSTraceSource.NewArgumentException(nameof(HelpMessageBaseName));
                _helpMessageBaseName = value;
        /// Gets or sets the Id of the resource for a help message.
        /// When this field is specified, HelpMessageBaseName must also be specified.
        public string HelpMessageResourceId
            get => _helpMessageResourceId;
                    throw PSTraceSource.NewArgumentException(nameof(HelpMessageResourceId));
                _helpMessageResourceId = value;
        /// Indicates that this parameter should not be shown to the user in this like intellisense
        /// This is primarily to be used in functions that are implementing the logic for dynamic keywords.
        public bool DontShow { get; set; }
    /// Specifies PSTypeName of a cmdlet or function parameter.
    /// This attribute is used to restrict the type name of the parameter, when the type goes beyond the .NET type system.
    /// For example one could say: [PSTypeName("System.Management.ManagementObject#root\cimv2\Win32_Process")]
    /// to only allow Win32_Process objects to be bound to the parameter.
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PSTypeNameAttribute : Attribute
        public string PSTypeName { get; }
        /// Creates a new PSTypeNameAttribute.
        /// <param name="psTypeName"></param>
        public PSTypeNameAttribute(string psTypeName)
            if (string.IsNullOrEmpty(psTypeName))
                throw PSTraceSource.NewArgumentException(nameof(psTypeName));
            this.PSTypeName = psTypeName;
    /// Specifies that a parameter supports wildcards.
    public sealed class SupportsWildcardsAttribute : ParsingBaseAttribute
    /// Specify a default value and/or help comment for a command parameter.  This attribute
    /// does not have any semantic meaning, it is simply an aid to tools to make it simpler
    /// to know the true default value of a command parameter (which may or may not have
    /// any correlation with, e.g., the backing store of the Parameter's property or field.
    public sealed class PSDefaultValueAttribute : ParsingBaseAttribute
        /// Specify the default value of a command parameter. The PowerShell engine does not
        /// use this value in any way, it exists for other tools that want to reflect on cmdlets.
        /// Specify the help string for the default value of a command parameter.
        public string Help { get; set; }
    /// Specify that the member is hidden for the purposes of cmdlets like Get-Member and that the
    /// member is not displayed by default by Format-* cmdlets.
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Event)]
    public sealed class HiddenAttribute : ParsingBaseAttribute
    #endregion Parsing guidelines Attributes
    #region Data validate Attributes
    /// Validates that the length of each parameter argument's Length falls in the range specified by
    /// <see cref="MinLength"/> and <see cref="MaxLength"/>.
    public sealed class ValidateLengthAttribute : ValidateEnumeratedArgumentsAttribute
        /// Gets the attribute's minimum length.
        public int MinLength { get; }
        /// Gets the attribute's maximum length.
        public int MaxLength { get; }
        /// Validates that the length of each parameter argument's Length falls in the range specified
        /// by <see cref="MinLength"/> and <see cref="MaxLength"/>.
        /// <param name="element">Object to validate.</param>
        /// <exception cref="ValidationMetadataException">If <paramref name="element"/> is not a string
        /// with length between minLength and maxLength</exception>
        protected override void ValidateElement(object element)
            if (element is not string objectString)
                    "ValidateLengthNotString",
                    Metadata.ValidateLengthNotString);
            int len = objectString.Length;
            if (len < MinLength)
                    "ValidateLengthMinLengthFailure",
                    Metadata.ValidateLengthMinLengthFailure,
                    MinLength, len);
            if (len > MaxLength)
                    "ValidateLengthMaxLengthFailure",
                    Metadata.ValidateLengthMaxLengthFailure,
                    MaxLength, len);
        /// Initializes a new instance of the <see cref="ValidateLengthAttribute"/> class.
        /// <param name="minLength">Minimum required length.</param>
        /// <param name="maxLength">Maximum required length.</param>
        /// <exception cref="ArgumentOutOfRangeException">For invalid arguments.</exception>
        /// <exception cref="ValidationMetadataException">If maxLength is less than minLength.</exception>
        public ValidateLengthAttribute(int minLength, int maxLength) : base()
            if (minLength < 0)
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(minLength), minLength);
            if (maxLength <= 0)
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(maxLength), maxLength);
            if (maxLength < minLength)
                    "ValidateLengthMaxLengthSmallerThanMinLength",
                    Metadata.ValidateLengthMaxLengthSmallerThanMinLength);
            MinLength = minLength;
            MaxLength = maxLength;
    /// Predefined range kind to use with ValidateRangeAttribute.
    public enum ValidateRangeKind
        /// Range is greater than 0.
        Positive,
        /// Range is greater than or equal to 0.
        NonNegative,
        /// Range is less than 0.
        Negative,
        /// Range is less than or equal to 0.
        NonPositive
    /// Validates that each parameter argument falls in the range specified by <see cref="MinRange"/>
    /// and <see cref="MaxRange"/>.
    public sealed class ValidateRangeAttribute : ValidateEnumeratedArgumentsAttribute
        /// Gets the attribute's minimum range.
        public object MinRange { get; }
        private readonly IComparable _minComparable;
        /// Gets the attribute's maximum range.
        public object MaxRange { get; }
        private readonly IComparable _maxComparable;
        /// The range values and the value to validate will all be converted to the promoted type.
        /// If minRange and maxRange are the same type,
        private readonly Type _promotedType;
        /// Gets the name of the predefined range.
        internal ValidateRangeKind? RangeKind { get => _rangeKind; }
        private readonly ValidateRangeKind? _rangeKind;
        /// Thrown if the object to be validated does not implement <see cref="IComparable"/>,
        /// if the element type is not the same as MinRange/MaxRange, or if the element is not between
        /// MinRange and MaxRange.
            if (element == null)
                        Metadata.ValidateNotNullFailure);
            var o = element as PSObject;
                element = o.BaseObject;
            if (_rangeKind.HasValue)
                ValidateRange(element, (ValidateRangeKind)_rangeKind);
                ValidateRange(element);
        /// Initializes a new instance of the <see cref="ValidateRangeAttribute"/> class.
        /// <param name="minRange">Minimum value of the range allowed.</param>
        /// <param name="maxRange">Maximum value of the range allowed.</param>
        /// <exception cref="ArgumentNullException">For invalid arguments.</exception>
        /// if <paramref name="maxRange"/> has a different type than <paramref name="minRange"/>
        /// if <paramref name="maxRange"/> is smaller than <paramref name="minRange"/>
        /// if <paramref name="maxRange"/>, <paramref name="minRange"/> are not <see cref="IComparable"/>
        public ValidateRangeAttribute(object minRange, object maxRange) : base()
            if (minRange == null)
                throw PSTraceSource.NewArgumentNullException(nameof(minRange));
            if (maxRange == null)
                throw PSTraceSource.NewArgumentNullException(nameof(maxRange));
            if (maxRange.GetType() != minRange.GetType())
                bool failure = true;
                _promotedType = GetCommonType(minRange.GetType(), maxRange.GetType());
                if (_promotedType != null)
                    if (LanguagePrimitives.TryConvertTo(minRange, _promotedType, out object minResultValue)
                        && LanguagePrimitives.TryConvertTo(maxRange, _promotedType, out object maxResultValue))
                        minRange = minResultValue;
                        maxRange = maxResultValue;
                        failure = false;
                if (failure)
                        "MinRangeNotTheSameTypeOfMaxRange",
                        Metadata.ValidateRangeMinRangeMaxRangeType,
                        minRange.GetType().Name, maxRange.GetType().Name);
                _promotedType = minRange.GetType();
            // minRange and maxRange have the same type, so we just need to check one of them
            _minComparable = minRange as IComparable;
            if (_minComparable == null)
                    "MinRangeNotIComparable",
                    Metadata.ValidateRangeNotIComparable);
            _maxComparable = maxRange as IComparable;
            Diagnostics.Assert(_maxComparable != null, "maxComparable comes from a type that is IComparable");
            // Thanks to the IComparable test above this will not throw. They have the same type and are IComparable.
            if (_minComparable.CompareTo(maxRange) > 0)
                    "MaxRangeSmallerThanMinRange",
                    Metadata.ValidateRangeMaxRangeSmallerThanMinRange);
            MinRange = minRange;
            MaxRange = maxRange;
        /// This constructor uses a predefined <see cref="ValidateRangeKind"/>.
        public ValidateRangeAttribute(ValidateRangeKind kind) : base()
            _rangeKind = kind;
        private static void ValidateRange(object element, ValidateRangeKind rangeKind)
            if (element is TimeSpan ts)
                ValidateTimeSpanRange(ts, rangeKind);
            Type commonType = GetCommonType(typeof(int), element.GetType());
            if (commonType == null)
                    "ValidationRangeElementType",
                    Metadata.ValidateRangeElementType,
                    element.GetType().Name,
                    nameof(Int32));
            object resultValue;
            IComparable dynamicZero = 0;
            if (LanguagePrimitives.TryConvertTo(element, commonType, out resultValue))
                element = resultValue;
                if (LanguagePrimitives.TryConvertTo(0, commonType, out resultValue))
                    dynamicZero = (IComparable)resultValue;
                    commonType.Name);
            switch (rangeKind)
                case ValidateRangeKind.Positive:
                    if (dynamicZero.CompareTo(element) >= 0)
                            "ValidateRangePositiveFailure",
                            Metadata.ValidateRangePositiveFailure,
                            element.ToString());
                case ValidateRangeKind.NonNegative:
                    if (dynamicZero.CompareTo(element) > 0)
                            "ValidateRangeNonNegativeFailure",
                            Metadata.ValidateRangeNonNegativeFailure,
                case ValidateRangeKind.Negative:
                    if (dynamicZero.CompareTo(element) <= 0)
                            "ValidateRangeNegativeFailure",
                            Metadata.ValidateRangeNegativeFailure,
                case ValidateRangeKind.NonPositive:
                    if (dynamicZero.CompareTo(element) < 0)
                            "ValidateRangeNonPositiveFailure",
                            Metadata.ValidateRangeNonPositiveFailure,
        private void ValidateRange(object element)
            // MinRange and MaxRange have the same type, so we just need to compare to one of them.
            if (element.GetType() != _promotedType)
                if (LanguagePrimitives.TryConvertTo(element, _promotedType, out object resultValue))
                        MinRange.GetType().Name);
            // They are the same type and are all IComparable, so this should not throw
            if (_minComparable.CompareTo(element) > 0)
                    "ValidateRangeTooSmall",
                    Metadata.ValidateRangeSmallerThanMinRangeFailure,
                    element.ToString(),
                    MinRange.ToString());
            if (_maxComparable.CompareTo(element) < 0)
                    "ValidateRangeTooBig",
                    Metadata.ValidateRangeGreaterThanMaxRangeFailure,
                    MaxRange.ToString());
        private static void ValidateTimeSpanRange(TimeSpan element, ValidateRangeKind rangeKind)
            TimeSpan zero = TimeSpan.Zero;
                    if (zero.CompareTo(element) >= 0)
                    if (zero.CompareTo(element) > 0)
                    if (zero.CompareTo(element) <= 0)
                    if (zero.CompareTo(element) < 0)
        private static Type GetCommonType(Type minType, Type maxType)
            Type resultType = null;
            TypeCode minTypeCode = LanguagePrimitives.GetTypeCode(minType);
            TypeCode maxTypeCode = LanguagePrimitives.GetTypeCode(maxType);
            TypeCode opTypeCode = (int)minTypeCode >= (int)maxTypeCode ? minTypeCode : maxTypeCode;
            if ((int)opTypeCode <= (int)TypeCode.Int32)
                resultType = typeof(int);
            else if ((int)opTypeCode <= (int)TypeCode.UInt32)
                // If one of the operands is signed, we need to promote to double if the value is negative.
                // We aren't checking the value, so we unconditionally promote to double.
                resultType = LanguagePrimitives.IsSignedInteger(minTypeCode) || LanguagePrimitives.IsSignedInteger(maxTypeCode)
                    ? typeof(double) : typeof(uint);
            else if ((int)opTypeCode <= (int)TypeCode.Int64)
                resultType = typeof(long);
            else if ((int)opTypeCode <= (int)TypeCode.UInt64)
                    ? typeof(double) : typeof(ulong);
            else if (opTypeCode == TypeCode.Decimal)
                resultType = typeof(decimal);
            else if (opTypeCode == TypeCode.Single || opTypeCode == TypeCode.Double)
                resultType = typeof(double);
            return resultType;
        /// Returns only the elements that passed the attribute's validation.
        /// <param name="elementsToValidate">The objects to validate.</param>
        internal IEnumerable GetValidatedElements(IEnumerable elementsToValidate)
            foreach (var el in elementsToValidate)
                    ValidateElement(el);
                catch (ValidationMetadataException)
                    // Element was not in range - drop
                yield return el;
    /// Validates that each parameter argument matches the <see cref="RegexPattern"/>.
    public sealed class ValidatePatternAttribute : ValidateEnumeratedArgumentsAttribute
        /// Gets the Regex pattern to be used in the validation.
        public string RegexPattern { get; }
        /// Gets or sets the Regex options to be used in the validation.
        public RegexOptions Options { get; set; } = RegexOptions.IgnoreCase;
        /// Gets or sets the custom error message pattern that is displayed to the user.
        /// The text representation of the object being validated and the validating regex is passed as
        /// the first and second formatting parameters to the ErrorMessage formatting pattern.
        /// <c>
        /// <code>
        /// [ValidatePattern("\s+", ErrorMessage="The text '{0}' did not pass validation of regex '{1}'")]
        /// </code>
        /// </c>
        public string ErrorMessage { get; set; }
        /// Validates that each parameter argument matches the RegexPattern.
        /// If <paramref name="element"/> is not a string that matches the pattern, and for invalid arguments.
            string objectString = element.ToString();
            var regex = new Regex(RegexPattern, Options);
            Match match = regex.Match(objectString);
                var errorMessageFormat = string.IsNullOrEmpty(ErrorMessage)
                    ? Metadata.ValidatePatternFailure
                    : ErrorMessage;
                    "ValidatePatternFailure",
                    errorMessageFormat,
                    objectString, RegexPattern);
        /// Initializes a new instance of the <see cref="ValidatePatternAttribute"/> class.
        /// <param name="regexPattern">Pattern string to match.</param>
        public ValidatePatternAttribute(string regexPattern)
            if (string.IsNullOrEmpty(regexPattern))
                throw PSTraceSource.NewArgumentException(nameof(regexPattern));
            RegexPattern = regexPattern;
    /// Class for validating against a script block.
    public sealed class ValidateScriptAttribute : ValidateEnumeratedArgumentsAttribute
        /// Gets or sets the custom error message that is displayed to the user.
        /// The item being validated and the validating scriptblock is passed as the first and second
        /// formatting argument.
        /// [ValidateScript("$_ % 2", ErrorMessage = "The item '{0}' did not pass validation of script '{1}'")]
        /// Gets the scriptblock to be used in the validation.
        public ScriptBlock ScriptBlock { get; }
        /// Validates that each parameter argument matches the scriptblock.
        /// <exception cref="ValidationMetadataException">If <paramref name="element"/> is invalid.</exception>
            object result = ScriptBlock.DoInvokeReturnAsIs(
                dollarUnder: LanguagePrimitives.AsPSObjectOrNull(element),
            if (!LanguagePrimitives.IsTrue(result))
                    ? Metadata.ValidateScriptFailure
                    "ValidateScriptFailure",
                    element, ScriptBlock);
        /// Initializes a new instance of the <see cref="ValidateScriptAttribute"/> class.
        /// <param name="scriptBlock">Scriptblock to match.</param>
        public ValidateScriptAttribute(ScriptBlock scriptBlock)
                throw PSTraceSource.NewArgumentException(nameof(scriptBlock));
            ScriptBlock = scriptBlock;
    /// Validates that the parameter argument count is in the specified range.
    public sealed class ValidateCountAttribute : ValidateArgumentsAttribute
        /// Gets the minimum length of this attribute.
        /// Gets the maximum length of this attribute.
        /// The engine APIs for the context under which the validation is being evaluated.
        /// if the element is none of <see cref="ICollection"/>, <see cref="IEnumerable"/>,
        /// <see cref="IList"/>, <see cref="IEnumerator"/>
        /// if the element's length is not between <see cref="MinLength"/> and <see cref="MaxLength"/>
            if (arguments == null || arguments == AutomationNull.Value)
                // treat a nul list the same as an empty list
                // with a count of zero.
                len = 0;
            else if (arguments is IList il)
                len = il.Count;
            else if (arguments is ICollection ic)
                len = ic.Count;
            else if (arguments is IEnumerable ie)
                IEnumerator e = ie.GetEnumerator();
            else if (arguments is IEnumerator enumerator)
                // No conversion succeeded so throw an exception...
                    "NotAnArrayParameter",
                    Metadata.ValidateCountNotInArray);
            if (MinLength == MaxLength && len != MaxLength)
                    "ValidateCountExactFailure",
                    Metadata.ValidateCountExactFailure,
            if (len < MinLength || len > MaxLength)
                    "ValidateCountMinMaxFailure",
                    Metadata.ValidateCountMinMaxFailure,
                    MinLength, MaxLength, len);
        /// Initializes a new instance of the <see cref="ValidateCountAttribute"/> class.
        /// <param name="minLength">Minimum number of values required.</param>
        /// <param name="maxLength">Maximum number of values required.</param>
        /// if <paramref name="minLength"/> is greater than <paramref name="maxLength"/>
        public ValidateCountAttribute(int minLength, int maxLength)
                    "ValidateRangeMaxLengthSmallerThanMinLength",
                    Metadata.ValidateCountMaxLengthSmallerThanMinLength);
    /// Optional base class for <see cref="IValidateSetValuesGenerator"/> implementations that want a default
    /// implementation to cache valid values.
    public abstract class CachedValidValuesGeneratorBase : IValidateSetValuesGenerator
        // Cached valid values.
        private string[] _validValues;
        private readonly int _validValuesCacheExpiration;
        /// Initializes a new instance of the <see cref="CachedValidValuesGeneratorBase"/> class.
        /// <param name="cacheExpirationInSeconds">
        /// Sets a time interval in seconds to reset the <see cref="_validValues"/> dynamic valid values cache.
        protected CachedValidValuesGeneratorBase(int cacheExpirationInSeconds)
            _validValuesCacheExpiration = cacheExpirationInSeconds;
        /// Abstract method to generate a valid values.
        public abstract string[] GenerateValidValues();
        /// Get a valid values.
        public string[] GetValidValues()
            // Because we have a background task to clear the cache by '_validValues = null'
            // we use the local variable to exclude a race condition.
            var validValuesLocal = _validValues;
            if (validValuesLocal != null)
                return validValuesLocal;
            var validValuesNoCache = GenerateValidValues();
            if (validValuesNoCache == null)
                    "ValidateSetGeneratedValidValuesListIsNull",
                    Metadata.ValidateSetGeneratedValidValuesListIsNull);
            if (_validValuesCacheExpiration > 0)
                _validValues = validValuesNoCache;
                Task.Delay(_validValuesCacheExpiration * 1000).ContinueWith((task) => _validValues = null);
            return validValuesNoCache;
    /// Validates that each parameter argument is present in a specified set.
    public sealed class ValidateSetAttribute : ValidateEnumeratedArgumentsAttribute
        // We can use either static '_validValues' or dynamic valid values list generated by instance
        // of 'validValuesGenerator'.
        private readonly string[] _validValues;
        private readonly IValidateSetValuesGenerator validValuesGenerator = null;
        // The valid values generator cache works across 'ValidateSetAttribute' instances.
        private static readonly ConcurrentDictionary<Type, IValidateSetValuesGenerator> s_ValidValuesGeneratorCache =
            new ConcurrentDictionary<Type, IValidateSetValuesGenerator>();
        /// The item being validated and a text representation of the validation set is passed as the
        /// first and second formatting argument to the <see cref="ErrorMessage"/> formatting pattern.
        /// [ValidateSet("A","B","C", ErrorMessage="The item '{0}' is not part of the set '{1}'.")
        /// Gets a flag specifying if we should ignore the case when performing string comparison.
        /// The default is true.
        public bool IgnoreCase { get; set; } = true;
        /// Gets the valid values in the set.
        [SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "<Pending>")]
        public IList<string> ValidValues
                if (validValuesGenerator == null)
                    return _validValues;
                var validValuesLocal = validValuesGenerator.GetValidValues();
                if (validValuesLocal == null)
        /// Validates that each parameter argument is present in the specified set.
        /// if element is not in the set
        /// for invalid argument
            string objString = element.ToString();
            foreach (string setString in ValidValues)
                if (CultureInfo.InvariantCulture.CompareInfo.Compare(
                    setString,
                    objString,
                    IgnoreCase ? CompareOptions.IgnoreCase : CompareOptions.None) == 0)
            var errorMessageFormat = string.IsNullOrEmpty(ErrorMessage) ? Metadata.ValidateSetFailure : ErrorMessage;
                "ValidateSetFailure",
                element.ToString(), SetAsString());
        private string SetAsString() => string.Join(CultureInfo.CurrentUICulture.TextInfo.ListSeparator, ValidValues);
        /// Initializes a new instance of the <see cref="ValidateSetAttribute"/> class.
        /// <param name="validValues">List of valid values.</param>
        /// <exception cref="ArgumentNullException">For null arguments.</exception>
        public ValidateSetAttribute(params string[] validValues)
            if (validValues == null)
                throw PSTraceSource.NewArgumentNullException(nameof(validValues));
            if (validValues.Length == 0)
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(validValues), validValues);
            _validValues = validValues;
        /// Valid values are returned dynamically from a custom class implementing
        /// <see cref="IValidateSetValuesGenerator"/>
        /// <param name="valuesGeneratorType">
        /// Class that implements the <see cref="IValidateSetValuesGenerator"/> interface.
        /// <exception cref="ArgumentException">For null arguments.</exception>
        public ValidateSetAttribute(Type valuesGeneratorType)
            // We check 'IsNotPublic' because we don't want allow 'Activator.CreateInstance' create an
            // instance of non-public type.
            if (!typeof(IValidateSetValuesGenerator).IsAssignableFrom(
                valuesGeneratorType) || valuesGeneratorType.IsNotPublic)
                throw PSTraceSource.NewArgumentException(nameof(valuesGeneratorType));
            // Add a valid values generator to the cache.
            // We don't cache valid values; we expect that valid values will be cached in the generator.
            validValuesGenerator = s_ValidValuesGeneratorCache.GetOrAdd(
                valuesGeneratorType, static (key) => (IValidateSetValuesGenerator)Activator.CreateInstance(key));
    /// Allows dynamically generate set of values for <see cref="ValidateSetAttribute"/>
    public interface IValidateSetValuesGenerator
        /// Gets valid values.
        /// <returns>A non-null array of non-null strings.</returns>
        string[] GetValidValues();
    /// Validates that each parameter argument is Trusted data.
    public sealed class ValidateTrustedDataAttribute : ValidateArgumentsAttribute
        /// Validates that the parameter argument is not untrusted.
        /// if the argument is untrusted.
            if (ExecutionContext.HasEverUsedConstrainedLanguage &&
                engineIntrinsics.SessionState.Internal.ExecutionContext.LanguageMode == PSLanguageMode.FullLanguage)
                if (ExecutionContext.IsMarkedAsUntrusted(arguments))
                            "ValidateTrustedDataFailure",
                            Metadata.ValidateTrustedDataFailure,
                            arguments);
                        context: null,
                        title: Metadata.WDACParameterArgNotTrustedLogTitle,
                        message: StringUtil.Format(Metadata.WDACParameterArgNotTrustedMessage, arguments),
                        fqid: "ParameterArgumentNotTrusted",
    #region Allow
    /// Allows a NULL as the argument to a mandatory parameter.
    public sealed class AllowNullAttribute : CmdletMetadataAttribute
        /// Initializes a new instance of the <see cref="AllowNullAttribute"/> class.
        public AllowNullAttribute() { }
    /// Allows an empty string as the argument to a mandatory string parameter.
    public sealed class AllowEmptyStringAttribute : CmdletMetadataAttribute
        /// Initializes a new instance of the <see cref="AllowEmptyStringAttribute"/> class.
        public AllowEmptyStringAttribute() { }
    /// Allows an empty collection as the argument to a mandatory collection parameter.
    public sealed class AllowEmptyCollectionAttribute : CmdletMetadataAttribute
        /// Initializes a new instance of the <see cref="AllowEmptyCollectionAttribute"/> class.
        public AllowEmptyCollectionAttribute() { }
    #endregion Allow
    #region Path validation attributes
    /// Validates that the path has an approved root drive.
    public class ValidateDriveAttribute : ValidateArgumentsAttribute
        private readonly string[] _validRootDrives;
        /// Gets the values in the set.
        public IList<string> ValidRootDrives { get => _validRootDrives; }
        /// Initializes a new instance of the <see cref="ValidateDriveAttribute"/> class.
        /// <param name="validRootDrives">List of approved root drives for path.</param>
        public ValidateDriveAttribute(params string[] validRootDrives)
            if (validRootDrives == null)
                throw PSTraceSource.NewArgumentException(nameof(validRootDrives));
            _validRootDrives = validRootDrives;
        /// Validates path argument.
        /// <param name="engineIntrinsics">Engine intrinsics.</param>
            if (arguments == null)
                    "PathArgumentIsEmpty",
            if (arguments is not string path)
                    "PathArgumentIsNotValid",
                    Metadata.ValidateDrivePathArgNotString);
            var resolvedPath = engineIntrinsics.SessionState.Internal.Globber.GetProviderPath(
                path: path,
                context: new CmdletProviderContext(engineIntrinsics.SessionState.Internal.ExecutionContext),
                isTrusted: true,
                provider: out ProviderInfo providerInfo,
                drive: out PSDriveInfo driveInfo);
            string rootDrive = driveInfo.Name;
            if (string.IsNullOrEmpty(rootDrive))
                    "PathArgumentNoRoot",
                    Metadata.ValidateDrivePathNoRoot);
            bool rootFound = false;
            foreach (var validDrive in _validRootDrives)
                if (rootDrive.Equals(validDrive, StringComparison.OrdinalIgnoreCase))
                    rootFound = true;
            if (!rootFound)
                    "PathRootInvalid",
                    Metadata.ValidateDrivePathFailure,
                    rootDrive, ValidDriveListAsString());
        private string ValidDriveListAsString()
            return string.Join(CultureInfo.CurrentUICulture.TextInfo.ListSeparator, _validRootDrives);
    /// Validates that the path parameter is a User drive.
    public sealed class ValidateUserDriveAttribute : ValidateDriveAttribute
        /// Initializes a new instance of the <see cref="ValidateUserDriveAttribute"/> class.
        public ValidateUserDriveAttribute()
            : base(new string[] { "User" })
    #region NULL validation attributes
    /// Base type of Null Validation attributes.
    public abstract class NullValidationAttributeBase : ValidateArgumentsAttribute
        /// Check if the argument type is a collection.
        protected bool IsArgumentCollection(Type argumentType, out bool isElementValueType)
            isElementValueType = false;
            var information = new ParameterCollectionTypeInformation(argumentType);
            switch (information.ParameterCollectionType)
                // If 'arguments' is an array, or implement 'IList', or implement 'ICollection<>'
                // then we continue to check each element of the collection.
                case ParameterCollectionType.Array:
                case ParameterCollectionType.IList:
                case ParameterCollectionType.ICollectionGeneric:
                    Type elementType = information.ElementType;
                    isElementValueType = elementType != null && elementType.IsValueType;
    /// Validates that the parameters's argument is not null.
    public sealed class ValidateNotNullAttribute : NullValidationAttributeBase
        /// Verifies the argument is not null. If the argument is a collection, verifies that each
        /// element in the collection is not null.
        /// <param name="arguments">The arguments to verify.</param>
        /// true if the argument is valid.
        /// if element is null or a collection with a null element
                    "ArgumentIsNull",
            else if (IsArgumentCollection(arguments.GetType(), out bool isElementValueType))
                // If the element of the collection is of value type, then no need to check for null
                // because a value-type value cannot be null.
                if (isElementValueType)
                IEnumerator enumerator = LanguagePrimitives.GetEnumerator(arguments);
                    object element = enumerator.Current;
                    if (LanguagePrimitives.IsNull(element))
                            Metadata.ValidateNotNullCollectionFailure);
    /// Validates that the parameters's argument is not null, is not an empty string or a
    /// string with white-space characters only, and is not an empty collection.
    public abstract class ValidateNotNullOrAttributeBase : NullValidationAttributeBase
        /// Used to check the type of string validation to perform.
        protected readonly bool _checkWhiteSpace;
        protected ValidateNotNullOrAttributeBase(bool checkWhiteSpace)
            _checkWhiteSpace = checkWhiteSpace;
        /// Validates that the parameters's argument is not null, is not an empty string, and is
        /// not an empty collection. If argument is a collection, each argument is verified.
        /// It can also validate that the parameters's argument is not a string that consists
        /// only of white-space characters.
        /// if the arguments are not valid.
                    Metadata.ValidateNotNullOrEmptyFailure);
            else if (arguments is string str)
                if (_checkWhiteSpace)
                    if (string.IsNullOrWhiteSpace(str))
                            "ArgumentIsEmptyOrWhiteSpace",
                            Metadata.ValidateNotNullOrWhiteSpaceFailure);
                else if (string.IsNullOrEmpty(str))
                if (!isEmpty && !isElementValueType)
                        if (element is string elementAsString)
                                if (string.IsNullOrWhiteSpace(elementAsString))
                                        "ArgumentCollectionContainsEmptyOrWhiteSpace",
                                        Metadata.ValidateNotNullOrWhiteSpaceCollectionFailure);
                            else if (string.IsNullOrEmpty(elementAsString))
                                    "ArgumentCollectionContainsEmpty",
                    } while (enumerator.MoveNext());
            else if (arguments is IDictionary dict)
                if (dict.Count == 0)
    public sealed class ValidateNotNullOrEmptyAttribute : ValidateNotNullOrAttributeBase
        public ValidateNotNullOrEmptyAttribute()
            : base(checkWhiteSpace: false)
    /// Validates that the parameters's argument is not null, is not an empty string, is not a string that
    /// consists only of white-space characters, and is not an empty collection. If argument is a collection,
    /// each argument is verified.
    public sealed class ValidateNotNullOrWhiteSpaceAttribute : ValidateNotNullOrAttributeBase
        public ValidateNotNullOrWhiteSpaceAttribute()
            : base(checkWhiteSpace: true)
    #endregion NULL validation attributes
    #endregion Data validate Attributes
    #region Data Generation Attributes
    /// Serves as the base class for attributes that perform argument transformation.
    /// Argument transformation attributes can be attached to <see cref="Cmdlet"/> and
    /// <see cref="Provider.CmdletProvider"/> parameters to automatically transform the argument
    /// value in some fashion. The transformation might change the object, convert the type, or
    /// even load a file or AD object based on the name. Existing argument transformation attributes
    /// include <see cref="ArgumentTypeConverterAttribute"/>.
    /// Custom argument transformation attributes should derive from
    /// <see cref="ArgumentTransformationAttribute"/> and override the
    /// <see cref="ArgumentTransformationAttribute.Transform"/> abstract method, after which they
    /// can apply the attribute to their parameters.
    /// It is also recommended to override <see cref="object.ToString"/> to return a readable
    /// string similar to the attribute declaration, for example "[ValidateRangeAttribute(5,10)]".
    /// If multiple transformations are defined on a parameter, they will be invoked in series,
    /// each getting the output of the previous transformation.
    /// <seealso cref="ArgumentTypeConverterAttribute"/>
    public abstract class ArgumentTransformationAttribute : CmdletMetadataAttribute
        /// Initializes a new instance of the <see cref="ArgumentTransformationAttribute"/> class.
        protected ArgumentTransformationAttribute()
        /// Method that will be overridden by the subclasses to transform the <paramref name="inputData"/>
        /// parameter argument into some other object that will be used for the parameter's value.
        /// The engine APIs for the context under which the transformation is being made.
        /// <param name="inputData">Parameter argument to mutate.</param>
        /// <returns>The transformed value(s) of <paramref name="inputData"/>.</returns>
        /// <exception cref="ArgumentException">Should be thrown for invalid arguments.</exception>
        /// <exception cref="ArgumentTransformationMetadataException">
        /// Should be thrown for any problems during transformation.
        public abstract object Transform(EngineIntrinsics engineIntrinsics, object inputData);
        /// Transform <paramref name="inputData"/> and track the flow of untrusted object.
        /// NOTE: All internal handling of <see cref="ArgumentTransformationAttribute"/> should use this method to
        /// track the trustworthiness of the data input source by default.
        /// The default value for <paramref name="trackDataInputSource"/> is true.
        /// You should stick to the default value for this parameter in most cases so that data input source is
        /// tracked during the transformation. The only acceptable exception is when this method is used in
        /// Compiler or Binder where you can generate extra code to track input source when it's necessary.
        /// This is to minimize the overhead when tracking is not needed.
        internal object TransformInternal(
            EngineIntrinsics engineIntrinsics,
            object inputData,
            bool trackDataInputSource = true)
            object result = Transform(engineIntrinsics, inputData);
            if (trackDataInputSource && engineIntrinsics != null)
                ExecutionContext.PropagateInputSource(
                    inputData,
                    result,
                    engineIntrinsics.SessionState.Internal.LanguageMode);
        /// The property is only checked when:
        ///   a) The parameter is not mandatory
        ///   b) The argument is null.
        public virtual bool TransformNullOptionalParameters { get => true; }
    #endregion Data Generation Attributes
