    /// Base class for items in the PSInformationalBuffers.
    /// A PSInformationalRecord consists of a string Message and the InvocationInfo and pipeline state corresponding
    /// to the command that created the record.
    public abstract class InformationalRecord
        /// This class can be instantiated only by its derived classes
        internal InformationalRecord(string message)
            _pipelineIterationInfo = null;
            _serializeExtendedInfo = false;
        /// Creates an InformationalRecord object from a record serialized as a PSObject by ToPSObjectForRemoting.
        internal InformationalRecord(PSObject serializedObject)
            _message = (string)SerializationUtilities.GetPropertyValue(serializedObject, "InformationalRecord_Message");
            _serializeExtendedInfo = (bool)SerializationUtilities.GetPropertyValue(serializedObject, "InformationalRecord_SerializeInvocationInfo");
                _invocationInfo = new InvocationInfo(serializedObject);
                ArrayList pipelineIterationInfo = (ArrayList)SerializationUtilities.GetPsObjectPropertyBaseObject(serializedObject, "InformationalRecord_PipelineIterationInfo");
                _pipelineIterationInfo = new ReadOnlyCollection<int>((int[])pipelineIterationInfo.ToArray(typeof(int)));
        /// The message written by the command that created this record.
        /// The InvocationInfo of the command that created this record.
        /// The InvocationInfo can be null if the record was not created by a command.
        public InvocationInfo InvocationInfo
        /// The PipelineIterationInfo can be null if the record was not created by a command.
        public ReadOnlyCollection<int> PipelineIterationInfo
                return _pipelineIterationInfo;
        /// Sets the InvocationInfo (and PipelineIterationInfo) for this record.
            // Copy a snapshot of the PipelineIterationInfo from the InvocationInfo to this InformationalRecord
            if (invocationInfo.PipelineIterationInfo != null)
        /// Whether to serialize the InvocationInfo and PipelineIterationInfo during remote calls.
                return _serializeExtendedInfo;
                _serializeExtendedInfo = value;
        /// Returns the record's message.
            return this.Message;
        internal virtual void ToPSObjectForRemoting(PSObject psObject)
            RemotingEncoder.AddNoteProperty<string>(psObject, "InformationalRecord_Message", () => this.Message);
            // The invocation info may be null if the record was created via WriteVerbose/Warning/DebugLine instead of WriteVerbose/Warning/Debug, in that case
            // we set InformationalRecord_SerializeInvocationInfo to false.
            if (!this.SerializeExtendedInfo || _invocationInfo == null)
                RemotingEncoder.AddNoteProperty(psObject, "InformationalRecord_SerializeInvocationInfo", () => false);
                RemotingEncoder.AddNoteProperty(psObject, "InformationalRecord_SerializeInvocationInfo", () => true);
                _invocationInfo.ToPSObjectForRemoting(psObject);
                RemotingEncoder.AddNoteProperty<object>(psObject, "InformationalRecord_PipelineIterationInfo", () => this.PipelineIterationInfo);
        private InvocationInfo _invocationInfo;
        private ReadOnlyCollection<int> _pipelineIterationInfo;
        private bool _serializeExtendedInfo;
    /// A warning record in the PSInformationalBuffers.
    public class WarningRecord : InformationalRecord
        public WarningRecord(string message)
        public WarningRecord(PSObject record)
            : base(record)
        /// Constructor for Fully qualified warning Id.
        /// <param name="fullyQualifiedWarningId">Fully qualified warning Id.</param>
        /// <param name="message">Warning message.</param>
        public WarningRecord(string fullyQualifiedWarningId, string message)
            _fullyQualifiedWarningId = fullyQualifiedWarningId;
        /// <param name="record">Warning serialized object.</param>
        public WarningRecord(string fullyQualifiedWarningId, PSObject record)
        /// String which uniquely identifies this warning condition.
        public string FullyQualifiedWarningId
                return _fullyQualifiedWarningId ?? string.Empty;
        private readonly string _fullyQualifiedWarningId;
    /// A debug record in the PSInformationalBuffers.
    public class DebugRecord : InformationalRecord
        public DebugRecord(string message)
        public DebugRecord(PSObject record)
    /// A verbose record in the PSInformationalBuffers.
    public class VerboseRecord : InformationalRecord
        public VerboseRecord(string message)
        public VerboseRecord(PSObject record)
