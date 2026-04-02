    /// Computer target type.
    public enum TargetMachineType
        /// Target is a machine with which the session is based on networking.
        RemoteMachine,
        /// Target is a virtual machine with which the session is based on Hyper-V socket.
        VirtualMachine,
        /// Target is a container with which the session is based on Hyper-V socket (Hyper-V
        /// container) or named pipe (windows container)
        Container
    /// Class that exposes read only properties and which conveys information
    /// about a remote runspace object to the user. The class serves the
    /// following purpose:
    ///     1. Exposes useful information to the user as properties
    ///     2. Shields the remote runspace object from directly being exposed
    ///        to the user. This way, the user will not be able to directly
    ///        act upon the object, but instead will have to use the remoting
    ///        cmdlets. This will prevent any unpredictable behavior.
    public sealed class PSSession
        private RemoteRunspace _remoteRunspace;
        private string _transportName;
        private static int s_seed = 0;
        /// Type of the computer target.
        public TargetMachineType ComputerType { get; set; }
        /// Name of the computer target.
                return _remoteRunspace.ConnectionInfo.ComputerName;
        /// Id of the container target.
        public string ContainerId
                if (ComputerType == TargetMachineType.Container)
                    ContainerConnectionInfo connectionInfo = _remoteRunspace.ConnectionInfo as ContainerConnectionInfo;
                    return connectionInfo.ContainerProc.ContainerId;
        /// Name of the virtual machine target.
        public string VMName
                if (ComputerType == TargetMachineType.VirtualMachine)
        /// Guid of the virtual machine target.
        public Guid? VMId
                    VMConnectionInfo connectionInfo = _remoteRunspace.ConnectionInfo as VMConnectionInfo;
                    return connectionInfo.VMGuid;
        /// Shell which is executed in the remote machine.
        public string ConfigurationName { get; }
        /// InstanceID that identifies this runspace.
                return _remoteRunspace.InstanceId;
        /// SessionId of this runspace. This is unique only across
        /// a session.
        /// Friendly name for identifying this runspace.
        /// Indicates whether the specified runspace is available
        /// for executing commands.
        public RunspaceAvailability Availability
                return Runspace.RunspaceAvailability;
        /// Optionally sent by the remote server when creating a new session / runspace.
        public PSPrimitiveDictionary ApplicationPrivateData
                return this.Runspace.GetApplicationPrivateData();
        /// The remote runspace object based on which this information object
        /// is derived.
        /// <remarks>This property is marked internal to allow other cmdlets
        /// to get access to the RemoteRunspace object and operate on it like
        /// for instance test-runspace, close-runspace etc</remarks>
                return _remoteRunspace;
        /// Name of the transport used.
        public string Transport => GetTransportName();
        /// ToString method override.
            // PSSession is a PowerShell type name and so should not be localized.
            const string formatString = "[PSSession]{0}";
            return StringUtil.Format(formatString, Name);
        /// Internal method to insert a runspace into a PSSession object.
        /// This is used only for Disconnect/Reconnect scenarios where the
        /// new runspace is a reconstructed runspace having the same Guid
        /// as the existing runspace.
        /// <param name="remoteRunspace">Runspace to insert.</param>
        /// <returns>Boolean indicating if runspace was inserted.</returns>
        internal bool InsertRunspace(RemoteRunspace remoteRunspace)
            if (remoteRunspace == null ||
                remoteRunspace.InstanceId != _remoteRunspace.InstanceId)
        /// This constructor will be used to created a remote runspace info
        /// object with a auto generated name.
        /// <param name="remoteRunspace">Remote runspace object for which
        /// the info object need to be created</param>
        internal PSSession(RemoteRunspace remoteRunspace)
            // Use passed in session Id, if available.
            if (remoteRunspace.PSSessionId != -1)
                Id = remoteRunspace.PSSessionId;
                Id = System.Threading.Interlocked.Increment(ref s_seed);
                remoteRunspace.PSSessionId = Id;
            // Use passed in friendly name, if available.
            if (!string.IsNullOrEmpty(remoteRunspace.PSSessionName))
                Name = remoteRunspace.PSSessionName;
                Name = "Runspace" + Id;
                remoteRunspace.PSSessionName = Name;
            switch (remoteRunspace.ConnectionInfo)
                case WSManConnectionInfo _:
                    ComputerType = TargetMachineType.RemoteMachine;
                    string fullShellName = WSManConnectionInfo.ExtractPropertyAsWsManConnectionInfo<string>(
                        remoteRunspace.ConnectionInfo,
                    ConfigurationName = GetDisplayShellName(fullShellName);
                case VMConnectionInfo vmConnectionInfo:
                    ComputerType = TargetMachineType.VirtualMachine;
                    ConfigurationName = vmConnectionInfo.ConfigurationName;
                case ContainerConnectionInfo containerConnectionInfo:
                    ComputerType = TargetMachineType.Container;
                    ConfigurationName = containerConnectionInfo.ContainerProc.ConfigurationName;
                case SSHConnectionInfo _:
                    ConfigurationName = "DefaultShell";
                case NewProcessConnectionInfo _:
                    // Default for custom connection and transports.
        /// Generates and returns the runspace name.
        /// <returns>Auto generated name.</returns>
        private string GetTransportName()
            switch (_remoteRunspace.ConnectionInfo)
                    return "WSMan";
                    return "SSH";
                case NamedPipeConnectionInfo _:
                    return "NamedPipe";
                case ContainerConnectionInfo _:
                    return "Container";
                    return "Process";
                case VMConnectionInfo _:
                    return "VMBus";
                    return string.IsNullOrEmpty(_transportName) ? "Custom" : _transportName;
        /// Returns shell configuration name with shell prefix removed.
        /// <param name="shell">Shell configuration name.</param>
        /// <returns>Display shell name.</returns>
        private static string GetDisplayShellName(string shell)
            const string shellPrefix = System.Management.Automation.Remoting.Client.WSManNativeApi.ResourceURIPrefix;
            int index = shell.IndexOf(shellPrefix, StringComparison.OrdinalIgnoreCase);
            return (index == 0) ? shell.Substring(shellPrefix.Length) : shell;
        /// Creates a PSSession object from the provided remote runspace object.
        /// If psCmdlet argument is non-null, then the new PSSession object is added to the
        /// session runspace repository (Get-PSSession).
        /// <param name="runspace">Runspace for the new PSSession.</param>
        /// <param name="transportName">Optional transport name.</param>
        /// <param name="psCmdlet">Optional cmdlet associated with the PSSession creation.</param>
        public static PSSession Create(
            string transportName,
            PSCmdlet psCmdlet)
                throw new PSArgumentException(RemotingErrorIdStrings.InvalidPSSessionArgument);
            var psSession = new PSSession(remoteRunspace)
                _transportName = transportName
            psCmdlet?.RunspaceRepository.Add(psSession);
            return psSession;
        /// Generates a unique runspace id.
        /// <param name="rtnId">Returned Id.</param>
        /// <returns>Returned name.</returns>
        internal static string GenerateRunspaceName(out int rtnId)
            int id = GenerateRunspaceId();
            rtnId = id;
            return "Runspace" + id.ToString();
        /// Increments and returns a session unique runspace Id.
        /// <returns>Id.</returns>
        internal static int GenerateRunspaceId()
            return System.Threading.Interlocked.Increment(ref s_seed);
