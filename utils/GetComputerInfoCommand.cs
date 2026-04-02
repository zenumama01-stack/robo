using System.Linq.Expressions;
    #region GetComputerInfoCommand cmdlet implementation
    /// The Get-ComputerInfo cmdlet gathers and reports information
    /// about a computer.
    [Cmdlet(VerbsCommon.Get, "ComputerInfo",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096810")]
    [Alias("gin")]
    [OutputType(typeof(ComputerInfo), typeof(PSObject))]
    public class GetComputerInfoCommand : PSCmdlet
        #region Inner Types
        private sealed class OSInfoGroup
            public WmiOperatingSystem os;
            public HotFix[] hotFixes;
            public WmiPageFileUsage[] pageFileUsage;
            public string halVersion;
            public TimeSpan? upTime;
            public RegWinNtCurrentVersion regCurVer;
        private sealed class SystemInfoGroup
            public WmiBaseBoard baseboard;
            public WmiBios bios;
            public WmiComputerSystem computer;
            public Processor[] processors;
            public NetworkAdapter[] networkAdapters;
        private sealed class HyperVInfo
            public bool? Present;
            public bool? VMMonitorModeExtensions;
            public bool? SecondLevelAddressTranslation;
            public bool? VirtualizationFirmwareEnabled;
            public bool? DataExecutionPreventionAvailable;
        private sealed class DeviceGuardInfo
            public DeviceGuardSmartStatus status;
            public DeviceGuard deviceGuard;
        private sealed class MiscInfoGroup
            public ulong? physicallyInstalledMemory;
            public string timeZone;
            public string logonServer;
            public FirmwareType? firmwareType;
            public PowerPlatformRole? powerPlatformRole;
            public WmiKeyboard[] keyboards;
            public HyperVInfo hyperV;
            public ServerLevel? serverLevel;
            public DeviceGuardInfo deviceGuard;
        #endregion Inner Types
        #region Static Data and Constants
        private const string activity = "Get-ComputerInfo";
        private const string localMachineName = null;
        #endregion Static Data and Constants
        #region Instance Data
        private readonly string _machineName = localMachineName;  // we might need to have cmdlet work on another machine
        /// Collection of property names from the Property parameter,
        /// including any names resulting from the expansion of wild-card
        /// patterns given. This list will itself contain no wildcard patterns.
        private List<string> _namedProperties = null;
        #endregion Instance Data
        /// The Property parameter contains the names of properties to be retrieved.
        /// If this parameter is given, the cmdlet returns a PSCustomObject
        /// containing only the requested properties.
        /// Wild-card patterns may be provided.
        /// Any named properties that are not recognized are ignored. If no
        /// recognized properties are provided the cmdlet returns an empty
        /// PSCustomObject.
        /// If a provided wild-card pattern contains only an asterisk ("*"),
        /// the cmdlet will operate as if the parameter were not given at all
        /// and will return a fully-populated ComputerInfo object.
        public string[] Property { get; set; }
        /// Perform any first-stage processing.
            // if the Property parameter was given, determine the requested
            // property names
            if (Property != null && Property.Length > 0)
                    _namedProperties = CollectPropertyNames(Property);
                catch (WildcardPatternException ex)
                    WriteError(new ErrorRecord(ex, "WildcardPattern", ErrorCategory.InvalidArgument, this));
        /// Performs the cmdlet's work.
            // if the user provided property names but no matching properties
            // were found, return an empty custom object
            if (_namedProperties != null && _namedProperties.Count == 0)
                WriteObject(new PSObject());
            MiscInfoGroup miscInfo = null;
            var osInfo = new OSInfoGroup();
            var systemInfo = new SystemInfoGroup();
            var now = DateTime.Now;
            using (var session = CimSession.Create(_machineName))
                UpdateProgress(ComputerInfoResources.LoadingOperationSystemInfo);
                osInfo.os = session.GetFirst<WmiOperatingSystem>(CIMHelper.ClassNames.OperatingSystem);
                osInfo.pageFileUsage = session.GetAll<WmiPageFileUsage>(CIMHelper.ClassNames.PageFileUsage);
                if (osInfo.os != null)
                    osInfo.halVersion = GetHalVersion(session, osInfo.os.SystemDirectory);
                    if (osInfo.os.LastBootUpTime != null)
                        osInfo.upTime = now - osInfo.os.LastBootUpTime.Value;
                UpdateProgress(ComputerInfoResources.LoadingHotPatchInfo);
                osInfo.hotFixes = session.GetAll<HotFix>(CIMHelper.ClassNames.HotFix);
                UpdateProgress(ComputerInfoResources.LoadingRegistryInfo);
                osInfo.regCurVer = RegistryInfo.GetWinNtCurrentVersion();
                UpdateProgress(ComputerInfoResources.LoadingBiosInfo);
                systemInfo.bios = session.GetFirst<WmiBios>(CIMHelper.ClassNames.Bios);
                UpdateProgress(ComputerInfoResources.LoadingMotherboardInfo);
                systemInfo.baseboard = session.GetFirst<WmiBaseBoard>(CIMHelper.ClassNames.BaseBoard);
                UpdateProgress(ComputerInfoResources.LoadingComputerInfo);
                systemInfo.computer = session.GetFirst<WmiComputerSystem>(CIMHelper.ClassNames.ComputerSystem);
                miscInfo = GetOtherInfo(session);
                UpdateProgress(ComputerInfoResources.LoadingProcessorInfo);
                systemInfo.processors = GetProcessors(session);
                UpdateProgress(ComputerInfoResources.LoadingNetworkAdapterInfo);
                systemInfo.networkAdapters = GetNetworkAdapters(session);
                UpdateProgress(null);   // close the progress bar
            var infoOutput = CreateFullOutputObject(systemInfo, osInfo, miscInfo);
            if (_namedProperties != null)
                // var output = CreateCustomOutputObject(namedProperties, systemInfo, osInfo, miscInfo);
                var output = CreateCustomOutputObject(infoOutput, _namedProperties);
                WriteObject(output);
                WriteObject(infoOutput);
        #endregion Cmdlet Overrides
        /// Display progress.
        /// <param name="status">
        /// Text to be displayed in status bar
        private void UpdateProgress(string status)
            ProgressRecord progress = new(0, activity, status ?? ComputerResources.ProgressStatusCompleted);
            progress.RecordType = status == null ? ProgressRecordType.Completed : ProgressRecordType.Processing;
        /// Retrieves the version of the system's hal.dll.
        /// A <see cref="Microsoft.Management.Infrastructure.CimSession"/> object
        /// representing the CIM session to query.
        /// <param name="systemDirectory">
        /// Path to the system directory, which should contain the hal.dll file.
        private static string GetHalVersion(CimSession session, string systemDirectory)
            string halVersion = null;
                var halPath = CIMHelper.EscapePath(System.IO.Path.Combine(systemDirectory, "hal.dll"));
                var query = string.Create(CultureInfo.InvariantCulture, $"SELECT * FROM CIM_DataFile Where Name='{halPath}'");
                var instance = session.QueryFirstInstance(query);
                    halVersion = instance.CimInstanceProperties["Version"].Value.ToString();
                // On any error, fall through to the return
            return halVersion;
        /// Create an array of <see cref="NetworkAdapter"/> object from values in
        /// Win32_NetworkAdapter and Win32_NetworkAdapterConfiguration instances.
        /// A <see cref="Microsoft.Management.Infrastructure.CimSession"/> object representing
        /// a CIM session.
        /// An array of NetworkAdapter objects.
        /// This method matches network adapters associated network adapter configurations.
        /// The returned array contains entries only for matched adapter/configuration objects.
        private static NetworkAdapter[] GetNetworkAdapters(CimSession session)
            var adaptersMsft = session.GetAll<WmiMsftNetAdapter>(CIMHelper.MicrosoftNetworkAdapterNamespace, CIMHelper.ClassNames.MicrosoftNetworkAdapter);
            var adapters = session.GetAll<WmiNetworkAdapter>(CIMHelper.ClassNames.NetworkAdapter);
            var configs = session.GetAll<WmiNetworkAdapterConfiguration>(CIMHelper.ClassNames.NetworkAdapterConfiguration);
            var list = new List<NetworkAdapter>();
            if (adapters != null && configs != null)
                var configDict = new Dictionary<uint, WmiNetworkAdapterConfiguration>();
                foreach (var config in configs)
                    if (config.Index != null)
                        configDict[config.Index.Value] = config;
                if (configDict.Count > 0)
                    foreach (var adapter in adapters)
                        // Only include adapters that have a non-null connection status
                        // and a non-null index
                        if (adapter.NetConnectionStatus != null
                            && adapter.Index != null)
                            if (configDict.ContainsKey(adapter.Index.Value))
                                var config = configDict[adapter.Index.Value];
                                var nwAdapter = new NetworkAdapter
                                    Description = adapter.Description,
                                    ConnectionID = adapter.NetConnectionID
                                var status = EnumConverter<NetConnectionStatus>.Convert(adapter.NetConnectionStatus);
                                nwAdapter.ConnectionStatus = status == null ? NetConnectionStatus.Other
                                                                            : status.Value;
                                if (nwAdapter.ConnectionStatus == NetConnectionStatus.Connected)
                                    nwAdapter.DHCPEnabled = config.DHCPEnabled;
                                    nwAdapter.DHCPServer = config.DHCPServer;
                                    nwAdapter.IPAddresses = config.IPAddress;
                                list.Add(nwAdapter);
            return list.ToArray();
        /// Create an array of <see cref="Processor"/> objects, using data acquired
        /// from WMI via the Win32_Processor class.
        private static Processor[] GetProcessors(CimSession session)
            var processors = session.GetAll<WmiProcessor>(CIMHelper.ClassNames.Processor);
            if (processors != null)
                var list = new List<Processor>();
                foreach (var processor in processors)
                    var proc = new Processor();
                    proc.AddressWidth = processor.AddressWidth;
                    proc.Architecture = EnumConverter<CpuArchitecture>.Convert(processor.Architecture);
                    proc.Availability = EnumConverter<CpuAvailability>.Convert(processor.Availability);
                    proc.CpuStatus = EnumConverter<CpuStatus>.Convert(processor.CpuStatus);
                    proc.CurrentClockSpeed = processor.CurrentClockSpeed;
                    proc.DataWidth = processor.DataWidth;
                    proc.Description = processor.Description;
                    proc.Manufacturer = processor.Manufacturer;
                    proc.MaxClockSpeed = processor.MaxClockSpeed;
                    proc.Name = processor.Name;
                    proc.NumberOfCores = processor.NumberOfCores;
                    proc.NumberOfLogicalProcessors = processor.NumberOfLogicalProcessors;
                    proc.ProcessorID = processor.ProcessorId;
                    proc.ProcessorType = EnumConverter<ProcessorType>.Convert(processor.ProcessorType);
                    proc.Role = processor.Role;
                    proc.SocketDesignation = processor.SocketDesignation;
                    proc.Status = processor.Status;
                    list.Add(proc);
        private static bool CheckDeviceGuardLicense()
            const string propertyName = "CodeIntegrity-AllowConfigurablePolicy";
            // DeviceGuard is supported on all versions of PowerShell that execute on "full" SKUs
            if (Platform.IsWindows &&
                !(Platform.IsNanoServer || Platform.IsIoT))
                    int policy = 0;
                    if (Native.SLGetWindowsInformationDWORD(propertyName, out policy) == Native.S_OK
                        && policy == 1)
                    // if we fail to load the native dll or if the call fails
                    // catastrophically there's not much we can do except to
                    // consider there to be no license.
        /// Retrieve information related to Device Guard.
        /// A <see cref="DeviceGuard"/> object containing information related to
        /// the Device Guard feature
        private static DeviceGuardInfo GetDeviceGuard(CimSession session)
            DeviceGuard guard = null;
            var status = DeviceGuardSmartStatus.Off;
            if (CheckDeviceGuardLicense())
                var wmiGuard = session.GetFirst<WmiDeviceGuard>(CIMHelper.DeviceGuardNamespace,
                                                                CIMHelper.ClassNames.DeviceGuard);
                if (wmiGuard != null)
                    var smartStatus = EnumConverter<DeviceGuardSmartStatus>.Convert((int?)wmiGuard.VirtualizationBasedSecurityStatus ?? 0);
                    if (smartStatus != null)
                        status = (DeviceGuardSmartStatus)smartStatus;
                    guard = wmiGuard.AsOutputType;
            return new DeviceGuardInfo
                status = status,
                deviceGuard = guard
        /// A helper method used by GetHyperVisorInfo to retrieve a boolean
        /// property value.
        private static bool? GetBooleanProperty(CimInstance instance, string propertyName)
                    var property = instance.CimInstanceProperties[propertyName];
                    if (property != null && property.Value != null)
                        return (bool)property.Value;
                    // just in case the cast fails
                    // fall through to the null return
        /// Retrieve information related to HyperVisor.
        /// A <see cref="HyperVInfo"/> object containing information related to
        /// HyperVisor
        private static HyperVInfo GetHyperVisorInfo(CimSession session)
            HyperVInfo info = new();
            bool ok = false;
            CimInstance instance = null;
            using (instance = session.QueryFirstInstance(CIMHelper.WqlQueryAll(CIMHelper.ClassNames.ComputerSystem)))
                    info.Present = GetBooleanProperty(instance, "HypervisorPresent");
                    ok = true;
            // don't bother checking requirements if the HyperV in present
            // when the HyperV is present, the requirements values are misleading
            if (ok && info.Present != null && info.Present.Value)
            using (instance = session.QueryFirstInstance(CIMHelper.WqlQueryAll(CIMHelper.ClassNames.OperatingSystem)))
                    info.DataExecutionPreventionAvailable = GetBooleanProperty(instance, "DataExecutionPrevention_Available");
            using (instance = session.QueryFirstInstance(CIMHelper.WqlQueryAll(CIMHelper.ClassNames.Processor)))
                    info.SecondLevelAddressTranslation = GetBooleanProperty(instance, "SecondLevelAddressTranslationExtensions");
                    info.VirtualizationFirmwareEnabled = GetBooleanProperty(instance, "VirtualizationFirmwareEnabled");
                    info.VMMonitorModeExtensions = GetBooleanProperty(instance, "VMMonitorModeExtensions");
            return ok ? info : null;
        /// Retrieve miscellaneous system information.
        /// A <see cref="MiscInfoGroup"/> object containing miscellaneous
        /// system information
        private static MiscInfoGroup GetOtherInfo(CimSession session)
            var rv = new MiscInfoGroup();
            // get platform role
                // TODO: Local machine only. Check for that?
                uint powerRole = Native.PowerDeterminePlatformRoleEx(Native.POWER_PLATFORM_ROLE_V2);
                if (powerRole >= (uint)PowerPlatformRole.MaximumEnumValue)
                    rv.powerPlatformRole = PowerPlatformRole.Unspecified;
                    rv.powerPlatformRole = EnumConverter<PowerPlatformRole>.Convert((int)powerRole);
                // probably failed to load the DLL with PowerDeterminePlatformRoleEx
                // either way, move on
            // get secure-boot info
            // TODO: Local machine only? Check for that?
            rv.firmwareType = GetFirmwareType();
            // get amount of memory physically installed
            rv.physicallyInstalledMemory = GetPhysicallyInstalledSystemMemory();
            // get time zone
            // we'll use .Net's TimeZoneInfo for now. systeminfo uses Caption from Win32_TimeZone
            var tzi = TimeZoneInfo.Local;
            if (tzi != null)
                rv.timeZone = tzi.DisplayName;
            rv.logonServer = RegistryInfo.GetLogonServer();
            rv.keyboards = session.GetAll<WmiKeyboard>(CIMHelper.ClassNames.Keyboard);
            rv.hyperV = GetHyperVisorInfo(session);
            var serverLevels = RegistryInfo.GetServerLevels();
            uint value;
            if (serverLevels.TryGetValue("NanoServer", out value) && value == 1)
                rv.serverLevel = ServerLevel.NanoServer;
            else if (serverLevels.TryGetValue("ServerCore", out value) && value == 1)
                rv.serverLevel = ServerLevel.ServerCore;
                if (serverLevels.TryGetValue("Server-Gui-Mgmt", out value) && value == 1)
                    rv.serverLevel = ServerLevel.ServerCoreWithManagementTools;
                    if (serverLevels.TryGetValue("Server-Gui-Shell", out value) && value == 1)
                        rv.serverLevel = ServerLevel.FullServer;
            rv.deviceGuard = GetDeviceGuard(session);
        /// Wrapper around the native GetFirmwareType function.
        /// null if unsuccessful, otherwise FirmwareType enum specifying
        /// the firmware type.
        private static FirmwareType? GetFirmwareType()
                FirmwareType firmwareType;
                if (Native.GetFirmwareType(out firmwareType))
                    return firmwareType;
                // Probably failed to load the DLL or to file the function entry point.
                // Fail silently
        /// Wrapper around the native GetPhysicallyInstalledSystemMemory function.
        /// null if unsuccessful, otherwise the amount of physically installed memory.
        private static ulong? GetPhysicallyInstalledSystemMemory()
                ulong memory;
                if (Native.GetPhysicallyInstalledSystemMemory(out memory))
                    return memory;
        /// Create a new ComputerInfo object populated with the specified data objects.
        /// <param name="systemInfo">
        /// A <see cref="SystemInfoGroup"/> object containing system-related info
        /// such as BIOS, mother-board, computer system, etc.
        /// <param name="osInfo">
        /// An <see cref="OSInfoGroup"/> object containing operating-system information.
        /// <param name="otherInfo">
        /// A <see cref="MiscInfoGroup"/> object containing other information to be reported.
        /// A new ComputerInfo object to be output to PowerShell.
        private static ComputerInfo CreateFullOutputObject(SystemInfoGroup systemInfo, OSInfoGroup osInfo, MiscInfoGroup otherInfo)
            var output = new ComputerInfo();
            var regCurVer = osInfo.regCurVer;
            if (regCurVer != null)
                output.WindowsBuildLabEx = regCurVer.BuildLabEx;
                output.WindowsCurrentVersion = regCurVer.CurrentVersion;
                output.WindowsEditionId = regCurVer.EditionId;
                output.WindowsInstallationType = regCurVer.InstallationType;
                output.WindowsInstallDateFromRegistry = regCurVer.InstallDate;
                output.WindowsProductId = regCurVer.ProductId;
                output.WindowsProductName = regCurVer.ProductName;
                output.WindowsRegisteredOrganization = regCurVer.RegisteredOrganization;
                output.WindowsRegisteredOwner = regCurVer.RegisteredOwner;
                output.WindowsSystemRoot = regCurVer.SystemRoot;
                output.WindowsVersion = regCurVer.ReleaseId;
                output.WindowsUBR = regCurVer.UBR;
            var os = osInfo.os;
            if (os != null)
                output.OsName = os.Caption;
                output.OsBootDevice = os.BootDevice;
                output.OsBuildNumber = os.BuildNumber;
                output.OsBuildType = os.BuildType;
                output.OsCodeSet = os.CodeSet;
                output.OsCountryCode = os.CountryCode;
                output.OsCSDVersion = os.CSDVersion;
                output.OsCurrentTimeZone = os.CurrentTimeZone;
                output.OsDataExecutionPreventionAvailable = os.DataExecutionPrevention_Available;
                output.OsDataExecutionPrevention32BitApplications = os.DataExecutionPrevention_32BitApplications;
                output.OsDataExecutionPreventionDrivers = os.DataExecutionPrevention_Drivers;
                output.OsDataExecutionPreventionSupportPolicy =
                    EnumConverter<DataExecutionPreventionSupportPolicy>.Convert(os.DataExecutionPrevention_SupportPolicy);
                output.OsDebug = os.Debug;
                output.OsDistributed = os.Distributed;
                output.OsEncryptionLevel = EnumConverter<OSEncryptionLevel>.Convert((int?)os.EncryptionLevel);
                output.OsForegroundApplicationBoost = EnumConverter<ForegroundApplicationBoost>.Convert(os.ForegroundApplicationBoost);
                output.OsTotalSwapSpaceSize = os.TotalSwapSpaceSize;
                output.OsTotalVisibleMemorySize = os.TotalVisibleMemorySize;
                output.OsFreePhysicalMemory = os.FreePhysicalMemory;
                output.OsFreeSpaceInPagingFiles = os.FreeSpaceInPagingFiles;
                output.OsTotalVirtualMemorySize = os.TotalVirtualMemorySize;
                output.OsFreeVirtualMemory = os.FreeVirtualMemory;
                if (os.TotalVirtualMemorySize != null && os.FreeVirtualMemory != null)
                    output.OsInUseVirtualMemory = os.TotalVirtualMemorySize - os.FreeVirtualMemory;
                output.OsInstallDate = os.InstallDate;
                output.OsLastBootUpTime = os.LastBootUpTime;
                output.OsLocalDateTime = os.LocalDateTime;
                output.OsLocaleID = os.Locale;
                output.OsManufacturer = os.Manufacturer;
                output.OsMaxNumberOfProcesses = os.MaxNumberOfProcesses;
                output.OsMaxProcessMemorySize = os.MaxProcessMemorySize;
                output.OsMuiLanguages = os.MUILanguages;
                output.OsNumberOfLicensedUsers = os.NumberOfLicensedUsers;
                output.OsNumberOfProcesses = os.NumberOfProcesses;
                output.OsNumberOfUsers = os.NumberOfUsers;
                output.OsOperatingSystemSKU = EnumConverter<OperatingSystemSKU>.Convert((int?)os.OperatingSystemSKU);
                output.OsOrganization = os.Organization;
                output.OsArchitecture = os.OSArchitecture;
                output.OsLanguage = os.LanguageName;
                output.OsProductSuites = os.ProductSuites;
                output.OsOtherTypeDescription = os.OtherTypeDescription;
                output.OsPAEEnabled = os.PAEEnabled;
                output.OsPortableOperatingSystem = os.PortableOperatingSystem;
                output.OsPrimary = os.Primary;
                output.OsProductType = EnumConverter<ProductType>.Convert((int?)os.ProductType);
                output.OsRegisteredUser = os.RegisteredUser;
                output.OsSerialNumber = os.SerialNumber;
                output.OsServicePackMajorVersion = os.ServicePackMajorVersion;
                output.OsServicePackMinorVersion = os.ServicePackMinorVersion;
                output.OsSizeStoredInPagingFiles = os.SizeStoredInPagingFiles;
                output.OsStatus = os.Status;
                output.OsSuites = os.Suites;
                output.OsSystemDevice = os.SystemDevice;
                output.OsSystemDirectory = os.SystemDirectory;
                output.OsSystemDrive = os.SystemDrive;
                output.OsType = EnumConverter<OSType>.Convert(os.OSType);
                output.OsVersion = os.Version;
                output.OsWindowsDirectory = os.WindowsDirectory;
                output.OsHardwareAbstractionLayer = osInfo.halVersion;
                output.OsLocale = os.GetLocale();
                output.OsUptime = osInfo.upTime;
                output.OsHotFixes = osInfo.hotFixes;
                var pageFileUsage = osInfo.pageFileUsage;
                if (pageFileUsage != null)
                    output.OsPagingFiles = new string[pageFileUsage.Length];
                    for (int i = 0; i < pageFileUsage.Length; i++)
                        output.OsPagingFiles[i] = pageFileUsage[i].Caption;
            var bios = systemInfo.bios;
            if (bios != null)
                output.BiosCharacteristics = bios.BiosCharacteristics;
                output.BiosBuildNumber = bios.BuildNumber;
                output.BiosBIOSVersion = bios.BIOSVersion;
                output.BiosCaption = bios.Caption;
                output.BiosCodeSet = bios.CodeSet;
                output.BiosCurrentLanguage = bios.CurrentLanguage;
                output.BiosDescription = bios.Description;
                output.BiosEmbeddedControllerMajorVersion = bios.EmbeddedControllerMajorVersion;
                output.BiosEmbeddedControllerMinorVersion = bios.EmbeddedControllerMinorVersion;
                output.BiosIdentificationCode = bios.IdentificationCode;
                output.BiosInstallableLanguages = bios.InstallableLanguages;
                output.BiosInstallDate = bios.InstallDate;
                output.BiosLanguageEdition = bios.LanguageEdition;
                output.BiosListOfLanguages = bios.ListOfLanguages;
                output.BiosManufacturer = bios.Manufacturer;
                output.BiosName = bios.Name;
                output.BiosOtherTargetOS = bios.OtherTargetOS;
                output.BiosPrimaryBIOS = bios.PrimaryBIOS;
                output.BiosReleaseDate = bios.ReleaseDate;
                output.BiosSerialNumber = bios.SerialNumber;
                output.BiosSMBIOSBIOSVersion = bios.SMBIOSBIOSVersion;
                output.BiosSMBIOSMajorVersion = bios.SMBIOSMajorVersion;
                output.BiosSMBIOSMinorVersion = bios.SMBIOSMinorVersion;
                output.BiosSMBIOSPresent = bios.SMBIOSPresent;
                output.BiosSoftwareElementState = EnumConverter<SoftwareElementState>.Convert(bios.SoftwareElementState);
                output.BiosStatus = bios.Status;
                output.BiosSystemBiosMajorVersion = bios.SystemBiosMajorVersion;
                output.BiosSystemBiosMinorVersion = bios.SystemBiosMinorVersion;
                output.BiosTargetOperatingSystem = bios.TargetOperatingSystem;
                output.BiosVersion = bios.Version;
                if (otherInfo != null)
                    output.BiosFirmwareType = otherInfo.firmwareType;
            var computer = systemInfo.computer;
            if (computer != null)
                output.CsAdminPasswordStatus = EnumConverter<HardwareSecurity>.Convert(computer.AdminPasswordStatus);
                output.CsAutomaticManagedPagefile = computer.AutomaticManagedPagefile;
                output.CsAutomaticResetBootOption = computer.AutomaticResetBootOption;
                output.CsAutomaticResetCapability = computer.AutomaticResetCapability;
                output.CsBootOptionOnLimit = EnumConverter<BootOptionAction>.Convert(computer.BootOptionOnLimit);
                output.CsBootOptionOnWatchDog = EnumConverter<BootOptionAction>.Convert(computer.BootOptionOnWatchDog);
                output.CsBootROMSupported = computer.BootROMSupported;
                output.CsBootStatus = computer.BootStatus;
                output.CsBootupState = computer.BootupState;
                output.CsCaption = computer.Caption;
                output.CsChassisBootupState = EnumConverter<SystemElementState>.Convert(computer.ChassisBootupState);
                output.CsChassisSKUNumber = computer.ChassisSKUNumber;
                output.CsCurrentTimeZone = computer.CurrentTimeZone;
                output.CsDaylightInEffect = computer.DaylightInEffect;
                output.CsDescription = computer.Description;
                output.CsDNSHostName = computer.DNSHostName;
                output.CsDomain = computer.Domain;
                output.CsDomainRole = EnumConverter<DomainRole>.Convert(computer.DomainRole);
                output.CsEnableDaylightSavingsTime = computer.EnableDaylightSavingsTime;
                output.CsFrontPanelResetStatus = EnumConverter<HardwareSecurity>.Convert(computer.FrontPanelResetStatus);
                output.CsHypervisorPresent = computer.HypervisorPresent;
                output.CsInfraredSupported = computer.InfraredSupported;
                output.CsInitialLoadInfo = computer.InitialLoadInfo;
                output.CsInstallDate = computer.InstallDate;
                output.CsKeyboardPasswordStatus = EnumConverter<HardwareSecurity>.Convert(computer.KeyboardPasswordStatus);
                output.CsLastLoadInfo = computer.LastLoadInfo;
                output.CsManufacturer = computer.Manufacturer;
                output.CsModel = computer.Model;
                output.CsName = computer.Name;
                output.CsNetworkAdapters = systemInfo.networkAdapters;
                output.CsNetworkServerModeEnabled = computer.NetworkServerModeEnabled;
                output.CsNumberOfLogicalProcessors = computer.NumberOfLogicalProcessors;
                output.CsNumberOfProcessors = computer.NumberOfProcessors;
                output.CsProcessors = systemInfo.processors;
                output.CsOEMStringArray = computer.OEMStringArray;
                output.CsPartOfDomain = computer.PartOfDomain;
                output.CsPauseAfterReset = computer.PauseAfterReset;
                output.CsPCSystemType = EnumConverter<PCSystemType>.Convert(computer.PCSystemType);
                output.CsPCSystemTypeEx = EnumConverter<PCSystemTypeEx>.Convert(computer.PCSystemTypeEx);
                output.CsPowerManagementCapabilities = computer.GetPowerManagementCapabilities();
                output.CsPowerManagementSupported = computer.PowerManagementSupported;
                output.CsPowerOnPasswordStatus = EnumConverter<HardwareSecurity>.Convert(computer.PowerOnPasswordStatus);
                output.CsPowerState = EnumConverter<PowerState>.Convert(computer.PowerState);
                output.CsPowerSupplyState = EnumConverter<SystemElementState>.Convert(computer.PowerSupplyState);
                output.CsPrimaryOwnerContact = computer.PrimaryOwnerContact;
                output.CsPrimaryOwnerName = computer.PrimaryOwnerName;
                output.CsResetCapability = EnumConverter<ResetCapability>.Convert(computer.ResetCapability);
                output.CsResetCount = computer.ResetCount;
                output.CsResetLimit = computer.ResetLimit;
                output.CsRoles = computer.Roles;
                output.CsStatus = computer.Status;
                output.CsSupportContactDescription = computer.SupportContactDescription;
                output.CsSystemFamily = computer.SystemFamily;
                output.CsSystemSKUNumber = computer.SystemSKUNumber;
                output.CsSystemType = computer.SystemType;
                output.CsThermalState = EnumConverter<SystemElementState>.Convert(computer.ThermalState);
                output.CsTotalPhysicalMemory = computer.TotalPhysicalMemory;
                output.CsUserName = computer.UserName;
                output.CsWakeUpType = EnumConverter<WakeUpType>.Convert(computer.WakeUpType);
                output.CsWorkgroup = computer.Workgroup;
                    output.CsPhysicallyInstalledMemory = otherInfo.physicallyInstalledMemory;
                output.TimeZone = otherInfo.timeZone;
                output.LogonServer = otherInfo.logonServer;
                output.PowerPlatformRole = otherInfo.powerPlatformRole;
                if (otherInfo.keyboards.Length > 0)
                    // TODO: handle multiple keyboards?
                    // there might be several keyboards found. For the moment
                    // we display info for only one
                    string layout = otherInfo.keyboards[0].Layout;
                    output.KeyboardLayout = Conversion.GetLocaleName(layout);
                if (otherInfo.hyperV != null)
                    output.HyperVisorPresent = otherInfo.hyperV.Present;
                    output.HyperVRequirementDataExecutionPreventionAvailable = otherInfo.hyperV.DataExecutionPreventionAvailable;
                    output.HyperVRequirementSecondLevelAddressTranslation = otherInfo.hyperV.SecondLevelAddressTranslation;
                    output.HyperVRequirementVirtualizationFirmwareEnabled = otherInfo.hyperV.VirtualizationFirmwareEnabled;
                    output.HyperVRequirementVMMonitorModeExtensions = otherInfo.hyperV.VMMonitorModeExtensions;
                output.OsServerLevel = otherInfo.serverLevel;
                var deviceGuardInfo = otherInfo.deviceGuard;
                if (deviceGuardInfo != null)
                    output.DeviceGuardSmartStatus = deviceGuardInfo.status;
                    var deviceGuard = deviceGuardInfo.deviceGuard;
                    if (deviceGuard != null)
                        output.DeviceGuardRequiredSecurityProperties = deviceGuard.RequiredSecurityProperties;
                        output.DeviceGuardAvailableSecurityProperties = deviceGuard.AvailableSecurityProperties;
                        output.DeviceGuardSecurityServicesConfigured = deviceGuard.SecurityServicesConfigured;
                        output.DeviceGuardSecurityServicesRunning = deviceGuard.SecurityServicesRunning;
                        output.DeviceGuardCodeIntegrityPolicyEnforcementStatus = deviceGuard.CodeIntegrityPolicyEnforcementStatus;
                        output.DeviceGuardUserModeCodeIntegrityPolicyEnforcementStatus = deviceGuard.UserModeCodeIntegrityPolicyEnforcementStatus;
            return output;
        /// Create a new PSObject, containing only those properties named in the
        /// namedProperties parameter.
        /// <param name="info">
        /// A <see cref="ComputerInfo"/> containing all the acquired system information
        /// <param name="namedProperties">
        /// A list of property names to be included in the returned object
        /// A new PSObject with the properties specified in the <paramref name="namedProperties"/>
        /// parameter
        private static PSObject CreateCustomOutputObject(ComputerInfo info, List<string> namedProperties)
            var rv = new PSObject();
            if (info != null && namedProperties != null && namedProperties.Count > 0)
                // Walk the list of named properties, find a matching property in the
                // info object, and create a new property on the results object
                // with the associated value.
                var type = info.GetType();
                foreach (var propertyName in namedProperties)
                    var propInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                    if (propInfo != null)
                        object value = propInfo.GetValue(info);
                        rv.Properties.Add(new PSNoteProperty(propertyName, value));
        /// Get the names of all <see cref="ComputerInfo"/> properties. This is
        /// part of the processes of validating property names provided by the user.
        private static List<string> GetComputerInfoPropertyNames()
            var rv = new List<string>();
            var type = typeof(ComputerInfo);
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                rv.Add(prop.Name);
        /// Expand any wild-card patterns into known property names.
        /// <param name="propertyNames">
        /// List of known property names
        /// <param name="pattern">
        /// The wild-card pattern used to perform globbing
        private static List<string> ExpandWildcardPropertyNames(List<string> propertyNames, string pattern)
            var wcp = new WildcardPattern(pattern, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
            foreach (var name in propertyNames)
                if (wcp.IsMatch(name))
                    rv.Add(name);
        /// Produce a list of known, valid property names from property-name
        /// parameters. These parameter may use wild-card patterns and may
        /// contain invalid property names. This method expands wild-card
        /// patterns and filter out any invalid property names.
        /// <param name="requestedProperties"></param>
        private static List<string> CollectPropertyNames(string[] requestedProperties)
            // A quick scan through the requested properties to make sure
            // we want to use user-specified properties
            foreach (var name in requestedProperties)
                if (WildcardPattern.ContainsWildcardCharacters(name))
                    if (name == "*")
                        return null;    // we treat a wild-card pattern of "*" as if no properties were named
            var availableProperties = GetComputerInfoPropertyNames();
            // walk though the requested properties again, expanding and collecting property names
                    foreach (var matchedName in ExpandWildcardPropertyNames(availableProperties, name))
                        if (!rv.Contains(matchedName))
                            rv.Add(matchedName);
                    // find a matching property name via case-insensitive string comparison
                    Predicate<string> pred = (s) =>
                                                    return string.Equals(s,
                                                                          StringComparison.OrdinalIgnoreCase);
                    var propertyName = availableProperties.Find(pred);
                    // add the properly-cased name, if found, to the list
                    if (propertyName != null && !rv.Contains(propertyName))
                        rv.Add(propertyName);
    #endregion GetComputerInfoCommand cmdlet implementation
    #region Helper classes
    internal static class Conversion
        /// Attempt to convert a string representation of a base-16 value
        /// into an integer.
        /// <param name="hexString">
        /// A string containing the text to be parsed.
        /// An integer into which the parsed value is stored. If the string
        /// cannot be converted, this parameter is set to 0.
        /// Returns true if the conversion was successful, false otherwise.
        /// The hexString parameter must contain a hexadecimal value, with no
        /// base-indication prefix. For example, the string "0409" will be
        /// parsed into the base-10 integer value 1033, while the string "0x0409"
        /// will fail to parse due to the "0x" base-indication prefix.
        internal static bool TryParseHex(string hexString, out uint value)
                value = Convert.ToUInt32(hexString, 16);
                value = 0;
        /// Attempt to create a <see cref="System.Globalization.CultureInfo"/>
        /// object from a locale string as retrieved from WMI.
        /// <param name="locale">
        /// A string containing WMI's notion (usually) of a locale.
        /// A CultureInfo object if successful, null otherwise.
        /// This method first tries to convert the string to a hex value
        /// and get the CultureInfo object from that value.
        /// Failing that it attempts to retrieve the CultureInfo object
        /// using the locale string as passed.
        internal static string GetLocaleName(string locale)
            CultureInfo culture = null;
            if (locale != null)
                    // The "locale" must contain a hexadecimal value, with no
                    // base-indication prefix. For example, the string "0409" will be
                    // parsed into the base-10 integer value 1033, while the string "0x0409"
                    // will fail to parse due to the "0x" base-indication prefix.
                    if (uint.TryParse(locale, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint localeNum))
                        culture = CultureInfo.GetCultureInfo((int)localeNum);
                    // If TryParse failed we'll try using the original string as culture name
                    culture ??= CultureInfo.GetCultureInfo(locale);
                    culture = null;
            return culture?.Name;
        /// Convert a Unix time, expressed in seconds, to a <see cref="DateTime"/>.
        /// <param name="seconds">Number of seconds since the Unix epoch.</param>
        /// A DateTime object representing the date and time represented by the
        /// <paramref name="seconds"/> parameter.
        internal static DateTime UnixSecondsToDateTime(long seconds)
#if false   // requires .NET 4.6 or higher
            return DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;
            const int DaysPerYear = 365;
            const int DaysPer4Years = DaysPerYear * 4 + 1;
            const int DaysPer100Years = DaysPer4Years * 25 - 1;
            const int DaysPer400Years = DaysPer100Years * 4 + 1;
            const int DaysTo1970 = DaysPer400Years * 4 + DaysPer100Years * 3 + DaysPer4Years * 17 + DaysPerYear;
            const long UnixEpochTicks = TimeSpan.TicksPerDay * DaysTo1970;
            long ticks = seconds * TimeSpan.TicksPerSecond + UnixEpochTicks;
            return new DateTimeOffset(ticks, TimeSpan.Zero).DateTime;
    /// The EnumConverter<typeparamref name="T"/> class contains a method
    /// for converting an integer to a nullable enum of the type specified
    /// in T.
    /// The type of enum to be the destination of the conversion.
    internal static class EnumConverter<T> where T : struct, IConvertible
        // The converter object
        private static readonly Func<int, T?> s_convert = MakeConverter();
        /// Convert an integer to a Nullable enum of type T.
        /// The integer value to be converted to the specified enum type.
        /// A Nullable<typeparamref name="T"/> enum object. If the value
        /// is convertible to a valid enum value, the returned object's
        /// value will contain the converted value, otherwise the returned
        /// object will be null.
        internal static T? Convert(int? value)
                if (value.HasValue)
                    return s_convert(value.Value);
                // nothing should go wrong, but just in case
                // fall through to the return null below
            return (T?)null;
        /// Create a converter using Linq Expression classes.
        /// A generic Func{} object to convert an int to the specified enum type.
        internal static Func<int, T?> MakeConverter()
            var param = Expression.Parameter(typeof(int));
            var method = Expression.Lambda<Func<int, T?>>
                            (Expression.Convert(param, typeof(T?)), param);
            return method.Compile();
    internal static class RegistryInfo
        public static Dictionary<string, uint> GetServerLevels()
            const string keyPath = @"Software\Microsoft\Windows NT\CurrentVersion\Server\ServerLevels";
            var rv = new Dictionary<string, uint>();
            using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
                    foreach (var name in key.GetValueNames())
                        if (key.GetValueKind(name) == RegistryValueKind.DWord)
                            var val = key.GetValue(name);
                            rv.Add(name, Convert.ToUInt32(val));
        public static string GetLogonServer()
            const string valueName = "LOGONSERVER";
            const string keyPath = "Volatile Environment";
            using (var key = Registry.CurrentUser.OpenSubKey(keyPath))
                    return (string)key.GetValue(valueName, null);
        public static RegWinNtCurrentVersion GetWinNtCurrentVersion()
            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion"))
                    object temp = key.GetValue("InstallDate");
                    return new RegWinNtCurrentVersion()
                        BuildLabEx = (string)key.GetValue("BuildLabEx"),
                        CurrentVersion = (string)key.GetValue("CurrentVersion"),
                        EditionId = (string)key.GetValue("EditionID"),
                        InstallationType = (string)key.GetValue("InstallationType"),
                        InstallDate = temp == null ? (DateTime?)null
                                                                : Conversion.UnixSecondsToDateTime((long)(int)temp),
                        ProductId = (string)key.GetValue("ProductId"),
                        ProductName = (string)key.GetValue("ProductName"),
                        RegisteredOrganization = (string)key.GetValue("RegisteredOrganization"),
                        RegisteredOwner = (string)key.GetValue("RegisteredOwner"),
                        SystemRoot = (string)key.GetValue("SystemRoot"),
                        ReleaseId = (string)key.GetValue("ReleaseId"),
                        UBR = (int?)key.GetValue("UBR")
    #endregion Helper classes
    #region Intermediate WMI classes
    /// Base class for some of the other Intermediate WMI classes,
    /// providing some shared methods.
    internal abstract class WmiClassBase
        /// Get a language name from a language identifier.
        /// <param name="lcid">
        /// A nullable integer containing the language ID for the desired language.
        /// A string containing the display name of the language identified by
        /// the language parameter. If the language parameter is null or has a
        /// value that is not a valid language ID, the method returns null.
        protected static string GetLanguageName(uint? lcid)
            if (lcid != null && lcid >= 0)
                    return CultureInfo.GetCultureInfo((int)lcid.Value).Name;
#pragma warning disable 649 // fields and properties in these class are assigned dynamically
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Class is instantiated directly from a CIM instance")]
    internal sealed class WmiBaseBoard
        public string Caption;
        public string[] ConfigOptions;
        public float? Depth;
        public string Description;
        public float? Height;
        public bool? HostingBoard;
        public bool? HotSwappable;
        public DateTime? InstallDate;
        public string Manufacturer;
        public string Model;
        public string Name;
        public string OtherIdentifyingInfo;
        public string PartNumber;
        public bool? PoweredOn;
        public string Product;
        public bool? Removable;
        public bool? Replaceable;
        public string RequirementsDescription;
        public bool? RequiresDaughterBoard;
        public string SerialNumber;
        public string SKU;
        public string SlotLayout;
        public bool? SpecialRequirements;
        public string Status;
        public string Tag;
        public string Version;
        public float? Weight;
        public float? Width;
    internal sealed class WmiBios : WmiClassBase
        public ushort[] BiosCharacteristics;
        public string[] BIOSVersion;
        public string BuildNumber;
        public string CodeSet;
        public string CurrentLanguage;
        public byte? EmbeddedControllerMajorVersion;
        public byte? EmbeddedControllerMinorVersion;
        public string IdentificationCode;
        public ushort? InstallableLanguages;
        public string LanguageEdition;
        public string[] ListOfLanguages;
        public string OtherTargetOS;
        public bool? PrimaryBIOS;
        public DateTime? ReleaseDate;
        public string SMBIOSBIOSVersion;
        public ushort? SMBIOSMajorVersion;
        public ushort? SMBIOSMinorVersion;
        public bool? SMBIOSPresent;
        public ushort? SoftwareElementState;
        public byte? SystemBiosMajorVersion;
        public byte? SystemBiosMinorVersion;
        public ushort? TargetOperatingSystem;
    internal sealed class WmiComputerSystem
        public ushort? AdminPasswordStatus;
        public bool? AutomaticManagedPagefile;
        public bool? AutomaticResetBootOption;
        public bool? AutomaticResetCapability;
        public ushort? BootOptionOnLimit;
        public ushort? BootOptionOnWatchDog;
        public bool? BootROMSupported;
        public string BootupState;
        public ushort[] BootStatus;
        public ushort? ChassisBootupState;
        public string ChassisSKUNumber;
        public short? CurrentTimeZone;
        public bool? DaylightInEffect;
        public string DNSHostName;
        public string Domain;
        public ushort? DomainRole;
        public bool? EnableDaylightSavingsTime;
        public ushort? FrontPanelResetStatus;
        public bool? HypervisorPresent;
        public bool? InfraredSupported;
        public string InitialLoadInfo;
        public ushort? KeyboardPasswordStatus;
        public string LastLoadInfo;
        public bool? NetworkServerModeEnabled;
        public uint? NumberOfLogicalProcessors;
        public uint? NumberOfProcessors;
        public string[] OEMStringArray;
        public bool? PartOfDomain;
        public long? PauseAfterReset;
        public ushort? PCSystemType;
        public ushort? PCSystemTypeEx;
        public ushort[] PowerManagementCapabilities;
        public bool? PowerManagementSupported;
        public ushort? PowerOnPasswordStatus;
        public ushort? PowerState;
        public ushort? PowerSupplyState;
        public string PrimaryOwnerContact;
        public string PrimaryOwnerName;
        public ushort? ResetCapability;
        public short? ResetCount;
        public short? ResetLimit;
        public string[] Roles;
        public string[] SupportContactDescription;
        public string SystemFamily;
        public string SystemSKUNumber;
        public string SystemType;
        public ushort? ThermalState;
        public ulong? TotalPhysicalMemory;
        public string UserName;
        public ushort? WakeUpType;
        public string Workgroup;
        public PowerManagementCapabilities[] GetPowerManagementCapabilities()
            if (PowerManagementCapabilities != null)
                var list = new List<PowerManagementCapabilities>();
                foreach (var cap in PowerManagementCapabilities)
                    var val = EnumConverter<PowerManagementCapabilities>.Convert(cap);
                    if (val != null)
                        list.Add(val.Value);
    internal sealed class WmiDeviceGuard
        public uint[] AvailableSecurityProperties;
        public uint? CodeIntegrityPolicyEnforcementStatus;
        public uint? UsermodeCodeIntegrityPolicyEnforcementStatus;
        public uint[] RequiredSecurityProperties;
        public uint[] SecurityServicesConfigured;
        public uint[] SecurityServicesRunning;
        public uint? VirtualizationBasedSecurityStatus;
        public DeviceGuard AsOutputType
                var guard = new DeviceGuard();
                var status = EnumConverter<DeviceGuardSmartStatus>.Convert((int?)VirtualizationBasedSecurityStatus);
                if (status != null && status != DeviceGuardSmartStatus.Off)
                    var listHardware = new List<DeviceGuardHardwareSecure>();
                    for (int i = 0; i < RequiredSecurityProperties.Length; i++)
                        var temp = EnumConverter<DeviceGuardHardwareSecure>.Convert((int?)RequiredSecurityProperties[i]);
                            listHardware.Add(temp.Value);
                    guard.RequiredSecurityProperties = listHardware.ToArray();
                    listHardware.Clear();
                    for (int i = 0; i < AvailableSecurityProperties.Length; i++)
                        var temp = EnumConverter<DeviceGuardHardwareSecure>.Convert((int?)AvailableSecurityProperties[i]);
                    guard.AvailableSecurityProperties = listHardware.ToArray();
                    var listSoftware = new List<DeviceGuardSoftwareSecure>();
                    for (int i = 0; i < SecurityServicesConfigured.Length; i++)
                        var temp = EnumConverter<DeviceGuardSoftwareSecure>.Convert((int?)SecurityServicesConfigured[i]);
                            listSoftware.Add(temp.Value);
                    guard.SecurityServicesConfigured = listSoftware.ToArray();
                    listSoftware.Clear();
                    for (int i = 0; i < SecurityServicesRunning.Length; i++)
                        var temp = EnumConverter<DeviceGuardSoftwareSecure>.Convert((int?)SecurityServicesRunning[i]);
                    guard.SecurityServicesRunning = listSoftware.ToArray();
                var configCiStatus = EnumConverter<DeviceGuardConfigCodeIntegrityStatus>.Convert((int?)CodeIntegrityPolicyEnforcementStatus);
                var userModeCiStatus = EnumConverter<DeviceGuardConfigCodeIntegrityStatus>.Convert((int?)UsermodeCodeIntegrityPolicyEnforcementStatus);
                guard.CodeIntegrityPolicyEnforcementStatus = configCiStatus;
                guard.UserModeCodeIntegrityPolicyEnforcementStatus = userModeCiStatus;
                return guard;
    internal sealed class WmiKeyboard
        public ushort? Availability;
        public uint? ConfigManagerErrorCode;
        public bool? ConfigManagerUserConfig;
        public string DeviceID;
        public bool? ErrorCleared;
        public string ErrorDescription;
        public bool? IsLocked;
        public uint? LastErrorCode;
        public string Layout;
        public ushort? NumberOfFunctionKeys;
        public ushort? Password;
        public string PNPDeviceID;
        public ushort? StatusInfo;
        public string SystemCreationClassName;
        public string SystemName;
    internal sealed class WMiLogicalMemory
        // TODO: fill this in!!!
        public uint? TotalPhysicalMemory;
    internal sealed class WmiMsftNetAdapter
        public ulong? Speed;
        public ulong? MaxSpeed;
        public ulong? RequestedSpeed;
        public ushort? UsageRestriction;
        public ushort? PortType;
        public string OtherPortType;
        public string OtherNetworkPortType;
        public ushort? PortNumber;
        public ushort? LinkTechnology;
        public string OtherLinkTechnology;
        public string PermanentAddress;
        public string[] NetworkAddresses;
        public bool? FullDuplex;
        public bool? AutoSense;
        public ulong? SupportedMaximumTransmissionUnit;
        public ulong? ActiveMaximumTransmissionUnit;
        public string InterfaceDescription;
        public string InterfaceName;
        public ulong? NetLuid;
        public string InterfaceGuid;
        public uint? InterfaceIndex;
        public string DeviceName;
        public uint? NetLuidIndex;
        public bool? Virtual;
        public bool? Hidden;
        public bool? NotUserRemovable;
        public bool? IMFilter;
        public uint? InterfaceType;
        public bool? HardwareInterface;
        public bool? WdmInterface;
        public bool? EndPointInterface;
        public bool? iSCSIInterface;
        public uint? State;
        public uint? NdisMedium;
        public uint? NdisPhysicalMedium;
        public uint? InterfaceOperationalStatus;
        public bool? OperationalStatusDownDefaultPortNotAuthenticated;
        public bool? OperationalStatusDownMediaDisconnected;
        public bool? OperationalStatusDownInterfacePaused;
        public bool? OperationalStatusDownLowPowerState;
        public uint? InterfaceAdminStatus;
        public uint? MediaConnectState;
        public uint? MtuSize;
        public ushort? VlanID;
        public ulong? TransmitLinkSpeed;
        public ulong? ReceiveLinkSpeed;
        public bool? PromiscuousMode;
        public bool? DeviceWakeUpEnable;
        public bool? ConnectorPresent;
        public uint? MediaDuplexState;
        public string DriverDate;
        public ulong? DriverDateData;
        public string DriverVersionString;
        public string DriverName;
        public string DriverDescription;
        public ushort? MajorDriverVersion;
        public ushort? MinorDriverVersion;
        public byte? DriverMajorNdisVersion;
        public byte? DriverMinorNdisVersion;
        public string PnPDeviceID;
        public string DriverProvider;
        public string ComponentID;
        public uint[] LowerLayerInterfaceIndices;
        public uint[] HigherLayerInterfaceIndices;
        public bool? AdminLocked;
    internal sealed class WmiNetworkAdapter
        public string AdapterType;
        public ushort? AdapterTypeID;
        public string GUID;
        public uint? Index;
        public bool? Installed;
        public string MACAddress;
        public uint? MaxNumberControlled;
        public string NetConnectionID;
        public ushort? NetConnectionStatus;
        public bool? NetEnabled;
        public bool? PhysicalAdapter;
        public string ProductName;
        public string ServiceName;
        public DateTime? TimeOfLastReset;
    internal sealed class WmiNetworkAdapterConfiguration
        public bool? ArpAlwaysSourceRoute;
        public bool? ArpUseEtherSNAP;
        public string DatabasePath;
        public bool? DeadGWDetectEnabled;
        public string[] DefaultIPGateway;
        public byte? DefaultTOS;
        public byte? DefaultTTL;
        public bool? DHCPEnabled;
        public DateTime? DHCPLeaseExpires;
        public DateTime? DHCPLeaseObtained;
        public string DHCPServer;
        public string DNSDomain;
        public string[] DNSDomainSuffixSearchOrder;
        public bool? DNSEnabledForWINSResolution;
        public string[] DNSServerSearchOrder;
        public bool? DomainDNSRegistrationEnabled;
        public uint? ForwardBufferMemory;
        public bool? FullDNSRegistrationEnabled;
        public ushort[] GatewayCostMetric;
        public byte? IGMPLevel;
        public string[] IPAddress;
        public uint? IPConnectionMetric;
        public bool? IPEnabled;
        public bool? IPFilterSecurityEnabled;
        public bool? IPPortSecurityEnabled;
        public string[] IPSecPermitIPProtocols;
        public string[] IPSecPermitTCPPorts;
        public string[] IPSecPermitUDPPorts;
        public string[] IPSubnet;
        public bool? IPUseZeroBroadcast;
        public string IPXAddress;
        public bool? IPXEnabled;
        public uint[] IPXFrameType;
        public uint? IPXMediaType;
        public string[] IPXNetworkNumber;
        public string IPXVirtualNetNumber;
        public uint? KeepAliveInterval;
        public uint? KeepAliveTime;
        public uint? MTU;
        public uint? NumForwardPackets;
        public bool? PMTUBHDetectEnabled;
        public bool? PMTUDiscoveryEnabled;
        public string SettingID;
        public uint? TcpipNetbiosOptions;
        public uint? TcpMaxConnectRetransmissions;
        public uint? TcpMaxDataRetransmissions;
        public uint? TcpNumConnections;
        public bool? TcpUseRFC1122UrgentPointer;
        public ushort? TcpWindowSize;
        public bool? WINSEnableLMHostsLookup;
        public string WINSHostLookupFile;
        public string WINSPrimaryServer;
        public string WINSScopeID;
        public string WINSSecondaryServer;
    internal sealed class WmiOperatingSystem : WmiClassBase
        public string BootDevice;
        public string BuildType;
        public string CountryCode;
        public string CSDVersion;
        public string CSName;
        public bool? DataExecutionPrevention_Available;
        public bool? DataExecutionPrevention_32BitApplications;
        public bool? DataExecutionPrevention_Drivers;
        public byte? DataExecutionPrevention_SupportPolicy;
        public bool? Debug;
        public bool? Distributed;
        public uint? EncryptionLevel;
        public byte? ForegroundApplicationBoost;
        public ulong? FreePhysicalMemory;
        public ulong? FreeSpaceInPagingFiles;
        public ulong? FreeVirtualMemory;
        public DateTime? LastBootUpTime;
        public DateTime? LocalDateTime;
        public string Locale;
        public uint? MaxNumberOfProcesses;
        public ulong? MaxProcessMemorySize;
        public string[] MUILanguages;
        public uint? NumberOfLicensedUsers;
        public uint? NumberOfProcesses;
        public uint? NumberOfUsers;
        public uint? OperatingSystemSKU;
        public string Organization;
        public string OSArchitecture;
        public uint? OSLanguage;
        public uint? OSProductSuite;
        public ushort? OSType;
        public string OtherTypeDescription;
        public bool? PAEEnabled;
        public bool? PortableOperatingSystem;
        public bool? Primary;
        public uint? ProductType;
        public string RegisteredUser;
        public ushort? ServicePackMajorVersion;
        public ushort? ServicePackMinorVersion;
        public ulong? SizeStoredInPagingFiles;
        public uint? SuiteMask;
        public string SystemDevice;
        public string SystemDirectory;
        public string SystemDrive;
        public ulong? TotalSwapSpaceSize;
        public ulong? TotalVirtualMemorySize;
        public ulong? TotalVisibleMemorySize;
        public string WindowsDirectory;
        public string LanguageName
            get { return GetLanguageName(OSLanguage); }
        public OSProductSuite[] ProductSuites
            get { return MakeProductSuites(OSProductSuite); }
        public OSProductSuite[] Suites
            get { return MakeProductSuites(SuiteMask); }
        #endregion Public Properties
        public string GetLocale()
            return Conversion.GetLocaleName(Locale);
        private static OSProductSuite[] MakeProductSuites(uint? suiteMask)
            if (suiteMask == null)
            var mask = suiteMask.Value;
            var list = new List<OSProductSuite>();
            foreach (OSProductSuite suite in Enum.GetValues<OSProductSuite>())
                if ((mask & (uint)suite) != 0)
                    list.Add(suite);
    internal sealed class WmiPageFileUsage
        public uint? AllocatedBaseSize;
        public uint? CurrentUsage;
        public uint? PeakUsage;
        public bool? TempPageFile;
    internal sealed class WmiProcessor
        public ushort? AddressWidth;
        public ushort? Architecture;
        public string AssetTag;
        public uint? Characteristics;
        public ushort? CpuStatus;
        public uint? CurrentClockSpeed;
        public ushort? CurrentVoltage;
        public ushort? DataWidth;
        public uint? ExtClock;
        public ushort? Family;
        public uint? L2CacheSize;
        public uint? L2CacheSpeed;
        public uint? L3CacheSize;
        public uint? L3CacheSpeed;
        public ushort? Level;
        public ushort? LoadPercentage;
        public uint? MaxClockSpeed;
        public uint? NumberOfCores;
        public uint? NumberOfEnabledCore;
        public string OtherFamilyDescription;
        public string ProcessorId;
        public ushort? ProcessorType;
        public ushort? Revision;
        public string Role;
        public bool? SecondLevelAddressTranslationExtensions;
        public string SocketDesignation;
        public string Stepping;
        public uint? ThreadCount;
        public string UniqueId;
        public ushort? UpgradeMethod;
        public uint? VoltageCaps;
#pragma warning restore 649
    #endregion Intermediate WMI classes
    #region Other Intermediate classes
    internal sealed class RegWinNtCurrentVersion
        public string BuildLabEx;
        public string CurrentVersion;
        public string EditionId;
        public string InstallationType;
        public string ProductId;
        public string RegisteredOrganization;
        public string RegisteredOwner;
        public string SystemRoot;
        public string ReleaseId;
        public int? UBR;
    #endregion Other Intermediate classes
    #region Output components
    #region Classes comprising the output object
    /// Provides information about Device Guard.
    public class DeviceGuard
        /// Array of required security properties.
        public DeviceGuardHardwareSecure[] RequiredSecurityProperties { get; internal set; }
        /// Array of available security properties.
        public DeviceGuardHardwareSecure[] AvailableSecurityProperties { get; internal set; }
        /// Indicates which security services have been configured.
        public DeviceGuardSoftwareSecure[] SecurityServicesConfigured { get; internal set; }
        /// Indicates which security services are running.
        public DeviceGuardSoftwareSecure[] SecurityServicesRunning { get; internal set; }
        /// Indicates the status of the Device Guard Code Integrity policy.
        public DeviceGuardConfigCodeIntegrityStatus? CodeIntegrityPolicyEnforcementStatus { get; internal set; }
        /// Indicates the status of the Device Guard user mode Code Integrity policy.
        public DeviceGuardConfigCodeIntegrityStatus? UserModeCodeIntegrityPolicyEnforcementStatus { get; internal set; }
    /// Describes a Quick-Fix Engineering update.
    public class HotFix
        /// Unique identifier associated with a particular update.
        public string HotFixID
            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Class is instantiated directly from a CIM instance")]
            internal set;
        /// Description of the update.
        public string Description
        /// String containing the date that the update was installed.
        public string InstalledOn
        /// Additional comments that relate to the update.
        public string FixComments
    /// Provides information about a network adapter.
    public class NetworkAdapter
        /// Description of the network adapter.
        public string Description { get; internal set; }
        /// Name of the network connection as it appears in the Network
        /// Connections Control Panel program.
        public string ConnectionID { get; internal set; }
        /// Indicates whether the DHCP server automatically assigns an IP address
        /// to the computer system when establishing a network connection.
        public bool? DHCPEnabled { get; internal set; }
        /// IP Address of the DHCP server.
        public string DHCPServer { get; internal set; }
        /// State of the network adapter connection to the network.
        public NetConnectionStatus ConnectionStatus { get; internal set; }
        /// Array of all of the IP addresses associated with the current network adapter.
        public string[] IPAddresses { get; internal set; }
    /// Describes a processor on the computer.
    public class Processor
        /// Name of the processor.
        public string Name { get; internal set; }
        /// Name of the processor manufacturer.
        public string Manufacturer { get; internal set; }
        /// Description of the processor.
        /// Processor architecture used by the platform.
        public CpuArchitecture? Architecture { get; internal set; }
        /// Address width of the processor.
        public ushort? AddressWidth { get; internal set; }
        /// Data width of the processor.
        public ushort? DataWidth { get; internal set; }
        /// Maximum speed of the processor, in MHz.
        public uint? MaxClockSpeed { get; internal set; }
        /// Current speed of the processor, in MHz.
        public uint? CurrentClockSpeed { get; internal set; }
        /// Number of cores for the current instance of the processor.
        /// A core is a physical processor on the integrated circuit
        public uint? NumberOfCores { get; internal set; }
        /// Number of logical processors for the current instance of the processor.
        /// For processors capable of hyperthreading, this value includes only the
        /// processors which have hyperthreading enabled
        public uint? NumberOfLogicalProcessors { get; internal set; }
        /// Processor information that describes the processor features.
        /// For an x86 class CPU, the field format depends on the processor support
        /// of the CPUID instruction. If the instruction is supported, the property
        /// contains 2 (two) DWORD formatted values. The first is an offset of 08h-0Bh,
        /// which is the EAX value that a CPUID instruction returns with input EAX set
        /// to 1. The second is an offset of 0Ch-0Fh, which is the EDX value that the
        /// instruction returns. Only the first two bytes of the property are significant
        /// and contain the contents of the DX register at CPU reset—all others are set
        /// to 0 (zero), and the contents are in DWORD format
        public string ProcessorID { get; internal set; }
        /// Type of chip socket used on the circuit.
        public string SocketDesignation { get; internal set; }
        /// Primary function of the processor.
        public ProcessorType? ProcessorType { get; internal set; }
        /// Role of the processor.
        public string Role { get; internal set; }
        /// Current status of the processor.
        public string Status { get; internal set; }
        /// Status changes indicate processor usage, but not the physical
        /// condition of the processor.
        public CpuStatus? CpuStatus { get; internal set; }
        /// Availability and status of the processor.
        public CpuAvailability? Availability { get; internal set; }
    /// The ComputerInfo class is output to the PowerShell pipeline.
    public class ComputerInfo
        #region Registry
        /// Windows build lab information, from the Windows Registry.
        public string WindowsBuildLabEx { get; internal set; }
        /// Windows version number, from the Windows Registry.
        public string WindowsCurrentVersion { get; internal set; }
        /// Windows edition, from the Windows Registry.
        public string WindowsEditionId { get; internal set; }
        /// Windows installation type, from the Windows Registry.
        public string WindowsInstallationType { get; internal set; }
        /// The data Windows was installed, from the Windows Registry.
        public DateTime? WindowsInstallDateFromRegistry { get; internal set; }
        /// The Windows product ID, from the Windows Registry.
        public string WindowsProductId { get; internal set; }
        /// The Windows product name, from the Windows Registry.
        public string WindowsProductName { get; internal set; }
        /// Name of the organization that this installation of Windows is registered to, from the Windows Registry.
        public string WindowsRegisteredOrganization { get; internal set; }
        /// Name of the registered owner of this installation of Windows, from the Windows Registry.
        public string WindowsRegisteredOwner { get; internal set; }
        /// Path to the operating system's root directory, from the Windows Registry.
        public string WindowsSystemRoot { get; internal set; }
        /// The Windows ReleaseId, from the Windows Registry.
        public string WindowsVersion { get; internal set; }
        /// The Windows Update Build Revision (UBR), from the Windows Registry.
        public int? WindowsUBR { get; internal set; }
        #endregion Registry
        #region BIOS
        /// Array of BIOS characteristics supported by the system as defined by
        /// the System Management BIOS Reference Specification.
        public ushort[] BiosCharacteristics { get; internal set; }
        /// Array of the complete system BIOS information. In many computers
        /// there can be several version strings that are stored in the registry
        /// and represent the system BIOS information.
        public string[] BiosBIOSVersion { get; internal set; }
        /// Internal identifier for this compilation of the BIOS firmware.
        public string BiosBuildNumber { get; internal set; }
        /// Short description of the BIOS.
        public string BiosCaption { get; internal set; }
        /// Code set used by the BIOS.
        public string BiosCodeSet { get; internal set; }
        /// Name of the current BIOS language.
        public string BiosCurrentLanguage { get; internal set; }
        /// Description of the BIOS.
        public string BiosDescription { get; internal set; }
        /// Major version of the embedded controller firmware.
        public short? BiosEmbeddedControllerMajorVersion { get; internal set; }
        /// Minor version of the embedded controller firmware.
        public short? BiosEmbeddedControllerMinorVersion { get; internal set; }
        /// Firmware type of the local computer.
        /// This is acquired via the GetFirmwareType Windows API function
        public FirmwareType? BiosFirmwareType { get; internal set; }
        /// Manufacturer's identifier for this software element.
        /// Often this will be a stock keeping unit (SKU) or a part number.
        public string BiosIdentificationCode { get; internal set; }
        /// Number of languages available for installation on this system.
        /// Language may determine properties such as the need for Unicode and bidirectional text.
        public ushort? BiosInstallableLanguages { get; internal set; }
        /// Date and time the object was installed.
        // TODO: do we want this? On my system this is null
        public DateTime? BiosInstallDate { get; internal set; }
        /// Language edition of the BIOS firmware.
        /// The language codes defined in ISO 639 should be used.
        /// Where the software element represents a multilingual or international
        /// version of a product, the string "multilingual" should be used.
        public string BiosLanguageEdition { get; internal set; }
        /// Array of names of available BIOS-installable languages.
        public string[] BiosListOfLanguages { get; internal set; }
        /// Manufacturer of the BIOS.
        public string BiosManufacturer { get; internal set; }
        /// Name used to identify the BIOS.
        public string BiosName { get; internal set; }
        /// Records the manufacturer and operating system type for the BIOS when
        /// the BiosTargetOperatingSystem property has a value of 1 (Other).
        /// When TargetOperatingSystem has a value of 1, BiosOtherTargetOS must
        /// have a nonnull value. For all other values of BiosTargetOperatingSystem,
        /// BiosOtherTargetOS is NULL.
        public string BiosOtherTargetOS { get; internal set; }
        /// If true, this is the primary BIOS of the computer system.
        public bool? BiosPrimaryBIOS { get; internal set; }
        /// Release date of the Windows BIOS.
        public DateTime? BiosReleaseDate { get; internal set; }
        /// Assigned serial number of the BIOS.
        public string BiosSerialNumber { get; internal set; }
        /// BIOS version as reported by SMBIOS.
        public string BiosSMBIOSBIOSVersion { get; internal set; }
        /// SMBIOS major version number. This property is null if SMBIOS is not found.
        public ushort? BiosSMBIOSMajorVersion { get; internal set; }
        /// SMBIOS minor version number. This property is null if SMBIOS is not found.
        public ushort? BiosSMBIOSMinorVersion { get; internal set; }
        /// If true, the SMBIOS is available on this computer system.
        public bool? BiosSMBIOSPresent { get; internal set; }
        /// State of a BIOS software element.
        public SoftwareElementState? BiosSoftwareElementState { get; internal set; }
        /// Status of the BIOS.
        public string BiosStatus { get; internal set; }
        /// Major elease of the System BIOS.
        public ushort? BiosSystemBiosMajorVersion { get; internal set; }
        /// Minor release of the System BIOS.
        public ushort? BiosSystemBiosMinorVersion { get; internal set; }
        /// Target operating system.
        public ushort? BiosTargetOperatingSystem { get; internal set; }
        /// Version of the BIOS.
        /// This string is created by the BIOS manufacturer.
        public string BiosVersion { get; internal set; }
        #endregion BIOS
        #region Computer System
        /// System hardware security settings for administrator password status.
        // public AdminPasswordStatus? CsAdminPasswordStatus { get; internal set; }
        public HardwareSecurity? CsAdminPasswordStatus { get; internal set; }
        /// If true, the system manages the page file.
        public bool? CsAutomaticManagedPagefile { get; internal set; }
        /// If True, the automatic reset boot option is enabled.
        public bool? CsAutomaticResetBootOption { get; internal set; }
        /// If True, the automatic reset is enabled.
        public bool? CsAutomaticResetCapability { get; internal set; }
        /// Boot option limit is ON. Identifies the system action when the
        /// CsResetLimit value is reached.
        public BootOptionAction? CsBootOptionOnLimit { get; internal set; }
        /// Type of reboot action after the time on the watchdog timer is elapsed.
        public BootOptionAction? CsBootOptionOnWatchDog { get; internal set; }
        /// If true, indicates whether a boot ROM is supported.
        public bool? CsBootROMSupported { get; internal set; }
        /// Status and Additional Data fields that identify the boot status.
        public ushort[] CsBootStatus { get; internal set; }
        /// System is started. Fail-safe boot bypasses the user startup files—also called SafeBoot.
        public string CsBootupState { get; internal set; }
        /// The name of this computer.
        public string CsCaption { get; internal set; }  // TODO: remove this? Same as CsName???
        /// Boot up state of the chassis.
        // public ChassisBootupState? CsChassisBootupState { get; internal set; }
        public SystemElementState? CsChassisBootupState { get; internal set; }
        /// The chassis or enclosure SKU number as a string.
        public string CsChassisSKUNumber { get; internal set; }
        /// Amount of time the unitary computer system is offset from Coordinated
        /// Universal Time (UTC).
        public short? CsCurrentTimeZone { get; internal set; }
        /// If True, the daylight savings mode is ON.
        public bool? CsDaylightInEffect { get; internal set; }
        /// Description of the computer system.
        public string CsDescription { get; internal set; }
        /// Name of local computer according to the domain name server.
        public string CsDNSHostName { get; internal set; }
        /// Name of the domain to which a computer belongs.
        /// If the computer is not part of a domain, then the name of the workgroup is returned
        public string CsDomain { get; internal set; }
        /// Role of a computer in an assigned domain workgroup. A domain workgroup
        /// is a collection of computers on the same network. For example,
        /// a DomainRole property may show that a computer is a member workstation.
        public DomainRole? CsDomainRole { get; internal set; }
        /// Enables daylight savings time on a computer. A value of True indicates
        /// that the system time changes to an hour ahead or behind when DST starts
        /// or ends. A value of False indicates that the system time does not change
        /// to an hour ahead or behind when DST starts or ends. A value of NULL
        /// indicates that the DST status is unknown on a system.
        public bool? CsEnableDaylightSavingsTime { get; internal set; }
        /// Hardware security setting for the reset button on a computer.
        // public FrontPanelResetStatus? CsFrontPanelResetStatus { get; internal set; }
        public HardwareSecurity? CsFrontPanelResetStatus { get; internal set; }
        /// If True, a hypervisor is present.
        public bool? CsHypervisorPresent { get; internal set; }
        /// If True, an infrared port exists on a computer system.
        public bool? CsInfraredSupported { get; internal set; }
        /// Data required to find the initial load device or boot service to request that the operating system start up.
        public string CsInitialLoadInfo { get; internal set; }
        /// Object is installed. An object does not need a value to indicate that it is installed.
        public DateTime? CsInstallDate { get; internal set; }
        /// System hardware security setting for Keyboard Password Status.
        // public KeyboardPasswordStatus? CsKeyboardPasswordStatus { get; internal set; }
        public HardwareSecurity? CsKeyboardPasswordStatus { get; internal set; }
        /// Array entry of the CsInitialLoadInfo property that contains the data
        /// to start the loaded operating system.
        public string CsLastLoadInfo { get; internal set; }
        /// Name of the computer manufacturer.
        public string CsManufacturer { get; internal set; }
        /// Product name that a manufacturer gives to a computer.
        public string CsModel { get; internal set; }
        /// Key of a CIM_System instance in an enterprise environment.
        public string CsName { get; internal set; }
        /// An array of <see cref="NetworkAdapter"/> objects describing any
        /// network adapters on the system.
        public NetworkAdapter[] CsNetworkAdapters { get; internal set; }
        /// If True, the network Server Mode is enabled.
        public bool? CsNetworkServerModeEnabled { get; internal set; }
        /// Number of logical processors available on the computer.
        public uint? CsNumberOfLogicalProcessors { get; internal set; }
        /// Number of physical processors currently available on a system.
        /// This is the number of enabled processors for a system, which
        /// does not include the disabled processors. If a computer system
        /// has two physical processors each containing two logical processors,
        /// then the value of CsNumberOfProcessors is 2 and CsNumberOfLogicalProcessors
        /// is 4. The processors may be multicore or they may be hyperthreading processors
        public uint? CsNumberOfProcessors { get; internal set; }
        /// Array of <see cref="Processor"/> objects describing each processor on the system.
        public Processor[] CsProcessors { get; internal set; }
        /// Array of free-form strings that an OEM defines.
        /// For example, an OEM defines the part numbers for system reference
        /// documents, manufacturer contact information, and so on.
        public string[] CsOEMStringArray { get; internal set; }
        /// If True, the computer is part of a domain.
        /// If the value is NULL, the computer is not in a domain or the status is unknown.
        public bool? CsPartOfDomain { get; internal set; }
        /// Time delay before a reboot is initiated, in milliseconds.
        /// It is used after a system power cycle, local or remote system reset,
        /// and automatic system reset. A value of –1 (minus one) indicates that
        /// the pause value is unknown.
        public long? CsPauseAfterReset { get; internal set; }
        /// Type of the computer in use, such as laptop, desktop, or tablet.
        public PCSystemType? CsPCSystemType { get; internal set; }
        public PCSystemTypeEx? CsPCSystemTypeEx { get; internal set; }
        /// Array of the specific power-related capabilities of a logical device.
        public PowerManagementCapabilities[] CsPowerManagementCapabilities { get; internal set; }
        /// If True, device can be power-managed, for example, a device can be
        /// put into suspend mode, and so on.
        /// This property does not indicate that power management features are
        /// enabled currently, but it does indicate that the logical device is
        /// capable of power management
        public bool? CsPowerManagementSupported { get; internal set; }
        /// System hardware security setting for Power-On Password Status.
        // public PowerOnPasswordStatus? CsPowerOnPasswordStatus { get; internal set; }
        public HardwareSecurity? CsPowerOnPasswordStatus { get; internal set; }
        /// Current power state of a computer and its associated operating system.
        public PowerState? CsPowerState { get; internal set; }
        /// State of the power supply or supplies when last booted.
        // public PowerSupplyState? CsPowerSupplyState { get; internal set; }
        public SystemElementState? CsPowerSupplyState { get; internal set; }
        /// Contact information for the primary system owner.
        /// For example, phone number, email address, and so on.
        public string CsPrimaryOwnerContact { get; internal set; }
        /// Name of the primary system owner.
        public string CsPrimaryOwnerName { get; internal set; }
        /// Indicates if the computer system can be reset.
        public ResetCapability? CsResetCapability { get; internal set; }
        /// Number of automatic resets since the last reset.
        /// A value of –1 (minus one) indicates that the count is unknown.
        public short? CsResetCount { get; internal set; }
        /// Number of consecutive times a system reset is attempted.
        /// A value of –1 (minus one) indicates that the limit is unknown.
        public short? CsResetLimit { get; internal set; }
        /// Array that specifies the roles of a system in the information
        /// technology environment.
        public string[] CsRoles { get; internal set; }
        /// Statis pf the computer system.
        public string CsStatus { get; internal set; }
        /// Array of the support contact information for the Windows operating system.
        public string[] CsSupportContactDescription { get; internal set; }
        /// The family to which a particular computer belongs.
        /// A family refers to a set of computers that are similar but not
        /// identical from a hardware or software point of view.
        public string CsSystemFamily { get; internal set; }
        /// Identifies a particular computer configuration for sale.
        /// It is sometimes also called a product ID or purchase order number.
        public string CsSystemSKUNumber { get; internal set; }
        /// System running on the Windows-based computer.
        public string CsSystemType { get; internal set; }
        /// Thermal state of the system when last booted.
        // public ThermalState? CsThermalState { get; internal set; }
        public SystemElementState? CsThermalState { get; internal set; }
        /// Total size of physical memory.
        /// Be aware that, under some circumstances, this property may not
        /// return an accurate value for the physical memory. For example,
        /// it is not accurate if the BIOS is using some of the physical memory
        public ulong? CsTotalPhysicalMemory { get; internal set; }
        /// Size of physically installed memory, as reported by the Windows API
        /// function GetPhysicallyInstalledSystemMemory.
        public ulong? CsPhysicallyInstalledMemory { get; internal set; }
        /// Name of a user that is logged on currently.
        /// In a terminal services session, CsUserName is the name of the user
        /// that is logged on to the console—not the user logged on during the
        /// terminal service session
        public string CsUserName { get; internal set; }
        /// Event that causes the system to power up.
        public WakeUpType? CsWakeUpType { get; internal set; }
        /// Name of the workgroup for this computer.
        public string CsWorkgroup { get; internal set; }
        #endregion Computer System
        #region Operating System
        /// Name of the operating system.
        public string OsName { get; internal set; }
        /// Type of operating system.
        public OSType? OsType { get; internal set; }
        /// SKU number for the operating system.
        public OperatingSystemSKU? OsOperatingSystemSKU { get; internal set; }
        /// Version number of the operating system.
        public string OsVersion { get; internal set; }
        /// String that indicates the latest service pack installed on a computer.
        /// If no service pack is installed, the string is NULL.
        public string OsCSDVersion { get; internal set; }
        /// Build number of the operating system.
        public string OsBuildNumber { get; internal set; }
        /// Array of <see cref="HotFix"/> objects containing information about
        /// any Quick-Fix Engineering patches (Hot Fixes) applied to the operating
        /// system.
        public HotFix[] OsHotFixes { get; internal set; }
        /// Name of the disk drive from which the Windows operating system starts.
        public string OsBootDevice { get; internal set; }
        /// Physical disk partition on which the operating system is installed.
        public string OsSystemDevice { get; internal set; }
        /// System directory of the operating system.
        public string OsSystemDirectory { get; internal set; }
        /// Letter of the disk drive on which the operating system resides.
        public string OsSystemDrive { get; internal set; }
        /// Windows directory of the operating system.
        public string OsWindowsDirectory { get; internal set; }
        /// Code for the country/region that an operating system uses.
        /// Values are based on international phone dialing prefixes—also
        /// referred to as IBM country/region codes
        public string OsCountryCode { get; internal set; }
        /// Number, in minutes, an operating system is offset from Greenwich
        /// mean time (GMT). The number is positive, negative, or zero.
        public short? OsCurrentTimeZone { get; internal set; }
        /// Language identifier used by the operating system.
        /// A language identifier is a standard international numeric abbreviation
        /// for a country/region. Each language has a unique language identifier (LANGID),
        /// a 16-bit value that consists of a primary language identifier and a secondary
        /// language identifier
        public string OsLocaleID { get; internal set; }   // From Win32_OperatingSystem.Locale
        /// The culture name, such as "en-US", derived from the <see cref="OsLocaleID"/> property.
        public string OsLocale { get; internal set; }
        /// Operating system version of the local date and time-of-day.
        public DateTime? OsLocalDateTime { get; internal set; }
        /// Date and time the operating system was last restarted.
        public DateTime? OsLastBootUpTime { get; internal set; }
        /// The interval between the time the operating system was last
        /// restarted and the current time.
        public TimeSpan? OsUptime { get; internal set; }
        /// Type of build used for the operating system.
        public string OsBuildType { get; internal set; }
        /// Code page value the operating system uses.
        public string OsCodeSet { get; internal set; }
        /// If true, then the data execution prevention hardware feature is available.
        public bool? OsDataExecutionPreventionAvailable { get; internal set; }
        /// When the data execution prevention hardware feature is available,
        /// this property indicates that the feature is set to work for 32-bit
        /// applications if true.
        public bool? OsDataExecutionPrevention32BitApplications { get; internal set; }
        /// this property indicates that the feature is set to work for drivers
        /// if true.
        public bool? OsDataExecutionPreventionDrivers { get; internal set; }
        /// Indicates which Data Execution Prevention (DEP) setting is applied.
        /// The DEP setting specifies the extent to which DEP applies to 32-bit
        /// applications on the system. DEP is always applied to the Windows kernel.
        public DataExecutionPreventionSupportPolicy? OsDataExecutionPreventionSupportPolicy { get; internal set; }
        /// If true, the operating system is a checked (debug) build.
        public bool? OsDebug { get; internal set; }
        /// If True, the operating system is distributed across several computer
        /// system nodes. If so, these nodes should be grouped as a cluster.
        public bool? OsDistributed { get; internal set; }
        /// Encryption level for secure transactions: 40-bit, 128-bit, or n-bit.
        public OSEncryptionLevel? OsEncryptionLevel { get; internal set; }
        /// Increased priority given to the foreground application.
        public ForegroundApplicationBoost? OsForegroundApplicationBoost { get; internal set; }
        /// Total amount, in kilobytes, of physical memory available to the
        /// operating system.
        /// This value does not necessarily indicate the true amount of
        /// physical memory, but what is reported to the operating system
        /// as available to it.
        public ulong? OsTotalVisibleMemorySize { get; internal set; }
        /// Number, in kilobytes, of physical memory currently unused and available.
        public ulong? OsFreePhysicalMemory { get; internal set; }
        /// Number, in kilobytes, of virtual memory.
        public ulong? OsTotalVirtualMemorySize { get; internal set; }
        /// Number, in kilobytes, of virtual memory currently unused and available.
        public ulong? OsFreeVirtualMemory { get; internal set; }
        /// Number, in kilobytes, of virtual memory currently in use.
        public ulong? OsInUseVirtualMemory { get; internal set; }
        /// Total swap space in kilobytes.
        /// This value may be NULL (unspecified) if the swap space is not
        /// distinguished from page files. However, some operating systems
        /// distinguish these concepts. For example, in UNIX, whole processes
        /// can be swapped out when the free page list falls and remains below
        /// a specified amount
        public ulong? OsTotalSwapSpaceSize { get; internal set; }
        /// Total number of kilobytes that can be stored in the operating system
        /// paging files—0 (zero) indicates that there are no paging files.
        /// Be aware that this number does not represent the actual physical
        /// size of the paging file on disk.
        public ulong? OsSizeStoredInPagingFiles { get; internal set; }
        /// Number, in kilobytes, that can be mapped into the operating system
        /// paging files without causing any other pages to be swapped out.
        public ulong? OsFreeSpaceInPagingFiles { get; internal set; }
        /// Array of file paths to the operating system's paging files.
        public string[] OsPagingFiles { get; internal set; }
        /// Version of the operating system's Hardware Abstraction Layer (HAL)
        public string OsHardwareAbstractionLayer { get; internal set; }
        /// Indicates the install date.
        public DateTime? OsInstallDate { get; internal set; }
        /// Name of the operating system manufacturer.
        /// For Windows-based systems, this value is "Microsoft Corporation"
        public string OsManufacturer { get; internal set; }
        /// Maximum number of process contexts the operating system can support.
        public uint? OsMaxNumberOfProcesses { get; internal set; }
        /// Maximum number, in kilobytes, of memory that can be allocated to a process.
        public ulong? OsMaxProcessMemorySize { get; internal set; }
        /// Array of Multilingual User Interface Pack (MUI Pack) languages installed
        /// on the computer.
        public string[] OsMuiLanguages { get; internal set; }
        /// Number of user licenses for the operating system.
        public uint? OsNumberOfLicensedUsers { get; internal set; }
        /// Number of process contexts currently loaded or running on the operating system.
        public uint? OsNumberOfProcesses { get; internal set; }
        /// Number of user sessions for which the operating system is storing
        /// state information currently.
        public uint? OsNumberOfUsers { get; internal set; }
        /// Company name for the registered user of the operating system.
        public string OsOrganization { get; internal set; }
        /// Architecture of the operating system, as opposed to the processor.
        public string OsArchitecture { get; internal set; }
        /// Language version of the operating system installed.
        public string OsLanguage { get; internal set; }
        /// Array of <see cref="OSProductSuite"/> objects indicating installed
        /// and licensed product additions to the operating system.
        public OSProductSuite[] OsProductSuites { get; internal set; }
        /// Additional description for the current operating system version.
        public string OsOtherTypeDescription { get; internal set; }
        /// If True, the physical address extensions (PAE) are enabled by the
        /// operating system running on Intel processors.
        public bool? OsPAEEnabled { get; internal set; }
        /// Specifies whether the operating system booted from an external USB device.
        /// If true, the operating system has detected it is booting on a supported
        /// locally connected storage device.
        public bool? OsPortableOperatingSystem { get; internal set; }
        /// Specifies whether this is the primary operating system.
        public bool? OsPrimary { get; internal set; }
        /// Additional system information.
        public ProductType? OsProductType { get; internal set; }
        /// Name of the registered user of the operating system.
        public string OsRegisteredUser { get; internal set; }
        /// Operating system product serial identification number.
        public string OsSerialNumber { get; internal set; }
        /// Major version of the service pack installed on the computer system.
        public ushort? OsServicePackMajorVersion { get; internal set; }
        /// Minor version of the service pack installed on the computer system.
        public ushort? OsServicePackMinorVersion { get; internal set; }
        /// Current status.
        public string OsStatus { get; internal set; }
        /// Product suites available on the operating system.
        public OSProductSuite[] OsSuites { get; internal set; }
        /// Server level of the operating system, if the operating system is a server.
        public ServerLevel? OsServerLevel { get; internal set; }
        #endregion Operating System
        #region Misc Info
        /// Layout of the (first) keyboard attached to the system.
        public string KeyboardLayout { get; internal set; }
        /// Name of the system's current time zone.
        public string TimeZone { get; internal set; }
        /// Path to the system's logon server.
        public string LogonServer { get; internal set; }
        /// Power platform role.
        public PowerPlatformRole? PowerPlatformRole { get; internal set; }
        /// If true, a HyperVisor was detected.
        public bool? HyperVisorPresent { get; internal set; }
        /// If a HyperVisor is not present, indicates the state of the
        /// requirement that the Data Execution Prevention feature is available.
        public bool? HyperVRequirementDataExecutionPreventionAvailable { get; internal set; }
        /// requirement that the processor supports address translation
        /// extensions used for virtualization.
        public bool? HyperVRequirementSecondLevelAddressTranslation { get; internal set; }
        /// requirement that the firmware has enabled virtualization
        /// extensions.
        public bool? HyperVRequirementVirtualizationFirmwareEnabled { get; internal set; }
        /// requirement that the processor supports Intel or AMD Virtual
        /// Machine Monitor extensions.
        public bool? HyperVRequirementVMMonitorModeExtensions { get; internal set; }
        /// Indicates the status of the Device Guard features.
        public DeviceGuardSmartStatus? DeviceGuardSmartStatus { get; internal set; }
        /// Required Device Guard security properties.
        public DeviceGuardHardwareSecure[] DeviceGuardRequiredSecurityProperties { get; internal set; }
        /// Available Device Guard security properties.
        public DeviceGuardHardwareSecure[] DeviceGuardAvailableSecurityProperties { get; internal set; }
        /// Configured Device Guard security services.
        public DeviceGuardSoftwareSecure[] DeviceGuardSecurityServicesConfigured { get; internal set; }
        /// Running Device Guard security services.
        public DeviceGuardSoftwareSecure[] DeviceGuardSecurityServicesRunning { get; internal set; }
        /// Status of the Device Guard Code Integrity policy enforcement.
        public DeviceGuardConfigCodeIntegrityStatus? DeviceGuardCodeIntegrityPolicyEnforcementStatus { get; internal set; }
        /// Status of the Device Guard user mode Code Integrity policy enforcement.
        public DeviceGuardConfigCodeIntegrityStatus? DeviceGuardUserModeCodeIntegrityPolicyEnforcementStatus { get; internal set; }
        #endregion Misc Info
    #endregion Classes comprising the output object
    #region Enums used in the output objects
    public enum AdminPasswordStatus
        /// Feature is disabled.
        Disabled = 0,
        /// Feature is Enabled.
        Enabled = 1,
        /// Feature is not implemented.
        NotImplemented = 2,
        /// Status is unknown.
        Unknown = 3
    /// Actions related to the BootOptionOn* properties of the Win32_ComputerSystem
    /// CIM class.
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "The underlying MOF definition does not contain a zero value. The converter method will handle it appropriately.")]
    public enum BootOptionAction
        // <summary>
        // This value is reserved
        // </summary>
        // Reserved = 0,
        /// Boot into operating system.
        OperatingSystem = 1,
        /// Boot into system utilities.
        SystemUtilities = 2,
        /// Do not reboot.
        DoNotReboot = 3
    /// Indicates the state of a system element.
    public enum SystemElementState
        /// The element state is something other than those in this Enum.
        Other = 1,
        /// The element state is unknown.
        Unknown = 2,
        /// The element is in Safe state.
        Safe = 3,
        /// The element is in Warning state.
        Warning = 4,
        /// The element is in Critical state.
        Critical = 5,
        /// The element is in Non-Recoverable state.
        NonRecoverable = 6
    /// Specifies the processor architecture.
    public enum CpuArchitecture
        /// Architecture is Intel x86.
        x86 = 0,
        /// Architecture is MIPS.
        MIPs = 1,
        /// Architecture is DEC Alpha.
        Alpha = 2,
        /// Architecture is Motorola PowerPC.
        PowerPC = 3,
        /// Architecture is ARM.
        ARM = 5,
        /// Architecture is Itanium-based 64-bit.
        ia64 = 6,
        /// Architecture is Intel 64-bit.
        x64 = 9
    /// Specifies a CPU's availability and status.
    public enum CpuAvailability
        /// A state other than those specified in CpuAvailability.
        /// Availability status is unknown.
        /// The device is running or at full power.
        RunningOrFullPower = 3,
        /// Device is in a Warning state.
        /// Availability status is In Test.
        InTest = 5,
        /// Status is not applicable to this device.
        NotApplicable = 6,
        /// The device is powered off.
        PowerOff = 7,
        /// Availability status is Offline.
        OffLine = 8,
        /// Availability status is Off-Duty.
        OffDuty = 9,
        /// Availability status is Degraded.
        Degraded = 10,
        /// Availability status is Not Installed.
        NotInstalled = 11,
        /// Availability status is Install Error.
        InstallError = 12,
        /// The device is known to be in a power save state, but its exact status is unknown.
        PowerSaveUnknown = 13,
        /// The device is in a power save state, but is still functioning,
        /// and may exhibit decreased performance.
        PowerSaveLowPowerMode = 14,
        /// The device is not functioning, but can be brought to full power quickly.
        PowerSaveStandby = 15,
        /// The device is in a power-cycle state.
        PowerCycle = 16,
        /// The device is in a warning state, though also in a power save state.
        PowerSaveWarning = 17,
        /// The device is paused.
        Paused = 18,
        /// The device is not ready.
        NotReady = 19,
        /// The device is not configured.
        NotConfigured = 20,
        /// The device is quiet.
        Quiesced = 21
    /// Specifies that current status of the processor.
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "The underlying MOF definition is not a bit field.")]
    public enum CpuStatus
        /// CPU status is Unknown.
        Unknown = 0,
        /// CPU status is Enabled.
        /// CPU status is Disabled by User via BIOS Setup.
        DisabledByUser = 2,
        /// CPU status is Disabled by BIOS.
        DisabledByBIOS = 3,
        /// CPU is Idle.
        Idle = 4,
        // Reserved_5 = 5,
        // Reserved_6 = 6,
        /// CPU is in another state.
        Other = 7
    /// Data Execution Prevention (DEP) settings.
    public enum DataExecutionPreventionSupportPolicy
        // Unknown     = -1,
        /// DEP is turned off for all 32-bit applications on the computer with no exceptions.
        AlwaysOff = 0,
        /// DEP is enabled for all 32-bit applications on the computer.
        AlwaysOn = 1,
        /// DEP is enabled for a limited number of binaries, the kernel, and all
        /// Windows-based services. However, it is off by default for all 32-bit
        /// applications. A user or administrator must explicitly choose either
        /// the Always On or the Opt Out setting before DEP can be applied to
        /// 32-bit applications.
        OptIn = 2,
        /// DEP is enabled by default for all 32-bit applications. A user or
        /// administrator can explicitly remove support for a 32-bit
        /// application by adding the application to an exceptions list.
        OptOut = 3
    /// Status of the Device Guard feature.
    public enum DeviceGuardSmartStatus
        /// Device Guard is off.
        Off = 0,
        /// Device Guard is Configured.
        Configured = 1,
        /// Device Guard is Running.
        Running = 2
    /// Configuration status of the Device Guard Code Integrity.
    public enum DeviceGuardConfigCodeIntegrityStatus
        /// Code Integrity is off.
        /// Code Integrity uses Audit mode.
        AuditMode = 1,
        /// Code Integrity uses Enforcement mode.
        EnforcementMode = 2
    /// Device Guard hardware security properties.
    public enum DeviceGuardHardwareSecure
        /// Base Virtualization Support.
        BaseVirtualizationSupport = 1,
        /// Secure Boot.
        SecureBoot = 2,
        /// DMA Protection.
        DMAProtection = 3,
        /// Secure Memory Overwrite.
        SecureMemoryOverwrite = 4,
        /// UEFI Code Readonly.
        UEFICodeReadonly = 5,
        /// SMM Security Mitigations 1.0.
        SMMSecurityMitigations = 6,
        /// Mode Based Execution Control.
        ModeBasedExecutionControl = 7
    /// Device Guard software security properties.
    public enum DeviceGuardSoftwareSecure
        /// Credential Guard.
        CredentialGuard = 1,
        /// Hypervisor enforced Code Integrity.
        HypervisorEnforcedCodeIntegrity = 2
    /// Role of a computer in an assigned domain workgroup.
    public enum DomainRole
        /// Standalone Workstation.
        StandaloneWorkstation = 0,
        /// Member Workstation.
        MemberWorkstation = 1,
        /// Standalone Server.
        StandaloneServer = 2,
        /// Member Server.
        MemberServer = 3,
        /// Backup Domain Controller.
        BackupDomainController = 4,
        /// Primary Domain Controller.
        PrimaryDomainController = 5
    /// Specifies a firmware type.
    public enum FirmwareType
        /// The firmware type is unknown.
        /// The computer booted in legacy BIOS mode.
        Bios = 1,
        /// The computer booted in UEFI mode.
        Uefi = 2,
        /// Not implemented.
        Max = 3
    /// Increase in priority given to the foreground application.
    public enum ForegroundApplicationBoost
        /// The system boosts the quantum length by 6.
        None = 0,
        /// The system boosts the quantum length by 12.
        Minimum = 1,
        /// The system boosts the quantum length by 18.
        Maximum = 2
    /// Hardware security settings for the reset button on a computer.
    public enum FrontPanelResetStatus
        /// Reset button is disabled.
        /// Reset button is enabled.
        /// Hardware security settings are not implement.
        /// Unknown security setting.
    /// Indicates a hardware security setting.
    public enum HardwareSecurity
        /// Hardware security is disabled.
        /// Hardware security is enabled.
        /// Hardware security is not implemented.
        /// Hardware security setting is unknown.
    public enum NetConnectionStatus
        /// Adapter is disconnected.
        Disconnected = 0,
        /// Adapter is connecting.
        Connecting = 1,
        /// Adapter is connected.
        Connected = 2,
        /// Adapter is disconnecting.
        Disconnecting = 3,
        /// Adapter hardware is not present.
        HardwareNotPresent = 4,
        /// Adapter hardware is disabled.
        HardwareDisabled = 5,
        /// Adapter has a hardware malfunction.
        HardwareMalfunction = 6,
        /// Media is disconnected.
        MediaDisconnected = 7,
        /// Adapter is authenticating.
        Authenticating = 8,
        /// Authentication has succeeded.
        AuthenticationSucceeded = 9,
        /// Authentication has failed.
        AuthenticationFailed = 10,
        /// Address is invalid.
        InvalidAddress = 11,
        /// Credentials are required.
        CredentialsRequired = 12,
        /// Other unspecified state.
        Other = 13
    public enum OSEncryptionLevel
        /// 40-bit encryption.
        Encrypt40Bits = 0,
        /// 128-bit encryption.
        Encrypt128Bits = 1,
        /// N-bit encryption.
        EncryptNBits = 2
    /// Indicates installed and licensed system product additions to the operating system.
    [FlagsAttribute]
    public enum OSProductSuite
        /// Microsoft Small Business Server was once installed, but may have
        /// been upgraded to another version of Windows.
        SmallBusinessServer = 0x0001,
        /// Windows Server 2008 Enterprise is installed.
        Server2008Enterprise = 0x0002,
        /// Windows BackOffice components are installed.
        BackOfficeComponents = 0x0004,
        /// Communication Server is installed.
        CommunicationsServer = 0x0008,
        /// Terminal Services is installed.
        TerminalServices = 0x0010,
        /// Microsoft Small Business Server is installed with the restrictive
        /// client license.
        SmallBusinessServerRestricted = 0x0020,
        /// Windows Embedded is installed.
        WindowsEmbedded = 0x0040,
        /// A Datacenter edition is installed.
        DatacenterEdition = 0x0080,
        /// Terminal Services is installed, but only one interactive session is supported.
        TerminalServicesSingleSession = 0x0100,
        /// Windows Home Edition is installed.
        HomeEdition = 0x0200,
        /// Web Server Edition is installed.
        WebServerEdition = 0x0400,
        /// Storage Server Edition is installed.
        StorageServerEdition = 0x2000,
        /// Compute Cluster Edition is installed.
        ComputeClusterEdition = 0x4000
    /// Indicates the operating system Stock Keeping Unit (SKU)
    public enum OperatingSystemSKU
        /// The SKU is undefined.
        Undefined = 0,
        /// SKU is Ultimate Edition.
        UltimateEdition = 1,
        /// SKU is Home Basic Edition.
        HomeBasicEdition = 2,
        /// SKU is Home Premium Edition.
        HomePremiumEdition = 3,
        /// SKU is Enterprise Edition.
        EnterpriseEdition = 4,
        /// SKU is Home Basic N Edition.
        HomeBasicNEdition = 5,
        /// SKU is Business Edition.
        BusinessEdition = 6,
        /// SKU is Standard Server Edition.
        StandardServerEdition = 7,
        /// SKU is Datacenter Server Edition.
        DatacenterServerEdition = 8,
        /// SKU is Small Business Server Edition.
        SmallBusinessServerEdition = 9,
        /// SKU is Enterprise Server Edition.
        EnterpriseServerEdition = 10,
        /// SKU is Starter Edition.
        StarterEdition = 11,
        /// SKU is Datacenter Server Core Edition.
        DatacenterServerCoreEdition = 12,
        /// SKU is Standard Server Core Edition.
        StandardServerCoreEdition = 13,
        /// SKU is Enterprise Server Core Edition.
        EnterpriseServerCoreEdition = 14,
        /// SKU is Enterprise Server IA64 Edition.
        EnterpriseServerIA64Edition = 15,
        /// SKU is Business N Edition.
        BusinessNEdition = 16,
        /// SKU is Web Server Edition.
        WebServerEdition = 17,
        /// SKU is Cluster Server Edition.
        ClusterServerEdition = 18,
        /// SKU is Home Server Edition.
        HomeServerEdition = 19,
        /// SKU is Storage Express Server Edition.
        StorageExpressServerEdition = 20,
        /// SKU is Storage Standard Server Edition.
        StorageStandardServerEdition = 21,
        /// SKU is Storage Workgroup Server Edition.
        StorageWorkgroupServerEdition = 22,
        /// SKU is Storage Enterprise Server Edition.
        StorageEnterpriseServerEdition = 23,
        /// SKU is Server For Small Business Edition.
        ServerForSmallBusinessEdition = 24,
        /// SKU is Small Business Server Premium Edition.
        SmallBusinessServerPremiumEdition = 25,
        /// SKU is to be determined.
        TBD = 26,
        /// SKU is Windows Enterprise.
        WindowsEnterprise = 27,
        /// SKU is Windows Ultimate.
        WindowsUltimate = 28,
        /// SKU is Web Server (core installation)
        WebServerCore = 29,
        /// SKU is Server Foundation.
        ServerFoundation = 33,
        /// SKU is Windows Home Server.
        WindowsHomeServer = 34,
        /// SKU is Windows Server Standard without Hyper-V.
        WindowsServerStandardNoHyperVFull = 36,
        /// SKU is Windows Server Datacenter without Hyper-V (full installation)
        WindowsServerDatacenterNoHyperVFull = 37,
        /// SKU is Windows Server Enterprise without Hyper-V (full installation)
        WindowsServerEnterpriseNoHyperVFull = 38,
        /// SKU is Windows Server Datacenter without Hyper-V (core installation)
        WindowsServerDatacenterNoHyperVCore = 39,
        /// SKU is Windows Server Standard without Hyper-V (core installation)
        WindowsServerStandardNoHyperVCore = 40,
        /// SKU is Windows Server Enterprise without Hyper-V (core installation)
        WindowsServerEnterpriseNoHyperVCore = 41,
        /// SKU is Microsoft Hyper-V Server.
        MicrosoftHyperVServer = 42,
        /// SKU is Storage Server Express (core installation)
        StorageServerExpressCore = 43,
        /// SKU is Storage Server Standard (core installation)
        StorageServerStandardCore = 44,
        /// SKU is Storage Server Workgroup (core installation)
        StorageServerWorkgroupCore = 45,
        /// SKU is Storage Server Enterprise (core installation)
        StorageServerEnterpriseCore = 46,
        /// SKU is Windows Small Business Server 2011 Essentials.
        WindowsSmallBusinessServer2011Essentials = 50,
        /// SKU is Small Business Server Premium (core installation)
        SmallBusinessServerPremiumCore = 63,
        /// SKU is Windows Server Hyper Core V.
        WindowsServerHyperCoreV = 64,
        /// SKU is Windows Thin PC.
        WindowsThinPC = 87,
        /// SKU is Windows Embedded Industry.
        WindowsEmbeddedIndustry = 89,
        /// SKU is Windows RT.
        WindowsRT = 97,
        /// SKU is Windows Home.
        WindowsHome = 101,
        /// SKU is Windows Professional with Media Center.
        WindowsProfessionalWithMediaCenter = 103,
        /// SKU is Windows Mobile.
        WindowsMobile = 104,
        /// SKU is Windows Embedded Handheld.
        WindowsEmbeddedHandheld = 118,
        /// SKU is Windows IoT (Internet of Things) Core.
        WindowsIotCore = 123
    public enum OSType
        /// OS is unknown.
        /// OS is one other than covered by this Enum.
        /// OS is MacOS.
        MACROS = 2,
        /// OS is AT&amp;T UNIX.
        ATTUNIX = 3,
        /// OS is DG/UX.
        DGUX = 4,
        /// OS is DECNT.
        DECNT = 5,
        /// OS is Digital UNIX.
        DigitalUNIX = 6,
        /// OS is OpenVMS.
        OpenVMS = 7,
        /// OS is HP-UX.
        HPUX = 8,
        /// OS is AIX.
        AIX = 9,
        /// OS is MVS.
        MVS = 10,
        /// OS is OS/400.
        OS400 = 11,
        /// OS is OS/2.
        OS2 = 12,
        /// OS is Java Virtual Machine.
        JavaVM = 13,
        /// OS is MS-DOS.
        MSDOS = 14,
        /// OS is Windows 3x.
        WIN3x = 15,
        /// OS is Windows 95.
        WIN95 = 16,
        /// OS is Windows 98.
        WIN98 = 17,
        /// OS is Windows NT.
        WINNT = 18,
        /// OS is Windows CE.
        WINCE = 19,
        /// OS is NCR System 3000.
        NCR3000 = 20,
        /// OS is NetWare.
        NetWare = 21,
        /// OS is OSF.
        OSF = 22,
        /// OS is DC/OS.
        DC_OS = 23,
        /// OS is Reliant UNIX.
        ReliantUNIX = 24,
        /// OS is SCO UnixWare.
        SCOUnixWare = 25,
        /// OS is SCO OpenServer.
        SCOOpenServer = 26,
        /// OS is Sequent.
        Sequent = 27,
        /// OS is IRIX.
        IRIX = 28,
        /// OS is Solaris.
        Solaris = 29,
        /// OS is SunOS.
        SunOS = 30,
        /// OS is U6000.
        U6000 = 31,
        /// OS is ASERIES.
        ASERIES = 32,
        /// OS is Tandem NSK.
        TandemNSK = 33,
        /// OS is Tandem NT.
        TandemNT = 34,
        /// OS is BS2000.
        BS2000 = 35,
        /// OS is Linux.
        LINUX = 36,
        /// OS is Lynx.
        Lynx = 37,
        /// OS is XENIX.
        XENIX = 38,
        /// OS is VM/ESA.
        VM_ESA = 39,
        /// OS is Interactive UNIX.
        InteractiveUNIX = 40,
        /// OS is BSD UNIX.
        BSDUNIX = 41,
        /// OS is FreeBSD.
        FreeBSD = 42,
        /// OS is NetBSD.
        NetBSD = 43,
        /// OS is GNU Hurd.
        GNUHurd = 44,
        /// OS is OS 9.
        OS9 = 45,
        /// OS is Mach Kernel.
        MACHKernel = 46,
        /// OS is Inferno.
        Inferno = 47,
        /// OS is QNX.
        QNX = 48,
        /// OS is EPOC.
        EPOC = 49,
        /// OS is IxWorks.
        IxWorks = 50,
        /// OS is VxWorks.
        VxWorks = 51,
        /// OS is MiNT.
        MiNT = 52,
        /// OS is BeOS.
        BeOS = 53,
        /// OS is HP MPE.
        HP_MPE = 54,
        /// OS is NextStep.
        NextStep = 55,
        /// OS is PalmPilot.
        PalmPilot = 56,
        /// OS is Rhapsody.
        Rhapsody = 57,
        /// OS is Windows 2000.
        Windows2000 = 58,
        /// OS is Dedicated.
        Dedicated = 59,
        /// OS is OS/390.
        OS_390 = 60,
        /// OS is VSE.
        VSE = 61,
        /// OS is TPF.
        TPF = 62
    /// Specifies the type of the computer in use, such as laptop, desktop, or Tablet.
    public enum PCSystemType
        /// System type is unspecified.
        Unspecified = 0,
        /// System is a desktop.
        Desktop = 1,
        /// System is a mobile device.
        Mobile = 2,
        /// System is a workstation.
        Workstation = 3,
        /// System is an Enterprise Server.
        EnterpriseServer = 4,
        /// System is a Small Office and Home Office (SOHO) Server.
        SOHOServer = 5,
        /// System is an appliance PC.
        AppliancePC = 6,
        /// System is a performance server.
        PerformanceServer = 7,
        /// Maximum enum value.
        Maximum = 8
    /// This is an extended version of PCSystemType.
    // TODO: conflate these two enums???
    public enum PCSystemTypeEx
        /// System is a Slate.
        Slate = 8,
        Maximum = 9
    /// Specifies power-related capabilities of a logical device.
    public enum PowerManagementCapabilities
        /// Unknown capability.
        /// Power management not supported.
        NotSupported = 1,
        /// Power management features are currently disabled.
        Disabled = 2,
        /// The power management features are currently enabled,
        /// but the exact feature set is unknown or the information is unavailable.
        Enabled = 3,
        /// The device can change its power state based on usage or other criteria.
        PowerSavingModesEnteredAutomatically = 4,
        /// The power state may be set through the Win32_LogicalDevice class.
        PowerStateSettable = 5,
        /// Power may be done through the Win32_LogicalDevice class.
        PowerCyclingSupported = 6,
        /// Timed power-on is supported.
        TimedPowerOnSupported = 7
    /// Specified power states.
    public enum PowerState
        /// Power state is unknown.
        /// Full power.
        FullPower = 1,
        /// Power Save - Low Power mode.
        PowerSaveLowPowerMode = 2,
        /// Power Save - Standby.
        PowerSaveStandby = 3,
        /// Unknown Power Save mode.
        PowerSaveUnknown = 4,
        /// Power Cycle.
        PowerCycle = 5,
        /// Power Off.
        PowerOff = 6,
        /// Power Save - Warning.
        PowerSaveWarning = 7,
        /// Power Save - Hibernate.
        PowerSaveHibernate = 8,
        /// Power Save - Soft off.
        PowerSaveSoftOff = 9
    /// Specifies the primary function of a processor.
    public enum ProcessorType
        /// Processor ype is other than provided in these enumeration values.
        /// Processor type is.
        /// Processor is a Central Processing Unit (CPU)
        CentralProcessor = 3,
        /// Processor is a Math processor.
        MathProcessor = 4,
        /// Processor is a Digital Signal processor (DSP)
        DSPProcessor = 5,
        /// Processor is a Video processor.
        VideoProcessor = 6
    /// Specifies a computer's reset capability.
    public enum ResetCapability
        /// Capability is a value other than provided in these enumerated values.
        /// Reset capability is unknown.
        /// Capability is disabled.
        Disabled = 3,
        /// Capability is enabled.
        Enabled = 4,
        /// Capability is not implemented.
        NotImplemented = 5
    /// Specifies the kind of event that causes a computer to power up.
    public enum WakeUpType
        /// An event other than specified in this enumeration.
        /// Event type is unknown.
        /// Event is APM timer.
        APMTimer = 3,
        /// Event is a Modem Ring.
        ModemRing = 4,
        /// Event is a LAN Remove.
        LANRemote = 5,
        /// Event is a power switch.
        PowerSwitch = 6,
        /// Event is a PCI PME# signal.
        PCIPME = 7,
        /// AC power was restored.
        ACPowerRestored = 8
    /// Indicates the OEM's preferred power management profile.
    public enum PowerPlatformRole
        /// The OEM did not specify a specific role.
        /// The OEM specified a desktop role.
        /// The OEM specified a mobile role (for example, a laptop)
        /// The OEM specified a workstation role.
        /// The OEM specified an enterprise server role.
        /// The OEM specified a single office/home office (SOHO) server role.
        /// The OEM specified an appliance PC role.
        /// The OEM specified a performance server role.
        PerformanceServer = 7,    // v1 last supported
        /// The OEM specified a tablet form factor role.
        Slate = 8,    // v2 last supported
        /// Max enum value.
        MaximumEnumValue
    /// Additional system information, from Win32_OperatingSystem.
    public enum ProductType
        /// Product type is unknown.
        Unknown = 0,    // this value is not specified in Win32_OperatingSystem, but may prove useful
        WorkStation = 1,
        /// System is a domain controller.
        DomainController = 2,
        /// System is a server.
        Server = 3
    /// Specifies the system server level.
    public enum ServerLevel
        /// An unknown or unrecognized level was detected.
        /// Nano server.
        NanoServer,
        /// Server core.
        ServerCore,
        /// Server core with management tools.
        ServerCoreWithManagementTools,
        /// Full server.
        FullServer
    /// State of a software element.
    public enum SoftwareElementState
        /// Software element is deployable.
        Deployable = 0,
        /// Software element is installable.
        Installable = 1,
        /// Software element is executable.
        Executable = 2,
        /// Software element is running.
        Running = 3
    #endregion Enums used in the output objects
    #endregion Output components
    #region Native
    internal static partial class Native
        private static class PInvokeDllNames
            public const string GetPhysicallyInstalledSystemMemoryDllName = "api-ms-win-core-sysinfo-l1-2-1.dll";
            public const string PowerDeterminePlatformRoleExDllName = "api-ms-win-power-base-l1-1-0.dll";
            public const string GetFirmwareTypeDllName = "api-ms-win-core-kernel32-legacy-l1-1-1";
        public const int LOCALE_NAME_MAX_LENGTH = 85;
        public const uint POWER_PLATFORM_ROLE_V1 = 0x1;
        public const uint POWER_PLATFORM_ROLE_V2 = 0x2;
        public const uint S_OK = 0;
        /// Import WINAPI function PowerDeterminePlatformRoleEx.
        /// <param name="version">The version of the POWER_PLATFORM_ROLE enumeration for the platform.</param>
        /// <returns>POWER_PLATFORM_ROLE enumeration.</returns>
        [LibraryImport(PInvokeDllNames.PowerDeterminePlatformRoleExDllName, EntryPoint = "PowerDeterminePlatformRoleEx")]
        public static partial uint PowerDeterminePlatformRoleEx(uint version);
        /// Retrieve the amount of RAM physically installed in the computer.
        /// <param name="MemoryInKilobytes"></param>
        [LibraryImport(PInvokeDllNames.GetPhysicallyInstalledSystemMemoryDllName)]
        public static partial bool GetPhysicallyInstalledSystemMemory(out ulong MemoryInKilobytes);
        /// Retrieve the firmware type of the local computer.
        /// <param name="firmwareType">
        /// A reference to a <see cref="FirmwareType"/> enumeration to contain
        /// the resultant firmware type
        [LibraryImport(PInvokeDllNames.GetFirmwareTypeDllName)]
        public static partial bool GetFirmwareType(out FirmwareType firmwareType);
        /// Gets the data specified for the passed in property name from the
        /// Software Licensing API.
        /// <param name="licenseProperty">Name of the licensing property to get.</param>
        /// <param name="propertyValue">Out parameter for the value.</param>
        /// <returns>An hresult indicating success or failure.</returns>
        [LibraryImport("slc.dll", StringMarshalling = StringMarshalling.Utf16)]
        internal static partial int SLGetWindowsInformationDWORD(string licenseProperty, out int propertyValue);
    #endregion Native
