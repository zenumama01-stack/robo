    public abstract class Cmdlet : InternalCommand
        /// Lists the common parameters that are added by the PowerShell engine to any cmdlet that derives
        /// from PSCmdlet.
        public static HashSet<string> CommonParameters
                return s_commonParameters.Value;
        private static readonly Lazy<HashSet<string>> s_commonParameters = new Lazy<HashSet<string>>(
                    "Verbose", "Debug", "ErrorAction", "WarningAction", "InformationAction", "ProgressAction",
                    "ErrorVariable", "WarningVariable", "OutVariable",
                    "OutBuffer", "PipelineVariable", "InformationVariable" };
        /// Lists the common parameters that are added by the PowerShell engine when a cmdlet defines
        /// additional capabilities (SupportsShouldProcess, SupportsTransactions)
        public static HashSet<string> OptionalCommonParameters
                return s_optionalCommonParameters.Value;
        private static readonly Lazy<HashSet<string>> s_optionalCommonParameters = new Lazy<HashSet<string>>(
                    "WhatIf", "Confirm", "UseTransaction" };
        /// Is this command stopping?
        /// If Stopping is true, many Cmdlet methods will throw
        /// In general, if a Cmdlet's override implementation of ProcessRecord etc.
        /// throws <see cref="System.Management.Automation.PipelineStoppedException"/>, the best thing to do is to
        /// shut down the operation and return to the caller.
        /// It is acceptable to not catch <see cref="System.Management.Automation.PipelineStoppedException"/>
        /// and allow the exception to reach ProcessRecord.
        public bool Stopping
                    return this.IsStopping;
        public CancellationToken PipelineStopToken => StopToken;
        internal string _ParameterSetName
            get { return _parameterSetName; }
        /// Sets the parameter set.
        /// The name of the valid parameter set.
        internal void SetParameterSetName(string parameterSetName)
            _parameterSetName = parameterSetName;
        private string _parameterSetName = string.Empty;
        #region Override Internal
        /// <exception cref="Exception">
        /// This method is overridden in the implementation of
        /// individual cmdlets, and can throw literally any exception.
        internal override void DoBeginProcessing()
            MshCommandRuntime mshRuntime = this.CommandRuntime as MshCommandRuntime;
            if (mshRuntime != null)
                if (mshRuntime.UseTransaction &&
                   (!this.Context.TransactionManager.HasTransaction))
                    string error = TransactionStrings.NoTransactionStarted;
                    if (this.Context.TransactionManager.IsLastTransactionCommitted)
                        error = TransactionStrings.NoTransactionStartedFromCommit;
                    else if (this.Context.TransactionManager.IsLastTransactionRolledBack)
                        error = TransactionStrings.NoTransactionStartedFromRollback;
        internal override void DoProcessRecord()
            this.ProcessRecord();
        internal override void DoEndProcessing()
        #endregion Override Internal
        protected Cmdlet()
        #region Cmdlet virtuals
        /// Gets the resource string corresponding to
        /// baseName and resourceId from the current assembly.
        /// You should override this if you require a different behavior.
        /// <returns>The resource string corresponding to baseName and resourceId.</returns>
        /// Invalid <paramref name="baseName"/> or <paramref name="resourceId"/>, or
        /// string not found in resources
        /// This behavior may be used when the Cmdlet specifies
        /// HelpMessageBaseName and HelpMessageResourceId when defining
        /// <see cref="System.Management.Automation.ParameterAttribute"/>,
        /// or when it uses the
        /// constructor variants which take baseName and resourceId.
        /// <seealso cref="System.Management.Automation.ParameterAttribute"/>
        /// <seealso cref="System.Management.Automation.ErrorDetails"/>
        public virtual string GetResourceString(string baseName, string resourceId)
                ResourceManager manager = ResourceManagerCache.GetResourceManager(this.GetType().Assembly, baseName);
                string retValue = null;
                    retValue = manager.GetString(resourceId, CultureInfo.CurrentUICulture);
                    throw PSTraceSource.NewArgumentException(nameof(baseName), GetErrorText.ResourceBaseNameFailure, baseName);
                if (retValue == null)
                    throw PSTraceSource.NewArgumentException(nameof(resourceId), GetErrorText.ResourceIdFailure, resourceId);
        #endregion Cmdlet virtuals
        /// Holds the command runtime object for this command. This object controls
        /// what actually happens when a write is called.
        public ICommandRuntime CommandRuntime
                    return commandRuntime;
                    commandRuntime = value;
                if (commandRuntime != null)
                    commandRuntime.WriteError(errorRecord);
                    throw new System.NotImplementedException("WriteError");
                    commandRuntime.WriteObject(sendToPipeline);
                    throw new System.NotImplementedException("WriteObject");
                    commandRuntime.WriteObject(sendToPipeline, enumerateCollection);
        /// WriteVerbose may only be called during a call to this Cmdlets's
                    commandRuntime.WriteVerbose(text);
                    throw new System.NotImplementedException("WriteVerbose");
            => commandRuntime is not MshCommandRuntime mshRuntime || mshRuntime.IsWriteVerboseEnabled();
                    commandRuntime.WriteWarning(text);
                    throw new System.NotImplementedException("WriteWarning");
            => commandRuntime is not MshCommandRuntime mshRuntime || mshRuntime.IsWriteWarningEnabled();
                    commandRuntime.WriteCommandDetail(text);
                    throw new System.NotImplementedException("WriteCommandDetail");
                    commandRuntime.WriteProgress(progressRecord);
                    throw new System.NotImplementedException("WriteProgress");
                commandRuntime.WriteProgress(sourceId, progressRecord);
            => commandRuntime is not MshCommandRuntime mshRuntime || mshRuntime.IsWriteProgressEnabled();
                    commandRuntime.WriteDebug(text);
                    throw new System.NotImplementedException("WriteDebug");
            => commandRuntime is not MshCommandRuntime mshRuntime || mshRuntime.IsWriteDebugEnabled();
        /// Route information to the user or host.
        /// <param name="messageData">The object / message data to transmit to the hosting application.</param>
        /// <param name="tags">
        /// Any tags to be associated with the message data. These can later be used to filter
        /// or separate objects being sent to the host.
        /// WriteInformation may only be called during a call to this Cmdlet's
        /// Use WriteInformation to transmit information to the user about the activity
        /// of your Cmdlet.  By default, informational output will
        /// InformationPreference shell variable or the -InformationPreference command-line option.
        public void WriteInformation(object messageData, string[] tags)
                ICommandRuntime2 commandRuntime2 = commandRuntime as ICommandRuntime2;
                if (commandRuntime2 != null)
                    string source = this.MyInvocation.PSCommandPath;
                        source = this.MyInvocation.MyCommand.Name;
                    InformationRecord informationRecord = new InformationRecord(messageData, source);
                    if (tags != null)
                        informationRecord.Tags.AddRange(tags);
                    commandRuntime2.WriteInformation(informationRecord);
                    throw new System.NotImplementedException("WriteInformation");
        /// <param name="informationRecord">The information record to write.</param>
            => commandRuntime is not MshCommandRuntime mshRuntime || mshRuntime.IsWriteInformationEnabled();
        ///             public class RemoveMyObjectType1 : Cmdlet
                    return commandRuntime.ShouldProcess(target);
        ///             public class RemoveMyObjectType2 : Cmdlet
                    return commandRuntime.ShouldProcess(target, action);
        ///             public class RemoveMyObjectType3 : Cmdlet
                    return commandRuntime.ShouldProcess(verboseDescription, verboseWarning, caption);
                    return commandRuntime.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
        ///             public class RemoveMyObjectType4 : Cmdlet
                    return commandRuntime.ShouldContinue(query, caption);
        ///             public class RemoveMyObjectType5 : Cmdlet
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
                    return commandRuntime.ShouldContinue(query, caption, ref yesToAll, ref noToAll);
                    ICommandRuntime2 runtime2 = commandRuntime as ICommandRuntime2;
                    if (runtime2 != null)
                        return runtime2.ShouldContinue(query, caption, hasSecurityImpact, ref yesToAll, ref noToAll);
        /// Run the cmdlet and get the results as a collection. This is an internal
        /// routine that is used by Invoke to build the underlying collection of
        /// results.
        /// <returns>Returns an list of results.</returns>
        internal List<object> GetResults()
            // Prevent invocation of things that derive from PSCmdlet.
            if (this is PSCmdlet)
                string msg = CommandBaseStrings.CannotInvokePSCmdletsDirectly;
                throw new System.InvalidOperationException(msg);
            var result = new List<object>();
            if (this.commandRuntime == null)
                this.CommandRuntime = new DefaultCommandRuntime(result);
        /// Invoke this cmdlet object returning a collection of results.
        /// <returns>The results that were produced by this class.</returns>
        public IEnumerable Invoke()
                List<object> data = this.GetResults();
                for (int i = 0; i < data.Count; i++)
                    yield return data[i];
        /// Returns a strongly-typed enumerator for the results of this cmdlet.
        /// <typeparam name="T">The type returned by the enumerator</typeparam>
        /// <returns>An instance of the appropriate enumerator.</returns>
        /// <exception cref="InvalidCastException">Thrown when the object returned by the cmdlet cannot be converted to the target type.</exception>
        public IEnumerable<T> Invoke<T>()
                    yield return (T)data[i];
                    return commandRuntime.TransactionAvailable();
                    throw new System.NotImplementedException("TransactionAvailable");
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
                    return commandRuntime.CurrentPSTransaction;
                    throw new System.NotImplementedException("CurrentPSTransaction");
        /// Terminate the command and report an error.
                    commandRuntime.ThrowTerminatingError(errorRecord);
                else if (errorRecord.Exception != null)
        #region Exposed API Override
        /// individual Cmdlets, and can throw literally any exception.
        protected virtual void BeginProcessing()
        protected virtual void ProcessRecord()
        protected virtual void EndProcessing()
        protected virtual void StopProcessing()
        #endregion Exposed API Override
    /// This describes the reason why ShouldProcess returned what it returned.
    /// Not all possible reasons are covered.
    public enum ShouldProcessReason
        /// <summary> none of the reasons below </summary>
        /// WhatIf behavior was requested.
        /// In the host, WhatIf behavior can be requested explicitly
        /// for one cmdlet instance using the -WhatIf commandline parameter,
        /// or implicitly for all SupportsShouldProcess cmdlets with $WhatIfPreference.
        /// Other hosts may have other ways to request WhatIf behavior.
        WhatIf = 0x1,
