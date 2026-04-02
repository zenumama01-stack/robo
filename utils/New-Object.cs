    /// <summary>Create a new .net object</summary>
    [Cmdlet(VerbsCommon.New, "Object", DefaultParameterSetName = netSetName, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096620")]
    public sealed class NewObjectCommand : PSCmdlet
        /// <summary> the number</summary>
        [Parameter(ParameterSetName = netSetName, Mandatory = true, Position = 0)]
        public string TypeName { get; set; }
        private Guid _comObjectClsId = Guid.Empty;
        /// The ProgID of the Com object.
        [Parameter(ParameterSetName = "Com", Mandatory = true, Position = 0)]
        public string ComObject { get; set; }
        /// The parameters for the constructor.
        [Parameter(ParameterSetName = netSetName, Mandatory = false, Position = 1)]
        public object[] ArgumentList { get; set; }
        /// True if we should have an error when Com objects will use an interop assembly.
        [Parameter(ParameterSetName = "Com")]
        public SwitchParameter Strict { get; set; }
        // Updated from Hashtable to IDictionary to support the work around ordered hashtables.
        /// Gets the properties to be set.
        private object CallConstructor(Type type, ConstructorInfo[] constructors, object[] args)
                result = DotNetAdapter.ConstructorInvokeDotNet(type, constructors, args);
            catch (MethodException e)
                    "ConstructorInvokedThrowException",
                    ErrorCategory.InvalidOperation, null));
            // let other exceptions propagate
        private void CreateMemberNotFoundError(PSObject pso, DictionaryEntry property, Type resultType)
            string message = StringUtil.Format(NewObjectStrings.MemberNotFound, null, property.Key.ToString(), ParameterSet2ResourceString(ParameterSetName));
                    new InvalidOperationException(message),
                    "InvalidOperationException",
        private void CreateMemberSetValueError(SetValueException e)
            Exception ex = new(StringUtil.Format(NewObjectStrings.InvalidValue, e));
                new ErrorRecord(ex, "SetValueException", ErrorCategory.InvalidData, null));
        private static string ParameterSet2ResourceString(string parameterSet)
            if (parameterSet.Equals(netSetName, StringComparison.OrdinalIgnoreCase))
                return ".NET";
            else if (parameterSet.Equals("Com", StringComparison.OrdinalIgnoreCase))
                return "COM";
                Dbg.Assert(false, "Should never get here - unknown parameter set");
                return parameterSet;
        /// <summary> Create the object </summary>
            Type type = null;
            PSArgumentException mshArgE = null;
            if (string.Equals(ParameterSetName, netSetName, StringComparison.Ordinal))
                object _newObject = null;
                    type = LanguagePrimitives.ConvertTo(TypeName, typeof(Type), CultureInfo.InvariantCulture) as Type;
                    // these complications in Exception handling are aim to make error messages better.
                    if (e is InvalidCastException || e is ArgumentException)
                        if (e.InnerException != null && e.InnerException is TypeResolver.AmbiguousTypeException)
                                    "AmbiguousTypeReference",
                        mshArgE = PSTraceSource.NewArgumentException(
                            "TypeName",
                            NewObjectStrings.TypeNotFound,
                            TypeName);
                                mshArgE,
                                "TypeNotFound",
                Diagnostics.Assert(type != null, "LanguagePrimitives.TryConvertTo failed but returned true");
                if (type.IsByRefLike)
                                NewObjectStrings.CannotInstantiateBoxedByRefLikeType,
                                type),
                            nameof(NewObjectStrings.CannotInstantiateBoxedByRefLikeType),
                switch (Context.LanguageMode)
                    case PSLanguageMode.ConstrainedLanguage:
                        if (!CoreTypes.Contains(type))
                                        new PSNotSupportedException(NewObjectStrings.CannotCreateTypeConstrainedLanguage),
                                        "CannotCreateTypeConstrainedLanguage",
                                title: NewObjectStrings.TypeWDACLogTitle,
                                message: StringUtil.Format(NewObjectStrings.TypeWDACLogMessage, type.FullName),
                                fqid: "NewObjectCmdletCannotCreateType",
                    case PSLanguageMode.NoLanguage:
                    case PSLanguageMode.RestrictedLanguage:
                        if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce
                            && !CoreTypes.Contains(type))
                                    new PSNotSupportedException(
                                        string.Format(NewObjectStrings.CannotCreateTypeLanguageMode, Context.LanguageMode.ToString())),
                                    nameof(NewObjectStrings.CannotCreateTypeLanguageMode),
                // WinRT does not support creating instances of attribute & delegate WinRT types.
                if (WinRTHelper.IsWinRTType(type) && ((typeof(System.Attribute)).IsAssignableFrom(type) || (typeof(System.Delegate)).IsAssignableFrom(type)))
                    ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(NewObjectStrings.CannotInstantiateWinRTType),
                        "CannotInstantiateWinRTType", ErrorCategory.InvalidOperation, null));
                if (ArgumentList == null || ArgumentList.Length == 0)
                    ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
                    if (ci != null && ci.IsPublic)
                        _newObject = CallConstructor(type, new ConstructorInfo[] { ci }, Array.Empty<object>());
                        if (_newObject != null && Property != null)
                            // The method invocation is disabled for "Hashtable to Object conversion" (Win8:649519), but we need to keep it enabled for New-Object for compatibility to PSv2
                            _newObject = LanguagePrimitives.SetObjectProperties(_newObject, Property, type, CreateMemberNotFoundError, CreateMemberSetValueError, enableMethodCall: true);
                        WriteObject(_newObject);
                    else if (type.IsValueType)
                        // This is for default parameterless struct ctor which is not returned by
                        // Type.GetConstructor(System.Type.EmptyTypes).
                            _newObject = Activator.CreateInstance(type);
                                // Win8:649519
                        catch (TargetInvocationException e)
                                e.InnerException ?? e,
                                "ConstructorCalledThrowException",
                    ConstructorInfo[] ctorInfos = type.GetConstructors();
                    if (ctorInfos.Length != 0)
                        _newObject = CallConstructor(type, ctorInfos, ArgumentList);
                    "TypeName", NewObjectStrings.CannotFindAppropriateCtor, TypeName);
                     "CannotFindAppropriateCtor",
                     ErrorCategory.ObjectNotFound, null));
            else // Parameterset -Com
                int result = NewObjectNativeMethods.CLSIDFromProgID(ComObject, out _comObjectClsId);
                // If we're in ConstrainedLanguage, do additional restrictions
                    bool isAllowed = false;
                    // If it's a system-wide lockdown, we may allow additional COM types
                    var systemLockdownPolicy = SystemPolicy.GetSystemLockdownPolicy();
                    if (systemLockdownPolicy == SystemEnforcementMode.Enforce || systemLockdownPolicy == SystemEnforcementMode.Audit)
                        isAllowed = (result >= 0) && SystemPolicy.IsClassInApprovedList(_comObjectClsId);
                    if (!isAllowed)
                                    "CannotCreateComTypeConstrainedLanguage",
                            title: NewObjectStrings.ComWDACLogTitle,
                            message: StringUtil.Format(NewObjectStrings.ComWDACLogMessage, ComObject ?? string.Empty),
                            fqid: "NewObjectCmdletCannotCreateCOM",
                object comObject = CreateComObject();
                string comObjectTypeName = comObject.GetType().FullName;
                if (!comObjectTypeName.Equals("System.__ComObject"))
                        "TypeName", NewObjectStrings.ComInteropLoaded, comObjectTypeName);
                    WriteVerbose(mshArgE.Message);
                    if (Strict)
                         "ComInteropLoaded",
                         ErrorCategory.InvalidArgument, comObject));
                if (comObject != null && Property != null)
                    comObject = LanguagePrimitives.SetObjectProperties(comObject, Property, type, CreateMemberNotFoundError, CreateMemberSetValueError, enableMethodCall: true);
                WriteObject(comObject);
        #region Com
        private object SafeCreateInstance(Type t)
                result = Activator.CreateInstance(t);
            // Does not catch InvalidComObjectException because ComObject is obtained from GetTypeFromProgID
                    "CannotNewNonRuntimeType",
                    "CannotNewTypeBuilderTypedReferenceArgIteratorRuntimeArgumentHandle",
            catch (MethodAccessException e)
                    "CtorAccessDenied",
            catch (MissingMethodException e)
                    "NoPublicCtorMatch",
            catch (MemberAccessException e)
                    "CannotCreateAbstractClass",
            catch (COMException e)
                if (e.HResult == RPC_E_CHANGED_MODE)
                    "NoCOMClassIdentified",
                    ErrorCategory.ResourceUnavailable, null));
        private sealed class ComCreateInfo
            public object objectCreated;
            public bool success;
            public Exception e;
        private ComCreateInfo createInfo;
        private void STAComCreateThreadProc(object createstruct)
            ComCreateInfo info = (ComCreateInfo)createstruct;
                Type type = Type.GetTypeFromCLSID(_comObjectClsId);
                    PSArgumentException mshArgE = PSTraceSource.NewArgumentException(
                        "ComObject",
                        NewObjectStrings.CannotLoadComObjectType,
                        ComObject);
                    info.e = mshArgE;
                    info.success = false;
                info.objectCreated = SafeCreateInstance(type);
                info.success = true;
                info.e = e;
        private object CreateComObject()
                Type type = Marshal.GetTypeFromCLSID(_comObjectClsId);
                            "CannotLoadComObjectType",
                return SafeCreateInstance(type);
                // Check Error Code to see if Error is because of Com apartment Mismatch.
                    createInfo = new ComCreateInfo();
                    Thread thread = new(new ParameterizedThreadStart(STAComCreateThreadProc));
                    thread.Start(createInfo);
                    if (createInfo.success)
                        return createInfo.objectCreated;
                             new ErrorRecord(createInfo.e, "NoCOMClassIdentified",
        #endregion Com
        // HResult code '-2147417850' - Cannot change thread mode after it is set.
        private const int RPC_E_CHANGED_MODE = unchecked((int)0x80010106);
        private const string netSetName = "Net";
    /// Native methods for dealing with COM objects.
    internal static class NewObjectNativeMethods
        /// Return Type: HRESULT->LONG->int
        [DllImport(PinvokeDllNames.CLSIDFromProgIDDllName)]
        internal static extern int CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string lpszProgID, out Guid pclsid);
