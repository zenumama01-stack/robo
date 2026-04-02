#if NOT_USED
    /// This represents that a remote data is incorrectly encoded.
    public class RemotingEncodingException : RuntimeException
        public RemotingEncodingException()
        public RemotingEncodingException(string message)
            : base (message)
        public RemotingEncodingException(string message, Exception innerException)
        public RemotingEncodingException(string message, Exception innerException, ErrorRecord errorRecord)
            : base(message, innerException, errorRecord)
#endif // NOT_USED
    /// Constants used by hosts in remoting.
    internal static class RemotingConstants
        internal static readonly Version HostVersion = PSVersionInfo.PSVersion;
        internal static readonly Version ProtocolVersion_2_0 = new(2, 0); // Window 7 RC
        internal static readonly Version ProtocolVersion_2_1 = new(2, 1); // Window 7 RTM
        internal static readonly Version ProtocolVersion_2_2 = new(2, 2); // Window 8 RTM
        internal static readonly Version ProtocolVersion_2_3 = new(2, 3); // Window 10 RTM
        internal static readonly Version ProtocolVersion_2_4 = new(2, 4); // PowerShell 7.6
        // Minor will be incremented for each change in PSRP client/server stack and new versions will be
        // forked on early major release/drop changes history.
        //      2.101 to 2.102 - Disconnect support as of M2
        //      2.102 to 2.103 - Key exchange protocol changes in M3
        //      2.103 to 2.2   - Final ship protocol version value, no change to protocol
        //      2.2 to 2.3     - Enabling informational stream
        //      2.3 to 2.4     - Deprecate the 'Session_Key' exchange. The following messages are obsolete when both server and client are v2.4+:
        //                        - PUBLIC_KEY
        //                        - PUBLIC_KEY_REQUEST
        //                        - ENCRYPTED_SESSION_KEY
        //                       The padding algorithm 'RSAEncryptionPadding.Pkcs1' used in the 'Session_Key' exchange is NOT secure, and therefore,
        //                       PSRP needs to be used on top of a secure transport and the 'Session_Key' doesn't add any extra security.
        //                       So, we decided to deprecate the 'Session_Key' exchange in PSRP and skip encryption and decryption for 'SecureString'
        //                       objects. Instead, we require the transport to be secure for secure data transfer between PSRP clients and servers.
        internal static readonly Version ProtocolVersionCurrent = new(2, 4);
        internal static readonly Version ProtocolVersion = ProtocolVersionCurrent;
        // Used by remoting commands to add remoting specific note properties.
        internal static readonly string ComputerNameNoteProperty = "PSComputerName";
        internal static readonly string RunspaceIdNoteProperty = "RunspaceId";
        internal static readonly string SourceJobInstanceId = "PSSourceJobInstanceId";
        internal static readonly string EventObject = "PSEventObject";
        // used by Custom Shell related cmdlets.
        internal const string PSSessionConfigurationNoun = "PSSessionConfiguration";
        internal const string PSRemotingNoun = "PSRemoting";
        internal const string PSPluginDLLName = "pwrshplugin.dll";
        internal const string DefaultShellName = "Microsoft.PowerShell";
        internal const string MaxIdleTimeoutMS = "2147483647";
    /// String constants used for names of properties that are for storing
    /// remoting message fields in a PSObject property bag.
    internal static class RemoteDataNameStrings
        internal const string Destination = "Destination";
        internal const string RemotingTargetInterface = "RemotingTargetInterface";
        internal const string ClientRunspacePoolId = "ClientRunspacePoolId";
        internal const string ClientPowerShellId = "ClientPowerShellId";
        internal const string Action = "Action";
        internal const string DataType = "DataType";
        // used by negotiation algorithm to figure out client's timezone.
        internal const string TimeZone = "TimeZone";
        internal const string SenderInfoPreferenceVariable = "PSSenderInfo";
        // used by negotiation algorithm to figure out if the negotiation
        // request (from client) must comply.
        internal const string MustComply = "MustComply";
        // used by negotiation algorithm. Server sends this information back
        // to client to let client know if the negotiation succeeded.
        internal const string IsNegotiationSucceeded = "IsNegotiationSucceeded";
        #region Host Related Strings
        internal const string CallId = "ci";
        internal const string MethodId = "mi";
        internal const string MethodParameters = "mp";
        internal const string MethodReturnValue = "mr";
        internal const string MethodException = "me";
        internal const string PS_STARTUP_PROTOCOL_VERSION_NAME = "protocolversion";
        internal const string PublicKeyAsXml = "PublicKeyAsXml";
        internal const string PSVersion = "PSVersion";
        internal const string SerializationVersion = "SerializationVersion";
        internal const string MethodArrayElementType = "mat";
        internal const string MethodArrayLengths = "mal";
        internal const string MethodArrayElements = "mae";
        internal const string ObjectType = "T";
        internal const string ObjectValue = "V";
        #region Command discovery pipeline
        internal const string DiscoveryName = "Name";
        internal const string DiscoveryType = "CommandType";
        internal const string DiscoveryModule = "Namespace";
        internal const string DiscoveryFullyQualifiedModule = "FullyQualifiedModule";
        internal const string DiscoveryArgumentList = "ArgumentList";
        internal const string DiscoveryCount = "Count";
        #region PowerShell
        internal const string PSInvocationSettings = "PSInvocationSettings";
        internal const string ApartmentState = "ApartmentState";
        internal const string RemoteStreamOptions = "RemoteStreamOptions";
        internal const string AddToHistory = "AddToHistory";
        internal const string PowerShell = "PowerShell";
        internal const string IsNested = "IsNested";
        internal const string HistoryString = "History";
        internal const string RedirectShellErrorOutputPipe = "RedirectShellErrorOutputPipe";
        internal const string Commands = "Cmds";
        internal const string ExtraCommands = "ExtraCmds";
        internal const string CommandText = "Cmd";
        internal const string IsScript = "IsScript";
        internal const string UseLocalScopeNullable = "UseLocalScope";
        internal const string MergeUnclaimedPreviousCommandResults = "MergePreviousResults";
        internal const string MergeMyResult = "MergeMyResult";
        internal const string MergeToResult = "MergeToResult";
        internal const string MergeError = "MergeError";
        internal const string MergeWarning = "MergeWarning";
        internal const string MergeVerbose = "MergeVerbose";
        internal const string MergeDebug = "MergeDebug";
        internal const string MergeInformation = "MergeInformation";
        internal const string Parameters = "Args";
        internal const string ParameterName = "N";
        internal const string ParameterValue = "V";
        internal const string NoInput = "NoInput";
        #endregion PowerShell
        #region StateInfo
        /// Name of property when Exception is serialized as error record.
        internal const string ExceptionAsErrorRecord = "ExceptionAsErrorRecord";
        /// Property used for encoding state of pipeline when serializing PipelineStateInfo.
        internal const string PipelineState = "PipelineState";
        /// Property used for encoding state of runspace when serializing RunspaceStateInfo.
        internal const string RunspaceState = "RunspaceState";
        #endregion StateInfo
        #region PSEventArgs
        /// Properties used for serialization of PSEventArgs.
        internal const string PSEventArgsComputerName = "PSEventArgs.ComputerName";
        internal const string PSEventArgsRunspaceId = "PSEventArgs.RunspaceId";
        internal const string PSEventArgsEventIdentifier = "PSEventArgs.EventIdentifier";
        internal const string PSEventArgsSourceIdentifier = "PSEventArgs.SourceIdentifier";
        internal const string PSEventArgsTimeGenerated = "PSEventArgs.TimeGenerated";
        internal const string PSEventArgsSender = "PSEventArgs.Sender";
        internal const string PSEventArgsSourceArgs = "PSEventArgs.SourceArgs";
        internal const string PSEventArgsMessageData = "PSEventArgs.MessageData";
        #endregion PSEventArgs
        internal const string MinRunspaces = "MinRunspaces";
        internal const string MaxRunspaces = "MaxRunspaces";
        internal const string ThreadOptions = "PSThreadOptions";
        internal const string HostInfo = "HostInfo";
        internal const string RunspacePoolOperationResponse = "SetMinMaxRunspacesResponse";
        internal const string AvailableRunspaces = "AvailableRunspaces";
        internal const string PublicKey = "PublicKey";
        internal const string EncryptedSessionKey = "EncryptedSessionKey";
        internal const string ApplicationArguments = "ApplicationArguments";
        internal const string ApplicationPrivateData = "ApplicationPrivateData";
        #endregion RunspacePool
        #region ProgressRecord
        internal const string ProgressRecord_Activity = "Activity";
        internal const string ProgressRecord_ActivityId = "ActivityId";
        internal const string ProgressRecord_CurrentOperation = "CurrentOperation";
        internal const string ProgressRecord_ParentActivityId = "ParentActivityId";
        internal const string ProgressRecord_PercentComplete = "PercentComplete";
        internal const string ProgressRecord_Type = "Type";
        internal const string ProgressRecord_SecondsRemaining = "SecondsRemaining";
        internal const string ProgressRecord_StatusDescription = "StatusDescription";
    /// The destination of the remote message.
    internal enum RemotingDestination : uint
        InvalidDestination = 0x0,
        Client = 0x1,
        Server = 0x2,
        Listener = 0x4,
    /// The layer the remoting message is being communicated between.
    /// Please keep in sync with RemotingTargetInterface from
    internal enum RemotingTargetInterface : int
        InvalidTargetInterface = 0,
        Session = 1,
        RunspacePool = 2,
        PowerShell = 3,
    /// The type of the remoting message.
    /// Please keep in sync with RemotingDataType from
    internal enum RemotingDataType : uint
        InvalidDataType = 0,
        /// This data type is used when an Exception derived from IContainsErrorRecord
        /// is caught on server and is sent to client. This exception gets
        /// serialized as an error record. On the client this data type is deserialized in
        /// to an ErrorRecord.
        /// ErrorRecord on the client has an instance of RemoteException as exception.
        ExceptionAsErrorRecord = 1,
        // Session messages
        SessionCapability = 0x00010002,
        CloseSession = 0x00010003,
        CreateRunspacePool = 0x00010004,
        PublicKey = 0x00010005,
        EncryptedSessionKey = 0x00010006,
        PublicKeyRequest = 0x00010007,
        ConnectRunspacePool = 0x00010008,
        // Runspace Pool messages
        SetMaxRunspaces = 0x00021002,
        SetMinRunspaces = 0x00021003,
        RunspacePoolOperationResponse = 0x00021004,
        RunspacePoolStateInfo = 0x00021005,
        CreatePowerShell = 0x00021006,
        AvailableRunspaces = 0x00021007,
        PSEventArgs = 0x00021008,
        ApplicationPrivateData = 0x00021009,
        GetCommandMetadata = 0x0002100A,
        RunspacePoolInitData = 0x0002100B,
        ResetRunspaceState = 0x0002100C,
        // Runspace host messages
        RemoteHostCallUsingRunspaceHost = 0x00021100,
        RemoteRunspaceHostResponseData = 0x00021101,
        // PowerShell messages
        PowerShellInput = 0x00041002,
        PowerShellInputEnd = 0x00041003,
        PowerShellOutput = 0x00041004,
        PowerShellErrorRecord = 0x00041005,
        PowerShellStateInfo = 0x00041006,
        PowerShellDebug = 0x00041007,
        PowerShellVerbose = 0x00041008,
        PowerShellWarning = 0x00041009,
        PowerShellProgress = 0x00041010,
        PowerShellInformationStream = 0x00041011,
        StopPowerShell = 0x00041012,
        // PowerShell host messages
        RemoteHostCallUsingPowerShellHost = 0x00041100,
        RemotePowerShellHostResponseData = 0x00041101,
    /// Converts C# types to PSObject properties for embedding in PSObjects transported across the wire.
    internal static class RemotingEncoder
        #region NotePropertyHelpers
        internal delegate T ValueGetterDelegate<T>();
        internal static void AddNoteProperty<T>(PSObject pso, string propertyName, ValueGetterDelegate<T> valueGetter)
                value = valueGetter();
                Dbg.Assert(false, "Internal code shouldn't throw exceptions during serialization");
                    valueGetter.Target == null ? string.Empty : valueGetter.Target.GetType().FullName,
                pso.Properties.Add(new PSNoteProperty(propertyName, value));
                // Member already exists, just make sure the value is the same.
                var existingValue = pso.Properties[propertyName].Value;
                Diagnostics.Assert(object.Equals(existingValue, value),
                                    "Property already exists but new value differs.");
        internal static PSObject CreateEmptyPSObject()
            PSObject pso = new PSObject();
            // we don't care about serializing/deserializing TypeNames in remoting objects/messages
            // so we just omit TypeNames info to lower packet size and improve performance
            pso.InternalTypeNames = ConsolidatedString.Empty;
        private static PSNoteProperty CreateHostInfoProperty(HostInfo hostInfo)
            return new PSNoteProperty(
                RemoteDataNameStrings.HostInfo,
                RemoteHostEncoder.EncodeObject(hostInfo));
        #endregion NotePropertyHelpers
        #region RunspacePool related
        /// This method generates a Remoting data structure handler message for
        /// creating a RunspacePool on the server.
        /// <param name="clientRunspacePoolId">Id of the clientRunspacePool.</param>
        /// <param name="minRunspaces">minRunspaces for the RunspacePool
        /// to be created at the server</param>
        /// <param name="maxRunspaces">maxRunspaces for the RunspacePool
        /// <param name="runspacePool">Local runspace pool.</param>
        /// <param name="host">host for the runspacepool at the client end
        /// from this host, information will be extracted and sent to
        /// server</param>
        /// <returns>Data structure handler message encoded as RemoteDataObject.</returns>
        /// The message format is as under for this message
        /// --------------------------------------------------------------------------------------
        /// | D |    TI     |  RPID  |   PID   |   Action   |      Data      |        Type         |
        /// | S |  Session  | CRPID  |    0    | CreateRuns | minRunspaces,  |   InvalidDataType   |
        /// |   |           |        |         | pacePool   | maxRunspaces,  |                     |
        /// |   |           |        |         |            | threadOptions, |                     |
        /// |   |           |        |         |            | apartmentState,|                     |
        /// |   |           |        |         |            | hostInfo       |                     |
        /// |   |           |        |         |            | appParameters  |                     |
        internal static RemoteDataObject GenerateCreateRunspacePool(
            Guid clientRunspacePoolId,
            PSPrimitiveDictionary applicationArguments)
            PSObject dataAsPSObject = CreateEmptyPSObject();
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.MinRunspaces, minRunspaces));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.MaxRunspaces, maxRunspaces));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ThreadOptions, runspacePool.ThreadOptions));
            ApartmentState poolState = runspacePool.ApartmentState;
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ApartmentState, poolState));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ApplicationArguments, applicationArguments));
            // a runspace's host info always needs to be cached. This is because
            // at a later point in time, a powershell may choose to use the
            // runspace's host and may require that it uses cached Raw UI properties
            dataAsPSObject.Properties.Add(CreateHostInfoProperty(new HostInfo(host)));
            return RemoteDataObject.CreateFrom(RemotingDestination.Server,
                                            RemotingDataType.CreateRunspacePool,
                                            clientRunspacePoolId,
                                            dataAsPSObject);
        /// | S |  Runspace | CRPID  |    0    | ConnectRun | minRunspaces,  |   InvalidDataType   |
        /// |   |           |        |         | spacePool  | maxRunspaces,  |                     |
        /// |   |           |        |         |            |                |                     |
        internal static RemoteDataObject GenerateConnectRunspacePool(
            int maxRunspaces)
            int propertyCount = 0;
            if (minRunspaces != -1)
                propertyCount++;
            if (maxRunspaces != -1)
            if (propertyCount > 0)
                                            RemotingDataType.ConnectRunspacePool,
        /// Generates a response message to ConnectRunspace that includes
        /// sufficient information to construction client RunspacePool state.
        /// <param name="runspacePoolId">Id of the clientRunspacePool.</param>
        /// | C |  Runspace | CRPID  |    0    | RunspacePo | minRunspaces,  |   InvalidDataType   |
        /// |   |           |        |         | olInitData | maxRunspaces,  |                     |
        internal static RemoteDataObject GenerateRunspacePoolInitData(
            Guid runspacePoolId,
            return RemoteDataObject.CreateFrom(RemotingDestination.Client,
                                            RemotingDataType.RunspacePoolInitData,
                                            runspacePoolId,
        /// modifying the maxrunspaces of the specified runspace pool on the server.
        /// <param name="maxRunspaces">new value of maxRunspaces for the
        /// specified RunspacePool  </param>
        /// <param name="callId">Call id of the call at client.</param>
        /// | D |    TI     |  RPID  |   PID   |   Action   |      Data     |        Type         |
        /// | S | Runspace  | CRPID  |    0    |   SetMax   | maxRunspaces  |   InvalidDataType   |
        /// |   |   Pool    |        |         |  Runspaces |               |                     |
        /// |   |           |        |         |            |               |                     |
        internal static RemoteDataObject GenerateSetMaxRunspaces(Guid clientRunspacePoolId,
                                    int maxRunspaces, long callId)
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.CallId, callId));
                                               RemotingDataType.SetMaxRunspaces,
        /// <param name="minRunspaces">new value of minRunspaces for the
        /// | S | Runspace  | CRPID  |    0    |   SetMin   | minRunspaces  |   InvalidDataType   |
        internal static RemoteDataObject GenerateSetMinRunspaces(Guid clientRunspacePoolId,
                                    int minRunspaces, long callId)
                                               RemotingDataType.SetMinRunspaces,
        /// that contains a response to SetMaxRunspaces or SetMinRunspaces.
        /// <param name="response">Response to the call.</param>
        internal static RemoteDataObject GenerateRunspacePoolOperationResponse(Guid clientRunspacePoolId,
                                    object response, long callId)
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.RunspacePoolOperationResponse, response));
                                               RemotingDataType.RunspacePoolOperationResponse,
        /// getting the available runspaces on the server.
        /// <param name="clientRunspacePoolId">guid of the runspace pool on which
        /// this needs to be queried</param>
        /// <param name="callId">Call id of the call at the client.</param>
        /// --------------------------------------------------------------------------
        /// | D |    TI     |  RPID  |   PID   |      Data     |        Type          |
        /// ---------------------------------------------------------------------------
        /// | S | Runspace  | CRPID  |    0    |     null      |GetAvailableRunspaces |
        /// |   |   Pool    |        |         |               |                      |
        internal static RemoteDataObject GenerateGetAvailableRunspaces(Guid clientRunspacePoolId,
                                    long callId)
                                               RemotingDataType.AvailableRunspaces,
        /// This method generates a remoting data structure handler message for
        /// transferring a roles public key to the other side.
        /// <param name="runspacePoolId">Runspace pool id.</param>
        /// <param name="publicKey">Public key to send across.</param>
        /// <param name="destination">destination that this message is
        /// targeted to</param>
        /// <returns>Data structure message.</returns>
        /// | S | Runspace  | CRPID  |    0    |    public     |      PublicKey       |
        /// |   |   Pool    |        |         |     key       |                      |
        internal static RemoteDataObject GenerateMyPublicKey(Guid runspacePoolId,
                                    string publicKey, RemotingDestination destination)
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.PublicKey, publicKey));
            return RemoteDataObject.CreateFrom(destination,
                                               RemotingDataType.PublicKey,
        /// requesting a public key from the client to the server.
        /// | S | Runspace  | CRPID  |    0    |               |   PublicKeyRequest   |
        internal static RemoteDataObject GeneratePublicKeyRequest(Guid runspacePoolId)
                                               RemotingDataType.PublicKeyRequest,
        /// sending an encrypted session key to the client.
        /// <param name="encryptedSessionKey">Encrypted session key.</param>
        /// | S | Runspace  | CRPID  |    0    |  encrypted    | EncryptedSessionKey  |
        /// |   |   Pool    |        |         | session key   |                      |
        internal static RemoteDataObject GenerateEncryptedSessionKeyResponse(Guid runspacePoolId,
            string encryptedSessionKey)
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.EncryptedSessionKey,
                                            encryptedSessionKey));
                                               RemotingDataType.EncryptedSessionKey,
        /// This methods generates a Remoting data structure handler message for
        /// creating a command discovery pipeline on the server.
        /// <param name="shell">The client remote powershell from which the
        /// message needs to be generated.
        /// The data is extracted from parameters of the first command named "Get-Command".
        /// -------------------------------------------------------------------------
        /// | D |    TI     |  RPID  |   PID   |     Data      |        Type         |
        /// | S |  Runspace | CRPID  |  CPID   | name,         | GetCommandMetadata  |
        /// |   |  Pool     |        |         | commandType,  |                     |
        /// |   |           |        |         | module,FQM,   |                     |
        /// |   |           |        |         | argumentList  |                     |
        internal static RemoteDataObject GenerateGetCommandMetadata(ClientRemotePowerShell shell)
            Command getCommand = null;
            foreach (Command c in shell.PowerShell.Commands.Commands)
                if (c.CommandText.Equals("Get-Command", StringComparison.OrdinalIgnoreCase))
                    getCommand = c;
            Dbg.Assert(getCommand != null, "Whoever sets PowerShell.IsGetCommandMetadataSpecialPipeline needs to make sure Get-Command is present");
            string[] name = null;
            CommandTypes commandTypes = CommandTypes.Alias | CommandTypes.Cmdlet | CommandTypes.Function | CommandTypes.Filter | CommandTypes.Configuration;
            string[] module = null;
            ModuleSpecification[] fullyQualifiedModule = null;
            object[] argumentList = null;
            foreach (CommandParameter p in getCommand.Parameters)
                if (p.Name.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    name = (string[])LanguagePrimitives.ConvertTo(p.Value, typeof(string[]), CultureInfo.InvariantCulture);
                else if (p.Name.Equals("CommandType", StringComparison.OrdinalIgnoreCase))
                    commandTypes = (CommandTypes)LanguagePrimitives.ConvertTo(p.Value, typeof(CommandTypes), CultureInfo.InvariantCulture);
                else if (p.Name.Equals("Module", StringComparison.OrdinalIgnoreCase))
                    module = (string[])LanguagePrimitives.ConvertTo(p.Value, typeof(string[]), CultureInfo.InvariantCulture);
                else if (p.Name.Equals("FullyQualifiedModule", StringComparison.OrdinalIgnoreCase))
                    fullyQualifiedModule = (ModuleSpecification[])LanguagePrimitives.ConvertTo(p.Value, typeof(ModuleSpecification[]), CultureInfo.InvariantCulture);
                else if (p.Name.Equals("ArgumentList", StringComparison.OrdinalIgnoreCase))
                    argumentList = (object[])LanguagePrimitives.ConvertTo(p.Value, typeof(object[]), CultureInfo.InvariantCulture);
            RunspacePool rsPool = shell.PowerShell.GetRunspaceConnection() as RunspacePool;
            Dbg.Assert(rsPool != null, "Runspacepool cannot be null for a CreatePowerShell request");
            Guid clientRunspacePoolId = rsPool.InstanceId;
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.DiscoveryName, name));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.DiscoveryType, commandTypes));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.DiscoveryModule, module));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.DiscoveryFullyQualifiedModule, fullyQualifiedModule));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.DiscoveryArgumentList, argumentList));
                                               RemotingDataType.GetCommandMetadata,
                                               shell.InstanceId,
        /// creating a PowerShell on the server.
        /// create powershell message needs to be generated</param>
        /// | S |  Runspace | CRPID  |  CPID   | serialized    | CreatePowerShell    |
        /// |   |  Pool     |        |         | powershell,   |                     |
        /// |   |           |        |         | noInput,      |                     |
        /// |   |           |        |         | hostInfo,     |                     |
        /// |   |           |        |         | invocationset |                     |
        /// |   |           |        |         | tings, stream |                     |
        /// |   |           |        |         | options       |                     |
        internal static RemoteDataObject GenerateCreatePowerShell(ClientRemotePowerShell shell)
            PowerShell powerShell = shell.PowerShell;
            PSInvocationSettings settings = shell.Settings;
            Guid clientRunspacePoolId = Guid.Empty;
            HostInfo hostInfo;
            PSNoteProperty hostInfoProperty;
            RunspacePool rsPool = powerShell.GetRunspaceConnection() as RunspacePool;
            clientRunspacePoolId = rsPool.InstanceId;
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.PowerShell, powerShell.ToPSObjectForRemoting()));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.NoInput, shell.NoInput));
                hostInfo = new HostInfo(null);
                hostInfo.UseRunspaceHost = true;
                ApartmentState passedApartmentState = rsPool.ApartmentState;
                dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ApartmentState, passedApartmentState));
                dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.RemoteStreamOptions, RemoteStreamOptions.AddInvocationInfo));
                dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.AddToHistory, false));
                hostInfo = new HostInfo(settings.Host);
                if (settings.Host == null)
                ApartmentState passedApartmentState = settings.ApartmentState;
                dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.RemoteStreamOptions, settings.RemoteStreamOptions));
                dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.AddToHistory, settings.AddToHistory));
            hostInfoProperty = CreateHostInfoProperty(hostInfo);
            dataAsPSObject.Properties.Add(hostInfoProperty);
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.IsNested, shell.PowerShell.IsNested));
                                               RemotingDataType.CreatePowerShell,
        /// This method creates a remoting data structure handler message for transporting
        /// application private data from server to client.
        /// <param name="clientRunspacePoolId">Id of the client RunspacePool.</param>
        /// <param name="applicationPrivateData">Application private data.</param>
        /// | C |  Runspace | CRPID  |   -1    |    Data    | appl. private | PSPrimitive         |
        /// |   |    Pool   |        |         |            | data          |           Dictionary|
        internal static RemoteDataObject GenerateApplicationPrivateData(
                                    PSPrimitiveDictionary applicationPrivateData)
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.ApplicationPrivateData, applicationPrivateData));
                                               RemotingDataType.ApplicationPrivateData,
        /// This method creates a remoting data structure handler message for transporting a state
        /// information from server to client.
        /// | C |  Runspace | CRPID  |   -1    |    Data    | RunspacePool  | RunspacePoolState   |
        /// |   |    Pool   |        |         |            | StateInfo     | Info                |
        internal static RemoteDataObject GenerateRunspacePoolStateInfo(
                                    RunspacePoolStateInfo stateInfo)
            // BUGBUG: This object creation needs to be relooked
            // Add State Property
            PSNoteProperty stateProperty =
                        new PSNoteProperty(RemoteDataNameStrings.RunspaceState,
                            (int)(stateInfo.State));
            dataAsPSObject.Properties.Add(stateProperty);
            // Add Reason property
            if (stateInfo.Reason != null)
                PSNoteProperty exceptionProperty = GetExceptionProperty(
                    exception: stateInfo.Reason,
                    errorId: "RemoteRunspaceStateInfoReason",
                    category: ErrorCategory.NotSpecified);
                dataAsPSObject.Properties.Add(exceptionProperty);
                                               RemotingDataType.RunspacePoolStateInfo,
        /// This method creates a remoting data structure handler message for transporting a PowerShell
        /// event from server to client.
        /// <param name="e">PowerShell event.</param>
        /// | C |  Runspace | CRPID  |   -1    |    Data    | RunspacePool  | PSEventArgs         |
        /// |   |    Pool   |        |         |            | StateInfo     |                     |
        internal static RemoteDataObject GeneratePSEventArgs(Guid clientRunspacePoolId, PSEventArgs e)
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.PSEventArgsEventIdentifier, e.EventIdentifier));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.PSEventArgsSourceIdentifier, e.SourceIdentifier));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.PSEventArgsTimeGenerated, e.TimeGenerated));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.PSEventArgsSender, e.Sender));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.PSEventArgsSourceArgs, e.SourceArgs));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.PSEventArgsMessageData, e.MessageData));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.PSEventArgsComputerName, e.ComputerName));
            dataAsPSObject.Properties.Add(new PSNoteProperty(RemoteDataNameStrings.PSEventArgsRunspaceId, e.RunspaceId));
                                               RemotingDataType.PSEventArgs,
        /// This method creates a remoting data structure handler message to instruct the server to reset
        /// the single runspace on the server.
        /// <param name="clientRunspacePoolId"></param>
        /// --------------------------------------------------------------------------------------------
        /// | D |    TI     |  RPID  |   PID   |   Action          |      Data     |        Type        |
        /// | S |  Runspace | CRPID  |   -1    |  Reset server     |     None      | ResetRunspaceState |
        /// |   |    Pool   |        |         |  runspace state   |               |                    |
        /// ---------------------------------------------------------------------------------------------
        internal static RemoteDataObject GenerateResetRunspaceState(Guid clientRunspacePoolId, long callId)
                                               RemotingDataType.ResetRunspaceState,
        /// Returns the PS remoting protocol version associated with the provided.
        /// <param name="rsPool">RunspacePool.</param>
        /// <returns>PS remoting protocol version.</returns>
        internal static Version GetPSRemotingProtocolVersion(RunspacePool rsPool)
            return (rsPool != null && rsPool.RemoteRunspacePoolInternal != null) ?
                rsPool.RemoteRunspacePoolInternal.PSRemotingProtocolVersion : null;
        #endregion RunspacePool related
        #region PowerShell related
        /// This method creates a remoting data structure handler message for sending a powershell
        /// input data from the client to the server.
        /// <param name="data">Input data to send.</param>
        /// <param name="clientRemoteRunspacePoolId">Client runspace pool id.</param>
        /// <param name="clientPowerShellId">Client powershell id.</param>
        /// | S |PowerShell | CRPID  |   CPID  |    Data    |  input data   |   PowerShellInput   |
        internal static RemoteDataObject GeneratePowerShellInput(object data, Guid clientRemoteRunspacePoolId,
            Guid clientPowerShellId)
                                               RemotingDataType.PowerShellInput,
                                               clientRemoteRunspacePoolId,
                                               clientPowerShellId,
                                               data);
        /// This method creates a remoting data structure handler message for signalling
        /// end of input data for powershell.
        /// | S |PowerShell | CRPID  |   CPID  |    Data    | bool.         | PowerShellInputEnd  |
        /// |   |           |        |         |            | TrueString    |                     |
        internal static RemoteDataObject GeneratePowerShellInputEnd(Guid clientRemoteRunspacePoolId,
                                               RemotingDataType.PowerShellInputEnd,
        /// This method creates a remoting data structure handler message for transporting a
        /// powershell output data from server to client.
        /// <param name="data">Data to be sent.</param>
        /// <param name="clientPowerShellId">id of client powershell
        /// to which this information need to be delivered</param>
        /// <param name="clientRunspacePoolId">id of client runspacepool
        /// associated with this powershell</param>
        /// | C |PowerShell |  CRPID |  CPID   |    Data    | data to send  |  PowerShellOutput   |
        internal static RemoteDataObject GeneratePowerShellOutput(PSObject data, Guid clientPowerShellId,
            Guid clientRunspacePoolId)
                                               RemotingDataType.PowerShellOutput,
        /// powershell informational message (debug/verbose/warning/progress)from
        /// server to client.
        /// <param name="dataType">data type of this informational
        /// message</param>
        /// | C |PowerShell |  CRPID |  CPID   |    Data    | data to send  | DataType - debug,   |
        /// |   |           |        |         |            |               | verbose, warning    |
        internal static RemoteDataObject GeneratePowerShellInformational(object data,
            Guid clientRunspacePoolId, Guid clientPowerShellId, RemotingDataType dataType)
                                               PSObject.AsPSObject(data));
        /// powershell progress message from
        /// <param name="progressRecord">Progress record to send.</param>
        /// | C |PowerShell |  CRPID |  CPID   |    Data    | progress      | PowerShellProgress  |
        /// |   |           |        |         |            |   message     |                     |
        internal static RemoteDataObject GeneratePowerShellInformational(ProgressRecord progressRecord,
                                               RemotingDataType.PowerShellProgress,
                                               progressRecord.ToPSObjectForRemoting());
        /// powershell information stream message from
        /// <param name="informationRecord">Information record to send.</param>
        /// -----------------------------------------------------------------------------------------------
        /// | D |    TI     |  RPID  |   PID   |   Action   |      Data     |        Type                 |
        /// | C |PowerShell |  CRPID |  CPID   |    Data    | information   | PowerShellInformationStream |
        /// |   |           |        |         |            |   message     |                             |
        internal static RemoteDataObject GeneratePowerShellInformational(InformationRecord informationRecord,
            if (informationRecord == null)
                throw PSTraceSource.NewArgumentNullException(nameof(informationRecord));
                                               RemotingDataType.PowerShellInformationStream,
                                               informationRecord.ToPSObjectForRemoting());
        /// powershell error record from server to client.
        /// <param name="errorRecord">Error record to be sent.</param>
        /// | C |PowerShell |  CRPID |  CPID   |    Data    | error record  |   PowerShellError   |
        /// |   |           |        |         |            |    to send    |                     |
        internal static RemoteDataObject GeneratePowerShellError(object errorRecord,
                                               RemotingDataType.PowerShellErrorRecord,
                                               PSObject.AsPSObject(errorRecord));
        /// powershell state information from server to client.
        /// | C |PowerShell |  CRPID |  CPID   |    Data    | PSInvocation  | PowerShellStateInfo |
        /// |   |           |        |         |            | StateInfo     |                     |
        internal static RemoteDataObject GeneratePowerShellStateInfo(PSInvocationStateInfo stateInfo,
            Guid clientPowerShellId, Guid clientRunspacePoolId)
            // Encode Pipeline StateInfo as PSObject
            // Convert the state to int and add as property
            PSNoteProperty stateProperty = new PSNoteProperty(
                RemoteDataNameStrings.PipelineState, (int)(stateInfo.State));
            // Add exception property
                    errorId: "RemotePSInvocationStateInfoReason",
                                               RemotingDataType.PowerShellStateInfo,
        #endregion PowerShell related
        #region Exception
        /// Gets the error record from exception of type IContainsErrorRecord.
        /// ErrorRecord if exception is of type IContainsErrorRecord
        /// Null if exception is not of type IContainsErrorRecord
        internal static ErrorRecord GetErrorRecordFromException(Exception exception)
            Dbg.Assert(exception != null, "Caller should validate the data");
            IContainsErrorRecord cer = exception as IContainsErrorRecord;
            if (cer != null)
                // Exception inside the error record is ParentContainsErrorRecordException which
                // doesn't have stack trace. Replace it with top level exception.
                er = new ErrorRecord(er, exception);
        /// Gets a Note Property for the exception.
        /// If <paramref name="exception"/> is of not type IContainsErrorRecord, a new ErrorRecord is created.
        /// <param name="errorId">ErrorId to use if exception is not of type IContainsErrorRecord.</param>
        /// <param name="category">ErrorCategory to use if exception is not of type IContainsErrorRecord.</param>
        private static PSNoteProperty GetExceptionProperty(Exception exception, string errorId, ErrorCategory category)
            ErrorRecord er = GetErrorRecordFromException(exception) ??
                             new ErrorRecord(exception, errorId, category, null);
            return new PSNoteProperty(RemoteDataNameStrings.ExceptionAsErrorRecord, er);
        #endregion Exception
        #region Session related
        /// This method creates a remoting data structure handler message for transporting a session
        /// capability message. Should be used by client.
        /// <param name="capability">RemoteSession capability object to encode.</param>
        /// <param name="runspacePoolId"></param>
        /// | C |  Session  |  RPID  |  Empty  |    Data    |    session    | SessionCapability   |
        /// | / |           |        |         |            |   capability  |                     |
        /// | S |           |        |         |            |               |                     |
        internal static RemoteDataObject GenerateClientSessionCapability(RemoteSessionCapability capability,
                Guid runspacePoolId)
            PSObject temp = GenerateSessionCapability(capability);
            return RemoteDataObject.CreateFrom(capability.RemotingDestination,
                RemotingDataType.SessionCapability, runspacePoolId, Guid.Empty, temp);
        internal static RemoteDataObject GenerateServerSessionCapability(RemoteSessionCapability capability,
        private static PSObject GenerateSessionCapability(RemoteSessionCapability capability)
            PSObject temp = CreateEmptyPSObject();
            temp.Properties.Add(
                new PSNoteProperty(RemoteDataNameStrings.PS_STARTUP_PROTOCOL_VERSION_NAME, capability.ProtocolVersion));
                new PSNoteProperty(RemoteDataNameStrings.PSVersion, capability.PSVersion));
                new PSNoteProperty(RemoteDataNameStrings.SerializationVersion, capability.SerializationVersion));
        #endregion Session related
    /// Converts fields of PSObjects containing remoting messages to C# types.
    internal static class RemotingDecoder
        private static T ConvertPropertyValueTo<T>(string propertyName, object propertyValue)
            if (propertyName == null) // comes from internal caller
                throw PSTraceSource.NewArgumentNullException(nameof(propertyName));
                        T value = (T)Enum.Parse(typeof(T), stringValue, true);
                        throw new PSRemotingDataStructureException(
                            RemotingErrorIdStrings.CantCastPropertyToExpectedType,
                            typeof(T).FullName,
                            propertyValue.GetType().FullName);
                    Type underlyingType = Enum.GetUnderlyingType(typeof(T));
                    object underlyingValue = LanguagePrimitives.ConvertTo(propertyValue, underlyingType, CultureInfo.InvariantCulture);
                    T value = (T)underlyingValue;
            else if (typeof(T).Equals(typeof(PSObject)))
                if (propertyValue == null)
                    return default(T); // => "return null" for PSObject
                    return (T)(object)PSObject.AsPSObject(propertyValue);
            else if (propertyValue == null)
                if (!typeof(T).IsValueType)
                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    propertyValue != null ? propertyValue.GetType().FullName : "null");
            else if (propertyValue is T)
                return (T)(propertyValue);
            else if (propertyValue is PSObject)
                PSObject psObject = (PSObject)propertyValue;
                return ConvertPropertyValueTo<T>(propertyName, psObject.BaseObject);
            else if ((propertyValue is Hashtable) && (typeof(T).Equals(typeof(PSPrimitiveDictionary))))
                // rehydration of PSPrimitiveDictionary might not work when CreateRunspacePool message is received
                // (there is no runspace and so no type table at this point) so try converting manually
                    return (T)(object)(new PSPrimitiveDictionary((Hashtable)propertyValue));
        private static PSPropertyInfo GetProperty(PSObject psObject, string propertyName)
                throw PSTraceSource.NewArgumentNullException(nameof(psObject));
            PSPropertyInfo property = psObject.Properties[propertyName];
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.MissingProperty, propertyName);
        internal static T GetPropertyValue<T>(PSObject psObject, string propertyName)
            PSPropertyInfo property = GetProperty(psObject, propertyName);
            return ConvertPropertyValueTo<T>(propertyName, propertyValue);
        internal static IEnumerable<T> EnumerateListProperty<T>(PSObject psObject, string propertyName)
            IEnumerable e = GetPropertyValue<IEnumerable>(psObject, propertyName);
                foreach (object o in e)
                    yield return ConvertPropertyValueTo<T>(propertyName, o);
        internal static IEnumerable<KeyValuePair<TKey, TValue>> EnumerateHashtableProperty<TKey, TValue>(PSObject psObject, string propertyName)
            Hashtable h = GetPropertyValue<Hashtable>(psObject, propertyName);
            if (h != null)
                foreach (DictionaryEntry e in h)
                    TKey key = ConvertPropertyValueTo<TKey>(propertyName, e.Key);
                    TValue value = ConvertPropertyValueTo<TValue>(propertyName, e.Value);
                    yield return new KeyValuePair<TKey, TValue>(key, value);
        /// Decode and obtain the RunspacePool state info from the
        /// data object specified.
        /// <param name="dataAsPSObject">Data object to decode.</param>
        /// <returns>RunspacePoolStateInfo.</returns>
        internal static RunspacePoolStateInfo GetRunspacePoolStateInfo(PSObject dataAsPSObject)
            if (dataAsPSObject == null)
                throw PSTraceSource.NewArgumentNullException(nameof(dataAsPSObject));
            RunspacePoolState state = GetPropertyValue<RunspacePoolState>(dataAsPSObject, RemoteDataNameStrings.RunspaceState);
            Exception reason = GetExceptionFromStateInfoObject(dataAsPSObject);
            return new RunspacePoolStateInfo(state, reason);
        /// Decode and obtain the application private data from the
        /// <returns>Application private data.</returns>
        internal static PSPrimitiveDictionary GetApplicationPrivateData(PSObject dataAsPSObject)
            return GetPropertyValue<PSPrimitiveDictionary>(dataAsPSObject, RemoteDataNameStrings.ApplicationPrivateData);
        /// Gets the public key from the encoded message.
        /// <returns>Public key as string.</returns>
        internal static string GetPublicKey(PSObject dataAsPSObject)
            return GetPropertyValue<string>(dataAsPSObject, RemoteDataNameStrings.PublicKey);
        /// Gets the encrypted session key from the encoded message.
        /// <returns>Encrypted session key as string.</returns>
        internal static string GetEncryptedSessionKey(PSObject dataAsPSObject)
            return GetPropertyValue<string>(dataAsPSObject, RemoteDataNameStrings.EncryptedSessionKey);
        internal static PSEventArgs GetPSEventArgs(PSObject dataAsPSObject)
            int eventIdentifier = GetPropertyValue<int>(dataAsPSObject, RemoteDataNameStrings.PSEventArgsEventIdentifier);
            string sourceIdentifier = GetPropertyValue<string>(dataAsPSObject, RemoteDataNameStrings.PSEventArgsSourceIdentifier);
            object sender = GetPropertyValue<object>(dataAsPSObject, RemoteDataNameStrings.PSEventArgsSender);
            object messageData = GetPropertyValue<object>(dataAsPSObject, RemoteDataNameStrings.PSEventArgsMessageData);
            string computerName = GetPropertyValue<string>(dataAsPSObject, RemoteDataNameStrings.PSEventArgsComputerName);
            Guid runspaceId = GetPropertyValue<Guid>(dataAsPSObject, RemoteDataNameStrings.PSEventArgsRunspaceId);
            var sourceArgs = new List<object>();
            foreach (object argument in RemotingDecoder.EnumerateListProperty<object>(dataAsPSObject, RemoteDataNameStrings.PSEventArgsSourceArgs))
                sourceArgs.Add(argument);
            PSEventArgs eventArgs = new PSEventArgs(
                runspaceId,
                eventIdentifier,
                sender,
                sourceArgs.ToArray(),
                messageData == null ? null : PSObject.AsPSObject(messageData));
            eventArgs.TimeGenerated = GetPropertyValue<DateTime>(dataAsPSObject, RemoteDataNameStrings.PSEventArgsTimeGenerated);
        /// Decode and obtain the minimum runspaces to create in the
        /// runspace pool from the data object specified.
        /// <returns>Minimum runspaces.</returns>
        internal static int GetMinRunspaces(PSObject dataAsPSObject)
            return GetPropertyValue<int>(dataAsPSObject, RemoteDataNameStrings.MinRunspaces);
        /// Decode and obtain the maximum runspaces to create in the
        /// <returns>Maximum runspaces.</returns>
        internal static int GetMaxRunspaces(PSObject dataAsPSObject)
            return GetPropertyValue<int>(dataAsPSObject, RemoteDataNameStrings.MaxRunspaces);
        /// Decode and obtain the thread options for the runspaces in the
        /// <returns>Thread options.</returns>
        internal static PSPrimitiveDictionary GetApplicationArguments(PSObject dataAsPSObject)
            // rehydration might not work yet (there is no type table before a runspace is created)
            // so try to cast ApplicationArguments to PSPrimitiveDictionary manually
            return GetPropertyValue<PSPrimitiveDictionary>(dataAsPSObject, RemoteDataNameStrings.ApplicationArguments);
        /// Generates RunspacePoolInitInfo object from a received PSObject.
        /// <returns>RunspacePoolInitInfo generated.</returns>
        internal static RunspacePoolInitInfo GetRunspacePoolInitInfo(PSObject dataAsPSObject)
            int maxRS = GetPropertyValue<int>(dataAsPSObject, RemoteDataNameStrings.MaxRunspaces);
            int minRS = GetPropertyValue<int>(dataAsPSObject, RemoteDataNameStrings.MinRunspaces);
            return new RunspacePoolInitInfo(minRS, maxRS);
        internal static PSThreadOptions GetThreadOptions(PSObject dataAsPSObject)
            return GetPropertyValue<PSThreadOptions>(dataAsPSObject, RemoteDataNameStrings.ThreadOptions);
        /// Decode and obtain the host info for the host
        /// associated with the runspace pool.
        /// <param name="dataAsPSObject">DataAsPSObject object to decode.</param>
        /// <returns>Host information.</returns>
        internal static HostInfo GetHostInfo(PSObject dataAsPSObject)
            PSObject propertyValue = GetPropertyValue<PSObject>(dataAsPSObject, RemoteDataNameStrings.HostInfo);
            return RemoteHostEncoder.DecodeObject(propertyValue, typeof(HostInfo)) as HostInfo;
        /// Gets the exception if any from the serialized state info object.
        private static Exception GetExceptionFromStateInfoObject(PSObject stateInfo)
            // Check if exception is encoded as errorrecord
            PSPropertyInfo property = stateInfo.Properties[RemoteDataNameStrings.ExceptionAsErrorRecord];
                return GetExceptionFromSerializedErrorRecord(property.Value);
            // Exception is not present and return null.
        /// Get the exception from serialized error record.
        /// <param name="serializedErrorRecord"></param>
        internal static Exception GetExceptionFromSerializedErrorRecord(object serializedErrorRecord)
            ErrorRecord er = ErrorRecord.FromPSObjectForRemoting(PSObject.AsPSObject(serializedErrorRecord));
            if (er == null)
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.DecodingErrorForErrorRecord);
                return er.Exception;
        /// Gets the output from the message.
        /// <param name="data">Object to decode.</param>
        /// <returns>Output object.</returns>
        /// <remarks>the current implementation does nothing,
        /// however this method is there in place as the
        /// packaging of output data may change in the future</remarks>
        internal static object GetPowerShellOutput(object data)
        /// Gets the PSInvocationStateInfo from the data.
        /// <returns>PSInvocationInfo.</returns>
        internal static PSInvocationStateInfo GetPowerShellStateInfo(object data)
            if (data is not PSObject dataAsPSObject)
                    RemotingErrorIdStrings.DecodingErrorForPowerShellStateInfo);
            PSInvocationState state = GetPropertyValue<PSInvocationState>(dataAsPSObject, RemoteDataNameStrings.PipelineState);
            return new PSInvocationStateInfo(state, reason);
        /// Gets the ErrorRecord from the message.
        /// <param name="data">Data to decode.</param>
        /// <returns>Error record.</returns>
        internal static ErrorRecord GetPowerShellError(object data)
                throw PSTraceSource.NewArgumentNullException(nameof(data));
            PSObject dataAsPSObject = data as PSObject;
            ErrorRecord errorRecord = ErrorRecord.FromPSObjectForRemoting(dataAsPSObject);
        /// Gets the WarningRecord from the message.
        internal static WarningRecord GetPowerShellWarning(object data)
            return new WarningRecord((PSObject)data);
        /// Gets the VerboseRecord from the message.
        internal static VerboseRecord GetPowerShellVerbose(object data)
            return new VerboseRecord((PSObject)data);
        /// Gets the DebugRecord from the message.
        internal static DebugRecord GetPowerShellDebug(object data)
            return new DebugRecord((PSObject)data);
        /// Gets the ProgressRecord from the message.
        internal static ProgressRecord GetPowerShellProgress(object data)
            PSObject dataAsPSObject = PSObject.AsPSObject(data);
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.CantCastRemotingDataToPSObject, data.GetType().FullName);
            return ProgressRecord.FromPSObjectForRemoting(dataAsPSObject);
        /// Gets the InformationRecord from the message.
        internal static InformationRecord GetPowerShellInformation(object data)
            return InformationRecord.FromPSObjectForRemoting(dataAsPSObject);
        /// Gets the PowerShell object from the specified data.
        /// <returns>Deserialized PowerShell object.</returns>
        internal static PowerShell GetPowerShell(object data)
            PSObject powerShellAsPSObject = GetPropertyValue<PSObject>(dataAsPSObject, RemoteDataNameStrings.PowerShell);
            return PowerShell.FromPSObjectForRemoting(powerShellAsPSObject);
        internal static PowerShell GetCommandDiscoveryPipeline(object data)
            CommandTypes commandType = GetPropertyValue<CommandTypes>(dataAsPSObject, RemoteDataNameStrings.DiscoveryType);
            string[] name;
            if (GetPropertyValue<PSObject>(dataAsPSObject, RemoteDataNameStrings.DiscoveryName) != null)
                IEnumerable<string> tmp = EnumerateListProperty<string>(dataAsPSObject, RemoteDataNameStrings.DiscoveryName);
                name = new List<string>(tmp).ToArray();
                name = new string[] { "*" };
            string[] module;
            if (GetPropertyValue<PSObject>(dataAsPSObject, RemoteDataNameStrings.DiscoveryModule) != null)
                IEnumerable<string> tmp = EnumerateListProperty<string>(dataAsPSObject, RemoteDataNameStrings.DiscoveryModule);
                module = new List<string>(tmp).ToArray();
                module = new string[] { string.Empty };
            ModuleSpecification[] fullyQualifiedName = null;
            if (DeserializingTypeConverter.GetPropertyValue<PSObject>(dataAsPSObject,
                                                                      RemoteDataNameStrings.DiscoveryFullyQualifiedModule,
                                                                      DeserializingTypeConverter.RehydrationFlags.NullValueOk | DeserializingTypeConverter.RehydrationFlags.MissingPropertyOk) != null)
                IEnumerable<ModuleSpecification> tmp = EnumerateListProperty<ModuleSpecification>(dataAsPSObject, RemoteDataNameStrings.DiscoveryFullyQualifiedModule);
                fullyQualifiedName = new List<ModuleSpecification>(tmp).ToArray();
            object[] argumentList;
            if (GetPropertyValue<PSObject>(dataAsPSObject, RemoteDataNameStrings.DiscoveryArgumentList) != null)
                IEnumerable<object> tmp = EnumerateListProperty<object>(dataAsPSObject, RemoteDataNameStrings.DiscoveryArgumentList);
                argumentList = new List<object>(tmp).ToArray();
                argumentList = null;
            powerShell.AddParameter("Name", name);
            powerShell.AddParameter("CommandType", commandType);
                powerShell.AddParameter("FullyQualifiedModule", fullyQualifiedName);
                powerShell.AddParameter("Module", module);
            powerShell.AddParameter("ArgumentList", argumentList);
        /// Gets the NoInput setting from the specified data.
        /// <returns><see langword="true"/> if there is no pipeline input; <see langword="false"/> otherwise.</returns>
        internal static bool GetNoInput(object data)
            return GetPropertyValue<bool>(dataAsPSObject, RemoteDataNameStrings.NoInput);
        /// Gets the AddToHistory setting from the specified data.
        /// <returns><see langword="true"/> if there is addToHistory data; <see langword="false"/> otherwise.</returns>
        internal static bool GetAddToHistory(object data)
            return GetPropertyValue<bool>(dataAsPSObject, RemoteDataNameStrings.AddToHistory);
        /// Gets the IsNested setting from the specified data.
        /// <returns><see langword="true"/> if there is IsNested data; <see langword="false"/> otherwise.</returns>
        internal static bool GetIsNested(object data)
            return GetPropertyValue<bool>(dataAsPSObject, RemoteDataNameStrings.IsNested);
        /// Gets the invocation settings information from the message.
        /// <param name="data"></param>
        internal static ApartmentState GetApartmentState(object data)
            return GetPropertyValue<ApartmentState>(dataAsPSObject, RemoteDataNameStrings.ApartmentState);
        /// Gets the stream options from the message.
        internal static RemoteStreamOptions GetRemoteStreamOptions(object data)
            return GetPropertyValue<RemoteStreamOptions>(dataAsPSObject, RemoteDataNameStrings.RemoteStreamOptions);
        /// Decodes a RemoteSessionCapability object.
        /// <returns>RemoteSessionCapability object.</returns>
        internal static RemoteSessionCapability GetSessionCapability(object data)
                    RemotingErrorIdStrings.CantCastRemotingDataToPSObject, data.GetType().FullName);
            Version protocolVersion = GetPropertyValue<Version>(dataAsPSObject, RemoteDataNameStrings.PS_STARTUP_PROTOCOL_VERSION_NAME);
            Version psVersion = GetPropertyValue<Version>(dataAsPSObject, RemoteDataNameStrings.PSVersion);
            Version serializationVersion = GetPropertyValue<Version>(dataAsPSObject,
                RemoteDataNameStrings.SerializationVersion);
            RemoteSessionCapability result = new RemoteSessionCapability(
                RemotingDestination.InvalidDestination,
                protocolVersion, psVersion, serializationVersion);
        /// Checks if the server supports batch invocation.
        /// <param name="runspace">Runspace instance.</param>
        /// <returns>True if batch invocation is supported, false if not.</returns>
        internal static bool ServerSupportsBatchInvocation(Runspace runspace)
            if (runspace == null || runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen)
            return (runspace.GetRemoteProtocolVersion() >= RemotingConstants.ProtocolVersion_2_2);
