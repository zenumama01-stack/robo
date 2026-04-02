using System.Management.Automation.Interpreter;
using TypeTable = System.Management.Automation.Runspaces.TypeTable;
#pragma warning disable 56503
    #region PSMemberInfo
    /// Enumerates all possible types of members.
    [TypeConverter(typeof(LanguagePrimitives.EnumMultipleTypeConverter))]
    public enum PSMemberTypes
        /// An alias to another member.
        AliasProperty = 1,
        /// A property defined as a reference to a method.
        CodeProperty = 2,
        /// A property from the BaseObject.
        Property = 4,
        /// A property defined by a Name-Value pair.
        NoteProperty = 8,
        /// A property defined by script language.
        ScriptProperty = 16,
        /// A set of properties.
        PropertySet = 32,
        /// A method from the BaseObject.
        Method = 64,
        /// A method defined as a reference to another method.
        CodeMethod = 128,
        /// A method defined as a script.
        ScriptMethod = 256,
        /// A member that acts like a Property that takes parameters. This is not consider to be a property or a method.
        ParameterizedProperty = 512,
        /// A set of members.
        MemberSet = 1024,
        /// All events.
        Event = 2048,
        /// All dynamic members (where PowerShell cannot know the type of the member)
        Dynamic = 4096,
        /// Members that are inferred by type inference for PSObject and hashtable.
        InferredProperty = 8192,
        /// All property member types.
        Properties = AliasProperty | CodeProperty | Property | NoteProperty | ScriptProperty | InferredProperty,
        /// All method member types.
        Methods = CodeMethod | Method | ScriptMethod,
        /// All member types.
        All = Properties | Methods | Event | PropertySet | MemberSet | ParameterizedProperty | Dynamic
    /// Enumerator for all possible views available on a PSObject.
    public enum PSMemberViewTypes
        /// Extended methods / properties.
        Extended = 1,
        /// Adapted methods / properties.
        Adapted = 2,
        /// Base methods / properties.
        Base = 4,
        /// All methods / properties.
        All = Extended | Adapted | Base
    /// Match options.
    internal enum MshMemberMatchOptions
        /// No options.
        /// Hidden members should be displayed.
        IncludeHidden = 1,
        /// Only include members with <see cref="PSMemberInfo.ShouldSerialize"/> property set to <see langword="true"/>
        OnlySerializable = 2
    /// Serves as the base class for all members of an PSObject.
    public abstract class PSMemberInfo
        internal object instance;
        internal bool ShouldSerialize { get; set; }
        internal virtual void ReplicateInstance(object particularInstance)
            this.instance = particularInstance;
        internal void SetValueNoConversion(object setValue)
            if (this is not PSProperty thisAsProperty)
                this.Value = setValue;
            thisAsProperty.SetAdaptedValue(setValue, false);
        /// Initializes a new instance of an PSMemberInfo derived class.
        protected PSMemberInfo()
            ShouldSerialize = true;
            IsInstance = true;
        internal void CloneBaseProperties(PSMemberInfo destiny)
            destiny.name = name;
            destiny.IsHidden = IsHidden;
            destiny.IsReservedMember = IsReservedMember;
            destiny.IsInstance = IsInstance;
            destiny.instance = instance;
            destiny.ShouldSerialize = ShouldSerialize;
        /// Gets the member type.
        public abstract PSMemberTypes MemberType { get; }
        /// Gets the member name.
        public string Name => this.name;
        /// Allows a derived class to set the member name...
        protected void SetMemberName(string name)
        /// True if this is one of the reserved members.
        internal bool IsReservedMember { get; set; }
        /// True if the member should be hidden when searching with PSMemberInfoInternalCollection's Match
        /// or enumerating a collection.
        /// This should not be settable as it would make the count of hidden properties in
        /// PSMemberInfoInternalCollection invalid.
        /// For now, we are carefully setting this.isHidden before adding
        /// the members toPSObjectMembersetCollection. In the future, we might need overload for all
        /// PSMemberInfo constructors to take isHidden.
        internal bool IsHidden { get; set; }
        /// True if this member has been added to the instance as opposed to
        /// coming from the adapter or from type data.
        public bool IsInstance { get; internal set; }
        /// Gets and Sets the value of this member.
        /// <exception cref="GetValueException">When getting the value of a property throws an exception.
        /// This exception is also thrown if the property is an <see cref="PSScriptProperty"/> and there
        /// is no Runspace to run the script.</exception>
        /// <exception cref="SetValueException">When setting the value of a property throws an exception.
        /// <exception cref="ExtendedTypeSystemException">When some problem other then getting/setting the value happened.</exception>
        public abstract object Value { get; set; }
        /// Gets the type of the value for this member.
        /// <exception cref="ExtendedTypeSystemException">When there was a problem getting the property.</exception>
        public abstract string TypeNameOfValue { get; }
        /// Returns a new PSMemberInfo that is a copy of this PSMemberInfo.
        /// <returns>A new PSMemberInfo that is a copy of this PSMemberInfo.</returns>
        public abstract PSMemberInfo Copy();
        internal bool MatchesOptions(MshMemberMatchOptions options)
            if (this.IsHidden && ((options & MshMemberMatchOptions.IncludeHidden) == 0))
            if (!this.ShouldSerialize && ((options & MshMemberMatchOptions.OnlySerializable) != 0))
    /// Serves as a base class for all members that behave like properties.
    public abstract class PSPropertyInfo : PSMemberInfo
        /// Initializes a new instance of an PSPropertyInfo derived class.
        protected PSPropertyInfo()
        /// Gets true if this property can be set.
        public abstract bool IsSettable { get; }
        /// Gets true if this property can be read.
        public abstract bool IsGettable { get; }
        internal Exception NewSetValueException(Exception e, string errorId)
            return new SetValueInvocationException(errorId,
                this.Name, e.Message);
        internal Exception NewGetValueException(Exception e, string errorId)
            return new GetValueInvocationException(errorId,
    /// Serves as an alias to another member.
    /// It is permitted to subclass <see cref="PSAliasProperty"/>
    public class PSAliasProperty : PSPropertyInfo
        /// Returns the string representation of this property.
        /// <returns>This property as a string.</returns>
            returnValue.Append(this.Name);
            returnValue.Append(" = ");
            if (ConversionType != null)
                returnValue.Append('(');
                returnValue.Append(ConversionType);
                returnValue.Append(')');
            returnValue.Append(ReferencedMemberName);
        /// Initializes a new instance of PSAliasProperty setting the name of the alias
        /// and the name of the member this alias refers to.
        /// <param name="name">Name of the alias.</param>
        /// <param name="referencedMemberName">Name of the member this alias refers to.</param>
        public PSAliasProperty(string name, string referencedMemberName)
            if (string.IsNullOrEmpty(referencedMemberName))
                throw PSTraceSource.NewArgumentException(nameof(referencedMemberName));
            ReferencedMemberName = referencedMemberName;
        /// Initializes a new instance of PSAliasProperty setting the name of the alias,
        /// the name of the member this alias refers to and the type to convert the referenced
        /// member's value.
        /// <param name="conversionType">The type to convert the referenced member's value.</param>
        public PSAliasProperty(string name, string referencedMemberName, Type conversionType)
            // conversionType is optional and can be null
            ConversionType = conversionType;
        /// Gets the name of the member this alias refers to.
        public string ReferencedMemberName { get; }
        /// Gets the member this alias refers to.
        internal PSMemberInfo ReferencedMember => this.LookupMember(ReferencedMemberName);
        /// Gets the type to convert the referenced member's value. It might be
        /// null when no conversion is done.
        public Type ConversionType { get; private set; }
        #region virtual implementation
        public override PSMemberInfo Copy()
            PSAliasProperty alias = new PSAliasProperty(name, ReferencedMemberName) { ConversionType = ConversionType };
            CloneBaseProperties(alias);
        public override PSMemberTypes MemberType => PSMemberTypes.AliasProperty;
        /// When
        ///     the alias has not been added to an PSObject or
        ///     the alias has a cycle or
        ///     an aliased member is not present
        public override string TypeNameOfValue
                    return ConversionType.FullName;
                return this.ReferencedMember.TypeNameOfValue;
        public override bool IsSettable
                if (this.ReferencedMember is PSPropertyInfo memberProperty)
                    return memberProperty.IsSettable;
        ///     When
        ///         the alias has not been added to an PSObject or
        ///         the alias has a cycle or
        ///         an aliased member is not present
        public override bool IsGettable
                    return memberProperty.IsGettable;
        private PSMemberInfo LookupMember(string name)
            LookupMember(name, new HashSet<string>(StringComparer.OrdinalIgnoreCase), out PSMemberInfo returnValue, out bool hasCycle);
            if (hasCycle)
                throw new
                    ExtendedTypeSystemException(
                        "CycleInAliasLookup",
                        ExtendedTypeSystem.CycleInAlias,
        private void LookupMember(string name, HashSet<string> visitedAliases, out PSMemberInfo returnedMember, out bool hasCycle)
            returnedMember = null;
            if (this.instance == null)
                throw new ExtendedTypeSystemException("AliasLookupMemberOutsidePSObject",
                    ExtendedTypeSystem.AccessMemberOutsidePSObject,
            PSMemberInfo member = PSObject.AsPSObject(this.instance).Properties[name];
                    "AliasLookupMemberNotPresent",
                    ExtendedTypeSystem.MemberNotPresent,
            if (member is not PSAliasProperty aliasMember)
                hasCycle = false;
                returnedMember = member;
            if (visitedAliases.Contains(name))
                hasCycle = true;
            visitedAliases.Add(name);
            LookupMember(aliasMember.ReferencedMemberName, visitedAliases, out returnedMember, out hasCycle);
        /// <exception cref="GetValueException">When getting the value of a property throws an exception.</exception>
        /// <exception cref="SetValueException">When setting the value of a property throws an exception.</exception>
                object returnValue = this.ReferencedMember.Value;
                    returnValue = LanguagePrimitives.ConvertTo(returnValue, ConversionType, CultureInfo.InvariantCulture);
            set => this.ReferencedMember.Value = value;
        #endregion virtual implementation
    /// Serves as a property implemented with references to methods for getter and setter.
    /// It is permitted to subclass <see cref="PSCodeProperty"/>
    public class PSCodeProperty : PSPropertyInfo
            returnValue.Append(this.TypeNameOfValue);
            returnValue.Append('{');
            if (this.IsGettable)
                returnValue.Append("get=");
                returnValue.Append(GetterCodeReference.Name);
                returnValue.Append(';');
            if (this.IsSettable)
                returnValue.Append("set=");
                returnValue.Append(SetterCodeReference.Name);
        /// Called from TypeTableUpdate before SetSetterFromTypeTable is called.
        internal void SetGetterFromTypeTable(Type type, string methodName)
            MethodInfo methodAsMember = null;
                methodAsMember = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
            catch (AmbiguousMatchException)
                // Ignore the AmbiguousMatchException.
                // We will generate error below if we cannot find exactly one match method.
            if (methodAsMember == null)
                    "GetterFormatFromTypeTable",
                    ExtendedTypeSystem.CodePropertyGetterFormat);
            SetGetter(methodAsMember);
        /// Called from TypeTableUpdate after SetGetterFromTypeTable is called.
        internal void SetSetterFromTypeTable(Type type, string methodName)
                    "SetterFormatFromTypeTable",
                    ExtendedTypeSystem.CodePropertySetterFormat);
            SetSetter(methodAsMember, GetterCodeReference);
        /// Used from TypeTable with the internal constructor.
        internal void SetGetter(MethodInfo methodForGet)
            if (methodForGet == null)
                GetterCodeReference = null;
            if (!CheckGetterMethodInfo(methodForGet))
                    "GetterFormat",
            GetterCodeReference = methodForGet;
        internal static bool CheckGetterMethodInfo(MethodInfo methodForGet)
            ParameterInfo[] parameters = methodForGet.GetParameters();
            return methodForGet.IsPublic
                   && methodForGet.IsStatic
                   && methodForGet.ReturnType != typeof(void)
                   && parameters.Length == 1
                   && parameters[0].ParameterType == typeof(PSObject);
        private void SetSetter(MethodInfo methodForSet, MethodInfo methodForGet)
            if (methodForSet == null)
                        "SetterAndGetterNullFormat",
                        ExtendedTypeSystem.CodePropertyGetterAndSetterNull);
                SetterCodeReference = null;
            if (!CheckSetterMethodInfo(methodForSet, methodForGet))
                    "SetterFormat",
            SetterCodeReference = methodForSet;
        internal static bool CheckSetterMethodInfo(MethodInfo methodForSet, MethodInfo methodForGet)
            ParameterInfo[] parameters = methodForSet.GetParameters();
            return methodForSet.IsPublic
                   && methodForSet.IsStatic
                   && methodForSet.ReturnType == typeof(void)
                   && parameters.Length == 2
                   && parameters[0].ParameterType == typeof(PSObject)
                   && (methodForGet == null || methodForGet.ReturnType == parameters[1].ParameterType);
        /// Used from TypeTable to delay setting getter and setter.
        internal PSCodeProperty(string name)
        /// Initializes a new instance of the PSCodeProperty class as a read only property.
        /// <param name="name">Name of the property.</param>
        /// <param name="getterCodeReference">This should be a public static non void method taking one PSObject parameter.</param>
        /// <exception cref="ArgumentException">If name is null or empty or getterCodeReference is null.</exception>
        /// <exception cref="ExtendedTypeSystemException">If getterCodeReference doesn't have the right format.</exception>
        public PSCodeProperty(string name, MethodInfo getterCodeReference)
            if (getterCodeReference == null)
                throw PSTraceSource.NewArgumentNullException(nameof(getterCodeReference));
            SetGetter(getterCodeReference);
        /// Initializes a new instance of the PSCodeProperty class. Setter or getter can be null, but both cannot be null.
        /// <param name="setterCodeReference">This should be a public static void method taking 2 parameters, where the first is an PSObject.</param>
        /// <exception cref="ArgumentException">When methodForGet and methodForSet are null.</exception>
        /// if:
        ///     - getterCodeReference doesn't have the right format,
        ///     - setterCodeReference doesn't have the right format,
        ///     - both getterCodeReference and setterCodeReference are null.
        public PSCodeProperty(string name, MethodInfo getterCodeReference, MethodInfo setterCodeReference)
            if (getterCodeReference == null && setterCodeReference == null)
                throw PSTraceSource.NewArgumentNullException("getterCodeReference setterCodeReference");
            SetSetter(setterCodeReference, getterCodeReference);
        /// Gets the method used for the properties' getter. It might be null.
        public MethodInfo GetterCodeReference { get; private set; }
        /// Gets the method used for the properties' setter. It might be null.
        public MethodInfo SetterCodeReference { get; private set; }
            PSCodeProperty property = new PSCodeProperty(name, GetterCodeReference, SetterCodeReference);
            CloneBaseProperties(property);
        public override PSMemberTypes MemberType => PSMemberTypes.CodeProperty;
        public override bool IsSettable => this.SetterCodeReference != null;
        public override bool IsGettable => GetterCodeReference != null;
        /// <exception cref="GetValueException">When getting and there is no getter or when the getter throws an exception.</exception>
        /// <exception cref="SetValueException">When setting and there is no setter or when the setter throws an exception.</exception>
                if (GetterCodeReference == null)
                        "GetWithoutGetterFromCodePropertyValue",
                        ExtendedTypeSystem.GetWithoutGetterException,
                    return GetterCodeReference.Invoke(null, new object[] { this.instance });
                        "CatchFromCodePropertyGetTI",
                        this.name,
                    if (e is GetValueException)
                        "CatchFromCodePropertyGet",
                if (SetterCodeReference == null)
                        "SetWithoutSetterFromCodeProperty",
                        ExtendedTypeSystem.SetWithoutSetterException,
                    SetterCodeReference.Invoke(null, new object[] { this.instance, value });
                        "CatchFromCodePropertySetTI",
                    if (e is SetValueException)
                        "CatchFromCodePropertySet",
        /// <exception cref="GetValueException">If there is no property getter.</exception>
                        "GetWithoutGetterFromCodePropertyTypeOfValue",
                return GetterCodeReference.ReturnType.FullName;
    /// Type used to capture the properties inferred from Hashtable and PSObject.
    internal class PSInferredProperty : PSPropertyInfo
        public PSInferredProperty(string name, PSTypeName typeName)
        internal PSTypeName TypeName { get; }
        public override PSMemberTypes MemberType => PSMemberTypes.InferredProperty;
        public override object Value { get; set; }
        public override string TypeNameOfValue => TypeName.Name;
        public override PSMemberInfo Copy() => new PSInferredProperty(Name, TypeName);
        public override bool IsSettable => false;
        public override bool IsGettable => false;
        public override string ToString() => $"{ToStringCodeMethods.Type(TypeName.Type)} {Name}";
    /// Used to access the adapted or base properties from the BaseObject.
    public class PSProperty : PSPropertyInfo
            if (this.isDeserialized)
                returnValue.Append(" {get;set;}");
            Diagnostics.Assert((this.baseObject != null) && (this.adapter != null), "if it is deserialized, it should have all these properties set");
            return adapter.BasePropertyToString(this);
        /// Used by the adapters to keep intermediate data used between DoGetProperty and
        /// DoGetValue or DoSetValue.
        internal string typeOfValue;
        internal object serializedValue;
        internal bool isDeserialized;
        /// This will be either instance.adapter or instance.clrAdapter.
        internal Adapter adapter;
        internal object adapterData;
        internal object baseObject;
        /// Constructs a property from a serialized value.
        /// <param name="serializedValue">Value of the property.</param>
        internal PSProperty(string name, object serializedValue)
            this.isDeserialized = true;
            this.serializedValue = serializedValue;
        /// Constructs this property.
        /// <param name="adapter">Adapter used in DoGetProperty.</param>
        /// <param name="baseObject">Object passed to DoGetProperty.</param>
        /// <param name="adapterData">Adapter specific data.</param>
        internal PSProperty(string name, Adapter adapter, object baseObject, object adapterData)
            this.adapter = adapter;
            this.adapterData = adapterData;
            this.baseObject = baseObject;
            PSProperty property = new PSProperty(this.name, this.adapter, this.baseObject, this.adapterData);
            property.typeOfValue = this.typeOfValue;
            property.serializedValue = this.serializedValue;
            property.isDeserialized = this.isDeserialized;
        public override PSMemberTypes MemberType => PSMemberTypes.Property;
        private object GetAdaptedValue()
                return serializedValue;
            object o = adapter.BasePropertyGet(this);
        internal void SetAdaptedValue(object setValue, bool shouldConvert)
                serializedValue = setValue;
            adapter.BasePropertySet(this, setValue, shouldConvert);
        /// Gets or sets the value of this property.
            get => GetAdaptedValue();
            set => SetAdaptedValue(value, true);
                return adapter.BasePropertyIsSettable(this);
                return adapter.BasePropertyIsGettable(this);
                    if (serializedValue == null)
                    if (serializedValue is PSObject serializedValueAsPSObject)
                        var typeNames = serializedValueAsPSObject.InternalTypeNames;
                        if ((typeNames != null) && (typeNames.Count >= 1))
                            // type name at 0-th index is the most specific type (i.e. deserialized.system.io.directoryinfo)
                            // type names at other indices are less specific (i.e. deserialized.system.object)
                    return serializedValue.GetType().FullName;
                return adapter.BasePropertyType(this);
    /// A property created by a user-defined PSPropertyAdapter.
    public class PSAdaptedProperty : PSProperty
        /// Creates a property for the given base object.
        /// <param name="tag">An adapter can use this object to keep any arbitrary data it needs.</param>
        public PSAdaptedProperty(string name, object tag)
            : base(name, null, null, tag)
            // Note that the constructor sets the adapter and base object to null; the ThirdPartyAdapter managing this property must set these values
        internal PSAdaptedProperty(string name, Adapter adapter, object baseObject, object adapterData)
            : base(name, adapter, baseObject, adapterData)
        /// Copy an adapted property.
            PSAdaptedProperty property = new PSAdaptedProperty(this.name, this.adapter, this.baseObject, this.adapterData);
        /// Gets the object the property belongs to.
        public object BaseObject => this.baseObject;
        /// Gets the data attached to this property.
        public object Tag => this.adapterData;
    /// Serves as a property that is a simple name-value pair.
    public class PSNoteProperty : PSPropertyInfo
            returnValue.Append(GetDisplayTypeNameOfValue(this.Value));
            returnValue.Append('=');
            returnValue.Append(this.noteValue == null ? "null" : this.noteValue.ToString());
        internal object noteValue;
        /// Initializes a new instance of the PSNoteProperty class.
        /// <param name="value">Value of the property.</param>
        /// <exception cref="ArgumentException">For an empty or null name.</exception>
        public PSNoteProperty(string name, object value)
            // value can be null
            this.noteValue = value;
            PSNoteProperty property = new PSNoteProperty(this.name, this.noteValue);
        /// Gets PSMemberTypes.NoteProperty.
        public override PSMemberTypes MemberType => PSMemberTypes.NoteProperty;
        /// Gets true since the value of an PSNoteProperty can always be set.
        public override bool IsSettable => this.IsInstance;
        /// Gets true since the value of an PSNoteProperty can always be obtained.
        public override bool IsGettable => true;
            get => this.noteValue;
                if (!this.IsInstance)
                    throw new SetValueException("ChangeValueOfStaticNote",
                        ExtendedTypeSystem.ChangeStaticMember,
                object val = this.Value;
                    return typeof(object).FullName;
                if (val is PSObject valAsPSObject)
                    var typeNames = valAsPSObject.InternalTypeNames;
                        // type name at 0-th index is the most specific type (i.e. system.string)
                        // type names at other indices are less specific (i.e. system.object)
                return val.GetType().FullName;
        internal static string GetDisplayTypeNameOfValue(object val)
            string displayTypeName = null;
                    displayTypeName = typeNames[0];
            if (string.IsNullOrEmpty(displayTypeName))
                displayTypeName = val == null
                    ? "object"
                    : ToStringCodeMethods.Type(val.GetType(), dropNamespaces: true);
            return displayTypeName;
    /// It is permitted to subclass <see cref="PSNoteProperty"/>
    public class PSVariableProperty : PSNoteProperty
            returnValue.Append(GetDisplayTypeNameOfValue(_variable.Value));
            returnValue.Append(_variable.Name);
            returnValue.Append(_variable.Value ?? "null");
        internal PSVariable _variable;
        /// Initializes a new instance of the PSVariableProperty class. This is
        /// a subclass of the NoteProperty that wraps a variable instead of a simple value.
        /// <param name="variable">The variable to wrap.</param>
        public PSVariableProperty(PSVariable variable)
            : base(variable?.Name, null)
            _variable = variable ?? throw PSTraceSource.NewArgumentException(nameof(variable));
        /// Returns a new PSMemberInfo that is a copy of this PSMemberInfo,
        /// Note that it returns another reference to the variable, not a reference
        /// to a new variable...
            PSNoteProperty property = new PSVariableProperty(_variable);
        /// True if the underlying variable is settable...
        public override bool IsSettable => (_variable.Options & (ScopedItemOptions.Constant | ScopedItemOptions.ReadOnly)) == ScopedItemOptions.None;
            get => _variable.Value;
                _variable.Value = value;
                object val = _variable.Value;
    /// Serves as a property implemented with getter and setter scripts.
    /// It is permitted to subclass <see cref="PSScriptProperty"/>
    public class PSScriptProperty : PSPropertyInfo
                returnValue.Append(this.GetterScript.ToString());
                returnValue.Append(this.SetterScript.ToString());
        private readonly PSLanguageMode? _languageMode;
        private readonly string _getterScriptText;
        private ScriptBlock _getterScript;
        private readonly string _setterScriptText;
        private ScriptBlock _setterScript;
        private bool _shouldCloneOnAccess;
        /// Gets the script used for the property getter. It might be null.
        public ScriptBlock GetterScript
                // If we don't have a script block for the getter, see if we
                // have the text for it (to support delayed script compilation).
                if ((_getterScript == null) && (_getterScriptText != null))
                    _getterScript = ScriptBlock.Create(_getterScriptText);
                    if (_languageMode.HasValue)
                        _getterScript.LanguageMode = _languageMode;
                    _getterScript.DebuggerStepThrough = true;
                if (_getterScript == null)
                if (_shouldCloneOnAccess)
                    // returning a clone as TypeTable might be shared between multiple
                    // runspaces and ScriptBlock is not shareable. We decided to
                    // Clone as needed instead of Cloning whenever a shared TypeTable is
                    // attached to a Runspace to save on Memory.
                    ScriptBlock newGetterScript = _getterScript.Clone();
                    newGetterScript.LanguageMode = _getterScript.LanguageMode;
                    return newGetterScript;
                    return _getterScript;
        /// Gets the script used for the property setter. It might be null.
        public ScriptBlock SetterScript
                // If we don't have a script block for the setter, see if we
                if ((_setterScript == null) && (_setterScriptText != null))
                    _setterScript = ScriptBlock.Create(_setterScriptText);
                        _setterScript.LanguageMode = _languageMode;
                    _setterScript.DebuggerStepThrough = true;
                if (_setterScript == null)
                    ScriptBlock newSetterScript = _setterScript.Clone();
                    newSetterScript.LanguageMode = _setterScript.LanguageMode;
                    return newSetterScript;
                    return _setterScript;
        /// Initializes an instance of the PSScriptProperty class as a read only property.
        /// <param name="getterScript">Script to be used for the property getter. $this will be this PSObject.</param>
        public PSScriptProperty(string name, ScriptBlock getterScript)
            _getterScript = getterScript ?? throw PSTraceSource.NewArgumentNullException(nameof(getterScript));
        /// Initializes an instance of the PSScriptProperty class as a read only
        /// property. getterScript or setterScript can be null, but not both.
        /// <param name="name">Name of this property.</param>
        /// <param name="setterScript">Script to be used for the property setter. $this will be this PSObject and $args(1) will be the value to set.</param>
        public PSScriptProperty(string name, ScriptBlock getterScript, ScriptBlock setterScript)
            if (getterScript == null && setterScript == null)
                // we only do not allow both getterScript and setterScript to be null
                throw PSTraceSource.NewArgumentException("getterScript setterScript");
            if (getterScript != null)
                getterScript.DebuggerStepThrough = true;
            if (setterScript != null)
                setterScript.DebuggerStepThrough = true;
            _getterScript = getterScript;
            _setterScript = setterScript;
        /// property, using the text of the properties to support lazy initialization.
        /// <param name="languageMode">Language mode to be used during script block evaluation.</param>
        internal PSScriptProperty(string name, string getterScript, string setterScript, PSLanguageMode? languageMode)
            _getterScriptText = getterScript;
            _setterScriptText = setterScript;
            _languageMode = languageMode;
        internal PSScriptProperty(string name, ScriptBlock getterScript, ScriptBlock setterScript, bool shouldCloneOnAccess)
            : this(name, getterScript, setterScript)
            _shouldCloneOnAccess = shouldCloneOnAccess;
        internal PSScriptProperty(string name, string getterScript, string setterScript, PSLanguageMode? languageMode, bool shouldCloneOnAccess)
            : this(name, getterScript, setterScript, languageMode)
            var property = new PSScriptProperty(name, this.GetterScript, this.SetterScript) { _shouldCloneOnAccess = _shouldCloneOnAccess };
        public override PSMemberTypes MemberType => PSMemberTypes.ScriptProperty;
        public override bool IsSettable => this._setterScript != null || this._setterScriptText != null;
        public override bool IsGettable => this._getterScript != null || this._getterScriptText != null;
        /// Gets and Sets the value of this property.
        /// <exception cref="GetValueException">When getting and there is no getter,
        /// when the getter throws an exception or when there is no Runspace to run the script.
        /// <exception cref="SetValueException">When setting and there is no setter,
        /// when the setter throws an exception or when there is no Runspace to run the script.</exception>
                if (this.GetterScript == null)
                    throw new GetValueException("GetWithoutGetterFromScriptPropertyValue",
                return InvokeGetter(this.instance);
                if (this.SetterScript == null)
                    throw new SetValueException("SetWithoutSetterFromScriptProperty",
                InvokeSetter(this.instance, value);
        internal object InvokeSetter(object scriptThis, object value)
                SetterScript.DoInvokeReturnAsIs(
                    scriptThis: scriptThis,
                    args: new[] { value });
                throw NewSetValueException(e, "ScriptSetValueRuntimeException");
                // The debugger is terminating the execution; let the exception bubble up
            catch (FlowControlException e)
                throw NewSetValueException(e, "ScriptSetValueFlowControlException");
                throw NewSetValueException(e, "ScriptSetValueInvalidOperationException");
        internal object InvokeGetter(object scriptThis)
                return GetterScript.DoInvokeReturnAsIs(
                    errorHandlingBehavior: ScriptBlock.ErrorHandlingBehavior.SwallowErrors,
                throw NewGetValueException(e, "ScriptGetValueRuntimeException");
                throw NewGetValueException(e, "ScriptGetValueFlowControlException");
                throw NewGetValueException(e, "ScriptgetValueInvalidOperationException");
        /// Gets the type of the value for this member. Currently this always returns typeof(object).FullName.
                if ((this.GetterScript != null) &&
                    (this.GetterScript.OutputType.Count > 0))
                    return this.GetterScript.OutputType[0].Name;
    internal class PSMethodInvocationConstraints
        internal PSMethodInvocationConstraints(
            Type methodTargetType,
            Type[] parameterTypes)
            : this(methodTargetType, parameterTypes, genericTypeParameters: null)
            Type[] parameterTypes,
            object[] genericTypeParameters)
            MethodTargetType = methodTargetType;
            ParameterTypes = parameterTypes;
            GenericTypeParameters = genericTypeParameters;
        /// If <see langword="null"/> then there are no constraints
        public Type MethodTargetType { get; }
        public Type[] ParameterTypes { get; }
        /// Gets the generic type parameters for the method invocation.
        public object[] GenericTypeParameters { get; }
        internal static bool EqualsForCollection<T>(ICollection<T> xs, ICollection<T> ys)
            if (xs == null)
                return ys == null;
            if (ys == null)
            if (xs.Count != ys.Count)
            return xs.SequenceEqual(ys);
        public bool Equals(PSMethodInvocationConstraints other)
            if (ReferenceEquals(this, other))
            if (other.MethodTargetType != this.MethodTargetType)
            if (!EqualsForCollection(ParameterTypes, other.ParameterTypes))
            if (!EqualsForCollection(GenericTypeParameters, other.GenericTypeParameters))
            if (obj is null)
            if (ReferenceEquals(this, obj))
            if (obj.GetType() != typeof(PSMethodInvocationConstraints))
            return Equals((PSMethodInvocationConstraints)obj);
            => HashCode.Combine(MethodTargetType, ParameterTypes.SequenceGetHashCode(), GenericTypeParameters.SequenceGetHashCode());
            if (MethodTargetType is not null)
                sb.Append("this: ");
                sb.Append(ToStringCodeMethods.Type(MethodTargetType, dropNamespaces: true));
                separator = " ";
            if (GenericTypeParameters is not null)
                sb.Append(separator);
                sb.Append("genericTypeParams: ");
                separator = string.Empty;
                foreach (object parameter in GenericTypeParameters)
                    switch (parameter)
                        case Type paramType:
                            sb.Append(ToStringCodeMethods.Type(paramType, dropNamespaces: true));
                        case ITypeName paramTypeName:
                            sb.Append(paramTypeName.ToString());
                            throw new ArgumentException("Unexpected value");
            if (ParameterTypes is not null)
                sb.Append("args: ");
                foreach (var p in ParameterTypes)
                    sb.Append(ToStringCodeMethods.Type(p, dropNamespaces: true));
            if (sb.Length == 0)
                sb.Append("<empty>");
    /// Serves as a base class for all members that behave like methods.
    public abstract class PSMethodInfo : PSMemberInfo
        /// Initializes a new instance of a class derived from PSMethodInfo.
        protected PSMethodInfo()
        /// Invokes the appropriate method overload for the given arguments and returns its result.
        /// <param name="arguments">Arguments to the method.</param>
        /// <returns>Return value from the method.</returns>
        /// <exception cref="ArgumentException">If arguments is null.</exception>
        /// <exception cref="MethodException">For problems finding an appropriate method for the arguments.</exception>
        /// <exception cref="MethodInvocationException">For exceptions invoking the method.
        /// This exception is also thrown for an <see cref="PSScriptMethod"/> when there is no Runspace to run the script.</exception>
        public abstract object Invoke(params object[] arguments);
        /// Gets a list of all the overloads for this method.
        public abstract Collection<string> OverloadDefinitions { get; }
        /// Gets the value of this member. The getter returns the PSMethodInfo itself.
        /// <exception cref="ExtendedTypeSystemException">When setting the member.</exception>
        /// This is not the returned value of the method even for Methods with no arguments.
        /// The getter returns this (the PSMethodInfo itself). The setter is not supported.
        public sealed override object Value
            get => this;
            set => throw new ExtendedTypeSystemException("CannotChangePSMethodInfoValue",
                ExtendedTypeSystem.CannotSetValueForMemberType,
                this.GetType().FullName);
    /// Serves as a method implemented with a reference to another method.
    /// It is permitted to subclass <see cref="PSCodeMethod"/>
    public class PSCodeMethod : PSMethodInfo
        /// Returns the string representation of this member.
            foreach (string overload in OverloadDefinitions)
        private MethodInformation[] _codeReferenceMethodInformation;
        internal static bool CheckMethodInfo(MethodInfo method)
            return method.IsStatic
                   && method.IsPublic
                   && parameters.Length != 0
        internal void SetCodeReference(Type type, string methodName)
                throw new ExtendedTypeSystemException("WrongMethodFormatFromTypeTable", null,
                    ExtendedTypeSystem.CodeMethodMethodFormat);
            CodeReference = methodAsMember;
            if (!CheckMethodInfo(CodeReference))
                throw new ExtendedTypeSystemException("WrongMethodFormat", null, ExtendedTypeSystem.CodeMethodMethodFormat);
        /// Used from TypeTable.
        internal PSCodeMethod(string name)
        /// Initializes a new instance of the PSCodeMethod class.
        /// <param name="codeReference">This should be a public static method where the first parameter is an PSObject.</param>
        /// <exception cref="ExtendedTypeSystemException">If the codeReference does not have the right format.</exception>
        public PSCodeMethod(string name, MethodInfo codeReference)
            if (codeReference == null)
                throw PSTraceSource.NewArgumentNullException(nameof(codeReference));
            if (!CheckMethodInfo(codeReference))
            CodeReference = codeReference;
        /// Gets the method referenced by this PSCodeMethod.
        public MethodInfo CodeReference { get; private set; }
            PSCodeMethod member = new PSCodeMethod(name, CodeReference);
            CloneBaseProperties(member);
        public override PSMemberTypes MemberType => PSMemberTypes.CodeMethod;
        /// Invokes CodeReference method and returns its results.
        /// <exception cref="MethodException">
        ///         could CodeReference cannot match the given argument count or
        ///         could not convert an argument to the type required
        /// <exception cref="MethodInvocationException">For exceptions invoking the CodeReference.</exception>
        public override object Invoke(params object[] arguments)
                throw PSTraceSource.NewArgumentNullException(nameof(arguments));
            object[] newArguments = new object[arguments.Length + 1];
            newArguments[0] = this.instance;
                newArguments[i + 1] = arguments[i];
            _codeReferenceMethodInformation ??= DotNetAdapter.GetMethodInformationArray(new[] { CodeReference });
            Adapter.GetBestMethodAndArguments(CodeReference.Name, _codeReferenceMethodInformation, newArguments, out object[] convertedArguments);
            return DotNetAdapter.AuxiliaryMethodInvoke(null, convertedArguments, _codeReferenceMethodInformation[0], newArguments);
        /// Gets the definition for CodeReference.
        public override Collection<string> OverloadDefinitions => new Collection<string>
            DotNetAdapter.GetMethodInfoOverloadDefinition(null, CodeReference, 0)
        /// Gets the type of the value for this member. Currently this always returns typeof(PSCodeMethod).FullName.
        public override string TypeNameOfValue => typeof(PSCodeMethod).FullName;
    /// Serves as a method implemented with a script.
    /// It is permitted to subclass <see cref="PSScriptMethod"/>
    public class PSScriptMethod : PSMethodInfo
            returnValue.Append("();");
        private readonly ScriptBlock _script;
        /// Gets the script implementing this PSScriptMethod.
        public ScriptBlock Script
                    ScriptBlock newScript = _script.Clone();
                    newScript.LanguageMode = _script.LanguageMode;
                    return newScript;
        /// Initializes a new instance of PSScriptMethod.
        /// <param name="name">Name of the method.</param>
        /// <param name="script">Script to be used when calling the method.</param>
        public PSScriptMethod(string name, ScriptBlock script)
            _script = script ?? throw PSTraceSource.NewArgumentNullException(nameof(script));
        /// <param name="script"></param>
        /// <param name="shouldCloneOnAccess">
        /// Used by TypeTable.
        /// TypeTable might be shared between multiple runspaces and
        /// ScriptBlock is not shareable. We decided to Clone as needed
        /// instead of Cloning whenever a shared TypeTable is attached
        /// to a Runspace to save on Memory.
        internal PSScriptMethod(string name, ScriptBlock script, bool shouldCloneOnAccess)
            : this(name, script)
        /// Invokes Script method and returns its results.
        /// <exception cref="MethodInvocationException">For exceptions invoking the Script or if there is no Runspace to run the script.</exception>
            return InvokeScript(Name, _script, this.instance, arguments);
        internal static object InvokeScript(string methodName, ScriptBlock script, object @this, object[] arguments)
                return script.DoInvokeReturnAsIs(
                    scriptThis: @this,
                    args: arguments);
                    "ScriptMethodRuntimeException",
                    methodName, arguments.Length, e.Message);
                    "ScriptMethodFlowControlException",
                    "ScriptMethodInvalidOperationException",
        public override Collection<string> OverloadDefinitions
                Collection<string> retValue = new Collection<string> { this.ToString() };
            var method = new PSScriptMethod(this.name, _script) { _shouldCloneOnAccess = _shouldCloneOnAccess };
            CloneBaseProperties(method);
        public override PSMemberTypes MemberType => PSMemberTypes.ScriptMethod;
        public override string TypeNameOfValue => typeof(object).FullName;
    /// Used to access the adapted or base methods from the BaseObject.
    /// It is permitted to subclass <see cref="PSMethod"/>
    public class PSMethod : PSMethodInfo
        internal override void ReplicateInstance(object particularInstance)
            base.ReplicateInstance(particularInstance);
            baseObject = particularInstance;
            return _adapter.BaseMethodToString(this);
        internal Adapter _adapter;
        /// Constructs this method.
        /// <param name="name">Name.</param>
        /// <param name="adapter">Adapter to be used invoking.</param>
        /// <param name="baseObject">BaseObject for the methods.</param>
        /// <param name="adapterData">AdapterData from adapter.GetMethodData.</param>
        internal PSMethod(string name, Adapter adapter, object baseObject, object adapterData)
            this._adapter = adapter;
        /// Constructs a PSMethod.
        /// <param name="isSpecial">True if this member is a special member, false otherwise.</param>
        /// <param name="isHidden">True if this member is hidden, false otherwise.</param>
        internal PSMethod(string name, Adapter adapter, object baseObject, object adapterData, bool isSpecial, bool isHidden)
            : this(name, adapter, baseObject, adapterData)
            this.IsSpecial = isSpecial;
            this.IsHidden = isHidden;
            PSMethod member = new PSMethod(this.name, _adapter, this.baseObject, this.adapterData, this.IsSpecial, this.IsHidden);
        public override PSMemberTypes MemberType => PSMemberTypes.Method;
        /// <exception cref="MethodInvocationException">For exceptions invoking the method.</exception>
            return this.Invoke(null, arguments);
        /// <param name="invocationConstraints">Constraints.</param>
        internal object Invoke(PSMethodInvocationConstraints invocationConstraints, params object[] arguments)
            return _adapter.BaseMethodInvoke(this, invocationConstraints, arguments);
        public override Collection<string> OverloadDefinitions => _adapter.BaseMethodDefinitions(this);
        /// Gets the type of the value for this member. This always returns typeof(PSMethod).FullName.
        public override string TypeNameOfValue => typeof(PSMethod).FullName;
        /// True if the method is a special method like GET/SET property accessor methods.
        internal bool IsSpecial { get; }
        internal static PSMethod Create(string name, DotNetAdapter dotNetInstanceAdapter, object baseObject, DotNetAdapter.MethodCacheEntry method)
            return Create(name, dotNetInstanceAdapter, baseObject, method, false, false);
        internal static PSMethod Create(string name, DotNetAdapter dotNetInstanceAdapter, object baseObject, DotNetAdapter.MethodCacheEntry method, bool isSpecial, bool isHidden)
            if (method[0].method is ConstructorInfo)
                // Constructor cannot be converted to a delegate, so just return a simple PSMethod instance
                return new PSMethod(name, dotNetInstanceAdapter, baseObject, method, isSpecial, isHidden);
            method.PSMethodCtor ??= CreatePSMethodConstructor(method.methodInformationStructures);
            return method.PSMethodCtor.Invoke(name, dotNetInstanceAdapter, baseObject, method, isSpecial, isHidden);
        private static Type GetMethodGroupType(MethodInfo methodInfo)
            if (methodInfo.DeclaringType.IsGenericTypeDefinition)
                // If the method is from a generic type definition, consider it not convertible.
                return typeof(Func<PSNonBindableType>);
            if (methodInfo.IsGenericMethodDefinition)
                // For a generic method, it's possible to infer the generic parameters based on the target delegate.
                // However, we don't yet handle generic methods in PSMethod-to-Delegate conversion, so for now, we
                // don't produce the metadata type that represents the signature of a generic method.
                // Say one day we want to support generic method in PSMethod-to-Delegate conversion and need to produce
                // the metadata type, we should use the generic parameter types from the MethodInfo directly to construct
                // the Func<> metadata type. See the concept shown in the following scripts:
                //    $class = "public class Zoo { public static T GetName<T>(int index, T input) { return default(T); } }"
                //    Add-Type -TypeDefinition $class
                //    $method = [Zoo].GetMethod("GetName")
                //    $allTypes = $method.GetParameters().ParameterType + $method.ReturnType
                //    $metadataType = [Func`3].MakeGenericType($allTypes)
                // In this way, '$metadataType.ContainsGenericParameters' returns 'True', indicating it represents a generic method.
                // And also, given a generic argument type from `$metadataType.GetGenericArguments()`, it's easy to tell if it's a
                // generic parameter (for example, 'T') based on the property 'IsGenericParameter'.
                // Moreover, it's also easy to get constraints of the generic parameter, via 'GetGenericParameterConstraints()'
                // and 'GenericParameterAttributes'.
            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length > 16)
                // Too many parameters, an unlikely scenario.
                var methodTypes = new Type[parameterInfos.Length + 1];
                for (int i = 0; i < parameterInfos.Length; i++)
                    var parameterInfo = parameterInfos[i];
                    Type parameterType = parameterInfo.ParameterType;
                    methodTypes[i] = GetPSMethodProjectedType(parameterType, parameterInfo.IsOut);
                methodTypes[parameterInfos.Length] = GetPSMethodProjectedType(methodInfo.ReturnType);
                return DelegateHelpers.MakeDelegate(methodTypes);
        private static Type GetPSMethodProjectedType(Type type, bool isOut = false)
            if (type == typeof(void))
                return typeof(VOID);
            if (type == typeof(TypedReference))
                return typeof(PSTypedReference);
                var elementType = GetPSMethodProjectedType(type.GetElementType());
                type = isOut ? typeof(PSOutParameter<>).MakeGenericType(elementType)
                             : typeof(PSReference<>).MakeGenericType(elementType);
                type = typeof(PSPointer<>).MakeGenericType(elementType);
        private static Func<string, DotNetAdapter, object, object, bool, bool, PSMethod> CreatePSMethodConstructor(MethodInformation[] methods)
            // Produce the PSMethod creator for MethodInfo objects
            var types = new Type[methods.Length];
                types[i] = GetMethodGroupType((MethodInfo)methods[i].method);
            var methodGroupType = CreateMethodGroup(types, 0, types.Length);
            Type psMethodType = typeof(PSMethod<>).MakeGenericType(methodGroupType);
            var delegateType = typeof(Func<string, DotNetAdapter, object, object, bool, bool, PSMethod>);
            return (Func<string, DotNetAdapter, object, object, bool, bool, PSMethod>)Delegate.CreateDelegate(delegateType,
                psMethodType.GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static));
        private static Type CreateMethodGroup(Type[] sourceTypes, int start, int count)
            var types = sourceTypes;
            if (count != sourceTypes.Length)
                types = new Type[count];
                Array.Copy(sourceTypes, start, types, 0, count);
                case 1: return typeof(MethodGroup<>).MakeGenericType(types);
                case 2: return typeof(MethodGroup<,>).MakeGenericType(types);
                case 3: return typeof(MethodGroup<,>).MakeGenericType(types[0], CreateMethodGroup(types, 1, 2));
                case 4: return typeof(MethodGroup<,,,>).MakeGenericType(types);
                case int i when i < 8: return typeof(MethodGroup<,,,>).MakeGenericType(types[0], types[1], types[2], CreateMethodGroup(types, 3, i - 3));
                case 8: return typeof(MethodGroup<,,,,,,,>).MakeGenericType(types);
                case int i when i < 16:
                    return typeof(MethodGroup<,,,,,,,>).MakeGenericType(types[0], types[1], types[2], types[3], types[4], types[5], types[6], CreateMethodGroup(types, 7, i - 7));
                case 16: return typeof(MethodGroup<,,,,,,,,,,,,,,,>).MakeGenericType(types);
                case int i when i < 32:
                    return typeof(MethodGroup<,,,,,,,,,,,,,,,>).MakeGenericType(types[0], types[1], types[2], types[3], types[4], types[5], types[6], types[7], types[8], types[9], types[10],
                        types[11], types[12], types[13], types[14], CreateMethodGroup(types, 15, i - 15));
                case 32: return typeof(MethodGroup<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types);
                    return typeof(MethodGroup<,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,>).MakeGenericType(types[0], types[1], types[2], types[3], types[4], types[5], types[6], types[7], types[8],
                        types[9], types[10], types[11], types[12], types[13], types[14], types[15], types[16], types[17], types[18], types[19], types[20], types[21], types[22], types[23],
                        types[24], types[25], types[26], types[27], types[28], types[29], types[30], CreateMethodGroup(sourceTypes, start + 31, count - 31));
    internal abstract class PSNonBindableType
    internal class VOID
    internal class PSOutParameter<T>
    internal struct PSPointer<T>
    internal struct PSTypedReference
    internal abstract class MethodGroup
    internal class MethodGroup<T1> : MethodGroup
    internal class MethodGroup<T1, T2> : MethodGroup
    internal class MethodGroup<T1, T2, T3, T4> : MethodGroup
    internal class MethodGroup<T1, T2, T3, T4, T5, T6, T7, T8> : MethodGroup
    internal class MethodGroup<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : MethodGroup
    internal class MethodGroup<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31,
        T32> : MethodGroup
    internal struct PSMethodSignatureEnumerator : IEnumerator<Type>
        private int _currentIndex;
        private readonly Type _t;
        internal PSMethodSignatureEnumerator(Type t)
            Diagnostics.Assert(t.IsSubclassOf(typeof(PSMethod)), "Must be a PSMethod<MethodGroup<>>");
            _t = t.GenericTypeArguments[0];
            Current = null;
            _currentIndex = -1;
            _currentIndex++;
            return MoveNext(_t, _currentIndex);
        private bool MoveNext(Type type, int index)
            var genericTypeArguments = type.GenericTypeArguments;
            var length = genericTypeArguments.Length;
            if (index < length - 1)
                Current = genericTypeArguments[index];
            var t = genericTypeArguments[length - 1];
            if (t.IsSubclassOf(typeof(MethodGroup)))
                var remaining = index - (length - 1);
                return MoveNext(t, remaining);
            if (index >= length)
            Current = t;
        public Type Current { get; private set; }
        object IEnumerator.Current => Current;
    internal sealed class PSMethod<T> : PSMethod
            PSMethod member = new PSMethod<T>(this.name, this._adapter, this.baseObject, this.adapterData, this.IsSpecial, this.IsHidden);
            : base(name, adapter, baseObject, adapterData, isSpecial, isHidden)
        /// Helper factory function since we cannot bind a delegate to a ConstructorInfo.
        internal static PSMethod<T> Create(string name, Adapter adapter, object baseObject, object adapterData, bool isSpecial, bool isHidden)
            return new PSMethod<T>(name, adapter, baseObject, adapterData, isSpecial, isHidden);
    /// Used to access parameterized properties from the BaseObject.
    /// It is permitted to subclass <see cref="PSParameterizedProperty"/>
    public class PSParameterizedProperty : PSMethodInfo
            Diagnostics.Assert((this.baseObject != null) && (this.adapter != null) && (this.adapterData != null), "it should have all these properties set");
            return this.adapter.BaseParameterizedPropertyToString(this);
        /// Constructs this parameterized property.
        /// <param name="adapter">Adapter used in DoGetMethod.</param>
        /// <param name="baseObject">Object passed to DoGetMethod.</param>
        internal PSParameterizedProperty(string name, Adapter adapter, object baseObject, object adapterData)
        internal PSParameterizedProperty(string name)
        public bool IsSettable => adapter.BaseParameterizedPropertyIsSettable(this);
        public bool IsGettable => adapter.BaseParameterizedPropertyIsGettable(this);
        /// Invokes the getter method and returns its result.
            return this.adapter.BaseParameterizedPropertyGet(this, arguments);
        /// Invokes the setter method.
        /// <param name="valueToSet">Value to set this property with.</param>
        public void InvokeSet(object valueToSet, params object[] arguments)
            this.adapter.BaseParameterizedPropertySet(this, valueToSet, arguments);
        /// Returns a collection of the definitions for this property.
        public override Collection<string> OverloadDefinitions => adapter.BaseParameterizedPropertyDefinitions(this);
        public override string TypeNameOfValue => adapter.BaseParameterizedPropertyType(this);
            PSParameterizedProperty property = new PSParameterizedProperty(this.name, this.adapter, this.baseObject, this.adapterData);
        public override PSMemberTypes MemberType => PSMemberTypes.ParameterizedProperty;
    /// Serves as a set of members.
    public class PSMemberSet : PSMemberInfo
            foreach (var member in Members)
                member.ReplicateInstance(particularInstance);
            foreach (PSMemberInfo member in this.Members)
                returnValue.Append(member.Name);
            returnValue.Insert(0, this.Name);
        private readonly PSMemberInfoIntegratingCollection<PSMemberInfo> _members;
        private readonly PSMemberInfoIntegratingCollection<PSPropertyInfo> _properties;
        private readonly PSMemberInfoIntegratingCollection<PSMethodInfo> _methods;
        internal PSMemberInfoInternalCollection<PSMemberInfo> internalMembers;
        private readonly PSObject _constructorPSObject;
        private static readonly Collection<CollectionEntry<PSMemberInfo>> s_emptyMemberCollection = new Collection<CollectionEntry<PSMemberInfo>>();
        private static readonly Collection<CollectionEntry<PSMethodInfo>> s_emptyMethodCollection = new Collection<CollectionEntry<PSMethodInfo>>();
        private static readonly Collection<CollectionEntry<PSPropertyInfo>> s_emptyPropertyCollection = new Collection<CollectionEntry<PSPropertyInfo>>();
        /// Initializes a new instance of PSMemberSet with no initial members.
        /// <param name="name">Name for the member set.</param>
        public PSMemberSet(string name)
            this.internalMembers = new PSMemberInfoInternalCollection<PSMemberInfo>();
            _members = new PSMemberInfoIntegratingCollection<PSMemberInfo>(this, s_emptyMemberCollection);
            _properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(this, s_emptyPropertyCollection);
            _methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>(this, s_emptyMethodCollection);
        /// Initializes a new instance of PSMemberSet with all the initial members in <paramref name="members"/>
        /// <param name="members">Members in the member set.</param>
        public PSMemberSet(string name, IEnumerable<PSMemberInfo> members)
            if (members == null)
                throw PSTraceSource.NewArgumentNullException(nameof(members));
                this.internalMembers.Add(member.Copy());
        /// Initializes a new instance of PSMemberSet with all the initial members in <paramref name="members"/>.
        /// This constructor is supposed to be used in TypeTable to reuse the passed-in member collection.
        /// Null-argument check is skipped here, so callers need to check arguments before passing in.
        internal PSMemberSet(string name, PSMemberInfoInternalCollection<PSMemberInfo> members)
            Diagnostics.Assert(!string.IsNullOrEmpty(name), "Caller needs to guarantee not null or empty.");
            Diagnostics.Assert(members != null, "Caller needs to guarantee not null.");
            this.internalMembers = members;
        private static readonly Collection<CollectionEntry<PSMemberInfo>> s_typeMemberCollection = GetTypeMemberCollection();
        private static readonly Collection<CollectionEntry<PSMethodInfo>> s_typeMethodCollection = GetTypeMethodCollection();
        private static readonly Collection<CollectionEntry<PSPropertyInfo>> s_typePropertyCollection = GetTypePropertyCollection();
        private static Collection<CollectionEntry<PSMemberInfo>> GetTypeMemberCollection()
            Collection<CollectionEntry<PSMemberInfo>> returnValue = new Collection<CollectionEntry<PSMemberInfo>>();
            returnValue.Add(new CollectionEntry<PSMemberInfo>(
                PSObject.TypeTableGetMembersDelegate<PSMemberInfo>,
                PSObject.TypeTableGetMemberDelegate<PSMemberInfo>,
                PSObject.TypeTableGetFirstMemberOrDefaultDelegate<PSMemberInfo>,
                true, true, "type table members"));
        private static Collection<CollectionEntry<PSMethodInfo>> GetTypeMethodCollection()
            Collection<CollectionEntry<PSMethodInfo>> returnValue = new Collection<CollectionEntry<PSMethodInfo>>();
            returnValue.Add(new CollectionEntry<PSMethodInfo>(
                PSObject.TypeTableGetMembersDelegate<PSMethodInfo>,
                PSObject.TypeTableGetMemberDelegate<PSMethodInfo>,
                PSObject.TypeTableGetFirstMemberOrDefaultDelegate<PSMethodInfo>,
        private static Collection<CollectionEntry<PSPropertyInfo>> GetTypePropertyCollection()
            Collection<CollectionEntry<PSPropertyInfo>> returnValue = new Collection<CollectionEntry<PSPropertyInfo>>();
            returnValue.Add(new CollectionEntry<PSPropertyInfo>(
                PSObject.TypeTableGetMembersDelegate<PSPropertyInfo>,
                PSObject.TypeTableGetMemberDelegate<PSPropertyInfo>,
                PSObject.TypeTableGetFirstMemberOrDefaultDelegate<PSPropertyInfo>,
        /// Used to create the Extended MemberSet.
        /// <param name="name">Name of the memberSet.</param>
        /// <param name="mshObject">Object associated with this memberset.</param>
        internal PSMemberSet(string name, PSObject mshObject)
            if (mshObject == null)
                throw PSTraceSource.NewArgumentNullException(nameof(mshObject));
            _constructorPSObject = mshObject;
            this.internalMembers = mshObject.InstanceMembers;
            _members = new PSMemberInfoIntegratingCollection<PSMemberInfo>(this, s_typeMemberCollection);
            _properties = new PSMemberInfoIntegratingCollection<PSPropertyInfo>(this, s_typePropertyCollection);
            _methods = new PSMemberInfoIntegratingCollection<PSMethodInfo>(this, s_typeMethodCollection);
        internal bool inheritMembers = true;
        /// Gets a flag indicating whether the memberset will inherit members of the memberset
        /// of the same name in the "parent" class.
        public bool InheritMembers => this.inheritMembers;
        /// Gets the internal member collection.
        internal virtual PSMemberInfoInternalCollection<PSMemberInfo> InternalMembers => this.internalMembers;
        /// Gets the member collection.
        public PSMemberInfoCollection<PSMemberInfo> Members => _members;
        /// Gets the Property collection, or the members that are actually properties.
        public PSMemberInfoCollection<PSPropertyInfo> Properties => _properties;
        /// Gets the Method collection, or the members that are actually methods.
        public PSMemberInfoCollection<PSMethodInfo> Methods => _methods;
            if (_constructorPSObject == null)
                PSMemberSet memberSet = new PSMemberSet(name);
                    memberSet.Members.Add(member);
                CloneBaseProperties(memberSet);
                return memberSet;
                return new PSMemberSet(name, _constructorPSObject);
        /// Gets the member type. For PSMemberSet the member type is PSMemberTypes.MemberSet.
        public override PSMemberTypes MemberType => PSMemberTypes.MemberSet;
        /// Gets the value of this member. The getter returns the PSMemberSet itself.
        /// <exception cref="ExtendedTypeSystemException">When trying to set the property.</exception>
            set => throw new ExtendedTypeSystemException("CannotChangePSMemberSetValue", null,
                ExtendedTypeSystem.CannotSetValueForMemberType, this.GetType().FullName);
        /// Gets the type of the value for this member. This returns typeof(PSMemberSet).FullName.
        public override string TypeNameOfValue => typeof(PSMemberSet).FullName;
    /// This MemberSet is used internally to represent the memberset for properties
    /// PSObject, PSBase, PSAdapted members of a PSObject. Having a specialized
    /// memberset enables delay loading the members for these members. This saves
    /// time loading the members of a PSObject.
    /// This is added to improve hosting PowerShell's PSObjects in a ASP.Net GridView
    /// Control
    internal class PSInternalMemberSet : PSMemberSet
        private readonly PSObject _psObject;
        /// Constructs the specialized member set.
        /// Should be one of PSObject, PSBase, PSAdapted
        /// <param name="psObject">
        /// original PSObject to use to generate members
        internal PSInternalMemberSet(string propertyName, PSObject psObject)
            this.internalMembers = null;
            _psObject = psObject;
        #region virtual overrides
        /// Generates the members when needed.
        internal override PSMemberInfoInternalCollection<PSMemberInfo> InternalMembers
                // do not cache "psadapted"
                if (name.Equals(PSObject.AdaptedMemberSetName, StringComparison.OrdinalIgnoreCase))
                    return GetInternalMembersFromAdapted();
                // cache "psbase" and "psobject"
                if (internalMembers == null)
                            internalMembers = new PSMemberInfoInternalCollection<PSMemberInfo>();
                            switch (name.ToLowerInvariant())
                                case PSObject.BaseObjectMemberSetName:
                                    GenerateInternalMembersFromBase();
                                case PSObject.PSObjectMemberSetName:
                                    GenerateInternalMembersFromPSObject();
                                        string.Create(CultureInfo.InvariantCulture, $"PSInternalMemberSet cannot process {name}"));
                return internalMembers;
        private void GenerateInternalMembersFromBase()
            if (_psObject.IsDeserialized)
                if (_psObject.ClrMembers != null)
                    foreach (PSMemberInfo member in _psObject.ClrMembers)
                        internalMembers.Add(member.Copy());
                foreach (PSMemberInfo member in
                    PSObject.DotNetInstanceAdapter.BaseGetMembers<PSMemberInfo>(_psObject.ImmediateBaseObject))
        private PSMemberInfoInternalCollection<PSMemberInfo> GetInternalMembersFromAdapted()
            PSMemberInfoInternalCollection<PSMemberInfo> retVal = new PSMemberInfoInternalCollection<PSMemberInfo>();
                if (_psObject.AdaptedMembers != null)
                    foreach (PSMemberInfo member in _psObject.AdaptedMembers)
                        retVal.Add(member.Copy());
                foreach (PSMemberInfo member in _psObject.InternalAdapter.BaseGetMembers<PSMemberInfo>(
                    _psObject.ImmediateBaseObject))
        private void GenerateInternalMembersFromPSObject()
            PSMemberInfoCollection<PSMemberInfo> members = PSObject.DotNetInstanceAdapter.BaseGetMembers<PSMemberInfo>(
                _psObject);
    /// Serves as a list of property names.
    /// It is permitted to subclass <see cref="PSPropertySet"/>
    public class PSPropertySet : PSMemberInfo
            if (ReferencedPropertyNames.Count != 0)
                foreach (string property in ReferencedPropertyNames)
                    returnValue.Append(property);
        /// Initializes a new instance of PSPropertySet with a name and list of property names.
        /// <param name="name">Name of the set.</param>
        /// <param name="referencedPropertyNames">Name of the properties in the set.</param>
        public PSPropertySet(string name, IEnumerable<string> referencedPropertyNames)
            if (referencedPropertyNames == null)
                throw PSTraceSource.NewArgumentNullException(nameof(referencedPropertyNames));
            ReferencedPropertyNames = new Collection<string>();
            foreach (string referencedPropertyName in referencedPropertyNames)
                if (string.IsNullOrEmpty(referencedPropertyName))
                    throw PSTraceSource.NewArgumentException(nameof(referencedPropertyNames));
                ReferencedPropertyNames.Add(referencedPropertyName);
        /// This constructor is supposed to be used in TypeTable to reuse the passed-in property name list.
        /// <param name="referencedPropertyNameList">Name of the properties in the set.</param>
        internal PSPropertySet(string name, List<string> referencedPropertyNameList)
            Diagnostics.Assert(referencedPropertyNameList != null, "Caller needs to guarantee not null.");
            // We use the constructor 'public Collection(IList<T> list)' to create the collection,
            // so that the passed-in list is directly used as the backing store of the collection.
            ReferencedPropertyNames = new Collection<string>(referencedPropertyNameList);
        /// Gets the property names in this property set.
        public Collection<string> ReferencedPropertyNames { get; }
            PSPropertySet member = new PSPropertySet(name, ReferencedPropertyNames);
        public override PSMemberTypes MemberType => PSMemberTypes.PropertySet;
        /// Gets the PSPropertySet itself.
            set => throw new ExtendedTypeSystemException("CannotChangePSPropertySetValue", null,
        /// Gets the type of the value for this member. This returns typeof(PSPropertySet).FullName.
        public override string TypeNameOfValue => typeof(PSPropertySet).FullName;
    /// Used to access the adapted or base events from the BaseObject.
    public class PSEvent : PSMemberInfo
            StringBuilder eventDefinition = new StringBuilder();
            eventDefinition.Append(this.baseEvent.ToString());
            eventDefinition.Append('(');
            foreach (ParameterInfo parameter in baseEvent.EventHandlerType.GetMethod("Invoke").GetParameters())
                if (loopCounter > 0)
                    eventDefinition.Append(", ");
                eventDefinition.Append(parameter.ParameterType.ToString());
            eventDefinition.Append(')');
            return eventDefinition.ToString();
        internal EventInfo baseEvent;
        /// Constructs this event.
        /// <param name="baseEvent">The actual event.</param>
        internal PSEvent(EventInfo baseEvent)
            this.baseEvent = baseEvent;
            this.name = baseEvent.Name;
            PSEvent member = new PSEvent(this.baseEvent);
        public override PSMemberTypes MemberType => PSMemberTypes.Event;
        /// Gets the value of this member. The getter returns the
        /// actual .NET event that this type wraps.
            get => baseEvent;
            set => throw new ExtendedTypeSystemException("CannotChangePSEventInfoValue", null,
        public override string TypeNameOfValue => typeof(PSEvent).FullName;
    /// A dynamic member.
    public class PSDynamicMember : PSMemberInfo
        internal PSDynamicMember(string name)
            return "dynamic " + Name;
        public override PSMemberTypes MemberType => PSMemberTypes.Dynamic;
            get => throw PSTraceSource.NewInvalidOperationException();
            set => throw PSTraceSource.NewInvalidOperationException();
        public override string TypeNameOfValue => "dynamic";
            return new PSDynamicMember(Name);
    #endregion PSMemberInfo
    #region Member collection classes and its auxiliary classes
    /// /// This class is used in PSMemberInfoInternalCollection and ReadOnlyPSMemberInfoCollection.
    internal static class MemberMatch
        internal static WildcardPattern GetNamePattern(string name)
            if (name != null && WildcardPattern.ContainsWildcardCharacters(name))
                return WildcardPattern.Get(name, WildcardOptions.IgnoreCase);
        /// Returns all members in memberList matching name and memberTypes.
        /// <param name="memberList">Members to look for member with the correct types and name.</param>
        /// <param name="name">Name of the members to look for. The name might contain globbing characters.</param>
        /// <param name="nameMatch">WildcardPattern out of name.</param>
        /// <param name="memberTypes">Type of members we want to retrieve.</param>
        /// <returns>A collection of members of the right types and name extracted from memberList.</returns>
        internal static PSMemberInfoInternalCollection<T> Match<T>(PSMemberInfoInternalCollection<T> memberList, string name, WildcardPattern nameMatch, PSMemberTypes memberTypes)
            where T : PSMemberInfo
            if (memberList == null)
                throw PSTraceSource.NewArgumentNullException(nameof(memberList));
            if (nameMatch == null)
                T member = memberList[name];
                if (member != null && (member.MemberType & memberTypes) != 0)
                    returnValue.Add(member);
            foreach (T member in memberList)
                if (nameMatch.IsMatch(member.Name) && ((member.MemberType & memberTypes) != 0))
    /// A Predicate that determine if a member name matches a criterion.
    /// <param name="memberName"></param>
    /// <returns><see langword="true"/> if the <paramref name="memberName"/> matches the predicate, otherwise <see langword="false"/>.</returns>
    public delegate bool MemberNamePredicate(string memberName);
    /// Serves as the collection of members in an PSObject or MemberSet.
    public abstract class PSMemberInfoCollection<T> : IEnumerable<T> where T : PSMemberInfo
        /// Initializes a new instance of an PSMemberInfoCollection derived class.
        protected PSMemberInfoCollection()
        #region abstract
        /// Adds a member to this collection.
        /// <param name="member">Member to be added.</param>
        ///     When:
        ///         adding a member to an PSMemberSet from the type configuration file or
        ///         adding a member with a reserved member name or
        ///         trying to add a member with a type not compatible with this collection or
        ///         a member by this name is already present
        public abstract void Add(T member);
        /// <param name="preValidated">flag to indicate that validation has already been done
        ///     on this new member.  Use only when you can guarantee that the input will not
        ///     cause any of the errors normally caught by this method.</param>
        public abstract void Add(T member, bool preValidated);
        /// Removes a member from this collection.
        /// <param name="name">Name of the member to be removed.</param>
        ///         removing a member from an PSMemberSet from the type configuration file
        ///         removing a member with a reserved member name or
        ///         trying to remove a member with a type not compatible with this collection
        public abstract void Remove(string name);
        /// Gets the member in this collection matching name. If the member does not exist, null is returned.
        /// <param name="name">Name of the member to look for.</param>
        /// <returns>The member matching name.</returns>
        public abstract T this[string name] { get; }
        #endregion abstract
        #region Match
        /// Returns all members in the collection matching name.
        /// <param name="name">Name of the members to be return. May contain wildcard characters.</param>
        /// <returns>All members in the collection matching name.</returns>
        public abstract ReadOnlyPSMemberInfoCollection<T> Match(string name);
        /// Returns all members in the collection matching name and types.
        /// <param name="memberTypes">Type of the members to be searched.</param>
        /// <returns>All members in the collection matching name and types.</returns>
        public abstract ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes);
        /// <param name="matchOptions">Match options.</param>
        internal abstract ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes, MshMemberMatchOptions matchOptions);
        #endregion Match
        internal static bool IsReservedName(string name)
            return (string.Equals(name, PSObject.BaseObjectMemberSetName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, PSObject.AdaptedMemberSetName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, PSObject.ExtendedMemberSetName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, PSObject.PSObjectMemberSetName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, PSObject.PSTypeNames, StringComparison.OrdinalIgnoreCase));
        #region IEnumerable
        /// Gets the general enumerator for this collection.
        /// <returns>The enumerator for this collection.</returns>
            return GetEnumerator();
        /// Gets the specific enumerator for this collection.
        public abstract IEnumerator<T> GetEnumerator();
        #endregion IEnumerable
        internal abstract T FirstOrDefault(MemberNamePredicate predicate);
    /// Serves as a read only collection of members.
    /// It is permitted to subclass <see cref="ReadOnlyPSMemberInfoCollection&lt;T&gt;"/>
    public class ReadOnlyPSMemberInfoCollection<T> : IEnumerable<T> where T : PSMemberInfo
        private readonly PSMemberInfoInternalCollection<T> _members;
        /// Initializes a new instance of ReadOnlyPSMemberInfoCollection with the given members.
        /// <param name="members"></param>
        internal ReadOnlyPSMemberInfoCollection(PSMemberInfoInternalCollection<T> members)
            _members = members;
        /// Return the member in this collection matching name. If the member does not exist, null is returned.
        public T this[string name]
                return _members[name];
        public ReadOnlyPSMemberInfoCollection<T> Match(string name)
            return _members.Match(name);
        public ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes)
            return _members.Match(name, memberTypes);
        public virtual IEnumerator<T> GetEnumerator()
            return _members.GetEnumerator();
        /// Gets the number of elements in this collection.
        public int Count => _members.Count;
        /// Returns the 0 based member identified by index.
        /// <param name="index">Index of the member to retrieve.</param>
        public T this[int index] => _members[index];
    /// Collection of members.
    internal class PSMemberInfoInternalCollection<T> : PSMemberInfoCollection<T>, IEnumerable<T> where T : PSMemberInfo
        private OrderedDictionary _members;
        private int _countHidden;
        /// Gets the OrderedDictionary for holding all members.
        /// We use this property to delay initializing _members until we absolutely need to.
        private OrderedDictionary Members
                if (_members == null)
                    System.Threading.Interlocked.CompareExchange(ref _members, new OrderedDictionary(StringComparer.OrdinalIgnoreCase), null);
                return _members;
        /// Constructs this collection.
        internal PSMemberInfoInternalCollection()
        /// Constructs this collection with an initial capacity.
        internal PSMemberInfoInternalCollection(int capacity)
            _members = new OrderedDictionary(capacity, StringComparer.OrdinalIgnoreCase);
        private void Replace(T oldMember, T newMember)
            Members[newMember.Name] = newMember;
            if (oldMember.IsHidden)
                _countHidden--;
            if (newMember.IsHidden)
                _countHidden++;
        /// Adds a member to the collection by replacing the one with the same name.
        /// <param name="newMember"></param>
        internal void Replace(T newMember)
            Diagnostics.Assert(newMember != null, "called from internal code that checks for new member not null");
            // Save to a local variable to reduce property access.
            var members = Members;
            lock (members)
                var oldMember = members[newMember.Name] as T;
                Diagnostics.Assert(oldMember != null, "internal code checks member already exists");
                Replace(oldMember, newMember);
        /// <exception cref="ExtendedTypeSystemException">When a member by this name is already present.</exception>
        public override void Add(T member)
            Add(member, false);
        public override void Add(T member, bool preValidated)
                if (members[member.Name] is T existingMember)
                    Replace(existingMember, member);
                    members[member.Name] = member;
                    if (member.IsHidden)
        /// <exception cref="ExtendedTypeSystemException">When removing a member with a reserved member name.</exception>
        public override void Remove(string name)
            if (IsReservedName(name))
                throw new ExtendedTypeSystemException("PSMemberInfoInternalCollectionRemoveReservedName",
                    ExtendedTypeSystem.ReservedMemberName,
            lock (_members)
                if (_members[name] is PSMemberInfo member)
                    _members.Remove(name);
        /// Returns the member in this collection matching name.
        public override T this[string name]
                    return _members[name] as T;
        public override ReadOnlyPSMemberInfoCollection<T> Match(string name)
            return Match(name, PSMemberTypes.All, MshMemberMatchOptions.None);
        public override ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes)
            return Match(name, memberTypes, MshMemberMatchOptions.None);
        internal override ReadOnlyPSMemberInfoCollection<T> Match(string name, PSMemberTypes memberTypes, MshMemberMatchOptions matchOptions)
            PSMemberInfoInternalCollection<T> internalMembers = GetInternalMembers(matchOptions);
            return new ReadOnlyPSMemberInfoCollection<T>(MemberMatch.Match(internalMembers, name, MemberMatch.GetNamePattern(name), memberTypes));
        private PSMemberInfoInternalCollection<T> GetInternalMembers(MshMemberMatchOptions matchOptions)
                foreach (T member in _members.Values.OfType<T>())
                    if (member.MatchesOptions(matchOptions))
        /// The number of elements in this collection.
        internal int Count
                    return _members.Count;
        /// The number of elements in this collection not marked as Hidden.
        internal int VisibleCount
                    return _members.Count - _countHidden;
        internal T this[int index]
                    return _members[index] as T;
        /// This virtual works around the difficulty of implementing
        /// interfaces virtually.
        public override IEnumerator<T> GetEnumerator()
                return Enumerable.Empty<T>().GetEnumerator();
                // Copy the members to a list so that iteration can be performed without holding a lock.
                return _members.Values.OfType<T>().ToList().GetEnumerator();
        /// Returns the first member that matches the specified <see cref="MemberNamePredicate"/>.
        internal override T FirstOrDefault(MemberNamePredicate predicate)
                foreach (DictionaryEntry entry in _members)
                    if (predicate((string)entry.Key))
                        return entry.Value as T;
    #region CollectionEntry
    internal class CollectionEntry<T> where T : PSMemberInfo
        internal delegate PSMemberInfoInternalCollection<T> GetMembersDelegate(PSObject obj);
        internal delegate T GetMemberDelegate(PSObject obj, string name);
        internal delegate T GetFirstOrDefaultDelegate(PSObject obj, MemberNamePredicate predicate);
        internal CollectionEntry(
            GetMembersDelegate getMembers,
            GetMemberDelegate getMember,
            GetFirstOrDefaultDelegate getFirstOrDefault,
            bool shouldReplicateWhenReturning,
            bool shouldCloneWhenReturning,
            string collectionNameForTracing)
            GetMembers = getMembers;
            GetMember = getMember;
            GetFirstOrDefault = getFirstOrDefault;
            _shouldReplicateWhenReturning = shouldReplicateWhenReturning;
            _shouldCloneWhenReturning = shouldCloneWhenReturning;
            CollectionNameForTracing = collectionNameForTracing;
        internal GetMembersDelegate GetMembers { get; }
        internal GetMemberDelegate GetMember { get; }
        internal GetFirstOrDefaultDelegate GetFirstOrDefault { get; }
        internal string CollectionNameForTracing { get; }
        private readonly bool _shouldReplicateWhenReturning;
        private readonly bool _shouldCloneWhenReturning;
        internal T CloneOrReplicateObject(object owner, T member)
            if (_shouldCloneWhenReturning)
                member = (T)member.Copy();
            if (_shouldReplicateWhenReturning)
                member.ReplicateInstance(owner);
    #endregion CollectionEntry
    internal static class ReservedNameMembers
        private static object GenerateMemberSet(string name, object obj)
            PSObject mshOwner = PSObject.AsPSObject(obj);
            var memberSet = mshOwner.InstanceMembers[name];
            if (memberSet == null)
                memberSet = new PSInternalMemberSet(name, mshOwner)
                    ShouldSerialize = false,
                    IsHidden = true,
                    IsReservedMember = true
                mshOwner.InstanceMembers.Add(memberSet);
                memberSet.instance = mshOwner;
        internal static object GeneratePSBaseMemberSet(object obj)
            return GenerateMemberSet(PSObject.BaseObjectMemberSetName, obj);
        internal static object GeneratePSAdaptedMemberSet(object obj)
            return GenerateMemberSet(PSObject.AdaptedMemberSetName, obj);
        internal static object GeneratePSObjectMemberSet(object obj)
            return GenerateMemberSet(PSObject.PSObjectMemberSetName, obj);
        internal static object GeneratePSExtendedMemberSet(object obj)
            var memberSet = mshOwner.InstanceMembers[PSObject.ExtendedMemberSetName];
                memberSet = new PSMemberSet(PSObject.ExtendedMemberSetName, mshOwner)
                memberSet.ReplicateInstance(mshOwner);
        // This is the implementation of the PSTypeNames CodeProperty.
        public static Collection<string> PSTypeNames(PSObject o)
            return o.TypeNames;
        internal static void GeneratePSTypeNames(object obj)
            if (mshOwner.InstanceMembers[PSObject.PSTypeNames] != null)
                // PSTypeNames member set is already generated..just return.
            PSCodeProperty codeProperty = new PSCodeProperty(PSObject.PSTypeNames, CachedReflectionInfo.ReservedNameMembers_PSTypeNames)
                instance = mshOwner,
            mshOwner.InstanceMembers.Add(codeProperty);
    internal class PSMemberInfoIntegratingCollection<T> : PSMemberInfoCollection<T>, IEnumerable<T> where T : PSMemberInfo
        #region reserved names
        private void GenerateAllReservedMembers()
            if (!_mshOwner.HasGeneratedReservedMembers)
                _mshOwner.HasGeneratedReservedMembers = true;
                ReservedNameMembers.GeneratePSExtendedMemberSet(_mshOwner);
                ReservedNameMembers.GeneratePSBaseMemberSet(_mshOwner);
                ReservedNameMembers.GeneratePSObjectMemberSet(_mshOwner);
                ReservedNameMembers.GeneratePSAdaptedMemberSet(_mshOwner);
                ReservedNameMembers.GeneratePSTypeNames(_mshOwner);
        #endregion reserved names
        #region Constructor, fields and properties
        internal Collection<CollectionEntry<T>> Collections { get; }
        private readonly PSObject _mshOwner;
        private readonly PSMemberSet _memberSetOwner;
        internal PSMemberInfoIntegratingCollection(object owner, Collection<CollectionEntry<T>> collections)
            if (owner == null)
                throw PSTraceSource.NewArgumentNullException(nameof(owner));
            _mshOwner = owner as PSObject;
            _memberSetOwner = owner as PSMemberSet;
            if (_mshOwner == null && _memberSetOwner == null)
                throw PSTraceSource.NewArgumentException(nameof(owner));
            if (collections == null)
                throw PSTraceSource.NewArgumentNullException(nameof(collections));
            Collections = collections;
        #endregion Constructor, fields and properties
        /// Adds member to the collection.
        ///         member is an PSProperty or PSMethod
        ///         adding a member to a MemberSet with a reserved name
        ///         adding a member with a type not compatible with this collection
        ///         a member with this name already exists
        ///         trying to add a member to a static memberset
            if (!preValidated)
                if (member.MemberType == PSMemberTypes.Property || member.MemberType == PSMemberTypes.Method)
                        "CannotAddMethodOrProperty",
                        ExtendedTypeSystem.CannotAddPropertyOrMethod);
                if (_memberSetOwner != null && _memberSetOwner.IsReservedMember)
                    throw new ExtendedTypeSystemException("CannotAddToReservedNameMemberset",
                        ExtendedTypeSystem.CannotChangeReservedMember,
                        _memberSetOwner.Name);
            AddToReservedMemberSet(member, preValidated);
        /// Auxiliary to add members from types.xml.
        /// <param name="preValidated"></param>
        internal void AddToReservedMemberSet(T member, bool preValidated)
                if (_memberSetOwner != null && !_memberSetOwner.IsInstance)
                    throw new ExtendedTypeSystemException("RemoveMemberFromStaticMemberSet",
                        member.Name);
            AddToTypesXmlCache(member, preValidated);
        ///    on this new member.  Use only when you can guarantee that the input will not
        ///    cause any of the errors normally caught by this method.</param>
        internal void AddToTypesXmlCache(T member, bool preValidated)
                if (IsReservedName(member.Name))
                    throw new ExtendedTypeSystemException("PSObjectMembersMembersAddReservedName",
            PSMemberInfo memberToBeAdded = member.Copy();
            if (_mshOwner != null)
                    TypeTable typeTable = _mshOwner.GetTypeTable();
                        var typesXmlMembers = typeTable.GetMembers(_mshOwner.InternalTypeNames);
                        var typesXmlMember = typesXmlMembers[member.Name];
                        if (typesXmlMember is T)
                                "AlreadyPresentInTypesXml",
                                ExtendedTypeSystem.MemberAlreadyPresentFromTypesXml,
                memberToBeAdded.ReplicateInstance(_mshOwner);
                _mshOwner.InstanceMembers.Add(memberToBeAdded, preValidated);
                // All member binders may need to invalidate dynamic sites, and now must generate
                // different binding restrictions (specifically, must check for an instance member
                // before considering the type table or adapted members.)
                PSGetMemberBinder.SetHasInstanceMember(memberToBeAdded.Name);
                PSVariableAssignmentBinder.NoteTypeHasInstanceMemberOrTypeName(PSObject.Base(_mshOwner).GetType());
            _memberSetOwner.InternalMembers.Add(memberToBeAdded, preValidated);
        /// Removes the member named name from the collection.
        /// When trying to remove a member with a type not compatible with this collection
        /// When trying to remove a member from a static memberset
        /// When trying to remove a member from a MemberSet with a reserved name
                _mshOwner.InstanceMembers.Remove(name);
            if (!_memberSetOwner.IsInstance)
                throw new ExtendedTypeSystemException("AddMemberToStaticMemberSet",
            if (IsReservedName(_memberSetOwner.Name))
                throw new ExtendedTypeSystemException("CannotRemoveFromReservedNameMemberset",
            _memberSetOwner.InternalMembers.Remove(name);
        /// Method which checks if the <paramref name="name"/> is reserved and if so
        /// it will ensure that the particular reserved member is loaded into the
        /// objects member collection.
        /// Caller should ensure that name is not null or empty.
        /// Name of the member to check and load (if needed).
        private void EnsureReservedMemberIsLoaded(string name)
            Diagnostics.Assert(!string.IsNullOrEmpty(name),
                "Name cannot be null or empty");
            // Length >= psbase (shortest special member)
            if (name.Length >= 6 && (name[0] == 'p' || name[0] == 'P') && (name[1] == 's' || name[1] == 'S'))
                    case PSObject.AdaptedMemberSetName:
                    case PSObject.ExtendedMemberSetName:
                    case PSObject.PSTypeNames:
        /// Returns the name corresponding to name or null if it is not present.
        /// <param name="name">Name of the member to return.</param>
                using (PSObject.MemberResolution.TraceScope("Lookup"))
                    PSMemberInfo member;
                    object delegateOwner;
                        // this will check if name is a reserved name like PSBase, PSTypeNames
                        // if it is a reserved name, ensures the value is loaded.
                        EnsureReservedMemberIsLoaded(name);
                        delegateOwner = _mshOwner;
                        PSMemberInfoInternalCollection<PSMemberInfo> instanceMembers;
                        if (PSObject.HasInstanceMembers(_mshOwner, out instanceMembers))
                            member = instanceMembers[name];
                            if (member is T memberAsT)
                                PSObject.MemberResolution.WriteLine("Found PSObject instance member: {0}.", name);
                                return memberAsT;
                        member = _memberSetOwner.InternalMembers[name];
                        delegateOwner = _memberSetOwner.instance;
                            // In membersets we cannot replicate the instance when adding
                            // since the memberset might not yet have an associated PSObject.
                            // We replicate the instance when returning the members of the memberset.
                            PSObject.MemberResolution.WriteLine("Found PSMemberSet member: {0}.", name);
                            member.ReplicateInstance(delegateOwner);
                    if (delegateOwner == null)
                    delegateOwner = PSObject.AsPSObject(delegateOwner);
                    foreach (CollectionEntry<T> collection in Collections)
                        Diagnostics.Assert(delegateOwner != null, "all integrating collections with non empty collections have an associated PSObject");
                        T memberAsT = collection.GetMember((PSObject)delegateOwner, name);
                            return collection.CloneOrReplicateObject(delegateOwner, memberAsT);
        private PSMemberInfoInternalCollection<T> GetIntegratedMembers(MshMemberMatchOptions matchOptions)
            using (PSObject.MemberResolution.TraceScope("Generating the total list of members"))
                    foreach (PSMemberInfo member in _mshOwner.InstanceMembers)
                    foreach (PSMemberInfo member in _memberSetOwner.InternalMembers)
                    PSMemberInfoInternalCollection<T> members = collection.GetMembers((PSObject)delegateOwner);
                    foreach (T member in members)
                        PSMemberInfo previousMember = returnValue[member.Name];
                            PSObject.MemberResolution.WriteLine("Member \"{0}\" of type \"{1}\" has been ignored because a member with the same name and type \"{2}\" is already present.",
                                member.Name, member.MemberType, previousMember.MemberType);
                        if (!member.MatchesOptions(matchOptions))
                            PSObject.MemberResolution.WriteLine("Skipping hidden member \"{0}\".", member.Name);
                        T memberToAdd = collection.CloneOrReplicateObject(delegateOwner, member);
                        returnValue.Add(memberToAdd);
        /// <param name="matchOptions">Search options.</param>
            using (PSObject.MemberResolution.TraceScope("Matching \"{0}\"", name))
                    GenerateAllReservedMembers();
                WildcardPattern nameMatch = MemberMatch.GetNamePattern(name);
                PSMemberInfoInternalCollection<T> allMembers = GetIntegratedMembers(matchOptions);
                ReadOnlyPSMemberInfoCollection<T> returnValue = new ReadOnlyPSMemberInfoCollection<T>(MemberMatch.Match(allMembers, name, nameMatch, memberTypes));
                PSObject.MemberResolution.WriteLine("{0} total matches.", returnValue.Count);
            return new Enumerator(this);
                    if (member is T memberAsT && predicate(memberAsT.Name))
                        memberAsT.ReplicateInstance(delegateOwner);
            var ownerAsPSObj = PSObject.AsPSObject(delegateOwner);
            for (int i = 0; i < Collections.Count; i++)
                var collectionEntry = Collections[i];
                var member = collectionEntry.GetFirstOrDefault(ownerAsPSObj, predicate);
                    return collectionEntry.CloneOrReplicateObject(ownerAsPSObj, member);
        /// Enumerable for this class.
        internal struct Enumerator : IEnumerator<T>
            private T _current;
            private readonly PSMemberInfoInternalCollection<T> _allMembers;
            /// Initializes a new instance of the <see cref="Enumerator"/> class to enumerate over members.
            /// <param name="integratingCollection">Members we are enumerating.</param>
            internal Enumerator(PSMemberInfoIntegratingCollection<T> integratingCollection)
                using (PSObject.MemberResolution.TraceScope("Enumeration Start"))
                    _current = null;
                    _allMembers = integratingCollection.GetIntegratedMembers(MshMemberMatchOptions.None);
                    if (integratingCollection._mshOwner != null)
                        integratingCollection.GenerateAllReservedMembers();
                        PSObject.MemberResolution.WriteLine("Enumerating PSObject with type \"{0}\".", integratingCollection._mshOwner.ImmediateBaseObject.GetType().FullName);
                        PSObject.MemberResolution.WriteLine("PSObject instance members: {0}", _allMembers.VisibleCount);
                        PSObject.MemberResolution.WriteLine("Enumerating PSMemberSet \"{0}\".", integratingCollection._memberSetOwner.Name);
                        PSObject.MemberResolution.WriteLine("MemberSet instance members: {0}", _allMembers.VisibleCount);
            /// Moves to the next element in the enumeration.
            /// If there are no more elements to enumerate, returns false.
            /// Returns true otherwise.
                T member = null;
                while (_currentIndex < _allMembers.Count)
                    member = _allMembers[_currentIndex];
                    if (!member.IsHidden)
                if (_currentIndex < _allMembers.Count)
                    _current = member;
            /// Gets the current PSMemberInfo in the enumeration.
            T IEnumerator<T>.Current
                    if (_currentIndex == -1)
                    return _current;
            object IEnumerator.Current => ((IEnumerator<T>)this).Current;
            void IEnumerator.Reset()
            /// Not supported.
    #endregion Member collection classes and its auxiliary classes
#pragma warning restore 56503
