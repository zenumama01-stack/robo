using System.ComponentModel; // Win32Exception
using System.Diagnostics; // Eventlog class
    #region GetEventLogCommand
    /// This class implements the Get-EventLog command.
    /// The CLR EventLogEntryCollection class has problems with managing
    /// rapidly spinning logs (i.e. logs set to "Overwrite" which are
    /// rapidly getting new events and discarding old events).
    /// In particular, if you enumerate forward
    ///     EventLogEntryCollection entries = log.Entries;
    ///     foreach (EventLogEntry entry in entries)
    /// it will occasionally skip an entry.  Conversely, if you are
    /// enumerating backward
    ///     int count = entries.Count;
    ///     for (int i = count-1; i >= 0; i--) {
    ///         EventLogEntry entry = entries[i];
    /// it will occasionally repeat an entry.  Accordingly, we enumerate
    /// backward and try to leave off the repeated entries.
    [Cmdlet(VerbsCommon.Get, "EventLog", DefaultParameterSetName = "LogName",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=113314", RemotingCapability = RemotingCapability.SupportedByCommand)]
    [OutputType(typeof(EventLog), typeof(EventLogEntry), typeof(string))]
    public sealed class GetEventLogCommand : PSCmdlet
        /// Read eventlog entries from this log.
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "LogName")]
        [Alias("LN")]
        public string LogName { get; set; }
        /// Read eventlog entries from this computer.
        /// Read only this number of entries.
        [Parameter(ParameterSetName = "LogName")]
        [ValidateRange(0, Int32.MaxValue)]
        public int Newest { get; set; } = Int32.MaxValue;
        /// Return entries "after " this date.
        public DateTime After
            get { return _after; }
                _after = value;
                _isDateSpecified = true;
                _isFilterSpecified = true;
        private DateTime _after;
        /// Return entries "Before" this date.
        public DateTime Before
            get { return _before; }
                _before = value;
        private DateTime _before;
        /// Return entries for this user.Wild characters is supported.
        public string[] UserName
            get { return _username; }
                _username = value;
        private string[] _username;
        /// Match eventlog entries by the InstanceIds
        /// gets or sets an array of instanceIds.
        [Parameter(Position = 1, ParameterSetName = "LogName")]
        [ValidateRangeAttribute((long)0, long.MaxValue)]
        public long[] InstanceId
            get { return _instanceIds; }
                _instanceIds = value;
        private long[] _instanceIds = null;
        /// Match eventlog entries by the Index
        /// gets or sets an array of indexes.
        [ValidateRangeAttribute((int)1, int.MaxValue)]
        public int[] Index
            get { return _indexes; }
                _indexes = value;
        private int[] _indexes = null;
        /// Match eventlog entries by the EntryType
        /// gets or sets an array of EntryTypes.
        [ValidateSetAttribute(new string[] { "Error", "Information", "FailureAudit", "SuccessAudit", "Warning" })]
        [Alias("ET")]
        public string[] EntryType
            get { return _entryTypes; }
                _entryTypes = value;
        private string[] _entryTypes = null;
        /// Get or sets an array of Source.
        [Alias("ABO")]
        public string[] Source
            { return _sources; }
                _sources = value;
        private string[] _sources;
        /// Get or Set Message string to searched in EventLog.
        [Alias("MSG")]
        public string Message
                return _message;
                _message = value;
        private string _message;
        /// Returns Log Entry as base object.
        public SwitchParameter AsBaseObject { get; set; }
        /// Return the Eventlog objects rather than the log contents.
        [Parameter(ParameterSetName = "List")]
        public SwitchParameter List { get; set; }
        /// Return the log names rather than the EventLog objects.
        public SwitchParameter AsString
                return _asString;
                _asString = value;
        private bool _asString /* = false */;
        /// Sets true when Filter is Specified.
        private bool _isFilterSpecified = false;
        private bool _isDateSpecified = false;
        private bool _isThrowError = true;
        /// Process the specified logs.
            if (ParameterSetName == "List")
                if (ComputerName.Length > 0)
                    foreach (string computerName in ComputerName)
                        foreach (EventLog log in EventLog.GetEventLogs(computerName))
                            if (AsString)
                                WriteObject(log.Log);
                                WriteObject(log);
                    foreach (EventLog log in EventLog.GetEventLogs())
                Diagnostics.Assert(ParameterSetName == "LogName", "Unexpected parameter set");
                if (!WildcardPattern.ContainsWildcardCharacters(LogName))
                    OutputEvents(LogName);
                    // If we were given a wildcard that matches more than one log, output the matching logs. Otherwise output the events in the matching log.
                    List<EventLog> matchingLogs = GetMatchingLogs(LogName);
                    if (matchingLogs.Count == 1)
                        OutputEvents(matchingLogs[0].Log);
                        foreach (EventLog log in matchingLogs)
        private void OutputEvents(string logName)
            // 2005/04/21-JonN This somewhat odd structure works
            // around the FXCOP DisposeObjectsBeforeLosingScope rule.
            bool processing = false;
                    using (EventLog specificLog = new EventLog(logName))
                        processing = true;
                        Process(specificLog);
                        using (EventLog specificLog = new EventLog(logName, computerName))
            catch (InvalidOperationException e)
                if (processing)
                ThrowTerminatingError(new ErrorRecord(
                    e, // default exception text is OK
                    "EventLogNotFound",
                    logName));
        private void Process(EventLog log)
            bool matchesfound = false;
            if (Newest == 0)
            // enumerate backward, skipping repeat entries
            EventLogEntryCollection entries = log.Entries;
            int count = entries.Count;
            int lastindex = Int32.MinValue;
            int processed = 0;
            for (int i = count - 1; (i >= 0) && (processed < Newest); i--)
                EventLogEntry entry = null;
                    entry = entries[i];
                    ErrorRecord er = new ErrorRecord(
                        "LogReadError",
                        ErrorCategory.ReadError,
                        null
                    er.ErrorDetails = new ErrorDetails(
                        "EventlogResources",
                        log.Log,
                        e.Message
                    WriteError(er);
                    // NTRAID#Windows Out Of Band Releases-2005/09/27-JonN
                    // Break after the first one, rather than repeating this
                    // over and over
                    Diagnostics.Assert(false,
                        "EventLogEntryCollection error "
                       + e.GetType().FullName
                        + ": " + e.Message);
                if ((entry != null) &&
                ((lastindex == Int32.MinValue
                  || lastindex - entry.Index == 1)))
                    lastindex = entry.Index;
                    if (_isFilterSpecified)
                        if (!FiltersMatch(entry))
                    if (!AsBaseObject)
                        // wrapping in PSobject to insert into PStypesnames
                        PSObject logentry = new PSObject(entry);
                        // inserting at zero position in reverse order
                        logentry.TypeNames.Insert(0, logentry.ImmediateBaseObject + "#" + log.Log + "/" + entry.Source);
                        logentry.TypeNames.Insert(0, logentry.ImmediateBaseObject + "#" + log.Log + "/" + entry.Source + "/" + entry.InstanceId);
                        WriteObject(logentry);
                        matchesfound = true;
                        WriteObject(entry);
                    processed++;
            if (!matchesfound && _isThrowError)
                Exception Ex = new ArgumentException(StringUtil.Format(EventlogResources.NoEntriesFound, log.Log, string.Empty));
                WriteError(new ErrorRecord(Ex, "GetEventLogNoEntriesFound", ErrorCategory.ObjectNotFound, null));
        private bool FiltersMatch(EventLogEntry entry)
            if (_indexes != null)
                if (!((IList)_indexes).Contains(entry.Index))
            if (_instanceIds != null)
                if (!((IList)_instanceIds).Contains(entry.InstanceId))
            if (_entryTypes != null)
                bool entrymatch = false;
                foreach (string type in _entryTypes)
                    if (type.Equals(entry.EntryType.ToString(), StringComparison.OrdinalIgnoreCase))
                        entrymatch = true;
                if (!entrymatch)
                    return entrymatch;
            if (_sources != null)
                bool sourcematch = false;
                foreach (string source in _sources)
                    if (WildcardPattern.ContainsWildcardCharacters(source))
                        _isThrowError = false;
                    WildcardPattern wildcardpattern = WildcardPattern.Get(source, WildcardOptions.IgnoreCase);
                    if (wildcardpattern.IsMatch(entry.Source))
                        sourcematch = true;
                if (!sourcematch)
                    return sourcematch;
            if (_message != null)
                if (WildcardPattern.ContainsWildcardCharacters(_message))
                WildcardPattern wildcardpattern = WildcardPattern.Get(_message, WildcardOptions.IgnoreCase);
                if (!wildcardpattern.IsMatch(entry.Message))
            if (_username != null)
                bool usernamematch = false;
                foreach (string user in _username)
                    if (entry.UserName != null)
                        WildcardPattern wildcardpattern = WildcardPattern.Get(user, WildcardOptions.IgnoreCase);
                        if (wildcardpattern.IsMatch(entry.UserName))
                            usernamematch = true;
                if (!usernamematch)
                    return usernamematch;
            if (_isDateSpecified)
                bool datematch = false;
                if (!_after.Equals(_initial) && _before.Equals(_initial))
                    if (entry.TimeGenerated > _after)
                        datematch = true;
                else if (!_before.Equals(_initial) && _after.Equals(_initial))
                    if (entry.TimeGenerated < _before)
                else if (!_after.Equals(_initial) && !_before.Equals(_initial))
                    if (_after > _before || _after == _before)
                        if ((entry.TimeGenerated > _after) || (entry.TimeGenerated < _before))
                        if ((entry.TimeGenerated > _after) && (entry.TimeGenerated < _before))
                if (!datematch)
                    return datematch;
        private List<EventLog> GetMatchingLogs(string pattern)
            WildcardPattern wildcardPattern = WildcardPattern.Get(pattern, WildcardOptions.IgnoreCase);
            List<EventLog> matchingLogs = new List<EventLog>();
                    if (wildcardPattern.IsMatch(log.Log))
                        matchingLogs.Add(log);
            return matchingLogs;
        // private string ErrorBase = "EventlogResources";
        private DateTime _initial = new DateTime();
        #endregion Private
    #endregion GetEventLogCommand
    #region ClearEventLogCommand
    /// This class implements the Clear-EventLog command.
    [Cmdlet(VerbsCommon.Clear, "EventLog", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135198", RemotingCapability = RemotingCapability.SupportedByCommand)]
    public sealed class ClearEventLogCommand : PSCmdlet
        /// Clear these logs.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public string[] LogName { get; set; }
        /// Clear eventlog entries from these Computers.
        public string[] ComputerName { get; set; } = { "." };
        /// Does the processing.
            string computer = string.Empty;
            foreach (string compName in ComputerName)
                if ((compName.Equals("localhost", StringComparison.OrdinalIgnoreCase)) || (compName.Equals(".", StringComparison.OrdinalIgnoreCase)))
                    computer = "localhost";
                    computer = compName;
                foreach (string eventString in LogName)
                        if (!EventLog.Exists(eventString, compName))
                            ErrorRecord er = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.LogDoesNotExist, eventString, computer)), null, ErrorCategory.InvalidOperation, null);
                        if (!ShouldProcess(StringUtil.Format(EventlogResources.ClearEventLogWarning, eventString, computer)))
                        EventLog Log = new EventLog(eventString, compName);
                        Log.Clear();
                    catch (System.IO.IOException)
                        ErrorRecord er = new ErrorRecord(new System.IO.IOException(StringUtil.Format(EventlogResources.PathDoesNotExist, null, computer)), null, ErrorCategory.InvalidOperation, null);
                    catch (Win32Exception)
                        ErrorRecord er = new ErrorRecord(new Win32Exception(StringUtil.Format(EventlogResources.NoAccess, null, computer)), null, ErrorCategory.PermissionDenied, null);
                    catch (InvalidOperationException)
                        ErrorRecord er = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.OSWritingError)), null, ErrorCategory.ReadError, null);
        // beginprocessing
    #endregion ClearEventLogCommand
    #region WriteEventLogCommand
    /// This class implements the Write-EventLog command.
    [Cmdlet(VerbsCommunications.Write, "EventLog", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135281", RemotingCapability = RemotingCapability.SupportedByCommand)]
    public sealed class WriteEventLogCommand : PSCmdlet
        /// Write eventlog entries in this log.
        [Parameter(Position = 0, Mandatory = true)]
        /// The source by which the application is registered on the specified computer.
        [Parameter(Position = 1, Mandatory = true)]
        [Alias("SRC")]
        public string Source { get; set; }
        /// String which represents One of the EventLogEntryType values.
        [Parameter(Position = 3)]
        public EventLogEntryType EntryType { get; set; } = EventLogEntryType.Information;
        /// The application-specific subcategory associated with the message.
        public Int16 Category { get; set; } = 1;
        /// The application-specific identifier for the event.
        [Parameter(Position = 2, Mandatory = true)]
        [Alias("ID", "EID")]
        [ValidateRange(0, UInt16.MaxValue)]
        public Int32 EventId { get; set; }
        /// The message goes here.
        [Parameter(Position = 4, Mandatory = true)]
        [ValidateLength(0, 32766)]
        public string Message { get; set; }
        /// Write eventlog entries of this log.
        [Alias("RD")]
        public byte[] RawData { get; set; }
        [Alias("CN")]
        public string ComputerName { get; set; } = ".";
        #region private
        private void WriteNonTerminatingError(Exception exception, string errorId, string errorMessage,
            ErrorCategory category)
            Exception ex = new Exception(errorMessage, exception);
            WriteError(new ErrorRecord(ex, errorId, category, null));
        #endregion private
            string _computerName = string.Empty;
            if ((ComputerName.Equals("localhost", StringComparison.OrdinalIgnoreCase)) || (ComputerName.Equals(".", StringComparison.OrdinalIgnoreCase)))
                _computerName = "localhost";
                _computerName = ComputerName;
                if (!(EventLog.SourceExists(Source, ComputerName)))
                    ErrorRecord er = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.SourceDoesNotExist, null, _computerName, Source)), null, ErrorCategory.InvalidOperation, null);
                    if (!(EventLog.Exists(LogName, ComputerName)))
                        ErrorRecord er = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.LogDoesNotExist, LogName, _computerName)), null, ErrorCategory.InvalidOperation, null);
                        EventLog _myevent = new EventLog(LogName, ComputerName, Source);
                        _myevent.WriteEntry(Message, EntryType, EventId, Category, RawData);
            catch (ArgumentException ex)
                WriteNonTerminatingError(ex, ex.Message, ex.Message, ErrorCategory.InvalidOperation);
            catch (InvalidOperationException ex)
                WriteNonTerminatingError(ex, "AccessDenied", StringUtil.Format(EventlogResources.AccessDenied, LogName, null, Source), ErrorCategory.PermissionDenied);
            catch (Win32Exception ex)
                WriteNonTerminatingError(ex, "OSWritingError", StringUtil.Format(EventlogResources.OSWritingError, null, null, null), ErrorCategory.WriteError);
            catch (System.IO.IOException ex)
                WriteNonTerminatingError(ex, "PathDoesNotExist", StringUtil.Format(EventlogResources.PathDoesNotExist, null, ComputerName, null), ErrorCategory.InvalidOperation);
    #endregion WriteEventLogCommand
    #region LimitEventLogCommand
    /// This class implements the Limit-EventLog command.
    [Cmdlet(VerbsData.Limit, "EventLog", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135227", RemotingCapability = RemotingCapability.SupportedByCommand)]
    public sealed class LimitEventLogCommand : PSCmdlet
        /// Limit the properties of this log.
        /// Limit eventlog entries of this computer.
        /// Minimum retention days for this log.
        [Alias("MRD")]
        [ValidateRange(1, 365)]
        public Int32 RetentionDays
            get { return _retention; }
                _retention = value;
                _retentionSpecified = true;
        private Int32 _retention;
        private bool _retentionSpecified = false;
        /// Overflow action to be taken.
        [Alias("OFA")]
        [ValidateSetAttribute(new string[] { "OverwriteOlder", "OverwriteAsNeeded", "DoNotOverwrite" })]
        public System.Diagnostics.OverflowAction OverflowAction
            get { return _overflowaction; }
                _overflowaction = value;
                _overflowSpecified = true;
        private System.Diagnostics.OverflowAction _overflowaction;
        private bool _overflowSpecified = false;
        /// Maximum size of this log.
        public Int64 MaximumSize
            get { return _maximumKilobytes; }
                _maximumKilobytes = value;
                _maxkbSpecified = true;
        private Int64 _maximumKilobytes;
        private bool _maxkbSpecified = false;
        private void WriteNonTerminatingError(Exception exception, string resourceId, string errorId,
      ErrorCategory category, string _logName, string _compName)
            Exception ex = new Exception(StringUtil.Format(resourceId, _logName, _compName), exception);
        protected override
        BeginProcessing()
            foreach (string compname in ComputerName)
                if ((compname.Equals("localhost", StringComparison.OrdinalIgnoreCase)) || (compname.Equals(".", StringComparison.OrdinalIgnoreCase)))
                    computer = compname;
                foreach (string logname in LogName)
                        if (!EventLog.Exists(logname, compname))
                            ErrorRecord er = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.LogDoesNotExist, logname, computer)), null, ErrorCategory.InvalidOperation, null);
                            if (!ShouldProcess(StringUtil.Format(EventlogResources.LimitEventLogWarning, logname, computer)))
                                EventLog newLog = new EventLog(logname, compname);
                                int _minRetention = newLog.MinimumRetentionDays;
                                System.Diagnostics.OverflowAction _newFlowAction = newLog.OverflowAction;
                                if (_retentionSpecified && _overflowSpecified)
                                    if (_overflowaction.CompareTo(System.Diagnostics.OverflowAction.OverwriteOlder) == 0)
                                        newLog.ModifyOverflowPolicy(_overflowaction, _retention);
                                        ErrorRecord er = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.InvalidOverflowAction)), null, ErrorCategory.InvalidOperation, null);
                                else if (_retentionSpecified && !_overflowSpecified)
                                    if (_newFlowAction.CompareTo(System.Diagnostics.OverflowAction.OverwriteOlder) == 0)
                                        newLog.ModifyOverflowPolicy(_newFlowAction, _retention);
                                else if (!_retentionSpecified && _overflowSpecified)
                                    newLog.ModifyOverflowPolicy(_overflowaction, _minRetention);
                                if (_maxkbSpecified)
                                    int kiloByte = 1024;
                                    _maximumKilobytes = _maximumKilobytes / kiloByte;
                                    newLog.MaximumKilobytes = _maximumKilobytes;
                        WriteNonTerminatingError(ex, EventlogResources.PermissionDenied, "PermissionDenied", ErrorCategory.PermissionDenied, logname, computer);
                        WriteNonTerminatingError(ex, EventlogResources.PathDoesNotExist, "PathDoesNotExist", ErrorCategory.InvalidOperation, null, computer);
                    catch (ArgumentOutOfRangeException ex)
                        if (!_retentionSpecified && !_maxkbSpecified)
                            WriteNonTerminatingError(ex, EventlogResources.InvalidArgument, "InvalidArgument", ErrorCategory.InvalidData, null, null);
                            WriteNonTerminatingError(ex, EventlogResources.ValueOutofRange, "ValueOutofRange", ErrorCategory.InvalidData, null, null);
        #endregion override
    #endregion LimitEventLogCommand
    #region ShowEventLogCommand
    /// This class implements the Show-EventLog command.
    [Cmdlet(VerbsCommon.Show, "EventLog", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135257", RemotingCapability = RemotingCapability.SupportedByCommand)]
    public sealed class ShowEventLogCommand : PSCmdlet
        /// Show eventviewer of this computer.
        [Parameter(Position = 0)]
                string eventVwrExe = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                    "eventvwr.exe");
                Process.Start(eventVwrExe, ComputerName);
            catch (Win32Exception e)
                if (e.NativeErrorCode.Equals(0x00000002))
                    string message = StringUtil.Format(EventlogResources.NotSupported);
                    InvalidOperationException ex = new InvalidOperationException(message);
                    ErrorRecord er = new ErrorRecord(ex, "Win32Exception", ErrorCategory.InvalidOperation, null);
                    ErrorRecord er = new ErrorRecord(e, "Win32Exception", ErrorCategory.InvalidArgument, null);
            catch (SystemException ex)
                ErrorRecord er = new ErrorRecord(ex, "InvalidComputerName", ErrorCategory.InvalidArgument, ComputerName);
    #endregion ShowEventLogCommand
    #region NewEventLogCommand
    /// This cmdlet creates the new event log .This cmdlet can also be used to
    /// configure a new source for writing entries to an event log on the local
    /// computer or a remote computer.
    /// You can create an event source for an existing event log or a new event log.
    /// When you create a new source for a new event log, the system registers the
    /// source for that log, but the log is not created until the first entry is
    /// written to it.
    /// The operating system stores event logs as files. The associated file is
    /// stored in the %SystemRoot%\System32\Config directory on the specified
    /// computer. The file name is set by appending the first 8 characters of the
    /// Log property with the ".evt" file name extension.
    /// You can register the event source with localized resource file(s) for your
    /// event category and message strings. Your application can write event log
    /// entries using resource identifiers, rather than specifying the actual
    /// string. You can register a separate file for event categories, messages and
    /// parameter insertion strings, or you can register the same resource file for
    /// all three types of strings.
    [Cmdlet(VerbsCommon.New, "EventLog", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135235", RemotingCapability = RemotingCapability.SupportedByCommand)]
    public class NewEventLogCommand : PSCmdlet
        #region Parameter
        /// The following is the definition of the input parameter "CategoryResourceFile".
        /// Specifies the path of the resource file that contains category strings for
        /// the source
        /// Resource File is expected to be present in Local/Remote Machines.
        [Alias("CRF")]
        public string CategoryResourceFile { get; set; }
        /// Specify the Computer Name. The default is local computer.
        [Parameter(Position = 2)]
        /// The following is the definition of the input parameter "LogName".
        /// Specifies the name of the log.
                   Position = 0)]
        /// The following is the definition of the input parameter "MessageResourceFile".
        /// Specifies the path of the message resource file that contains message
        /// formatting strings for the source
        [Alias("MRF")]
        public string MessageResourceFile { get; set; }
        /// The following is the definition of the input parameter "ParameterResourceFile".
        /// Specifies the path of the resource file that contains message parameter
        /// strings for the source
        [Alias("PRF")]
        public string ParameterResourceFile { get; set; }
        /// The following is the definition of the input parameter "Source".
        /// Specifies the Source of the EventLog.
                   Position = 1)]
        public string[] Source { get; set; }
        #endregion Parameter
            ErrorCategory category, string _logName, string _compName, string _source, string _resourceFile)
            Exception ex = new Exception(StringUtil.Format(resourceId, _logName, _compName, _source, _resourceFile), exception);
        #region override
                    foreach (string _sourceName in Source)
                        if (!EventLog.SourceExists(_sourceName, compname))
                            EventSourceCreationData newEventSource = new EventSourceCreationData(_sourceName, LogName);
                            newEventSource.MachineName = compname;
                            if (!string.IsNullOrEmpty(MessageResourceFile))
                                newEventSource.MessageResourceFile = MessageResourceFile;
                            if (!string.IsNullOrEmpty(ParameterResourceFile))
                                newEventSource.ParameterResourceFile = ParameterResourceFile;
                            if (!string.IsNullOrEmpty(CategoryResourceFile))
                                newEventSource.CategoryResourceFile = CategoryResourceFile;
                            EventLog.CreateEventSource(newEventSource);
                            ErrorRecord er = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.SourceExistInComp, null, computer, _sourceName)), null, ErrorCategory.InvalidOperation, null);
                    WriteNonTerminatingError(ex, EventlogResources.PermissionDenied, "PermissionDenied", ErrorCategory.PermissionDenied, LogName, computer, null, null);
                    ErrorRecord er = new ErrorRecord(ex, "NewEventlogException", ErrorCategory.InvalidArgument, null);
                catch (System.Security.SecurityException ex)
                    WriteNonTerminatingError(ex, EventlogResources.AccessIsDenied, "AccessIsDenied", ErrorCategory.InvalidOperation, null, null, null, null);
        // End BeginProcessing()
    #endregion NewEventLogCommand
    #region RemoveEventLogCommand
    /// This cmdlet is used to delete the specified event log from the specified
    /// computer. This can also be used to Clear the entries of the specified event
    /// log and also to unregister the Source associated with the eventlog.
    [Cmdlet(VerbsCommon.Remove, "EventLog",
             SupportsShouldProcess = true, DefaultParameterSetName = "Default",
             HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135248", RemotingCapability = RemotingCapability.SupportedByCommand)]
    public class RemoveEventLogCommand : PSCmdlet
        /// Specifies the Computer Name.
        /// Specifies the Event Log Name.
                   Position = 0, ParameterSetName = "Default")]
        /// The following is the definition of the input parameter "RemoveSource".
        /// Specifies either to remove the event log and associated source or
        /// source. alone.
        /// When this parameter is not specified, the cmdlet uses Delete Method which
        /// clears the eventlog and also the source associated with it.
        /// When this parameter value is true, then this cmdlet uses DeleteEventSource
        /// Method to delete the Source alone.
        [Parameter(ParameterSetName = "Source")]
                    if (ParameterSetName.Equals("Default"))
                        foreach (string log in LogName)
                                if (EventLog.Exists(log, compName))
                                    if (!ShouldProcess(StringUtil.Format(EventlogResources.RemoveEventLogWarning, log, computer)))
                                    EventLog.Delete(log, compName);
                                    ErrorRecord er = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.LogDoesNotExist, log, computer)), null, ErrorCategory.InvalidOperation, null);
                                ErrorRecord er = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.PathDoesNotExist, null, computer)), null, ErrorCategory.InvalidOperation, null);
                        foreach (string src in Source)
                                if (EventLog.SourceExists(src, compName))
                                    if (!ShouldProcess(StringUtil.Format(EventlogResources.RemoveSourceWarning, src, computer)))
                                    EventLog.DeleteEventSource(src, compName);
                                    ErrorRecord er = new ErrorRecord(new InvalidOperationException(StringUtil.Format(EventlogResources.SourceDoesNotExist, string.Empty, computer, src)), null, ErrorCategory.InvalidOperation, null);
                ErrorRecord er = new ErrorRecord(ex, "NewEventlogException", ErrorCategory.SecurityError, null);
    #endregion RemoveEventLogCommand
