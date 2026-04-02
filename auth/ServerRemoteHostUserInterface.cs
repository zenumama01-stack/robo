    /// The ServerRemoteHostUserInterface class.
    internal class ServerRemoteHostUserInterface : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection
        /// Constructor for ServerRemoteHostUserInterface.
        internal ServerRemoteHostUserInterface(ServerRemoteHost remoteHost)
            Dbg.Assert(remoteHost != null, "Expected remoteHost != null");
            ServerRemoteHost = remoteHost;
            Dbg.Assert(!remoteHost.HostInfo.IsHostUINull, "Expected !remoteHost.HostInfo.IsHostUINull");
            _serverMethodExecutor = remoteHost.ServerMethodExecutor;
            // Use HostInfo to duplicate host-RawUI as null or non-null based on the client's host-RawUI.
            RawUI = remoteHost.HostInfo.IsHostRawUINull ? null : new ServerRemoteHostRawUserInterface(this);
        /// Raw ui.
        public override PSHostRawUserInterface RawUI { get; }
        /// Server remote host.
        internal ServerRemoteHost ServerRemoteHost { get; }
        /// Read line.
            return _serverMethodExecutor.ExecuteMethod<string>(RemoteHostMethodId.ReadLine);
        /// Prompt for choice.
            return _serverMethodExecutor.ExecuteMethod<int>(RemoteHostMethodId.PromptForChoice, new object[] { caption, message, choices, defaultChoice });
        /// Prompt for choice. User can select multiple choices.
            return _serverMethodExecutor.ExecuteMethod<Collection<int>>(RemoteHostMethodId.PromptForChoiceMultipleSelection,
                new object[] { caption, message, choices, defaultChoices });
            // forward the call to the client host
            Dictionary<string, PSObject> results = _serverMethodExecutor.ExecuteMethod<Dictionary<string, PSObject>>(
                RemoteHostMethodId.Prompt, new object[] { caption, message, descriptions });
            // attempt to do the requested type casts on the server (it is okay to fail the cast and return the original object)
            foreach (FieldDescription description in descriptions)
                Type requestedType = InternalHostUserInterface.GetFieldType(description);
                if (requestedType != null)
                    PSObject valueFromClient;
                    if (results.TryGetValue(description.Name, out valueFromClient))
                        object conversionResult;
                        if (LanguagePrimitives.TryConvertTo(valueFromClient, requestedType, CultureInfo.InvariantCulture, out conversionResult))
                            if (conversionResult != null)
                                results[description.Name] = PSObject.AsPSObject(conversionResult);
                                results[description.Name] = null;
        public override void Write(string message)
            message = GetOutputString(message, supportsVirtualTerminal: true);
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.Write1, new object[] { message });
        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.Write2, new object[] { foregroundColor, backgroundColor, message });
        /// Write line.
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteLine1);
        public override void WriteLine(string message)
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteLine2, new object[] { message });
        public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string message)
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteLine3, new object[] { foregroundColor, backgroundColor, message });
        /// Write error line.
        public override void WriteErrorLine(string message)
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteErrorLine, new object[] { message });
        /// Write debug line.
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteDebugLine, new object[] { message });
        /// Write progress.
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteProgress, new object[] { sourceId, record });
        /// Write verbose line.
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteVerboseLine, new object[] { message });
        /// Write warning line.
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.WriteWarningLine, new object[] { message });
        /// Read line as secure string.
            return _serverMethodExecutor.ExecuteMethod<SecureString>(RemoteHostMethodId.ReadLineAsSecureString);
            return _serverMethodExecutor.ExecuteMethod<PSCredential>(RemoteHostMethodId.PromptForCredential1,
                    new object[] { caption, message, userName, targetName });
            return _serverMethodExecutor.ExecuteMethod<PSCredential>(RemoteHostMethodId.PromptForCredential2,
                    new object[] { caption, message, userName, targetName, allowedCredentialTypes, options });
