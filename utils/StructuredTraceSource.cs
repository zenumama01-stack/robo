    #region PSTraceSourceOptions
    /// These flags enable tracing based on the types of
    /// a tracing supplied. Each type of tracing will allow
    /// for one or more methods in the StructuredTraceSource class to become
    /// "enabled".
    public enum PSTraceSourceOptions
        /// All tracing off.
        /// No tracing is enabled
        /// Constructors will be traced.
        /// The TraceConstructor methods are enabled
        Constructor = 0x00000001,
        /// Dispose will be traced.
        /// The TraceDispose methods are enabled
        Dispose = 0x00000002,
        /// Finalize will be traced.
        /// The TraceFinalizer methods are enabled
        Finalizer = 0x00000004,
        /// Methods will be traced.
        /// The TraceMethod methods are enabled
        Method = 0x00000008,
        /// Properties will be traced.
        /// The TraceProperty methods are enabled
        Property = 0x00000010,
        /// Delegates will be traced.
        /// The TraceDelegate and TraceDelegateHandler methods are enabled
        Delegates = 0x00000020,
        /// Events will be traced.
        /// The TraceRaiseEvent and TraceEventHandler methods are enabled
        Events = 0x00000040,
        /// Exceptions will be traced.
        /// The TraceException method is enabled
        Exception = 0x00000080,
        /// Locks will be traced.
        /// The TraceLock methods are enabled
        Lock = 0x00000100,
        /// Errors will be traced.
        /// The TraceError methods are enabled
        Error = 0x00000200,
        /// Warnings will be traced.
        /// The TraceWarning methods are enabled
        Warning = 0x00000400,
        /// Verbose messages will be traced.
        Verbose = 0x00000800,
        /// WriteLines will be traced.
        /// The WriteLine methods are enabled
        WriteLine = 0x00001000,
        /// TraceScope calls will be traced.
        Scope = 0x00002000,
        /// Assertions will be traced.
        Assert = 0x00004000,
        /// A combination of flags that trace the execution flow.
        /// The methods associated with the flags; Constructor, Dispose,
        /// Finalizer, Method, Delegates, and Events will be enabled.
        ExecutionFlow =
            Constructor |
            Dispose |
            Finalizer |
            Method |
            Delegates |
            Events |
            Scope,
        /// A combination of flags that trace the data.
        /// Finalizer, Property, and WriteLine will be enabled.
        Data =
            Property |
            Verbose |
            WriteLine,
        /// A combination of flags that trace the errors.
        /// The methods associated with the flags; Error,
        /// and Exception will be enabled.
        Errors =
            Error |
        /// All combination of trace flags will be set.
        /// All methods for tracing will be enabled.
        All =
            Exception |
            Warning |
            Lock |
            WriteLine |
            Scope |
            Assert
    #endregion PSTraceSourceOptions
    /// The StructuredTraceSource class is derived from TraceSource to provide granular
    /// control over the tracing in a program.  An instance of StructuredTraceSource
    /// To get an instance of this class a user should define a static
    /// field of the type StructuredTraceSource, and assign the results of GetTracer() to it.
    /// If the category should be automatically put in the application config file the
    /// field should be decorated with the TraceSourceAttribute so that GenerateAppConfigFile.exe
    /// can find it through reflection.
    /// [TraceSourceAttribute("category", "description")]
    /// public static StructuredTraceSource tracer = GetTracer("category", "description", true);
    /// this class should throw no exceptions. Any call to a StructuredTraceSource method
        #region PSTraceSource construction methods
        /// Constructor that determines the name of the trace
        /// flag in the config file.
        /// <param name="fullName">
        /// The full name for the trace category. This is different from the name parameter as
        /// it is not limited to 16 characters.
        /// will control the tracing for. This parameter must always be 16 characters to ensure
        /// proper formatting of the output.
        internal PSTraceSource(string fullName, string name, string description, bool traceHeaders)
            ArgumentException.ThrowIfNullOrEmpty(fullName);
                FullName = fullName;
                // TODO: move this to startup json file instead of using env var
                string tracingEnvVar = Environment.GetEnvironmentVariable("MshEnableTrace");
                        tracingEnvVar,
                        "True",
                    string options = this.TraceSource.Attributes["Options"];
                        _flags = (PSTraceSourceOptions)Enum.Parse(typeof(PSTraceSourceOptions), options, true);
                ShowHeaders = traceHeaders;
                // This exception occurs when the config
                // file is malformed. Just default to Off.
                _flags = PSTraceSourceOptions.None;
            catch (System.Configuration.ConfigurationException)
        private static bool globalTraceInitialized;
        /// Traces the app domain header with information about the execution
        /// time, the platform, etc.
        internal void TraceGlobalAppDomainHeader()
            // Only trace the global header if it hasn't
            // already been traced
            if (globalTraceInitialized)
            // AppDomain
            OutputLine(
                PSTraceSourceOptions.All,
                "Initializing tracing for AppDomain: {0}",
                AppDomain.CurrentDomain.FriendlyName);
            // Current time
                "\tCurrent time: {0}",
                DateTime.Now.ToString());
            // OS build
                "\tOS Build: {0}",
                Environment.OSVersion.ToString());
            // .NET Framework version
                "\tFramework Build: {0}\n",
                Environment.Version.ToString());
            // Mark that we have traced the global header
            globalTraceInitialized = true;
        /// Outputs a header when a new StructuredTraceSource object is created.
        /// <param name="callingAssembly">
        /// The assembly that created the instance of the StructuredTraceSource.
        /// A header will be output that contains information such as;
        /// the category and description of the new trace object,
        /// the assembly in which the new trace object
        /// will be stored.
        internal void TracerObjectHeader(
            Assembly callingAssembly)
            if (_flags == PSTraceSourceOptions.None)
            // Write the header for the new trace object
            OutputLine(PSTraceSourceOptions.All, "Creating tracer:");
            // Category
                "\tCategory: {0}",
            if (callingAssembly != null)
                // Assembly name
                    "\tAssembly: {0}",
                    callingAssembly.FullName);
                // Assembly location
                    "\tAssembly Location: {0}",
                    callingAssembly.Location);
                // Assembly File timestamp
                FileInfo assemblyFileInfo =
                    new FileInfo(callingAssembly.Location);
                    "\tAssembly File Timestamp: {0}",
                    assemblyFileInfo.CreationTime.ToString());
            StringBuilder flagBuilder = new StringBuilder();
            // Label
            flagBuilder.Append("\tFlags: ");
            flagBuilder.Append(_flags.ToString());
            // Write out the flags
            OutputLine(PSTraceSourceOptions.All, flagBuilder.ToString());
        #endregion StructuredTraceSource constructor methods
        #region PSTraceSourceOptions.Scope
        internal IDisposable TraceScope(string msg)
            if (_flags.HasFlag(PSTraceSourceOptions.Scope))
                    return new ScopeTracer(this, PSTraceSourceOptions.Scope, null, null, string.Empty, msg);
        internal IDisposable TraceScope(string format, object arg1)
                    return new ScopeTracer(this, PSTraceSourceOptions.Scope, null, null, string.Empty, format, arg1);
        internal IDisposable TraceScope(string format, object arg1, object arg2)
                    return new ScopeTracer(this, PSTraceSourceOptions.Scope, null, null, string.Empty, format, arg1, arg2);
        #endregion PSTraceSourceOptions.Scope
        #region PSTraceSourceOptions.Method methods/helpers
        /// Traces the method name and indents the trace output.
        /// <param name="format">
        /// The format string for additional arguments to be traced
        /// The additional arguments given to the format string
        /// An object that supports IDisposable. The caller
        /// should dispose of the object when it goes out of
        /// public void MethodName(int count)
        ///     using (TraceMethod(
        ///                "count={0:d}",
        ///                count))
        ///         // do something here...
        /// This will produce output similar to the following:
        /// Entering MethodName: count=4
        ///     other trace output indented
        /// Leaving MethodName
        internal IDisposable TraceMethod(
            string format,
            if (_flags.HasFlag(PSTraceSourceOptions.Method))
                    // Get the name of the method that called this method
                    // 1, signifies the caller of this method, whereas 2
                    // would signify the caller of that method.
                    string methodName = GetCallingMethodNameAndParameters(1);
                    // Create the method tracer object
                    return (IDisposable)new ScopeTracer(
                        PSTraceSourceOptions.Method,
                        methodOutputFormatter,
                        methodLeavingFormatter,
                        methodName,
                    // Eat all exceptions
                    // Do not assert here because exceptions can be
                    // raised while a thread is shutting down during
                    // normal operation.
        #endregion PSTraceSourceOptions.Method methods/helpers
        #region PSTraceSourceOptions.Events methods/helpers
        /// Traces the entrance and exit from event handlers.
        internal IDisposable TraceEventHandlers()
            if (_flags.HasFlag(PSTraceSourceOptions.Events))
                    // Create the scope tracer object
                        PSTraceSourceOptions.Events,
                        eventHandlerOutputFormatter,
                        eventHandlerLeavingFormatter,
        internal IDisposable TraceEventHandlers(
        #endregion PSTraceSourceOptions.Events methods/helpers
        #region PSTraceSourceOptions.Lock methods/helpers
        /// Traces the user specified lock name and indents the trace output.
        /// public void MethodName()
        ///     lock (this)
        ///         using (TraceLock("my lock name"))
        ///             // do something here...
        /// Entering Lock: my lock name
        /// Leaving Lock: my lock name
        internal IDisposable TraceLock(string lockName)
            if (_flags.HasFlag(PSTraceSourceOptions.Lock))
                        PSTraceSourceOptions.Lock,
                        lockEnterFormatter,
                        lockLeavingFormatter,
                        lockName);
        /// Call this before acquiring a lock.
        /// <param name="lockName">
        /// User defined name given to the lock
        internal void TraceLockAcquiring(string lockName)
                TraceLockHelper(
                    lockAcquiringFormatter,
        /// Call this after acquiring a lock.
        /// Use this only if the TraceLock that returns
        /// an IDisposable won't work in your situation.
        /// You will not get automatic indentation or
        /// release tracing of the lock.
        internal void TraceLockAcquired(string lockName)
        /// Call this after releasing the lock, but only
        /// if you called TraceLockAcquired when you acquired
        /// the lock.
        internal void TraceLockReleased(string lockName)
        /// A helper to simplify tracing of the lock flags.
        /// <param name="formatter">
        /// A format string for the output.
        /// User defined name for the lock
        private void TraceLockHelper(
            string formatter,
            string lockName)
                    formatter,
        #endregion PSTraceSourceOptions.Lock methods/helpers
        #region PSTraceSourceOptions.Error,Warning,Normal methods/helpers
        /// Traces the specified formatted output when PSTraceSourceOptions.Error
        /// <param name="errorMessageFormat">
        /// The format string containing the error message
        /// The arguments for the format string
        internal void TraceError(
            string errorMessageFormat,
            if (_flags.HasFlag(PSTraceSourceOptions.Error))
                FormatOutputLine(
                    PSTraceSourceOptions.Error,
                    errorFormatter,
        /// Traces the specified formatted output when PSTraceSourceOptions.Warning
        /// <param name="warningMessageFormat">
        internal void TraceWarning(
            string warningMessageFormat,
            if (_flags.HasFlag(PSTraceSourceOptions.Warning))
                    PSTraceSourceOptions.Warning,
                    warningFormatter,
                    warningMessageFormat,
        /// Traces the specified formatted output when PSTraceSourceOptions.Verbose
        /// <param name="verboseMessageFormat">
        internal void TraceVerbose(
            string verboseMessageFormat,
            if (_flags.HasFlag(PSTraceSourceOptions.Verbose))
                    PSTraceSourceOptions.Verbose,
                    verboseFormatter,
                    verboseMessageFormat,
        /// Traces the formatted output when PSTraceSourceOptions.WriteLine is enabled.
        /// The format string
        internal void WriteLine(string format)
            if (_flags.HasFlag(PSTraceSourceOptions.WriteLine))
                    PSTraceSourceOptions.WriteLine,
                    writeLineFormatter,
                    Array.Empty<object>());
        /// <param name="format">The format string.</param>
        /// <param name="arg1"></param>
        internal void WriteLine(string format, object arg1)
                    new object[] { arg1 });
        internal void WriteLine(string format, bool arg1)
            WriteLine(format, (object)arg1.ToString());
        internal void WriteLine(string format, byte arg1)
        internal void WriteLine(string format, char arg1)
        internal void WriteLine(string format, decimal arg1)
        internal void WriteLine(string format, double arg1)
        internal void WriteLine(string format, float arg1)
        internal void WriteLine(string format, int arg1)
        internal void WriteLine(string format, long arg1)
        internal void WriteLine(string format, uint arg1)
        internal void WriteLine(string format, ulong arg1)
        /// <param name="arg2"></param>
        internal void WriteLine(string format, object arg1, object arg2)
                    new object[] { arg1, arg2 });
        /// <param name="arg3"></param>
        internal void WriteLine(string format, object arg1, object arg2, object arg3)
                    new object[] { arg1, arg2, arg3 });
        /// <param name="arg4"></param>
        internal void WriteLine(string format, object arg1, object arg2, object arg3, object arg4)
                    new object[] { arg1, arg2, arg3, arg4 });
        /// <param name="arg5"></param>
        internal void WriteLine(string format, object arg1, object arg2, object arg3, object arg4, object arg5)
                    new object[] { arg1, arg2, arg3, arg4, arg5 });
        /// <param name="arg6"></param>
        internal void WriteLine(string format, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
                    new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
        /// The object to be output
        internal void WriteLine(object arg)
                WriteLine("{0}", arg == null ? "null" : arg.ToString());
        /// Formats the specified text and then traces it.
        /// <param name="flag">
        /// The flag that met the criteria to have this line traced.
        /// <param name="classFormatter">
        /// This is the trace class formatter. For instance,
        /// TraceError has a formatter like "ERROR: {0}".
        /// Additional format string.
        /// Arguments for the additional format string
        private void FormatOutputLine(
            PSTraceSourceOptions flag,
            string classFormatter,
                // First format the class format string and the
                // user provided format string together
                if (classFormatter != null)
                    output.Append(classFormatter);
                if (format != null)
                // finally trace the output
                OutputLine(flag, output.ToString());
        #endregion PSTraceSourceOptions.Error methods/helpers
        #region Class helper methods and properties
        /// Gets the method name of the method that called this one
        /// plus the skipFrames.
        /// For instance, GetCallingMethodNameAndParameters(1)
        /// will return the method that called the method that is calling
        /// GetCallingMethodNameAndParameters.
        /// <param name="skipFrames">
        /// The number of frames to skip in the calling stack.
        /// The name of the method on the stack.
        private static string GetCallingMethodNameAndParameters(int skipFrames)
            StringBuilder methodAndParameters = null;
                // Use the stack to get the method and type information
                // for the calling method
                StackFrame stackFrame = new StackFrame(++skipFrames);
                MethodBase callingMethod = stackFrame.GetMethod();
                Type declaringType = callingMethod.DeclaringType;
                // Append the class name and method name together
                methodAndParameters = new StringBuilder();
                // Note: don't use the FullName for the declaringType
                // as it is usually way too long and makes the trace
                // output hard to read.
                methodAndParameters.AppendFormat(
                    "{0}.{1}(",
                    declaringType.Name,
                    callingMethod.Name);
                methodAndParameters.Append(')');
            return methodAndParameters.ToString();
        // The default formatter for TraceError
        private const string errorFormatter =
            "ERROR: ";
        // The default formatter for TraceWarning
        private const string warningFormatter =
            "Warning: ";
        // The default formatter for TraceVerbose
        private const string verboseFormatter =
            "Verbose: ";
        // The default formatter for WriteLine
        private const string writeLineFormatter =
            "";
        // The default formatter for TraceConstructor
        private const string constructorOutputFormatter =
            "Enter Ctor {0}";
        private const string constructorLeavingFormatter =
            "Leave Ctor {0}";
        // The default formatter for TraceDispose
        private const string disposeOutputFormatter =
            "Enter Disposer {0}";
        private const string disposeLeavingFormatter =
            "Leave Disposer {0}";
        // The default formatter for TraceMethod
        private const string methodOutputFormatter =
            "Enter {0}:";
        private const string methodLeavingFormatter =
            "Leave {0}";
        // The default formatter for TraceProperty
        private const string propertyOutputFormatter =
            "Enter property {0}:";
        private const string propertyLeavingFormatter =
            "Leave property {0}";
        // The default formatter for TraceDelegateHandler
        private const string delegateHandlerOutputFormatter =
            "Enter delegate handler: {0}:";
        private const string delegateHandlerLeavingFormatter =
            "Leave delegate handler: {0}";
        // The default formatter for TraceEventHandlers
        private const string eventHandlerOutputFormatter =
            "Enter event handler: {0}:";
        private const string eventHandlerLeavingFormatter =
            "Leave event handler: {0}";
        // The default formatters for TraceException
        private const string exceptionOutputFormatter =
            "{0}: {1}\n{2}";
        private const string innermostExceptionOutputFormatter =
            "Inner-most {0}: {1}\n{2}";
        // The default formatters for TraceLock
        private const string lockEnterFormatter =
            "Enter Lock: {0}";
        private const string lockLeavingFormatter =
            "Leave Lock: {0}";
        private const string lockAcquiringFormatter =
            "Acquiring Lock: {0}";
        private static StringBuilder GetLinePrefix(PSTraceSourceOptions flag)
            StringBuilder prefixBuilder = new StringBuilder();
            // Add the flag that caused this line to be traced
            prefixBuilder.AppendFormat(
                " {0,-11} ",
                Enum.GetName(typeof(PSTraceSourceOptions), flag));
            return prefixBuilder;
        private static void AddTab(StringBuilder lineBuilder)
            // The Trace.IndentSize does not change at all
            // through the running of the process so there
            // are no thread issues here.
            int indentSize = Trace.IndentSize;
            int threadIndentLevel = ThreadIndentLevel;
            lineBuilder.Append(System.Management.Automation.Internal.StringUtil.Padding(indentSize * threadIndentLevel));
        // used to find and blocks cyclic-loops in tracing.
        private bool _alreadyTracing = false;
        /// Composes a line of trace output and then writes it.
        /// The flag that caused the line to be traced.
        /// The string to write with format symbols if necessary.
        /// Arguments to the format string.
        /// The line is composed by prefixing the process name, thread ID,
        /// and tick count. Then the indenting is added. Then the
        /// specified string is formatted. Finally the finished string
        /// is output using the Trace class.
        internal void OutputLine(
            string arg = null)
            // if already tracing something for this current TraceSource,
            // dont trace again. This will block cyclic-loops from happening.
            if (_alreadyTracing)
            _alreadyTracing = true;
                    format != null,
                    "The format string should not be null");
                StringBuilder lineBuilder = new StringBuilder();
                if (ShowHeaders)
                    // Get the line prefix string which includes things
                    // like App name, clock tick, thread ID, etc.
                    lineBuilder.Append(GetLinePrefix(flag));
                // Add the spaces for the indent
                AddTab(lineBuilder);
                    lineBuilder.AppendFormat(
                        arg);
                    lineBuilder.Append(format);
                this.TraceSource.TraceInformation(lineBuilder.ToString());
                // reset tracing for the current trace source..
                // so future traces can go through.
                _alreadyTracing = false;
        /// Property to access the indent level in thread local storage.
        internal static int ThreadIndentLevel
                // The first time access the ThreadLocal instance, the default int value will be used
                // to initialize the instance. The default int value is 0.
                return s_localIndentLevel.Value;
                if (value >= 0)
                    // Set the new indent level in thread local storage
                    s_localIndentLevel.Value = value;
                    Diagnostics.Assert(value >= 0, "The indention value cannot be less than zero");
        /// Allocates some thread local storage to hold the indent level.
        private static readonly ThreadLocal<int> s_localIndentLevel = new ThreadLocal<int>();
        /// Local storage for the trace switch flags.
        private PSTraceSourceOptions _flags = PSTraceSourceOptions.None;
        /// Gets or sets the description for this trace sources.
        /// Determines if the line and switch headers should be shown.
        internal bool ShowHeaders { get; set; } = true;
        /// Gets the full name of the trace source category.
        internal string FullName { get; } = string.Empty;
        /// Creates an instance of the TraceSource on demand.
        internal TraceSource TraceSource
            get { return _traceSource ??= new MonadTraceSource(_name); }
        private TraceSource _traceSource;
        #endregion Class helper methods and properties
        /// Gets or sets the options for what will be traced.
        public PSTraceSourceOptions Options
                return _flags;
                _flags = value;
                this.TraceSource.Switch.Level = (SourceLevels)_flags;
        internal bool IsEnabled
            get { return _flags != PSTraceSourceOptions.None; }
        /// Gets the attributes of the TraceSource.
        public StringDictionary Attributes
                return TraceSource.Attributes;
        /// Gets the listeners for the TraceSource.
        public TraceListenerCollection Listeners
                return TraceSource.Listeners;
        /// Gets the TraceSource name (also known as category).
        /// Note, this name is truncated to 16 characters due to limitations
        /// in the TraceSource class.
        /// Gets or sets the TraceSource's Switch.
        public SourceSwitch Switch
                return TraceSource.Switch;
                TraceSource.Switch = value;
        #region TraceCatalog
        /// Storage for all the PSTraceSource instances.
        internal static Dictionary<string, PSTraceSource> TraceCatalog { get; } = new Dictionary<string, PSTraceSource>(StringComparer.OrdinalIgnoreCase);
        /// Storage for trace source instances which have not been instantiated but for which
        /// the user has specified Options.
        /// If the PSTraceSource cannot be found in the TraceCatalog, the same name is used
        /// to look in this dictionary to see if the PSTraceSource has been pre-configured.
        internal static Dictionary<string, PSTraceSource> PreConfiguredTraceSource { get; } = new Dictionary<string, PSTraceSource>(StringComparer.OrdinalIgnoreCase);
        #endregion TraceCatalog
    #region ScopeTracer object/helpers
    /// A light-weight object to manage the indention of
    /// trace output for each thread.
    /// An instance of this object is returned when any scoping
    /// Trace method (like TraceMethod, TraceProperty, etc.)
    /// is called. In the constructor to the object the indention
    /// level for the thread is incremented.
    /// The Dispose method will decrement the thread indent level.
    internal class ScopeTracer : IDisposable
        /// Constructor that traces the scope name
        /// and raises the indent level in thread
        /// local storage.
        /// <param name="tracer">
        /// The trace object that is to be used for output
        /// The PSTraceSourceOptions that is causing the scope object to
        /// be created.
        /// <param name="scopeOutputFormatter">
        /// This format string is used to determine the
        /// general output format for the scope. For instance,
        /// TraceMethod would probably provide a formatter similar
        /// to "Entering: {0}: {1}" where {0} is the name of the
        /// method and {1} is the additional formatted info provided.
        /// <param name="leavingScopeFormatter">
        /// The format string used to determine the general output
        /// format for the scope when the Dispose method is called.
        /// <param name="scopeName">
        /// The name of the scope that is being traced
        internal ScopeTracer(
            PSTraceSource tracer,
            string scopeOutputFormatter,
            string leavingScopeFormatter,
            string scopeName)
            _tracer = tracer;
            // Call the helper
            ScopeTracerHelper(
                flag,
                scopeOutputFormatter,
                leavingScopeFormatter,
                scopeName,
        /// The format of any additional arguments which will be appended
        /// to the line of trace output
            string scopeName,
        /// Helper for the ScopeTracer constructor.
        /// The flag that caused this line of tracing to be traced.
        /// The name of the scope being entered
        /// to the "Entering" line of trace output
        internal void ScopeTracerHelper(
            // Store the flags, scopeName, and the leavingScopeFormatter
            // so that it can be used in the Dispose method
            _flag = flag;
            _scopeName = scopeName;
            _leavingScopeFormatter = leavingScopeFormatter;
            // Format the string for output
            if (!string.IsNullOrEmpty(scopeOutputFormatter))
                    _scopeName);
            // Now write the trace
            _tracer.OutputLine(_flag, output.ToString());
            // Increment the current thread indent level
            PSTraceSource.ThreadIndentLevel++;
        /// Decrements the indent level in thread local
        /// storage and then traces the scope name.
            // Decrement the indent level in thread local storage
            PSTraceSource.ThreadIndentLevel--;
            // Trace out the scope name
            if (!string.IsNullOrEmpty(_leavingScopeFormatter))
                _tracer.OutputLine(_flag, _leavingScopeFormatter, _scopeName);
        /// The trace object that is used for any output.
        private readonly PSTraceSource _tracer;
        /// The flag which caused this scope object to be created.
        private PSTraceSourceOptions _flag;
        /// Stores the scope name that is passed to the constructor.
        private string _scopeName;
        /// Stores the format string used when formatting output when
        /// leaving the scope.
        private string _leavingScopeFormatter;
    #endregion ScopeTracer object/helpers
    #region PSTraceSourceAttribute
    /// This attribute is placed on the field of the PSTraceSource class
    /// in the class that is consuming the tracing methods defined in
    /// this file. It defines the trace category and description
    /// for that instance of PSTraceSource.
    /// This attribute is only allowed on fields and there can only
    /// be one for each instance. Only one instance of this attribute
    /// should be used in any one class.
    /// In order for the attribute to be used to help in constructing
    /// the PSTraceSource object, reflection is used to find the field
    /// that the PSTraceSource object will be assigned to. This attribute
    /// declares the category and description for the PSTraceSource object
    /// in that field.  Having multiple instances of this attribute on
    /// multiple fields in the same class will cause unexpected results.
    /// For instance, trace output for one category may actually be
    /// considered part of another category.
    [AttributeUsage(
         AttributeTargets.Field,
         AllowMultiple = false)]
    internal class TraceSourceAttribute : Attribute
        /// Constructor for the TraceSourceAttribute class.
        /// <param name="category">
        /// The name of the category for which the TraceSource instance
        /// A description for the category.
        internal TraceSourceAttribute(
            string category,
        /// The category to be used for the TraceSource.
        internal string Category { get; }
        /// The description for the category to be used for the TraceSource.
        internal string Description { get; set; }
    #endregion TraceSourceAttribute
    #region MonadTraceSource
    /// This derived class of TraceSource is required so that we can tell
    /// the configuration infrastructure which attributes are supported in
    /// the XML app-config file for our trace source.
    internal class MonadTraceSource : TraceSource
        internal MonadTraceSource(string name)
        /// Tells the config infrastructure which attributes are supported
        /// for our TraceSource.
        /// A string array with the names of the attributes supported by our
        /// trace source.
        protected override string[] GetSupportedAttributes()
            return new string[] { "Options" };
    #endregion MonadTraceSource
