    /// LogContext is the class to keep track of context information for each
    /// event to be logged.
    /// LogContext info is collected by Msh Log Engine and passed on to log provider
    internal class LogContext
        #region Context Properties
        internal string Severity { get; set; } = string.Empty;
        /// Name of the host.
        internal string HostName { get; set; } = string.Empty;
        /// Name of the host application.
        internal string HostApplication
        /// Version of the host.
        internal string HostVersion { get; set; } = string.Empty;
        /// Id of the host that is hosting current monad engine.
        internal string HostId { get; set; } = string.Empty;
        /// Version of monad engine.
        internal string EngineVersion { get; set; } = string.Empty;
        /// Id for currently running runspace.
        internal string RunspaceId { get; set; } = string.Empty;
        /// PipelineId of current running pipeline.
        internal string PipelineId { get; set; } = string.Empty;
        /// Command text that is typed in from commandline.
        internal string CommandName { get; set; } = string.Empty;
        /// Type of the command, which can be Alias, CommandLet, Script, Application, etc.
        /// The value of this property is a usually conversion of CommandTypes enum into a string.
        internal string CommandType { get; set; } = string.Empty;
        /// Script file name if current command is executed as a result of script run.
        internal string ScriptName { get; set; } = string.Empty;
        /// Path to the command executable file.
        internal string CommandPath { get; set; } = string.Empty;
        /// Extension for the command executable file.
        internal string CommandLine { get; set; } = string.Empty;
        /// Sequence Id for the event to be logged.
        internal string SequenceNumber { get; set; } = string.Empty;
        /// Current user.
        internal string User { get; set; } = string.Empty;
        /// The user connected to the machine, if being done with
        /// PowerShell remoting.
        internal string ConnectedUser { get; set; }
        /// Event happening time.
        internal string Time { get; set; } = string.Empty;
        #region Shell Id
        /// This property should be filled in when logging api is called directly
        /// with LogContext (when ExecutionContext is not available).
        internal string ShellId { get; set; }
        #region Execution context
        /// Execution context is necessary for GetVariableValue.
