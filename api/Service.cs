#if !UNIX // Not built on Unix
using System.Runtime.InteropServices; // Marshal, DllImport
using System.ServiceProcess;
using DWORD = System.UInt32;
using NakedWin32Handle = System.IntPtr;
    #region ServiceBaseCommand
    /// This class implements the base for service commands.
    public abstract class ServiceBaseCommand : Cmdlet
        /// Confirm that the operation should proceed.
        /// <param name="service">Service object to be acted on.</param>
        /// <returns>True if operation should continue, false otherwise.</returns>
        protected bool ShouldProcessServiceOperation(ServiceController service)
            return ShouldProcessServiceOperation(
                service.DisplayName,
                service.ServiceName);
        /// <param name="displayName">Display name of service to be acted on.</param>
        /// <param name="serviceName">Service name of service to be acted on.</param>
        protected bool ShouldProcessServiceOperation(
            string displayName, string serviceName)
            string name = StringUtil.Format(ServiceResources.ServiceNameForConfirmation,
                displayName,
                serviceName);
            return ShouldProcess(name);
        /// <param name="service"></param>
        /// <param name="errorMessage"></param>
            ServiceController service,
            string errorMessage,
                service.ServiceName,
                service,
        /// <param name="serviceName"></param>
        /// <param name="displayName"></param>
            string serviceName,
            string displayName,
            string message = StringUtil.Format(errorMessage,
                serviceName,
                (innerException == null) ? category.ToString() : innerException.Message);
            var exception = new ServiceCommandException(message, innerException);
            exception.ServiceName = serviceName;
            WriteError(new ErrorRecord(exception, errorId, category, targetObject));
        internal void SetServiceSecurityDescriptor(
            string securityDescriptorSddl,
            NakedWin32Handle hService)
            var rawSecurityDescriptor = new RawSecurityDescriptor(securityDescriptorSddl);
            RawAcl rawDiscretionaryAcl = rawSecurityDescriptor.DiscretionaryAcl;
            var discretionaryAcl = new DiscretionaryAcl(false, false, rawDiscretionaryAcl);
            byte[] rawDacl = new byte[discretionaryAcl.BinaryLength];
            discretionaryAcl.GetBinaryForm(rawDacl, 0);
            rawSecurityDescriptor.DiscretionaryAcl = new RawAcl(rawDacl, 0);
            byte[] securityDescriptorByte = new byte[rawSecurityDescriptor.BinaryLength];
            rawSecurityDescriptor.GetBinaryForm(securityDescriptorByte, 0);
            bool status = NativeMethods.SetServiceObjectSecurity(
                hService,
                SecurityInfos.DiscretionaryAcl,
                securityDescriptorByte);
            if (!status)
                int lastError = Marshal.GetLastWin32Error();
                Win32Exception exception = new(lastError);
                bool accessDenied = exception.NativeErrorCode == NativeMethods.ERROR_ACCESS_DENIED;
                    nameof(ServiceResources.CouldNotSetServiceSecurityDescriptorSddl),
                    StringUtil.Format(ServiceResources.CouldNotSetServiceSecurityDescriptorSddl, service.ServiceName, exception.Message),
                    accessDenied ? ErrorCategory.PermissionDenied : ErrorCategory.InvalidOperation);
    #endregion ServiceBaseCommand
    #region MultipleServiceCommandBase
    /// This class implements the base for service commands which can
    /// operate on multiple services.
    public abstract class MultipleServiceCommandBase : ServiceBaseCommand
        internal enum SelectionMode
            /// Select all services.
            Default = 0,
            /// Select services matching the supplied names.
            DisplayName = 1,
            /// Select services based on pipeline input.
            InputObject = 2,
            /// Select services by Service name.
            ServiceName = 3
        /// Holds the selection mode setting.
        internal SelectionMode selectionMode;
        /// The ServiceName parameter is declared in subclasses,
        /// since it is optional for GetService and mandatory otherwise.
        internal string[] serviceNames = null;
        /// Gets/sets an array of display names for services.
        [Parameter(ParameterSetName = "DisplayName", Mandatory = true)]
        public string[] DisplayName
                return displayNames;
                displayNames = value;
                selectionMode = SelectionMode.DisplayName;
        internal string[] displayNames = null;
        /// Lets you include particular services.  Services not matching
        /// one of these (if specified) are excluded.
        /// These are interpreted as either ServiceNames or DisplayNames
        /// according to the parameter set.
        public string[] Include
                return include;
                include = value;
        internal string[] include = null;
        /// Lets you exclude particular services.  Services matching
        public string[] Exclude
                return exclude;
                exclude = value;
        internal string[] exclude = null;
        // 1054295-2004/12/01-JonN This also works around 1054295.
        /// ServiceController objects, we bypass the ServiceName and
        /// DisplayName parameters and read the ServiceControllers
        /// directly.  This allows us to deal with services which
        /// have wildcard characters in their name (servicename or
        /// displayname).
        /// <value>ServiceController objects</value>
        [Parameter(ParameterSetName = "InputObject", ValueFromPipeline = true)]
        public ServiceController[] InputObject
                return _inputObject;
                _inputObject = value;
                selectionMode = SelectionMode.InputObject;
        private ServiceController[] _inputObject = null;
        /// Gets an array of all services.
        /// An array of <see cref="ServiceController"/> components that represents all the service resources.
        internal ServiceController[] AllServices => _allServices ??= ServiceController.GetServices();
        private ServiceController[] _allServices;
        internal ServiceController GetOneService(string nameOfService)
            Dbg.Assert(!WildcardPattern.ContainsWildcardCharacters(nameOfService), "Caller should verify that nameOfService doesn't contain wildcard characters");
                var sc = new ServiceController(nameOfService);
                // This will throw if the service doesn't exist
                var unused = sc.Status;
                // No exception, then this is an existing, valid service. Return it.
            catch (ArgumentException) { }
        /// Retrieve the list of all services matching the ServiceName,
        /// DisplayName, Include and Exclude parameters, sorted by ServiceName.
        internal List<ServiceController> MatchingServices()
            List<ServiceController> matchingServices;
            switch (selectionMode)
                case SelectionMode.DisplayName:
                    matchingServices = MatchingServicesByDisplayName();
                case SelectionMode.InputObject:
                    matchingServices = MatchingServicesByInput();
                    matchingServices = MatchingServicesByServiceName();
            // 2004/12/16 Note that the services will be sorted
            //  before being stopped.  JimTru confirms that this is OK.
            matchingServices.Sort(ServiceComparison);
            return matchingServices;
        // sort by servicename
        private static int ServiceComparison(ServiceController x, ServiceController y)
            return string.Compare(x.ServiceName, y.ServiceName, StringComparison.OrdinalIgnoreCase);
        /// Retrieves the list of all services matching the ServiceName,
        /// Include and Exclude parameters.
        /// service name which is not found even though it contains
        /// We do not use the ServiceController(string serviceName)
        /// constructor variant, since the resultant
        /// ServiceController.ServiceName is the provided serviceName
        /// even when that differs from the real ServiceName by case.
        private List<ServiceController> MatchingServicesByServiceName()
            List<ServiceController> matchingServices = new();
            if (serviceNames == null)
                foreach (ServiceController service in AllServices)
                    IncludeExcludeAdd(matchingServices, service, false);
            foreach (string pattern in serviceNames)
                if (WildcardPattern.ContainsWildcardCharacters(pattern))
                        if (!wildcard.IsMatch(service.ServiceName))
                        IncludeExcludeAdd(matchingServices, service, true);
                    ServiceController service = GetOneService(pattern);
                    if (service != null)
                        pattern,
                        "NoServiceFoundForGivenName",
                        ServiceResources.NoServiceFoundForGivenName,
        /// Retrieves the list of all services matching the DisplayName,
        /// display name which is not found even though it contains
        private List<ServiceController> MatchingServicesByDisplayName()
            if (DisplayName == null)
                Diagnostics.Assert(false, "null DisplayName");
            foreach (string pattern in DisplayName)
                    if (!wildcard.IsMatch(service.DisplayName))
                        "NoServiceFoundForGivenDisplayName",
                        ServiceResources.NoServiceFoundForGivenDisplayName,
        /// Retrieves the list of all services matching the InputObject,
        private List<ServiceController> MatchingServicesByInput()
            foreach (ServiceController service in InputObject)
                service.Refresh();
        /// Add <paramref name="service"/> to <paramref name="list"/>,
        /// but only if it passes the Include and Exclude filters (if present)
        /// and (if <paramref name="checkDuplicates"/>) if it is not
        /// already on  <paramref name="list"/>.
        /// <param name="list">List of services.</param>
        /// <param name="service">Service to add to list.</param>
        /// <param name="checkDuplicates">Check list for duplicates.</param>
        private void IncludeExcludeAdd(
            List<ServiceController> list,
            bool checkDuplicates)
            if (include != null && !Matches(service, include))
            if (exclude != null && Matches(service, exclude))
            if (checkDuplicates)
                foreach (ServiceController sc in list)
                    if (sc.ServiceName == service.ServiceName &&
                        sc.MachineName == service.MachineName)
            list.Add(service);
        /// Check whether <paramref name="service"/> matches the list of
        /// patterns in <paramref name="matchList"/>.
        /// <param name="matchList"></param>
        private bool Matches(ServiceController service, string[] matchList)
            if (matchList == null)
                throw PSTraceSource.NewArgumentNullException(nameof(matchList));
            string serviceID = (selectionMode == SelectionMode.DisplayName)
                                ? service.DisplayName
                                : service.ServiceName;
            foreach (string pattern in matchList)
                if (wildcard.IsMatch(serviceID))
    #endregion MultipleServiceCommandBase
    #region GetServiceCommand
    /// This class implements the get-service command.
    [Cmdlet(VerbsCommon.Get, "Service", DefaultParameterSetName = "Default",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096496", RemotingCapability = RemotingCapability.SupportedByCommand)]
    [OutputType(typeof(ServiceController))]
    public sealed class GetServiceCommand : MultipleServiceCommandBase
        /// Gets/sets an array of service names.
        [Parameter(Position = 0, ParameterSetName = "Default", ValueFromPipelineByPropertyName = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty()]
        [Alias("ServiceName")]
                return serviceNames;
                serviceNames = value;
                selectionMode = SelectionMode.ServiceName;
        /// This returns the DependentServices of the specified service.
        [Alias("DS")]
        public SwitchParameter DependentServices { get; set; }
        /// This returns the ServicesDependedOn of the specified service.
        [Alias("SDO", "ServicesDependedOn")]
        public SwitchParameter RequiredServices { get; set; }
        /// Write the service objects.
            nint scManagerHandle = nint.Zero;
            if (!DependentServices && !RequiredServices)
                // As Get-Service only works on local services we get this once
                // to retrieve extra properties added by PowerShell.
                scManagerHandle = NativeMethods.OpenSCManagerW(
                    lpMachineName: null,
                    lpDatabaseName: null,
                    dwDesiredAccess: NativeMethods.SC_MANAGER_CONNECT);
                if (scManagerHandle == nint.Zero)
                    Win32Exception exception = new();
                    string message = StringUtil.Format(ServiceResources.FailToOpenServiceControlManager, exception.Message);
                    ServiceCommandException serviceException = new ServiceCommandException(message, exception);
                    ErrorRecord err = new ErrorRecord(
                        serviceException,
                        "FailToOpenServiceControlManager",
                        ErrorCategory.PermissionDenied,
                    ThrowTerminatingError(err);
                foreach (ServiceController service in MatchingServices())
                    if (!DependentServices.IsPresent && !RequiredServices.IsPresent)
                        WriteObject(AddProperties(scManagerHandle, service));
                        if (DependentServices.IsPresent)
                            foreach (ServiceController dependantserv in service.DependentServices)
                                WriteObject(dependantserv);
                        if (RequiredServices.IsPresent)
                            foreach (ServiceController servicedependedon in service.ServicesDependedOn)
                                WriteObject(servicedependedon);
                if (scManagerHandle != nint.Zero)
                    bool succeeded = NativeMethods.CloseServiceHandle(scManagerHandle);
                    Diagnostics.Assert(succeeded, "SCManager handle close failed");
        /// Adds UserName, Description, BinaryPathName, DelayedAutoStart and StartupType to a ServiceController object.
        /// <param name="scManagerHandle">Handle to the local SCManager instance.</param>
        /// <returns>ServiceController as PSObject with UserName, Description and StartupType added.</returns>
        private static PSObject AddProperties(nint scManagerHandle, ServiceController service)
            NakedWin32Handle hService = nint.Zero;
            // As these are optional values, a failure due to permissions or
            // other problem is ignored and the properties are set to null.
            bool? isDelayedAutoStart = null;
            string? binPath = null;
            string? description = null;
            string? startName = null;
            ServiceStartupType startupType = ServiceStartupType.InvalidValue;
                // We don't use service.ServiceHandle as that requests
                // SERVICE_ALL_ACCESS when we only need SERVICE_QUERY_CONFIG.
                hService = NativeMethods.OpenServiceW(
                    scManagerHandle,
                    NativeMethods.SERVICE_QUERY_CONFIG
                if (hService != nint.Zero)
                    if (NativeMethods.QueryServiceConfig2(
                        NativeMethods.SERVICE_CONFIG_DESCRIPTION,
                        out NativeMethods.SERVICE_DESCRIPTIONW descriptionInfo))
                        description = descriptionInfo.lpDescription;
                        NativeMethods.SERVICE_CONFIG_DELAYED_AUTO_START_INFO,
                        out NativeMethods.SERVICE_DELAYED_AUTO_START_INFO autostartInfo))
                        isDelayedAutoStart = autostartInfo.fDelayedAutostart;
                    if (NativeMethods.QueryServiceConfig(
                        out NativeMethods.QUERY_SERVICE_CONFIG serviceInfo))
                        binPath = serviceInfo.lpBinaryPathName;
                        startName = serviceInfo.lpServiceStartName;
                        if (isDelayedAutoStart.HasValue)
                            startupType = NativeMethods.GetServiceStartupType(
                                (ServiceStartMode)serviceInfo.dwStartType,
                                isDelayedAutoStart.Value);
                if (hService != IntPtr.Zero)
                    bool succeeded = NativeMethods.CloseServiceHandle(hService);
                    Diagnostics.Assert(succeeded, "Failed to close service handle");
            PSObject serviceAsPSObj = PSObject.AsPSObject(service);
            PSNoteProperty noteProperty = new("UserName", startName);
            serviceAsPSObj.Properties.Add(noteProperty, true);
            serviceAsPSObj.TypeNames.Insert(0, "System.Service.ServiceController#UserName");
            noteProperty = new PSNoteProperty("Description", description);
            serviceAsPSObj.TypeNames.Insert(0, "System.Service.ServiceController#Description");
            noteProperty = new PSNoteProperty("DelayedAutoStart", isDelayedAutoStart);
            serviceAsPSObj.TypeNames.Insert(0, "System.Service.ServiceController#DelayedAutoStart");
            noteProperty = new PSNoteProperty("BinaryPathName", binPath);
            serviceAsPSObj.TypeNames.Insert(0, "System.Service.ServiceController#BinaryPathName");
            noteProperty = new PSNoteProperty("StartupType", startupType);
            serviceAsPSObj.TypeNames.Insert(0, "System.Service.ServiceController#StartupType");
            return serviceAsPSObj;
#nullable disable
    #endregion GetServiceCommand
    #region ServiceOperationBaseCommand
    /// This class implements the base for service commands which actually
    /// act on the service(s).
    public abstract class ServiceOperationBaseCommand : MultipleServiceCommandBase
        [Parameter(Position = 0, ParameterSetName = "Default", Mandatory = true, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true)]
        /// Service controller objects.
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "InputObject", ValueFromPipeline = true)]
        public new ServiceController[] InputObject
        /// Specifies whether to write the objects successfully operated upon
        /// to the success stream.
        /// Waits forever for the service to reach the desired status, but
        /// writes a string to WriteWarning every 2 seconds.
        /// <param name="serviceController">Service on which to operate.</param>
        /// <param name="targetStatus">Desired status.</param>
        /// <param name="pendingStatus">
        /// This is the expected status while the operation is incomplete.
        /// If the service is in some other state, this means that the
        /// operation failed.
        /// <param name="resourceIdPending">
        /// resourceId for a string to be written to verbose stream
        /// every 2 seconds
        /// <param name="errorId">
        /// errorId for a nonterminating error if operation fails
        ///  <param name="errorMessage">
        /// errorMessage for a nonterminating error if operation fails
        /// <returns>True if action succeeded.</returns>
        /// <exception cref="PipelineStoppedException">
        /// WriteWarning will throw this if the pipeline has been stopped.
        /// This means that the delay between hitting CTRL-C and stopping
        /// the cmdlet should be 2 seconds at most.
        internal bool DoWaitForStatus(
            ServiceController serviceController,
            ServiceControllerStatus targetStatus,
            ServiceControllerStatus pendingStatus,
            string resourceIdPending,
            string errorMessage)
                    // ServiceController.Start will return before the service is actually started
                    // This API will wait forever
                    serviceController.WaitForStatus(
                        targetStatus,
                        new TimeSpan(20000000) // 2 seconds
                    return true; // service reached target status
                catch (System.ServiceProcess.TimeoutException) // still waiting
                    if (serviceController.Status != pendingStatus
                        // NTRAID#Windows Out Of Band Releases-919945-2005/09/27-JonN
                        // Close window where service could complete at
                        // just the wrong time
                        && serviceController.Status != targetStatus)
                        WriteNonTerminatingError(serviceController, null,
                                                 ErrorCategory.OpenError);
                    string message = StringUtil.Format(resourceIdPending,
                        serviceController.ServiceName,
                        serviceController.DisplayName
                    // will throw PipelineStoppedException if user hit CTRL-C
                    WriteWarning(message);
        /// This will start the service.
        /// <param name="serviceController">Service to start.</param>
        /// <returns>True if-and-only-if the service was started.</returns>
        internal bool DoStartService(ServiceController serviceController)
                serviceController.Start();
                if (e.NativeErrorCode != NativeMethods.ERROR_SERVICE_ALREADY_RUNNING)
                if (e.InnerException is not Win32Exception eInner
                    || eInner.NativeErrorCode != NativeMethods.ERROR_SERVICE_ALREADY_RUNNING)
                // This service refused to accept the start command,
                WriteNonTerminatingError(serviceController,
                    "CouldNotStartService",
                    ServiceResources.CouldNotStartService,
            // ServiceController.Start will return
            // before the service is actually started.
            if (!DoWaitForStatus(
                serviceController,
                ServiceControllerStatus.Running,
                ServiceControllerStatus.StartPending,
                ServiceResources.StartingService,
                "StartServiceFailed",
                ServiceResources.StartServiceFailed))
        /// This will stop the service.
        /// <param name="serviceController">Service to stop.</param>
        /// <param name="force">Stop dependent services.</param>
        /// <param name="waitForServiceToStop"></param>
        /// <returns>True if-and-only-if the service was stopped.</returns>
        internal List<ServiceController> DoStopService(ServiceController serviceController, bool force, bool waitForServiceToStop)
            // Ignore ServiceController.CanStop.  CanStop will be set false
            // if the service is not running, but this is not an error.
            List<ServiceController> stoppedServices = new();
            ServiceController[] dependentServices = null;
                dependentServices = serviceController.DependentServices;
                WriteNonTerminatingError(serviceController, e,
                    "CouldNotAccessDependentServices",
                    ServiceResources.CouldNotAccessDependentServices,
                    ErrorCategory.InvalidOperation);
            if (!force)
                if ((dependentServices != null)
                    && (dependentServices.Length > 0))
                    // Check if all dependent services are stopped
                    if (!HaveAllDependentServicesStopped(dependentServices))
                        // This service has dependent services
                        //  and the force flag is not specified.
                        //  Add a non-critical error for it.
                        "ServiceHasDependentServices",
                        ServiceResources.ServiceHasDependentServices,
                        return stoppedServices;
            if (dependentServices != null)
                foreach (ServiceController service in dependentServices)
                    if ((service.Status == ServiceControllerStatus.Running ||
                        service.Status == ServiceControllerStatus.StartPending) &&
                        service.CanStop)
                        stoppedServices.Add(service);
                serviceController.Stop();
                if (e.NativeErrorCode != NativeMethods.ERROR_SERVICE_NOT_ACTIVE)
                    || eInner.NativeErrorCode != NativeMethods.ERROR_SERVICE_NOT_ACTIVE)
                // This service refused to accept the stop command,
                    "CouldNotStopService",
                    ServiceResources.CouldNotStopService,
                    ErrorCategory.CloseError);
                RemoveNotStoppedServices(stoppedServices);
            // ServiceController.Stop will return
            //  before the service is actually stopped.
            if (waitForServiceToStop)
                    ServiceControllerStatus.Stopped,
                    ServiceControllerStatus.StopPending,
                    ServiceResources.StoppingService,
                    "StopServiceFailed",
                    ServiceResources.StopServiceFailed))
            if ((serviceController.Status.Equals(ServiceControllerStatus.Stopped)) || (serviceController.Status.Equals(ServiceControllerStatus.StopPending)))
                stoppedServices.Add(serviceController);
        /// Check if all dependent services are stopped.
        /// <param name="dependentServices"></param>
        /// True if all dependent services are stopped
        /// False if not all dependent services are stopped
        private static bool HaveAllDependentServicesStopped(ServiceController[] dependentServices)
            return Array.TrueForAll(dependentServices, static service => service.Status == ServiceControllerStatus.Stopped);
        /// This removes all services that are not stopped from a list of services.
        /// <param name="services">A list of services.</param>
        internal void RemoveNotStoppedServices(List<ServiceController> services)
            // You shall not modify a collection during enumeration.
            services.RemoveAll(service =>
                service.Status != ServiceControllerStatus.Stopped &&
                service.Status != ServiceControllerStatus.StopPending);
        /// This will pause the service.
        /// <param name="serviceController">Service to pause.</param>
        /// <returns>True if-and-only-if the service was paused.</returns>
        internal bool DoPauseService(ServiceController serviceController)
            bool serviceNotRunning = false;
                serviceController.Pause();
                if (e.NativeErrorCode == NativeMethods.ERROR_SERVICE_NOT_ACTIVE)
                    serviceNotRunning = true;
                if (e.InnerException is Win32Exception eInner
                    && eInner.NativeErrorCode == NativeMethods.ERROR_SERVICE_NOT_ACTIVE)
                // This service refused to accept the pause command,
                string resourceIdAndErrorId = ServiceResources.CouldNotSuspendService;
                if (serviceNotRunning)
                        "CouldNotSuspendServiceNotRunning",
                        ServiceResources.CouldNotSuspendServiceNotRunning,
                else if (!serviceController.CanPauseAndContinue)
                        "CouldNotSuspendServiceNotSupported",
                        ServiceResources.CouldNotSuspendServiceNotSupported,
                    "CouldNotSuspendService",
                    ServiceResources.CouldNotSuspendService,
            // ServiceController.Pause will return
            // before the service is actually paused.
                ServiceControllerStatus.Paused,
                ServiceControllerStatus.PausePending,
                ServiceResources.SuspendingService,
                "SuspendServiceFailed",
                ServiceResources.SuspendServiceFailed))
        /// This will resume the service.
        /// <param name="serviceController">Service to resume.</param>
        /// <returns>True if-and-only-if the service was resumed.</returns>
        internal bool DoResumeService(ServiceController serviceController)
                serviceController.Continue();
                // This service refused to accept the continue command,
                        "CouldNotResumeServiceNotRunning",
                        ServiceResources.CouldNotResumeServiceNotRunning,
                        "CouldNotResumeServiceNotSupported",
                        ServiceResources.CouldNotResumeServiceNotSupported,
                    "CouldNotResumeService",
                    ServiceResources.CouldNotResumeService,
            // ServiceController.Continue will return
            // before the service is actually continued.
                ServiceControllerStatus.ContinuePending,
                ServiceResources.ResumingService,
                "ResumeServiceFailed",
                ServiceResources.ResumeServiceFailed))
    #endregion ServiceOperationBaseCommand
    #region StopServiceCommand
    /// This class implements the stop-service command.
    /// Note that the services will be sorted before being stopped.
    /// PM confirms that this is OK.
    [Cmdlet(VerbsLifecycle.Stop, "Service", DefaultParameterSetName = "InputObject", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097052")]
    public sealed class StopServiceCommand : ServiceOperationBaseCommand
        /// Specifies whether to force a service to stop
        /// Specifies whether to wait for a service to reach the stopped state before returning.
        public SwitchParameter NoWait { get; set; }
        /// Stop the services.
        /// It is a non-terminating error if -Force is not specified and
        ///  the service has dependent services, whether or not they
        ///  are running.
        /// It is a non-terminating error if the service stop operation fails.
            foreach (ServiceController serviceController in MatchingServices())
                if (!ShouldProcessServiceOperation(serviceController))
                List<ServiceController> stoppedServices = DoStopService(serviceController, Force, !NoWait);
                if (PassThru && stoppedServices.Count > 0)
                    foreach (ServiceController service in stoppedServices)
                        WriteObject(service);
    #endregion StopServiceCommand
    #region StartServiceCommand
    /// This class implements the start-service command.
    [Cmdlet(VerbsLifecycle.Start, "Service", DefaultParameterSetName = "InputObject", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097051")]
    public sealed class StartServiceCommand : ServiceOperationBaseCommand
        /// Start the services.
                if (DoStartService(serviceController))
                        WriteObject(serviceController);
    #endregion StartServiceCommand
    #region SuspendServiceCommand
    /// This class implements the suspend-service command.
    [Cmdlet(VerbsLifecycle.Suspend, "Service", DefaultParameterSetName = "InputObject", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097053")]
    public sealed class SuspendServiceCommand : ServiceOperationBaseCommand
                if (DoPauseService(serviceController))
    #endregion SuspendServiceCommand
    #region ResumeServiceCommand
    /// This class implements the resume-service command.
    [Cmdlet(VerbsLifecycle.Resume, "Service", DefaultParameterSetName = "InputObject", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097150")]
    public sealed class ResumeServiceCommand : ServiceOperationBaseCommand
                if (DoResumeService(serviceController))
    #endregion ResumeServiceCommand
    #region RestartServiceCommand
    /// This class implements the restart-service command.
    [Cmdlet(VerbsLifecycle.Restart, "Service", DefaultParameterSetName = "InputObject", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097059")]
    public sealed class RestartServiceCommand : ServiceOperationBaseCommand
        /// Stop and restart the services.
        /// It is a non-terminating error if the service is running,
        ///  -Force is not specified and the service has dependent services,
        ///  whether or not the dependent services are running.
                // Set the NoWait parameter to false since we are not adding this switch to this cmdlet.
                List<ServiceController> stoppedServices = DoStopService(serviceController, Force, true);
                if (stoppedServices.Count > 0)
                        if (DoStartService(service))
    #endregion RestartServiceCommand
    #region SetServiceCommand
    /// This class implements the set-service command.
    [Cmdlet(VerbsCommon.Set, "Service", SupportsShouldProcess = true, DefaultParameterSetName = "Name",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097148", RemotingCapability = RemotingCapability.SupportedByCommand)]
    public class SetServiceCommand : ServiceOperationBaseCommand
        /// Service name.
        [Parameter(Mandatory = true, ParameterSetName = "Name", Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Alias("ServiceName", "SN")]
        public new string Name
                return serviceName;
                serviceName = value;
        internal string serviceName = null;
        /// Specifies a ServiceController object that represents the service to change.
        /// Enter a variable that contains the objects or type a command or expression
        /// that gets the objects.
        [Parameter(Mandatory = true, ParameterSetName = "InputObject", Position = 0, ValueFromPipeline = true)]
        public new ServiceController InputObject { get; set; }
        /// The following is the definition of the input parameter "DisplayName".
        /// Specifies a new display name for the cmdlet.
        [Alias("DN")]
        public new string DisplayName
                return displayName;
                displayName = value;
        internal string displayName = null;
        /// Account under which the service should run.
        [Credential()]
        /// The following is the definition of the input parameter "Description".
        /// Specifies a new description for the service.
        /// The service description appears in Services in Computer Management.
        /// Description is not a property of the ServiceController object that
        /// Get-Service retrieve.
                return description;
                description = value;
        internal string description = null;
        /// The following is the definition of the input parameter "StartupType".
        /// "Set-Service -StartType" sets ServiceController.InputObject.StartType.
        /// Changes the starting mode of the service. Valid values for StartupType are:
        /// -- Automatic: Start when the system starts.
        /// -- Manual   : Starts only when started by a user or program.
        /// -- Disabled : Can.
        [Alias("StartMode", "SM", "ST", "StartType")]
        public ServiceStartupType StartupType
                return startupType;
                startupType = value;
        // We set the initial value to an invalid value so that we can
        // distinguish when this is and is not set.
        internal ServiceStartupType startupType = ServiceStartupType.InvalidValue;
        /// Sets the SecurityDescriptorSddl of the service using a SDDL string.
        [Alias("sd")]
        public string SecurityDescriptorSddl
        /// The following is the definition of the input parameter "Status".
        /// This specifies what state the service should be in (e.g. Running, Stopped,
        /// Paused).  If it is already in that state, do nothing.  If it is not, do the
        /// appropriate action to bring about the desired result (start/stop/suspend the
        /// service) and issue an error if this cannot be achieved.
        ///  Status can be Paused ,  Running and Stopped.
        [ValidateSetAttribute(new string[] { "Running", "Stopped", "Paused" })]
        public string Status
                return serviceStatus;
                serviceStatus = value;
        internal string serviceStatus = null;
        /// The following is the definition of the input parameter "Force".
        /// This parameter is useful only when parameter "Stop" is enabled.
        /// If "Force" is enabled, it will also stop the dependent services.
        /// If not, it will send an error when this service has dependent ones.
        /// This is not a parameter for this cmdlet.
        // This has been shadowed from base class and removed parameter tag to fix gcm "Set-Service" -syntax
        public new string[] Include
                include = null;
        internal new string[] include = null;
        public new string[] Exclude
                exclude = null;
        internal new string[] exclude = null;
            ServiceController service = null;
            bool objServiceShouldBeDisposed = false;
                if (InputObject != null)
                    service = InputObject;
                    Name = service.ServiceName;
                    objServiceShouldBeDisposed = false;
                    service = new ServiceController(serviceName);
                    objServiceShouldBeDisposed = true;
                Diagnostics.Assert(!string.IsNullOrEmpty(Name), "null ServiceName");
                // "new ServiceController" will succeed even if
                // there is no such service.  This checks whether
                // the service actually exists.
                string unusedByDesign = service.DisplayName;
                // cannot use WriteNonterminatingError as service is null
                ErrorRecord er = new(ex, "ArgumentException", ErrorCategory.ObjectNotFound, Name);
                ErrorRecord er = new(ex, "InvalidOperationException", ErrorCategory.ObjectNotFound, Name);
            try // In finally we ensure dispose, if object not pipelined.
                if (!ShouldProcessServiceOperation(service))
                NakedWin32Handle hScManager = IntPtr.Zero;
                NakedWin32Handle hService = IntPtr.Zero;
                IntPtr delayedAutoStartInfoBuffer = IntPtr.Zero;
                    hScManager = NativeMethods.OpenSCManagerW(
                        NativeMethods.SC_MANAGER_CONNECT
                    if (hScManager == IntPtr.Zero)
                            ServiceResources.FailToOpenServiceControlManager,
                            ErrorCategory.PermissionDenied);
                    var access = NativeMethods.SERVICE_CHANGE_CONFIG;
                    if (!string.IsNullOrEmpty(SecurityDescriptorSddl))
                        access |= NativeMethods.WRITE_DAC | NativeMethods.WRITE_OWNER;
                        hScManager,
                        access
                    if (hService == IntPtr.Zero)
                            "CouldNotSetService",
                            ServiceResources.CouldNotSetService,
                    // Modify startup type or display name or credential
                    if (!string.IsNullOrEmpty(DisplayName)
                        || StartupType != ServiceStartupType.InvalidValue || Credential != null)
                        DWORD dwStartType = NativeMethods.SERVICE_NO_CHANGE;
                        if (!NativeMethods.TryGetNativeStartupType(StartupType, out dwStartType))
                            WriteNonTerminatingError(StartupType.ToString(), "Set-Service", Name,
                                new ArgumentException(), "CouldNotSetService",
                                ServiceResources.UnsupportedStartupType,
                                ErrorCategory.InvalidArgument);
                        string username = null;
                        if (Credential != null)
                            username = Credential.UserName;
                            password = Marshal.SecureStringToCoTaskMemUnicode(Credential.Password);
                        bool succeeded = NativeMethods.ChangeServiceConfigW(
                            NativeMethods.SERVICE_NO_CHANGE,
                            dwStartType,
                            username,
                            password,
                            DisplayName
                        if (!succeeded)
                    NativeMethods.SERVICE_DESCRIPTIONW sd = new();
                    sd.lpDescription = Description;
                    int size = Marshal.SizeOf(sd);
                    IntPtr buffer = Marshal.AllocCoTaskMem(size);
                    Marshal.StructureToPtr(sd, buffer, false);
                    bool status = NativeMethods.ChangeServiceConfig2W(
                        buffer);
                            "CouldNotSetServiceDescription",
                            ServiceResources.CouldNotSetServiceDescription,
                    // Set the delayed auto start
                    NativeMethods.SERVICE_DELAYED_AUTO_START_INFO ds = new();
                    ds.fDelayedAutostart = StartupType == ServiceStartupType.AutomaticDelayedStart;
                    size = Marshal.SizeOf(ds);
                    delayedAutoStartInfoBuffer = Marshal.AllocCoTaskMem(size);
                    Marshal.StructureToPtr(ds, delayedAutoStartInfoBuffer, false);
                    status = NativeMethods.ChangeServiceConfig2W(
                        delayedAutoStartInfoBuffer);
                            DisplayName,
                            "CouldNotSetServiceDelayedAutoStart",
                            ServiceResources.CouldNotSetServiceDelayedAutoStart,
                    // Handle the '-Status' parameter
                    if (!string.IsNullOrEmpty(Status))
                        if (Status.Equals("Running", StringComparison.OrdinalIgnoreCase))
                            if (!service.Status.Equals(ServiceControllerStatus.Running))
                                if (service.Status.Equals(ServiceControllerStatus.Paused))
                                    // resume service
                                    DoResumeService(service);
                                    // start service
                                    DoStartService(service);
                        else if (Status.Equals("Stopped", StringComparison.OrdinalIgnoreCase))
                            if (!service.Status.Equals(ServiceControllerStatus.Stopped))
                                // Check for the dependent services as set-service dont have force parameter
                                ServiceController[] dependentServices = service.DependentServices;
                                if ((!Force) && (dependentServices != null) && (dependentServices.Length > 0))
                                    WriteNonTerminatingError(service, null, "ServiceHasDependentServicesNoForce", ServiceResources.ServiceHasDependentServicesNoForce, ErrorCategory.InvalidOperation);
                                // Stop service, pass 'true' to the force parameter as we have already checked for the dependent services.
                                DoStopService(service, Force, waitForServiceToStop: true);
                        else if (Status.Equals("Paused", StringComparison.OrdinalIgnoreCase))
                            if (!service.Status.Equals(ServiceControllerStatus.Paused))
                                DoPauseService(service);
                        SetServiceSecurityDescriptor(service, SecurityDescriptorSddl, hService);
                        // To display the service, refreshing the service would not show the display name after updating
                        ServiceController displayservice = new(Name);
                        WriteObject(displayservice);
                    if (delayedAutoStartInfoBuffer != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(delayedAutoStartInfoBuffer);
                    if (hScManager != IntPtr.Zero)
                        bool succeeded = NativeMethods.CloseServiceHandle(hScManager);
                if (objServiceShouldBeDisposed)
                    service.Dispose();
    #endregion SetServiceCommand
    #region NewServiceCommand
    /// This class implements the New-Service command.
    [Cmdlet(VerbsCommon.New, "Service", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096905")]
    public class NewServiceCommand : ServiceBaseCommand
        /// Name of the service to create.
            get { return serviceName; }
            set { serviceName = value; }
        /// The executable which implements this service.
        [Alias("Path")]
        public string BinaryPathName
            get { return binaryPathName; }
            set { binaryPathName = value; }
        internal string binaryPathName = null;
        /// DisplayName of the service to create.
            get { return displayName; }
            set { displayName = value; }
        /// Description of the service to create.
            get { return description; }
            set { description = value; }
        /// Should the service start automatically?
            get { return startupType; }
            set { startupType = value; }
        internal ServiceStartupType startupType = ServiceStartupType.Automatic;
            get { return credential; }
            set { credential = value; }
        internal PSCredential credential = null;
        /// Other services on which the new service depends.
        public string[] DependsOn
            get { return dependsOn; }
            set { dependsOn = value; }
        internal string[] dependsOn = null;
        /// Create the service.
            Diagnostics.Assert(!string.IsNullOrEmpty(Name),
                "null ServiceName");
            Diagnostics.Assert(!string.IsNullOrEmpty(BinaryPathName),
                "null BinaryPathName");
            if (!ShouldProcessServiceOperation(DisplayName ?? string.Empty, Name))
            // Connect to the service controller
                    NativeMethods.SC_MANAGER_CONNECT | NativeMethods.SC_MANAGER_CREATE_SERVICE
                        "CouldNotNewService",
                        ServiceResources.CouldNotNewService,
                if (!NativeMethods.TryGetNativeStartupType(StartupType, out DWORD dwStartType))
                    WriteNonTerminatingError(StartupType.ToString(), "New-Service", Name,
                        new ArgumentException(), "CouldNotNewService",
                // set up the double-null-terminated lpDependencies parameter
                IntPtr lpDependencies = IntPtr.Zero;
                if (DependsOn != null)
                    int numchars = 1; // final null
                    foreach (string dependedOn in DependsOn)
                        numchars += dependedOn.Length + 1;
                    char[] doubleNullArray = new char[numchars];
                    int pos = 0;
                        Array.Copy(
                            dependedOn.ToCharArray(), 0,
                            doubleNullArray, pos,
                            dependedOn.Length
                        pos += dependedOn.Length;
                        doubleNullArray[pos++] = (char)0; // null terminator
                    doubleNullArray[pos++] = (char)0; // double-null terminator
                    Diagnostics.Assert(pos == numchars, "lpDependencies build error");
                    lpDependencies = Marshal.AllocHGlobal(
                        numchars * Marshal.SystemDefaultCharSize);
                    Marshal.Copy(doubleNullArray, 0, lpDependencies, numchars);
                // set up the Credential parameter
                // Create the service
                hService = NativeMethods.CreateServiceW(
                    NativeMethods.SERVICE_CHANGE_CONFIG | NativeMethods.WRITE_DAC | NativeMethods.WRITE_OWNER,
                    NativeMethods.SERVICE_WIN32_OWN_PROCESS,
                    NativeMethods.SERVICE_ERROR_NORMAL,
                    BinaryPathName,
                    lpDependencies,
                    password
                // Set the service description
                bool succeeded = NativeMethods.ChangeServiceConfig2W(
                        "CouldNotNewServiceDescription",
                        ServiceResources.CouldNotNewServiceDescription,
                if (StartupType == ServiceStartupType.AutomaticDelayedStart)
                    ds.fDelayedAutostart = true;
                    succeeded = NativeMethods.ChangeServiceConfig2W(
                            "CouldNotNewServiceDelayedAutoStart",
                            ServiceResources.CouldNotNewServiceDelayedAutoStart,
                // write the ServiceController for the new service
                service = new ServiceController(Name);
    #endregion NewServiceCommand
    #region RemoveServiceCommand
    /// This class implements the Remove-Service command.
    [Cmdlet(VerbsCommon.Remove, "Service", SupportsShouldProcess = true, DefaultParameterSetName = "Name", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2248980")]
    public class RemoveServiceCommand : ServiceBaseCommand
        /// Name of the service to remove.
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Name")]
        /// Specifies ServiceController object representing the services to be removed.
        [Parameter(ValueFromPipeline = true, ParameterSetName = "InputObject")]
        public ServiceController InputObject { get; set; }
        /// Remove the service.
                // "new ServiceController" will succeed even if there is no such service.
                // This checks whether the service actually exists.
                // Cannot use WriteNonterminatingError as service is null
                // Confirm the operation first.
                // This is always false if WhatIf is set.
                        lpMachineName: string.Empty,
                        dwDesiredAccess: NativeMethods.SC_MANAGER_ALL_ACCESS
                        WriteObject(exception);
                        NativeMethods.SERVICE_DELETE
                            "CouldNotRemoveService",
                            ServiceResources.CouldNotRemoveService,
                    bool status = NativeMethods.DeleteService(hService);
                            Diagnostics.Assert(lastError != 0, "ErrorCode not success");
    #endregion RemoveServiceCommand
    #region ServiceCommandException
    /// Non-terminating errors occurring in the service noun commands.
    public class ServiceCommandException : SystemException
        public ServiceCommandException()
        public ServiceCommandException(string message)
        public ServiceCommandException(string message, Exception innerException)
        [Obsolete("Legacy serialization support is deprecated since .NET 8, hence this method is now marked as obsolete", DiagnosticId = "SYSLIB0051")]
        protected ServiceCommandException(SerializationInfo info, StreamingContext context)
        /// Name of the service which could not be found or operated upon.
        public string ServiceName
            get { return _serviceName; }
            set { _serviceName = value; }
        private string _serviceName = string.Empty;
    #endregion ServiceCommandException
    #region NativeMethods
    internal static class NativeMethods
        // from winuser.h
        internal const int ERROR_SERVICE_ALREADY_RUNNING = 1056;
        internal const int ERROR_SERVICE_NOT_ACTIVE = 1062;
        internal const int ERROR_INSUFFICIENT_BUFFER = 122;
        internal const DWORD ERROR_ACCESS_DENIED = 0x5;
        internal const DWORD SC_MANAGER_CONNECT = 1;
        internal const DWORD SC_MANAGER_CREATE_SERVICE = 2;
        internal const DWORD SC_MANAGER_ALL_ACCESS = 0xf003f;
        internal const DWORD SERVICE_QUERY_CONFIG = 1;
        internal const DWORD SERVICE_CHANGE_CONFIG = 2;
        internal const DWORD SERVICE_DELETE = 0x10000;
        internal const DWORD SERVICE_NO_CHANGE = 0xffffffff;
        internal const DWORD SERVICE_AUTO_START = 0x2;
        internal const DWORD SERVICE_DEMAND_START = 0x3;
        internal const DWORD SERVICE_DISABLED = 0x4;
        internal const DWORD SERVICE_CONFIG_DESCRIPTION = 1;
        internal const DWORD SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 3;
        internal const DWORD SERVICE_CONFIG_SERVICE_SID_INFO = 5;
        internal const DWORD WRITE_DAC = 262144;
        internal const DWORD WRITE_OWNER = 524288;
        internal const DWORD SERVICE_WIN32_OWN_PROCESS = 0x10;
        internal const DWORD SERVICE_ERROR_NORMAL = 1;
        // from winnt.h
        [DllImport(PinvokeDllNames.OpenSCManagerWDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern
        NakedWin32Handle OpenSCManagerW(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpMachineName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpDatabaseName,
            DWORD dwDesiredAccess
        [DllImport(PinvokeDllNames.OpenServiceWDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        NakedWin32Handle OpenServiceW(
            NakedWin32Handle hSCManager,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpServiceName,
        [DllImport(PinvokeDllNames.QueryServiceConfigDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        bool QueryServiceConfigW(
            IntPtr lpServiceConfig,
            DWORD cbBufSize,
            out DWORD pcbBytesNeeded
        [DllImport(PinvokeDllNames.QueryServiceConfig2DllName, CharSet = CharSet.Unicode, SetLastError = true)]
        bool QueryServiceConfig2W(
            NakedWin32Handle hService,
            DWORD dwInfoLevel,
            IntPtr lpBuffer,
        [DllImport(PinvokeDllNames.CloseServiceHandleDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        bool CloseServiceHandle(
            NakedWin32Handle hSCManagerOrService
        [DllImport(PinvokeDllNames.DeleteServiceDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        bool DeleteService(
            NakedWin32Handle hService
        [DllImport(PinvokeDllNames.ChangeServiceConfigWDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        bool ChangeServiceConfigW(
            DWORD dwServiceType,
            DWORD dwStartType,
            DWORD dwErrorControl,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpBinaryPathName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpLoadOrderGroup,
            IntPtr lpdwTagId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpDependencies,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpServiceStartName,
            [In] IntPtr lpPassword,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpDisplayName
        [DllImport(PinvokeDllNames.ChangeServiceConfig2WDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        bool ChangeServiceConfig2W(
            IntPtr lpInfo
        internal struct SERVICE_DESCRIPTIONW
            internal string lpDescription;
        internal struct QUERY_SERVICE_CONFIG
            internal uint dwServiceType;
            internal uint dwStartType;
            internal uint dwErrorControl;
            [MarshalAs(UnmanagedType.LPWStr)] internal string lpBinaryPathName;
            [MarshalAs(UnmanagedType.LPWStr)] internal string lpLoadOrderGroup;
            internal uint dwTagId;
            [MarshalAs(UnmanagedType.LPWStr)] internal string lpDependencies;
            [MarshalAs(UnmanagedType.LPWStr)] internal string lpServiceStartName;
            [MarshalAs(UnmanagedType.LPWStr)] internal string lpDisplayName;
        internal struct SERVICE_DELAYED_AUTO_START_INFO
            internal bool fDelayedAutostart;
        [DllImport(PinvokeDllNames.CreateServiceWDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        NakedWin32Handle CreateServiceW(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpDisplayName,
            DWORD dwDesiredAccess,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpdwTagId,
            [In] IntPtr lpDependencies,
            [In] IntPtr lpPassword
        [DllImport(PinvokeDllNames.SetServiceObjectSecurityDllName, CharSet = CharSet.Unicode, SetLastError = true)]
        bool SetServiceObjectSecurity(
            System.Security.AccessControl.SecurityInfos dwSecurityInformation,
            byte[] lpSecurityDescriptor
        internal static bool QueryServiceConfig(NakedWin32Handle hService, out NativeMethods.QUERY_SERVICE_CONFIG configStructure)
            IntPtr lpBuffer = IntPtr.Zero;
            configStructure = default(NativeMethods.QUERY_SERVICE_CONFIG);
            DWORD bufferSize, bufferSizeNeeded = 0;
            bool status = NativeMethods.QueryServiceConfigW(
                hSCManager: hService,
                lpServiceConfig: lpBuffer,
                cbBufSize: 0,
                pcbBytesNeeded: out bufferSizeNeeded);
            if (!status && Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
                return status;
                lpBuffer = Marshal.AllocCoTaskMem((int)bufferSizeNeeded);
                bufferSize = bufferSizeNeeded;
                status = NativeMethods.QueryServiceConfigW(
                    lpBuffer,
                    bufferSize,
                    out bufferSizeNeeded);
                configStructure = (NativeMethods.QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(lpBuffer, typeof(NativeMethods.QUERY_SERVICE_CONFIG));
                Marshal.FreeCoTaskMem(lpBuffer);
        internal static bool QueryServiceConfig2<T>(NakedWin32Handle hService, DWORD infolevel, out T configStructure)
            configStructure = default(T);
            bool status = NativeMethods.QueryServiceConfig2W(
                hService: hService,
                dwInfoLevel: infolevel,
                lpBuffer: lpBuffer,
                status = NativeMethods.QueryServiceConfig2W(
                    infolevel,
                configStructure = (T)Marshal.PtrToStructure(lpBuffer, typeof(T));
        /// Get appropriate win32 StartupType.
        /// <param name="StartupType">
        /// StartupType provided by the user.
        /// <param name="dwStartType">
        /// Out parameter of the native win32 StartupType
        /// If a supported StartupType is provided, funciton returns true, otherwise false.
        internal static bool TryGetNativeStartupType(ServiceStartupType StartupType, out DWORD dwStartType)
            bool success = true;
            dwStartType = NativeMethods.SERVICE_NO_CHANGE;
            switch (StartupType)
                case ServiceStartupType.Automatic:
                case ServiceStartupType.AutomaticDelayedStart:
                    dwStartType = NativeMethods.SERVICE_AUTO_START;
                case ServiceStartupType.Manual:
                    dwStartType = NativeMethods.SERVICE_DEMAND_START;
                case ServiceStartupType.Disabled:
                    dwStartType = NativeMethods.SERVICE_DISABLED;
                case ServiceStartupType.InvalidValue:
        internal static ServiceStartupType GetServiceStartupType(ServiceStartMode startMode, bool delayedAutoStart)
            ServiceStartupType result = ServiceStartupType.Disabled;
            switch (startMode)
                case ServiceStartMode.Automatic:
                    result = delayedAutoStart ? ServiceStartupType.AutomaticDelayedStart : ServiceStartupType.Automatic;
                case ServiceStartMode.Manual:
                    result = ServiceStartupType.Manual;
                case ServiceStartMode.Disabled:
                    result = ServiceStartupType.Disabled;
    #endregion NativeMethods
    #region ServiceStartupType
    /// Enum for usage with StartupType. Automatic, Manual and Disabled index matched from System.ServiceProcess.ServiceStartMode
    public enum ServiceStartupType
        /// <summary>Invalid service</summary>
        InvalidValue = -1,
        /// <summary>Automatic service</summary>
        Automatic = 2,
        /// <summary>Manual service</summary>
        Manual = 3,
        /// <summary>Disabled service</summary>
        Disabled = 4,
        /// <summary>Automatic (Delayed Start) service</summary>
        AutomaticDelayedStart = 10
    #endregion ServiceStartupType
#endif // Not built on Unix
