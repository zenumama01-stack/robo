    /// Implementation for the new-timespan command.
    [Cmdlet(VerbsCommon.New, "TimeSpan", DefaultParameterSetName = "Date",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096709", RemotingCapability = RemotingCapability.None)]
    [OutputType(typeof(TimeSpan))]
    public sealed class NewTimeSpanCommand : PSCmdlet
        /// This parameter indicates the date the time span begins;
        /// it is used if two times are being compared.
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Date")]
        public DateTime Start
                return _start;
                _start = value;
                _startSpecified = true;
        private DateTime _start;
        private bool _startSpecified;
        /// This parameter indicates the end of a time span.  It is used if two
        /// times are being compared.  If one of the times is not specified,
        /// the current system time is used.
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = "Date")]
        public DateTime End
                return _end;
                _end = value;
                _endSpecified = true;
        private DateTime _end;
        private bool _endSpecified = false;
        [Parameter(ParameterSetName = "Time")]
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public int Milliseconds { get; set; }
        /// Calculate and write out the appropriate timespan.
            // initially set start and end time to be equal
            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime;
            TimeSpan result;
                case "Date":
                    if (_startSpecified)
                        startTime = Start;
                    if (_endSpecified)
                        endTime = End;
                    result = endTime.Subtract(startTime);
                case "Time":
                    result = new TimeSpan(Days, Hours, Minutes, Seconds, Milliseconds);
                    Dbg.Diagnostics.Assert(false, "Only one of the specified parameter sets should be called.");
