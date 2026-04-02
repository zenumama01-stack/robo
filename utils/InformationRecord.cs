    /// Defines a data structure used to represent informational context destined for the host or user.
    /// InformationRecords are passed to <see cref="System.Management.Automation.Cmdlet.WriteInformation(object, string[])"/>,
    /// which, according to host or user preference, forwards that information on to the host for rendering to the user.
    /// <seealso cref="System.Management.Automation.Cmdlet.WriteInformation(object, string[])"/>
    [DataContract]
    public class InformationRecord
        /// Initializes a new instance of the InformationRecord class.
        /// <param name="messageData">The object to be transmitted to the host.</param>
        /// <param name="source">The source of the message (i.e.: script path, function name, etc.).</param>
        public InformationRecord(object messageData, string source)
            this.MessageData = messageData;
            this.Source = source;
            this.TimeGenerated = DateTime.Now;
            this.NativeThreadId = PsUtils.GetNativeThreadId();
            this.ManagedThreadId = (uint)Environment.CurrentManagedThreadId;
        private InformationRecord() { }
        /// Copy constructor.
        internal InformationRecord(InformationRecord baseRecord)
            this.MessageData = baseRecord.MessageData;
            this.Source = baseRecord.Source;
            this.TimeGenerated = baseRecord.TimeGenerated;
            this.Tags = baseRecord.Tags;
            this.User = baseRecord.User;
            this.Computer = baseRecord.Computer;
            this.ProcessId = baseRecord.ProcessId;
            this.NativeThreadId = baseRecord.NativeThreadId;
            this.ManagedThreadId = baseRecord.ManagedThreadId;
        // Some of these setters are internal, while others are public.
        // The ones that are public are left that way because systems that proxy
        // the events may need to alter them (i.e.: workflow). The ones that remain internal
        // are that way because they are fundamental properties of the record itself.
        /// The message data for this informational record.
        [DataMember]
        public object MessageData { get; internal set; }
        /// The source of this informational record (script path, function name, etc.)
        /// The time this informational record was generated.
        public DateTime TimeGenerated { get; set; }
        /// The tags associated with this informational record (if any)
        public List<string> Tags
            get { return _tags ??= new List<string>(); }
            internal set { _tags = value; }
        private List<string> _tags;
        /// The user that generated this informational record.
        public string User
                // domain\user on Windows, just user on Unix
                this._user ??=
                    Environment.UserName;
                    Environment.UserDomainName + "\\" + Environment.UserName;
                return _user;
                _user = value;
        private string _user;
        /// The computer that generated this informational record.
        public string Computer
            get { return this._computerName ??= PsUtils.GetHostName(); }
            set { this._computerName = value; }
        /// The process that generated this informational record.
        public uint ProcessId
                if (!this._processId.HasValue)
                    this._processId = (uint)Environment.ProcessId;
                return this._processId.Value;
                _processId = value;
        private uint? _processId;
        /// The native thread that generated this informational record.
        public uint NativeThreadId { get; set; }
        /// The managed thread that generated this informational record.
        public uint ManagedThreadId { get; set; }
        /// Converts an InformationRecord to a string-based representation.
            if (MessageData != null)
                return MessageData.ToString();
        internal static InformationRecord FromPSObjectForRemoting(PSObject inputObject)
            InformationRecord informationRecord = new InformationRecord();
            informationRecord.MessageData = RemotingDecoder.GetPropertyValue<object>(inputObject, "MessageData");
            informationRecord.Source = RemotingDecoder.GetPropertyValue<string>(inputObject, "Source");
            informationRecord.TimeGenerated = RemotingDecoder.GetPropertyValue<DateTime>(inputObject, "TimeGenerated");
            informationRecord.Tags = new List<string>();
            System.Collections.ArrayList tagsArrayList = RemotingDecoder.GetPropertyValue<System.Collections.ArrayList>(inputObject, "Tags");
            foreach (string tag in tagsArrayList)
                informationRecord.Tags.Add(tag);
            informationRecord.User = RemotingDecoder.GetPropertyValue<string>(inputObject, "User");
            informationRecord.Computer = RemotingDecoder.GetPropertyValue<string>(inputObject, "Computer");
            informationRecord.ProcessId = RemotingDecoder.GetPropertyValue<uint>(inputObject, "ProcessId");
            informationRecord.NativeThreadId = RemotingDecoder.GetPropertyValue<uint>(inputObject, "NativeThreadId");
            informationRecord.ManagedThreadId = RemotingDecoder.GetPropertyValue<uint>(inputObject, "ManagedThreadId");
            return informationRecord;
        /// Returns this object as a PSObject property bag
        /// that can be used in a remoting protocol data object.
        /// <returns>This object as a PSObject property bag.</returns>
        internal PSObject ToPSObjectForRemoting()
            PSObject informationAsPSObject = RemotingEncoder.CreateEmptyPSObject();
            informationAsPSObject.Properties.Add(new PSNoteProperty("MessageData", this.MessageData));
            informationAsPSObject.Properties.Add(new PSNoteProperty("Source", this.Source));
            informationAsPSObject.Properties.Add(new PSNoteProperty("TimeGenerated", this.TimeGenerated));
            informationAsPSObject.Properties.Add(new PSNoteProperty("Tags", this.Tags));
            informationAsPSObject.Properties.Add(new PSNoteProperty("User", this.User));
            informationAsPSObject.Properties.Add(new PSNoteProperty("Computer", this.Computer));
            informationAsPSObject.Properties.Add(new PSNoteProperty("ProcessId", this.ProcessId));
            informationAsPSObject.Properties.Add(new PSNoteProperty("NativeThreadId", this.NativeThreadId));
            informationAsPSObject.Properties.Add(new PSNoteProperty("ManagedThreadId", this.ManagedThreadId));
            return informationAsPSObject;
    /// Class that holds informational messages to represent output created by the
    /// Write-Host cmdlet.
    public class HostInformationMessage
        /// The message being output by the host.
        /// 'True' if the host should not append a NewLine to the message output.
        public bool? NoNewLine { get; set; }
        /// The foreground color of the message.
        public ConsoleColor? ForegroundColor { get; set; }
        /// The background color of the message.
        public ConsoleColor? BackgroundColor { get; set; }
        /// Returns a string-based representation of the host information message.
