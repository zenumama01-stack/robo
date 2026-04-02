    /// Executes methods; can be encoded and decoded for transmission over the
    /// wire.
    internal class RemoteHostCall
        /// Method name.
                return _methodInfo.Name;
        /// Method id.
        internal RemoteHostMethodId MethodId { get; }
        /// Parameters.
        internal object[] Parameters { get; }
        /// Method info.
        private readonly RemoteHostMethodInfo _methodInfo;
        /// Call id.
        private readonly long _callId;
        internal long CallId
                return _callId;
        /// Computer name to be used in messages.
        /// Constructor for RemoteHostCall.
        internal RemoteHostCall(long callId, RemoteHostMethodId methodId, object[] parameters)
            Dbg.Assert(parameters != null, "Expected parameters != null");
            _callId = callId;
            MethodId = methodId;
            Parameters = parameters;
            _methodInfo = RemoteHostMethodInfo.LookUp(methodId);
        /// Encode parameters.
        private static PSObject EncodeParameters(object[] parameters)
            // Encode the parameters and wrap the array into an ArrayList and then into a PSObject.
            ArrayList parameterList = new ArrayList();
                object parameter = parameters[i] == null ? null : RemoteHostEncoder.EncodeObject(parameters[i]);
                parameterList.Add(parameter);
            return new PSObject(parameterList);
        /// Decode parameters.
        private static object[] DecodeParameters(PSObject parametersPSObject, Type[] parameterTypes)
            // Extract the ArrayList and decode the parameters.
            ArrayList parameters = (ArrayList)parametersPSObject.BaseObject;
            List<object> decodedParameters = new List<object>();
            Dbg.Assert(parameters.Count == parameterTypes.Length, "Expected parameters.Count == parameterTypes.Length");
            for (int i = 0; i < parameters.Count; ++i)
                object parameter = parameters[i] == null ? null : RemoteHostEncoder.DecodeObject(parameters[i], parameterTypes[i]);
                decodedParameters.Add(parameter);
            return decodedParameters.ToArray();
        /// Encode.
        internal PSObject Encode()
            // Add all host information as data.
            PSObject data = RemotingEncoder.CreateEmptyPSObject();
            // Encode the parameters for transport.
            PSObject parametersPSObject = EncodeParameters(Parameters);
            // Embed everything into the main PSobject.
            data.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.CallId, _callId));
            data.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.MethodId, MethodId));
            data.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.MethodParameters, parametersPSObject));
        /// Decode.
        internal static RemoteHostCall Decode(PSObject data)
            Dbg.Assert(data != null, "Expected data != null");
            // Extract all the fields from data.
            PSObject parametersPSObject = RemotingDecoder.GetPropertyValue<PSObject>(data, RemoteDataNameStrings.MethodParameters);
            RemoteHostMethodId methodId = RemotingDecoder.GetPropertyValue<RemoteHostMethodId>(data, RemoteDataNameStrings.MethodId);
            // Look up all the info related to the method.
            RemoteHostMethodInfo methodInfo = RemoteHostMethodInfo.LookUp(methodId);
            // Decode the parameters.
            object[] parameters = DecodeParameters(parametersPSObject, methodInfo.ParameterTypes);
            // Create and return the RemoteHostCall.
            return new RemoteHostCall(callId, methodId, parameters);
        /// Is void method.
        internal bool IsVoidMethod
                return _methodInfo.ReturnType == typeof(void);
        /// Execute void method.
        internal void ExecuteVoidMethod(PSHost clientHost)
            // The clientHost can be null if the user creates a runspace object without providing
            // a host parameter.
            if (clientHost == null)
            RemoteRunspace remoteRunspaceToClose = null;
            if (this.IsSetShouldExitOrPopRunspace)
                remoteRunspaceToClose = GetRemoteRunspaceToClose(clientHost);
                object targetObject = this.SelectTargetObject(clientHost);
                MyMethodBase.Invoke(targetObject, Parameters);
                remoteRunspaceToClose?.Close();
        /// Get remote runspace to close.
        private static RemoteRunspace GetRemoteRunspaceToClose(PSHost clientHost)
            // Figure out if we need to close the remote runspace. Return null if we don't.
            // Are we a Start-PSSession enabled host?
            IHostSupportsInteractiveSession host = clientHost as IHostSupportsInteractiveSession;
            if (host == null || !host.IsRunspacePushed)
            // Now inspect the runspace.
            RemoteRunspace remoteRunspace = host.Runspace as RemoteRunspace;
            if (remoteRunspace == null || !remoteRunspace.ShouldCloseOnPop)
            // At this point it is clear we have to close the remote runspace, so return it.
        /// My method base.
        private MethodBase MyMethodBase
                return (MethodBase)_methodInfo.InterfaceType.GetMethod(_methodInfo.Name, _methodInfo.ParameterTypes);
        /// Execute non void method.
        internal RemoteHostResponse ExecuteNonVoidMethod(PSHost clientHost)
                throw RemoteHostExceptions.NewNullClientHostException();
            RemoteHostResponse remoteHostResponse = this.ExecuteNonVoidMethodOnObject(targetObject);
        /// Execute non void method on object.
        private RemoteHostResponse ExecuteNonVoidMethodOnObject(object instance)
            // Create variables to store result of execution.
            object returnValue = null;
            // Invoke the method and store its return values.
                if (MethodId == RemoteHostMethodId.GetBufferContents)
                    throw new PSRemotingDataStructureException(RemotingErrorIdStrings.RemoteHostGetBufferContents,
                        _computerName.ToUpper());
                returnValue = MyMethodBase.Invoke(instance, Parameters);
                exception = e.InnerException;
            // Create a RemoteHostResponse object to store the return value and exceptions.
            return new RemoteHostResponse(_callId, MethodId, returnValue, exception);
        /// Get the object that this method should be invoked on.
        private object SelectTargetObject(PSHost host)
            if (host == null || host.UI == null) { return null; }
            if (_methodInfo.InterfaceType == typeof(PSHost)) { return host; }
            if (_methodInfo.InterfaceType == typeof(IHostSupportsInteractiveSession)) { return host; }
            if (_methodInfo.InterfaceType == typeof(PSHostUserInterface)) { return host.UI; }
            if (_methodInfo.InterfaceType == typeof(IHostUISupportsMultipleChoiceSelection)) { return host.UI; }
            if (_methodInfo.InterfaceType == typeof(PSHostRawUserInterface)) { return host.UI.RawUI; }
            throw RemoteHostExceptions.NewUnknownTargetClassException(_methodInfo.InterfaceType.ToString());
        /// Is set should exit.
        internal bool IsSetShouldExit
                return MethodId == RemoteHostMethodId.SetShouldExit;
        /// Is set should exit or pop runspace.
        internal bool IsSetShouldExitOrPopRunspace
                    MethodId == RemoteHostMethodId.SetShouldExit ||
                    MethodId == RemoteHostMethodId.PopRunspace;
        /// This message performs various security checks on the
        /// remote host call message. If there is a need to modify
        /// the message or discard it for security reasons then
        /// such modifications will be made here.
        /// <param name="computerName">computer name to use in
        /// warning messages</param>
        /// <returns>A collection of remote host calls which will
        /// have to be executed before this host call can be
        /// executed.</returns>
        internal Collection<RemoteHostCall> PerformSecurityChecksOnHostMessage(string computerName)
            Dbg.Assert(!string.IsNullOrEmpty(computerName),
                "Computer Name must be passed for use in warning messages");
            Collection<RemoteHostCall> prerequisiteCalls = new Collection<RemoteHostCall>();
            // check if the incoming message is a PromptForCredential message
            // if so, do the following:
            //       (a) prepend "PowerShell Credential Request" in the title
            //       (b) prepend "Message from Server XXXXX" to the text message
            if (MethodId == RemoteHostMethodId.PromptForCredential1 ||
                MethodId == RemoteHostMethodId.PromptForCredential2)
                // modify the caption which is _parameters[0]
                string modifiedCaption = ModifyCaption((string)Parameters[0]);
                // modify the message which is _parameters[1]
                string modifiedMessage = ModifyMessage((string)Parameters[1], computerName);
                Parameters[0] = modifiedCaption;
                Parameters[1] = modifiedMessage;
            // Check if the incoming message is a Prompt message
            // if so, then do the following:
            //        (a) check if any of the field descriptions
            //            correspond to PSCredential
            //        (b) if field descriptions correspond to
            //            PSCredential modify the caption and
            //            message as in the previous case above
            else if (MethodId == RemoteHostMethodId.Prompt)
                // check if any of the field descriptions is for type
                // PSCredential
                if (Parameters.Length == 3)
                    Collection<FieldDescription> fieldDescs =
                        (Collection<FieldDescription>)Parameters[2];
                    bool havePSCredential = false;
                    foreach (FieldDescription fieldDesc in fieldDescs)
                        fieldDesc.IsFromRemoteHost = true;
                        Type fieldType = InternalHostUserInterface.GetFieldType(fieldDesc);
                        if (fieldType != null)
                            if (fieldType == typeof(PSCredential))
                                havePSCredential = true;
                                fieldDesc.ModifiedByRemotingProtocol = true;
                            else if (fieldType == typeof(System.Security.SecureString))
                                prerequisiteCalls.Add(ConstructWarningMessageForSecureString(
                                    computerName, RemotingErrorIdStrings.RemoteHostPromptSecureStringPrompt));
                    if (havePSCredential)
                        // modify the caption which is parameter[0]
                        // modify the message which is parameter[1]
            // Check if the incoming message is a readline as secure string
            // if so do the following:
            //      (a) Specify a warning message that the server is
            //          attempting to read something securely on the client
            else if (MethodId == RemoteHostMethodId.ReadLineAsSecureString)
                                    computerName, RemotingErrorIdStrings.RemoteHostReadLineAsSecureStringPrompt));
            // check if the incoming call is GetBufferContents
            //          attempting to read the screen buffer contents
            //          on screen and it has been blocked
            //      (b) Modify the message so that call is not executed
            else if (MethodId == RemoteHostMethodId.GetBufferContents)
                prerequisiteCalls.Add(ConstructWarningMessageForGetBufferContents(computerName));
            return prerequisiteCalls;
        /// Provides the modified caption for the given caption
        /// Used in ensuring that remote prompt messages are
        /// tagged with "PowerShell Credential Request"
        /// <param name="caption">Caption to modify.</param>
        /// <returns>New modified caption.</returns>
        private static string ModifyCaption(string caption)
            string pscaption = CredUI.PromptForCredential_DefaultCaption;
            if (!caption.Equals(pscaption, StringComparison.OrdinalIgnoreCase))
                string modifiedCaption = PSRemotingErrorInvariants.FormatResourceString(
                    RemotingErrorIdStrings.RemoteHostPromptForCredentialModifiedCaption, caption);
                return modifiedCaption;
            return caption;
        /// Provides the modified message for the given one
        /// Used in ensuring that remote prompt messages
        /// contain a warning that they originate from a
        /// different computer.
        /// <param name="message">Original message to modify.</param>
        /// <param name="computerName">computername to include in the
        /// <returns>Message which contains a warning as well.</returns>
        private static string ModifyMessage(string message, string computerName)
            string modifiedMessage = PSRemotingErrorInvariants.FormatResourceString(
                    RemotingErrorIdStrings.RemoteHostPromptForCredentialModifiedMessage,
                        computerName.ToUpper(),
            return modifiedMessage;
        /// Creates a warning message which displays to the user a
        /// warning stating that the remote host computer is
        /// actually attempting to read a line as a secure string.
        /// <param name="computerName">computer name to include
        /// in warning</param>
        /// <param name="resourceString">Resource string to use.</param>
        /// <returns>A constructed remote host call message
        /// which will display the warning.</returns>
        private static RemoteHostCall ConstructWarningMessageForSecureString(string computerName,
            string resourceString)
            string warning = PSRemotingErrorInvariants.FormatResourceString(
                    computerName.ToUpper());
            return new RemoteHostCall(ServerDispatchTable.VoidCallId,
                RemoteHostMethodId.WriteWarningLine, new object[] { warning });
        /// attempting to read the host's buffer contents and that
        /// it was suppressed.
        private static RemoteHostCall ConstructWarningMessageForGetBufferContents(string computerName)
                RemotingErrorIdStrings.RemoteHostGetBufferContents,
    /// Encapsulates the method response semantics. Method responses are generated when
    /// RemoteHostCallPacket objects are executed. They can contain both the return values of
    /// the execution as well as exceptions that were thrown in the RemoteHostCallPacket
    /// execution. They can be encoded and decoded for transporting over the wire. A
    /// method response can be used to transport the result of an execution and then to
    /// simulate the execution on the other end.
    internal class RemoteHostResponse
        private readonly RemoteHostMethodId _methodId;
        /// Return value.
        private readonly object _returnValue;
        /// Exception.
        private readonly Exception _exception;
        /// Constructor for RemoteHostResponse.
        internal RemoteHostResponse(long callId, RemoteHostMethodId methodId, object returnValue, Exception exception)
            _methodId = methodId;
            _returnValue = returnValue;
            _exception = exception;
        /// Simulate execution.
        internal object SimulateExecution()
            if (_exception != null)
                throw _exception;
            return _returnValue;
        /// Encode and add return value.
        private static void EncodeAndAddReturnValue(PSObject psObject, object returnValue)
            // Do nothing if the return value is null.
            if (returnValue == null) { return; }
            // Otherwise add the property.
            RemoteHostEncoder.EncodeAndAddAsProperty(psObject, RemoteDataNameStrings.MethodReturnValue, returnValue);
        /// Decode return value.
        private static object DecodeReturnValue(PSObject psObject, Type returnType)
            object returnValue = RemoteHostEncoder.DecodePropertyValue(psObject, RemoteDataNameStrings.MethodReturnValue, returnType);
        /// Encode and add exception.
        private static void EncodeAndAddException(PSObject psObject, Exception exception)
            RemoteHostEncoder.EncodeAndAddAsProperty(psObject, RemoteDataNameStrings.MethodException, exception);
        /// Decode exception.
        private static Exception DecodeException(PSObject psObject)
            object result = RemoteHostEncoder.DecodePropertyValue(psObject, RemoteDataNameStrings.MethodException, typeof(Exception));
            if (result == null) { return null; }
            if (result is Exception) { return (Exception)result; }
            throw RemoteHostExceptions.NewDecodingFailedException();
            // Create a data object and put everything in that and return it.
            EncodeAndAddReturnValue(data, _returnValue);
            EncodeAndAddException(data, _exception);
            data.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.MethodId, _methodId));
        internal static RemoteHostResponse Decode(PSObject data)
            // Decode the return value and the exception.
            object returnValue = DecodeReturnValue(data, methodInfo.ReturnType);
            Exception exception = DecodeException(data);
            // Use these values to create a RemoteHostResponse and return it.
            return new RemoteHostResponse(callId, methodId, returnValue, exception);
    /// The RemoteHostExceptions class.
    internal static class RemoteHostExceptions
        /// New remote runspace does not support push runspace exception.
        internal static Exception NewRemoteRunspaceDoesNotSupportPushRunspaceException()
            string resourceString = PSRemotingErrorInvariants.FormatResourceString(
                RemotingErrorIdStrings.RemoteRunspaceDoesNotSupportPushRunspace);
            return new PSRemotingDataStructureException(resourceString);
        /// New decoding failed exception.
        internal static Exception NewDecodingFailedException()
                RemotingErrorIdStrings.RemoteHostDecodingFailed);
        /// New not implemented exception.
        internal static Exception NewNotImplementedException(RemoteHostMethodId methodId)
                RemotingErrorIdStrings.RemoteHostMethodNotImplemented, methodInfo.Name);
            return new PSRemotingDataStructureException(resourceString, new PSNotImplementedException());
        /// New remote host call failed exception.
        internal static Exception NewRemoteHostCallFailedException(RemoteHostMethodId methodId)
                RemotingErrorIdStrings.RemoteHostCallFailed, methodInfo.Name);
        /// New decoding error for error record exception.
        internal static Exception NewDecodingErrorForErrorRecordException()
            return new PSRemotingDataStructureException(RemotingErrorIdStrings.DecodingErrorForErrorRecord);
        /// New remote host data encoding not supported exception.
        internal static Exception NewRemoteHostDataEncodingNotSupportedException(Type type)
            Dbg.Assert(type != null, "Expected type != null");
            return new PSRemotingDataStructureException(
                RemotingErrorIdStrings.RemoteHostDataEncodingNotSupported,
                type.ToString());
        /// New remote host data decoding not supported exception.
        internal static Exception NewRemoteHostDataDecodingNotSupportedException(Type type)
                RemotingErrorIdStrings.RemoteHostDataDecodingNotSupported,
        /// New unknown target class exception.
        internal static Exception NewUnknownTargetClassException(string className)
            Dbg.Assert(className != null, "Expected className != null");
            return new PSRemotingDataStructureException(RemotingErrorIdStrings.UnknownTargetClass, className);
        internal static Exception NewNullClientHostException()
            return new PSRemotingDataStructureException(RemotingErrorIdStrings.RemoteHostNullClientHost);
