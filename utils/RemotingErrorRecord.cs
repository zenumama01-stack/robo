    /// Error record in remoting cases.
    public class RemotingErrorRecord : ErrorRecord
        /// Contains the origin information.
        public OriginInfo OriginInfo
                return _originInfo;
        private readonly OriginInfo _originInfo;
        /// <param name="errorRecord">The error record that is wrapped.</param>
        /// <param name="originInfo">Origin information.</param>
        public RemotingErrorRecord(ErrorRecord errorRecord, OriginInfo originInfo)
            : this(errorRecord, originInfo, null) { }
        /// Constructor that is used to wrap an error record.
        /// <param name="originInfo"></param>
        /// <param name="replaceParentContainsErrorRecordException"></param>
        private RemotingErrorRecord(
            ErrorRecord errorRecord,
            OriginInfo originInfo,
            : base(errorRecord, replaceParentContainsErrorRecordException)
                base.SetInvocationInfo(errorRecord.InvocationInfo);
            _originInfo = originInfo;
        #region ISerializable implementation
        /// Deserializer constructor.
        /// <param name="info">Serializer information.</param>
        protected RemotingErrorRecord(SerializationInfo info, StreamingContext context) : base(info, context)
        /// Wrap the current ErrorRecord instance.
        internal override ErrorRecord WrapException(Exception replaceParentContainsErrorRecordException)
            return new RemotingErrorRecord(this, this.OriginInfo, replaceParentContainsErrorRecordException);
    /// Progress record containing origin information.
    public class RemotingProgressRecord : ProgressRecord
            get { return _originInfo; }
        /// <param name="progressRecord">The progress record that is wrapped.</param>
        public RemotingProgressRecord(ProgressRecord progressRecord, OriginInfo originInfo)
                  Validate(progressRecord).ActivityId,
                  Validate(progressRecord).Activity,
                  Validate(progressRecord).StatusDescription)
            if (progressRecord != null)
                this.PercentComplete = progressRecord.PercentComplete;
                this.ParentActivityId = progressRecord.ParentActivityId;
                this.RecordType = progressRecord.RecordType;
                this.SecondsRemaining = progressRecord.SecondsRemaining;
                if (!string.IsNullOrEmpty(progressRecord.CurrentOperation))
                    this.CurrentOperation = progressRecord.CurrentOperation;
        private static ProgressRecord Validate(ProgressRecord progressRecord)
            ArgumentNullException.ThrowIfNull(progressRecord);
            return progressRecord;
    /// Warning record containing origin information.
    public class RemotingWarningRecord : WarningRecord
        /// <param name="message">The warning message that is wrapped.</param>
        /// <param name="originInfo">The origin information.</param>
        public RemotingWarningRecord(string message, OriginInfo originInfo)
        /// Constructor taking WarningRecord to wrap and OriginInfo.
        /// <param name="warningRecord">WarningRecord to wrap.</param>
        /// <param name="originInfo">OriginInfo.</param>
        internal RemotingWarningRecord(
            WarningRecord warningRecord,
            OriginInfo originInfo)
            : base(warningRecord.FullyQualifiedWarningId, warningRecord.Message)
    /// Debug record containing origin information.
    public class RemotingDebugRecord : DebugRecord
        /// <param name="message">The debug message that is wrapped.</param>
        public RemotingDebugRecord(string message, OriginInfo originInfo)
    /// Verbose record containing origin information.
    public class RemotingVerboseRecord : VerboseRecord
        /// <param name="message">The verbose message that is wrapped.</param>
        public RemotingVerboseRecord(string message, OriginInfo originInfo)
    /// Information record containing origin information.
    public class RemotingInformationRecord : InformationRecord
        /// <param name="record">The Information message that is wrapped.</param>
        public RemotingInformationRecord(InformationRecord record, OriginInfo originInfo)
    /// Contains OriginInfo for an error record.
    /// <remarks>This class should only be used when
    /// defining origin information for error records.
    /// In case of output objects, the information
    /// should directly be added to the object as
    /// properties</remarks>
    public class OriginInfo
        /// The HostEntry information for the machine on
        /// which this information originated.
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "PSIP")]
        public string PSComputerName
                return _computerName;
        /// Runspace instance ID.
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        public Guid RunspaceID
                return _runspaceID;
        private readonly Guid _runspaceID;
        /// Error record source instance ID.
        public Guid InstanceID
        /// Public constructor.
        /// <param name="computerName">Machine name.</param>
        /// <param name="runspaceID">Instance id of runspace.</param>
        public OriginInfo(string computerName, Guid runspaceID)
            : this(computerName, runspaceID, Guid.Empty)
        /// <param name="instanceID">Instance id for the origin object.</param>
        public OriginInfo(string computerName, Guid runspaceID, Guid instanceID)
            _runspaceID = runspaceID;
            _instanceId = instanceID;
        /// Overridden ToString() method.
        /// <returns>Returns the computername.</returns>
            return PSComputerName;
