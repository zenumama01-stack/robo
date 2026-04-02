    /// A command to Set WMI Instance.
    [Cmdlet(VerbsCommon.Set, "WmiInstance", DefaultParameterSetName = "class", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=113402", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public sealed class SetWmiInstance : WmiBaseCmdlet
        public ManagementObject InputObject { get; set; } = null;
        public string Path { get; set; } = null;
        /// The property name /value pair.
        [Alias("Args", "Property")]
        public Hashtable Arguments { get; set; } = null;
        /// The Flag to use.
        public PutType PutType
            get { return _putType; }
            set { _putType = value; flagSpecified = true; }
        internal bool flagSpecified = false;
        private PutType _putType = PutType.None;
        /// Create or modify WMI Instance given either path,class name or pipeline input.
                RunAsJob("Set-WMIInstance");
                ManagementObject mObj = null;
                    PutOptions pOptions = new PutOptions();
                    mObj = SetWmiInstanceGetPipelineObject();
                    pOptions.Type = _putType;
                    if (mObj != null)
                        if (!ShouldProcess(mObj.Path.Path.ToString()))
                        mObj.Put(pOptions);
                        InvalidOperationException exp = new InvalidOperationException();
                        throw exp;
                    result = mObj;
                    ErrorRecord errorRecord = new ErrorRecord(e, "SetWMIManagementException", ErrorCategory.InvalidOperation, null);
                    ErrorRecord errorRecord = new ErrorRecord(e, "SetWMICOMException", ErrorCategory.InvalidOperation, null);
                // If Class is specified only CreateOnly flag is supported
                mPath = this.SetWmiInstanceBuildManagementPath();
                if (mPath != null)
                        mObject = this.SetWmiInstanceGetObject(mPath, name);
                        if (mObject != null)
                            if (!ShouldProcess(mObject.Path.Path.ToString()))
                            mObject.Put(pOptions);
                        result = mObject;
