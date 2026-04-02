    /// Enumeration of the possible PowerShell stream types.
    /// This enumeration is obsolete.
    /// This enumeration is a public type formerly used in PowerShell Workflow,
    /// but kept due to its generic name and public accessibility.
    /// It is not used by any other PowerShell API, and is now obsolete
    /// and should not be used if possible.
    [Obsolete("This enum type was used only in PowerShell Workflow and is now obsolete.", error: true)]
    public enum PowerShellStreamType
        /// PSObject.
        Input = 0,
        Output = 1,
        /// ErrorRecord.
        Error = 2,
        /// WarningRecord.
        Warning = 3,
        /// VerboseRecord.
        Verbose = 4,
        /// DebugRecord.
        Debug = 5,
        /// ProgressRecord.
        Progress = 6,
        /// InformationRecord.
        Information = 7
