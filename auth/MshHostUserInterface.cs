    /// Defines the properties and facilities providing by an hosting application deriving from
    /// <see cref="System.Management.Automation.Host.PSHost"/> that offers dialog-oriented and
    /// line-oriented interactive features.
    public abstract class PSHostUserInterface
        /// Gets hosting application's implementation of the
        /// <see cref="System.Management.Automation.Host.PSHostRawUserInterface"/> abstract base class
        /// that implements that class.
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface"/>, or null to indicate that
        /// low-level user interaction is not supported.
        public abstract System.Management.Automation.Host.PSHostRawUserInterface RawUI
        /// Returns true for hosts that support VT100 like virtual terminals.
        public virtual bool SupportsVirtualTerminal { get { return false; } }
        /// Reads characters from the console until a newline (a carriage return) is encountered.
        /// The characters typed by the user.
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.ReadLineAsSecureString"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.PromptForCredential(string, string, string, string)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.PromptForCredential(string, string, string, string, System.Management.Automation.PSCredentialTypes, System.Management.Automation.PSCredentialUIOptions)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.Prompt"/>
        public abstract string ReadLine();
        /// Same as ReadLine, except that the result is a SecureString, and that the input is not echoed to the user while it is
        /// collected (or is echoed in some obfuscated way, such as showing a dot for each character).
        /// The characters typed by the user in an encrypted form.
        /// Note that credentials (a user name and password) should be gathered with
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface.PromptForCredential(string, string, string, string)"/>
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface.PromptForCredential(string, string, string, string, System.Management.Automation.PSCredentialTypes, System.Management.Automation.PSCredentialUIOptions)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.ReadLine"/>
        public abstract SecureString ReadLineAsSecureString();
        /// Writes characters to the screen buffer.  Does not append a carriage return.
        /// <!-- Here we choose to just offer string parameters rather than the 18 overloads from TextWriter -->
        /// The characters to be written.  null is not allowed.
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.Write(ConsoleColor, ConsoleColor, string)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.WriteLine()"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.WriteLine(string)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.WriteLine(System.ConsoleColor, System.ConsoleColor, string)"/>
        public abstract void Write(string value);
        /// Same as <see cref="System.Management.Automation.Host.PSHostUserInterface.Write(string)"/>,
        /// except that colors can be specified.
        /// The foreground color to display the text with.
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.Write(string)"/>
        public abstract void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value);
        /// The default implementation writes a carriage return to the screen buffer.
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface.Write(string)"/>
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface.Write(System.ConsoleColor, System.ConsoleColor, string)"/>
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface.WriteLine(string)"/>
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface.WriteLine(System.ConsoleColor, System.ConsoleColor, string)"/>
        public virtual void WriteLine()
            WriteLine(string.Empty);
        /// Writes characters to the screen buffer, and appends a carriage return.
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.Write(System.ConsoleColor, System.ConsoleColor, string)"/>
        public abstract void WriteLine(string value);
        /// Same as <see cref="System.Management.Automation.Host.PSHostUserInterface.WriteLine(string)"/>,
        public virtual void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
            // #pragma warning disable 56506
            // expressly not checking for value == null so that attempts to write a null cause an exception
            if ((value != null) && (value.Length != 0))
                Write(foregroundColor, backgroundColor, value);
            Write("\n");
            // #pragma warning restore 56506
        /// Writes a line to the "error display" of the host, as opposed to the "output display," which is
        /// written to by the variants of
        /// <see cref="System.Management.Automation.Host.PSHostUserInterface.WriteLine()"/> and
        /// The characters to be written.
        public abstract void WriteErrorLine(string value);
        /// Invoked by <see cref="System.Management.Automation.Cmdlet.WriteDebug"/> to display a debugging message
        /// to the user.
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.WriteProgress"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.WriteVerboseLine"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.WriteWarningLine"/>
        public abstract void WriteDebugLine(string message);
        /// Invoked by <see cref="System.Management.Automation.Cmdlet.WriteProgress(Int64, System.Management.Automation.ProgressRecord)"/> to display a progress record.
        /// Unique identifier of the source of the record.  An int64 is used because typically, the 'this' pointer of
        /// the command from whence the record is originating is used, and that may be from a remote Runspace on a 64-bit
        /// machine.
        /// The record being reported to the host.
        /// <seealso cref="System.Management.Automation.Host.PSHostUserInterface.WriteDebugLine"/>
        public abstract void WriteProgress(Int64 sourceId, ProgressRecord record);
        /// Invoked by <see cref="System.Management.Automation.Cmdlet.WriteVerbose"/> to display a verbose processing message to the user.
        public abstract void WriteVerboseLine(string message);
        /// Invoked by <see cref="System.Management.Automation.Cmdlet.WriteWarning"/> to display a warning processing message to the user.
        public abstract void WriteWarningLine(string message);
        /// Invoked by <see cref="System.Management.Automation.Cmdlet.WriteInformation(InformationRecord)"/> to give the host a chance to intercept
        /// informational messages. These should not be displayed to the user by default, but may be useful to display in
        /// a separate area of the user interface.
        public virtual void WriteInformation(InformationRecord record) { }
        private static bool ShouldOutputPlainText(bool isHost, bool? supportsVirtualTerminal)
            var outputRendering = OutputRendering.PlainText;
            if (supportsVirtualTerminal != false)
                switch (PSStyle.Instance.OutputRendering)
                    case OutputRendering.Host:
                        outputRendering = isHost ? OutputRendering.Ansi : OutputRendering.PlainText;
                        outputRendering = PSStyle.Instance.OutputRendering;
            return outputRendering == OutputRendering.PlainText;
        /// The format styles that are supported by the host.
        public enum FormatStyle
            /// Reset the formatting to the default.
            /// Highlight text used in output formatting.
            FormatAccent,
            /// Highlight for table headers.
            TableHeader,
            /// Highlight for detailed error view.
            ErrorAccent,
            /// Style for error messages.
            /// Style for warning messages.
            /// Style for verbose messages.
            /// Style for debug messages.
        /// Get the ANSI escape sequence for the given format style.
        /// <param name="formatStyle">
        /// The format style to get the escape sequence for.
        /// The ANSI escape sequence for the given format style.
        public static string GetFormatStyleString(FormatStyle formatStyle)
            PSStyle psstyle = PSStyle.Instance;
            switch (formatStyle)
                case FormatStyle.Reset:
                    return psstyle.Reset;
                case FormatStyle.FormatAccent:
                    return psstyle.Formatting.FormatAccent;
                case FormatStyle.TableHeader:
                    return psstyle.Formatting.TableHeader;
                case FormatStyle.ErrorAccent:
                    return psstyle.Formatting.ErrorAccent;
                case FormatStyle.Error:
                    return psstyle.Formatting.Error;
                case FormatStyle.Warning:
                    return psstyle.Formatting.Warning;
                case FormatStyle.Verbose:
                    return psstyle.Formatting.Verbose;
                case FormatStyle.Debug:
                    return psstyle.Formatting.Debug;
        /// Get the appropriate output string based on different criteria.
        /// The text to format.
        /// <param name="supportsVirtualTerminal">
        /// True if the host supports virtual terminal.
        /// The formatted text.
        public static string GetOutputString(string text, bool supportsVirtualTerminal)
            return GetOutputString(text, isHost: true, supportsVirtualTerminal: supportsVirtualTerminal);
        internal static string GetOutputString(string text, bool isHost, bool? supportsVirtualTerminal = null)
            var sd = new ValueStringDecorated(text);
            if (sd.IsDecorated)
                var outputRendering = OutputRendering.Ansi;
                if (ShouldOutputPlainText(isHost, supportsVirtualTerminal))
                    outputRendering = OutputRendering.PlainText;
                text = sd.ToString(outputRendering);
        // Gets the state associated with PowerShell transcription.
        // Ideally, this would be associated with the host instance, but remoting recycles host instances
        // for each command that gets invoked (so that it can keep track of the order of commands and their
        // output.) Therefore, we store this transcription data in the runspace. However, the
        // Runspace.DefaultRunspace property isn't always available (i.e.: when the pipeline is being set up),
        // so we have to cache it the first time it becomes available.
        private TranscriptionData TranscriptionData
                // If we have access to a runspace, use the transcription data for that runspace.
                // This is important when you have multiple runspaces within a host.
                LocalRunspace localRunspace = Runspace.DefaultRunspace as LocalRunspace;
                    _volatileTranscriptionData = localRunspace.TranscriptionData;
                    if (_volatileTranscriptionData != null)
                        return _volatileTranscriptionData;
                // Otherwise, use the last stored transcription data. This will let us transcribe
                // errors where the runspace has gone away.
                TranscriptionData temporaryTranscriptionData = new TranscriptionData();
                return temporaryTranscriptionData;
        private TranscriptionData _volatileTranscriptionData;
        /// Transcribes a command being invoked.
        /// <param name="commandText">The text of the command being invoked.</param>
        /// <param name="invocation">The invocation info of the command being transcribed.</param>
        internal void TranscribeCommand(string commandText, InvocationInfo invocation)
            if (ShouldIgnoreCommand(commandText, invocation))
                // We don't actually log the output here, because there may be multiple command invocations
                // in a single input - especially in the case of API logging, which logs the command and
                // its parameters as separate calls.
                // Instead, we add this to the 'pendingOutput' collection, which we flush when either
                // the command generates output, or when we are told to invoke ignore the next command.
                foreach (TranscriptionOption transcript in TranscriptionData.Transcripts.Prepend<TranscriptionOption>(TranscriptionData.SystemTranscript))
                    if (transcript != null)
                        lock (transcript.OutputToLog)
                            if (transcript.OutputToLog.Count == 0)
                                if (transcript.IncludeInvocationHeader)
                                    transcript.OutputToLog.Add("**********************");
                                    transcript.OutputToLog.Add(
                                            Globalization.CultureInfo.InvariantCulture, InternalHostUserInterfaceStrings.CommandStartTime,
                                            DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture)));
                                transcript.OutputToLog.Add(TranscriptionData.PromptText + commandText);
                                transcript.OutputToLog.Add(">> " + commandText);
        private bool ShouldIgnoreCommand(string logElement, InvocationInfo invocation)
            string commandName = logElement;
                commandName = invocation.InvocationName;
                // Do not transcribe Out-Default
                CmdletInfo invocationCmdlet = invocation.MyCommand as CmdletInfo;
                if (invocationCmdlet != null)
                    if (invocationCmdlet.ImplementingType == typeof(Microsoft.PowerShell.Commands.OutDefaultCommand))
                        // We will ignore transcribing the command itself, but not call the IgnoreCommand() method
                        // (because that will ignore the results)
                // Don't log internal commands to the transcript.
                if (invocation.CommandOrigin == CommandOrigin.Internal)
                    IgnoreCommand(logElement, invocation);
            // Don't log helper commands to the transcript
            string[] helperCommands = { "TabExpansion2", "prompt", "TabExpansion", "PSConsoleHostReadline" };
            foreach (string helperCommand in helperCommands)
                if (string.Equals(helperCommand, commandName, StringComparison.OrdinalIgnoreCase))
                    // Record that this is a helper command. In this case, we ignore even the results
                    // from Out-Default
                    TranscriptionData.IsHelperCommand = true;
        /// Signals that a command being invoked (and its output) should be ignored.
        internal void IgnoreCommand(string commandText, InvocationInfo invocation)
            TranscribeCommandComplete(null);
            if (TranscriptionData.CommandBeingIgnored == null)
                TranscriptionData.CommandBeingIgnored = commandText;
                TranscriptionData.IsHelperCommand = false;
                if ((invocation != null) && (invocation.MyCommand != null))
                    TranscriptionData.CommandBeingIgnored = invocation.MyCommand.Name;
        /// Flag to determine whether the host is in "Transcribe Only" mode,
        /// so that when content is sent through Out-Default it doesn't
        /// make it to the actual host.
        internal bool TranscribeOnly => Interlocked.CompareExchange(ref _transcribeOnlyCount, 0, 0) != 0;
        private int _transcribeOnlyCount = 0;
        internal IDisposable SetTranscribeOnly() => new TranscribeOnlyCookie(this);
        private sealed class TranscribeOnlyCookie : IDisposable
            private readonly PSHostUserInterface _ui;
            public TranscribeOnlyCookie(PSHostUserInterface ui)
                Interlocked.Increment(ref _ui._transcribeOnlyCount);
                    Interlocked.Decrement(ref _ui._transcribeOnlyCount);
            ~TranscribeOnlyCookie() => Dispose();
        /// Flag to determine whether the host is transcribing.
                CheckSystemTranscript();
                return (TranscriptionData.Transcripts.Count > 0) || (TranscriptionData.SystemTranscript != null);
        private void CheckSystemTranscript()
            lock (TranscriptionData)
                if (TranscriptionData.SystemTranscript == null)
                    TranscriptionData.SystemTranscript = GetSystemTranscriptOption(TranscriptionData.SystemTranscript);
                    if (TranscriptionData.SystemTranscript != null)
                        LogTranscriptHeader(null, TranscriptionData.SystemTranscript);
        internal void StartTranscribing(string path, System.Management.Automation.Remoting.PSSenderInfo senderInfo, bool includeInvocationHeader, bool useMinimalHeader)
            TranscriptionOption transcript = new TranscriptionOption();
            transcript.Path = path;
            transcript.IncludeInvocationHeader = includeInvocationHeader;
            TranscriptionData.Transcripts.Add(transcript);
            LogTranscriptHeader(senderInfo, transcript, useMinimalHeader);
        private void LogTranscriptHeader(System.Management.Automation.Remoting.PSSenderInfo senderInfo, TranscriptionOption transcript, bool useMinimalHeader = false)
            // Transcribe the transcript header
            if (useMinimalHeader)
                line =
                        InternalHostUserInterfaceStrings.MinimalTranscriptPrologue,
                        DateTime.Now);
                string username = Environment.UserDomainName + "\\" + Environment.UserName;
                string runAsUser = username;
                if (senderInfo != null)
                    username = senderInfo.UserInfo.Identity.Name;
                // Add bits from PSVersionTable
                StringBuilder versionInfoFooter = new StringBuilder();
                Hashtable versionInfo = PSVersionInfo.GetPSVersionTable();
                foreach (string versionKey in versionInfo.Keys)
                    object value = versionInfo[versionKey];
                        var arrayValue = value as object[];
                        string valueString = arrayValue != null ? string.Join(", ", arrayValue) : value.ToString();
                        versionInfoFooter.AppendLine(versionKey + ": " + valueString);
                string configurationName = string.Empty;
                if (senderInfo != null && !string.IsNullOrEmpty(senderInfo.ConfigurationName))
                    configurationName = senderInfo.ConfigurationName;
                        InternalHostUserInterfaceStrings.TranscriptPrologue,
                        runAsUser,
                        configurationName,
                        Environment.OSVersion.VersionString,
                        string.Join(" ", Environment.GetCommandLineArgs()),
                        versionInfoFooter.ToString().TrimEnd());
                transcript.OutputToLog.Add(line);
            if (TranscriptionData.Transcripts.Count == 0)
                throw new PSInvalidOperationException(InternalHostUserInterfaceStrings.HostNotTranscribing);
            TranscriptionOption stoppedTranscript = TranscriptionData.Transcripts[TranscriptionData.Transcripts.Count - 1];
            LogTranscriptFooter(stoppedTranscript);
            stoppedTranscript.Dispose();
            TranscriptionData.Transcripts.Remove(stoppedTranscript);
            return stoppedTranscript.Path;
        private void LogTranscriptFooter(TranscriptionOption stoppedTranscript)
            // Transcribe the transcript epilogue
                    InternalHostUserInterfaceStrings.TranscriptEpilogue, DateTime.Now);
                lock (stoppedTranscript.OutputToLog)
                    stoppedTranscript.OutputToLog.Add(message);
                // Ignoring errors when stopping transcription (i.e.: file in use, access denied)
                // since this is probably handling exactly that error.
        internal void StopAllTranscribing()
            while (TranscriptionData.Transcripts.Count > 0)
                    LogTranscriptFooter(TranscriptionData.SystemTranscript);
                    TranscriptionData.SystemTranscript.Dispose();
                    TranscriptionData.SystemTranscript = null;
                    lock (s_systemTranscriptLock)
                        systemTranscript = null;
        /// Transcribes the supplied result text to the transcription buffer.
        /// <param name="sourceRunspace">The runspace that was used to generate this result, if it is not the current runspace.</param>
        /// <param name="resultText">The text to be transcribed.</param>
        internal void TranscribeResult(Runspace sourceRunspace, string resultText)
                // If the runspace that this result applies to is not the current runspace, update Runspace.DefaultRunspace
                // so that the transcript paths / etc. will be available to the TranscriptionData accessor.
                Runspace originalDefaultRunspace = null;
                if (sourceRunspace != null)
                    originalDefaultRunspace = Runspace.DefaultRunspace;
                    Runspace.DefaultRunspace = sourceRunspace;
                    // If we're ignoring a command, ignore its output.
                    if (TranscriptionData.CommandBeingIgnored != null)
                        // If we're ignoring a prompt, capture the value
                        if (string.Equals("prompt", TranscriptionData.CommandBeingIgnored, StringComparison.OrdinalIgnoreCase))
                            TranscriptionData.PromptText = resultText;
                    resultText = resultText.TrimEnd();
                    var text = new ValueStringDecorated(resultText);
                    if (text.IsDecorated)
                        resultText = text.ToString(OutputRendering.PlainText);
                                transcript.OutputToLog.Add(resultText);
                    if (originalDefaultRunspace != null)
                        Runspace.DefaultRunspace = originalDefaultRunspace;
        internal void TranscribeResult(string resultText)
            TranscribeResult(null, resultText);
        /// Transcribes / records the completion of a command.
        /// <param name="invocation"></param>
        internal void TranscribeCommandComplete(InvocationInfo invocation)
            FlushPendingOutput();
                // If we're ignoring a command that was internal, we still want the
                // results of Out-Default. However, if it was a host helper command,
                // ignore all output (including Out-Default)
                string commandNameToCheck = TranscriptionData.CommandBeingIgnored;
                if (TranscriptionData.IsHelperCommand)
                    commandNameToCheck = "Out-Default";
                // If we're completing a command that we were ignoring, start transcribing results / etc. again.
                if ((TranscriptionData.CommandBeingIgnored != null) &&
                    (invocation != null) && (invocation.MyCommand != null) &&
                    string.Equals(commandNameToCheck, invocation.MyCommand.Name, StringComparison.OrdinalIgnoreCase))
                    TranscriptionData.CommandBeingIgnored = null;
        internal void TranscribePipelineComplete()
        private void FlushPendingOutput()
                        lock (transcript.OutputBeingLogged)
                            bool alreadyLogging = transcript.OutputBeingLogged.Count > 0;
                            transcript.OutputBeingLogged.AddRange(transcript.OutputToLog);
                            transcript.OutputToLog.Clear();
                            // If there is already a thread trying to log output, add this output to its buffer
                            // and don't start a new thread.
                            if (alreadyLogging)
                    // Create the file in the main thread and flush the contents in the background thread.
                    // Transcription should begin only if file generation is successful.
                    // If there is an error in file generation, throw the exception.
                    string baseDirectory = Path.GetDirectoryName(transcript.Path);
                    if (Directory.Exists(transcript.Path) || (string.Equals(baseDirectory, transcript.Path.TrimEnd(Path.DirectorySeparatorChar), StringComparison.Ordinal)))
                            InternalHostUserInterfaceStrings.InvalidTranscriptFilePath,
                            transcript.Path);
                    if (!Directory.Exists(baseDirectory))
                        Directory.CreateDirectory(baseDirectory);
                    if (!File.Exists(transcript.Path))
                        File.Create(transcript.Path).Dispose();
                    // Do the actual writing in the background so that it doesn't hold up the UI thread.
                    Task writer = Task.Run(() =>
                        // System transcripts can have high contention. Do exponential back-off on writing
                        // if needed.
                        int delay = Random.Shared.Next(10) + 1;
                        bool written = false;
                        while (!written)
                                transcript.FlushContentToDisk();
                                written = true;
                                System.Threading.Thread.Sleep(delay);
                            // If we are trying to log, but weren't able too, back of the sleep.
                            // If we're already sleeping for 1 second between tries, then just continue
                            // at this pace until the write is successful.
                            if (delay < 1000)
                                delay *= 2;
        #region Dialog-oriented Interaction
        /// Constructs a 'dialog' where the user is presented with a number of fields for which to supply values.
        /// A text description of the set of fields to be prompt.
        /// Array of FieldDescriptions that contain information about each field to be prompted for.
        /// A Dictionary object with results of prompting.  The keys are the field names from the FieldDescriptions, the values
        /// are objects representing the values of the corresponding fields as collected from the user. To the extent possible,
        /// the host should return values of the type(s) identified in the FieldDescription.  When that is not possible (for
        /// example, the type is not available to the host), the host should return the value as a string.
        public abstract Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions);
        /// <!--In future, when we have Credential object from the security team,
        /// if so configured.-->
        /// Prompt for credential.
        /// Caption for the message.
        /// Text description for the credential to be prompt.
        /// Name of the user whose credential is to be prompted for. If set to null or empty
        /// string, the function will prompt for user name first.
        /// <param name="targetName">
        /// Name of the target for which the credential is being collected.
        /// User input credential.
        public abstract PSCredential PromptForCredential(string caption, string message,
            string userName, string targetName
        /// <param name="allowedCredentialTypes">
        /// Types of credential can be supplied by the user.
        /// Options that control the credential gathering UI behavior
            string userName, string targetName, PSCredentialTypes allowedCredentialTypes,
            PSCredentialUIOptions options
        /// Presents a dialog allowing the user to choose an option from a set of options.
        /// The index of the label in the choices collection element to be presented to the user as the default choice.  -1
        /// means "no default". Must be a valid index.
        /// The index of the choices element that corresponds to the option selected.
        public abstract int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice);
        #endregion Dialog-oriented interaction
        /// Creates a new instance of the PSHostUserInterface class.
        protected PSHostUserInterface()
        /// Helper to transcribe an error through formatting and output.
        /// <param name="context">The Execution Context.</param>
        /// <param name="invocation">The invocation info associated with the record.</param>
        /// <param name="errorWrap">The error record.</param>
        internal void TranscribeError(ExecutionContext context, InvocationInfo invocation, PSObject errorWrap)
            context.InternalHost.UI.TranscribeCommandComplete(invocation);
            Collection<PSObject> results = PowerShell.Create(minimalState).AddCommand("Out-String").Invoke(
                new List<PSObject>() { errorWrap });
            TranscribeResult(results[0].ToString());
        /// Get Module Logging information from the registry.
        internal static TranscriptionOption GetSystemTranscriptOption(TranscriptionOption currentTranscript)
            var transcription = InternalTestHooks.BypassGroupPolicyCaching
                ? Utils.GetPolicySetting<Transcription>(Utils.SystemWideThenCurrentUserConfig)
                : s_transcriptionSettingCache.Value;
            if (transcription != null)
                // If we have an existing system transcript for this process, use that.
                // Otherwise, populate the static variable with the result of the group policy setting.
                // This way, multiple runspaces opened by the same process will share the same transcript.
                    systemTranscript ??= PSHostUserInterface.GetTranscriptOptionFromSettings(transcription, currentTranscript);
            return systemTranscript;
        internal static TranscriptionOption systemTranscript = null;
        private static readonly object s_systemTranscriptLock = new object();
        private static readonly Lazy<Transcription> s_transcriptionSettingCache = new Lazy<Transcription>(
            static () => Utils.GetPolicySetting<Transcription>(Utils.SystemWideThenCurrentUserConfig),
            isThreadSafe: true);
        private static TranscriptionOption GetTranscriptOptionFromSettings(Transcription transcriptConfig, TranscriptionOption currentTranscript)
            TranscriptionOption transcript = null;
            if (transcriptConfig.EnableTranscripting == true)
                if (currentTranscript != null)
                    return currentTranscript;
                transcript = new TranscriptionOption();
                // Pull out the transcript path
                if (transcriptConfig.OutputDirectory != null)
                    transcript.Path = GetTranscriptPath(transcriptConfig.OutputDirectory, true);
                    transcript.Path = GetTranscriptPath();
                // Pull out the "enable invocation header"
                transcript.IncludeInvocationHeader = transcriptConfig.EnableInvocationHeader == true;
            return transcript;
        internal static string GetTranscriptPath()
            string baseDirectory = Platform.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return GetTranscriptPath(baseDirectory, false);
        internal static string GetTranscriptPath(string baseDirectory, bool includeDate)
                baseDirectory = Platform.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (!Path.IsPathRooted(baseDirectory))
                    baseDirectory = Path.Combine(
                        Platform.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        baseDirectory);
            if (includeDate)
                baseDirectory = Path.Combine(baseDirectory, DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
            // transcriptPath includes some randomness so that files can be collected on a central share,
            // and an attacker can't guess the filename and read the contents if the ACL was poor.
            // After testing, a computer can do about 10,000 remote path tests per second. So 6
            // bytes of randomness (2^48 = 2.8e14) would take an attacker about 891 years to guess
            // a filename (assuming they knew the time the transcript was started).
            // (5 bytes = 3 years, 4 bytes = about a month)
            Span<byte> randomBytes = stackalloc byte[6];
            System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
            string filename = string.Format(
                        "PowerShell_transcript.{0}.{1}.{2:yyyyMMddHHmmss}.txt",
                        Convert.ToBase64String(randomBytes).Replace('/', '_'),
            string transcriptPath = System.IO.Path.Combine(baseDirectory, filename);
            return transcriptPath;
    // Holds runspace-wide transcription data / settings for PowerShell transcription
    internal class TranscriptionData
        internal TranscriptionData()
            Transcripts = new List<TranscriptionOption>();
            SystemTranscript = null;
            CommandBeingIgnored = null;
            IsHelperCommand = false;
            PromptText = "PS>";
        internal List<TranscriptionOption> Transcripts { get; }
        internal TranscriptionOption SystemTranscript { get; set; }
        internal string CommandBeingIgnored { get; set; }
        internal bool IsHelperCommand { get; set; }
        internal string PromptText { get; set; }
    // Holds options for PowerShell transcription
    internal class TranscriptionOption : IDisposable
        internal TranscriptionOption()
            OutputToLog = new List<string>();
            OutputBeingLogged = new List<string>();
        /// The path that this transcript is being logged to.
        internal string Path { get; set; }
        /// Any output to log for this transcript.
        internal List<string> OutputToLog { get; }
        /// Any output currently being logged for this transcript.
        internal List<string> OutputBeingLogged { get; }
        /// Whether to include time stamp / command separators in
        /// transcript output.
        internal bool IncludeInvocationHeader { get; set; }
        /// Logs buffered content to disk. We use this instead of File.AppendAllLines
        /// so that we don't need to pay seek penalties all the time, and so that we
        /// don't need append permission to our own files.
        internal void FlushContentToDisk()
            static Encoding GetPathEncoding(string path)
                using StreamReader reader = new StreamReader(path, Encoding.Default, detectEncodingFromByteOrderMarks: true);
                _ = reader.Read();
                return reader.CurrentEncoding;
            lock (OutputBeingLogged)
                    if (_contentWriter == null)
                            var currentEncoding = GetPathEncoding(this.Path);
                            // Try to first open the file with permissions that will allow us to read from it
                            // later.
                            _contentWriter = new StreamWriter(
                                new FileStream(this.Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read),
                                currentEncoding);
                            _contentWriter.BaseStream.Seek(0, SeekOrigin.End);
                            // If that doesn't work (i.e.: logging to a tightly-ACL'd share), request fewer
                            // file permissions.
                                new FileStream(this.Path, FileMode.Append, FileAccess.Write, FileShare.Read),
                                Encoding.Default);
                        _contentWriter.AutoFlush = true;
                    foreach (string line in this.OutputBeingLogged)
                        _contentWriter.WriteLine(line);
                OutputBeingLogged.Clear();
        private StreamWriter _contentWriter = null;
            // Wait for any pending output to be flushed to disk so that Stop-Transcript
            // can be trusted to immediately have all content from that session in the file)
            int outputWait = 0;
            while (
                (outputWait < 1000) &&
                ((OutputToLog.Count > 0) || (OutputBeingLogged.Count > 0)))
                outputWait += 100;
            if (_contentWriter != null)
                    _contentWriter.Flush();
                    _contentWriter.Dispose();
                    // Do nothing
                _contentWriter = null;
    /// This interface needs to be implemented by PSHost objects that want to support PromptForChoice
    /// by giving the user ability to select more than one choice. The PromptForChoice method available
    /// in PSHostUserInterface class supports only one choice selection.
    public interface IHostUISupportsMultipleChoiceSelection
        /// The indices of the choice elements that corresponds to the options selected. The
        /// returned collection may contain duplicates depending on a particular host
        Collection<int> PromptForChoice(string? caption, string? message,
            Collection<ChoiceDescription> choices, IEnumerable<int>? defaultChoices);
    /// Helper methods used by PowerShell's Hosts: ConsoleHost and InternalHost to process
    internal static class HostUIHelperMethods
        /// Constructs a string of the choices and their hotkeys.
        /// <param name="hotkeysAndPlainLabels"></param>
        /// 1. Cannot process the hot key because a question mark ("?") cannot be used as a hot key.
        internal static void BuildHotkeysAndPlainLabels(Collection<ChoiceDescription> choices,
            out string[,] hotkeysAndPlainLabels)
            // we will allocate the result array
            hotkeysAndPlainLabels = new string[2, choices.Count];
                #region SplitLabel
                hotkeysAndPlainLabels[0, i] = string.Empty;
                int andPos = choices[i].Label.IndexOf('&');
                if (andPos >= 0)
                    Text.StringBuilder splitLabel = new Text.StringBuilder(choices[i].Label.Substring(0, andPos), choices[i].Label.Length);
                    if (andPos + 1 < choices[i].Label.Length)
                        splitLabel.Append(choices[i].Label.AsSpan(andPos + 1));
                        hotkeysAndPlainLabels[0, i] = CultureInfo.CurrentCulture.TextInfo.ToUpper(choices[i].Label.AsSpan(andPos + 1, 1).Trim().ToString());
                    hotkeysAndPlainLabels[1, i] = splitLabel.ToString().Trim();
                    hotkeysAndPlainLabels[1, i] = choices[i].Label;
                #endregion SplitLabel
                // ? is not localizable
                if (string.Equals(hotkeysAndPlainLabels[0, i], "?", StringComparison.Ordinal))
                        string.Create(Globalization.CultureInfo.InvariantCulture, $"choices[{i}].Label"),
                        InternalHostUserInterfaceStrings.InvalidChoiceHotKeyError);
        /// Searches for a corresponding match between the response string and the choices.  A match is either the response
        /// string is the full text of the label (sans hotkey marker), or is a hotkey.  Full labels are checked first, and take
        /// precedence over hotkey matches.
        /// Returns the index into the choices array matching the response string, or -1 if there is no match.
        internal static int DetermineChoicePicked(string response, Collection<ChoiceDescription> choices, string[,] hotkeysAndPlainLabels)
            Diagnostics.Assert(choices != null, "choices: expected a value");
            Diagnostics.Assert(hotkeysAndPlainLabels != null, "hotkeysAndPlainLabels: expected a value");
            // check the full label first, as this is the least ambiguous
                // pick the one that matches either the hot key or the full label
                if (string.Equals(response, hotkeysAndPlainLabels[1, i], StringComparison.CurrentCultureIgnoreCase))
                    result = i;
            // now check the hotkeys
                    // Ignore labels with empty hotkeys
                        if (string.Equals(response, hotkeysAndPlainLabels[0, i], StringComparison.CurrentCultureIgnoreCase))
