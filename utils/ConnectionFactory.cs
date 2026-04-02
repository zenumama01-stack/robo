    /// Defines a factory class for creating Runspace objects.
    public static class RunspaceFactory
        static RunspaceFactory()
            // Set ETW activity Id
            Guid activityId = EtwActivity.GetActivityId();
            if (activityId == Guid.Empty)
                EtwActivity.SetActivityId(EtwActivity.CreateActivityId());
        #region Runspace Factory
        /// Creates a runspace using host of type <see cref="DefaultHost"/>.
        /// A runspace object.
        public static Runspace CreateRunspace()
            PSHost host = new DefaultHost(CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
            return CreateRunspace(host);
        /// Creates a runspace using specified host. This runspace is created using the
        /// configuration information from EntryAssembly.
        /// The explicit PSHost implementation.
        /// A runspace object
        /// Thrown when host is null.
        public static Runspace CreateRunspace(PSHost host)
            return new LocalRunspace(host, InitialSessionState.CreateDefault());
        /// Creates a runspace using <see cref="DefaultHost"/>
        /// InitialSessionState information for the runspace.
        /// Thrown when initialSessionState is null
        public static Runspace CreateRunspace(InitialSessionState initialSessionState)
            return CreateRunspace(host, initialSessionState);
        /// Creates a runspace using specified PSHost and InitialSessionState.
        /// Host implementation for runspace.
        /// Thrown when host is null
        public static Runspace CreateRunspace(PSHost host, InitialSessionState initialSessionState)
            return new LocalRunspace(host, initialSessionState);
        internal static Runspace CreateRunspaceFromSessionStateNoClone(PSHost host, InitialSessionState initialSessionState)
            return new LocalRunspace(host, initialSessionState, true);
        #region RunspacePool Factory
        /// Creates a RunspacePool with MaxRunspaces 1 and MinRunspaces 1.
        public static RunspacePool CreateRunspacePool()
            return CreateRunspacePool(1, 1);
        /// Creates a RunspacePool
        /// <paramref name="maxRunspaces"/>
        /// limits the number of Runspaces that can exist in this
        /// pool. The minimum pool size is set to <paramref name="minPoolSoze"/>.
        /// <param name="minRunspaces">
        /// The minimum number of Runspaces that exist in this
        /// pool. Should be greater than or equal to 1.
        /// <param name="maxRunspaces">
        /// The maximum number of Runspaces that can exist in this
        /// Maximum runspaces is less than 1.
        /// Minimum runspaces is less than 1.
        public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces)
            return CreateRunspacePool(minRunspaces, maxRunspaces,
                new DefaultHost
                    CultureInfo.CurrentUICulture
        /// Creates a RunspacePool using the supplied <paramref name="initialSessionState"/>.
        /// The minimum runspaces size is set to 1. The maximum runspaces size is
        /// set to 1.
        /// initialSessionState to use when creating a new
        /// Runspace in the pool.
        /// InitialSessionState is null.
        public static RunspacePool CreateRunspacePool(InitialSessionState initialSessionState)
            return CreateRunspacePool(1, 1, initialSessionState,
        /// Creates a RunspacePool using the supplied <paramref name="host"/>,
        /// <paramref name="minRunspaces"/> and <paramref name="maxRunspaces"/>
        /// The minimum number of Runspaces that can exist in this pool.
        /// Should be greater than or equal to 1.
        /// The maximum number of Runspaces that can exist in this pool.
        /// <paramref name="host"/> is null.
        /// A local runspacepool instance.
        public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, PSHost host)
            return new RunspacePool(minRunspaces, maxRunspaces, host);
        /// Creates a RunspacePool using the supplied <paramref name="initialSessionState"/>,
        /// initialSessionState to use when creating a new Runspace in the
        /// pool.
        /// <paramref name="initialSessionState"/> is null.
        public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces,
            InitialSessionState initialSessionState, PSHost host)
            return new RunspacePool(minRunspaces,
                maxRunspaces, initialSessionState, host);
        #region RunspacePool - remote Factory
        /// on the specified remote computer.
        /// pool. The minimum pool size is set to
        /// <paramref name="minPoolSoze"/>.
        /// The minimum number of Runspace that should exist in this
        /// pool. Should be greater than 1.
        /// <param name="connectionInfo">RunspaceConnectionInfo object describing
        /// the remote computer on which this runspace pool needs to be
        /// created</param>
        /// Maximum Pool size is less than 1.
        /// Minimum Pool size is less than 1.
        /// connectionInfo is null</exception>
        public static RunspacePool CreateRunspacePool(int minRunspaces,
                                        int maxRunspaces, RunspaceConnectionInfo connectionInfo)
            return CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo, null);
        /// on the specified remote runspace computer.
        /// <param name="host">Host associated with this
        /// runspace pool</param>
            int maxRunspaces, RunspaceConnectionInfo connectionInfo, PSHost host)
            return CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo, host, null);
        /// <param name="typeTable">
        /// The TypeTable to use while deserializing/serializing remote objects.
        /// TypeTable has the following information used by serializer:
        ///   1. SerializationMethod
        ///   2. SerializationDepth
        ///   3. SpecificSerializationProperties
        /// TypeTable has the following information used by deserializer:
        ///   1. TargetTypeForDeserialization
        ///   2. TypeConverter
        /// If <paramref name="typeTable"/> is null no custom serialization/deserialization
        /// can be done. Default PowerShell behavior will be used in this case.
            int maxRunspaces, RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
            return CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo, host, typeTable, null);
        /// <param name="applicationArguments">
        /// Application arguments the server can see in <see cref="System.Management.Automation.Remoting.PSSenderInfo.ApplicationArguments"/>
            int maxRunspaces, RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable, PSPrimitiveDictionary applicationArguments)
            if (connectionInfo is not WSManConnectionInfo &&
                connectionInfo is not NewProcessConnectionInfo &&
                connectionInfo is not NamedPipeConnectionInfo &&
                connectionInfo is not VMConnectionInfo &&
                connectionInfo is not ContainerConnectionInfo)
            if (connectionInfo is WSManConnectionInfo)
                RemotingCommandUtil.CheckHostRemotingPrerequisites();
            return new RunspacePool(minRunspaces, maxRunspaces, typeTable, host, applicationArguments, connectionInfo);
        #endregion RunspacePool - remote Factory
        #region Runspace - Remote Factory
        /// Creates a remote Runspace.
        /// <param name="connectionInfo">It defines connection path to a remote runspace that needs to be created.</param>
        /// <returns>A remote Runspace.</returns>
        public static Runspace CreateRunspace(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
            return CreateRunspace(connectionInfo, host, typeTable, null, null);
        public static Runspace CreateRunspace(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable, PSPrimitiveDictionary applicationArguments)
            return CreateRunspace(connectionInfo, host, typeTable, applicationArguments, null);
        /// <param name="name">Name for remote runspace.</param>
        public static Runspace CreateRunspace(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable, PSPrimitiveDictionary applicationArguments, string name)
            return new RemoteRunspace(typeTable, connectionInfo, host, applicationArguments, name);
        public static Runspace CreateRunspace(PSHost host, RunspaceConnectionInfo connectionInfo)
            return CreateRunspace(connectionInfo, host, null);
        public static Runspace CreateRunspace(RunspaceConnectionInfo connectionInfo)
            return CreateRunspace(null, connectionInfo);
        #endregion Runspace - Remote Factory
        #region V3 Extensions
        /// Creates an out-of-process remote Runspace.
        /// <returns>An out-of-process remote Runspace.</returns>
        public static Runspace CreateOutOfProcessRunspace(TypeTable typeTable)
            NewProcessConnectionInfo connectionInfo = new NewProcessConnectionInfo(null);
            return CreateRunspace(connectionInfo, null, typeTable);
        /// <param name="processInstance">It represents a PowerShell process that is used for an out-of-process remote Runspace</param>
        public static Runspace CreateOutOfProcessRunspace(TypeTable typeTable, PowerShellProcessInstance processInstance)
            NewProcessConnectionInfo connectionInfo = new NewProcessConnectionInfo(null) { Process = processInstance };
        #endregion V3 Extensions
