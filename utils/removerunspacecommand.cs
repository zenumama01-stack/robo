    /// This cmdlet stops the runspace and frees the resources associated with
    /// that runspace. If any execution is in process in that runspace, it is
    /// stopped. Also, the runspace is removed from the global cache.
    /// This cmdlet can be used in the following ways:
    /// Remove the runspace specified
    ///     $runspace = New-PSSession
    ///     Remove-PSSession -remoterunspaceinfo $runspace
    /// Remove the runspace specified (no need for a parameter name)
    ///     Remove-PSSession $runspace.
    [Cmdlet(VerbsCommon.Remove, "PSSession", SupportsShouldProcess = true,
            DefaultParameterSetName = RemovePSSessionCommand.IdParameterSet,
            HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096963", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class RemovePSSessionCommand : PSRunspaceCmdlet
        /// Specifies the PSSession objects which need to be
                   ParameterSetName = RemovePSSessionCommand.SessionParameterSet)]
        /// Do the following actions:
        ///     1. If runspace is in opened state,
        ///             a. stop any execution in process in the runspace
        ///             b. close the runspace
        ///     2. Remove the runspace from the global cache.
            ICollection<PSSession> toRemove = null;
                case RemovePSSessionCommand.ComputerNameParameterSet:
                case RemovePSSessionCommand.NameParameterSet:
                case RemovePSSessionCommand.InstanceIdParameterSet:
                case RemovePSSessionCommand.IdParameterSet:
                case RemovePSSessionCommand.ContainerIdParameterSet:
                case RemovePSSessionCommand.VMIdParameterSet:
                case RemovePSSessionCommand.VMNameParameterSet:
                        Dictionary<Guid, PSSession> matches = GetMatchingRunspaces(false, true);
                        toRemove = matches.Values;
                case RemovePSSessionCommand.SessionParameterSet:
                        toRemove = Session;
                    Diagnostics.Assert(false, "Invalid Parameter Set");
                    toRemove = new Collection<PSSession>(); // initialize toRemove to turn off PREfast warning about it being null
            foreach (PSSession remoteRunspaceInfo in toRemove)
                if (ShouldProcess(remoteRunspace.ConnectionInfo.ComputerName, "Remove"))
                    // If the remote runspace is in a disconnected state, first try to connect it so that
                    // it can be removed from both the client and server.
                    if (remoteRunspaceInfo.Runspace.RunspaceStateInfo.State == RunspaceState.Disconnected)
                        bool ConnectSucceeded;
                            remoteRunspaceInfo.Runspace.Connect();
                            ConnectSucceeded = true;
                            ConnectSucceeded = false;
                        if (!ConnectSucceeded)
                            // Write error notification letting user know that session cannot be removed
                            // from server due to lack of connection.
                            string msg = System.Management.Automation.Internal.StringUtil.Format(
                                RemotingErrorIdStrings.RemoveRunspaceNotConnected, remoteRunspace.PSSessionName);
                            ErrorRecord errorRecord = new ErrorRecord(reason, "RemoveSessionCannotConnectToServer",
                                ErrorCategory.InvalidOperation, remoteRunspace);
                            // Continue removing the runspace from the client.
                        // Dispose internally calls Close() and Close()
                        // is a no-op if the state is not Opened, so just
                        // dispose the runspace
                        // just ignore, there is some transport error
                        // on Close()
                        // Remove the runspace from the repository
                        this.RunspaceRepository.Remove(remoteRunspaceInfo);
                        // just ignore, the runspace may already have
                        // been removed
