    /// A command to Invoke WMI Method.
    [Cmdlet(VerbsLifecycle.Invoke, "WmiMethod", DefaultParameterSetName = "class", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=113346", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public sealed class InvokeWmiMethod : WmiBaseCmdlet
        /// The WMI Object to use.
        [Parameter(ValueFromPipeline = true, Mandatory = true, ParameterSetName = "object")]
        public ManagementObject InputObject
            get { return _inputObject; }
            set { _inputObject = value; }
        /// The WMI Path to use.
        [Parameter(ParameterSetName = "path", Mandatory = true)]
        /// The WMI class to use.
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "class")]
        public string Class
            get { return _className; }
            set { _className = value; }
        /// The WMI Method to execute.
            get { return _methodName; }
            set { _methodName = value; }
        /// The parameters to the method specified by MethodName.
        [Parameter(ParameterSetName = "path")]
        [Parameter(Position = 2, ParameterSetName = "class")]
        [Parameter(ParameterSetName = "object")]
        [Alias("Args")]
        public object[] ArgumentList
            get { return _argumentList; }
            set { _argumentList = value; }
        private string _path = null;
        private string _className = null;
        private string _methodName = null;
        private ManagementObject _inputObject = null;
        private object[] _argumentList = null;
        /// Invoke WMI method given either path,class name or pipeline input.
                RunAsJob("Invoke-WMIMethod");
            if (_inputObject != null)
                ManagementBaseObject inputParameters = null;
                    inputParameters = _inputObject.GetMethodParameters(_methodName);
                    if (_argumentList != null)
                        int inParamCount = _argumentList.Length;
                        foreach (PropertyData property in inputParameters.Properties)
                            if (inParamCount == 0)
                            property.Value = _argumentList[_argumentList.Length - inParamCount];
                            inParamCount--;
                    if (!ShouldProcess(
                       StringUtil.Format(WmiResources.WmiMethodNameForConfirmation,
                       _inputObject["__CLASS"].ToString(),
                       this.Name)
                    result = _inputObject.InvokeMethod(_methodName, inputParameters, null);
                    ErrorRecord errorRecord = new ErrorRecord(e, "InvokeWMIManagementException", ErrorCategory.InvalidOperation, null);
                    ErrorRecord errorRecord = new ErrorRecord(e, "InvokeWMICOMException", ErrorCategory.InvalidOperation, null);
                ManagementPath mPath = null;
                ManagementObject mObject = null;
                if (_path != null)
                    mPath = new ManagementPath(_path);
                    if (string.IsNullOrEmpty(mPath.NamespacePath))
                        mPath.NamespacePath = this.Namespace;
                    else if (namespaceSpecified)
                        // ThrowTerminatingError
                            new InvalidOperationException(),
                            "NamespaceSpecifiedWithPath",
                            ErrorCategory.InvalidOperation,
                            this.Namespace));
                    if (mPath.Server != "." && serverNameSpecified)
                            "ComputerNameSpecifiedWithPath",
                            ComputerName));
                    // If server name is specified loop through it.
                    if (!(mPath.Server == "." && serverNameSpecified))
                        string[] serverName = new string[] { mPath.Server };
                        ComputerName = serverName;
                    result = null;
                            mPath.Server = name;
                            if (mPath.IsClass)
                                ManagementClass mClass = new ManagementClass(mPath);
                                mObject = mClass;
                                ManagementObject mInstance = new ManagementObject(mPath);
                                mObject = mInstance;
                            ManagementScope mScope = new ManagementScope(mPath, options);
                            mObject.Scope = mScope;
                            ManagementClass mClass = new ManagementClass(_className);
                            mObject.Scope = scope;
                        ManagementBaseObject inputParameters = mObject.GetMethodParameters(_methodName);
                                object argument = PSObject.Base(_argumentList[_argumentList.Length - inParamCount]);
                                if (property.IsArray)
                                    property.Value = MakeBaseObjectArray(argument);
                                    property.Value = argument;
                           mObject["__CLASS"].ToString(),
                        result = mObject.InvokeMethod(_methodName, inputParameters, null);
        /// Ensure that the argument is a collection containing no PSObjects.
        /// <param name="argument"></param>
        private static object MakeBaseObjectArray(object argument)
            if (argument == null)
            IList listArgument = argument as IList;
            if (listArgument == null)
                return new object[] { argument };
            bool needCopy = false;
            foreach (object argElement in listArgument)
                if (argElement is PSObject)
                    needCopy = true;
            if (needCopy)
                var copiedArgument = new object[listArgument.Count];
                int index = 0;
                    copiedArgument[index++] = argElement != null ? PSObject.Base(argElement) : null;
                return copiedArgument;
                return argument;
