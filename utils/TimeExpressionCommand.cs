    /// Implements a cmdlet that applies a script block
    /// to each element of the pipeline.
    [Cmdlet(VerbsDiagnostic.Measure, "Command", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097029", RemotingCapability = RemotingCapability.None)]
    public sealed class MeasureCommandCommand : PSCmdlet
        /// This parameter specifies the current pipeline object.
        /// The script block to apply.
        public ScriptBlock Expression { get; set; }
        private readonly System.Diagnostics.Stopwatch _stopWatch = new();
        /// Output the timer.
            WriteObject(_stopWatch.Elapsed);
        /// Execute the script block passing in the current pipeline object as it's only parameter.
            // Only accumulate the time used by this scriptblock...
            // As results are discarded, write directly to a null pipe instead of accumulating.
            _stopWatch.Start();
            Expression.InvokeWithPipe(
                dollarUnder: InputObject,   // $_
                input: Array.Empty<object>(), // $input
                outputPipe: new Pipe { NullPipe = true },
                invocationInfo: null);
            _stopWatch.Stop();
