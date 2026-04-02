    /// Flags.
    internal enum ParameterBindingFlags
        /// No flags specified.
        /// Set when the argument should be converted to the parameter type.
        ShouldCoerceType = 0x01,
        /// Set when the argument should not be validated or recorded in BoundParameters.
        IsDefaultValue = 0x02,
        /// Set when script blocks can be bound as a script block parameter instead of a normal argument.
        DelayBindScriptBlock = 0x04,
        /// Set when an exception will be thrown if a matching parameter could not be found.
        ThrowOnParameterNotFound = 0x08,
    /// An abstract class used by the CommandProcessor to bind parameters to a bindable object.
    /// Derived classes are used to provide specific binding behavior for different object types,
    /// like Cmdlet, PsuedoParameterCollection, and dynamic parameter objects.
    [DebuggerDisplay("Command = {command}")]
    internal abstract class ParameterBinderBase
        [TraceSource("ParameterBinderBase", "A abstract helper class for the CommandProcessor that binds parameters to the specified object.")]
        private static readonly PSTraceSource s_tracer = PSTraceSource.GetTracer("ParameterBinderBase", "A abstract helper class for the CommandProcessor that binds parameters to the specified object.");
        [TraceSource("ParameterBinding", "Traces the process of binding the arguments to the parameters of cmdlets, scripts, and applications.")]
        internal static readonly PSTraceSource bindingTracer =
                "ParameterBinding",
                "Traces the process of binding the arguments to the parameters of cmdlets, scripts, and applications.",
        /// Constructs the parameter binder with the specified type metadata. The binder is only valid
        /// for a single instance of a bindable object and only for the duration of a command.
        /// The target object that the parameter values will be bound to.
        /// The invocation information for the code that is being bound.
        /// The command that the parameter binder is binding to. The command can be null.
        internal ParameterBinderBase(
            InternalCommand command)
            Diagnostics.Assert(target != null, "caller to verify target is not null.");
            Diagnostics.Assert(invocationInfo != null, "caller to verify invocationInfo is not null.");
            Diagnostics.Assert(context != null, "caller to verify context is not null.");
            bindingTracer.ShowHeaders = false;
            _engine = context.EngineIntrinsics;
        /// Gets or sets the bindable object that the binder will bind parameters to.
        internal object Target
                    _target != null,
                    "The target should always be set for the binder");
                return _target;
                _target = value;
        /// The bindable object that parameters will be bound to.
        private object _target;
        /// Holds the set of parameters that have been bound from the command line...
        internal CommandLineParameters CommandLineParameters
            get { return _commandLineParameters ??= new CommandLineParameters(); }
            // Setter is needed to pass into RuntimeParameterBinder instances
            set { _commandLineParameters = value; }
        private CommandLineParameters _commandLineParameters;
        /// If this is true, then we want to record the list of bound parameters...
        internal bool RecordBoundParameters = true;
        /// Full Qualified ID for the obsolete parameter warning.
        internal const string FQIDParameterObsolete = "ParameterObsolete";
        #region Parameter default values
        /// Derived classes must override this method to get the default parameter
        /// value so that it can be restored between pipeline input.
        internal abstract object GetDefaultParameterValue(string name);
        #endregion Parameter default values
        /// Derived classes define this method to bind the specified value
        /// to the specified parameter.
        ///     The name of the parameter to bind the value to.
        ///     place and that any validation metadata has been satisfied.
        internal abstract void BindParameter(string name, object value, CompiledCommandParameter parameterMetadata);
        private void ValidatePSTypeName(
            CommandParameterInternal parameter,
            CompiledCommandParameter parameterMetadata,
            bool retryOtherBindingAfterFailure,
            object parameterValue)
            Dbg.Assert(parameter != null, "Caller should verify parameter != null");
            Dbg.Assert(parameterMetadata != null, "Caller should verify parameterMetadata != null");
            IEnumerable<string> psTypeNamesOfArgumentValue = PSObject.AsPSObject(parameterValue).InternalTypeNames;
            string psTypeNameRequestedByParameter = parameterMetadata.PSTypeName;
            if (!psTypeNamesOfArgumentValue.Contains(psTypeNameRequestedByParameter, StringComparer.OrdinalIgnoreCase))
                // win8: 228176..The callers know when to ignore and when not to ignore invalid cast exceptions.
                PSInvalidCastException e = new PSInvalidCastException(nameof(ErrorCategory.InvalidArgument),
                        ParameterBinderStrings.MismatchedPSTypeName,
                        (_invocationInfo != null) && (_invocationInfo.MyCommand != null) ? _invocationInfo.MyCommand.Name : string.Empty,
                        parameterMetadata.Type,
                        parameterValue.GetType(),
                        psTypeNameRequestedByParameter);
                ParameterBindingException parameterBindingException;
                if (!retryOtherBindingAfterFailure)
                    parameterBindingException = new ParameterBindingArgumentTransformationException(
                        GetErrorExtent(parameter),
                        "MismatchedPSTypeName",
                    parameterBindingException = new ParameterBindingException(
        /// Does all the type coercion, data generation, and validation necessary to bind the
        /// parameter, then calls the protected BindParameter method to have
        /// the derived class do the actual binding.
        /// The parameter to be bound.
        /// The metadata for the parameter to use in guiding the binding.
        /// True if the parameter was successfully bound. False if <paramref name="coerceTypeIfNeeded"/>
        /// is false and the type does not match the parameter type.
        /// The binding algorithm goes as follows:
        /// 1. The data generation attributes are run
        /// 2. The data is coerced into the correct type
        /// 3. The data if validated using the validation attributes
        /// 4. The data is encoded into the bindable object using the
        ///    protected BindParameter method.
        /// If <paramref name="parameter"/> or <paramref name="parameterMetadata"/> is null.
        internal virtual bool BindParameter(
            bool coerceTypeIfNeeded = (flags & ParameterBindingFlags.ShouldCoerceType) != 0;
            bool isDefaultValue = (flags & ParameterBindingFlags.IsDefaultValue) != 0;
                throw PSTraceSource.NewArgumentNullException(nameof(parameter));
            using (bindingTracer.TraceScope(
                       "BIND arg [{0}] to parameter [{1}]",
                       parameter.ArgumentValue,
                       parameterMetadata.Name))
                // Set the complete parameter name
                parameter.ParameterName = parameterMetadata.Name;
                object parameterValue = parameter.ArgumentValue;
                    // Now call any argument transformation attributes that might be present on the parameter
                    ScriptParameterBinder spb = this as ScriptParameterBinder;
                    bool usesCmdletBinding = false;
                    if (spb != null)
                        usesCmdletBinding = spb.Script.UsesCmdletBinding;
                    // Now do the argument transformation. No transformation is done for the default values in script that meet the following 2 conditions:
                    //  1. the default value is not specified by the user, but is the powershell default value.
                    //     e.g. the powershell default value for a class type is null, for the string type is string.empty.
                    //  2. the powershell default value is null.
                    // This is to prevent ArgumentTransformationAttributes from making a non-mandatory parameter behave like a mandatory one.
                    //   ## without the fix, 'CredentialAttribute' would make $Credential like a mandatory parameter
                    //   ## 'PS> test-credential' would prompt for credential input
                    //   function test-credential {
                    //           [System.Management.Automation.CredentialAttribute()]
                    //           $Credential
                    //       $Credential
                    //   }
                    foreach (ArgumentTransformationAttribute dma in parameterMetadata.ArgumentTransformationAttributes)
                            "Executing DATA GENERATION metadata: [{0}]",
                            dma.GetType()))
                                ArgumentTypeConverterAttribute argumentTypeConverter = dma as ArgumentTypeConverterAttribute;
                                if (argumentTypeConverter != null)
                                    if (coerceTypeIfNeeded)
                                        parameterValue = argumentTypeConverter.Transform(_engine, parameterValue, true, usesCmdletBinding);
                                    // Only apply argument transformation when the argument is not null, is mandatory, or disallows null as a value.
                                    // If we are binding default value for an unbound script parameter, this parameter is guaranteed not mandatory
                                    // in the chosen parameter set. This is because:
                                    //  1. If we use cmdlet binding for this script (CmdletParameterBinderController is used), then binding
                                    //     default value to unbound parameters won't happen until after all mandatory parameters from the
                                    //     chosen parameter set are handled. Therefore, the unbound parameter we are dealing with here won't
                                    //     be mandatory in the chosen parameter set.
                                    //  2. If we use script binding (ScriptParameterBinderController is used), then parameters won't have the
                                    //     ParameterAttribute declared for them, and thus are definitely not mandatory.
                                    // So we check 'IsParameterMandatory' only if we are not binding default values.
                                    if ((parameterValue != null) ||
                                        (!isDefaultValue && (parameterMetadata.IsMandatoryInSomeParameterSet ||
                                                             parameterMetadata.CannotBeNull ||
                                                             dma.TransformNullOptionalParameters)))
                                        parameterValue = dma.TransformInternal(_engine, parameterValue);
                                bindingTracer.WriteLine(
                                    "result returned from DATA GENERATION: {0}",
                                    "ERROR: DATA GENERATION: {0}",
                                            parameterValue?.GetType(),
                                            ParameterBinderStrings.ParameterArgumentTransformationError,
                                            "ParameterArgumentTransformationError",
                    // Only try to coerce the type if asked. If not asked,
                    // see if the value type matches or is a subclass of
                    // the parameter type.
                        // Now do the type coercion
                        parameterValue =
                            CoerceTypeAsNeeded(
                                parameterMetadata.CollectionTypeInformation,
                        if (!ShouldContinueUncoercedBind(parameter, parameterMetadata, flags, ref parameterValue))
                            // Don't attempt the bind because the value
                            // is not of the correct
                            // type for the parameter.
                    if ((parameterMetadata.PSTypeName != null) && (parameterValue != null))
                        IEnumerable parameterValueAsEnumerable = LanguagePrimitives.GetEnumerable(parameterValue);
                        if (parameterValueAsEnumerable != null)
                            foreach (object o in parameterValueAsEnumerable)
                                this.ValidatePSTypeName(parameter, parameterMetadata, !coerceTypeIfNeeded, o);
                            this.ValidatePSTypeName(parameter, parameterMetadata, !coerceTypeIfNeeded, parameterValue);
                    // Now do the data validation.  No validation is done for default values in script as that is
                    // one way for people to have a known "bad" value to detect unspecified parameters.
                    if (!isDefaultValue)
                        for (int i = 0; i < parameterMetadata.ValidationAttributes.Length; i++)
                            var validationAttribute = parameterMetadata.ValidationAttributes[i];
                                "Executing VALIDATION metadata: [{0}]",
                                validationAttribute.GetType()))
                                    validationAttribute.InternalValidate(parameterValue, _engine);
                                        "ERROR: VALIDATION FAILED: {0}",
                                    ParameterBindingValidationException bindingException =
                                        new ParameterBindingValidationException(
                                            ParameterBinderStrings.ParameterArgumentValidationError,
                                            "ParameterArgumentValidationError",
                                s_tracer.WriteLine("Validation attribute on {0} returned {1}.", parameterMetadata.Name, result);
                        // If the is null, an empty string, or an empty collection,
                        // check the parameter metadata to ensure that binding can continue
                        // This method throws an appropriate ParameterBindingException
                        // if binding cannot continue. If it returns then binding can
                        // proceed.
                        if (parameterMetadata.IsMandatoryInSomeParameterSet)
                            ValidateNullOrEmptyArgument(parameter, parameterMetadata, parameterMetadata.Type, parameterValue, true);
                    // Write out obsolete parameter warning only if
                    //  1. We are binding parameters for a simple function/script
                    //  2. We are not binding a default parameter value
                    if (parameterMetadata.ObsoleteAttribute != null &&
                        (!isDefaultValue) &&
                        spb != null && !usesCmdletBinding)
                            parameterMetadata.ObsoleteAttribute.Message);
                        var mshCommandRuntime = this.Command.commandRuntime as MshCommandRuntime;
                        // Write out warning only if we are in the context of MshCommandRuntime.
                        // This is because
                        //  1. The overload method WriteWarning(WarningRecord) is only available in MshCommandRuntime;
                        //  2. We write out warnings for obsolete commands and obsolete cmdlet parameters only when in
                        //     the context of MshCommandRuntime. So we do it here to keep consistency.
                        mshCommandRuntime?.WriteWarning(new WarningRecord(FQIDParameterObsolete, obsoleteWarning));
                    // Finally bind the argument to the parameter
                    Exception bindError = null;
                        BindParameter(parameter.ParameterName, parameterValue, parameterMetadata);
                        bindError = setValueException;
                    if (bindError != null)
                        Type specifiedType = parameterValue?.GetType();
                                bindError,
                                bindError.Message);
                    "BIND arg [{0}] to param [{1}] {2}",
                    (result) ? "SUCCESSFUL" : "SKIPPED");
                    // Add this name to the set of bound parameters...
                    if (RecordBoundParameters)
                        this.CommandLineParameters.Add(parameter.ParameterName, parameterValue);
                    MshCommandRuntime cmdRuntime = this.Command.commandRuntime as MshCommandRuntime;
                    if ((cmdRuntime != null) &&
                        (cmdRuntime.LogPipelineExecutionDetail || _isTranscribing) &&
                        (cmdRuntime.PipelineProcessor != null))
                        string stringToPrint = null;
                            // Unroll parameter value
                            IEnumerable values = LanguagePrimitives.GetEnumerable(parameterValue);
                                var sb = new Text.StringBuilder(256);
                                foreach (var value in values)
                                    sb.Append(sep);
                                    sep = ", ";
                                    // For better performance, avoid logging too much
                                    if (sb.Length > 256)
                                        sb.Append(", ...");
                                stringToPrint = sb.ToString();
                            else if (parameterValue != null)
                                stringToPrint = parameterValue.ToString();
                        if (stringToPrint != null)
                            cmdRuntime.PipelineProcessor.LogExecutionParameterBinding(this.InvocationInfo, parameter.ParameterName, stringToPrint);
        /// This method ensures that if the parameter is mandatory, and AllowNull, AllowEmptyString,
        /// and/or AllowEmptyCollection is not specified, then argument is not null or empty.
        /// The argument token.
        /// The metadata for the parameter.
        /// <param name="argumentType">
        /// The type of the argument to validate against.
        /// The value that will be bound to the parameter.
        /// <param name="recurseIntoCollections">
        /// If true, then elements of collections will be validated against the metadata.
        private void ValidateNullOrEmptyArgument(
            Type argumentType,
            bool recurseIntoCollections)
            if (parameterValue == null && argumentType != typeof(bool?))
                if (!parameterMetadata.AllowsNullArgument)
                    bindingTracer.WriteLine("ERROR: Argument cannot be null");
                            argumentType,
                            ParameterBinderStrings.ParameterArgumentValidationErrorNullNotAllowed,
                            "ParameterArgumentValidationErrorNullNotAllowed");
            if (argumentType == typeof(string))
                // Since the parameter is of type string, verify that either the argument
                // is not null and not empty or that the parameter can accept null or empty.
                string stringParamValue = parameterValue as string;
                    stringParamValue != null,
                    "Type coercion should have already converted the argument value to a string");
                if (stringParamValue.Length == 0 && !parameterMetadata.AllowsEmptyStringArgument)
                    bindingTracer.WriteLine("ERROR: Argument cannot be an empty string");
                            ParameterBinderStrings.ParameterArgumentValidationErrorEmptyStringNotAllowed,
                            "ParameterArgumentValidationErrorEmptyStringNotAllowed");
            if (!recurseIntoCollections)
            switch (parameterMetadata.CollectionTypeInformation.ParameterCollectionType)
                    // not a recognized collection, no need to recurse
            // All these collection types implement IEnumerable
            IEnumerator ienum = LanguagePrimitives.GetEnumerator(parameterValue);
                ienum != null,
                "Type coercion should have already converted the argument value to an IEnumerator");
            // Ensure that each element abides by the metadata
            Type elementType = parameterMetadata.CollectionTypeInformation.ElementType;
            bool isElementValueType = elementType != null && elementType.IsValueType;
            // Note - we explicitly don't pass the context here because we don't want
            // the overhead of the calls that check for stopping.
            if (ParserOps.MoveNext(null, null, ienum))
                    object element = ParserOps.Current(null, ienum);
                    ValidateNullOrEmptyArgument(
                        parameterMetadata.CollectionTypeInformation.ElementType,
                        element,
                } while (ParserOps.MoveNext(null, null, ienum));
            if (isEmpty && !parameterMetadata.AllowsEmptyCollectionArgument)
                bindingTracer.WriteLine("ERROR: Argument cannot be an empty collection");
                string errorId, resourceString;
                if (parameterMetadata.CollectionTypeInformation.ParameterCollectionType == ParameterCollectionType.Array)
                    errorId = "ParameterArgumentValidationErrorEmptyArrayNotAllowed";
                    resourceString = ParameterBinderStrings.ParameterArgumentValidationErrorEmptyArrayNotAllowed;
                    errorId = "ParameterArgumentValidationErrorEmptyCollectionNotAllowed";
                    resourceString = ParameterBinderStrings.ParameterArgumentValidationErrorEmptyCollectionNotAllowed;
        private bool ShouldContinueUncoercedBind(
            ParameterBindingFlags flags,
            ref object parameterValue)
            Type parameterType = parameterMetadata.Type;
                return parameterType == null ||
                       isDefaultValue ||
                       (!parameterType.IsValueType &&
                        parameterType != typeof(string));
            // If the types are not a direct match, or
            // the value type is not a subclass of the parameter type, or
            // the value is an PSObject and the parameter type is not object and
            //     the PSObject.BaseObject type does not match or is not a subclass
            //     of the parameter type, or
            // the value must be encoded into a collection but it is not of the correct element type
            // then return false
            if (parameterType.IsInstanceOfType(parameterValue))
            var psobj = parameterValue as PSObject;
            if (psobj != null && !psobj.ImmediateBaseObjectIsEmpty)
                // See if the base object is of the same type or
                // as subclass of the parameter
                parameterValue = psobj.BaseObject;
            // Maybe the parameter type is a collection and the value needs to
            // be encoded
            if (parameterMetadata.CollectionTypeInformation.ParameterCollectionType != ParameterCollectionType.NotCollection)
                // See if the value needs to be encoded in a collection
                bool coercionRequired;
                object encodedValue =
                    EncodeCollection(
                        out coercionRequired);
                if (encodedValue == null || coercionRequired)
                    // Don't attempt the bind because the
                    // PSObject BaseObject is not of the correct
                parameterValue = encodedValue;
        internal InvocationInfo InvocationInfo
                return _invocationInfo;
        /// An instance of InternalCommand that the binder is binding to.
        /// The engine APIs that need to be passed the attributes when evaluated.
        private readonly EngineIntrinsics _engine;
        #region Private helpers
        /// Coerces the argument type to the parameter value type as needed.
        /// The argument as was specified by the command line.
        /// The name of the parameter that the coercion is taking place to bind to. It is
        /// used only for error reporting.
        /// <param name="toType">
        /// The type to coerce the value to.
        /// <param name="collectionTypeInfo">
        /// The information about the collection type, like element type, etc.
        /// <param name="currentValue">
        /// The current value of the argument.
        /// The value of the argument in the type of the parameter.
        /// If <paramref name="argument"/> or <paramref name="toType"/> is null.
        /// If the argument value is missing and the parameter is not a bool or SwitchParameter.
        /// If the argument value could not be converted to the parameter type.
        private object CoerceTypeAsNeeded(
            Type toType,
            ParameterCollectionTypeInformation collectionTypeInfo,
            object currentValue)
                throw PSTraceSource.NewArgumentNullException(nameof(argument));
            if (toType == null)
                throw PSTraceSource.NewArgumentNullException(nameof(toType));
            // Construct the collection type information if it wasn't passed in.
            collectionTypeInfo ??= new ParameterCollectionTypeInformation(toType);
            object originalValue = currentValue;
            object result = currentValue;
                "COERCE arg to [{0}]", toType))
                Type argumentType = null;
                        if (IsNullParameterValue(currentValue))
                            result = HandleNullParameterForSpecialTypes(argument, parameterName, toType, currentValue);
                        // Do the coercion
                        argumentType = currentValue.GetType();
                        // If the types are identical (or can be cast directly,) then no coercion
                        // needs to be done
                        if (toType.IsAssignableFrom(argumentType))
                                "Parameter and arg types the same, no coercion is needed.");
                            result = currentValue;
                        bindingTracer.WriteLine("Trying to convert argument value from {0} to {1}", argumentType, toType);
                        // Likewise shortcircuit the case were the user has asked for a shell object.
                        // He always gets a shell object regardless of the actual type of the object.
                            // It may be the case that we're binding the current pipeline object
                            // as is to a PSObject parameter in which case, we want to make
                            // sure that we're using the same shell object instead of creating an
                            // alias object.
                            if (_command != null &&
                                currentValue == _command.CurrentPipelineObject.BaseObject)
                                currentValue = _command.CurrentPipelineObject;
                                "The parameter is of type [{0}] and the argument is an PSObject, so the parameter value is the argument value wrapped into an PSObject.",
                                toType);
                            result = LanguagePrimitives.AsPSObjectOrNull(currentValue);
                        // NTRAID#Windows OS Bugs-1064175-2004/02/28-JeffJon
                        // If we have an PSObject with null base and we are trying to
                        // convert to a string, then we need to use null instead of
                        // calling LanguagePrimitives.ConvertTo as that will return
                        // string.Empty.
                        if (toType == typeof(string) &&
                            argumentType == typeof(PSObject))
                            PSObject currentValueAsPSObject = (PSObject)currentValue;
                            if (currentValueAsPSObject == AutomationNull.Value)
                                    "CONVERT a null PSObject to a null string.");
                        // NTRAID#Windows OS Bugs -<bug id here> - Nana
                        // If we have a boolean, we have to ensure that it can
                        // only take parameters of type boolean or numbers with
                        // 0 indicating false and everything else indicating true
                        // Anything else passed should be reported as an error
                        if (toType == typeof(bool) || toType == typeof(SwitchParameter) ||
                            toType == typeof(bool?))
                            Type boType = null;
                            if (argumentType == typeof(PSObject))
                                // Unwrap the PSObject at this point...
                                currentValue = currentValueAsPSObject.BaseObject;
                                if (currentValue is SwitchParameter)
                                    currentValue = ((SwitchParameter)currentValue).IsPresent;
                                boType = currentValue.GetType();
                                boType = argumentType;
                            if (boType == typeof(bool))
                                if (LanguagePrimitives.IsBooleanType(toType))
                                    result = ParserOps.BoolToObject((bool)currentValue);
                                    result = new SwitchParameter((bool)currentValue);
                            else if (boType == typeof(int))
                                if ((int)LanguagePrimitives.ConvertTo(currentValue,
                                            typeof(int), CultureInfo.InvariantCulture) != 0)
                                        result = ParserOps.BoolToObject(true);
                                        result = new SwitchParameter(true);
                                        result = ParserOps.BoolToObject(false);
                                        result = new SwitchParameter(false);
                            else if (LanguagePrimitives.IsNumeric(boType.GetTypeCode()))
                                double currentValueAsDouble = (double)LanguagePrimitives.ConvertTo(
                                                                        currentValue, typeof(double), CultureInfo.InvariantCulture);
                                if (currentValueAsDouble != 0)
                                // Invalid types which cannot be associated with a bool
                                // Since there is a catch block which appropriately
                                // handles this situation we just throw an exception here
                                // throw new PSInvalidCastException();
                                ParameterBindingException pbe =
                                        toType,
                                        ParameterBinderStrings.CannotConvertArgument,
                                        "CannotConvertArgument",
                                        boType,
                                throw pbe;
                        // NTRAID#Windows OS Bugs-1009284-2004/05/05-JeffJon
                        // Need to handle other collection types here as well
                        // Before attempting to encode a collection, we check if we can convert the argument directly via
                        // a restricted set of conversions (as the general conversion mechanism is far too general, it may
                        // succeed where it shouldn't.  We don't bother checking arrays parameters because they won't have
                        // conversions we're allowing.
                        if (collectionTypeInfo.ParameterCollectionType == ParameterCollectionType.ICollectionGeneric
                            || collectionTypeInfo.ParameterCollectionType == ParameterCollectionType.IList)
                            object currentValueToConvert = PSObject.Base(currentValue);
                            if (currentValueToConvert != null)
                                ConversionRank rank = LanguagePrimitives.GetConversionRank(currentValueToConvert.GetType(), toType);
                                if (rank == ConversionRank.Constructor || rank == ConversionRank.ImplicitCast || rank == ConversionRank.ExplicitCast)
                                    // This conversion will fail in the common case, and when it does, we'll use EncodeCollection below.
                                    if (LanguagePrimitives.TryConvertTo(currentValue, toType, CultureInfo.CurrentCulture, out result))
                        if (collectionTypeInfo.ParameterCollectionType != ParameterCollectionType.NotCollection)
                                "ENCODING arg into collection");
                            bool ignored = false;
                                    collectionTypeInfo,
                                    currentValue,
                                    (collectionTypeInfo.ElementType != null),
                                    out ignored);
                            // Check to see if the current value is a collection. If so, fail because
                            // we don't want to attempt to bind a collection to a scalar unless
                            // the parameter type is Object or PSObject or enum.
                            if (GetIList(currentValue) != null &&
                                toType != typeof(object) &&
                                toType != typeof(PSObject) &&
                                toType != typeof(PSListModifier) &&
                                (!toType.IsGenericType || toType.GetGenericTypeDefinition() != typeof(PSListModifier<>)) &&
                                (!toType.IsGenericType || toType.GetGenericTypeDefinition() != typeof(FlagsExpression<>)) &&
                                !toType.IsEnum)
                            "CONVERT arg type to param type using LanguagePrimitives.ConvertTo");
                        // If we are in constrained language mode and the target command is trusted, which is often
                        // the case for C# cmdlets, then we allow type conversion to the target parameter type.
                        // However, we don't allow Hashtable-to-Object conversion (PSObject and IDictionary) because
                        // those can lead to property setters that probably aren't expected. This is enforced by
                        // setting 'Context.LanguageModeTransitionInParameterBinding' to true before the conversion.
                        var currentLanguageMode = Context.LanguageMode;
                        bool changeLanguageModeForTrustedCommand =
                            currentLanguageMode == PSLanguageMode.ConstrainedLanguage &&
                            this.Command.CommandInfo.DefiningLanguageMode == PSLanguageMode.FullLanguage;
                        bool oldLangModeTransitionStatus = Context.LanguageModeTransitionInParameterBinding;
                            if (changeLanguageModeForTrustedCommand)
                                Context.LanguageMode = PSLanguageMode.FullLanguage;
                            result = LanguagePrimitives.ConvertTo(currentValue, toType, CultureInfo.CurrentCulture);
                                Context.LanguageMode = currentLanguageMode;
                                Context.LanguageModeTransitionInParameterBinding = oldLangModeTransitionStatus;
                            "CONVERT SUCCESSFUL using LanguagePrimitives.ConvertTo: [{0}]",
                            (result == null) ? "null" : result.ToString());
                    bindingTracer.TraceError(
                        "ERROR: COERCE FAILED: arg [{0}] could not be converted to the parameter type [{1}]",
                        result ?? "null",
                            notSupported,
                            notSupported.Message);
                catch (PSInvalidCastException invalidCast)
                            invalidCast,
                            ParameterBinderStrings.CannotConvertArgumentNoMessage,
                            "CannotConvertArgumentNoMessage",
                            invalidCast.Message);
                // Set the converted result object untrusted if necessary
                ExecutionContext.PropagateInputSource(originalValue, result, Context.LanguageMode);
        private static bool IsNullParameterValue(object currentValue)
            if (currentValue == null ||
                currentValue == AutomationNull.Value ||
                currentValue == UnboundParameter.Value)
        private object HandleNullParameterForSpecialTypes(
            // The presence of the name switch for SwitchParameters (and not booleans)
            // makes them true.
                        "ERROR: No argument is specified for parameter and parameter type is BOOL");
                        "ParameterArgumentValidationErrorNullNotAllowed",
                if (toType == typeof(SwitchParameter))
                    "Arg is null or not present, parameter type is SWITCHPARAMTER, value is true.");
                result = SwitchParameter.Present;
            else if (currentValue == UnboundParameter.Value)
                    "ERROR: No argument was specified for the parameter and the parameter is not of type bool");
                        GetParameterErrorExtent(argument),
                        ParameterBinderStrings.MissingArgument,
                        "MissingArgument");
                    "Arg is null, parameter type not bool or SwitchParameter, value is null.");
        /// Takes the current value specified and converts or adds it to
        /// a collection of the appropriate type.
        /// The argument the current value comes from. Used for error reporting.
        /// <param name="collectionTypeInformation">
        /// The collection type information to which the current value will be
        /// encoded.
        /// The type the current value will be converted to.
        /// The value to be encoded.
        /// <param name="coerceElementTypeIfNeeded">
        /// If true, the element will be coerced into the appropriate type
        /// for the collection. If false, and the element isn't of the appropriate
        /// type then the <paramref name="coercionRequired"/> out parameter will
        /// be true.
        /// <param name="coercionRequired">
        /// This out parameter will be true if <paramref name="coerceElementTypeIfNeeded"/>
        /// is true and the value could not be encoded into the collection because it
        /// requires coercion to the element type.
        /// A collection of the appropriate type containing the specified value.
        /// If <paramref name="currentValue"/> is a collection and one of its values
        /// cannot be coerced into the appropriate type.
        /// A collection of the appropriate <paramref name="collectionTypeInformation"/>
        /// could not be created.
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Consider Simplifying it")]
        private object EncodeCollection(
            ParameterCollectionTypeInformation collectionTypeInformation,
            object currentValue,
            bool coerceElementTypeIfNeeded,
            out bool coercionRequired)
            coercionRequired = false;
                    "Binding collection parameter {0}: argument type [{1}], parameter type [{2}], collection type {3}, element type [{4}], {5}",
                    (currentValue == null) ? "null" : currentValue.GetType().Name,
                    collectionTypeInformation.ParameterCollectionType,
                    collectionTypeInformation.ElementType,
                    coerceElementTypeIfNeeded ? "coerceElementType" : "no coerceElementType");
                if (currentValue == null)
                int numberOfElements = 1;
                Type collectionElementType = collectionTypeInformation.ElementType;
                // If the current value is an IList, get the count of the elements
                // Or if it is an PSObject which wraps an IList
                IList currentValueAsIList = GetIList(currentValue);
                    numberOfElements = currentValueAsIList.Count;
                    s_tracer.WriteLine("current value is an IList with {0} elements", numberOfElements);
                        "Arg is IList with {0} elements",
                        numberOfElements);
                object resultCollection = null;
                IList resultAsIList = null;
                MethodInfo addMethod = null;
                // We must special case System.Array to be like an object array since it is an
                // abstract base class and cannot be created in the IList path below.
                bool isSystemDotArray = (toType == typeof(System.Array));
                if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.Array ||
                    isSystemDotArray)
                    if (isSystemDotArray)
                        // If System.Array is the type we are encoding to, then
                        // the element type should be System.Object.
                        collectionElementType = typeof(object);
                        "Creating array with element type [{0}] and {1} elements",
                        collectionElementType,
                    // Since the destination is an array we will have to create an array
                    // of the element type with the correct length
                    resultCollection = resultAsIList =
                        (IList)Array.CreateInstance(
                else if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.IList ||
                         collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.ICollectionGeneric)
                        "Creating collection [{0}]",
                    // Create an instance of the parameter type
                    // NTRAID#Windows Out Of Band Releases-906820-2005/09/01
                    // This code previously used the ctor(int) ctor form.
                    // System.Collections.ObjectModel.Collection<T> does not
                    // support this ctor form.  More generally, there is no
                    // guarantee that the ctor parameter has the semantic
                    // meaning of "likely list size".  Blindly calling the
                    // parameterless ctor is also risky, but seems like a
                    // safer choice.
                    bool errorOccurred = false;
                        resultCollection =
                            Activator.CreateInstance(
                                Array.Empty<object>(),
                                System.Globalization.CultureInfo.InvariantCulture);
                        if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.IList)
                            resultAsIList = (IList)resultCollection;
                                collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.ICollectionGeneric,
                                "invalid collection type"
                            // extract the ICollection<T>::Add(T) method
                            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                            Type elementType = collectionTypeInformation.ElementType;
                            Diagnostics.Assert(elementType != null, "null ElementType");
                            Exception getMethodError = null;
                                addMethod = toType.GetMethod("Add", bindingFlags, null, new Type[1] { elementType }, null);
                                bindingTracer.WriteLine("Ambiguous match to Add(T) for type {0}: {1}", toType.FullName, e.Message);
                                getMethodError = e;
                                    "ArgumentException matching Add(T) for type {0}: {1}", toType.FullName, e.Message);
                            if (addMethod == null)
                                        getMethodError,
                                        currentValue.GetType(),
                                        ParameterBinderStrings.CannotExtractAddMethod,
                                        "CannotExtractAddMethod",
                                        (getMethodError == null) ? string.Empty : getMethodError.Message);
                        errorOccurred = true;
                        error = argException;
                    catch (TargetInvocationException targetInvocationException)
                        error = targetInvocationException;
                    catch (MethodAccessException methodAccessException)
                        error = methodAccessException;
                        error = memberAccessException;
                    catch (System.Runtime.InteropServices.InvalidComObjectException invalidComObject)
                        error = invalidComObject;
                    catch (System.Runtime.InteropServices.COMException comException)
                        error = comException;
                        error = typeLoadException;
                    if (errorOccurred)
                        // Throw a ParameterBindingException
                                "null",
                        "This method should not be called for a parameter that is not a collection");
                // NTRAID#Windows OS Bugs-966440-2004/05/05-JeffJon
                // This coercion can only go to a collection type.  It cannot take a
                // collection type and coerce it into a scalar type.
                // Now that the new collection instance has been created, coerce each element type
                // of the current value to the element type of the property value and add it
                    // Since arrays don't support the Add method, we must use indexing
                    // to set the value.
                    int arrayIndex = 0;
                        "Argument type {0} is IList",
                        currentValue.GetType());
                    foreach (object valueElement in currentValueAsIList)
                        object currentValueElement = PSObject.Base(valueElement);
                        if (coerceElementTypeIfNeeded)
                                "COERCE collection element from type {0} to type {1}",
                                (valueElement == null) ? "null" : valueElement.GetType().Name,
                                collectionElementType);
                            // Coerce the element to the appropriate type.
                            // Note, this may be recursive if the element is a
                            // collection itself.
                            currentValueElement =
                                        valueElement);
                        else if (collectionElementType != null && currentValueElement != null)
                            Type currentValueElementType = currentValueElement.GetType();
                            Type desiredElementType = collectionElementType;
                            if (currentValueElementType != desiredElementType &&
                                !currentValueElementType.IsSubclassOf(desiredElementType))
                                    "COERCION REQUIRED: Did not attempt to coerce collection element from type {0} to type {1}",
                                coercionRequired = true;
                        // Add() will fail with ArgumentException
                        // for Collection<T> with the wrong type.
                                    "Adding element of type {0} to array position {1}",
                                    (currentValueElement == null) ? "null" : currentValueElement.GetType().Name,
                                    arrayIndex);
                                resultAsIList[arrayIndex++] = currentValueElement;
                            else if (collectionTypeInformation.ParameterCollectionType == ParameterCollectionType.IList)
                                    "Adding element of type {0} via IList.Add",
                                    (currentValueElement == null) ? "null" : currentValueElement.GetType().Name);
                                resultAsIList.Add(currentValueElement);
                                    "Adding element of type {0} via ICollection<T>::Add()",
                                addMethod.Invoke(resultCollection, new object[1] { currentValueElement });
                        catch (Exception error) // OK, we catch all here by design
                            // The inner exception to TargetInvocationException
                            // (if present) has a better Message
                            if (error is TargetInvocationException &&
                                error.InnerException != null)
                                error = error.InnerException;
                                    currentValueElement?.GetType(),
                                    currentValueElement ?? "null",
                else // (currentValueAsIList == null)
                        "Argument type {0} is not IList, treating this as scalar",
                        currentValue.GetType().Name);
                    if (collectionElementType != null)
                                "Coercing scalar arg value to type {0}",
                            // Coerce the scalar type into the collection
                            currentValue =
                                    currentValue);
                            Type currentValueElementType = currentValue.GetType();
                                    "COERCION REQUIRED: Did not coerce scalar arg value to type {1}",
                                "Adding scalar element of type {0} to array position {1}",
                            resultAsIList[0] = currentValue;
                                "Adding scalar element of type {0} via IList.Add",
                                (currentValue == null) ? "null" : currentValue.GetType().Name);
                            resultAsIList.Add(currentValue);
                                "Adding scalar element of type {0} via ICollection<T>::Add()",
                            addMethod.Invoke(resultCollection, new object[1] { currentValue });
                                currentValue?.GetType(),
                                currentValue ?? "null",
                if (!coercionRequired)
                    result = resultCollection;
        internal static IList GetIList(object value)
            var baseObj = PSObject.Base(value);
            var result = baseObj as IList;
                // Reference comparison to determine if 'value' is a PSObject
                s_tracer.WriteLine(baseObj == value
                                     ? "argument is IList"
                                     : "argument is PSObject with BaseObject as IList");
        protected IScriptExtent GetErrorExtent(CommandParameterInternal cpi)
            var result = cpi.ErrorExtent;
            if (result == PositionUtilities.EmptyExtent)
                result = InvocationInfo.ScriptPosition;
            // Can't use this assertion - we don't have useful positions when invoked via PowerShell API
            // Diagnostics.Assert(result != PositionUtilities.EmptyExtent, "We are missing a valid position somewhere");
        protected IScriptExtent GetParameterErrorExtent(CommandParameterInternal cpi)
            var result = cpi.ParameterExtent;
        #endregion private helpers
    /// Represents an unbound parameter object in the engine. It's similar to
    /// AutomationNull.Value however AutomationNull.Value as a parameter value
    /// is used to say "use the default value for this object" whereas UnboundParameter
    /// says "this parameter is unbound, use the default only if the target type
    /// supports permits this."
    /// <remarks>It's a singleton class. Sealed to prevent subclassing</remarks>
    internal sealed class UnboundParameter
        // Private constructor
        private UnboundParameter() { }
        /// Represents an object of the same class (singleton class).
        internal static object Value { get; } = new object();
    // This class is a thin wrapper around Dictionary, but adds a member BoundPositionally.
    // $PSBoundParameters used to be a PSObject with an instance member, but that was quite
    // slow for a relatively common case, this class should work identically, except maybe
    // if somebody depends on the typename being the same.
    internal sealed class PSBoundParametersDictionary : Dictionary<string, object>
        internal PSBoundParametersDictionary()
            BoundPositionally = new List<string>();
            ImplicitUsingParameters = s_emptyUsingParameters;
        private static readonly IDictionary s_emptyUsingParameters = new ReadOnlyDictionary<object, object>(new Dictionary<object, object>());
        public List<string> BoundPositionally { get; }
        internal IDictionary ImplicitUsingParameters { get; set; }
    internal sealed class CommandLineParameters
        private readonly PSBoundParametersDictionary _dictionary = new PSBoundParametersDictionary();
        internal bool ContainsKey(string name)
            Dbg.Assert(!string.IsNullOrEmpty(name), "parameter names should not be empty");
            return _dictionary.ContainsKey(name);
        internal void Add(string name, object value)
            _dictionary[name] = value;
        internal void MarkAsBoundPositionally(string name)
            _dictionary.BoundPositionally.Add(name);
        internal void SetPSBoundParametersVariable(ExecutionContext context)
            Dbg.Assert(context != null, "caller should verify that context != null");
            context.SetVariable(SpecialVariables.PSBoundParametersVarPath, _dictionary);
        internal void SetImplicitUsingParameters(object obj)
            _dictionary.ImplicitUsingParameters = PSObject.Base(obj) as IDictionary;
            if (_dictionary.ImplicitUsingParameters == null)
                // Handle downlevel V4 case where using parameters are passed as an array list.
                IList implicitArrayUsingParameters = PSObject.Base(obj) as IList;
                if ((implicitArrayUsingParameters != null) && (implicitArrayUsingParameters.Count > 0))
                    // Convert array to hash table.
                    _dictionary.ImplicitUsingParameters = new Hashtable();
                    for (int index = 0; index < implicitArrayUsingParameters.Count; index++)
                        _dictionary.ImplicitUsingParameters.Add(index, implicitArrayUsingParameters[index]);
        internal IDictionary GetImplicitUsingParameters()
            return _dictionary.ImplicitUsingParameters;
        internal object GetValueToBindToPSBoundParameters()
            return _dictionary;
        internal void UpdateInvocationInfo(InvocationInfo invocationInfo)
            Dbg.Assert(invocationInfo != null, "caller should verify that invocationInfo != null");
            invocationInfo.BoundParameters = _dictionary;
        internal HashSet<string> CopyBoundPositionalParameters()
            HashSet<string> result = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (string item in _dictionary.BoundPositionally)
