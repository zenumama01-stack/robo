    internal partial
    class ConsoleHostUserInterface : PSHostUserInterface
        /// Called at the end of a prompt loop to take down any progress display that might have appeared and purge any
        /// outstanding progress activity state.
        ResetProgress()
            // destroy the data structures representing outstanding progress records
            // take down and destroy the progress display
            // If we have multiple runspaces on the host then any finished pipeline in any runspace will lead to call 'ResetProgress'
            // so we need the lock
                if (_progPaneUpdateTimer != null)
                    // Stop update a progress pane and destroy timer
                    _progPaneUpdateTimer.Dispose();
                    _progPaneUpdateTimer = null;
                // We don't set 'progPaneUpdateFlag = 0' here, because:
                // 1. According to MSDN, the timer callback can occur after the Dispose() method has been called.
                //    So we cannot guarantee the flag is truly set to 0.
                // 2. When creating a new timer in 'HandleIncomingProgressRecord', we will set the flag to 1 anyway
                if (_progPane != null)
                    Dbg.Assert(_pendingProgress != null, "How can you have a progress pane and no backing data structure?");
                    _progPane.Hide();
                    _progPane = null;
                _pendingProgress = null;
                if (SupportsVirtualTerminal && !PSHost.IsStdOutputRedirected && PSStyle.Instance.Progress.UseOSCIndicator)
                    // OSC sequence to turn off progress indicator
                    // https://github.com/microsoft/terminal/issues/6700
                    Console.Write("\x1b]9;4;0\x1b\\");
        /// Invoked by ConsoleHostUserInterface.WriteProgress to update the set of outstanding activities for which
        /// ProgressRecords have been received.
        HandleIncomingProgressRecord(long sourceId, ProgressRecord record)
            Dbg.Assert(record != null, "record should not be null");
            if (_pendingProgress == null)
                Dbg.Assert(_progPane == null, "If there is no data struct, there shouldn't be a pane, either.");
                _pendingProgress = new PendingProgress();
            _pendingProgress.Update(sourceId, record);
            if (_progPane == null)
                // This is the first time we've received a progress record
                // Create a progress pane
                // Set up a update flag
                // Create a timer for updating the flag
                _progPane = new ProgressPane(this);
                if (_progPaneUpdateTimer == null)
                    // Show a progress pane at the first time we've received a progress record
                    progPaneUpdateFlag = 1;
                    // The timer will be auto restarted every 'UpdateTimerThreshold' ms
                    _progPaneUpdateTimer = new Timer(new TimerCallback(ProgressPaneUpdateTimerElapsed), null, UpdateTimerThreshold, UpdateTimerThreshold);
            if (Interlocked.CompareExchange(ref progPaneUpdateFlag, 0, 1) == 1 || record.RecordType == ProgressRecordType.Completed)
                // Update the progress pane only when the timer set up the update flag or WriteProgress is completed.
                // As a result, we do not block WriteProgress and whole script and eliminate unnecessary console locks and updates.
                    int percentComplete = record.PercentComplete;
                    if (percentComplete < 0)
                        // Write-Progress allows for negative percent complete, but not greater than 100
                        // but OSC sequence is limited from 0 to 100.
                        percentComplete = 0;
                    // OSC sequence to turn on progress indicator
                    Console.Write($"\x1b]9;4;1;{percentComplete}\x1b\\");
                // If VT is not supported, we change ProgressView to classic
                if (!SupportsVirtualTerminal)
                    PSStyle.Instance.Progress.View = ProgressView.Classic;
                _progPane.Show(_pendingProgress);
        /// TimerCallback for '_progPaneUpdateTimer' to update 'progPaneUpdateFlag'
        ProgressPaneUpdateTimerElapsed(object sender)
            Interlocked.CompareExchange(ref progPaneUpdateFlag, 1, 0);
        PreWrite()
            _progPane?.Hide();
        PostWrite()
            _progPane?.Show();
        PostWrite(ReadOnlySpan<char> value, bool newLine)
                    _parent.WriteToTranscript(value, newLine);
                    _parent.IsTranscribing = false;
        PreRead()
        PostRead()
        PostRead(string value)
                    _parent.WriteLineToTranscript(value);
        private ProgressPane _progPane = null;
        private PendingProgress _pendingProgress = null;
        // The timer set up 'progPaneUpdateFlag' every 'UpdateTimerThreshold' milliseconds to update 'ProgressPane'
        private Timer _progPaneUpdateTimer = null;
        private const int UpdateTimerThreshold = 200;
        private int progPaneUpdateFlag = 0;
