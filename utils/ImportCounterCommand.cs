    [Cmdlet(VerbsData.Import, "Counter", DefaultParameterSetName = "GetCounterSet", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=138338")]
    public sealed class ImportCounterCommand : PSCmdlet
        public string[] Path
        private string[] _path;
        private StringCollection _resolvedPaths = new StringCollection();
        private List<string> _accumulatedFileNames = new List<string>();
        public string[] ListSet
            get { return _listSet; }
            set { _listSet = value; }
        private string[] _listSet = Array.Empty<string>();
        // StartTime parameter
        public DateTime StartTime
            get { return _startTime; }
            set { _startTime = value; }
        private DateTime _startTime = DateTime.MinValue;
        // EndTime parameter
        public DateTime EndTime
            get { return _endTime; }
            set { _endTime = value; }
        private DateTime _endTime = DateTime.MaxValue;
            get { return _counter; }
            set { _counter = value; }
        private string[] _counter = Array.Empty<string>();
        // Summary switch
        [Parameter(ParameterSetName = "SummarySet")]
        public SwitchParameter Summary
            get { return _summary; }
            set { _summary = value; }
        private SwitchParameter _summary;
            get { return _maxSamples; }
            set { _maxSamples = value; }
        private Int64 _maxSamples = KEEP_ON_SAMPLING;
        // AccumulatePipelineFileNames() accumulates counter file paths in the pipeline scenario:
        // we do not want to construct a Pdh query until all the file names are supplied.
            _accumulatedFileNames.AddRange(_path);
            _pdhHelper = new PdhHelper(System.Environment.OSVersion.Version.Major < 6);
            // Resolve and validate the Path argument: present for all parametersets.
            if (!ResolveFilePaths())
            ValidateFilePaths();
                case "SummarySet":
                    ProcessSummary();
                    Debug.Assert(false, $"Invalid parameter set name: {ParameterSetName}");
        // ProcessSummary().
        // Does the work to process Summary parameter set.
        private void ProcessSummary()
            uint res = _pdhHelper.ConnectToDataSource(_resolvedPaths);
            CounterFileInfo summaryObj;
            res = _pdhHelper.GetFilesSummary(out summaryObj);
            WriteObject(summaryObj);
        // ProcessListSet().
        // Does the work to process ListSet parameter set.
            StringCollection machineNames = new StringCollection();
            res = _pdhHelper.EnumBlgFilesMachines(ref machineNames);
            foreach (string machine in machineNames)
                StringCollection counterSets = new StringCollection();
                res = _pdhHelper.EnumObjects(machine, ref counterSets);
                StringCollection validPaths = new StringCollection();
                foreach (string pattern in _listSet)
                    WildcardPattern wildLogPattern = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
                        StringCollection counterSetCounters = new StringCollection();
                        StringCollection counterSetInstances = new StringCollection();
                        Dictionary<string, string[]> counterInstanceMapping = new Dictionary<string, string[]>();
                            counterInstanceMapping.Add(counter, instanceArray);
                        CounterSet setObj = new CounterSet(counterSet, machine, categoryType, setHelp, ref counterInstanceMapping);
                        string msg = _resourceMgr.GetString("NoMatchingCounterSetsInFile");
                        Exception exc = new Exception(string.Format(CultureInfo.InvariantCulture, msg,
                        CommonUtilities.StringArrayToString(_resolvedPaths),
                        pattern));
                        WriteError(new ErrorRecord(exc, "NoMatchingCounterSetsInFile", ErrorCategory.ObjectNotFound, null));
            // Validate StartTime-EndTime, if present
            if (_startTime != DateTime.MinValue || _endTime != DateTime.MaxValue)
                if (_startTime >= _endTime)
                    string msg = string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("CounterInvalidDateRange"));
                    ThrowTerminatingError(new ErrorRecord(exc, "CounterInvalidDateRange", ErrorCategory.InvalidArgument, null));
            if (_counter.Length > 0)
                foreach (string path in _counter)
                    res = _pdhHelper.ExpandWildCardPath(path, out expandedPaths);
                        WriteDebug(path);
                            string msg = string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("CounterPathIsInvalid"), path);
                        validPaths.Add(expandedPath);
                if (validPaths.Count == 0)
                res = _pdhHelper.GetValidPathsFromFiles(ref validPaths);
                string msg = string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("CounterPathsInFilesInvalid"));
                ThrowTerminatingError(new ErrorRecord(exc, "CounterPathsInFilesInvalid", ErrorCategory.InvalidResult, null));
                res = _pdhHelper.SetQueryTimeRange(_startTime, _endTime);
            res = _pdhHelper.AddCounters(ref validPaths, true);
            uint samplesRead = 0;
            while (!_stopping)
                res = _pdhHelper.ReadNextSet(out nextSet, false);
                if (res == PdhResults.PDH_NO_MORE_DATA)
                if (res != 0 && res != PdhResults.PDH_INVALID_DATA)
                // Display data
                WriteSampleSetObject(nextSet, (samplesRead == 0));
                samplesRead++;
                if (_maxSamples != KEEP_ON_SAMPLING && samplesRead >= _maxSamples)
        // ValidateFilePaths() helper.
        // Validates the _resolvedPaths: present for all parametersets.
        // We cannot have more than 32 blg files, or more than one CSV or TSC file.
        // Files have to all be of the same type (.blg, .csv, .tsv).
        private void ValidateFilePaths()
            Debug.Assert(_resolvedPaths.Count > 0);
            string firstExt = System.IO.Path.GetExtension(_resolvedPaths[0]);
            foreach (string fileName in _resolvedPaths)
                WriteVerbose(fileName);
                string curExtension = System.IO.Path.GetExtension(fileName);
                if (!curExtension.Equals(".blg", StringComparison.OrdinalIgnoreCase)
                    && !curExtension.Equals(".csv", StringComparison.OrdinalIgnoreCase)
                    && !curExtension.Equals(".tsv", StringComparison.OrdinalIgnoreCase))
                    string msg = string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("CounterNotALogFile"), fileName);
                    ThrowTerminatingError(new ErrorRecord(exc, "CounterNotALogFile", ErrorCategory.InvalidResult, null));
                if (!curExtension.Equals(firstExt, StringComparison.OrdinalIgnoreCase))
                    string msg = string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("CounterNoMixedLogTypes"), fileName);
                    ThrowTerminatingError(new ErrorRecord(exc, "CounterNoMixedLogTypes", ErrorCategory.InvalidResult, null));
            if (firstExt.Equals(".blg", StringComparison.OrdinalIgnoreCase))
                if (_resolvedPaths.Count > 32)
                    string msg = string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("Counter32FileLimit"));
                    ThrowTerminatingError(new ErrorRecord(exc, "Counter32FileLimit", ErrorCategory.InvalidResult, null));
                string msg = string.Format(CultureInfo.InvariantCulture, _resourceMgr.GetString("Counter1FileLimit"));
                ThrowTerminatingError(new ErrorRecord(exc, "Counter1FileLimit", ErrorCategory.InvalidResult, null));
        // ResolveFilePath helper.
        private bool ResolveFilePaths()
            StringCollection retColl = new StringCollection();
            foreach (string origPath in _accumulatedFileNames)
                    resolvedPathSubset = SessionState.Path.GetResolvedPSPathFromPSPath(origPath);
                    WriteError(new ErrorRecord(notSupported, string.Empty, ErrorCategory.ObjectNotFound, origPath));
                    WriteError(new ErrorRecord(driveNotFound, string.Empty, ErrorCategory.ObjectNotFound, origPath));
                    WriteError(new ErrorRecord(providerNotFound, string.Empty, ErrorCategory.ObjectNotFound, origPath));
                    WriteError(new ErrorRecord(pathNotFound, string.Empty, ErrorCategory.ObjectNotFound, origPath));
                    WriteError(new ErrorRecord(exc, string.Empty, ErrorCategory.ObjectNotFound, origPath));
                        Exception exc = new Exception(string.Format(CultureInfo.InvariantCulture, msg, origPath));
                        WriteError(new ErrorRecord(exc, "NotAFileSystemPath", ErrorCategory.InvalidArgument, origPath));
                    _resolvedPaths.Add(pi.ProviderPath.ToLowerInvariant());
            return (_resolvedPaths.Count > 0);
        // The only exception is the first set, where we allow for the formatted value to be 0 -
        // this is expected for CSV and TSV files.
        private void WriteSampleSetObject(PerformanceCounterSampleSet set, bool firstSet)
            if (!firstSet)
