    /// Defines a PowerShell command / script object which can be used with
    /// <see cref="PowerShell"/> object.
    public sealed class PSCommand
        private PowerShell _owner;
        private CommandCollection _commands;
        private Command _currentCommand;
        /// Creates an empty PSCommand; a command or script must be added to this PSCommand before it can be executed.
        public PSCommand()
            Initialize(null, false, null);
        /// Internal copy constructor.
        /// <param name="commandToClone"></param>
        internal PSCommand(PSCommand commandToClone)
            _commands = new CommandCollection();
            foreach (Command command in commandToClone.Commands)
                Command clone = command.Clone();
                // Attach the cloned Command to this instance.
                _commands.Add(clone);
                _currentCommand = clone;
        /// Creates a PSCommand from the specified command.
        /// <param name="command">Command object to use.</param>
        internal PSCommand(Command command)
            _currentCommand = command;
            _commands.Add(_currentCommand);
        #region Command / Parameter Construction
        /// Add a command to construct a command pipeline.
        /// For example, to construct a command string "get-process | sort-object",
        ///         PSCommand command = new PSCommand("get-process").AddCommand("sort-object");
        /// A string representing the command.
        /// <exception cref="InvalidPowerShellStateException">
        /// Powershell instance cannot be changed in its
        /// current state.
        /// A PSCommand instance with <paramref name="command"/> added.
        /// This method is not thread safe.
        /// cmdlet is null.
        public PSCommand AddCommand(string command)
            _owner?.AssertChangesAreAccepted();
            _currentCommand = new Command(command, false);
        /// Add a cmdlet to construct a command pipeline.
        /// A string representing cmdlet.
        /// if true local scope is used to run the script command.
        /// A PSCommand instance with <paramref name="cmdlet"/> added.
        public PSCommand AddCommand(string cmdlet, bool useLocalScope)
            _currentCommand = new Command(cmdlet, false, useLocalScope);
        /// Add a piece of script to construct a command pipeline.
        /// For example, to construct a command string "get-process | foreach { $_.Name }"
        ///         PSCommand command = new PSCommand("get-process")
        ///             .AddScript("foreach { $_.Name }", true);
        /// A string representing the script.
        /// A PSCommand instance with <paramref name="script"/> added.
        /// This method is not thread-safe.
        public PSCommand AddScript(string script)
                throw PSTraceSource.NewArgumentNullException(nameof(script));
            _currentCommand = new Command(script, true);
        public PSCommand AddScript(string script, bool useLocalScope)
            _currentCommand = new Command(script, true, useLocalScope);
        /// Add a <see cref="Command"/> element to the current command
        /// Command to add.
        public PSCommand AddCommand(Command command)
        /// Add a parameter to the last added command.
        /// For example, to construct a command string "get-process | select-object -property name"
        ///             .AddCommand("select-object")
        ///             .AddParameter("property", "name");
        /// Value for the parameter.
        /// A PSCommand instance with <paramref name="parameterName"/> added
        /// to the parameter list of the last command.
        /// Name is non null and name length is zero after trimming whitespace.
        public PSCommand AddParameter(string parameterName, object value)
            if (_currentCommand == null)
                throw PSTraceSource.NewInvalidOperationException(PSCommandStrings.ParameterRequiresCommand,
                                                                 new object[] { "PSCommand" });
            _currentCommand.Parameters.Add(parameterName, value);
        /// Adds a switch parameter to the last added command.
        /// For example, to construct a command string "get-process | sort-object -descending"
        ///             .AddCommand("sort-object")
        ///             .AddParameter("descending");
        public PSCommand AddParameter(string parameterName)
            _currentCommand.Parameters.Add(parameterName, true);
        /// Adds a <see cref="CommandParameter"/> instance to the last added command.
        internal PSCommand AddParameter(CommandParameter parameter)
            _currentCommand.Parameters.Add(parameter);
        /// Adds an argument to the last added command.
        /// For example, to construct a command string "get-process | select-object name"
        ///             .AddArgument("name");
        /// This will add the value "name" to the positional parameter list of "select-object"
        /// cmdlet. When the command is invoked, this value will get bound to positional parameter 0
        /// of the "select-object" cmdlet which is "Property".
        /// A PSCommand instance parameter value <paramref name="value"/> added
        public PSCommand AddArgument(object value)
            _currentCommand.Parameters.Add(null, value);
        /// Adds an additional statement for execution
        ///         Runspace rs = RunspaceFactory.CreateRunspace();
        ///         PowerShell ps = PowerShell.Create();
        ///         ps.Runspace = rs;
        ///         ps.AddCommand("Get-Process").AddArgument("idle");
        ///         ps.AddStatement().AddCommand("Get-Service").AddArgument("audiosrv");
        ///         ps.Invoke();
        /// A PowerShell instance with the items in <paramref name="parameters"/> added
        public PSCommand AddStatement()
            _commands[_commands.Count - 1].IsEndOfStatement = true;
        #region Properties and Methods
        /// Gets the collection of commands from this PSCommand
        public CommandCollection Commands
        /// The PowerShell instance this PSCommand is associated to, or null if it is an standalone command.
        internal PowerShell Owner
                return _owner;
                _owner = value;
        /// Clears the command(s).
            _commands.Clear();
            _currentCommand = null;
        /// Creates a shallow copy of the current PSCommand.
        /// A shallow copy of the current PSCommand
        public PSCommand Clone()
            return new PSCommand(this);
        /// Initializes the instance. Called from the constructor.
        /// Command to initialize the instance with.
        /// <param name="isScript">
        /// true if the <paramref name="command"/> is script,
        /// Caller should check the input.
        private void Initialize(string command, bool isScript, bool? useLocalScope)
                _currentCommand = new Command(command, isScript, useLocalScope);
