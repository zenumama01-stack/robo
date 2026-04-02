        private const int ValueFactoryCacheCount = 6;
        private static readonly Func<string, PSMemberInfoInternalCollection<PSMemberInfo>>[] s_valueFactoryCache;
        private static Func<string, PSMemberInfoInternalCollection<PSMemberInfo>> GetValueFactoryBasedOnInitCapacity(int capacity)
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
            if (capacity > ValueFactoryCacheCount)
                return CreateValueFactory(capacity);
            int cacheIndex = capacity - 1;
            if (s_valueFactoryCache[cacheIndex] == null)
                Interlocked.CompareExchange(
                    ref s_valueFactoryCache[cacheIndex],
                    CreateValueFactory(capacity),
                    comparand: null);
            return s_valueFactoryCache[cacheIndex];
            // Local helper function to avoid creating an instance of the generated delegate helper class
            // every time 'GetValueFactoryBasedOnInitCapacity' is invoked.
            static Func<string, PSMemberInfoInternalCollection<PSMemberInfo>> CreateValueFactory(int capacity)
                return key => new PSMemberInfoInternalCollection<PSMemberInfo>(capacity);
        private static MethodInfo GetMethodInfo(Type type, string method)
            return type.GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);
        private static ScriptBlock GetScriptBlock(string s)
            var sb = ScriptBlock.CreateDelayParsedScriptBlock(s, isProductCode: true);
            return sb;
        private void Process_Types_Ps1Xml(string filePath, ConcurrentBag<string> errors)
            #region System.Xml.XmlNode
            typeName = @"System.Xml.XmlNode";
            newMembers.Add(@"ToString");
                new PSCodeMethod(
                    @"ToString",
                    GetMethodInfo(typeof(Microsoft.PowerShell.ToStringCodeMethods), @"XmlNode")),
            #endregion System.Xml.XmlNode
            #region System.Xml.XmlNodeList
            typeName = @"System.Xml.XmlNodeList";
                    GetMethodInfo(typeof(Microsoft.PowerShell.ToStringCodeMethods), @"XmlNodeList")),
            #endregion System.Xml.XmlNodeList
            #region System.Management.Automation.PSDriveInfo
            typeName = @"System.Management.Automation.PSDriveInfo";
            newMembers.Add(@"Used");
                    @"Used",
                    GetScriptBlock(@"## Ensure that this is a FileSystem drive
          if($this.Provider.ImplementingType -eq
          [Microsoft.PowerShell.Commands.FileSystemProvider])
          $driveInfo = [System.IO.DriveInfo]::New($this.Root)
          if ( $driveInfo.IsReady ) { $driveInfo.TotalSize - $driveInfo.AvailableFreeSpace }
            newMembers.Add(@"Free");
                    @"Free",
          [System.IO.DriveInfo]::New($this.Root).AvailableFreeSpace
            #endregion System.Management.Automation.PSDriveInfo
            #region System.DirectoryServices.PropertyValueCollection
            typeName = @"System.DirectoryServices.PropertyValueCollection";
                    GetMethodInfo(typeof(Microsoft.PowerShell.ToStringCodeMethods), @"PropertyValueCollection")),
            #endregion System.DirectoryServices.PropertyValueCollection
            #region System.Drawing.Printing.PrintDocument
            typeName = @"System.Drawing.Printing.PrintDocument";
            newMembers.Add(@"Name");
                    @"Name",
                    GetScriptBlock(@"$this.PrinterSettings.PrinterName"),
            newMembers.Add(@"Color");
                    @"Color",
                    GetScriptBlock(@"$this.PrinterSettings.SupportsColor"),
            newMembers.Add(@"Duplex");
                    @"Duplex",
                    GetScriptBlock(@"$this.PrinterSettings.Duplex"),
            #endregion System.Drawing.Printing.PrintDocument
            #region System.Management.Automation.ApplicationInfo
            typeName = @"System.Management.Automation.ApplicationInfo";
            newMembers.Add(@"FileVersionInfo");
                    @"FileVersionInfo",
                    GetScriptBlock(@"[System.Diagnostics.FileVersionInfo]::getversioninfo( $this.Path )"),
            #endregion System.Management.Automation.ApplicationInfo
            #region System.DateTime
            typeName = @"System.DateTime";
            newMembers.Add(@"DateTime");
                    @"DateTime",
                    GetScriptBlock(@"if ((& { Set-StrictMode -Version 1; $this.DisplayHint }) -ieq  ""Date"")
          ""{0}"" -f $this.ToLongDateString()
          elseif ((& { Set-StrictMode -Version 1; $this.DisplayHint }) -ieq ""Time"")
          ""{0}"" -f  $this.ToLongTimeString()
          ""{0} {1}"" -f $this.ToLongDateString(), $this.ToLongTimeString()
            #endregion System.DateTime
            #region System.Net.IPAddress
            typeName = @"System.Net.IPAddress";
            newMembers.Add(@"IPAddressToString");
                    @"IPAddressToString",
                    GetScriptBlock(@"$this.Tostring()"),
            memberSetMembers = new PSMemberInfoInternalCollection<PSMemberInfo>(capacity: 2);
                new PSNoteProperty(@"DefaultDisplayProperty", @"IPAddressToString"),
            #endregion System.Net.IPAddress
            #region Deserialized.System.Net.IPAddress
            typeName = @"Deserialized.System.Net.IPAddress";
            #endregion Deserialized.System.Net.IPAddress
            #region System.Diagnostics.ProcessModule
            typeName = @"System.Diagnostics.ProcessModule";
            typeMembers = _extendedMembers.GetOrAdd(typeName, GetValueFactoryBasedOnInitCapacity(capacity: 6));
            newMembers.Add(@"Size");
                    @"Size",
                    GetScriptBlock(@"$this.ModuleMemorySize / 1024"),
            newMembers.Add(@"Company");
                    @"Company",
                    GetScriptBlock(@"$this.FileVersionInfo.CompanyName"),
            newMembers.Add(@"FileVersion");
                    @"FileVersion",
                    GetScriptBlock(@"$this.FileVersionInfo.FileVersion"),
            newMembers.Add(@"ProductVersion");
                    @"ProductVersion",
                    GetScriptBlock(@"$this.FileVersionInfo.ProductVersion"),
            newMembers.Add(@"Description");
                    @"Description",
                    GetScriptBlock(@"$this.FileVersionInfo.FileDescription"),
            newMembers.Add(@"Product");
                    @"Product",
                    GetScriptBlock(@"$this.FileVersionInfo.ProductName"),
            #endregion System.Diagnostics.ProcessModule
            #region System.Collections.DictionaryEntry
            typeName = @"System.Collections.DictionaryEntry";
                new PSAliasProperty(@"Name", @"Key", conversionType: null),
            #endregion System.Collections.DictionaryEntry
            #region System.Management.Automation.PSModuleInfo
            typeName = @"System.Management.Automation.PSModuleInfo";
                    new List<string> { "Name", "Path", "Description", "Guid", "Version", "ModuleBase", "ModuleType", "PrivateData", "AccessMode", "ExportedAliases", "ExportedCmdlets", "ExportedFunctions", "ExportedVariables", "NestedModules" }),
            #endregion System.Management.Automation.PSModuleInfo
            #region System.ServiceProcess.ServiceController
            typeName = @"System.ServiceProcess.ServiceController";
            typeMembers = _extendedMembers.GetOrAdd(typeName, GetValueFactoryBasedOnInitCapacity(capacity: 4));
                new PSAliasProperty(@"Name", @"ServiceName", conversionType: null),
            newMembers.Add(@"RequiredServices");
                new PSAliasProperty(@"RequiredServices", @"ServicesDependedOn", conversionType: null),
                new PSScriptMethod(
                    GetScriptBlock(@"$this.ServiceName"),
                    new List<string> { "Status", "Name", "DisplayName" }),
            #endregion System.ServiceProcess.ServiceController
            #region Deserialized.System.ServiceProcess.ServiceController
            typeName = @"Deserialized.System.ServiceProcess.ServiceController";
            #endregion Deserialized.System.ServiceProcess.ServiceController
            #region System.Management.Automation.CmdletInfo
            typeName = @"System.Management.Automation.CmdletInfo";
            newMembers.Add(@"DLL");
                    @"DLL",
                    GetScriptBlock(@"$this.ImplementingType.Assembly.Location"),
            #endregion System.Management.Automation.CmdletInfo
            #region System.Management.Automation.AliasInfo
            typeName = @"System.Management.Automation.AliasInfo";
            newMembers.Add(@"ResolvedCommandName");
                    @"ResolvedCommandName",
                    GetScriptBlock(@"$this.ResolvedCommand.Name"),
            newMembers.Add(@"DisplayName");
                    @"DisplayName",
                    GetScriptBlock(@"if ($null -ne $this.ResolvedCommand)
          $this.Name + "" -> "" + $this.ResolvedCommand.Name
          $this.Name + "" -> "" + $this.Definition
          "),
            #endregion System.Management.Automation.AliasInfo
            #region System.DirectoryServices.DirectoryEntry
            typeName = @"System.DirectoryServices.DirectoryEntry";
            newMembers.Add(@"ConvertLargeIntegerToInt64");
                    @"ConvertLargeIntegerToInt64",
                    GetMethodInfo(typeof(Microsoft.PowerShell.AdapterCodeMethods), @"ConvertLargeIntegerToInt64")),
            newMembers.Add(@"ConvertDNWithBinaryToString");
                    @"ConvertDNWithBinaryToString",
                    GetMethodInfo(typeof(Microsoft.PowerShell.AdapterCodeMethods), @"ConvertDNWithBinaryToString")),
                    new List<string> { "distinguishedName", "Path" }),
            #endregion System.DirectoryServices.DirectoryEntry
            #region System.IO.DirectoryInfo
            typeName = @"System.IO.DirectoryInfo";
            typeMembers = _extendedMembers.GetOrAdd(typeName, static key => new PSMemberInfoInternalCollection<PSMemberInfo>(capacity: 9));
            newMembers.Add(@"Mode");
                    @"Mode",
                    GetMethodInfo(typeof(Microsoft.PowerShell.Commands.FileSystemProvider), @"Mode"),
                    setterCodeReference: null),
            newMembers.Add(@"ModeWithoutHardLink");
                    @"ModeWithoutHardLink",
                    GetMethodInfo(typeof(Microsoft.PowerShell.Commands.FileSystemProvider), @"ModeWithoutHardLink"),
            newMembers.Add(@"BaseName");
                    @"BaseName",
                    GetScriptBlock(@"$this.Name"),
            newMembers.Add(@"ResolvedTarget");
                    @"ResolvedTarget",
                    GetMethodInfo(typeof(Microsoft.PowerShell.Commands.InternalSymbolicLinkLinkCodeMethods), @"ResolvedTarget"),
            newMembers.Add(@"Target");
                new PSAliasProperty(@"Target", @"LinkTarget", conversionType: null),
            newMembers.Add(@"LinkType");
                    @"LinkType",
                    GetMethodInfo(typeof(Microsoft.PowerShell.Commands.InternalSymbolicLinkLinkCodeMethods), @"GetLinkType"),
            newMembers.Add(@"NameString");
                    @"NameString",
                    GetMethodInfo(typeof(Microsoft.PowerShell.Commands.FileSystemProvider), @"NameString"),
            newMembers.Add(@"LengthString");
                    @"LengthString",
                    GetMethodInfo(typeof(Microsoft.PowerShell.Commands.FileSystemProvider), @"LengthString"),
            newMembers.Add(@"LastWriteTimeString");
                    @"LastWriteTimeString",
                    GetMethodInfo(typeof(Microsoft.PowerShell.Commands.FileSystemProvider), @"LastWriteTimeString"),
                new PSNoteProperty(@"DefaultDisplayProperty", @"Name"),
            #endregion System.IO.DirectoryInfo
            #region System.IO.FileInfo
            typeName = @"System.IO.FileInfo";
            typeMembers = _extendedMembers.GetOrAdd(typeName, static key => new PSMemberInfoInternalCollection<PSMemberInfo>(capacity: 10));
            newMembers.Add(@"VersionInfo");
                    @"VersionInfo",
                    GetScriptBlock(@"[System.Diagnostics.FileVersionInfo]::GetVersionInfo($this.FullName)"),
                    GetScriptBlock(@"if ($this.Extension.Length -gt 0){$this.Name.Remove($this.Name.Length - $this.Extension.Length)}else{$this.Name}"),
                    new List<string> { "LastWriteTime", "Length", "Name" }),
            #endregion System.IO.FileInfo
            #region System.Diagnostics.FileVersionInfo
            typeName = @"System.Diagnostics.FileVersionInfo";
            newMembers.Add(@"FileVersionRaw");
                    @"FileVersionRaw",
                    GetScriptBlock(@"New-Object System.Version -ArgumentList @(
            $this.FileMajorPart
            $this.FileMinorPart
            $this.FileBuildPart
            $this.FilePrivatePart)"),
            newMembers.Add(@"ProductVersionRaw");
                    @"ProductVersionRaw",
            $this.ProductMajorPart
            $this.ProductMinorPart
            $this.ProductBuildPart
            $this.ProductPrivatePart)"),
            #endregion System.Diagnostics.FileVersionInfo
            #region System.Diagnostics.EventLogEntry
            typeName = @"System.Diagnostics.EventLogEntry";
            newMembers.Add(@"EventID");
                    @"EventID",
                    GetScriptBlock(@"$this.get_EventID() -band 0xFFFF"),
            #endregion System.Diagnostics.EventLogEntry
            #region System.Management.ManagementBaseObject
            typeName = @"System.Management.ManagementBaseObject";
            newMembers.Add(@"PSComputerName");
                new PSAliasProperty(@"PSComputerName", @"__SERVER", conversionType: null),
            #endregion System.Management.ManagementBaseObject
            #region System.Management.ManagementObject#root\cimv2\Win32_PingStatus
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_PingStatus";
            newMembers.Add(@"IPV4Address");
                    @"IPV4Address",
                    GetScriptBlock(@"$iphost = [System.Net.Dns]::GetHostEntry($this.address)
          $iphost.AddressList | Where-Object { $_.AddressFamily -eq [System.Net.Sockets.AddressFamily]::InterNetwork } | Select-Object -first 1"),
            newMembers.Add(@"IPV6Address");
                    @"IPV6Address",
          $iphost.AddressList | Where-Object { $_.AddressFamily -eq [System.Net.Sockets.AddressFamily]::InterNetworkV6 } | Select-Object -first 1"),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_PingStatus
            #region System.Management.ManagementObject#root\cimv2\Win32_Process
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_Process";
            typeMembers = _extendedMembers.GetOrAdd(typeName, GetValueFactoryBasedOnInitCapacity(capacity: 5));
            newMembers.Add(@"ProcessName");
                new PSAliasProperty(@"ProcessName", @"Name", conversionType: null),
            newMembers.Add(@"Handles");
                new PSAliasProperty(@"Handles", @"Handlecount", conversionType: null),
            newMembers.Add(@"VM");
                new PSAliasProperty(@"VM", @"VirtualSize", conversionType: null),
            newMembers.Add(@"WS");
                new PSAliasProperty(@"WS", @"WorkingSetSize", conversionType: null),
            newMembers.Add(@"Path");
                    @"Path",
                    GetScriptBlock(@"$this.ExecutablePath"),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_Process
            #region System.Diagnostics.Process
            typeName = @"System.Diagnostics.Process";
            typeMembers = _extendedMembers.GetOrAdd(typeName, static key => new PSMemberInfoInternalCollection<PSMemberInfo>(capacity: 19));
            newMembers.Add(@"PSConfiguration");
                    @"PSConfiguration",
                    new List<string> { "Name", "Id", "PriorityClass", "FileVersion" }),
            newMembers.Add(@"PSResources");
                    @"PSResources",
                    new List<string> { "Name", "Id", "Handlecount", "WorkingSet", "NonPagedMemorySize", "PagedMemorySize", "PrivateMemorySize", "VirtualMemorySize", "Threads.Count", "TotalProcessorTime" }),
                new PSAliasProperty(@"Name", @"ProcessName", conversionType: null),
            newMembers.Add(@"SI");
                new PSAliasProperty(@"SI", @"SessionId", conversionType: null),
                new PSAliasProperty(@"VM", @"VirtualMemorySize64", conversionType: null),
                new PSAliasProperty(@"WS", @"WorkingSet64", conversionType: null),
            newMembers.Add(@"PM");
                new PSAliasProperty(@"PM", @"PagedMemorySize64", conversionType: null),
            newMembers.Add(@"NPM");
                new PSAliasProperty(@"NPM", @"NonpagedSystemMemorySize64", conversionType: null),
                    GetScriptBlock(@"$this.Mainmodule.FileName"),
            newMembers.Add(@"CommandLine");
                    @"CommandLine",
                    GetScriptBlock(@"
                        if ($IsWindows) {
                            (Get-CimInstance Win32_Process -Filter ""ProcessId = $($this.Id)"").CommandLine
                        } elseif ($IsLinux) {
                            $rawCmd = Get-Content -LiteralPath ""/proc/$($this.Id)/cmdline""
                            $rawCmd.Substring(0, $rawCmd.Length - 1) -replace ""`0"", "" ""
            newMembers.Add(@"Parent");
                    @"Parent",
                    GetMethodInfo(typeof(Microsoft.PowerShell.ProcessCodeMethods), @"GetParentProcess"),
                    GetScriptBlock(@"$this.Mainmodule.FileVersionInfo.CompanyName"),
            newMembers.Add(@"CPU");
                    @"CPU",
                    GetScriptBlock(@"$this.TotalProcessorTime.TotalSeconds"),
                    GetScriptBlock(@"$this.Mainmodule.FileVersionInfo.FileVersion"),
                    GetScriptBlock(@"$this.Mainmodule.FileVersionInfo.ProductVersion"),
                    GetScriptBlock(@"$this.Mainmodule.FileVersionInfo.FileDescription"),
                    GetScriptBlock(@"$this.Mainmodule.FileVersionInfo.ProductName"),
            newMembers.Add(@"__NounName");
                new PSNoteProperty(@"__NounName", @"Process"),
                    new List<string> { "Id", "Handles", "CPU", "SI", "Name" }),
            #endregion System.Diagnostics.Process
            #region Deserialized.System.Diagnostics.Process
            typeName = @"Deserialized.System.Diagnostics.Process";
                    new List<string> { "Id", "Handles", "CPU", "Name" }),
            #endregion Deserialized.System.Diagnostics.Process
            #region System.Management.ManagementObject#root\cli\Msft_CliAlias
            typeName = @"System.Management.ManagementObject#root\cli\Msft_CliAlias";
                    new List<string> { "FriendlyName", "PWhere", "Target" }),
            #endregion System.Management.ManagementObject#root\cli\Msft_CliAlias
            #region System.Management.ManagementObject#root\cimv2\Win32_BaseBoard
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_BaseBoard";
            newMembers.Add(@"PSStatus");
                    @"PSStatus",
                    new List<string> { "Status", "Name", "PoweredOn" }),
                    new List<string> { "Manufacturer", "Model", "Name", "SerialNumber", "SKU", "Product" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_BaseBoard
            #region System.Management.ManagementObject#root\cimv2\Win32_BIOS
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_BIOS";
                    new List<string> { "Status", "Name", "Caption", "SMBIOSPresent" }),
                    new List<string> { "SMBIOSBIOSVersion", "Manufacturer", "Name", "SerialNumber", "Version" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_BIOS
            #region System.Management.ManagementObject#root\cimv2\Win32_BootConfiguration
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_BootConfiguration";
                    new List<string> { "Name", "SettingID", "ConfigurationPath" }),
                    new List<string> { "BootDirectory", "Name", "SettingID", "Caption" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_BootConfiguration
            #region System.Management.ManagementObject#root\cimv2\Win32_CDROMDrive
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_CDROMDrive";
                    new List<string> { "Availability", "Drive", "ErrorCleared", "MediaLoaded", "NeedsCleaning", "Status", "StatusInfo" }),
                    new List<string> { "Caption", "Drive", "Manufacturer", "VolumeName" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_CDROMDrive
            #region System.Management.ManagementObject#root\cimv2\Win32_ComputerSystem
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_ComputerSystem";
                    new List<string> { "AdminPasswordStatus", "BootupState", "ChassisBootupState", "KeyboardPasswordStatus", "PowerOnPasswordStatus", "PowerSupplyState", "PowerState", "FrontPanelResetStatus", "ThermalState", "Status", "Name" }),
            newMembers.Add(@"POWER");
                    @"POWER",
                    new List<string> { "Name", "PowerManagementCapabilities", "PowerManagementSupported", "PowerOnPasswordStatus", "PowerState", "PowerSupplyState" }),
                    new List<string> { "Domain", "Manufacturer", "Model", "Name", "PrimaryOwnerName", "TotalPhysicalMemory" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_ComputerSystem
            #region System.Management.ManagementObject#root\cimv2\WIN32_PROCESSOR
            typeName = @"System.Management.ManagementObject#root\cimv2\WIN32_PROCESSOR";
                    new List<string> { "Availability", "CpuStatus", "CurrentVoltage", "DeviceID", "ErrorCleared", "ErrorDescription", "LastErrorCode", "LoadPercentage", "Status", "StatusInfo" }),
                    new List<string> { "AddressWidth", "DataWidth", "DeviceID", "ExtClock", "L2CacheSize", "L2CacheSpeed", "MaxClockSpeed", "PowerManagementSupported", "ProcessorType", "Revision", "SocketDesignation", "Version", "VoltageCaps" }),
                    new List<string> { "Caption", "DeviceID", "Manufacturer", "MaxClockSpeed", "Name", "SocketDesignation" }),
            #endregion System.Management.ManagementObject#root\cimv2\WIN32_PROCESSOR
            #region System.Management.ManagementObject#root\cimv2\Win32_ComputerSystemProduct
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_ComputerSystemProduct";
                    new List<string> { "Name", "Version" }),
                    new List<string> { "IdentifyingNumber", "Name", "Vendor", "Version", "Caption" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_ComputerSystemProduct
            #region System.Management.ManagementObject#root\cimv2\CIM_DataFile
            typeName = @"System.Management.ManagementObject#root\cimv2\CIM_DataFile";
                    new List<string> { "Status", "Name" }),
                    new List<string> { "Compressed", "Encrypted", "Size", "Hidden", "Name", "Readable", "System", "Version", "Writeable" }),
            #endregion System.Management.ManagementObject#root\cimv2\CIM_DataFile
            #region System.Management.ManagementObject#root\cimv2\WIN32_DCOMApplication
            typeName = @"System.Management.ManagementObject#root\cimv2\WIN32_DCOMApplication";
                    new List<string> { "Name", "Status" }),
                    new List<string> { "AppID", "InstallDate", "Name" }),
            #endregion System.Management.ManagementObject#root\cimv2\WIN32_DCOMApplication
            #region System.Management.ManagementObject#root\cimv2\WIN32_DESKTOP
            typeName = @"System.Management.ManagementObject#root\cimv2\WIN32_DESKTOP";
                    new List<string> { "Name", "ScreenSaverActive" }),
                    new List<string> { "Name", "ScreenSaverActive", "ScreenSaverSecure", "ScreenSaverTimeout", "SettingID" }),
            #endregion System.Management.ManagementObject#root\cimv2\WIN32_DESKTOP
            #region System.Management.ManagementObject#root\cimv2\WIN32_DESKTOPMONITOR
            typeName = @"System.Management.ManagementObject#root\cimv2\WIN32_DESKTOPMONITOR";
                    new List<string> { "DeviceID", "Name", "PixelsPerXLogicalInch", "PixelsPerYLogicalInch", "ScreenHeight", "ScreenWidth" }),
                    new List<string> { "DeviceID", "IsLocked", "LastErrorCode", "Name", "Status", "StatusInfo" }),
                    new List<string> { "DeviceID", "DisplayType", "MonitorManufacturer", "Name", "ScreenHeight", "ScreenWidth" }),
            #endregion System.Management.ManagementObject#root\cimv2\WIN32_DESKTOPMONITOR
            #region System.Management.ManagementObject#root\cimv2\Win32_DeviceMemoryAddress
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_DeviceMemoryAddress";
                    new List<string> { "Status", "Name", "MemoryType" }),
                    new List<string> { "MemoryType", "Name", "Status" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_DeviceMemoryAddress
            #region System.Management.ManagementObject#root\cimv2\Win32_DiskDrive
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_DiskDrive";
                    new List<string> { "ConfigManagerErrorCode", "LastErrorCode", "NeedsCleaning", "Status", "DeviceID", "StatusInfo", "Partitions" }),
                    new List<string> { "BytesPerSector", "ConfigManagerUserConfig", "DefaultBlockSize", "DeviceID", "Index", "InstallDate", "InterfaceType", "MaxBlockSize", "MaxMediaSize", "MinBlockSize", "NumberOfMediaSupported", "Partitions", "SectorsPerTrack", "Size", "TotalCylinders", "TotalHeads", "TotalSectors", "TotalTracks", "TracksPerCylinder" }),
                    new List<string> { "Partitions", "DeviceID", "Model", "Size", "Caption" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_DiskDrive
            #region System.Management.ManagementObject#root\cimv2\Win32_DiskQuota
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_DiskQuota";
                    new List<string> { "__PATH", "Status" }),
                    new List<string> { "DiskSpaceUsed", "Limit", "QuotaVolume", "User" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_DiskQuota
            #region System.Management.ManagementObject#root\cimv2\Win32_DMAChannel
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_DMAChannel";
                    new List<string> { "AddressSize", "DMAChannel", "MaxTransferSize", "Name", "Port" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_DMAChannel
            #region System.Management.ManagementObject#root\cimv2\Win32_Environment
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_Environment";
                    new List<string> { "Status", "Name", "SystemVariable" }),
                    new List<string> { "VariableValue", "Name", "UserName" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_Environment
            #region System.Management.ManagementObject#root\cimv2\Win32_Directory
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_Directory";
                    new List<string> { "Status", "Compressed", "Encrypted", "Name", "Readable", "Writeable" }),
                    new List<string> { "Hidden", "Archive", "EightDotThreeFileName", "FileSize", "Name", "Compressed", "Encrypted", "Readable" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_Directory
            #region System.Management.ManagementObject#root\cimv2\Win32_Group
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_Group";
                    new List<string> { "Caption", "Domain", "Name", "SID" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_Group
            #region System.Management.ManagementObject#root\cimv2\Win32_IDEController
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_IDEController";
                    new List<string> { "Manufacturer", "Name", "ProtocolSupported", "Status", "StatusInfo" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_IDEController
            #region System.Management.ManagementObject#root\cimv2\Win32_IRQResource
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_IRQResource";
                    new List<string> { "Status", "Caption", "Availability" }),
                    new List<string> { "Hardware", "IRQNumber", "Name", "Shareable", "TriggerLevel", "TriggerType" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_IRQResource
            #region System.Management.ManagementObject#root\cimv2\Win32_ScheduledJob
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_ScheduledJob";
                    new List<string> { "Status", "JobId", "JobStatus", "ElapsedTime", "StartTime", "Owner" }),
                    new List<string> { "JobId", "Name", "Owner", "Priority", "Command" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_ScheduledJob
            #region System.Management.ManagementObject#root\cimv2\Win32_LoadOrderGroup
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_LoadOrderGroup";
                    new List<string> { "GroupOrder", "Name" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_LoadOrderGroup
            #region System.Management.ManagementObject#root\cimv2\Win32_LogicalDisk
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_LogicalDisk";
                    new List<string> { "Status", "Availability", "DeviceID", "StatusInfo" }),
                    new List<string> { "DeviceID", "DriveType", "ProviderName", "FreeSpace", "Size", "VolumeName" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_LogicalDisk
            #region System.Management.ManagementObject#root\cimv2\Win32_LogonSession
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_LogonSession";
                    new List<string> { "AuthenticationPackage", "LogonId", "LogonType", "Name", "StartTime", "Status" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_LogonSession
            #region System.Management.ManagementObject#root\cimv2\WIN32_CACHEMEMORY
            typeName = @"System.Management.ManagementObject#root\cimv2\WIN32_CACHEMEMORY";
            newMembers.Add(@"ERROR");
                    @"ERROR",
                    new List<string> { "DeviceID", "ErrorCorrectType" }),
                    new List<string> { "Availability", "DeviceID", "Status", "StatusInfo" }),
                    new List<string> { "BlockSize", "CacheSpeed", "CacheType", "DeviceID", "InstalledSize", "Level", "MaxCacheSize", "NumberOfBlocks", "Status", "WritePolicy" }),
                    new List<string> { "BlockSize", "CacheSpeed", "CacheType", "DeviceID", "InstalledSize", "Level", "MaxCacheSize", "NumberOfBlocks", "Status" }),
            #endregion System.Management.ManagementObject#root\cimv2\WIN32_CACHEMEMORY
            #region System.Management.ManagementObject#root\cimv2\Win32_LogicalMemoryConfiguration
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_LogicalMemoryConfiguration";
                    new List<string> { "AvailableVirtualMemory", "Name", "TotalVirtualMemory" }),
                    new List<string> { "Name", "TotalVirtualMemory", "TotalPhysicalMemory", "TotalPageFileSpace" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_LogicalMemoryConfiguration
            #region System.Management.ManagementObject#root\cimv2\Win32_PhysicalMemoryArray
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_PhysicalMemoryArray";
                    new List<string> { "Status", "Name", "Replaceable", "Location" }),
                    new List<string> { "Model", "Name", "MaxCapacity", "MemoryDevices" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_PhysicalMemoryArray
            #region System.Management.ManagementObject#root\cimv2\WIN32_NetworkClient
            typeName = @"System.Management.ManagementObject#root\cimv2\WIN32_NetworkClient";
                    new List<string> { "Caption", "InstallDate", "Manufacturer", "Name" }),
            #endregion System.Management.ManagementObject#root\cimv2\WIN32_NetworkClient
            #region System.Management.ManagementObject#root\cimv2\Win32_NetworkLoginProfile
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_NetworkLoginProfile";
                    new List<string> { "Caption", "Privileges", "Profile", "UserId", "UserType", "Workstations" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_NetworkLoginProfile
            #region System.Management.ManagementObject#root\cimv2\Win32_NetworkProtocol
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_NetworkProtocol";
            newMembers.Add(@"FULLXXX");
                    @"FULLXXX",
                    new List<string> { "ConnectionlessService", "Description", "GuaranteesDelivery", "GuaranteesSequencing", "InstallDate", "MaximumAddressSize", "MaximumMessageSize", "MessageOriented", "MinimumAddressSize", "Name", "PseudoStreamOriented", "Status", "SupportsBroadcasting", "SupportsConnectData", "SupportsDisconnectData", "SupportsEncryption", "SupportsExpeditedData", "SupportsFragmentation", "SupportsGracefulClosing", "SupportsGuaranteedBandwidth", "SupportsMulticasting", "SupportsQualityofService" }),
                    new List<string> { "Name", "Status", "SupportsBroadcasting", "SupportsConnectData", "SupportsDisconnectData", "SupportsEncryption", "SupportsExpeditedData", "SupportsFragmentation", "SupportsGracefulClosing", "SupportsGuaranteedBandwidth", "SupportsMulticasting", "SupportsQualityofService" }),
                    new List<string> { "Caption", "GuaranteesDelivery", "GuaranteesSequencing", "ConnectionlessService", "Status", "Name" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_NetworkProtocol
            #region System.Management.ManagementObject#root\cimv2\Win32_NetworkConnection
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_NetworkConnection";
                    new List<string> { "Status", "ConnectionState", "Persistent", "LocalName", "RemoteName" }),
                    new List<string> { "LocalName", "RemoteName", "ConnectionState", "Status" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_NetworkConnection
            #region System.Management.ManagementObject#root\cimv2\Win32_NetworkAdapter
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_NetworkAdapter";
                    new List<string> { "Availability", "Name", "Status", "StatusInfo", "DeviceID" }),
                    new List<string> { "ServiceName", "MACAddress", "AdapterType", "DeviceID", "Name", "NetworkAddresses", "Speed" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_NetworkAdapter
            #region System.Management.ManagementObject#root\cimv2\Win32_NetworkAdapterConfiguration
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_NetworkAdapterConfiguration";
                    new List<string> { "DHCPLeaseExpires", "Index", "Description" }),
            newMembers.Add(@"DHCP");
                    @"DHCP",
                    new List<string> { "Description", "DHCPEnabled", "DHCPLeaseExpires", "DHCPLeaseObtained", "DHCPServer", "Index" }),
            newMembers.Add(@"DNS");
                    @"DNS",
                    new List<string> { "Description", "DNSDomain", "DNSDomainSuffixSearchOrder", "DNSEnabledForWINSResolution", "DNSHostName", "DNSServerSearchOrder", "DomainDNSRegistrationEnabled", "FullDNSRegistrationEnabled", "Index" }),
            newMembers.Add(@"IP");
                    @"IP",
                    new List<string> { "Description", "Index", "IPAddress", "IPConnectionMetric", "IPEnabled", "IPFilterSecurityEnabled" }),
            newMembers.Add(@"WINS");
                    @"WINS",
                    new List<string> { "Description", "Index", "WINSEnableLMHostsLookup", "WINSHostLookupFile", "WINSPrimaryServer", "WINSScopeID", "WINSSecondaryServer" }),
                    new List<string> { "DHCPEnabled", "IPAddress", "DefaultIPGateway", "DNSDomain", "ServiceName", "Description", "Index" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_NetworkAdapterConfiguration
            #region System.Management.ManagementObject#root\cimv2\Win32_NTDomain
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_NTDomain";
                    new List<string> { "Status", "DomainName" }),
            newMembers.Add(@"GUID");
                    @"GUID",
                    new List<string> { "DomainName", "DomainGuid" }),
                    new List<string> { "ClientSiteName", "DcSiteName", "Description", "DnsForestName", "DomainControllerAddress", "DomainControllerName", "DomainName", "Roles", "Status" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_NTDomain
            #region System.Management.ManagementObject#root\cimv2\Win32_NTLogEvent
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_NTLogEvent";
                    new List<string> { "Category", "CategoryString", "EventCode", "EventIdentifier", "TypeEvent", "InsertionStrings", "LogFile", "Message", "RecordNumber", "SourceName", "TimeGenerated", "TimeWritten", "Type", "UserName" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_NTLogEvent
            #region System.Management.ManagementObject#root\cimv2\Win32_NTEventlogFile
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_NTEventlogFile";
                    new List<string> { "Status", "LogfileName", "Name" }),
                    new List<string> { "FileSize", "LogfileName", "Name", "NumberOfRecords" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_NTEventlogFile
            #region System.Management.ManagementObject#root\cimv2\Win32_OnBoardDevice
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_OnBoardDevice";
                    new List<string> { "Status", "Description" }),
                    new List<string> { "DeviceType", "SerialNumber", "Enabled", "Description" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_OnBoardDevice
            #region System.Management.ManagementObject#root\cimv2\Win32_OperatingSystem
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_OperatingSystem";
            newMembers.Add(@"FREE");
                    @"FREE",
                    new List<string> { "FreePhysicalMemory", "FreeSpaceInPagingFiles", "FreeVirtualMemory", "Name" }),
                    new List<string> { "SystemDirectory", "Organization", "BuildNumber", "RegisteredUser", "SerialNumber", "Version" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_OperatingSystem
            #region System.Management.ManagementObject#root\cimv2\Win32_PageFileUsage
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_PageFileUsage";
                    new List<string> { "Status", "Name", "CurrentUsage" }),
                    new List<string> { "Caption", "Name", "PeakUsage" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_PageFileUsage
            #region System.Management.ManagementObject#root\cimv2\Win32_PageFileSetting
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_PageFileSetting";
                    new List<string> { "MaximumSize", "Name", "Caption" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_PageFileSetting
            #region System.Management.ManagementObject#root\cimv2\Win32_DiskPartition
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_DiskPartition";
                    new List<string> { "Index", "Status", "StatusInfo", "Name" }),
                    new List<string> { "NumberOfBlocks", "BootPartition", "Name", "PrimaryPartition", "Size", "Index" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_DiskPartition
            #region System.Management.ManagementObject#root\cimv2\Win32_PortResource
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_PortResource";
                    new List<string> { "NetConnectionStatus", "Status", "Name", "StartingAddress", "EndingAddress" }),
                    new List<string> { "Caption", "Name", "Alias" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_PortResource
            #region System.Management.ManagementObject#root\cimv2\Win32_PortConnector
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_PortConnector";
                    new List<string> { "Status", "Name", "ExternalReferenceDesignator" }),
                    new List<string> { "Tag", "ConnectorType", "SerialNumber", "ExternalReferenceDesignator", "PortType" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_PortConnector
            #region System.Management.ManagementObject#root\cimv2\Win32_Printer
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_Printer";
                    new List<string> { "Location", "Name", "PrinterState", "PrinterStatus", "ShareName", "SystemName" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_Printer
            #region System.Management.ManagementObject#root\cimv2\Win32_PrinterConfiguration
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_PrinterConfiguration";
                    new List<string> { "DriverVersion", "Name" }),
                    new List<string> { "PrintQuality", "DriverVersion", "Name", "PaperSize", "Caption" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_PrinterConfiguration
            #region System.Management.ManagementObject#root\cimv2\Win32_PrintJob
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_PrintJob";
                    new List<string> { "Document", "JobId", "JobStatus", "Name", "PagesPrinted", "Status", "JobIdCopy", "Name" }),
                    new List<string> { "Document", "JobId", "JobStatus", "Owner", "Priority", "Size", "Name" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_PrintJob
            #region System.Management.ManagementObject#root\cimv2\Win32_ProcessXXX
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_ProcessXXX";
                    new List<string> { "Status", "Name", "ProcessId" }),
            newMembers.Add(@"MEMORY");
                    @"MEMORY",
                    new List<string> { "Handle", "MaximumWorkingSetSize", "MinimumWorkingSetSize", "Name", "PageFaults", "PageFileUsage", "PeakPageFileUsage", "PeakVirtualSize", "PeakWorkingSetSize", "PrivatePageCount", "QuotaNonPagedPoolUsage", "QuotaPagedPoolUsage", "QuotaPeakNonPagedPoolUsage", "QuotaPeakPagedPoolUsage", "VirtualSize", "WorkingSetSize" }),
            newMembers.Add(@"IO");
                    @"IO",
                    new List<string> { "Name", "ProcessId", "ReadOperationCount", "ReadTransferCount", "WriteOperationCount", "WriteTransferCount" }),
            newMembers.Add(@"STATISTICS");
                    @"STATISTICS",
                    new List<string> { "HandleCount", "Name", "KernelModeTime", "MaximumWorkingSetSize", "MinimumWorkingSetSize", "OtherOperationCount", "OtherTransferCount", "PageFaults", "PageFileUsage", "PeakPageFileUsage", "PeakVirtualSize", "PeakWorkingSetSize", "PrivatePageCount", "ProcessId", "QuotaNonPagedPoolUsage", "QuotaPagedPoolUsage", "QuotaPeakNonPagedPoolUsage", "QuotaPeakPagedPoolUsage", "ReadOperationCount", "ReadTransferCount", "ThreadCount", "UserModeTime", "VirtualSize", "WorkingSetSize", "WriteOperationCount", "WriteTransferCount" }),
                    new List<string> { "ThreadCount", "HandleCount", "Name", "Priority", "ProcessId", "WorkingSetSize" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_ProcessXXX
            #region System.Management.ManagementObject#root\cimv2\Win32_Product
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_Product";
                    new List<string> { "Name", "Version", "InstallState" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_Product
            #region System.Management.ManagementObject#root\cimv2\Win32_QuickFixEngineering
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_QuickFixEngineering";
            newMembers.Add(@"InstalledOn");
                    @"InstalledOn",
                    GetScriptBlock(@"if ([environment]::osversion.version.build -ge 7000)
          # WMI team fixed the formatting issue related to InstalledOn
          # property in Windows7 (to return string)..so returning the WMI's
          # version directly
           [DateTime]::Parse($this.psBase.properties[""InstalledOn""].Value, [System.Globalization.DateTimeFormatInfo]::new())
          $orig = $this.psBase.properties[""InstalledOn""].Value
          $date = [datetime]::FromFileTimeUTC($(""0x"" + $orig))
          if ($date -lt ""1/1/1980"")
          if ($orig -match ""([0-9]{4})([01][0-9])([012][0-9])"")
          new-object datetime @([int]$matches[1], [int]$matches[2], [int]$matches[3])
          $date
                    new List<string> { "Description", "FixComments", "HotFixID", "InstallDate", "InstalledBy", "InstalledOn", "Name", "ServicePackInEffect", "Status" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_QuickFixEngineering
            #region System.Management.ManagementObject#root\cimv2\Win32_QuotaSetting
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_QuotaSetting";
                    new List<string> { "State", "VolumePath", "Caption" }),
                    new List<string> { "Caption", "DefaultLimit", "SettingID", "State", "VolumePath", "DefaultWarningLimit" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_QuotaSetting
            #region System.Management.ManagementObject#root\cimv2\Win32_OSRecoveryConfiguration
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_OSRecoveryConfiguration";
                    new List<string> { "DebugFilePath", "Name", "SettingID" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_OSRecoveryConfiguration
            #region System.Management.ManagementObject#root\cimv2\Win32_Registry
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_Registry";
                    new List<string> { "Status", "CurrentSize", "MaximumSize", "ProposedSize" }),
                    new List<string> { "CurrentSize", "MaximumSize", "Name", "Caption" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_Registry
            #region System.Management.ManagementObject#root\cimv2\Win32_SCSIController
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_SCSIController";
                    new List<string> { "Status", "Name", "StatusInfo" }),
                    new List<string> { "DriverName", "Manufacturer", "Name", "ProtocolSupported", "Status", "StatusInfo" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_SCSIController
            #region System.Management.ManagementObject#root\cimv2\Win32_PerfRawData_PerfNet_Server
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_PerfRawData_PerfNet_Server";
                    new List<string> { "Caption", "LogonPerSec", "LogonTotal", "Name", "ServerSessions", "WorkItemShortages" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_PerfRawData_PerfNet_Server
            #region System.Management.ManagementObject#root\cimv2\Win32_Service
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_Service";
                    new List<string> { "Name", "Status", "ExitCode" }),
                    new List<string> { "DesktopInteract", "ErrorControl", "Name", "PathName", "ServiceType", "StartMode" }),
                    new List<string> { "ExitCode", "Name", "ProcessId", "StartMode", "State", "Status" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_Service
            #region System.Management.ManagementObject#root\cimv2\Win32_Share
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_Share";
                    new List<string> { "Status", "Type", "Name" }),
                    new List<string> { "Name", "Path", "Description" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_Share
            #region System.Management.ManagementObject#root\cimv2\Win32_SoftwareElement
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_SoftwareElement";
                    new List<string> { "Status", "SoftwareElementState", "Name" }),
                    new List<string> { "Caption", "Name", "Path", "SerialNumber", "SoftwareElementID", "Version" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_SoftwareElement
            #region System.Management.ManagementObject#root\cimv2\Win32_SoftwareFeature
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_SoftwareFeature";
                    new List<string> { "Status", "Name", "InstallState", "LastUse" }),
                    new List<string> { "Caption", "IdentifyingNumber", "ProductName", "Vendor", "Version" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_SoftwareFeature
            #region System.Management.ManagementObject#root\cimv2\WIN32_SoundDevice
            typeName = @"System.Management.ManagementObject#root\cimv2\WIN32_SoundDevice";
                    new List<string> { "ConfigManagerUserConfig", "Name", "Status", "StatusInfo" }),
                    new List<string> { "Manufacturer", "Name", "Status", "StatusInfo" }),
            #endregion System.Management.ManagementObject#root\cimv2\WIN32_SoundDevice
            #region System.Management.ManagementObject#root\cimv2\Win32_StartupCommand
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_StartupCommand";
                    new List<string> { "Command", "User", "Caption" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_StartupCommand
            #region System.Management.ManagementObject#root\cimv2\Win32_SystemAccount
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_SystemAccount";
                    new List<string> { "Status", "SIDType", "Name", "Domain" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_SystemAccount
            #region System.Management.ManagementObject#root\cimv2\Win32_SystemDriver
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_SystemDriver";
                    new List<string> { "Status", "Name", "State", "ExitCode", "Started", "ServiceSpecificExitCode" }),
                    new List<string> { "DisplayName", "Name", "State", "Status", "Started" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_SystemDriver
            #region System.Management.ManagementObject#root\cimv2\Win32_SystemEnclosure
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_SystemEnclosure";
                    new List<string> { "Tag", "Status", "Name", "SecurityStatus" }),
                    new List<string> { "Manufacturer", "Model", "LockPresent", "SerialNumber", "SMBIOSAssetTag", "SecurityStatus" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_SystemEnclosure
            #region System.Management.ManagementObject#root\cimv2\Win32_SystemSlot
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_SystemSlot";
                    new List<string> { "Status", "SlotDesignation" }),
                    new List<string> { "SlotDesignation", "Tag", "SupportsHotPlug", "Status", "Shared", "PMESignal", "MaxDataWidth" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_SystemSlot
            #region System.Management.ManagementObject#root\cimv2\Win32_TapeDrive
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_TapeDrive";
                    new List<string> { "Status", "Availability", "DeviceID", "NeedsCleaning", "StatusInfo" }),
                    new List<string> { "DeviceID", "Id", "Manufacturer", "Name", "Caption" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_TapeDrive
            #region System.Management.ManagementObject#root\cimv2\Win32_TemperatureProbe
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_TemperatureProbe";
                    new List<string> { "Status", "CurrentReading", "DeviceID", "Name", "StatusInfo" }),
                    new List<string> { "CurrentReading", "Name", "Description", "MinReadable", "MaxReadable", "Status" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_TemperatureProbe
            #region System.Management.ManagementObject#root\cimv2\Win32_TimeZone
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_TimeZone";
                    new List<string> { "Bias", "SettingID", "Caption" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_TimeZone
            #region System.Management.ManagementObject#root\cimv2\Win32_UninterruptiblePowerSupply
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_UninterruptiblePowerSupply";
                    new List<string> { "Status", "DeviceID", "EstimatedChargeRemaining", "EstimatedRunTime", "Name", "StatusInfo", "TimeOnBackup" }),
                    new List<string> { "DeviceID", "EstimatedRunTime", "Name", "TimeOnBackup", "UPSPort", "Caption" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_UninterruptiblePowerSupply
            #region System.Management.ManagementObject#root\cimv2\Win32_UserAccount
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_UserAccount";
                    new List<string> { "Status", "Caption", "PasswordExpires" }),
                    new List<string> { "AccountType", "Caption", "Domain", "SID", "FullName", "Name" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_UserAccount
            #region System.Management.ManagementObject#root\cimv2\Win32_VoltageProbe
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_VoltageProbe";
                    new List<string> { "Status", "DeviceID", "Name", "NominalReading", "StatusInfo" }),
                    new List<string> { "Status", "Description", "CurrentReading", "MaxReadable", "MinReadable" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_VoltageProbe
            #region System.Management.ManagementObject#root\cimv2\Win32_VolumeQuotaSetting
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_VolumeQuotaSetting";
                    new List<string> { "Element", "Setting" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_VolumeQuotaSetting
            #region System.Management.ManagementObject#root\cimv2\Win32_WMISetting
            typeName = @"System.Management.ManagementObject#root\cimv2\Win32_WMISetting";
                    new List<string> { "BuildVersion", "Caption", "DatabaseDirectory", "EnableEvents", "LoggingLevel", "SettingID" }),
            #endregion System.Management.ManagementObject#root\cimv2\Win32_WMISetting
            #region System.Management.ManagementObject
            typeName = @"System.Management.ManagementObject";
            newMembers.Add(@"ConvertToDateTime");
                    @"ConvertToDateTime",
                    GetScriptBlock(@"[System.Management.ManagementDateTimeConverter]::ToDateTime($args[0])"),
            newMembers.Add(@"ConvertFromDateTime");
                    @"ConvertFromDateTime",
                    GetScriptBlock(@"[System.Management.ManagementDateTimeConverter]::ToDmtfDateTime($args[0])"),
            #endregion System.Management.ManagementObject
            #region Microsoft.PowerShell.Commands.HistoryInfo
            typeName = @"Microsoft.PowerShell.Commands.HistoryInfo";
                    @"DefaultKeyPropertySet",
                    new List<string> { "Id" }),
            #endregion Microsoft.PowerShell.Commands.HistoryInfo
            #region System.Management.ManagementClass
            typeName = @"System.Management.ManagementClass";
                new PSAliasProperty(@"Name", @"__Class", conversionType: null),
            #endregion System.Management.ManagementClass
            #region System.Management.Automation.Runspaces.PSSession
            typeName = @"System.Management.Automation.Runspaces.PSSession";
            newMembers.Add(@"State");
                    @"State",
                    GetScriptBlock(@"$this.Runspace.RunspaceStateInfo.State"),
            newMembers.Add(@"IdleTimeout");
                    @"IdleTimeout",
                    GetScriptBlock(@"$this.Runspace.ConnectionInfo.IdleTimeout"),
            newMembers.Add(@"OutputBufferingMode");
                    @"OutputBufferingMode",
                    GetScriptBlock(@"$this.Runspace.ConnectionInfo.OutputBufferingMode"),
            newMembers.Add(@"DisconnectedOn");
                    @"DisconnectedOn",
                    GetScriptBlock(@"$this.Runspace.DisconnectedOn"),
            newMembers.Add(@"ExpiresOn");
                    @"ExpiresOn",
                    GetScriptBlock(@"$this.Runspace.ExpiresOn"),
            #endregion System.Management.Automation.Runspaces.PSSession
            #region System.Guid
            typeName = @"System.Guid";
            newMembers.Add(@"Guid");
                    @"Guid",
                    GetScriptBlock(@"$this.ToString()"),
            #endregion System.Guid
            #region System.Management.Automation.Signature
            typeName = @"System.Management.Automation.Signature";
            #endregion System.Management.Automation.Signature
            #region System.Management.Automation.Job
            typeName = @"System.Management.Automation.Job";
                    GetScriptBlock(@"$this.JobStateInfo.State.ToString()"),
                    new List<string> { "HasMoreData", "StatusMessage", "Location", "Command", "JobStateInfo", "InstanceId", "Id", "Name", "State", "ChildJobs", "PSJobTypeName", "PSBeginTime", "PSEndTime" }),
            #endregion System.Management.Automation.Job
            #region System.Management.Automation.JobStateInfo
            typeName = @"System.Management.Automation.JobStateInfo";
            #endregion System.Management.Automation.JobStateInfo
            #region Deserialized.System.Management.Automation.JobStateInfo
            typeName = @"Deserialized.System.Management.Automation.JobStateInfo";
            #endregion Deserialized.System.Management.Automation.JobStateInfo
            #region Microsoft.PowerShell.DeserializingTypeConverter
            typeName = @"Microsoft.PowerShell.DeserializingTypeConverter";
            // Process type converter.
            ProcessTypeConverter(
                typeof(Microsoft.PowerShell.DeserializingTypeConverter),
                _typeConverters,
            #endregion Microsoft.PowerShell.DeserializingTypeConverter
            #region System.Net.Mail.MailAddress
            typeName = @"System.Net.Mail.MailAddress";
            #endregion System.Net.Mail.MailAddress
            #region Deserialized.System.Net.Mail.MailAddress
            typeName = @"Deserialized.System.Net.Mail.MailAddress";
            #endregion Deserialized.System.Net.Mail.MailAddress
            #region System.Globalization.CultureInfo
            typeName = @"System.Globalization.CultureInfo";
                    new List<string> { "LCID", "Name", "DisplayName", "IetfLanguageTag", "ThreeLetterISOLanguageName", "ThreeLetterWindowsLanguageName", "TwoLetterISOLanguageName" }),
            #endregion System.Globalization.CultureInfo
            #region Deserialized.System.Globalization.CultureInfo
            typeName = @"Deserialized.System.Globalization.CultureInfo";
            #endregion Deserialized.System.Globalization.CultureInfo
            #region System.Management.Automation.PSCredential
            typeName = @"System.Management.Automation.PSCredential";
            #endregion System.Management.Automation.PSCredential
            #region Deserialized.System.Management.Automation.PSCredential
            typeName = @"Deserialized.System.Management.Automation.PSCredential";
            #endregion Deserialized.System.Management.Automation.PSCredential
            #region System.Management.Automation.PSPrimitiveDictionary
            typeName = @"System.Management.Automation.PSPrimitiveDictionary";
            #endregion System.Management.Automation.PSPrimitiveDictionary
            #region Deserialized.System.Management.Automation.PSPrimitiveDictionary
            typeName = @"Deserialized.System.Management.Automation.PSPrimitiveDictionary";
            #endregion Deserialized.System.Management.Automation.PSPrimitiveDictionary
            #region System.Management.Automation.SwitchParameter
            typeName = @"System.Management.Automation.SwitchParameter";
            #endregion System.Management.Automation.SwitchParameter
            #region Deserialized.System.Management.Automation.SwitchParameter
            typeName = @"Deserialized.System.Management.Automation.SwitchParameter";
            #endregion Deserialized.System.Management.Automation.SwitchParameter
            #region System.Management.Automation.PSListModifier
            typeName = @"System.Management.Automation.PSListModifier";
            #endregion System.Management.Automation.PSListModifier
            #region Deserialized.System.Management.Automation.PSListModifier
            typeName = @"Deserialized.System.Management.Automation.PSListModifier";
            #endregion Deserialized.System.Management.Automation.PSListModifier
                    new List<string> { "RawData" }),
            #region Deserialized.System.Security.Cryptography.X509Certificates.X509Certificate2
            typeName = @"Deserialized.System.Security.Cryptography.X509Certificates.X509Certificate2";
            #endregion Deserialized.System.Security.Cryptography.X509Certificates.X509Certificate2
            #region System.Security.Cryptography.X509Certificates.X500DistinguishedName
            typeName = @"System.Security.Cryptography.X509Certificates.X500DistinguishedName";
            #endregion System.Security.Cryptography.X509Certificates.X500DistinguishedName
            #region Deserialized.System.Security.Cryptography.X509Certificates.X500DistinguishedName
            typeName = @"Deserialized.System.Security.Cryptography.X509Certificates.X500DistinguishedName";
            #endregion Deserialized.System.Security.Cryptography.X509Certificates.X500DistinguishedName
            #region System.Security.AccessControl.RegistrySecurity
            typeName = @"System.Security.AccessControl.RegistrySecurity";
            #endregion System.Security.AccessControl.RegistrySecurity
            #region Deserialized.System.Security.AccessControl.RegistrySecurity
            typeName = @"Deserialized.System.Security.AccessControl.RegistrySecurity";
            #endregion Deserialized.System.Security.AccessControl.RegistrySecurity
            #region System.Security.AccessControl.FileSystemSecurity
            typeName = @"System.Security.AccessControl.FileSystemSecurity";
            #endregion System.Security.AccessControl.FileSystemSecurity
            #region Deserialized.System.Security.AccessControl.FileSystemSecurity
            typeName = @"Deserialized.System.Security.AccessControl.FileSystemSecurity";
            #endregion Deserialized.System.Security.AccessControl.FileSystemSecurity
            #region HelpInfo
            typeName = @"HelpInfo";
            #endregion HelpInfo
            #region System.Management.Automation.PSTypeName
            typeName = @"System.Management.Automation.PSTypeName";
                new PSNoteProperty(@"SerializationMethod", @"String"),
                new PSAliasProperty(@"StringSerializationSource", @"Name", conversionType: null),
            #endregion System.Management.Automation.PSTypeName
            #region System.Management.Automation.ParameterMetadata
            typeName = @"System.Management.Automation.ParameterMetadata";
                    new List<string> { "Name", "ParameterType", "Aliases", "IsDynamic", "SwitchParameter" }),
            #endregion System.Management.Automation.ParameterMetadata
            #region System.Management.Automation.CommandInfo
            typeName = @"System.Management.Automation.CommandInfo";
            newMembers.Add(@"Namespace");
                new PSAliasProperty(@"Namespace", @"ModuleName", conversionType: null) { IsHidden = true },
            newMembers.Add(@"HelpUri");
                    @"HelpUri",
                    GetScriptBlock(@"$oldProgressPreference = $ProgressPreference
          $ProgressPreference = 'SilentlyContinue'
          [Microsoft.PowerShell.Commands.GetHelpCodeMethods]::GetHelpUri($this)
          catch {}
          $ProgressPreference = $oldProgressPreference
            #endregion System.Management.Automation.CommandInfo
            #region System.Management.Automation.ParameterSetMetadata
            typeName = @"System.Management.Automation.ParameterSetMetadata";
            newMembers.Add(@"Flags");
                    @"Flags",
                    GetMethodInfo(typeof(Microsoft.PowerShell.DeserializingTypeConverter), @"GetParameterSetMetadataFlags"),
                    new List<string> { "Position", "Flags", "HelpMessage" }),
            #endregion System.Management.Automation.ParameterSetMetadata
            #region Deserialized.System.Management.Automation.ParameterSetMetadata
            typeName = @"Deserialized.System.Management.Automation.ParameterSetMetadata";
            #endregion Deserialized.System.Management.Automation.ParameterSetMetadata
            #region Deserialized.System.Management.Automation.ExtendedTypeDefinition
            typeName = @"Deserialized.System.Management.Automation.ExtendedTypeDefinition";
            #endregion Deserialized.System.Management.Automation.ExtendedTypeDefinition
            #region System.Management.Automation.ExtendedTypeDefinition
            typeName = @"System.Management.Automation.ExtendedTypeDefinition";
            memberSetMembers = new PSMemberInfoInternalCollection<PSMemberInfo>(capacity: 4);
                    new List<string> { "TypeNames", "FormatViewDefinition" }),
                    new List<string> { "TypeName", "TypeNames", "FormatViewDefinition" }),
            #endregion System.Management.Automation.ExtendedTypeDefinition
            #region Deserialized.System.Management.Automation.FormatViewDefinition
            typeName = @"Deserialized.System.Management.Automation.FormatViewDefinition";
            #endregion Deserialized.System.Management.Automation.FormatViewDefinition
            #region System.Management.Automation.FormatViewDefinition
            typeName = @"System.Management.Automation.FormatViewDefinition";
            newMembers.Add(@"InstanceId");
                    @"InstanceId",
                    GetMethodInfo(typeof(Microsoft.PowerShell.DeserializingTypeConverter), @"GetFormatViewDefinitionInstanceId"),
            #endregion System.Management.Automation.FormatViewDefinition
            #region Deserialized.System.Management.Automation.PSControl
            typeName = @"Deserialized.System.Management.Automation.PSControl";
            #endregion Deserialized.System.Management.Automation.PSControl
            #region System.Management.Automation.PSControl
            typeName = @"System.Management.Automation.PSControl";
            #endregion System.Management.Automation.PSControl
            #region Deserialized.System.Management.Automation.PSControlGroupBy
            typeName = @"Deserialized.System.Management.Automation.PSControlGroupBy";
            #endregion Deserialized.System.Management.Automation.PSControlGroupBy
            #region System.Management.Automation.PSControlGroupBy
            typeName = @"System.Management.Automation.PSControlGroupBy";
            #endregion System.Management.Automation.PSControlGroupBy
            #region Deserialized.System.Management.Automation.EntrySelectedBy
            typeName = @"Deserialized.System.Management.Automation.EntrySelectedBy";
            #endregion Deserialized.System.Management.Automation.EntrySelectedBy
            #region System.Management.Automation.EntrySelectedBy
            typeName = @"System.Management.Automation.EntrySelectedBy";
            #endregion System.Management.Automation.EntrySelectedBy
            #region Deserialized.System.Management.Automation.DisplayEntry
            typeName = @"Deserialized.System.Management.Automation.DisplayEntry";
            #endregion Deserialized.System.Management.Automation.DisplayEntry
            #region System.Management.Automation.DisplayEntry
            typeName = @"System.Management.Automation.DisplayEntry";
            #endregion System.Management.Automation.DisplayEntry
            #region Deserialized.System.Management.Automation.TableControlColumnHeader
            typeName = @"Deserialized.System.Management.Automation.TableControlColumnHeader";
            #endregion Deserialized.System.Management.Automation.TableControlColumnHeader
            #region System.Management.Automation.TableControlColumnHeader
            typeName = @"System.Management.Automation.TableControlColumnHeader";
            #endregion System.Management.Automation.TableControlColumnHeader
            #region Deserialized.System.Management.Automation.TableControlRow
            typeName = @"Deserialized.System.Management.Automation.TableControlRow";
            #endregion Deserialized.System.Management.Automation.TableControlRow
            #region System.Management.Automation.TableControlRow
            typeName = @"System.Management.Automation.TableControlRow";
            #endregion System.Management.Automation.TableControlRow
            #region Deserialized.System.Management.Automation.TableControlColumn
            typeName = @"Deserialized.System.Management.Automation.TableControlColumn";
            #endregion Deserialized.System.Management.Automation.TableControlColumn
            #region System.Management.Automation.TableControlColumn
            typeName = @"System.Management.Automation.TableControlColumn";
            #endregion System.Management.Automation.TableControlColumn
            #region Deserialized.System.Management.Automation.ListControlEntry
            typeName = @"Deserialized.System.Management.Automation.ListControlEntry";
            #endregion Deserialized.System.Management.Automation.ListControlEntry
            #region System.Management.Automation.ListControlEntry
            typeName = @"System.Management.Automation.ListControlEntry";
                    new List<string> { "Items", "EntrySelectedBy" }),
                    new List<string> { "Items", "SelectedBy", "EntrySelectedBy" }),
            #endregion System.Management.Automation.ListControlEntry
            #region Deserialized.System.Management.Automation.ListControlEntryItem
            typeName = @"Deserialized.System.Management.Automation.ListControlEntryItem";
            #endregion Deserialized.System.Management.Automation.ListControlEntryItem
            #region System.Management.Automation.ListControlEntryItem
            typeName = @"System.Management.Automation.ListControlEntryItem";
            #endregion System.Management.Automation.ListControlEntryItem
            #region Deserialized.System.Management.Automation.WideControlEntryItem
            typeName = @"Deserialized.System.Management.Automation.WideControlEntryItem";
            #endregion Deserialized.System.Management.Automation.WideControlEntryItem
            #region System.Management.Automation.WideControlEntryItem
            typeName = @"System.Management.Automation.WideControlEntryItem";
            #endregion System.Management.Automation.WideControlEntryItem
            #region Deserialized.System.Management.Automation.CustomControlEntry
            typeName = @"Deserialized.System.Management.Automation.CustomControlEntry";
            #endregion Deserialized.System.Management.Automation.CustomControlEntry
            #region System.Management.Automation.CustomControlEntry
            typeName = @"System.Management.Automation.CustomControlEntry";
            #endregion System.Management.Automation.CustomControlEntry
            #region Deserialized.System.Management.Automation.CustomItemBase
            typeName = @"Deserialized.System.Management.Automation.CustomItemBase";
            #endregion Deserialized.System.Management.Automation.CustomItemBase
            #region System.Management.Automation.CustomItemBase
            typeName = @"System.Management.Automation.CustomItemBase";
            #endregion System.Management.Automation.CustomItemBase
            #region System.Web.Services.Protocols.SoapException
            typeName = @"System.Web.Services.Protocols.SoapException";
            newMembers.Add(@"PSMessageDetails");
                    @"PSMessageDetails",
                    GetScriptBlock(@"$this.Detail.""#text"""),
            #endregion System.Web.Services.Protocols.SoapException
            #region System.Management.Automation.ErrorRecord
            typeName = @"System.Management.Automation.ErrorRecord";
                    GetScriptBlock(@"& { Set-StrictMode -Version 1; $this.Exception.InnerException.PSMessageDetails }"),
            #endregion System.Management.Automation.ErrorRecord
            #region Deserialized.System.Enum
            typeName = @"Deserialized.System.Enum";
            newMembers.Add(@"Value");
                    @"Value",
            #endregion Deserialized.System.Enum
            #region Microsoft.PowerShell.Commands.Internal.Format.FormatInfoData
            typeName = @"Microsoft.PowerShell.Commands.Internal.Format.FormatInfoData";
            #endregion Microsoft.PowerShell.Commands.Internal.Format.FormatInfoData
            #region Deserialized.Microsoft.PowerShell.Commands.Internal.Format.FormatInfoData
            typeName = @"Deserialized.Microsoft.PowerShell.Commands.Internal.Format.FormatInfoData";
            #endregion Deserialized.Microsoft.PowerShell.Commands.Internal.Format.FormatInfoData
            #region System.Management.ManagementEventArgs
            typeName = @"System.Management.ManagementEventArgs";
            #endregion System.Management.ManagementEventArgs
            #region Deserialized.System.Management.ManagementEventArgs
            typeName = @"Deserialized.System.Management.ManagementEventArgs";
            #endregion Deserialized.System.Management.ManagementEventArgs
            #region System.Management.Automation.CallStackFrame
            typeName = @"System.Management.Automation.CallStackFrame";
            newMembers.Add(@"Command");
                    @"Command",
                    GetScriptBlock(@"if ($null -eq $this.InvocationInfo) { return $this.FunctionName }
          $commandInfo = $this.InvocationInfo.MyCommand
          if ($null -eq $commandInfo) { return $this.InvocationInfo.InvocationName }
          if ($commandInfo.Name -ne """") { return $commandInfo.Name }
          return $this.FunctionName"),
            newMembers.Add(@"Location");
                    @"Location",
                    GetScriptBlock(@"$this.GetScriptLocation()"),
            newMembers.Add(@"Arguments");
                    @"Arguments",
                    GetScriptBlock(@"$argumentsBuilder = new-object System.Text.StringBuilder
          $null = $(
          $argumentsBuilder.Append(""{"")
          foreach ($entry in $this.InvocationInfo.BoundParameters.GetEnumerator())
          if ($argumentsBuilder.Length -gt 1)
          $argumentsBuilder.Append("", "");
          $argumentsBuilder.Append($entry.Key).Append(""="")
          if ($entry.Value)
          $argumentsBuilder.Append([string]$entry.Value)
          foreach ($arg in $this.InvocationInfo.UnboundArguments.GetEnumerator())
          $argumentsBuilder.Append("", "")
          if ($arg)
          $argumentsBuilder.Append([string]$arg)
          $argumentsBuilder.Append('$null')
          $argumentsBuilder.Append('}');
          return $argumentsBuilder.ToString();"),
            #endregion System.Management.Automation.CallStackFrame
            #region Microsoft.PowerShell.Commands.PSSessionConfigurationCommands#PSSessionConfiguration
            typeName = @"Microsoft.PowerShell.Commands.PSSessionConfigurationCommands#PSSessionConfiguration";
            newMembers.Add(@"Permission");
                    @"Permission",
                    GetScriptBlock(@"trap { continue; }
          $private:sd = $null
          $private:sd = new-object System.Security.AccessControl.CommonSecurityDescriptor $false,$false,$this.SecurityDescriptorSddl
          if ($private:sd)
          # reset trap
          trap { }
          $private:dacls = """";
          $private:first = $true
          $private:sd.DiscretionaryAcl | ForEach-Object {
          if ($private:first)
          $private:first = $false;
          $private:dacls += "", ""
          $private:dacls += $_.SecurityIdentifier.Translate([System.Security.Principal.NTAccount]).ToString() + "" "" + $_.AceType
          } # end of foreach
          return $private:dacls
            #endregion Microsoft.PowerShell.Commands.PSSessionConfigurationCommands#PSSessionConfiguration
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PingStatus
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PingStatus";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PingStatus
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Process
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Process";
                    new List<string> { "ProcessId", "Name", "HandleCount", "WorkingSetSize", "VirtualSize" }),
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Process
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Msft_CliAlias
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Msft_CliAlias";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Msft_CliAlias
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_BaseBoard
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_BaseBoard";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_BaseBoard
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_BIOS
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_BIOS";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_BIOS
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_BootConfiguration
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_BootConfiguration";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_BootConfiguration
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_CDROMDrive
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_CDROMDrive";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_CDROMDrive
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ComputerSystem
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ComputerSystem";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ComputerSystem
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_PROCESSOR
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_PROCESSOR";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_PROCESSOR
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ComputerSystemProduct
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ComputerSystemProduct";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ComputerSystemProduct
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/CIM_DataFile
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/CIM_DataFile";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/CIM_DataFile
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_DCOMApplication
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_DCOMApplication";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_DCOMApplication
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_DESKTOP
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_DESKTOP";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_DESKTOP
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_DESKTOPMONITOR
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_DESKTOPMONITOR";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_DESKTOPMONITOR
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DeviceMemoryAddress
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DeviceMemoryAddress";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DeviceMemoryAddress
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DiskDrive
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DiskDrive";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DiskDrive
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DiskQuota
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DiskQuota";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DiskQuota
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DMAChannel
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DMAChannel";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DMAChannel
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Environment
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Environment";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Environment
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Directory
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Directory";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Directory
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Group
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Group";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Group
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_IDEController
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_IDEController";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_IDEController
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_IRQResource
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_IRQResource";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_IRQResource
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ScheduledJob
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ScheduledJob";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ScheduledJob
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LoadOrderGroup
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LoadOrderGroup";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LoadOrderGroup
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LogicalDisk
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LogicalDisk";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LogicalDisk
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LogonSession
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LogonSession";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LogonSession
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_CACHEMEMORY
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_CACHEMEMORY";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_CACHEMEMORY
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LogicalMemoryConfiguration
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LogicalMemoryConfiguration";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_LogicalMemoryConfiguration
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PhysicalMemoryArray
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PhysicalMemoryArray";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PhysicalMemoryArray
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_NetworkClient
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_NetworkClient";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_NetworkClient
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkLoginProfile
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkLoginProfile";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkLoginProfile
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkProtocol
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkProtocol";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkProtocol
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkConnection
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkConnection";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkConnection
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkAdapter
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkAdapter";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkAdapter
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkAdapterConfiguration
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkAdapterConfiguration";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NetworkAdapterConfiguration
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NTDomain
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NTDomain";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NTDomain
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NTLogEvent
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NTLogEvent";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NTLogEvent
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NTEventlogFile
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NTEventlogFile";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_NTEventlogFile
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_OnBoardDevice
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_OnBoardDevice";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_OnBoardDevice
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_OperatingSystem
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_OperatingSystem";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_OperatingSystem
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PageFileUsage
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PageFileUsage";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PageFileUsage
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PageFileSetting
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PageFileSetting";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PageFileSetting
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DiskPartition
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DiskPartition";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_DiskPartition
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PortResource
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PortResource";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PortResource
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PortConnector
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PortConnector";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PortConnector
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Printer
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Printer";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Printer
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PrinterConfiguration
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PrinterConfiguration";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PrinterConfiguration
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PrintJob
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PrintJob";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PrintJob
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ProcessXXX
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ProcessXXX";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_ProcessXXX
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Product
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Product";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Product
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_QuickFixEngineering
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_QuickFixEngineering";
          [DateTime]::Parse($this.psBase.CimInstanceProperties[""InstalledOn""].Value, [System.Globalization.DateTimeFormatInfo]::new())
          $orig = $this.psBase.CimInstanceProperties[""InstalledOn""].Value
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_QuickFixEngineering
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_QuotaSetting
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_QuotaSetting";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_QuotaSetting
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_OSRecoveryConfiguration
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_OSRecoveryConfiguration";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_OSRecoveryConfiguration
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Registry
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Registry";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Registry
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SCSIController
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SCSIController";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SCSIController
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PerfRawData_PerfNet_Server
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PerfRawData_PerfNet_Server";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_PerfRawData_PerfNet_Server
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Service
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Service";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Service
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Share
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Share";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_Share
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SoftwareElement
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SoftwareElement";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SoftwareElement
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SoftwareFeature
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SoftwareFeature";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SoftwareFeature
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_SoundDevice
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_SoundDevice";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/WIN32_SoundDevice
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_StartupCommand
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_StartupCommand";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_StartupCommand
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemAccount
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemAccount";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemAccount
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemDriver
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemDriver";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemDriver
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemEnclosure
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemEnclosure";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemEnclosure
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemSlot
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemSlot";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_SystemSlot
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_TapeDrive
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_TapeDrive";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_TapeDrive
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_TemperatureProbe
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_TemperatureProbe";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_TemperatureProbe
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_TimeZone
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_TimeZone";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_TimeZone
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_UninterruptiblePowerSupply
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_UninterruptiblePowerSupply";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_UninterruptiblePowerSupply
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_UserAccount
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_UserAccount";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_UserAccount
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_VoltageProbe
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_VoltageProbe";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_VoltageProbe
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_VolumeQuotaSetting
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_VolumeQuotaSetting";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_VolumeQuotaSetting
            #region Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_WMISetting
            typeName = @"Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_WMISetting";
            #endregion Microsoft.Management.Infrastructure.CimInstance#root/cimv2/Win32_WMISetting
            #region Microsoft.Management.Infrastructure.CimClass
            typeName = @"Microsoft.Management.Infrastructure.CimClass";
            newMembers.Add(@"CimClassName");
                    @"CimClassName",
                    GetScriptBlock(@"[OutputType([string])]
          param()
          $this.PSBase.CimSystemProperties.ClassName"),
            #endregion Microsoft.Management.Infrastructure.CimClass
            #region Microsoft.Management.Infrastructure.CimCmdlets.CimIndicationEventInstanceEventArgs
            typeName = @"Microsoft.Management.Infrastructure.CimCmdlets.CimIndicationEventInstanceEventArgs";
            #endregion Microsoft.Management.Infrastructure.CimCmdlets.CimIndicationEventInstanceEventArgs
            #region System.Management.Automation.Breakpoint
            typeName = @"System.Management.Automation.Breakpoint";
            #endregion System.Management.Automation.Breakpoint
            #region Deserialized.System.Management.Automation.Breakpoint
            typeName = @"Deserialized.System.Management.Automation.Breakpoint";
            #endregion Deserialized.System.Management.Automation.Breakpoint
            #region System.Management.Automation.BreakpointUpdatedEventArgs
            typeName = @"System.Management.Automation.BreakpointUpdatedEventArgs";
            #endregion System.Management.Automation.BreakpointUpdatedEventArgs
            #region Deserialized.System.Management.Automation.BreakpointUpdatedEventArgs
            typeName = @"Deserialized.System.Management.Automation.BreakpointUpdatedEventArgs";
            #endregion Deserialized.System.Management.Automation.BreakpointUpdatedEventArgs
            #region System.Management.Automation.DebuggerCommand
            typeName = @"System.Management.Automation.DebuggerCommand";
            #endregion System.Management.Automation.DebuggerCommand
            #region Deserialized.System.Management.Automation.DebuggerCommand
            typeName = @"Deserialized.System.Management.Automation.DebuggerCommand";
            #endregion Deserialized.System.Management.Automation.DebuggerCommand
            #region System.Management.Automation.DebuggerCommandResults
            typeName = @"System.Management.Automation.DebuggerCommandResults";
            #endregion System.Management.Automation.DebuggerCommandResults
            #region Deserialized.System.Management.Automation.DebuggerCommandResults
            typeName = @"Deserialized.System.Management.Automation.DebuggerCommandResults";
            #endregion Deserialized.System.Management.Automation.DebuggerCommandResults
            #region System.Version#IncludeLabel
            typeName = @"System.Version#IncludeLabel";
          $suffix = """"
          if (![String]::IsNullOrEmpty($this.PSSemVerPreReleaseLabel))
              $suffix = ""-""+$this.PSSemVerPreReleaseLabel
          if (![String]::IsNullOrEmpty($this.PSSemVerBuildLabel))
              $suffix += ""+""+$this.PSSemVerBuildLabel
          ""$($this.Major).$($this.Minor).$($this.Build)""+$suffix
            #endregion System.Version#IncludeLabel
            #region UnixStat
            typeName = @"System.IO.FileSystemInfo";
            // Where we have a method to invoke below, first check to be sure that the object is present
            // to avoid null reference issues
            newMembers.Add(@"UnixMode");
                new PSScriptProperty(@"UnixMode", GetScriptBlock(@"if ($this.UnixStat) { $this.UnixStat.GetModeString() }")),
            newMembers.Add(@"User");
                new PSScriptProperty(@"User", GetScriptBlock(@" if ($this.UnixStat) { $this.UnixStat.GetUserName() } ")),
            newMembers.Add(@"Group");
                new PSScriptProperty(@"Group", GetScriptBlock(@" if ($this.UnixStat) { $this.UnixStat.GetGroupName() } ")),
                new PSScriptProperty(@"Size", GetScriptBlock(@"$this.UnixStat.Size")),
