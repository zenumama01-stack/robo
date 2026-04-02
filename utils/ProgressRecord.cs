    /// Defines a data structure used to represent the status of an ongoing operation at a point in time.
    /// ProgressRecords are passed to <see cref="System.Management.Automation.Cmdlet.WriteProgress(ProgressRecord)"/>,
    /// which, according to user preference, forwards that information on to the host for rendering to the user.
    class ProgressRecord
        #region Public API
        /// Initializes a new instance of the ProgressRecord class and defines the activity Id,
        /// activity description, and status description.
        /// A unique numeric key that identifies the activity to which this record applies.
        /// A description of the activity for which progress is being reported.
        /// A description of the status of the activity.
        ProgressRecord(int activityId, string activity, string statusDescription)
            if (activityId < 0)
                // negative Ids are reserved to indicate "no id" for parent Ids.
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(activityId), activityId, ProgressRecordStrings.ArgMayNotBeNegative, "activityId");
                throw PSTraceSource.NewArgumentException(nameof(activity), ProgressRecordStrings.ArgMayNotBeNullOrEmpty, "activity");
                throw PSTraceSource.NewArgumentException(nameof(activity), ProgressRecordStrings.ArgMayNotBeNullOrEmpty, "statusDescription");
            this.id = activityId;
            this.activity = activity;
            this.status = statusDescription;
        /// Initializes a new instance of the ProgressRecord class and defines the activity Id.
        ProgressRecord(int activityId)
        /// Cloning constructor (all fields are value types - can treat our implementation of cloning as "deep" copy)
        internal ProgressRecord(ProgressRecord other)
            this.activity = other.activity;
            this.currentOperation = other.currentOperation;
            this.id = other.id;
            this.parentId = other.parentId;
            this.percent = other.percent;
            this.secondsRemaining = other.secondsRemaining;
            this.status = other.status;
            this.type = other.type;
        /// Gets the Id of the activity to which this record corresponds.  Used as a 'key' for the
        /// linking of subordinate activities.
        ActivityId
        /// Gets and sets the Id of the activity for which this record is a subordinate.
        /// Used to allow chaining of progress records (such as when one installation invokes a child installation). UI:
        /// normally not directly visible except as already displayed as its own activity. Usually a sub-activity will be
        /// positioned below and to the right of its parent.
        /// A negative value (the default) indicates that the activity is not a subordinate.
        /// May not be the same as ActivityId.
        /// <!--NTRAID#Windows OS Bugs-1161549 the default value for this should be picked up from a variable in the
        /// shell so that a script can set that variable, and have all subsequent calls to WriteProgress (the API) be
        /// subordinate to the "current parent id".-->
        ParentActivityId
                return parentId;
                if (value == ActivityId)
                    throw PSTraceSource.NewArgumentException("value", ProgressRecordStrings.ParentActivityIdCantBeActivityId);
                parentId = value;
        /// Gets and sets the description of the activity for which progress is being reported.
        /// States the overall intent of whats being accomplished, such as "Recursively removing item c:\temp." Typically
        /// displayed in conjunction with a progress bar.
        string
        Activity
                return activity;
                    throw PSTraceSource.NewArgumentException("value", ProgressRecordStrings.ArgMayNotBeNullOrEmpty, "value");
                activity = value;
        /// Gets and sets the current status of the operation, e.g., "35 of 50 items Copied." or "95% completed." or "100 files purged."
        StatusDescription
                status = value;
        /// Gets and sets the current operation of the many required to accomplish the activity (such as "copying foo.txt"). Normally displayed
        /// below its associated progress bar, e.g., "deleting file foo.bar"
        /// Set to null or empty in the case a sub-activity will be used to show the current operation.
        CurrentOperation
                return currentOperation;
                // null or empty string is allowed
                currentOperation = value;
        /// Gets and sets the estimate of the percentage of total work for the activity that is completed.  Typically displayed as a progress bar.
        /// Set to a negative value to indicate that the percentage completed should not be displayed.
        PercentComplete
                return percent;
                // negative values are allowed
                if (value > 100)
                        PSTraceSource.NewArgumentOutOfRangeException(
                            "value", value, ProgressRecordStrings.PercentMayNotBeMoreThan100, "PercentComplete");
                percent = value;
        /// Gets and sets the estimate of time remaining until this activity is completed.  This can be based upon a measurement of time since
        /// started and the percent complete or another approach deemed appropriate by the caller.
        /// Normally displayed beside the progress bar, as "N seconds remaining."
        /// A value less than 0 means "don't display a time remaining."
        SecondsRemaining
                return secondsRemaining;
                secondsRemaining = value;
        /// Gets and sets the type of record represented by this instance.
        ProgressRecordType
        RecordType
                if (value != ProgressRecordType.Completed && value != ProgressRecordType.Processing)
                    throw PSTraceSource.NewArgumentException("value");
        /// Overrides <see cref="object.ToString"/>
        /// "parent = a id = b act = c stat = d cur = e pct = f sec = g type = h" where
        /// a, b, c, d, e, f, and g are the values of ParentActivityId, ActivityId, Activity, StatusDescription,
        /// CurrentOperation, PercentComplete, SecondsRemaining and RecordType properties.
        ToString()
                    "parent = {0} id = {1} act = {2} stat = {3} cur = {4} pct = {5} sec = {6} type = {7}",
                    parentId,
                    id,
                    secondsRemaining,
                    type);
        internal static int? GetSecondsRemaining(DateTime startTime, double percentageComplete)
            Dbg.Assert(percentageComplete >= 0.0, "Caller should verify percentageComplete >= 0.0");
            Dbg.Assert(percentageComplete <= 1.0, "Caller should verify percentageComplete <= 1.0");
                startTime.Kind == DateTimeKind.Utc,
                "DateTime arithmetic should always be done in utc mode [to avoid problems when some operands are calculated right before and right after switching to /from a daylight saving time");
            if ((percentageComplete < 0.00001) || double.IsNaN(percentageComplete))
            DateTime now = DateTime.UtcNow;
            Dbg.Assert(startTime <= now, "Caller should pass a valid startTime");
            TimeSpan elapsedTime = now - startTime;
            TimeSpan totalTime;
                totalTime = TimeSpan.FromMilliseconds(elapsedTime.TotalMilliseconds / percentageComplete);
            TimeSpan remainingTime = totalTime - elapsedTime;
            return (int)(remainingTime.TotalSeconds);
        /// Returns percentage complete when it is impossible to predict how long an operation might take.
        /// The percentage complete will slowly converge toward 100%.
        /// At the <paramref name="expectedDuration"/> the percentage complete will be 90%.
        /// <param name="startTime">When did the operation start.</param>
        /// <param name="expectedDuration">How long does the operation usually take.</param>
        /// <returns>Estimated percentage complete of the operation (always between 0 and 99% - never returns 100%).</returns>
        /// Thrown when
        /// 1) <paramref name="startTime"/> is in the future
        /// 2) <paramref name="expectedDuration"/> is negative or zero
        internal static int GetPercentageComplete(DateTime startTime, TimeSpan expectedDuration)
            ArgumentOutOfRangeException.ThrowIfGreaterThan(startTime, now);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(expectedDuration, TimeSpan.Zero);
             * According to the spec of Checkpoint-Computer
             * (http://cmdletdesigner/SpecViewer/Default.aspx?Project=PowerShell&Cmdlet=Checkpoint-Computer)
             * we have percentage remaining = f(t) where
             * f(inf) = 0%
             * f(0) = 100%
             * f(90) = <something small> = 10%
             * The spec talks about exponential decay, but function based on 1/x seems better:
             * f(t) = a / (T + b)
             * This by definition has f(inf) = 0, so we have to find a and b for the last 2 cases:
             * E1: f(0) = a / (0 + b) = 100
             * E2: f(T = 90) = a / (T + b) = 10
             * From E1 we have a = 100 * b, which we can use in E2:
             * (100 * b) / (T + b) = 10
             * 100 * b = 10 * T + 10 * b
             * 90 * b = 10 * T
             * b = T / 9
             * Some sample values (for T=90):
             * t   | %rem
             * -----------
             * 0   | 100.0%
             * 5   |  66.6%
             * 10  |  50.0%
             * 30  |  25.0%
             * 70  |  12.5%
             * 90  |  10.0%
             * 300 |   3.2%
             * 600 |   1.6%
             * 3600|   0.2%
            TimeSpan timeElapsed = now - startTime;
            double b = expectedDuration.TotalSeconds / 9.0;
            double a = 100.0 * b;
            double percentageRemaining = a / (timeElapsed.TotalSeconds + b);
            double percentageCompleted = 100.0 - percentageRemaining;
            return (int)Math.Floor(percentageCompleted);
        #region DO NOT REMOVE OR RENAME THESE FIELDS - it will break remoting compatibility with Windows PowerShell
        private readonly int id;
        private int parentId = -1;
        private string activity;
        private string status;
        private string currentOperation;
        private int percent = -1;
        private int secondsRemaining = -1;
        private ProgressRecordType type = ProgressRecordType.Processing;
        #region Serialization / deserialization for remoting
        /// Creates a ProgressRecord object from a PSObject property bag.
        /// PSObject has to be in the format returned by ToPSObjectForRemoting method.
        /// <param name="progressAsPSObject">PSObject to rehydrate.</param>
        /// ProgressRecord rehydrated from a PSObject property bag
        /// Thrown if the PSObject is null.
        /// <exception cref="System.Management.Automation.Remoting.PSRemotingDataStructureException">
        /// Thrown when the PSObject is not in the expected format
        internal static ProgressRecord FromPSObjectForRemoting(PSObject progressAsPSObject)
            if (progressAsPSObject == null)
                throw PSTraceSource.NewArgumentNullException(nameof(progressAsPSObject));
            string activity = RemotingDecoder.GetPropertyValue<string>(progressAsPSObject, RemoteDataNameStrings.ProgressRecord_Activity);
            int activityId = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, RemoteDataNameStrings.ProgressRecord_ActivityId);
            string statusDescription = RemotingDecoder.GetPropertyValue<string>(progressAsPSObject, RemoteDataNameStrings.ProgressRecord_StatusDescription);
            ProgressRecord result = new ProgressRecord(activityId, activity, statusDescription);
            result.CurrentOperation = RemotingDecoder.GetPropertyValue<string>(progressAsPSObject, RemoteDataNameStrings.ProgressRecord_CurrentOperation);
            result.ParentActivityId = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, RemoteDataNameStrings.ProgressRecord_ParentActivityId);
            result.PercentComplete = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, RemoteDataNameStrings.ProgressRecord_PercentComplete);
            result.RecordType = RemotingDecoder.GetPropertyValue<ProgressRecordType>(progressAsPSObject, RemoteDataNameStrings.ProgressRecord_Type);
            result.SecondsRemaining = RemotingDecoder.GetPropertyValue<int>(progressAsPSObject, RemoteDataNameStrings.ProgressRecord_SecondsRemaining);
            // Activity used to be mandatory but that's no longer the case.
            // We ensure the string has a value to maintain compatibility with older versions.
            string activity = string.IsNullOrEmpty(Activity) ? " " : Activity;
            PSObject progressAsPSObject = RemotingEncoder.CreateEmptyPSObject();
            progressAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ProgressRecord_Activity, activity));
            progressAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ProgressRecord_ActivityId, this.ActivityId));
            progressAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ProgressRecord_StatusDescription, this.StatusDescription));
            progressAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ProgressRecord_CurrentOperation, this.CurrentOperation));
            progressAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ProgressRecord_ParentActivityId, this.ParentActivityId));
            progressAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ProgressRecord_PercentComplete, this.PercentComplete));
            progressAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ProgressRecord_Type, this.RecordType));
            progressAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ProgressRecord_SecondsRemaining, this.SecondsRemaining));
            return progressAsPSObject;
    /// Defines two types of progress record that refer to the beginning (or middle) and end of an operation.
    enum ProgressRecordType
        /// Operation just started or is not yet complete.
        /// A cmdlet can call WriteProgress with ProgressRecordType.Processing
        /// as many times as it wishes.  However, at the end of the operation,
        /// it should call once more with ProgressRecordType.Completed.
        /// The first time that a host receives a progress record
        /// for a given activity, it will typically display a progress
        /// indicator for that activity.  For each subsequent record
        /// of the same Id, the host will update that display.
        /// Finally, when the host receives a 'completed' record
        /// for that activity, it will remove the progress indicator.
        Processing,
        /// Operation is complete.
        /// If a cmdlet uses WriteProgress, it should use
        /// ProgressRecordType.Completed exactly once, in the last call
        /// to WriteProgress.
        Completed
