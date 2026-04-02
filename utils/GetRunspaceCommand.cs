    /// This cmdlet returns runspaces in the PowerShell session.
    [Cmdlet(VerbsCommon.Get, "Runspace", DefaultParameterSetName = GetRunspaceCommand.NameParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096616")]
    [OutputType(typeof(Runspace))]
    public sealed class GetRunspaceCommand : PSCmdlet
        /// Specifies name or names of Runspaces to return.
                   ParameterSetName = GetRunspaceCommand.NameParameterSet)]
        /// Specifies one or more Ids of Runspaces to return.
                   ParameterSetName = GetRunspaceCommand.IdParameterSet)]
        /// Specifies one or more InstanceId Guids of Runspaces to return.
                   ParameterSetName = GetRunspaceCommand.InstanceIdParameterSet)]
        /// Process record.
            IReadOnlyList<Runspace> results;
            if ((ParameterSetName == GetRunspaceCommand.NameParameterSet) && ((Name == null) || Name.Length == 0))
                    case GetRunspaceCommand.NameParameterSet:
                        results = GetRunspaceUtils.GetRunspacesByName(Name);
                    case GetRunspaceCommand.IdParameterSet:
                        results = GetRunspaceUtils.GetRunspacesById(Id);
                    case GetRunspaceCommand.InstanceIdParameterSet:
                        results = GetRunspaceUtils.GetRunspacesByInstanceId(InstanceId);
                        Dbg.Assert(false, "Unknown parameter set in GetRunspaceCommand");
                        results = new List<Runspace>().AsReadOnly();
            foreach (Runspace runspace in results)
                WriteObject(runspace);
    #region GetRunspaceUtils
    internal static class GetRunspaceUtils
        internal static IReadOnlyList<Runspace> GetAllRunspaces()
            return Runspace.RunspaceList;
        internal static IReadOnlyList<Runspace> GetRunspacesByName(string[] names)
            List<Runspace> rtnRunspaces = new();
            IReadOnlyList<Runspace> runspaces = Runspace.RunspaceList;
            foreach (string name in names)
                WildcardPattern namePattern = WildcardPattern.Get(name, WildcardOptions.IgnoreCase);
                foreach (Runspace runspace in runspaces)
                    if (namePattern.IsMatch(runspace.Name))
                        rtnRunspaces.Add(runspace);
            return rtnRunspaces.AsReadOnly();
        internal static IReadOnlyList<Runspace> GetRunspacesById(int[] ids)
            foreach (int id in ids)
                WeakReference<Runspace> runspaceRef;
                if (Runspace.RunspaceDictionary.TryGetValue(id, out runspaceRef))
                    Runspace runspace;
                    if (runspaceRef.TryGetTarget(out runspace))
        internal static IReadOnlyList<Runspace> GetRunspacesByInstanceId(Guid[] instanceIds)
            foreach (Guid instanceId in instanceIds)
                    if (runspace.InstanceId == instanceId)
                        // Because of disconnected remote runspace sessions, it is possible to have
                        // more than one runspace with the same instance Id (remote session ids are
                        // the same as the runspace object instance Id).
    /// This cmdlet is used to retrieve runspaces from the global cache
    /// and write it to the pipeline. The runspaces are wrapped and
    /// returned as PSSession objects.
    /// List all the available runspaces
    ///     get-pssession
    /// Get the PSSession from session name
    ///     get-pssession -Name sessionName
    /// Get the PSSession for the specified ID
    ///     get-pssession -Id sessionId
    /// Get the PSSession for the specified instance Guid
    ///     get-pssession -InstanceId sessionGuid
    /// Get PSSessions from remote computer.  Optionally filter on state, session instanceid or session name.
    ///     get-psession -ComputerName computerName -StateFilter Disconnected
    /// Get PSSessions from virtual machine. Optionally filter on state, session instanceid or session name.
    ///     get-psession -VMName vmName -Name sessionName
    /// Get PSSessions from container. Optionally filter on state, session instanceid or session name.
    ///     get-psession -ContainerId containerId -InstanceId instanceId.
    [Cmdlet(VerbsCommon.Get, "PSSession", DefaultParameterSetName = PSRunspaceCmdlet.NameParameterSet,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096697", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class GetPSSessionCommand : PSRunspaceCmdlet, IDisposable
                   ParameterSetName = GetPSSessionCommand.ComputerNameParameterSet)]
                   ParameterSetName = GetPSSessionCommand.ComputerInstanceIdParameterSet)]
                   ParameterSetName = GetPSSessionCommand.ConnectionUriParameterSet)]
                   ParameterSetName = GetPSSessionCommand.ConnectionUriInstanceIdParameterSet)]
        /// If this parameter is not specified then all sessions that match other filters are returned.
                           ParameterSetName = GetPSSessionCommand.ContainerIdParameterSet)]
                           ParameterSetName = GetPSSessionCommand.ContainerIdInstanceIdParameterSet)]
                           ParameterSetName = GetPSSessionCommand.VMIdParameterSet)]
                           ParameterSetName = GetPSSessionCommand.VMIdInstanceIdParameterSet)]
                           ParameterSetName = GetPSSessionCommand.VMNameParameterSet)]
                           ParameterSetName = GetPSSessionCommand.VMNameInstanceIdParameterSet)]
        [Parameter(ParameterSetName = GetPSSessionCommand.ConnectionUriParameterSet)]
        [Parameter(ParameterSetName = GetPSSessionCommand.ConnectionUriInstanceIdParameterSet)]
        /// Session names to filter on.
        [Parameter(ParameterSetName = GetPSSessionCommand.ComputerNameParameterSet)]
        [Parameter(ParameterSetName = GetPSSessionCommand.ContainerIdParameterSet)]
        [Parameter(ParameterSetName = GetPSSessionCommand.VMIdParameterSet)]
        [Parameter(ParameterSetName = GetPSSessionCommand.VMNameParameterSet)]
        /// Instance Ids to filter on.
        [Parameter(ParameterSetName = GetPSSessionCommand.ComputerInstanceIdParameterSet,
        [Parameter(ParameterSetName = GetPSSessionCommand.ConnectionUriInstanceIdParameterSet,
        [Parameter(ParameterSetName = GetPSSessionCommand.ContainerIdInstanceIdParameterSet,
        [Parameter(ParameterSetName = GetPSSessionCommand.VMIdInstanceIdParameterSet,
        [Parameter(ParameterSetName = GetPSSessionCommand.VMNameInstanceIdParameterSet,
        [Parameter(ParameterSetName = GetPSSessionCommand.ComputerInstanceIdParameterSet)]
        /// Filters returned remote runspaces based on runspace state.
        [Parameter(ParameterSetName = GetPSSessionCommand.ContainerIdInstanceIdParameterSet)]
        [Parameter(ParameterSetName = GetPSSessionCommand.VMIdInstanceIdParameterSet)]
        [Parameter(ParameterSetName = GetPSSessionCommand.VMNameInstanceIdParameterSet)]
        public SessionFilterState State { get; set; }
        /// Resolves shellname.
            if (ComputerName?.Length > 0)
                ErrorRecord err = new(
                    new NotImplementedException(
                            RemotingErrorIdStrings.UnsupportedOSForRemoteEnumeration,
                            RuntimeInformation.OSDescription)),
                    "PSSessionComputerNameUnix",
            ConfigurationName ??= string.Empty;
        /// Get the list of runspaces from the global cache and write them
        /// down. If no computername or instance id is specified then
        /// list all runspaces.
            if ((ParameterSetName == GetPSSessionCommand.NameParameterSet) && ((Name == null) || (Name.Length == 0)))
                // that means Get-PSSession (with no parameters)..so retrieve all the runspaces.
                GetAllRunspaces(true, true);
            else if (ParameterSetName == GetPSSessionCommand.ComputerNameParameterSet ||
                     ParameterSetName == GetPSSessionCommand.ComputerInstanceIdParameterSet ||
                     ParameterSetName == GetPSSessionCommand.ConnectionUriParameterSet ||
                     ParameterSetName == GetPSSessionCommand.ConnectionUriInstanceIdParameterSet)
                // Perform the remote query for each provided computer name.
                QueryForRemoteSessions();
                GetMatchingRunspaces(true, true, this.State, this.ConfigurationName);
        /// Creates a connectionInfo object for each computer name and performs a remote
        /// session query for each computer filtered by the filterState parameter.
        private void QueryForRemoteSessions()
            // Get collection of connection objects for each computer name or
            // connection uri.
            // Query for sessions.
            Collection<PSSession> results = _queryRunspaces.GetDisconnectedSessions(connectionInfos, this.Host, _stream,
                                                                                        State, InstanceId, Name, ConfigurationName);
                if (this.IsStopping)
            // Write each session object.
            foreach (PSSession session in results)
            if (ParameterSetName == GetPSSessionCommand.ComputerNameParameterSet ||
                ParameterSetName == GetPSSessionCommand.ComputerInstanceIdParameterSet)
            else if (ParameterSetName == GetPSSessionCommand.ConnectionUriParameterSet ||
            if (ParameterSetName != GetPSSessionCommand.ConnectionUriParameterSet &&
                ParameterSetName != GetPSSessionCommand.ConnectionUriInstanceIdParameterSet)
        /// Dispose method of IDisposable.
