    /// Suspend shell, script, or runspace activity for the specified period of time.
    [Cmdlet(VerbsLifecycle.Start, "Sleep", DefaultParameterSetName = "Seconds", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097041")]
    public sealed class StartSleepCommand : PSCmdlet, IDisposable
        /// Allows sleep time to be specified in seconds.
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Seconds", ValueFromPipeline = true,
        [ValidateRange(0.0, (double)(int.MaxValue / 1000))]
        public double Seconds { get; set; }
        /// Allows sleep time to be specified in milliseconds.
        [Parameter(Mandatory = true, ParameterSetName = "Milliseconds", ValueFromPipelineByPropertyName = true)]
        [Alias("ms")]
        /// Allows sleep time to be specified as a TimeSpan.
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "FromTimeSpan", ValueFromPipeline = true,
        [ValidateRange(ValidateRangeKind.NonNegative)]
        [Alias("ts")]
        public TimeSpan Duration { get; set; }
        // object used for synchronizes pipeline thread and stop thread
        // access to waitHandle
        private readonly object _syncObject = new();
        // this is set to true by stopProcessing
        /// This method causes calling thread to sleep for specified milliseconds.
        private void Sleep(int milliSecondsToSleep)
                if (!_stopping)
            _waitHandle?.WaitOne(milliSecondsToSleep, true);
            int sleepTime = 0;
                case "Seconds":
                    sleepTime = (int)(Seconds * 1000);
                case "Milliseconds":
                    sleepTime = Milliseconds;
                case "FromTimeSpan":
                    if (Duration.TotalMilliseconds > int.MaxValue)
                        PSArgumentException argumentException = PSTraceSource.NewArgumentException(
                            nameof(Duration),
                            StartSleepStrings.MaximumDurationExceeded,
                            TimeSpan.FromMilliseconds(int.MaxValue),
                            Duration);
                                argumentException,
                                "MaximumDurationExceeded",
                    sleepTime = (int)Math.Floor(Duration.TotalMilliseconds);
            Sleep(sleepTime);
        /// StopProcessing override.
