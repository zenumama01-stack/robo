    /// Write progress record of given activity
    internal sealed class CimWriteProgress : CimBaseAction
        /// Initializes a new instance of the <see cref="CimWriteProgress"/> class.
        /// <param name="activity">
        ///  Activity identifier of the given activity
        /// <param name="currentOperation">
        /// current operation description of the given activity
        /// <param name="statusDescription">
        /// current status description of the given activity
        /// <param name="percentageCompleted">
        /// percentage completed of the given activity
        /// <param name="secondsRemaining">
        /// how many seconds remained for the given activity
        public CimWriteProgress(
            string theActivity,
            int theActivityID,
            string theCurrentOperation,
            string theStatusDescription,
            uint thePercentageCompleted,
            uint theSecondsRemaining)
            this.Activity = theActivity;
            this.ActivityID = theActivityID;
            this.CurrentOperation = theCurrentOperation;
            if (string.IsNullOrEmpty(theStatusDescription))
                this.StatusDescription = CimCmdletStrings.DefaultStatusDescription;
                this.StatusDescription = theStatusDescription;
            this.PercentageCompleted = thePercentageCompleted;
            this.SecondsRemaining = theSecondsRemaining;
        /// Write progress record to powershell
            DebugHelper.WriteLog(
                "...Activity {0}: id={1}, remain seconds ={2}, percentage completed = {3}",
                4,
                this.Activity,
                this.ActivityID,
                this.SecondsRemaining,
                this.PercentageCompleted);
            ProgressRecord record = new(
                this.StatusDescription);
            record.Activity = this.Activity;
            record.ParentActivityId = 0;
            record.SecondsRemaining = (int)this.SecondsRemaining;
            record.PercentComplete = (int)this.PercentageCompleted;
            cmdlet.WriteProgress(record);
        /// Gets the activity of the given activity.
        internal string Activity { get; }
        /// Gets the activity identifier of the given activity.
        internal int ActivityID { get; }
        /// Gets the current operation text of the given activity.
        internal string CurrentOperation { get; }
        /// Gets the status description of the given activity.
        internal string StatusDescription { get; }
        /// Gets the percentage completed of the given activity.
        internal uint PercentageCompleted { get; }
        /// Gets the number of seconds remaining for the given activity.
        internal uint SecondsRemaining { get; }
