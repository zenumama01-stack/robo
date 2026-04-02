namespace System.Management.Automation.Remoting.Internal
    /// PSStreamObjectType is for internal (PowerShell) consumption and should not be treated as a public API.
    public enum PSStreamObjectType
        MethodExecutor = 3,
        BlockingError = 5,
        ShouldMethod = 6,
        WarningRecord = 7,
        Debug = 8,
        Progress = 9,
        Information = 11,
        Exception = 12,
    /// Struct which describes whether an object written
    /// to an ObjectStream is of type - output, error,
    /// verbose, debug.
    /// PSStreamObject is for internal (PowerShell) consumption
    /// and should not be treated as a public API.
    public class PSStreamObject
        public PSStreamObjectType ObjectType { get; set; }
        internal object Value { get; set; }
        internal Guid Id { get; set; }
        internal PSStreamObject(PSStreamObjectType objectType, object value, Guid id)
            ObjectType = objectType;
        /// <param name="objectType"></param>
        public PSStreamObject(PSStreamObjectType objectType, object value)
            : this(objectType, value, Guid.Empty)
        /// <param name="cmdlet">Cmdlet to use for outputting the object.</param>
        /// <param name="overrideInquire">Used by Receive-Job to suppress inquire preference.</param>
        public void WriteStreamObject(Cmdlet cmdlet, bool overrideInquire = false)
                switch (this.ObjectType)
                    case PSStreamObjectType.Output:
                            cmdlet.WriteObject(this.Value);
                    case PSStreamObjectType.Error:
                            ErrorRecord errorRecord = (ErrorRecord)this.Value;
                            MshCommandRuntime mshCommandRuntime = cmdlet.CommandRuntime as MshCommandRuntime;
                            mshCommandRuntime?.WriteError(errorRecord, overrideInquire);
                    case PSStreamObjectType.Debug:
                            string debug = (string)Value;
                            DebugRecord debugRecord = new DebugRecord(debug);
                            mshCommandRuntime?.WriteDebug(debugRecord, overrideInquire);
                    case PSStreamObjectType.Warning:
                            string warning = (string)Value;
                            WarningRecord warningRecord = new WarningRecord(warning);
                            mshCommandRuntime?.WriteWarning(warningRecord, overrideInquire);
                    case PSStreamObjectType.Verbose:
                            string verbose = (string)Value;
                            VerboseRecord verboseRecord = new VerboseRecord(verbose);
                            mshCommandRuntime?.WriteVerbose(verboseRecord, overrideInquire);
                    case PSStreamObjectType.Progress:
                            mshCommandRuntime?.WriteProgress((ProgressRecord)Value, overrideInquire);
                    case PSStreamObjectType.Information:
                            mshCommandRuntime?.WriteInformation((InformationRecord)Value, overrideInquire);
                    case PSStreamObjectType.WarningRecord:
                            WarningRecord warningRecord = (WarningRecord)Value;
                            mshCommandRuntime?.AppendWarningVarList(warningRecord);
                    case PSStreamObjectType.MethodExecutor:
                            Dbg.Assert(this.Value is ClientMethodExecutor,
                                       "Expected psstreamObject.value is ClientMethodExecutor");
                            ClientMethodExecutor methodExecutor = (ClientMethodExecutor)Value;
                            methodExecutor.Execute(cmdlet);
                    case PSStreamObjectType.BlockingError:
                            CmdletMethodInvoker<object> methodInvoker = (CmdletMethodInvoker<object>)Value;
                            InvokeCmdletMethodAndWaitForResults(methodInvoker, cmdlet);
                    case PSStreamObjectType.ShouldMethod:
                            CmdletMethodInvoker<bool> methodInvoker = (CmdletMethodInvoker<bool>)Value;
                    case PSStreamObjectType.Exception:
                            Exception e = (Exception)Value;
            else if (ObjectType == PSStreamObjectType.Exception)
        private static void GetIdentifierInfo(string message, out Guid jobInstanceId, out string computerName)
            jobInstanceId = Guid.Empty;
            computerName = string.Empty;
            string[] parts = message.Split(':', 3);
            if (parts.Length != 3)
            if (!Guid.TryParse(parts[0], out jobInstanceId))
            computerName = parts[1];
        /// <param name="overrideInquire">Suppresses prompt on messages with Inquire preference.
        /// Needed for Receive-Job</param>
        internal void WriteStreamObject(Cmdlet cmdlet, Guid instanceId, bool overrideInquire = false)
            switch (ObjectType)
                        if (instanceId != Guid.Empty)
                            PSObject o = Value as PSObject;
                                AddSourceJobNoteProperty(o, instanceId);
                        cmdlet.WriteObject(Value);
                        RemotingErrorRecord remoteErrorRecord = errorRecord as RemotingErrorRecord;
                        if (remoteErrorRecord == null)
                            // if we get a base ErrorRecord object, check if the computerName is
                            // populated in the RecommendedAction field
                            if (errorRecord.ErrorDetails != null && !string.IsNullOrEmpty(errorRecord.ErrorDetails.RecommendedAction))
                                string computerName;
                                Guid jobInstanceId;
                                GetIdentifierInfo(errorRecord.ErrorDetails.RecommendedAction,
                                                  out jobInstanceId, out computerName);
                                errorRecord = new RemotingErrorRecord(errorRecord,
                                                                      new OriginInfo(computerName, Guid.Empty,
                                                                                     jobInstanceId));
                            errorRecord = remoteErrorRecord;
                        ProgressRecord progressRecord = (ProgressRecord)Value;
                        RemotingProgressRecord remotingProgressRecord = progressRecord as RemotingProgressRecord;
                        if (remotingProgressRecord == null)
                            GetIdentifierInfo(progressRecord.CurrentOperation, out jobInstanceId,
                                              out computerName);
                            OriginInfo info = new OriginInfo(computerName, Guid.Empty, jobInstanceId);
                            progressRecord = new RemotingProgressRecord(progressRecord, info);
                            progressRecord = remotingProgressRecord;
                        mshCommandRuntime?.WriteProgress(progressRecord, overrideInquire);
                        InformationRecord informationRecord = (InformationRecord)this.Value;
                        RemotingInformationRecord remoteInformationRecord = informationRecord as RemotingInformationRecord;
                        if (remoteInformationRecord == null)
                            // if we get a base InformationRecord object, check if the computerName is
                            // populated in the Source field
                            if (!string.IsNullOrEmpty(informationRecord.Source))
                                GetIdentifierInfo(informationRecord.Source, out jobInstanceId, out computerName);
                                informationRecord = new RemotingInformationRecord(informationRecord,
                            informationRecord = remoteInformationRecord;
                        mshCommandRuntime?.WriteInformation(informationRecord, overrideInquire);
                        WriteStreamObject(cmdlet, overrideInquire);
        /// <param name="writeSourceIdentifier"></param>
        /// <param name="overrideInquire">Overrides the inquire preference, used in Receive-Job to suppress prompts.</param>
        internal void WriteStreamObject(Cmdlet cmdlet, bool writeSourceIdentifier, bool overrideInquire)
            if (writeSourceIdentifier)
                WriteStreamObject(cmdlet, Id, overrideInquire);
        private static void InvokeCmdletMethodAndWaitForResults<T>(CmdletMethodInvoker<T> cmdletMethodInvoker, Cmdlet cmdlet)
            Dbg.Assert(cmdletMethodInvoker != null, "Caller should verify cmdletMethodInvoker != null");
            cmdletMethodInvoker.MethodResult = default(T);
                T tmpMethodResult = cmdletMethodInvoker.Action(cmdlet);
                lock (cmdletMethodInvoker.SyncObject)
                    cmdletMethodInvoker.MethodResult = tmpMethodResult;
                    cmdletMethodInvoker.ExceptionThrownOnCmdletThread = e;
                cmdletMethodInvoker.Finished?.Set();
        internal static void AddSourceJobNoteProperty(PSObject psObj, Guid instanceId)
            Dbg.Assert(psObj != null, "psObj is null trying to add a note property.");
            if (psObj.Properties[RemotingConstants.SourceJobInstanceId] != null)
                psObj.Properties.Remove(RemotingConstants.SourceJobInstanceId);
            psObj.Properties.Add(new PSNoteProperty(RemotingConstants.SourceJobInstanceId, instanceId));
        internal static string CreateInformationalMessage(Guid instanceId, string message)
            var newMessage = new StringBuilder(instanceId.ToString());
            newMessage.Append(':');
            newMessage.Append(message);
            return newMessage.ToString();
        internal static ErrorRecord AddSourceTagToError(ErrorRecord errorRecord, Guid sourceId)
            errorRecord.ErrorDetails ??= new ErrorDetails(string.Empty);
            errorRecord.ErrorDetails.RecommendedAction = CreateInformationalMessage(sourceId, errorRecord.ErrorDetails.RecommendedAction);
    public class CmdletMethodInvoker<T>
        public Func<Cmdlet, T> Action { get; set; }
        public Exception ExceptionThrownOnCmdletThread { get; set; }
        public ManualResetEventSlim Finished { get; set; }
        public object SyncObject { get; set; }
        public T MethodResult { get; set; }
