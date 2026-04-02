    /// Contains information about a single history entry.
    public class HistoryInfo
        /// <param name="pipelineId">Id of pipeline in which command associated
        /// with this history entry is executed</param>
        /// <param name="cmdline">Command string.</param>
        /// <param name="status">Status of pipeline execution.</param>
        /// <param name="startTime">StartTime of execution.</param>
        /// <param name="endTime">EndTime of execution.</param>
        internal HistoryInfo(long pipelineId, string cmdline, PipelineState status, DateTime startTime, DateTime endTime)
            Dbg.Assert(cmdline != null, "caller should validate the parameter");
            _pipelineId = pipelineId;
            CommandLine = cmdline;
            ExecutionStatus = status;
            StartExecutionTime = startTime;
            EndExecutionTime = endTime;
            Cleared = false;
        /// <param name="history"></param>
        private HistoryInfo(HistoryInfo history)
            Id = history.Id;
            _pipelineId = history._pipelineId;
            CommandLine = history.CommandLine;
            ExecutionStatus = history.ExecutionStatus;
            StartExecutionTime = history.StartExecutionTime;
            EndExecutionTime = history.EndExecutionTime;
            Cleared = history.Cleared;
        /// Id of this history entry.
        public long Id { get; private set; }
        /// CommandLine string.
        public string CommandLine { get; private set; }
        /// Execution status of associated pipeline.
        public PipelineState ExecutionStatus { get; private set; }
        /// Start time of execution of associated pipeline.
        public DateTime StartExecutionTime { get; }
        /// End time of execution of associated pipeline.
        public DateTime EndExecutionTime { get; private set; }
        /// The time it took to execute the associeated pipeline.
        public TimeSpan Duration => EndExecutionTime - StartExecutionTime;
        /// Override for ToString() method.
            if (string.IsNullOrEmpty(CommandLine))
                return CommandLine;
        /// Cleared status of an entry.
        internal bool Cleared { get; set; } = false;
        /// Sets Id.
        internal void SetId(long id) => Id = id;
        /// Set status.
        internal void SetStatus(PipelineState status) => ExecutionStatus = status;
        /// Set endtime.
        /// <param name="endTime"></param>
        internal void SetEndTime(DateTime endTime) => EndExecutionTime = endTime;
        /// Sets command.
        internal void SetCommand(string command) => CommandLine = command;
        /// Id of the pipeline corresponding to this history entry.
        private readonly long _pipelineId;
        /// Returns a clone of this object.
        public HistoryInfo Clone()
            return new HistoryInfo(this);
    /// This class implements history and provides APIs for adding and fetching
    /// entries from history.
    internal class History
        /// Default history size.
        internal const int DefaultHistorySize = 4096;
        /// Constructs history store.
        internal History(ExecutionContext context)
            // Create history size variable. Add ValidateRangeAttribute to
            // validate the range.
            Collection<Attribute> attrs = new Collection<Attribute>();
            attrs.Add(new ValidateRangeAttribute(1, (int)Int16.MaxValue));
            PSVariable historySizeVar = new PSVariable(SpecialVariables.HistorySize, DefaultHistorySize, ScopedItemOptions.None, attrs);
            historySizeVar.Description = SessionStateStrings.MaxHistoryCountDescription;
            context.EngineSessionState.SetVariable(historySizeVar, false, CommandOrigin.Internal);
            _capacity = DefaultHistorySize;
            _buffer = new HistoryInfo[_capacity];
        /// Create a new history entry.
        /// <param name="pipelineId"></param>
        /// <param name="cmdline"></param>
        /// <param name="startTime"></param>
        /// <param name="skipIfLocked">If true, the entry will not be added when the history is locked.</param>
        /// <returns>Id for the new created entry. Use this id to fetch the
        /// entry. Returns -1 if the entry is not added.</returns>
        /// <remarks>This function is thread safe</remarks>
        internal long AddEntry(long pipelineId, string cmdline, PipelineState status, DateTime startTime, DateTime endTime, bool skipIfLocked)
            if (!System.Threading.Monitor.TryEnter(_syncRoot, skipIfLocked ? 0 : System.Threading.Timeout.Infinite))
                ReallocateBufferIfNeeded();
                HistoryInfo entry = new HistoryInfo(pipelineId, cmdline, status, startTime, endTime);
                return Add(entry);
                System.Threading.Monitor.Exit(_syncRoot);
        /// Update the history entry corresponding to id.
        /// <param name="id">Id of history entry to be updated.</param>
        /// <param name="status">Status to be updated.</param>
        /// <param name="endTime">EndTime to be updated.</param>
        internal void UpdateEntry(long id, PipelineState status, DateTime endTime, bool skipIfLocked)
                HistoryInfo entry = CoreGetEntry(id);
                if (entry != null)
                    entry.SetStatus(status);
                    entry.SetEndTime(endTime);
        /// Gets entry from buffer for given id. This id should be the
        /// id returned by Add method.
        /// <param name="id">Id of the entry to be fetched.</param>
        /// <returns>Entry corresponding to id if it is present else null
        internal HistoryInfo GetEntry(long id)
                    if (!entry.Cleared)
                        return entry.Clone();
        /// Get count HistoryEntries.
        /// <param name="newest"></param>
        /// <returns>History entries.</returns>
        internal HistoryInfo[] GetEntries(long id, long count, SwitchParameter newest)
            if (count < -1)
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(count), count);
            if (newest.ToString() == null)
                throw PSTraceSource.NewArgumentNullException(nameof(newest));
            if (count == -1 || count > _countEntriesAdded || count > _countEntriesInBuffer)
                count = _countEntriesInBuffer;
            if (count == 0 || _countEntriesInBuffer == 0)
                return Array.Empty<HistoryInfo>();
                // Using list instead of an array to store the entries.With array we are getting null values
                // when the historybuffer size is changed
                List<HistoryInfo> entriesList = new List<HistoryInfo>();
                if (id > 0)
                    long firstId, baseId;
                    baseId = id;
                    // get id,count,newest values
                    if (!newest.IsPresent)
                        // get older entries
                        // Calculate the first id (i.e lowest id to fetch)
                        firstId = baseId - count + 1;
                        // If first id is less than the lowest id in history store,
                        // assign lowest id as first ID
                        if (firstId < 1)
                            firstId = 1;
                        for (long i = baseId; i >= firstId; --i)
                            if (firstId <= 1) break;
                            // if entry is null , continue the loop with the next entry
                            if (_buffer[GetIndexFromId(i)] == null) continue;
                            if (_buffer[GetIndexFromId(i)].Cleared)
                                // we have to clear count entries before an id, so if an entry is null,decrement
                                // first id as long as its is greater than the lowest entry in the buffer.
                                firstId--;
                        for (long i = firstId; i <= baseId; ++i)
                            // if an entry is null after being cleared by clear-history cmdlet,
                            // continue with the next entry
                            if (_buffer[GetIndexFromId(i)] == null || _buffer[GetIndexFromId(i)].Cleared)
                            entriesList.Add(_buffer[GetIndexFromId(i)].Clone());
                    { // get latest entries
                        // first id becomes the id +count no of entries from the end of the buffer
                        firstId = baseId + count - 1;
                        // if first id is more than the no of entries in the buffer, first id will be the last entry in the buffer
                        if (firstId >= _countEntriesAdded)
                            firstId = _countEntriesAdded;
                        for (long i = baseId; i <= firstId; i++)
                            if (firstId >= _countEntriesAdded) break;
                                // we have to clear count entries before an id, so if an entry is null,increment first id
                                firstId++;
                        for (long i = firstId; i >= baseId; --i)
                    // get entries for count,newest
                    long index, SmallestID = 0;
                    // if we change the defaulthistory size and when no of entries exceed the size, then
                    // we need to get the smallest entry in the buffer when we want to clear the oldest entry
                    // eg if size is 5 and then the entries can be 7,6,1,2,3
                    if (_capacity != DefaultHistorySize)
                        SmallestID = SmallestIDinBuffer();
                        // get oldest count entries
                            if (_countEntriesAdded > _capacity)
                                index = SmallestID;
                        for (long i = count - 1; i >= 0;)
                            if (index > _countEntriesAdded) break;
                            if ((index <= 0 || GetIndexFromId(index) >= _buffer.Length) ||
                                (_buffer[GetIndexFromId(index)].Cleared))
                                index++; continue;
                                entriesList.Add(_buffer[GetIndexFromId(index)].Clone());
                                i--; index++;
                        index = _countEntriesAdded; //SmallestIDinBuffer
                            // if an entry is cleared continue to the next entry
                                    if (index < SmallestID)
                            if (index < 1) break;
                            { index--; continue; }
                                // clone the entry from the history buffer
                                i--; index--;
                HistoryInfo[] entries = new HistoryInfo[entriesList.Count];
                entriesList.CopyTo(entries);
                return entries;
        /// Get History Entries based on the WildCard Pattern value.
        /// If passed 0, returns all the values, else return on the basis of count.
        /// <param name="wildcardpattern"></param>
        internal HistoryInfo[] GetEntries(WildcardPattern wildcardpattern, long count, SwitchParameter newest)
                if (count > _countEntriesAdded || count == -1)
                List<HistoryInfo> cmdlist = new List<HistoryInfo>();
                long SmallestID = 1;
                // if buffersize is changes,Get the smallest entry that's not cleared in the buffer
                if (count != 0)
                        long id = 1;
                                id = SmallestID;
                        for (long i = 0; i <= count - 1;)
                            if (id > _countEntriesAdded) break;
                            if (!_buffer[GetIndexFromId(id)].Cleared && wildcardpattern.IsMatch(_buffer[GetIndexFromId(id)].CommandLine.Trim()))
                                cmdlist.Add(_buffer[GetIndexFromId(id)].Clone()); i++;
                            id++;
                        long id = _countEntriesAdded;
                            // if buffersize is changed,we have to loop from max entry to min entry that's not cleared
                                    if (id < SmallestID)
                            if (id < 1) break;
                            id--;
                    for (long i = 1; i <= _countEntriesAdded; i++)
                        if (!_buffer[GetIndexFromId(i)].Cleared && wildcardpattern.IsMatch(_buffer[GetIndexFromId(i)].CommandLine.Trim()))
                            cmdlist.Add(_buffer[GetIndexFromId(i)].Clone());
                HistoryInfo[] entries = new HistoryInfo[cmdlist.Count];
                cmdlist.CopyTo(entries);
        /// Clears the history entry from buffer for a given id.
        /// <param name="id">Id of the entry to be Cleared.</param>
        /// <returns>Nothing.</returns>
        internal void ClearEntry(long id)
                    throw PSTraceSource.NewArgumentOutOfRangeException(nameof(id), id);
                // no entries are present to clear
                if (_countEntriesInBuffer == 0)
                // throw an exception if id is out of range
                if (id > _countEntriesAdded)
                    entry.Cleared = true;
                    _countEntriesInBuffer--;
        /// gets the total number of entries added
        /// <returns>count of total entries added.</returns>
        internal int Buffercapacity()
            return _capacity;
        /// Adds an entry to the buffer. If buffer is full, overwrites
        /// oldest entry in the buffer.
        /// <returns>Returns id for the entry. This id should be used to fetch
        /// the entry from the buffer.</returns>
        /// <remarks>Id starts from 1 and is incremented by 1 for each new entry</remarks>
        private long Add(HistoryInfo entry)
            _buffer[GetIndexForNewEntry()] = entry;
            // Increment count of entries added so far
            _countEntriesAdded++;
            // Id of an entry in history is same as its number in history store.
            entry.SetId(_countEntriesAdded);
            // Increment count of entries in buffer by 1
            IncrementCountOfEntriesInBuffer();
            return _countEntriesAdded;
        private HistoryInfo CoreGetEntry(long id)
            if (id <= 0)
            // if (_buffer[GetIndexFromId(id)].Cleared == false )
            return _buffer[GetIndexFromId(id)];
            //    return null;
        /// Gets the smallest id in the buffer.
        private long SmallestIDinBuffer()
            long minID = 0;
            if (_buffer == null)
                return minID;
            for (int i = 0; i < _buffer.Length; i++)
                // assign the first entry in the buffer as min.
                if (_buffer[i] != null && !_buffer[i].Cleared)
                    minID = _buffer[i].Id;
            // check for the minimum id that is not cleared
                    if (minID > _buffer[i].Id)
        /// Reallocates the buffer if history size changed.
        private void ReallocateBufferIfNeeded()
            // Get current value of histoysize variable
            int historySize = GetHistorySize();
            if (historySize == _capacity)
            HistoryInfo[] tempBuffer = new HistoryInfo[historySize];
            // Calculate number of entries to copy in new buffer.
            int numberOfEntries = _countEntriesInBuffer;
            // when buffer size is changed,we have to consider the totalnumber of entries added
            if (numberOfEntries < _countEntriesAdded)
                numberOfEntries = (int)_countEntriesAdded;
            if (_countEntriesInBuffer > historySize)
                numberOfEntries = historySize;
            for (int i = numberOfEntries; i > 0; --i)
                long nextId = _countEntriesAdded - i + 1;
                tempBuffer[GetIndexFromId(nextId, historySize)] = _buffer[GetIndexFromId(nextId)];
            _countEntriesInBuffer = numberOfEntries;
            _capacity = historySize;
            _buffer = tempBuffer;
        /// Get the index for new entry.
        /// <returns>Index for new entry.</returns>
        private int GetIndexForNewEntry()
            return (int)(_countEntriesAdded % _capacity);
        /// Gets index in buffer for an entry with given Id.
        private int GetIndexFromId(long id)
            return (int)((id - 1) % _capacity);
        /// Gets index in buffer for an entry with given Id using passed in
        /// capacity.
        private static int GetIndexFromId(long id, int capacity)
            return (int)((id - 1) % capacity);
        /// Increment number of entries in buffer by 1.
        private void IncrementCountOfEntriesInBuffer()
            if (_countEntriesInBuffer < _capacity)
                _countEntriesInBuffer++;
        /// Get the current history size.
        private static int GetHistorySize()
            int historySize = 0;
            var executionContext = LocalPipeline.GetExecutionContextFromTLS();
            object obj = executionContext?.GetVariableValue(SpecialVariables.HistorySizeVarPath);
                    historySize = (int)LanguagePrimitives.ConvertTo(obj, typeof(int), System.Globalization.CultureInfo.InvariantCulture);
            if (historySize <= 0)
                historySize = DefaultHistorySize;
            return historySize;
        /// Buffer.
        private HistoryInfo[] _buffer;
        /// Capacity of circular buffer.
        private int _capacity;
        /// Number of entries in buffer currently.
        private int _countEntriesInBuffer;
        /// Total number of entries added till now including those which have
        /// been overwritten after buffer got full. This is also number of
        /// last entry added.
        private long _countEntriesAdded;
        /// Private object for synchronization.
        /// Return the ID of the next history item to be added.
        internal long GetNextHistoryId()
            return _countEntriesAdded + 1;
        #region invoke_loop_detection
        /// This is a set of HistoryInfo ids which are currently being executed in the
        /// pipelines of the Runspace that is holding this 'History' instance.
        private readonly HashSet<long> _invokeHistoryIds = new HashSet<long>();
        internal bool PresentInInvokeHistoryEntrySet(HistoryInfo entry)
            return _invokeHistoryIds.Contains(entry.Id);
        internal void AddToInvokeHistoryEntrySet(HistoryInfo entry)
            _invokeHistoryIds.Add(entry.Id);
        internal void RemoveFromInvokeHistoryEntrySet(HistoryInfo entry)
            _invokeHistoryIds.Remove(entry.Id);
        #endregion invoke_loop_detection
    /// This class Implements the get-history command.
    [Cmdlet(VerbsCommon.Get, "History", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096788")]
    [OutputType(typeof(HistoryInfo))]
    public class GetHistoryCommand : PSCmdlet
        /// Ids of entries to display.
        private long[] _id;
        public long[] Id
        /// Is Count parameter specified.
        private bool _countParameterSpecified;
        /// Count of entries to display. By default, count is the length of the history buffer.
        /// So "Get-History" returns all history entries.
        private int _count;
        /// No of History Entries (starting from last) that are to be displayed.
        [ValidateRange(0, (int)Int16.MaxValue)]
                return _count;
                _countParameterSpecified = true;
                _count = value;
        /// Implements the Processing() method for show/History command.
            History history = ((LocalRunspace)Context.CurrentRunspace).History;
            if (_id != null)
                if (!_countParameterSpecified)
                    // If Id parameter is specified and count is not specified,
                    // get history
                    foreach (long id in _id)
                        Dbg.Assert(id > 0, "ValidateRangeAttribute should not allow this");
                        HistoryInfo entry = history.GetEntry(id);
                        if (entry != null && entry.Id == id)
                            Exception ex =
                                new ArgumentException
                                    StringUtil.Format(HistoryStrings.NoHistoryForId, id)
                            WriteError
                                new ErrorRecord
                                    "GetHistoryNoHistoryForId",
                                    id
                else if (_id.Length > 1)
                            StringUtil.Format(HistoryStrings.NoCountWithMultipleIds)
                    ThrowTerminatingError
                            "GetHistoryNoCountWithMultipleIds",
                            _count
                    long id = _id[0];
                    WriteObject(history.GetEntries(id, _count, false), true);
                // The default value for _count is the size of the history buffer.
                    _count = history.Buffercapacity();
                HistoryInfo[] entries = history.GetEntries(0, _count, true);
                for (long i = entries.Length - 1; i >= 0; i--)
                    WriteObject(entries[i]);
    /// This class implements the Invoke-History command.
    [Cmdlet(VerbsLifecycle.Invoke, "History", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096586")]
    public class InvokeHistoryCommand : PSCmdlet
        /// Invoke cmd can execute only one history entry. If multiple
        /// ids are provided, we throw error.
        private bool _multipleIdProvided;
        private string _id;
        /// Accepts a string value indicating a previously executed command to
        /// re-execute.
        /// If string can be parsed to long,
        /// it will be used as HistoryId
        /// else
        /// as a string value indicating a previously executed command to
        /// re-execute. This string is the first n characters of the command
        /// that is to be re-executed.
        public string Id
                    // Id has been set already.
                    _multipleIdProvided = true;
        /// Implements the BeginProcessing() method for eval/History command.
            // Invoke-history can execute only one command. If multiple
            // ids were provided, throw exception
            if (_multipleIdProvided)
                        new ArgumentException(HistoryStrings.InvokeHistoryMultipleCommandsError),
                        "InvokeHistoryMultipleCommandsError",
            var ctxRunspace = (LocalRunspace)Context.CurrentRunspace;
            History history = ctxRunspace.History;
            Dbg.Assert(history != null, "History should be non null");
            // Get the history entry to invoke
            HistoryInfo entry = GetHistoryEntryToInvoke(history);
            string commandToInvoke = entry.CommandLine;
            if (!ShouldProcess(commandToInvoke))
            // Check if there is a loop in invoke-history
            if (history.PresentInInvokeHistoryEntrySet(entry))
                        new InvalidOperationException(HistoryStrings.InvokeHistoryLoopDetected),
                        "InvokeHistoryLoopDetected",
                history.AddToInvokeHistoryEntrySet(entry);
            // Replace Invoke-History with string which is getting invoked
            ReplaceHistoryString(entry, ctxRunspace);
                // Echo command
                Host.UI.WriteLine(commandToInvoke);
            catch (HostException)
                // when the host is not interactive, HostException is thrown
            // Items invoked as History should act as though they were submitted by the user - so should still come from
            // the runspace itself. For this reason, it is insufficient to just use the InvokeScript method on the Cmdlet class.
            using (System.Management.Automation.PowerShell ps = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace))
                ps.AddScript(commandToInvoke);
                EventHandler<DataAddedEventArgs> debugAdded = (object sender, DataAddedEventArgs e) => { DebugRecord record = (DebugRecord)((PSDataCollection<DebugRecord>)sender)[e.Index]; WriteDebug(record.Message); };
                EventHandler<DataAddedEventArgs> errorAdded = (object sender, DataAddedEventArgs e) => { ErrorRecord record = (ErrorRecord)((PSDataCollection<ErrorRecord>)sender)[e.Index]; WriteError(record); };
                EventHandler<DataAddedEventArgs> informationAdded = (object sender, DataAddedEventArgs e) => { InformationRecord record = (InformationRecord)((PSDataCollection<InformationRecord>)sender)[e.Index]; WriteInformation(record); };
                EventHandler<DataAddedEventArgs> progressAdded = (object sender, DataAddedEventArgs e) => { ProgressRecord record = (ProgressRecord)((PSDataCollection<ProgressRecord>)sender)[e.Index]; WriteProgress(record); };
                EventHandler<DataAddedEventArgs> verboseAdded = (object sender, DataAddedEventArgs e) => { VerboseRecord record = (VerboseRecord)((PSDataCollection<VerboseRecord>)sender)[e.Index]; WriteVerbose(record.Message); };
                EventHandler<DataAddedEventArgs> warningAdded = (object sender, DataAddedEventArgs e) => { WarningRecord record = (WarningRecord)((PSDataCollection<WarningRecord>)sender)[e.Index]; WriteWarning(record.Message); };
                ps.Streams.Debug.DataAdded += debugAdded;
                ps.Streams.Error.DataAdded += errorAdded;
                ps.Streams.Information.DataAdded += informationAdded;
                ps.Streams.Progress.DataAdded += progressAdded;
                ps.Streams.Verbose.DataAdded += verboseAdded;
                ps.Streams.Warning.DataAdded += warningAdded;
                LocalRunspace localRunspace = ps.Runspace as LocalRunspace;
                    // Indicate to the system that we are in nested prompt mode, since we are emulating running the command at the prompt.
                    // This ensures that the command being run as nested runs in the correct language mode, because CreatePipelineProcessor()
                    // always forces CommandOrigin to Internal for nested running commands, and Command.CreateCommandProcessor() forces Internal
                    // commands to always run in FullLanguage mode unless in a nested prompt.
                    if (localRunspace != null)
                        localRunspace.InInternalNestedPrompt = ps.IsNested;
                    Collection<PSObject> results = ps.Invoke();
                    history.RemoveFromInvokeHistoryEntrySet(entry);
                        localRunspace.InInternalNestedPrompt = false;
                    ps.Streams.Debug.DataAdded -= debugAdded;
                    ps.Streams.Error.DataAdded -= errorAdded;
                    ps.Streams.Information.DataAdded -= informationAdded;
                    ps.Streams.Progress.DataAdded -= progressAdded;
                    ps.Streams.Verbose.DataAdded -= verboseAdded;
                    ps.Streams.Warning.DataAdded -= warningAdded;
        /// Helper function which gets history entry to invoke.
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "It's ok to use ID in the ArgumentException")]
        private HistoryInfo GetHistoryEntryToInvoke(History history)
            HistoryInfo entry = null;
            // User didn't specify any input parameter. Invoke the last
            // entry
            if (_id == null)
                HistoryInfo[] entries = history.GetEntries(0, 1, true);
                if (entries.Length == 1)
                    entry = entries[0];
                        new InvalidOperationException
                            StringUtil.Format(HistoryStrings.NoLastHistoryEntryFound)
                            "InvokeHistoryNoLastHistoryEntryFound",
                // Parse input
                PopulateIdAndCommandLine();
                // User specified a commandline. Get list of all history entries
                // and find latest match
                if (_commandLine != null)
                    HistoryInfo[] entries = history.GetEntries(0, -1, false);
                    // and search backwards through the entries
                    for (int i = entries.Length - 1; i >= 0; i--)
                        if (entries[i].CommandLine.StartsWith(_commandLine, StringComparison.Ordinal))
                                StringUtil.Format(HistoryStrings.NoHistoryForCommandline, _commandLine)
                                "InvokeHistoryNoHistoryForCommandline",
                                _commandLine
                    if (_historyId <= 0)
                            new ArgumentOutOfRangeException
                                "Id",
                                StringUtil.Format(HistoryStrings.InvalidIdGetHistory, _historyId)
                                "InvokeHistoryInvalidIdGetHistory",
                                _historyId
                        // Retrieve the command at the index we've specified
                        entry = history.GetEntry(_historyId);
                        if (entry == null || entry.Id != _historyId)
                                    StringUtil.Format(HistoryStrings.NoHistoryForId, _historyId)
                                    "InvokeHistoryNoHistoryForId",
        /// Id of history entry to execute.
        private long _historyId = -1;
        /// Commandline to execute.
        private string _commandLine;
        /// Parse Id parameter to populate _historyId and _commandLine.
        private void PopulateIdAndCommandLine()
                _historyId = (long)LanguagePrimitives.ConvertTo(_id, typeof(long), System.Globalization.CultureInfo.InvariantCulture);
                _commandLine = _id;
        /// Invoke-history is replaced in history by the command it executed.
        /// This replacement happens only if Invoke-History is single element
        /// in the pipeline. If there are more than one element in pipeline
        /// (ex A | Invoke-History 2 | B) then we cannot do this replacement.
        private static void ReplaceHistoryString(HistoryInfo entry, LocalRunspace localRunspace)
            var pipeline = (LocalPipeline)localRunspace.GetCurrentlyRunningPipeline();
            if (pipeline.AddToHistory)
                pipeline.HistoryString = entry.CommandLine;
    /// This class Implements the add-history command.
    [Cmdlet(VerbsCommon.Add, "History", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096479")]
    public class AddHistoryCommand : PSCmdlet
        private bool _passthru;
        /// A Boolean that indicates whether history objects should be
        /// passed to the next element in the pipeline.
            get { return _passthru; }
            set { _passthru = value; }
        /// Override for BeginProcessing.
        override
        void BeginProcessing()
            // Get currently running pipeline and add history entry for
            // this pipeline.
            // Note:Generally History entry for current pipeline is added
            // on completion of pipeline (See LocalPipeline implementation).
            // However Add-history adds additional entries in to history and
            // additional entries must be added after history for current pipeline.
            // This is done by adding the history entry for current pipeline below.
            LocalPipeline lpl = (LocalPipeline)((RunspaceBase)Context.CurrentRunspace).GetCurrentlyRunningPipeline();
            lpl.AddHistoryEntryFromAddHistoryCmdlet();
        /// Override for ProcessRecord.
        void ProcessRecord()
                foreach (PSObject input in InputObject)
                    // Wrap the inputobject in PSObject and convert it to
                    // HistoryInfo object.
                    HistoryInfo infoToAdd = GetHistoryInfoObject(input);
                    if (infoToAdd != null)
                        long id = history.AddEntry
                                    infoToAdd.CommandLine,
                                    infoToAdd.ExecutionStatus,
                                    infoToAdd.StartExecutionTime,
                                    infoToAdd.EndExecutionTime,
                                    false
                        if (Passthru)
                            HistoryInfo infoAdded = history.GetEntry(id);
                            WriteObject(infoAdded);
        /// Convert mshObject that has the properties of an HistoryInfo
        /// object in to HistoryInfo object.
        /// mshObject to be converted to HistoryInfo.
        /// HistoryInfo object if conversion is successful else null.
        HistoryInfo
        GetHistoryInfoObject(PSObject mshObject)
                // Read CommandLine property
                if (GetPropertyValue(mshObject, "CommandLine") is not string commandLine)
                // Read ExecutionStatus property
                object pipelineState = GetPropertyValue(mshObject, "ExecutionStatus");
                if (pipelineState == null || !LanguagePrimitives.TryConvertTo<PipelineState>(pipelineState, out PipelineState executionStatus))
                // Read StartExecutionTime property
                object temp = GetPropertyValue(mshObject, "StartExecutionTime");
                if (temp == null || !LanguagePrimitives.TryConvertTo<DateTime>(temp, CultureInfo.CurrentCulture, out DateTime startExecutionTime))
                // Read EndExecutionTime property
                temp = GetPropertyValue(mshObject, "EndExecutionTime");
                if (temp == null || !LanguagePrimitives.TryConvertTo<DateTime>(temp, CultureInfo.CurrentCulture, out DateTime endExecutionTime))
                return new HistoryInfo(
                    pipelineId: 0,
                    commandLine,
                    executionStatus,
                    startExecutionTime,
                    endExecutionTime
            // If we are here, an error has occurred.
                new InvalidDataException
                    StringUtil.Format(HistoryStrings.AddHistoryInvalidInput)
                    "AddHistoryInvalidInput",
                    mshObject
        GetPropertyValue(PSObject mshObject, string propertyName)
            PSMemberInfo propertyInfo = mshObject.Properties[propertyName];
            return propertyInfo.Value;
    /// This Class implements the Clear History cmdlet
    [Cmdlet(VerbsCommon.Clear, "History", SupportsShouldProcess = true, DefaultParameterSetName = "IDParameter", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096691")]
    public class ClearHistoryCommand : PSCmdlet
        /// Specifies the ID of a command in the session history.Clear history clears the entries
        /// wit the specified ID(s)
        [Parameter(ParameterSetName = "IDParameter", Position = 0,
           HelpMessage = "Specifies the ID of a command in the session history.Clear history clears only the specified command")]
        /// Id of a history entry.
        private int[] _id;
        /// Command line name of an entry in the session history.
        [Parameter(ParameterSetName = "CommandLineParameter", HelpMessage = "Specifies the name of a command in the session history")]
        public string[] CommandLine
                return _commandline;
                _commandline = value;
        /// Commandline parameter.
        private string[] _commandline = null;
        /// Clears the specified number of history entries
        [Parameter(Mandatory = false, Position = 1, HelpMessage = "Clears the specified number of history entries")]
        /// Count of the history entries.
        private int _count = 32;
        /// A boolean variable to indicate if the count parameter specified.
        private bool _countParameterSpecified = false;
        /// Specifies whether new entries to be cleared or the default old ones.
        [Parameter(Mandatory = false, HelpMessage = "Specifies whether new entries to be cleared or the default old ones.")]
        public SwitchParameter Newest
                return _newest;
                _newest = value;
        /// Switch parameter on the history entries.
        private SwitchParameter _newest;
        /// Overriding Begin Processing.
            _history = ((LocalRunspace)Context.CurrentRunspace).History;
        /// Overriding Process Record.
            // case statement to identify the parameter set
                case "IDParameter":
                    ClearHistoryByID();
                case "CommandLineParameter":
                    ClearHistoryByCmdLine();
                            new ArgumentException("Invalid ParameterSet Name"),
                            "Unable to access the session history", ErrorCategory.InvalidOperation, null));
        /// Clears the session history based on the id parameter
        /// takes no parameters
        private void ClearHistoryByID()
            if (_countParameterSpecified && Count < 0)
                       StringUtil.Format("HistoryStrings", "InvalidCountValue")
                        "ClearHistoryInvalidCountValue",
            // if id parameter is not present
                // if count parameter is not present
                    // clearing the entry for each id in the id[] parameter.
                        HistoryInfo entry = _history.GetEntry(id);
                            _history.ClearEntry(entry.Id);
                        {// throw an exception if an entry for an id is not found
                {// throwing an exception for invalid parameter combinations
                {// if id,count and newest parameters are present
                    // throw an exception for invalid count values
                    ClearHistoryEntries(id, _count, null, _newest);
                // confirmation message if all the clearhistory cmdlet is used without any parameters
                    string message = StringUtil.Format(HistoryStrings.ClearHistoryWarning, "Warning"); // "The command would clear all the entry(s) from the session history,Are you sure you want to continue ?";
                    if (!ShouldProcess(message))
                    ClearHistoryEntries(0, -1, null, _newest);
                    ClearHistoryEntries(0, _count, null, _newest);
        /// Clears the session history based on the Commandline parameter
        private void ClearHistoryByCmdLine()
                       StringUtil.Format(HistoryStrings.InvalidCountValue)
            // if command line is not present
            if (_commandline != null)
                    foreach (string cmd in _commandline)
                        ClearHistoryEntries(0, 1, cmd, _newest);
                else if (_commandline.Length > 1)
                {// throwing exceptions for invalid parameter combinations
                            StringUtil.Format(HistoryStrings.NoCountWithMultipleCmdLine)
                            "NoCountWithMultipleCmdLine ",
                            _commandline
                {   // if commandline,count and newest parameters are present.
                    ClearHistoryEntries(0, _count, _commandline[0], _newest);
        /// Clears the session history based on the input parameter
        /// <param name="id">Id of the entry to be cleared.</param>
        /// <param name="count">Count of entries to be cleared.</param>
        /// <param name="cmdline">Cmdline string to be cleared.</param>
        /// <param name="newest">Order of the entries.</param>
        private void ClearHistoryEntries(long id, int count, string cmdline, SwitchParameter newest)
            // if cmdline is null,use default parameter set notion.
            if (cmdline == null)
                // if id is present,clears count entries from id
                    if (entry == null || entry.Id != id)
                    _entries = _history.GetEntries(id, count, newest);
                {// if only count is present
                    _entries = _history.GetEntries(0, count, newest);
                // creates a wild card pattern
                WildcardPattern wildcardpattern = WildcardPattern.Get(cmdline, WildcardOptions.IgnoreCase);
                // count set to zero if not specified.
                if (!_countParameterSpecified && WildcardPattern.ContainsWildcardCharacters(cmdline))
                // Return the matching history entries for the command line parameter
                // if newest id false...gets the oldest entry
                _entries = _history.GetEntries(wildcardpattern, count, newest);
            // Clear the History value.
            foreach (HistoryInfo entry in _entries)
                if (entry != null && !entry.Cleared)
        /// History obj.
        private History _history;
        /// Array of historyinfo objects.
        private HistoryInfo[] _entries;
