    /// The context of the core command that is being run. This
    /// includes data like the user name and password, as well
    /// as callbacks for streaming output, prompting, and progress.
    /// This allows the providers to be called in a variety of situations.
    /// The most common will be from the core cmdlets themselves but they
    /// can also be called programmatically either by having the results
    /// accumulated or by providing delegates for the various streams.
    /// NOTE:  USER Feedback mechanism are only enabled for the CoreCmdlet
    /// case.  This is because we have not seen a use-case for them in the
    /// other scenarios.
    internal sealed class CmdletProviderContext
        #region Trace object
        /// using "CmdletProviderContext" as the category.
             "CmdletProviderContext",
             "The context under which a core command is being run.")]
            Dbg.PSTraceSource.GetTracer("CmdletProviderContext",
             "The context under which a core command is being run.");
        #endregion Trace object
        /// Constructs the context under which the core command providers
        /// operate.
        /// <param name="executionContext">
        /// The context of the engine.
        /// If <paramref name="executionContext"/> is null.
        internal CmdletProviderContext(ExecutionContext executionContext)
            ExecutionContext = executionContext;
            Origin = CommandOrigin.Internal;
            Drive = executionContext.EngineSessionState.CurrentDrive;
            if ((executionContext.CurrentCommandProcessor != null) &&
                (executionContext.CurrentCommandProcessor.Command is Cmdlet))
                _command = (Cmdlet)executionContext.CurrentCommandProcessor.Command;
        internal CmdletProviderContext(ExecutionContext executionContext, CommandOrigin origin)
            Origin = origin;
        /// The command object that is running.
        /// <param name="credentials">
        /// The credentials the core command provider should use.
        /// The drive under which this context should operate.
        /// If <paramref name="command"/> is null.
        /// If <paramref name="command"/> contains a null Host or Context reference.
        internal CmdletProviderContext(
            PSCmdlet command,
            PSCredential credentials,
            PSDriveInfo drive)
            // verify the command parameter
            Origin = command.CommandOrigin;
                _credentials = credentials;
            Drive = drive;
            if (command.Host == null)
                throw PSTraceSource.NewArgumentException("command.Host");
            if (command.Context == null)
                throw PSTraceSource.NewArgumentException("command.Context");
            ExecutionContext = command.Context;
            // Stream will default to true because command methods will be used.
            PassThru = true;
            _streamErrors = true;
            PSCredential credentials)
        /// operate using an existing context.
        /// <param name="contextToCopyFrom">
        /// A CmdletProviderContext instance to copy the filters, ExecutionContext,
        /// Credentials, Drive, and Force options from.
        /// If <paramref name="contextToCopyFrom"/> is null.
            CmdletProviderContext contextToCopyFrom)
            if (contextToCopyFrom == null)
                throw PSTraceSource.NewArgumentNullException(nameof(contextToCopyFrom));
            ExecutionContext = contextToCopyFrom.ExecutionContext;
            _command = contextToCopyFrom._command;
            if (contextToCopyFrom.Credential != null)
                _credentials = contextToCopyFrom.Credential;
            Drive = contextToCopyFrom.Drive;
            _force = contextToCopyFrom.Force;
            this.CopyFilters(contextToCopyFrom);
            SuppressWildcardExpansion = contextToCopyFrom.SuppressWildcardExpansion;
            DynamicParameters = contextToCopyFrom.DynamicParameters;
            Origin = contextToCopyFrom.Origin;
            // Copy the stopping state incase the source context
            // has already been signaled for stopping
            Stopping = contextToCopyFrom.Stopping;
            // add this context to the stop referral on the copied
            // context
            contextToCopyFrom.StopReferrals.Add(this);
            _copiedContext = contextToCopyFrom;
        /// If the constructor that takes a context to copy is
        /// called, this will be set to the context being copied.
        private readonly CmdletProviderContext _copiedContext;
        /// The credentials under which the operation should run.
        private readonly PSCredential _credentials = PSCredential.Empty;
        /// The force parameter gives guidance to providers on how vigorously they
        /// should try to perform an operation.
        /// The command which defines the context. This should not be
        /// made visible to anyone and should only be set through the
        private readonly Cmdlet _command;
        /// This makes the origin of the provider request visible to the internals.
        internal CommandOrigin Origin { get; } = CommandOrigin.Internal;
        /// This defines the default behavior for the WriteError method.
        /// If it is true, a call to this method will result in an immediate call
        /// to the command WriteError method, or to the writeErrorDelegate if
        /// one has been supplied.
        /// If it is false, the objects will be accumulated until the
        /// GetErrorObjects method is called.
        private readonly bool _streamErrors;
        /// A collection in which objects that are written using the WriteObject(s)
        /// methods are accumulated if <see cref="PassThru"/> is false.
        private Collection<PSObject> _accumulatedObjects = new Collection<PSObject>();
        /// A collection in which objects that are written using the WriteError
        /// method are accumulated if <see cref="PassThru"/> is false.
        private Collection<ErrorRecord> _accumulatedErrorObjects = new Collection<ErrorRecord>();
        /// The instance of the provider that is currently executing in this context.
        private System.Management.Automation.Provider.CmdletProvider _providerInstance;
        #endregion private properties
        /// Gets the execution context of the engine.
        /// Gets or sets the provider instance for the current
        internal System.Management.Automation.Provider.CmdletProvider ProviderInstance
                return _providerInstance;
                _providerInstance = value;
        /// Copies the include, exclude, and provider filters from
        /// the specified context to this context.
        /// The context to copy the filters from.
        private void CopyFilters(CmdletProviderContext context)
            Include = context.Include;
            Exclude = context.Exclude;
            Filter = context.Filter;
        internal void RemoveStopReferral() => _copiedContext?.StopReferrals.Remove(this);
        /// Gets or sets the dynamic parameters for the context.
        internal object DynamicParameters { get; set; }
        /// Returns MyInvocation from the underlying cmdlet.
                    return _command.MyInvocation;
        /// Determines if the Write* calls should be passed through to the command
        /// instance if there is one.  The default value is true.
        internal bool PassThru { get; set; }
        /// The drive associated with this context.
        /// If <paramref name="value"/> is null on set.
        internal PSDriveInfo Drive { get; set; }
        /// Gets the user name under which the operation should run.
        internal PSCredential Credential
                PSCredential result = _credentials;
                // If the username wasn't specified, use the drive credentials
                if (_credentials == null && Drive != null)
                    result = Drive.Credential;
        /// Gets the flag that determines if the command requested a transaction.
        internal bool UseTransaction
                if ((_command != null) && (_command.CommandRuntime != null))
                    MshCommandRuntime mshRuntime = _command.CommandRuntime as MshCommandRuntime;
                        return mshRuntime.UseTransaction;
                return _command.TransactionAvailable();
                    return _command.CurrentPSTransaction;
        /// Gets or sets the Force property that is passed to providers.
        internal SwitchParameter Force
        /// The provider specific filter that should be used when determining
        /// which items an action should take place on.
        internal string Filter { get; set; }
        /// A glob string that signifies which items should be included when determining
        /// which items the action should occur on.
        internal Collection<string> Include { get; private set; }
        /// A glob string that signifies which items should be excluded when determining
        internal Collection<string> Exclude { get; private set; }
        /// Gets or sets the property that tells providers (that
        /// declare their own wildcard support) to suppress wildcard
        /// expansion. This is set when the user specifies the
        /// -LiteralPath parameter to one of the core commands.
        public bool SuppressWildcardExpansion { get; internal set; }
        #region User feedback mechanisms
        /// Confirm the operation with the user.
        /// Name of the target resource being acted upon
        /// triggered a terminating error.  The pipeline failure will be
        /// ActionPreferenceStopException.
        /// Also, this occurs if the pipeline was already stopped.
        internal bool ShouldProcess(
            string target)
                result = _command.ShouldProcess(target);
        /// <param name="action">What action was being performed.</param>
            string target,
            string action)
                result = _command.ShouldProcess(target, action);
        /// This should contain a textual description of the action to be
        /// performed.  This is what will be displayed to the user for
        /// This should contain a textual query of whether the action
        /// should be performed, usually in the form of a question.
                result = _command.ShouldProcess(
                    caption);
        /// Ask the user whether to continue/stop or break to a subshell.
        /// Message to display to the user. This routine will append
        /// the text "Continue" to ensure that people know what question
        /// Dialog caption if the host uses a dialog.
        /// True if the user wants to continue, false if not.
        internal bool ShouldContinue(
                result = _command.ShouldContinue(query, caption);
        /// Indicates whether the user selected YesToAll
        /// Indicates whether the user selected NoToAll
                result = _command.ShouldContinue(
                    query, caption, ref yesToAll, ref noToAll);
                yesToAll = false;
                noToAll = false;
        /// Writes the object to the Verbose pipe.
        /// The string that needs to be written.
        internal void WriteVerbose(string text) => _command?.WriteVerbose(text);
        /// Writes the object to the Warning pipe.
        internal void WriteWarning(string text) => _command?.WriteWarning(text);
        internal void WriteProgress(ProgressRecord record) => _command?.WriteProgress(record);
        /// Writes a debug string.
        /// The String that needs to be written.
        internal void WriteDebug(string text) => _command?.WriteDebug(text);
        internal void WriteInformation(InformationRecord record) => _command?.WriteInformation(record);
        internal void WriteInformation(object messageData, string[] tags) => _command?.WriteInformation(messageData, tags);
        #endregion User feedback mechanisms
        /// Sets the filters that are used within this context.
        /// <param name="include">
        /// The include filters which determines which items are included in
        /// operations within this context.
        /// <param name="exclude">
        /// The exclude filters which determines which items are excluded from
        /// The provider specific filter for the operation.
        internal void SetFilters(Collection<string> include, Collection<string> exclude, string filter)
            Include = include;
            Exclude = exclude;
            Filter = filter;
        /// Gets an array of the objects that have been accumulated
        /// and the clears the collection.
        /// An object array of the objects that have been accumulated
        /// through the WriteObject method.
        internal Collection<PSObject> GetAccumulatedObjects()
            // Get the contents as an array
            Collection<PSObject> results = _accumulatedObjects;
            _accumulatedObjects = new Collection<PSObject>();
            // Return the array
        /// Gets an array of the error objects that have been accumulated
        /// through the WriteError method.
        internal Collection<ErrorRecord> GetAccumulatedErrorObjects()
            Collection<ErrorRecord> results = _accumulatedErrorObjects;
            _accumulatedErrorObjects = new Collection<ErrorRecord>();
        /// If there are any errors accumulated, the first error is thrown.
        /// If a CmdletProvider wrote any exceptions to the error pipeline, it is
        /// wrapped and then thrown.
        internal void ThrowFirstErrorOrDoNothing()
            ThrowFirstErrorOrDoNothing(true);
        /// <param name="wrapExceptionInProviderException">
        /// If true, the error will be wrapped in a ProviderInvocationException before
        /// being thrown. If false, the error will be thrown as is.
        /// If <paramref name="wrapExceptionInProviderException"/> is true, the
        /// first exception that was written to the error pipeline by a CmdletProvider
        /// is wrapped and thrown.
        /// <exception>
        /// If <paramref name="wrapExceptionInProviderException"/> is false,
        /// the first exception that was written to the error pipeline by a CmdletProvider
        /// is thrown.
        internal void ThrowFirstErrorOrDoNothing(bool wrapExceptionInProviderException)
            if (HasErrors())
                Collection<ErrorRecord> errors = GetAccumulatedErrorObjects();
                if (errors != null && errors.Count > 0)
                    // Throw the first exception
                    if (wrapExceptionInProviderException)
                        if (this.ProviderInstance != null)
                            providerInfo = this.ProviderInstance.ProviderInfo;
                                errors[0]);
                            providerInfo != null ? providerInfo.Name : "unknown provider",
                        throw errors[0].Exception;
        /// Writes all the accumulated errors to the specified context using WriteError.
        /// <param name="errorContext">
        /// The context to write the errors to.
        /// If <paramref name="errorContext"/> is null.
        internal void WriteErrorsToContext(CmdletProviderContext errorContext)
                foreach (ErrorRecord errorRecord in GetAccumulatedErrorObjects())
                    errorContext.WriteError(errorRecord);
        /// Writes an object to the output.
        /// The object to be written.
        /// If streaming is on and the writeObjectHandler was specified then the object
        /// gets written to the writeObjectHandler. If streaming is on and the writeObjectHandler
        /// was not specified and the command object was specified, the object gets written to
        /// the WriteObject method of the command object.
        /// If streaming is off the object gets written to an accumulator collection. The collection
        /// of written object can be retrieved using the AccumulatedObjects method.
        /// The CmdletProvider could not stream the results because no
        /// cmdlet was specified to stream the output through.
        /// If the pipeline has been signaled for stopping but
        /// the provider calls this method.
        internal void WriteObject(object obj)
            // Making sure to obey the StopProcessing by
            // throwing an exception anytime a provider tries
            // to WriteObject
                PipelineStoppedException stopPipeline =
                    new PipelineStoppedException();
                throw stopPipeline;
                    s_tracer.WriteLine("Writing to command pipeline");
                    // Since there was no writeObject handler use
                    // the command WriteObject method.
                    _command.WriteObject(obj);
                    // The flag was set for streaming but we have no where
                    // to stream to.
                            SessionStateStrings.OutputStreamingNotEnabled);
                s_tracer.WriteLine("Writing to accumulated objects");
                // Convert the object to a PSObject if it's not already
                // one.
                PSObject newObj = PSObject.AsPSObject(obj);
                // Since we are not streaming, just add the object to the accumulatedObjects
                _accumulatedObjects.Add(newObj);
        /// Writes the error to the pipeline or accumulates the error in an internal
        /// The error record to write to the pipeline or the internal buffer.
        /// The CmdletProvider could not stream the error because no
        internal void WriteError(ErrorRecord errorRecord)
            // to WriteError
            if (_streamErrors)
                    s_tracer.WriteLine("Writing error package to command error pipe");
                    _command.WriteError(errorRecord);
                            SessionStateStrings.ErrorStreamingNotEnabled);
                // Since we are not streaming, just add the object to the accumulatedErrorObjects
                _accumulatedErrorObjects.Add(errorRecord);
                        this.ProviderInstance.ProviderInfo.Name,
        /// If the error pipeline hasn't been supplied a delegate or a command then this method
        /// will determine if any errors have accumulated.
        /// True if the errors are being accumulated and some errors have been accumulated.  False otherwise.
        internal bool HasErrors()
            return _accumulatedErrorObjects != null && _accumulatedErrorObjects.Count > 0;
        /// Call this on a separate thread when a provider is using
        /// this context to do work. This method will call the StopProcessing
        /// method of the provider.
            Stopping = true;
            // We don't need to catch any of the exceptions here because
            // we are terminating the pipeline and any exception will
            // be caught by the engine.
            _providerInstance?.StopProcessing();
            // Call the stop referrals if any
            foreach (CmdletProviderContext referralContext in StopReferrals)
                referralContext.StopProcessing();
        internal bool Stopping { get; private set; }
        /// The list of contexts to which the StopProcessing calls
        /// should be referred.
        internal Collection<CmdletProviderContext> StopReferrals { get; } = new Collection<CmdletProviderContext>();
        internal bool HasIncludeOrExclude
                return ((Include != null && Include.Count > 0) ||
                        (Exclude != null && Exclude.Count > 0));
