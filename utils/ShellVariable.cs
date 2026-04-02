    /// Represents a variable in the PowerShell language.
    public class PSVariable : IHasSessionStateEntryVisibility
        /// Constructs a variable with the given name.
        /// The name of the variable.
        public PSVariable(string name)
            : this(name, null, ScopedItemOptions.None, (Collection<Attribute>)null)
        /// Constructs a variable with the given name, and value.
        /// The value of the variable.
        public PSVariable(string name, object value)
            : this(name, value, ScopedItemOptions.None, (Collection<Attribute>)null)
        /// Constructs a variable with the given name, value, and options.
        /// The constraints of the variable. Note, variables can only be made constant
        /// in the constructor.
        public PSVariable(string name, object value, ScopedItemOptions options)
            : this(name, value, options, (Collection<Attribute>)null)
        /// Constructs a variable with the given name, value, options, and description.
        /// The description for the variable.
        internal PSVariable(string name, object value, ScopedItemOptions options, string description)
            _description = description;
        /// The attributes for the variable. ValidateArgumentsAttribute and derived types
        /// will be used to validate a value before setting it.
        internal PSVariable(
            Collection<Attribute> attributes,
            string description)
                : this(name, value, options, attributes)
        /// Constructs a variable with the given name, value, options, and attributes.
        /// If the validation metadata identified in <paramref name="attributes"/>
        public PSVariable(
            Collection<Attribute> attributes)
            _attributes = new PSVariableAttributeCollection(this);
            // Note, it is OK to set the value before setting the attributes
            // because each attribute will be validated as it is set.
            SetValueRawImpl(value, true);
            // Set the options after setting the initial value.
            if (IsAllScope)
                Language.VariableAnalysis.NoteAllScopeVariable(name);
        // Should be protected, but that makes it public which we don't want.
        // The dummy parameter is to make the signature distinct from the public constructor taking a string.
        // This constructor exists to avoid calling SetValueRaw, which when overridden, might not work because
        // the derived class isn't fully constructed yet.
        internal PSVariable(string name, bool dummy)
        /// Gets the name of the variable.
        /// Gets or sets the description of the variable.
        public virtual string Description
                return _description;
        private string _description = string.Empty;
        internal void DebuggerCheckVariableRead()
            var context = SessionState != null
                              ? SessionState.ExecutionContext
                              : LocalPipeline.GetExecutionContextFromTLS();
            if (context != null && context._debuggingMode > 0)
                context.Debugger.CheckVariableRead(Name);
        internal void DebuggerCheckVariableWrite()
                context.Debugger.CheckVariableWrite(Name);
        /// Gets the value without triggering debugger check.
        internal virtual object GetValueRaw()
        /// If the variable is read-only or constant upon call to set.
        /// <paramref name="value"/> is not valid according to one or more
        /// of the attributes of this shell variable.
        public virtual object Value
                SetValue(value);
        /// If true, then this variable is visible outside the runspace.
        public SessionStateEntryVisibility Visibility { get; set; } = SessionStateEntryVisibility.Public;
        /// The module where this variable was defined.
        /// The name of the module that defined this variable.
        /// Gets or sets the scope options on the variable.
        /// Upon set, if the variable is constant or if <paramref name="value"/>
        /// contains the constant flag.
        public virtual ScopedItemOptions Options
            // Check to see if the variable is constant or readonly, if so
            if (IsConstant || (!force && IsReadOnly))
                            "VariableCannotBeMadeConstant",
                            SessionStateStrings.VariableCannotBeMadeConstant);
            // Now check to see if the caller is trying to
            // remove the AllScope option. This is not allowed
            // at any time.
            if (IsAllScope && ((newOptions & ScopedItemOptions.AllScope) == 0))
                // user is trying to remove the AllScope option from the variable.
                            "VariableAllScopeOptionCannotBeRemoved",
                            SessionStateStrings.VariableAllScopeOptionCannotBeRemoved);
        /// Gets the collection that contains the attributes for the variable.
        /// To add or remove attributes, get the collection and then add or remove
        /// attributes to that collection.
            get { return _attributes ??= new PSVariableAttributeCollection(this); }
        private PSVariableAttributeCollection _attributes;
        /// Checks if the given value meets the validation attribute constraints on the PSVariable.
        /// value which needs to be checked
        /// If <paramref name="value"/> is null or if no attributes are set, then
        /// the value is deemed valid.
        /// If the validation metadata throws an exception.
        public virtual bool IsValidValue(object value)
            return IsValidValue(_attributes, value);
        internal static bool IsValidValue(IEnumerable<Attribute> attributes, object value)
                    if (!IsValidValue(value, attribute))
        /// Determines if the value is valid for the specified attribute.
        /// The variable value to validate.
        /// The attribute to use to validate that value.
        /// True if the value is valid with respect to the attribute, or false otherwise.
        internal static bool IsValidValue(object value, Attribute attribute)
            ValidateArgumentsAttribute validationAttribute = attribute as ValidateArgumentsAttribute;
            if (validationAttribute != null)
                    // Get an EngineIntrinsics instance using the context of the thread.
                    ExecutionContext context = Runspaces.LocalPipeline.GetExecutionContextFromTLS();
                    EngineIntrinsics engine = null;
                        engine = context.EngineIntrinsics;
                    validationAttribute.InternalValidate(value, engine);
        /// Runs all ArgumentTransformationAttributes that are specified in the Attributes
        /// collection on the given value in the order that they are in the collection.
        /// The attributes to use to transform the value.
        /// The value to be transformed.
        /// The transformed value.
        /// If the argument transformation fails.
        internal static object TransformValue(IEnumerable<Attribute> attributes, object value)
            Diagnostics.Assert(attributes != null, "caller to verify attributes is not null");
            object result = value;
                ArgumentTransformationAttribute transformationAttribute =
                    attribute as ArgumentTransformationAttribute;
                if (transformationAttribute != null)
                    result = transformationAttribute.TransformInternal(engine, result);
        /// Parameter binding does the checking and conversions as specified by the
        /// attributes, so repeating that process is slow and wrong.  This function
        /// applies the attributes without repeating the checks.
        /// <param name="attributes">The list of attributes to add.</param>
        internal void AddParameterAttributesNoChecks(Collection<Attribute> attributes)
                _attributes.AddAttributeNoCheck(attribute);
        /// Returns true if the PSVariable is constant (only visible in the
        /// current scope), false otherwise.
        internal bool IsConstant
                return (_options & ScopedItemOptions.Constant) != 0;
        /// Returns true if the PSVariable is readonly (only visible in the
        internal bool IsReadOnly
                return (_options & ScopedItemOptions.ReadOnly) != 0;
        /// Returns true if the PSVariable is private (only visible in the
        internal bool IsPrivate
                return (_options & ScopedItemOptions.Private) != 0;
        /// Returns true if the PSVariable is propagated to all scopes
        /// when the scope is created.
        internal bool IsAllScope
                return (_options & ScopedItemOptions.AllScope) != 0;
        /// Indicates that the variable has been removed from session state
        /// and should no longer be considered valid. This is necessary because
        /// we surface variable references and can consequently not maintain
        /// transparent integrity.
        internal bool WasRemoved
                return _wasRemoved;
                _wasRemoved = value;
                // If set to true, clean up the variable...
                    _options = ScopedItemOptions.None;
                    _value = null;
                    _wasRemoved = true;
                    _attributes = null;
        private bool _wasRemoved;
        internal SessionStateInternal SessionState { get; set; }
        /// Verifies the constraints and attributes before setting the value.
        /// If the validation metadata throws an exception or the value doesn't
        /// pass the validation metadata.
        private void SetValue(object value)
            if ((_options & (ScopedItemOptions.ReadOnly | ScopedItemOptions.Constant)) != ScopedItemOptions.None)
            // Now perform all ArgumentTransformations that are needed
            object transformedValue = value;
                transformedValue = TransformValue(_attributes, value);
                // Next check to make sure the value is valid
                if (!IsValidValue(transformedValue))
                    ValidationMetadataException e = new ValidationMetadataException(
                        Metadata.InvalidValueFailure,
                        ((transformedValue != null) ? transformedValue.ToString() : "$null"));
            if (transformedValue != null)
                transformedValue = CopyMutableValues(transformedValue);
            // Set the value before triggering any write breakpoints
            _value = transformedValue;
            DebuggerCheckVariableWrite();
        private void SetValueRawImpl(object newValue, bool preserveValueTypeSemantics)
            if (preserveValueTypeSemantics)
                newValue = CopyMutableValues(newValue);
            _value = newValue;
        internal virtual void SetValueRaw(object newValue, bool preserveValueTypeSemantics)
            SetValueRawImpl(newValue, preserveValueTypeSemantics);
        private readonly CallSite<Func<CallSite, object, object>> _copyMutableValueSite =
            CallSite<Func<CallSite, object, object>>.Create(PSVariableAssignmentBinder.Get());
        internal object CopyMutableValues(object o)
            // The variable assignment binder copies mutable values and returns other values as is.
            return _copyMutableValueSite.Target.Invoke(_copyMutableValueSite, o);
        internal void WrapValue()
            if (!this.IsConstant)
                    _value = PSObject.AsPSObject(_value);
#if FALSE
        // Replaced with a DLR based binder - but code is preserved in case that approach doesn't
        // work well performance wise.
        // See if it's a value type being assigned and
        // make a copy if it is...
        private static object PreserveValueType(object value)
            // Primitive types are immutable so just return them...
            Type valueType = value.GetType();
            if (valueType.IsPrimitive)
            PSObject valueAsPSObject = value as PSObject;
            if (valueAsPSObject != null)
                object baseObject = valueAsPSObject.BaseObject;
                    valueType = baseObject.GetType();
                    if (valueType.IsValueType && !valueType.IsPrimitive)
                        return valueAsPSObject.Copy();
            else if (valueType.IsValueType)
                return PSObject.CopyValueType(value);
    internal class LocalVariable : PSVariable
        private readonly MutableTuple _tuple;
        private readonly int _tupleSlot;
        public LocalVariable(string name, MutableTuple tuple, int tupleSlot)
            : base(name, false)
            _tuple = tuple;
            _tupleSlot = tupleSlot;
        public override ScopedItemOptions Options
                return base.Options;
                // Throw, but only if someone is actually changing the options.
                if (value != base.Options)
                            "VariableOptionsNotSettable",
                            SessionStateStrings.VariableOptionsNotSettable);
                return _tuple.GetValue(_tupleSlot);
                _tuple.SetValue(_tupleSlot, value);
        internal override object GetValueRaw()
        internal override void SetValueRaw(object newValue, bool preserveValueTypeSemantics)
            this.Value = newValue;
    /// This class is used for $null.  It always returns null as a value and accepts
    /// any value when it is set and throws it away.
    internal class NullVariable : PSVariable
        /// Constructor that calls the base class constructor with name "null" and
        /// value null.
        internal NullVariable() : base(StringLiterals.Null, null, ScopedItemOptions.Constant | ScopedItemOptions.AllScope)
        /// Always returns null from get, and always accepts
        /// but ignores the value on set.
                // All values are just ignored
        /// Gets the description for $null.
            get { return _description ??= SessionStateStrings.DollarNullDescription; }
            set { /* Do nothing */ }
        /// Gets the scope options for $null which is always None.
            get { return ScopedItemOptions.None; }
    /// The options that define some of the constraints for session state items like
    /// variables, aliases, and functions.
    public enum ScopedItemOptions
        /// There are no constraints on the item.
        /// The item is readonly. It can be removed but cannot be changed.
        ReadOnly = 0x1,
        /// The item cannot be removed or changed.
        /// This flag can only be set a variable creation.
        Constant = 0x2,
        /// The item is private to the scope it was created in and
        /// cannot be seen from child scopes.
        Private = 0x4,
        /// The item is propagated to each new child scope created.
        AllScope = 0x8,
        /// The option is not specified by the user.
        Unspecified = 0x10
