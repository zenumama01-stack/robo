    /// Default implementation of ICommandRuntime for running Cmdlets standalone.
    internal class DefaultCommandRuntime : ICommandRuntime2
        private readonly List<object> _output;
        /// Constructs an instance of the default ICommandRuntime object
        /// that will write objects into the list that was passed.
        public DefaultCommandRuntime(List<object> outputList)
            ArgumentNullException.ThrowIfNull(outputList);
            _output = outputList;
        /// Return the instance of PSHost - null by default.
        public PSHost Host { get; set; }
        #region Write
        /// Implementation of WriteDebug - just discards the input.
        /// <param name="text">Text to write.</param>
        public void WriteDebug(string text) { }
        /// Default implementation of WriteError - if the error record contains
        /// an exception then that exception will be thrown. If not, then an
        /// InvalidOperationException will be constructed and thrown.
        /// <param name="errorRecord">Error record instance to process.</param>
        public void WriteError(ErrorRecord errorRecord)
            if (errorRecord.Exception != null)
                throw errorRecord.Exception;
                throw new InvalidOperationException(errorRecord.ToString());
        /// Default implementation of WriteObject - adds the object to the list
        /// passed to the objects constructor.
        /// <param name="sendToPipeline">Object to write.</param>
        public void WriteObject(object sendToPipeline)
            _output.Add(sendToPipeline);
        /// Default implementation of the enumerated WriteObject. Either way, the
        /// objects are added to the list passed to this object in the constructor.
        /// <param name="enumerateCollection">If true, the collection is enumerated, otherwise
        /// it's written as a scalar.
        public void WriteObject(object sendToPipeline, bool enumerateCollection)
                IEnumerator e = LanguagePrimitives.GetEnumerator(sendToPipeline);
                        _output.Add(e.Current);
        /// Default implementation - just discards it's arguments.
        /// <param name="progressRecord">Progress record to write.</param>
        public void WriteProgress(ProgressRecord progressRecord) { }
        /// <param name="sourceId">Source ID to write for.</param>
        /// <param name="progressRecord">Record to write.</param>
        public void WriteProgress(Int64 sourceId, ProgressRecord progressRecord) { }
        public void WriteVerbose(string text) { }
        public void WriteWarning(string text) { }
        public void WriteCommandDetail(string text) { }
        /// <param name="informationRecord">Record to write.</param>
        public void WriteInformation(InformationRecord informationRecord) { }
        #endregion Write
        #region Should
        /// Default implementation - always returns true.
        /// <param name="target">Ignored.</param>
        /// <returns>True.</returns>
        public bool ShouldProcess(string target) { return true; }
        /// <param name="action">Ignored.</param>
        public bool ShouldProcess(string target, string action) { return true; }
        /// <param name="verboseDescription">Ignored.</param>
        /// <param name="verboseWarning">Ignored.</param>
        /// <param name="caption">Ignored.</param>
        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption) { return true; }
        /// <param name="shouldProcessReason">Ignored.</param>
        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason) { shouldProcessReason = ShouldProcessReason.None; return true; }
        /// <param name="query">Ignored.</param>
        public bool ShouldContinue(string query, string caption) { return true; }
        /// <param name="yesToAll">Ignored.</param>
        /// <param name="noToAll">Ignored.</param>
        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll) { return true; }
        /// <param name="hasSecurityImpact">Ignored.</param>
        public bool ShouldContinue(string query, string caption, bool hasSecurityImpact, ref bool yesToAll, ref bool noToAll) { return true; }
        #endregion Should
        #region Transaction Support
        /// Returns true if a transaction is available and active.
        public bool TransactionAvailable() { return false; }
        /// Gets an object that surfaces the current PowerShell transaction.
        /// When this object is disposed, PowerShell resets the active transaction.
        public PSTransactionContext CurrentPSTransaction
                string error = TransactionStrings.CmdletRequiresUseTx;
                // We want to throw in this situation, and want to use a
                // property because it mimics the C# using(TransactionScope ...) syntax
#pragma warning suppress 56503
        #endregion Transaction Support
        #region Misc
        /// Implementation of the dummy default ThrowTerminatingError API - it just
        /// does what the base implementation does anyway - rethrow the exception
        /// if it exists, otherwise throw an invalid operation exception.
        /// <param name="errorRecord">The error record to throw.</param>
        public void ThrowTerminatingError(ErrorRecord errorRecord)
                throw new System.InvalidOperationException(errorRecord.ToString());
