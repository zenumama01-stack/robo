    /// This class implements Get-Uptime.
    [Cmdlet(VerbsCommon.Get, "Uptime", DefaultParameterSetName = TimespanParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?linkid=834862")]
    [OutputType(typeof(TimeSpan), ParameterSetName = new string[] { TimespanParameterSet })]
    [OutputType(typeof(DateTime), ParameterSetName = new string[] { SinceParameterSet })]
    public class GetUptimeCommand : PSCmdlet
        /// The system startup time.
        [Parameter(ParameterSetName = SinceParameterSet)]
        public SwitchParameter Since { get; set; } = new SwitchParameter();
            // Get-Uptime throw if IsHighResolution = false
            // because stopwatch.GetTimestamp() return DateTime.UtcNow.Ticks
            // instead of ticks from system startup.
            // InternalTestHooks.StopwatchIsNotHighResolution is used as test hook.
            if (Stopwatch.IsHighResolution && !InternalTestHooks.StopwatchIsNotHighResolution)
                TimeSpan uptime = TimeSpan.FromSeconds(Stopwatch.GetTimestamp() / Stopwatch.Frequency);
                if (Since)
                    // Output the time of the last system boot.
                    WriteObject(DateTime.Now.Subtract(uptime));
                    // Output the time elapsed since the last system boot.
                    WriteObject(uptime);
                WriteDebug("System.Diagnostics.Stopwatch.IsHighResolution returns 'False'.");
                Exception exc = new NotSupportedException(GetUptimeStrings.GetUptimePlatformIsNotSupported);
                ThrowTerminatingError(new ErrorRecord(exc, "GetUptimePlatformIsNotSupported", ErrorCategory.NotImplemented, null));
        /// Parameter set name for Timespan OutputType.
        private const string TimespanParameterSet = "Timespan";
        /// Parameter set name for DateTime OutputType.
        private const string SinceParameterSet = "Since";
