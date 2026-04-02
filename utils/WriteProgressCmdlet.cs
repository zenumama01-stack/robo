    /// Implements the write-progress cmdlet.
    [Cmdlet(VerbsCommunications.Write, "Progress", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097036", RemotingCapability = RemotingCapability.None)]
    public sealed class WriteProgressCommand : PSCmdlet
        /// Describes the activity for which progress is being reported.
            HelpMessageBaseName = HelpMessageBaseName,
            HelpMessageResourceId = "ActivityParameterHelpMessage")]
        public string Activity { get; set; }
        /// Describes the current state of the activity.
            HelpMessageResourceId = "StatusParameterHelpMessage")]
        public string Status { get; set; } = WriteProgressResourceStrings.Processing;
        /// Uniquely identifies this activity for purposes of chaining subordinate activities.
        /// Percentage completion of the activity, or -1 if n/a.
        [ValidateRange(-1, 100)]
        public int PercentComplete { get; set; } = -1;
        /// Seconds remaining to complete the operation, or -1 if n/a.
        public int SecondsRemaining { get; set; } = -1;
        /// Description of current operation in activity, empty if n/a.
        public string CurrentOperation { get; set; }
        /// Identifies the parent Id of this activity, or -1 if none.
        public int ParentId { get; set; } = -1;
        /// Identifies whether the activity has completed (and the display for it should be removed),
        /// or if it is proceeding (and the display for it should be shown).
        public SwitchParameter Completed
                return _completed;
                _completed = value;
        /// Identifies the source of the record.
        public int SourceId { get; set; }
        /// Writes a ProgressRecord created from the parameters.
        ProcessRecord()
            ProgressRecord pr;
            if (string.IsNullOrEmpty(Activity))
                if (!Completed)
                    new ArgumentException("Missing value for mandatory parameter.", nameof(Activity)),
                    "MissingActivity",
                    Activity));
                    pr = new(Id);
                    pr.StatusDescription = Status;
                pr = new(Id, Activity, Status);
            pr.ParentActivityId = ParentId;
            pr.PercentComplete = PercentComplete;
            pr.SecondsRemaining = SecondsRemaining;
            pr.CurrentOperation = CurrentOperation;
            pr.RecordType = this.Completed ? ProgressRecordType.Completed : ProgressRecordType.Processing;
            WriteProgress(SourceId, pr);
        private bool _completed;
        private const string HelpMessageBaseName = "WriteProgressResourceStrings";
