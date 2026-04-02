    /// Show-Command displays a GUI for a cmdlet, or for all cmdlets if no specific cmdlet is specified.
    [Cmdlet(VerbsCommon.Show, "Command", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2109589")]
    public class ShowCommandCommand : PSCmdlet, IDisposable
        /// Set to true when ProcessRecord is reached, since it will always open a window.
        private bool _hasOpenedWindow;
        /// Determines if the command should be sent to the pipeline as a string instead of run.
        /// Uses ShowCommandProxy to invoke WPF GUI object.
        private ShowCommandProxy _showCommandProxy;
        /// Data container for all cmdlets. This is populated when show-command is called with no command name.
        private List<ShowCommandCommandInfo> _commands;
        /// List of modules that have been loaded indexed by module name.
        private Dictionary<string, ShowCommandModuleInfo> _importedModules;
        /// Record the EndProcessing error.
        private PSDataCollection<ErrorRecord> _errors = new();
        /// Field used for the NoCommonParameter parameter.
        private SwitchParameter _noCommonParameter;
        /// Object used for ShowCommand with a command name that holds the view model created for the command.
        private object _commandViewModelObj;
        #region Input Cmdlet Parameter
        /// Gets or sets the command name.
        [Alias("CommandName")]
        /// Gets or sets the Width.
        [ValidateRange(300, int.MaxValue)]
        public double Height { get; set; }
        public double Width { get; set; }
        /// Gets or sets a value indicating Common Parameters should not be displayed.
        public SwitchParameter NoCommonParameter
            get { return _noCommonParameter; }
            set { _noCommonParameter = value; }
        /// Gets or sets a value indicating errors should not cause a message window to be displayed.
        public SwitchParameter ErrorPopup { get; set; }
        /// Gets or sets a value indicating the command should be sent to the pipeline as a string instead of run.
            get { return _passThrough; }
            set { _passThrough = value; }
        #region Public and Protected Methods
        /// Executes a PowerShell script, writing the output objects to the pipeline.
        /// <param name="script">Script to execute.</param>
        public void RunScript(string script)
            if (_showCommandProxy == null || string.IsNullOrEmpty(script))
            if (_passThrough)
                this.WriteObject(script);
            if (ErrorPopup)
                this.RunScriptSilentlyAndWithErrorHookup(script);
            if (_showCommandProxy.HasHostWindow)
                if (!_showCommandProxy.SetPendingISECommand(script))
            // Don't send newline at end as PSReadLine shows it rather than executing
            if (!ConsoleInputWithNativeMethods.AddToConsoleInputBuffer(script, newLine: false))
                this.WriteDebug(FormatAndOut_out_gridview.CannotWriteToConsoleInputBuffer);
        /// Initialize a proxy instance for show-command.
            _showCommandProxy = new ShowCommandProxy(this);
            if (_showCommandProxy.ScreenHeight < this.Height)
                                    new NotSupportedException(string.Format(CultureInfo.CurrentUICulture, FormatAndOut_out_gridview.PropertyValidate, "Height", _showCommandProxy.ScreenHeight)),
                                    "PARAMETER_DATA_ERROR",
            if (_showCommandProxy.ScreenWidth < this.Width)
                                    new NotSupportedException(string.Format(CultureInfo.CurrentUICulture, FormatAndOut_out_gridview.PropertyValidate, "Width", _showCommandProxy.ScreenWidth)),
        /// ProcessRecord with or without CommandName.
            if (Name == null)
                _hasOpenedWindow = this.CanProcessRecordForAllCommands();
                _hasOpenedWindow = this.CanProcessRecordForOneCommand();
        /// Optionally displays errors in a message.
            if (!_hasOpenedWindow)
            // We wait until the window is loaded and then activate it
            // to work around the console window gaining activation somewhere
            // in the end of ProcessRecord, which causes the keyboard focus
            // (and use oif tab key to focus controls) to go away from the window
            _showCommandProxy.WindowLoaded.WaitOne();
            _showCommandProxy.ActivateWindow();
            this.WaitForWindowClosedOrHelpNeeded();
            this.RunScript(_showCommandProxy.GetScript());
            if (_errors.Count == 0 || !ErrorPopup)
            StringBuilder errorString = new();
            for (int i = 0; i < _errors.Count; i++)
                    errorString.AppendLine();
                ErrorRecord error = _errors[i];
                errorString.Append(error.Exception.Message);
            _showCommandProxy.ShowErrorString(errorString.ToString());
        /// StopProcessing is called close the window when user press Ctrl+C in the command prompt.
            _showCommandProxy.CloseWindow();
        /// Runs the script in a new PowerShell instance and hooks up error stream to potentially display error popup.
        /// This method has the inconvenience of not showing to the console user the script being executed.
        /// <param name="script">Script to be run.</param>
        private void RunScriptSilentlyAndWithErrorHookup(string script)
            // errors are not created here, because there is a field for it used in the final pop up
            PSDataCollection<object> output = new();
            output.DataAdded += this.Output_DataAdded;
            _errors.DataAdded += this.Error_DataAdded;
            System.Management.Automation.PowerShell ps = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace);
            ps.Streams.Error = _errors;
            ps.Commands.AddScript(script);
            ps.Invoke(null, output, null);
        /// Issues an error when this.commandName was not found.
        private void IssueErrorForNoCommand()
            InvalidOperationException errorException = new(
                    FormatAndOut_out_gridview.CommandNotFound,
                    Name));
            this.ThrowTerminatingError(new ErrorRecord(errorException, "NoCommand", ErrorCategory.InvalidOperation, Name));
        /// Issues an error when there is more than one command matching this.commandName.
        private void IssueErrorForMoreThanOneCommand()
                    FormatAndOut_out_gridview.MoreThanOneCommand,
                    "Show-Command"));
            this.ThrowTerminatingError(new ErrorRecord(errorException, "MoreThanOneCommand", ErrorCategory.InvalidOperation, Name));
        /// Called from CommandProcessRecord to run the command that will get the CommandInfo and list of modules.
        /// <param name="command">Command to be retrieved.</param>
        /// <param name="modules">List of loaded modules.</param>
        private void GetCommandInfoAndModules(out CommandInfo command, out Dictionary<string, ShowCommandModuleInfo> modules)
            command = null;
            modules = null;
            string commandText = _showCommandProxy.GetShowCommandCommand(Name, true);
            Collection<PSObject> commandResults = this.InvokeCommand.InvokeScript(commandText);
            object[] commandObjects = (object[])commandResults[0].BaseObject;
            object[] moduleObjects = (object[])commandResults[1].BaseObject;
            if (commandResults == null || moduleObjects == null || commandObjects.Length == 0)
                this.IssueErrorForNoCommand();
            if (commandObjects.Length > 1)
                this.IssueErrorForMoreThanOneCommand();
            command = ((PSObject)commandObjects[0]).BaseObject as CommandInfo;
            if (command.CommandType == CommandTypes.Alias)
                commandText = _showCommandProxy.GetShowCommandCommand(command.Definition, false);
                commandResults = this.InvokeCommand.InvokeScript(commandText);
                if (commandResults == null || commandResults.Count != 1)
                command = (CommandInfo)commandResults[0].BaseObject;
            modules = _showCommandProxy.GetImportedModulesDictionary(moduleObjects);
        /// ProcessRecord when a command name is specified.
        /// <returns>True if there was no exception processing this record.</returns>
        private bool CanProcessRecordForOneCommand()
            CommandInfo commandInfo;
            this.GetCommandInfoAndModules(out commandInfo, out _importedModules);
            Diagnostics.Assert(commandInfo != null, "GetCommandInfoAndModules would throw a terminating error/exception");
                _commandViewModelObj = _showCommandProxy.GetCommandViewModel(new ShowCommandCommandInfo(commandInfo), _noCommonParameter.ToBool(), _importedModules, this.Name.Contains('\\'));
                _showCommandProxy.ShowCommandWindow(_commandViewModelObj, _passThrough);
            catch (TargetInvocationException ti)
                this.WriteError(new ErrorRecord(ti.InnerException, "CannotProcessRecordForOneCommand", ErrorCategory.InvalidOperation, Name));
        /// ProcessRecord when a command name is not specified.
        private bool CanProcessRecordForAllCommands()
            Collection<PSObject> rawCommands = this.InvokeCommand.InvokeScript(_showCommandProxy.GetShowAllModulesCommand());
            _commands = _showCommandProxy.GetCommandList((object[])rawCommands[0].BaseObject);
            _importedModules = _showCommandProxy.GetImportedModulesDictionary((object[])rawCommands[1].BaseObject);
                _showCommandProxy.ShowAllModulesWindow(_importedModules, _commands, _noCommonParameter.ToBool(), _passThrough);
                this.WriteError(new ErrorRecord(ti.InnerException, "CannotProcessRecordForAllCommands", ErrorCategory.InvalidOperation, Name));
        /// Waits until the window has been closed answering HelpNeeded events.
        private void WaitForWindowClosedOrHelpNeeded()
                int which = WaitHandle.WaitAny(new WaitHandle[] { _showCommandProxy.WindowClosed, _showCommandProxy.HelpNeeded, _showCommandProxy.ImportModuleNeeded });
                if (which == 0)
                if (which == 1)
                    Collection<PSObject> helpResults = this.InvokeCommand.InvokeScript(_showCommandProxy.GetHelpCommand(_showCommandProxy.CommandNeedingHelp));
                    _showCommandProxy.DisplayHelp(helpResults);
                Diagnostics.Assert(which == 2, "which is 0,1 or 2 and 0 and 1 have been eliminated in the ifs above");
                string commandToRun = _showCommandProxy.GetImportModuleCommand(_showCommandProxy.ParentModuleNeedingImportModule);
                Collection<PSObject> rawCommands;
                    rawCommands = this.InvokeCommand.InvokeScript(commandToRun);
                    _showCommandProxy.ImportModuleFailed(e);
                _showCommandProxy.ImportModuleDone(_importedModules, _commands);
        /// Writes the output of a script being run into the pipeline.
        /// <param name="sender">Output collection.</param>
        /// <param name="e">Output event.</param>
        private void Output_DataAdded(object sender, DataAddedEventArgs e)
            this.WriteObject(((PSDataCollection<object>)sender)[e.Index]);
        /// Writes the errors of a script being run into the pipeline.
        /// <param name="sender">Error collection.</param>
        /// <param name="e">Error event.</param>
        private void Error_DataAdded(object sender, DataAddedEventArgs e)
            this.WriteError(((PSDataCollection<ErrorRecord>)sender)[e.Index]);
                if (_errors != null)
                    _errors.Dispose();
                    _errors = null;
        /// Wraps interop code for console input buffer.
        internal static class ConsoleInputWithNativeMethods
            /// Constant used in calls to GetStdHandle.
            internal const int STD_INPUT_HANDLE = -10;
            /// Adds a string to the console input buffer.
            /// <param name="str">String to add to console input buffer.</param>
            /// <param name="newLine">True to add Enter after the string.</param>
            /// <returns>True if it was successful in adding all characters to console input buffer.</returns>
            internal static bool AddToConsoleInputBuffer(string str, bool newLine)
                IntPtr handle = ConsoleInputWithNativeMethods.GetStdHandle(ConsoleInputWithNativeMethods.STD_INPUT_HANDLE);
                if (handle == IntPtr.Zero)
                uint strLen = (uint)str.Length;
                ConsoleInputWithNativeMethods.INPUT_RECORD[] records = new ConsoleInputWithNativeMethods.INPUT_RECORD[strLen + (newLine ? 1 : 0)];
                for (int i = 0; i < strLen; i++)
                    ConsoleInputWithNativeMethods.INPUT_RECORD.SetInputRecord(ref records[i], str[i]);
                uint written;
                if (!ConsoleInputWithNativeMethods.WriteConsoleInput(handle, records, strLen, out written) || written != strLen)
                    // I do not know of a case where written is not going to be strlen. Maybe for some character that
                    // is not supported in the console. The API suggests this can happen,
                    // so we handle it by returning false
                // Enter is written separately, because if this is a command, and one of the characters in the command was not written
                // (written != strLen) it is desireable to fail (return false) before typing enter and running the command
                if (newLine)
                    ConsoleInputWithNativeMethods.INPUT_RECORD[] enterArray = new ConsoleInputWithNativeMethods.INPUT_RECORD[1];
                    ConsoleInputWithNativeMethods.INPUT_RECORD.SetInputRecord(ref enterArray[0], (char)13);
                    written = 0;
                    if (!ConsoleInputWithNativeMethods.WriteConsoleInput(handle, enterArray, 1, out written))
                        // I don't think this will happen
                    Diagnostics.Assert(written == 1, "only Enter is being added and it is a supported character");
            /// Gets the console handle.
            /// <param name="nStdHandle">Which console handle to get.</param>
            /// <returns>The console handle.</returns>
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr GetStdHandle(int nStdHandle);
            /// Writes to the console input buffer.
            /// <param name="hConsoleInput">Console handle.</param>
            /// <param name="lpBuffer">Inputs to be written.</param>
            /// <param name="nLength">Number of inputs to be written.</param>
            /// <param name="lpNumberOfEventsWritten">Returned number of inputs actually written.</param>
            /// <returns>0 if the function fails.</returns>
            internal static extern bool WriteConsoleInput(
                IntPtr hConsoleInput,
                INPUT_RECORD[] lpBuffer,
                uint nLength,
                out uint lpNumberOfEventsWritten);
            /// A record to be added to the console buffer.
            internal struct INPUT_RECORD
                /// The proper event type for a KeyEvent KEY_EVENT_RECORD.
                internal const int KEY_EVENT = 0x0001;
                /// Input buffer event type.
                internal ushort EventType;
                /// The actual event. The original structure is a union of many others, but this is the largest of them.
                /// And we don't need other kinds of events.
                internal KEY_EVENT_RECORD KeyEvent;
                /// Sets the necessary fields of <paramref name="inputRecord"/> for a KeyDown event for the <paramref name="character"/>
                /// <param name="inputRecord">Input record to be set.</param>
                /// <param name="character">Character to set the record with.</param>
                internal static void SetInputRecord(ref INPUT_RECORD inputRecord, char character)
                    inputRecord.EventType = INPUT_RECORD.KEY_EVENT;
                    inputRecord.KeyEvent.bKeyDown = true;
                    inputRecord.KeyEvent.UnicodeChar = character;
            /// Type of INPUT_RECORD which is a key.
            internal struct KEY_EVENT_RECORD
                /// True for key down and false for key up, but only needed if wVirtualKeyCode is used.
                internal bool bKeyDown;
                /// Repeat count.
                internal ushort wRepeatCount;
                /// Virtual key code.
                internal ushort wVirtualKeyCode;
                /// Virtual key scan code.
                internal ushort wVirtualScanCode;
                /// Character in input. If this is specified, wVirtualKeyCode, and others don't need to be.
                internal char UnicodeChar;
                /// State of keys like Shift and control.
                internal uint dwControlKeyState;
