    /// ObjectModelWrapper integrates OM-specific operations into generic cmdletization framework.
    /// For example - CimCmdletAdapter knows how to invoke a static method "Foo" in the CIM OM.
    public abstract class CmdletAdapter<TObjectInstance>
        internal void Initialize(PSCmdlet cmdlet, string className, string classVersion, IDictionary<string, string> privateData)
            ArgumentException.ThrowIfNullOrEmpty(className);
            // possible and ok to have classVersion==string.Empty
            ArgumentNullException.ThrowIfNull(classVersion);
            ArgumentNullException.ThrowIfNull(privateData);
            _className = className;
            _classVersion = classVersion;
            if (this.Cmdlet is PSScriptCmdlet compiledScript)
                compiledScript.StoppingEvent += delegate { this.StopProcessing(); };
                compiledScript.DisposingEvent +=
                            var disposable = this as IDisposable;
                            disposable?.Dispose();
        /// Class constructor.
        /// <param name="classVersion"></param>
        /// <param name="privateData"></param>
        public void Initialize(PSCmdlet cmdlet, string className, string classVersion, Version moduleVersion, IDictionary<string, string> privateData)
            _moduleVersion = moduleVersion;
            Initialize(cmdlet, className, classVersion, privateData);
        /// When overridden in the derived class, creates a query builder for a given object model.
        /// <returns>Query builder for a given object model.</returns>
        public virtual QueryBuilder GetQueryBuilder()
        public virtual void ProcessRecord(QueryBuilder query)
        /// When overridden in the derived class, performs initialization of cmdlet execution.
        /// Default implementation in the base class just returns.
        public virtual void BeginProcessing()
        /// When overridden in the derived class, performs cleanup after cmdlet execution.
        public virtual void EndProcessing()
        /// When overridden in the derived class, interrupts currently
        /// running code within the <see cref="CmdletAdapter&lt;TObjectInstance&gt;"/>.
        /// The PowerShell engine will call this method on a separate thread
        /// from the pipeline thread where BeginProcessing, EndProcessing
        /// and other methods are normally being executed.
        public virtual void StopProcessing()
        public virtual void ProcessRecord(TObjectInstance objectInstance, MethodInvocationInfo methodInvocationInfo, bool passThru)
        /// Combines <see cref="ProcessRecord(QueryBuilder)"/> and <see cref="ProcessRecord(TObjectInstance,Microsoft.PowerShell.Cmdletization.MethodInvocationInfo,bool)"/>.
        public virtual void ProcessRecord(QueryBuilder query, MethodInvocationInfo methodInvocationInfo, bool passThru)
        public virtual void ProcessRecord(
            MethodInvocationInfo methodInvocationInfo)
        /// Cmdlet that this ObjectModelWrapper is associated with.
        public PSCmdlet Cmdlet
                return _cmdlet;
        private PSCmdlet _cmdlet;
        /// Name of the class (from the object model handled by this ObjectModelWrapper) that is wrapped by the currently executing cmdlet.
                return _className;
        private string _className;
        /// This value can be <see langword="null"/> (i.e. when ClassVersion attribute is omitted in the ps1xml)
        public string ClassVersion
                return _classVersion;
        private string _classVersion;
        /// Module version.
        public Version ModuleVersion
                return _moduleVersion;
        private Version _moduleVersion;
        /// Private data from Cmdlet Definition XML (from &lt;ObjectModelWrapperPrivateData&gt; element)
        public IDictionary<string, string> PrivateData
                return _privateData;
        private IDictionary<string, string> _privateData;
