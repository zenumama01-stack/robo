    /// The parameters for the paging support enabled by <see cref="CmdletCommonMetadataAttribute.SupportsPaging"/>.
    /// Includes: -IncludeTotalCount, -Skip [int], -First [int]
    public sealed class PagingParameters
        internal PagingParameters(MshCommandRuntime commandRuntime)
            commandRuntime.PagingParameters = this;
        /// Gets or sets the value of the -IncludeTotalCount parameter for all cmdlets that support paging.
        public SwitchParameter IncludeTotalCount { get; set; }
        /// Gets or sets the value of the -Skip parameter for all cmdlets that support paging.
        /// If the user doesn't specify anything, the default is <c>0</c>.
        public UInt64 Skip { get; set; }
        /// Gets or sets the value of the -First parameter for all cmdlets that support paging.
        /// If the user doesn't specify anything, the default is <see cref="System.UInt64.MaxValue"/>.
        public UInt64 First { get; set; } = UInt64.MaxValue;
        #region emitting total count
        /// A helper method for creating an object that represents a total count
        /// of objects that the cmdlet would return without paging
        /// (this can be more than the size of the page specified in the <see cref="First"/> cmdlet parameter).
        /// <param name="totalCount">A total count of objects that the cmdlet would return without paging.</param>
        /// <param name="accuracy">
        /// accuracy of the <paramref name="totalCount"/> parameter.
        /// <c>1.0</c> means 100% accurate;
        /// <c>0.0</c> means that total count is unknown;
        /// anything in-between means that total count is estimated
        /// <returns>An object that represents a total count of objects that the cmdlet would return without paging.</returns>
        public PSObject NewTotalCount(UInt64 totalCount, double accuracy)
            PSObject result = new PSObject(totalCount);
            string toStringMethodBody = string.Format(
                    $totalCount = $this.PSObject.BaseObject
                    switch ($this.Accuracy) {{
                        {{ $_ -ge 1.0 }} {{ '{0}' -f $totalCount }}
                        {{ $_ -le 0.0 }} {{ '{1}' -f $totalCount }}
                        default          {{ '{2}' -f $totalCount }}
                CodeGeneration.EscapeSingleQuotedStringContent(CommandBaseStrings.PagingSupportAccurateTotalCountTemplate),
                CodeGeneration.EscapeSingleQuotedStringContent(CommandBaseStrings.PagingSupportUnknownTotalCountTemplate),
                CodeGeneration.EscapeSingleQuotedStringContent(CommandBaseStrings.PagingSupportEstimatedTotalCountTemplate));
            PSScriptMethod toStringMethod = new PSScriptMethod("ToString", ScriptBlock.Create(toStringMethodBody));
            result.Members.Add(toStringMethod);
            accuracy = Math.Max(0.0, Math.Min(1.0, accuracy));
            PSNoteProperty statusProperty = new PSNoteProperty("Accuracy", accuracy);
            result.Members.Add(statusProperty);
        #endregion emitting total count
    /// The declaration of parameters for the ShouldProcess mechanisms. -Whatif, and -Confirm.
    public sealed class ShouldProcessParameters
        internal ShouldProcessParameters(MshCommandRuntime commandRuntime)
        /// Gets or sets the value of the -Whatif parameter for all cmdlets.
        [Alias("wi")]
        public SwitchParameter WhatIf
                return _commandRuntime.WhatIf;
                _commandRuntime.WhatIf = value;
        /// Gets or sets the value of the -Confirm parameter for all cmdlets.
        [Alias("cf")]
        public SwitchParameter Confirm
                return _commandRuntime.Confirm;
                _commandRuntime.Confirm = value;
    /// The declaration of parameters for the Transactions mechanisms. -UseTransaction, and -BypassTransaction.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA903:InternalNamespaceShouldNotContainPublicTypes", Justification = "These are only exposed by way of the PowerShell cmdlets that surface them.")]
    public sealed class TransactionParameters
        internal TransactionParameters(MshCommandRuntime commandRuntime)
        /// Gets or sets the value of the -UseTransaction parameter for all cmdlets.
        [Alias("usetx")]
        public SwitchParameter UseTransaction
                return _commandRuntime.UseTransaction;
                _commandRuntime.UseTransaction = value;
