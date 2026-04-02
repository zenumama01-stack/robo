    /// Corresponds to -OutputVariable, -ErrorVariable, -WarningVariable, and -InformationVariable.
    internal enum VariableStreamKind
    /// Pipe provides a way to stitch two commands.
    /// The Pipe class is not thread-safe, so methods such as
    /// AddItems and Retrieve should not be called simultaneously.
    /// ExternalReader and ExternalWriter can provide thread-safe buffering.
    internal class Pipe
        // If a pipeline object has been added, then
        // write objects to it, stepping one at a time...
        internal PipelineProcessor PipelineProcessor { get; }
        /// This is the downstream cmdlet in the "streamlet model"
        /// which is invoked during each call to Add/AddItems.
        internal CommandProcessorBase DownstreamCmdlet
                return _downstreamCmdlet;
                Diagnostics.Assert(_resultList == null, "Tried to set downstream cmdlet when _resultList not null");
                _downstreamCmdlet = value;
        private CommandProcessorBase _downstreamCmdlet;
        /// This is the upstream external object source.  If this is set,
        /// Retrieve() will attempt to read objects from the upstream source
        /// before indicating that the pipe is empty.
        /// It is improper to change this once the pipeline has started
        /// executing, although the checks for this are in the
        /// PipelineProcessor class and not here.
        internal PipelineReader<object> ExternalReader { get; set; }
        /// This is the downstream object recipient.  If this is set,
        /// Add() and AddItems() write to this recipient instead of
        /// to the internal queue.  This also disables the
        /// DownstreamCmdlet.
        internal PipelineWriter ExternalWriter
                return _externalWriter;
                Diagnostics.Assert(_resultList == null, "Tried to set Pipe ExternalWriter when resultList not null");
                _externalWriter = value;
        private PipelineWriter _externalWriter;
            if (_downstreamCmdlet != null)
                return _downstreamCmdlet.ToString();
        /// OutBufferCount configures the number of objects to buffer before calling the downstream Cmdlet.
        internal int OutBufferCount { get; set; } = 0;
        /// Gets whether the out variable list should be ignored.
        /// This is used for scenarios like the `clean` block, where writing to output stream is intentionally
        /// disabled and thus out variables should also be ignored.
        internal bool IgnoreOutVariableList { get; set; }
        /// If true, then all input added to this pipe will simply be discarded...
        internal bool NullPipe
                return _nullPipe;
                _isRedirected = true;
                _nullPipe = value;
        private bool _nullPipe;
        /// A queue that is shared between commands on either side of the pipe to transfer objects.
        internal Queue<object> ObjectQueue { get; }
        /// True if there are items in this pipe that need processing...
        /// This does not take into account the presence of ExternalInput;
        /// it only indicates whether there is currently any data queued up
        /// or if there is data in the enumerator...
        internal bool Empty
                if (_enumeratorToProcess != null)
                    return _enumeratorToProcessIsEmpty;
                if (ObjectQueue != null)
                    return ObjectQueue.Count == 0;
        /// Is true if there is someone consuming this pipe already, either through
        /// a Pipe object that processes it's output or there is downstream cmdlet...
        internal bool IsRedirected
            get { return _downstreamCmdlet != null || _isRedirected; }
        private bool _isRedirected;
        /// If non-null, output written to the pipe are also added to this list.
        private List<IList> _outVariableList;
        /// If non-null, errors written to the pipe are also added to this list.
        private List<IList> _errorVariableList;
        /// If non-null, warnings written to the pipe are also added to this list.
        private List<IList> _warningVariableList;
        /// If non-null, information objects written to the pipe are also added to this list.
        private List<IList> _informationVariableList;
        /// If non-null, the current object being written to the pipe is stored in
        /// this variable.
        private PSVariable _pipelineVariableObject;
        private static void AddToVarList(List<IList> varList, object obj)
            if (varList != null && varList.Count > 0)
                for (int i = 0; i < varList.Count; i++)
                    varList[i].Add(obj);
        internal void AppendVariableList(VariableStreamKind kind, object obj)
                case VariableStreamKind.Error:
                    AddToVarList(_errorVariableList, obj);
                case VariableStreamKind.Warning:
                    AddToVarList(_warningVariableList, obj);
                case VariableStreamKind.Output:
                    AddToVarList(_outVariableList, obj);
                case VariableStreamKind.Information:
                    AddToVarList(_informationVariableList, obj);
        internal void AddVariableList(VariableStreamKind kind, IList list)
                    _errorVariableList ??= new List<IList>();
                    _errorVariableList.Add(list);
                    _warningVariableList ??= new List<IList>();
                    _warningVariableList.Add(list);
                    _outVariableList ??= new List<IList>();
                    _outVariableList.Add(list);
                    _informationVariableList ??= new List<IList>();
                    _informationVariableList.Add(list);
        internal void SetPipelineVariable(PSVariable pipelineVariable)
            _pipelineVariableObject = pipelineVariable;
        internal void RemoveVariableList(VariableStreamKind kind, IList list)
                    _errorVariableList.Remove(list);
                    _warningVariableList.Remove(list);
                    _outVariableList.Remove(list);
                    _informationVariableList.Remove(list);
            if (_pipelineVariableObject != null)
                _pipelineVariableObject.Value = null;
                _pipelineVariableObject = null;
        /// When a temporary pipe is used in the middle of execution, then we need to pass along
        /// the error and warning variable list to hold the errors and warnings get written out
        /// while the temporary pipe is being used.
        /// We don't need to pass along the out variable list because we don't care about the output
        /// generated in the middle of execution.
        internal void SetVariableListForTemporaryPipe(Pipe tempPipe)
            CopyVariableToTempPipe(VariableStreamKind.Error, _errorVariableList, tempPipe);
            CopyVariableToTempPipe(VariableStreamKind.Warning, _warningVariableList, tempPipe);
            CopyVariableToTempPipe(VariableStreamKind.Information, _informationVariableList, tempPipe);
        private static void CopyVariableToTempPipe(VariableStreamKind streamKind, List<IList> variableList, Pipe tempPipe)
            if (variableList != null && variableList.Count > 0)
                for (int i = 0; i < variableList.Count; i++)
                    tempPipe.AddVariableList(streamKind, variableList[i]);
        /// Default constructor - Creates the object queue.
        /// The initial Queue capacity is 1, but it will grow automatically.
        internal Pipe()
            ObjectQueue = new Queue<object>();
        /// This overload causes output to be written into a List.
        /// <param name="resultList"></param>
        internal Pipe(List<object> resultList)
            Diagnostics.Assert(resultList != null, "resultList cannot be null");
            _resultList = resultList;
        private readonly List<object> _resultList;
        /// This overload causes output to be
        /// written onto an Collection[PSObject] which is more useful
        /// in many circumstances than arraylist.
        /// <param name="resultCollection">The collection to write into.</param>
        internal Pipe(System.Collections.ObjectModel.Collection<PSObject> resultCollection)
            Diagnostics.Assert(resultCollection != null, "resultCollection cannot be null");
            _resultCollection = resultCollection;
        private readonly System.Collections.ObjectModel.Collection<PSObject> _resultCollection;
        /// This pipe writes into another pipeline processor allowing
        /// pipelines to be chained together...
        /// <param name="context">The execution context object for this engine instance.</param>
        /// <param name="outputPipeline">The pipeline to write into...</param>
        internal Pipe(ExecutionContext context, PipelineProcessor outputPipeline)
            Diagnostics.Assert(outputPipeline != null, "outputPipeline cannot be null");
            Diagnostics.Assert(outputPipeline != null, "context cannot be null");
            PipelineProcessor = outputPipeline;
        /// Read from an enumerator instead of a pipeline reader...
        /// <param name="enumeratorToProcess">The enumerator to process...</param>
        internal Pipe(IEnumerator enumeratorToProcess)
            Diagnostics.Assert(enumeratorToProcess != null, "enumeratorToProcess cannot be null");
            _enumeratorToProcess = enumeratorToProcess;
            // since there is an enumerator specified, we
            // assume that there is some stuff to read
            _enumeratorToProcessIsEmpty = false;
        private readonly IEnumerator _enumeratorToProcess;
        private bool _enumeratorToProcessIsEmpty;
        /// Writes an object to the pipe.  This could recursively call to the
        /// downstream cmdlet, or write the object to the external output.
        /// <param name="obj">The object to add to the pipe.</param>
        /// AutomationNull.Value is ignored
        /// <exception cref="PipelineClosedException">
        /// The ExternalWriter stream is closed
        internal void Add(object obj)
            if (obj == AutomationNull.Value)
            // OutVariable is appended for null pipes so that the following works:
            //     foo -OutVariable bar > $null
            if (_nullPipe)
            // Store the current pipeline variable
                _pipelineVariableObject.Value = obj;
            AddToPipe(obj);
        internal void AddWithoutAppendingOutVarList(object obj)
            if (obj == AutomationNull.Value || _nullPipe)
        private void AddToPipe(object obj)
            if (PipelineProcessor != null)
                // Put the pipeline on the notification stack for stop.
                _context.PushPipelineProcessor(PipelineProcessor);
                PipelineProcessor.Step(obj);
                _context.PopPipelineProcessor(false);
            else if (_resultCollection != null)
                _resultCollection.Add(obj != null ? PSObject.AsPSObject(obj) : null);
            else if (_resultList != null)
                _resultList.Add(obj);
            else if (_externalWriter != null)
                _externalWriter.Write(obj);
            else if (ObjectQueue != null)
                ObjectQueue.Enqueue(obj);
                // This is the "streamlet" recursive call
                if (_downstreamCmdlet != null && ObjectQueue.Count > OutBufferCount)
                    _downstreamCmdlet.DoExecute();
        /// Writes a set of objects to the pipe.  This could recursively
        /// call to the downstream cmdlet, or write the objects to the
        /// external output.
        /// <param name="objects">
        /// Each of the objects are added to the pipe
        /// The pipeline has already been stopped,
        /// or a terminating error occurred in a downstream cmdlet.
        internal void AddItems(object objects)
            // Use the extended type system to try and get an enumerator for the object being added.
            // If we get an enumerator, then add the individual elements. If the object isn't
            // enumerable (i.e. the call returned null) then add the object to the pipe
            // as a single element.
            IEnumerator ie = LanguagePrimitives.GetEnumerator(objects);
                if (ie == null)
                    Add(objects);
                    while (ParserOps.MoveNext(_context, null, ie))
                        object o = ParserOps.Current(null, ie);
                        // Slip over any instance of AutomationNull.Value in the pipeline...
                        if (o == AutomationNull.Value)
                        Add(o);
                // If our object came from GetEnumerator (and hence is not IEnumerator), then we need to dispose
                // Otherwise, we don't own the object, so don't dispose.
                var disposable = ie as IDisposable;
                if (disposable != null && objects is not IEnumerator)
                    disposable.Dispose();
            if (_externalWriter != null)
            // If there are objects waiting for the downstream command
            // call it now
            if (_downstreamCmdlet != null && ObjectQueue != null && ObjectQueue.Count > OutBufferCount)
        /// Returns an object from the pipe. If pipe is empty returns null.
        /// This will try the ExternalReader if there are no queued objects.
        /// object that is retrieved, or AutomationNull.Value if none
        internal object Retrieve()
            if (ObjectQueue != null && ObjectQueue.Count != 0)
                return ObjectQueue.Dequeue();
            else if (_enumeratorToProcess != null)
                if (_enumeratorToProcessIsEmpty)
                    if (!ParserOps.MoveNext(_context, errorPosition: null, _enumeratorToProcess))
                        _enumeratorToProcessIsEmpty = true;
                    object retValue = ParserOps.Current(errorPosition: null, _enumeratorToProcess);
                    if (retValue == AutomationNull.Value)
                        // 'AutomationNull.Value' from the enumerator won't be sent to the pipeline.
                        // We try to get the next value in this case.
            else if (ExternalReader != null)
                    object o = ExternalReader.Read();
                    if (AutomationNull.Value == o)
                        // NOTICE-2004/06/08-JonN 963367
                        // The fix to this bug involves making one last
                        // attempt to read from the pipeline in DoComplete.
                        // We should be sure to not hit the ExternalReader
                        // again if it already reported completion.
                        ExternalReader = null;
        /// Removes all the objects from the Pipe.
        internal void Clear() => ObjectQueue?.Clear();
        /// Returns the currently queued items in the pipe.  Note that this will
        /// not block on ExternalInput, and it does not modify the contents of
        /// the pipe.
        /// <returns>Possibly empty array of objects, but not null.</returns>
        internal object[] ToArray()
            if (ObjectQueue == null || ObjectQueue.Count == 0)
                return MshCommandRuntime.StaticEmptyArray;
            return ObjectQueue.ToArray();
