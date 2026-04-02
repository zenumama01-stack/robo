using PowerShellApi = System.Management.Automation.PowerShell;
using WSManNativeApi = System.Management.Automation.Remoting.Client.WSManNativeApi;
    #region Register-PSSessionConfiguration cmdlet
    /// Class implementing Register-PSSessionConfiguration.
    [Cmdlet(VerbsLifecycle.Register, RemotingConstants.PSSessionConfigurationNoun,
        DefaultParameterSetName = PSSessionConfigurationCommandBase.NameParameterSetName,
        ConfirmImpact = ConfirmImpact.Medium, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096793")]
    public sealed class RegisterPSSessionConfigurationCommand : PSSessionConfigurationCommandBase
        // To Escape " -- ""
        private const string newPluginSbFormat = @"
function Register-PSSessionConfiguration
    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=""Medium"")]
      [string] $filepath,
      [string] $pluginName,
      [bool] $shouldShowUI,
      [bool] $force,
      [string] $restartWSManTarget,
      [string] $restartWSManAction,
      [string] $restartWSManRequired,
      [string] $runAsUserName,
      [system.security.securestring] $runAsPassword,
      [System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode] $accessMode,
      [bool] $isSddlSpecified,
      [string] $configTableSddl,
      [bool] $noRestart
        ## Construct SID for network users
        [system.security.principal.wellknownsidtype]$evst = ""NetworkSid""
        $networkSID = new-object system.security.principal.securityidentifier $evst,$null
        ## If all session configurations have Network Access disabled,
        ## then we create this endpoint as Local as well.
        $newSDDL = $null
        $foundRemoteEndpoint = $false;
        Get-PSSessionConfiguration -Force:$force | Foreach-Object {{
            if ($_.Enabled)
                $sddl = $null
                if ($_.psobject.members[""SecurityDescriptorSddl""])
                    $sddl = $_.psobject.members[""SecurityDescriptorSddl""].Value
                if($sddl)
                    # See if it has 'Disable Network Access'
                    $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddl
                    $disableNetworkExists = $false
                    $sd.DiscretionaryAcl | ForEach-Object {{
                        if (($_.acequalifier -eq ""accessdenied"") -and ($_.securityidentifier -match $networkSID) -and ($_.AccessMask -eq 268435456))
                            $disableNetworkExists = $true
                    if(-not $disableNetworkExists) {{ $foundRemoteEndpoint = $true }}
        if(-not $foundRemoteEndpoint)
            $newSDDL = ""{1}""
        if ($force)
            if (Test-Path (Join-Path WSMan:\localhost\Plugin ""$pluginName""))
                Unregister-PSSessionConfiguration -name ""$pluginName"" -force
            new-item -path WSMan:\localhost\Plugin -file ""$filepath"" -name ""$pluginName""
        catch [System.InvalidOperationException] # WS2012/R2 WinRM w/o WMF has limitation where MaxConcurrentUsers can't be greater than 100
            $xml = [xml](get-content ""$filepath"")
            $xml.PlugInConfiguration.Quotas.MaxConcurrentUsers = 100
            Set-Content -path ""$filepath"" -Value $xml.OuterXml
        if ($? -and $runAsUserName)
                $runAsCredential = new-object system.management.automation.PSCredential($runAsUserName, $runAsPassword)
                $pluginWsmanRunAsUserPath = [System.IO.Path]::Combine(""WSMan:\localhost\Plugin"", ""$pluginName"", ""RunAsUser"")
                set-item -WarningAction SilentlyContinue $pluginWsmanRunAsUserPath $runAsCredential -confirm:$false
                remove-item (Join-Path WSMan:\localhost\Plugin ""$pluginName"") -recurse -force
                write-error $_
                # Do not add anymore clean up code after Write-Error, because if EA=Stop is set by user
                # any code at this point will not execute.
        ## Replace the SDDL with any groups or restrictions defined in the PSSessionConfigurationFile
        if($? -and $configTableSddl -and (-not $isSddlSpecified))
            $null = Set-PSSessionConfiguration -Name $pluginName -SecurityDescriptorSddl $configTableSddl -NoServiceRestart:$noRestart -Force:$force
        if ($? -and $shouldShowUI)
           $null = winrm configsddl ""{0}$pluginName""
           # if AccessMode is Disabled OR the winrm configsddl failed, we just return
           if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Disabled.Equals($accessMode) -or !$?)
        }} # end of if ($shouldShowUI)
        if ($?)
           # if AccessMode is Local or Remote, we need to check the SDDL the user set in the UI or passed in to the cmdlet.
           $curPlugin = Get-PSSessionConfiguration -Name $pluginName -Force:$force
           $curSDDL = $curPlugin.SecurityDescriptorSddl
           if (!$curSDDL)
               if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode))
               # Construct SID for network users
               $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$curSDDL
               $haveDisableACE = $false
               $securityIdentifierToPurge = $null
                        $haveDisableACE = $true
                        $securityIdentifierToPurge = $_.securityidentifier
               if (([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode) -or
                    [System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode)) -and $haveDisableACE)
                    # Add network deny ACE for local access or remote access with PSRemoting disabled.
                    $sd.DiscretionaryAcl.AddAccess(""deny"", $networkSID, 268435456, ""None"", ""None"")
                    $newSDDL = $sd.GetSddlForm(""all"")
               if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode) -and $haveDisableACE)
                    # Remove the specific ACE
                    $sd.discretionaryacl.RemoveAccessSpecific('Deny', $securityIdentifierToPurge, 268435456, 'none', 'none')
                    # if there is no discretionaryacl..add Builtin Administrators and Remote Management Users
                    # to the DACL group as this is the default WSMan behavior
                    if ($sd.discretionaryacl.count -eq 0)
                        [system.security.principal.wellknownsidtype]$bast = ""BuiltinAdministratorsSid""
                        $basid = new-object system.security.principal.securityidentifier $bast,$null
                        $sd.DiscretionaryAcl.AddAccess('Allow',$basid, 268435456, 'none', 'none')
                        # Remote Management Users, Win8+ only
                        if ([System.Environment]::OSVersion.Version -ge ""6.2.0.0"")
                            $rmSidId = new-object system.security.principal.securityidentifier ""{2}""
                            $sd.DiscretionaryAcl.AddAccess('Allow', $rmSidId, 268435456, 'none', 'none')
                        # Interactive Users
                        $iaSidId = new-object system.security.principal.securityidentifier ""{3}""
                        $sd.DiscretionaryAcl.AddAccess('Allow', $iaSidId, 268435456, 'none', 'none')
           }} # end of if(!$curSDDL)
        }} # end of if ($?)
        if ($? -and $newSDDL)
                if ($runAsUserName)
                    $null = Set-PSSessionConfiguration -Name $pluginName -SecurityDescriptorSddl $newSDDL -NoServiceRestart:$noRestart -Force:$force -WarningAction 0 -RunAsCredential $runAsCredential
                    $null = Set-PSSessionConfiguration -Name $pluginName -SecurityDescriptorSddl $newSDDL -NoServiceRestart:$noRestart -Force:$force -WarningAction 0
        if ($?){{
            try{{
                $s = New-PSSession -ComputerName localhost -ConfigurationName $pluginName -ErrorAction Stop
                # session is ok, no need to restart WinRM service
                Remove-PSSession $s -Confirm:$false
            }}catch{{
                # session is NOT ok, we need to restart winrm if -Force was specified, otherwise show a warning
                if ($force){{
                    Restart-Service -Name WinRM -Force -Confirm:$false
                }}else{{
                    $warningWSManRestart = [Microsoft.PowerShell.Commands.Internal.RemotingErrorResources]::WinRMRestartWarning -f $PSCmdlet.MyInvocation.MyCommand.Name
                    Write-Warning $warningWSManRestart
if ($null -eq $args[15])
    Register-PSSessionConfiguration -filepath $args[0] -pluginName $args[1] -shouldShowUI $args[2] -force $args[3] -whatif:$args[4] -confirm:$args[5] -restartWSManTarget $args[6] -restartWSManAction $args[7] -restartWSManRequired $args[8] -runAsUserName $args[9] -runAsPassword $args[10] -accessMode $args[11] -isSddlSpecified $args[12] -configTableSddl $args[13] -noRestart $args[14]
    Register-PSSessionConfiguration -filepath $args[0] -pluginName $args[1] -shouldShowUI $args[2] -force $args[3] -whatif:$args[4] -confirm:$args[5] -restartWSManTarget $args[6] -restartWSManAction $args[7] -restartWSManRequired $args[8] -runAsUserName $args[9] -runAsPassword $args[10] -accessMode $args[11] -isSddlSpecified $args[12] -configTableSddl $args[13] -noRestart $args[14] -erroraction $args[15]
        private static readonly ScriptBlock s_newPluginSb;
        private const string pluginXmlFormat = @"
<PlugInConfiguration xmlns='http://schemas.microsoft.com/wbem/wsman/1/config/PluginConfiguration'
    Name='{0}'
    Filename='{1}'
    SDKVersion='{12}'
    XmlRenderingType='text' {2} {6} {7} {8} {9} {10}>
  <InitializationParameters>
  </InitializationParameters>
  <Resources>
    <Resource ResourceUri='{4}' SupportsOptions='true' ExactMatch='true'>
{5}
      <Capability Type='Shell' />
    </Resource>
  </Resources>
  {11}
</PlugInConfiguration>
        private const string architectureAttribFormat = @"
    Architecture='{0}'";
        private const string sharedHostAttribFormat = @"
    UseSharedProcess='{0}'";
        private const string runasVirtualAccountAttribFormat = @"
    RunAsVirtualAccount='{0}'";
        private const string runAsVirtualAccountGroupsAttribFormat = @"
    RunAsVirtualAccountGroups='{0}'";
        private const string allowRemoteShellAccessFormat = @"
    Enabled='{0}'";
        private const string initParamFormat = @"
<Param Name='{0}' Value='{1}' />{2}";
        private const string privateDataFormat = @"<Param Name='PrivateData'>{0}</Param>";
        private const string securityElementFormat = "<Security Uri='{0}' ExactMatch='true' Sddl='{1}' />";
        private const string SessionConfigDataFormat = @"<SessionConfigurationData>{0}</SessionConfigurationData>";
        private string _gmsaAccount;
        private string _configTableSDDL;
        // true if there are errors running the wsman's configuration
        // command
        private bool _isErrorReported;
        /// Parameter used to specify the Processor Architecture that this shell targets.
        /// On a 64bit base OS, specifying a value of 32 means that the shell is configured
        /// to launch like a 32bit process (WOW64).
        [Alias("PA")]
        [ValidateSet("x86", "amd64")]
        public string ProcessorArchitecture { get; set; }
        static RegisterPSSessionConfigurationCommand()
            string localSDDL = GetLocalSddl();
            // compile the script block statically and reuse the same instance
            // every time the command is run..This will save on parsing time.
            string newPluginSbString = string.Format(CultureInfo.InvariantCulture,
                newPluginSbFormat,
                WSManNativeApi.ResourceURIPrefix, localSDDL, RemoteManagementUsersSID, InteractiveUsersSID);
            s_newPluginSb = ScriptBlock.Create(newPluginSbString);
            s_newPluginSb.LanguageMode = PSLanguageMode.FullLanguage;
        /// 1. Either both "AssemblyName" and "ConfigurationTypeName" must be specified
        /// or both must not be specified.
            if (isSddlSpecified && showUISpecified)
                string message = StringUtil.Format(RemotingErrorIdStrings.ShowUIAndSDDLCannotExist,
                    "SecurityDescriptorSddl",
                    "ShowSecurityDescriptorUI");
            if (isRunAsCredentialSpecified)
                WriteWarning(RemotingErrorIdStrings.RunAsSessionConfigurationSecurityWarning);
            if (isSddlSpecified)
                // Constructor call should succeed. The sddl is check in the property setter
                CommonSecurityDescriptor descriptor = new CommonSecurityDescriptor(false, false, sddl);
                SecurityIdentifier networkSidIdentifier = new SecurityIdentifier(WellKnownSidType.NetworkSid, null);
                bool networkDenyAllExists = false;
                foreach (CommonAce ace in descriptor.DiscretionaryAcl)
                    if (ace.AceQualifier.Equals(AceQualifier.AccessDenied) && ace.SecurityIdentifier.Equals(networkSidIdentifier) && ace.AccessMask == 268435456)
                        networkDenyAllExists = true;
                switch (AccessMode)
                    case PSSessionConfigurationAccessMode.Local:
                        if (!networkDenyAllExists)
                            descriptor.DiscretionaryAcl.AddAccess(AccessControlType.Deny, networkSidIdentifier, 268435456, InheritanceFlags.None, PropagationFlags.None);
                            sddl = descriptor.GetSddlForm(AccessControlSections.All);
                    case PSSessionConfigurationAccessMode.Remote:
                        if (networkDenyAllExists)
                            // Remove the specific ACE
                            descriptor.DiscretionaryAcl.RemoveAccessSpecific(AccessControlType.Deny, networkSidIdentifier, 268435456, InheritanceFlags.None, PropagationFlags.None);
                            // If the discretionaryAcl becomes empty, add the BA and RM which is the default WinRM behavior
                            if (descriptor.DiscretionaryAcl.Count == 0)
                                // BA
                                SecurityIdentifier baSidIdentifier = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                                descriptor.DiscretionaryAcl.AddAccess(AccessControlType.Allow, baSidIdentifier, 268435456, InheritanceFlags.None, PropagationFlags.None);
                                // Only for Win8+
                                if (Environment.OSVersion.Version >= new Version(6, 2))
                                    // Remote Management Users
                                    SecurityIdentifier rmSidIdentifier = new SecurityIdentifier(RemoteManagementUsersSID);
                                    descriptor.DiscretionaryAcl.AddAccess(AccessControlType.Allow, rmSidIdentifier, 268435456, InheritanceFlags.None, PropagationFlags.None);
                                // Interactive Users
                                SecurityIdentifier iaSidIdentifier = new SecurityIdentifier(InteractiveUsersSID);
                                descriptor.DiscretionaryAcl.AddAccess(AccessControlType.Allow, iaSidIdentifier, 268435456, InheritanceFlags.None, PropagationFlags.None);
                    case PSSessionConfigurationAccessMode.Disabled:
            if (!isSddlSpecified && !showUISpecified)
                if (AccessMode.Equals(PSSessionConfigurationAccessMode.Local))
                    // If AccessMode is Local or Disabled and no SDDL specified, use the default local SDDL
                    sddl = GetLocalSddl();
                else if (AccessMode.Equals(PSSessionConfigurationAccessMode.Remote))
                    // If AccessMode is Remote and no SDDL specified then use the default remote SDDL
                    sddl = GetRemoteSddl();
            // check if we have compatible WSMan
            RemotingCommandUtil.CheckRemotingCmdletPrerequisites();
            PSSessionConfigurationCommandUtilities.ThrowIfNotAdministrator();
            WSManConfigurationOption wsmanOption = transportOption as WSManConfigurationOption;
            if (wsmanOption != null)
                if (wsmanOption.ProcessIdleTimeoutSec != null && !isUseSharedProcessSpecified)
                    PSInvalidOperationException ioe = new PSInvalidOperationException(
                        StringUtil.Format(RemotingErrorIdStrings.InvalidConfigurationXMLAttribute, "ProcessIdleTimeoutSec",
                        "UseSharedProcess"));
                    ThrowTerminatingError(ioe.ErrorRecord);
            string pluginPath = PSSessionConfigurationCommandUtilities.GetWinrmPluginDllPath();
            pluginPath = Environment.ExpandEnvironmentVariables(pluginPath);
            if (!System.IO.File.Exists(pluginPath))
                        StringUtil.Format(RemotingErrorIdStrings.PluginDllMissing, RemotingConstants.PSPluginDLLName));
        /// For each record, execute it, and push the results into the
        /// success stream.
            WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.NcsScriptMessageV, newPluginSbFormat));
                string shouldProcessAction = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction,
                string shouldProcessTarget;
                    shouldProcessTarget = StringUtil.Format(RemotingErrorIdStrings.NcsShouldProcessTargetSDDL, Name, sddl);
                    shouldProcessTarget = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessTargetAdminEnable, Name);
                string action = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction,
                WriteWarning(StringUtil.Format(RemotingErrorIdStrings.WinRMRestartWarning, action));
                if (!ShouldProcess(shouldProcessTarget, shouldProcessAction))
            // Configuration file copy information.
            string srcConfigFilePath;
            string destConfigFilePath;
            // construct plugin config file.
            string pluginContent = ConstructPluginContent(out srcConfigFilePath, out destConfigFilePath);
            // Create temporary file with the content.
            string file = ConstructTemporaryFile(pluginContent);
            // Move the WinRM service to its own service host if the endpoint is given elevated credentials.
            if (isRunAsCredentialSpecified || RunAsVirtualAccountSpecified)
                PSSessionConfigurationCommandUtilities.MoveWinRmToIsolatedServiceHost(RunAsVirtualAccountSpecified);
            // Use the Group Managed Service Account if provided.
            if (!isRunAsCredentialSpecified && !string.IsNullOrEmpty(_gmsaAccount))
                runAsCredential = PSSessionConfigurationCommandUtilities.CreateGMSAAccountCredentials(_gmsaAccount);
                // restart-service winrm to make the changes effective.
                string restartServiceAction = RemotingErrorIdStrings.RestartWSManServiceAction;
                string restartServiceTarget = StringUtil.Format(RemotingErrorIdStrings.RestartWSManServiceTarget, "WinRM");
                string restartWSManRequiredForUI = StringUtil.Format(RemotingErrorIdStrings.RestartWSManRequiredShowUI,
                    string.Create(CultureInfo.InvariantCulture, $"Set-PSSessionConfiguration {shellName} -ShowSecurityDescriptorUI"));
                // gather -WhatIf, -Confirm parameter data and pass it to the script block
                bool whatIf = false;
                // confirm is always true to start with
                bool confirm = true;
                PSSessionConfigurationCommandUtilities.CollectShouldProcessParameters(this, out whatIf, out confirm);
                // gather -ErrorAction parameter data and pass it to the script block. if -ErrorAction is not set, pass $null in
                object errorAction = null;
                if (Context.CurrentCommandProcessor.CommandRuntime.IsErrorActionSet)
                    errorAction = Context.CurrentCommandProcessor.CommandRuntime.ErrorAction;
                ArrayList errorList = (ArrayList)Context.DollarErrorVariable;
                int errorCountBefore = errorList.Count;
                if (force &&
                this.Context != null &&
                this.Context.EngineHostInterface != null &&
                this.Context.EngineHostInterface.ExternalHost != null &&
                this.Context.EngineHostInterface.ExternalHost is System.Management.Automation.Remoting.ServerRemoteHost)
                    WriteWarning(RemotingErrorIdStrings.WinRMForceRestartWarning);
                s_newPluginSb.InvokeUsingCmdlet(
                    input: Array.Empty<object>(),
                    args: new object[] {
                                            file,
                                            shellName,
                                            ShowSecurityDescriptorUI.ToBool(),
                                            whatIf,
                                            confirm,
                                            restartServiceTarget,
                                            restartServiceAction,
                                            restartWSManRequiredForUI,
                                            runAsCredential?.UserName,
                                            runAsCredential?.Password,
                                            AccessMode,
                                            isSddlSpecified,
                                            _configTableSDDL,
                                            noRestart,
                                            errorAction
                errorList = (ArrayList)Context.DollarErrorVariable;
                _isErrorReported = errorList.Count > errorCountBefore;
                DeleteFile(file);
            // If the file no longer exists then re-copy the configuration file to the dest location after
            // newPluginSb script is run the file no longer exists.
            if ((srcConfigFilePath != null) && (destConfigFilePath != null) &&
                !File.Exists(destConfigFilePath))
                    File.Copy(srcConfigFilePath, destConfigFilePath, true);
                catch (NotSupportedException) { }
            System.Management.Automation.Tracing.Tracer tracer = new System.Management.Automation.Tracing.Tracer();
            tracer.EndpointRegistered(this.Name, WindowsIdentity.GetCurrent().Name);
        /// <param name="tmpFileName"></param>
        /// 1. New shell successfully registered. However cannot delete temporary plugin file {0}.
        ///    Reason for failure: {1}.
        private static void DeleteFile(string tmpFileName)
            Dbg.Assert(!string.IsNullOrEmpty(tmpFileName), "tmpFile cannot be null or empty.");
                File.Delete(tmpFileName);
                // WriteWarning(tmpFileName);
                e = uae;
                e = ae;
            catch (PathTooLongException pe)
                e = pe;
            catch (DirectoryNotFoundException dnfe)
                e = dnfe;
            catch (IOException ioe)
                e = ioe;
            catch (NotSupportedException nse)
                e = nse;
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.NcsCannotDeleteFileAfterInstall,
                                                                 tmpFileName,
        /// <param name="pluginContent"></param>
        /// 1. Cannot delete temporary file {0}. Try again. Reason for failure: {1}.
        /// 2. Cannot write shell configuration data into temporary file {0}. Try again.
        private static string ConstructTemporaryFile(string pluginContent)
            // Path.GetTempFileName creates a temporary file whereas GetRandomFileName does not.
            string tmpFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName()) + "psshell.xml";
            // Remove the temp file if it exists.
            if (File.Exists(tmpFileName))
                FileInfo destfile = new FileInfo(tmpFileName);
                if (destfile != null)
                        destfile.Attributes &= ~(FileAttributes.ReadOnly | FileAttributes.Hidden);
                        destfile.Delete();
                    catch (FileNotFoundException fnf)
                        e = fnf;
                    catch (DirectoryNotFoundException dnf)
                        e = dnf;
                    catch (UnauthorizedAccessException uac)
                        e = uac;
                        e = se;
                    catch (ArgumentNullException ane)
                        e = ane;
                    catch (NotSupportedException ns)
                        e = ns;
                        throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.NcsCannotDeleteFile,
                using (StreamWriter fileStream = File.CreateText(tmpFileName))
                    fileStream.Write(pluginContent);
                throw PSTraceSource.NewInvalidOperationException(RemotingErrorIdStrings.NcsCannotWritePluginContent,
            return tmpFileName;
        private string ConstructPluginContent(out string srcConfigFilePath, out string destConfigFilePath)
            srcConfigFilePath = null;
            destConfigFilePath = null;
            StringBuilder initParameters = new StringBuilder();
            const bool assemblyAndTypeTokensSet = false;
            // DISC endpoint
                string filePath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(Path, out provider, out drive);
                if (!provider.NameEquals(Context.ProviderNames.FileSystem) || !filePath.EndsWith(StringLiterals.PowerShellDISCFileExtension, StringComparison.OrdinalIgnoreCase))
                    string message = StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFilePath, filePath);
                    ErrorRecord er = new ErrorRecord(ioe, "InvalidPSSessionConfigurationFilePath",
                        ErrorCategory.InvalidArgument, Path);
                Guid sessionGuid = Guid.Empty;
                // Load session GUID from config file
                Hashtable configTable = null;
                    scriptInfo = DISCUtils.GetScriptInfoForFile(this.Context, filePath, out scriptName);
                    configTable = DISCUtils.LoadConfigFile(this.Context, scriptInfo);
                    string message = StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFileErrorProcessing, filePath, rte.Message);
                    InvalidOperationException ioe = new InvalidOperationException(message, rte);
                if (configTable == null)
                    string message = StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFile, filePath);
                    ErrorRecord er = new ErrorRecord(ioe, "InvalidPSSessionConfigurationFile",
                    if (configTable.ContainsKey(ConfigFileConstants.Guid))
                            if (configTable[ConfigFileConstants.Guid] != null)
                                sessionGuid = Guid.Parse(configTable[ConfigFileConstants.Guid].ToString());
                                InvalidOperationException invalidOperationException = new InvalidOperationException(StringUtil.Format(RemotingErrorIdStrings.ErrorParsingTheKeyInPSSessionConfigurationFile, ConfigFileConstants.Guid, filePath));
                                ThrowTerminatingError(new ErrorRecord(invalidOperationException, "InvalidGuidInPSSessionConfigurationFile", ErrorCategory.InvalidOperation, null));
                            ThrowTerminatingError(new ErrorRecord(e, "InvalidGuidInPSSessionConfigurationFile", ErrorCategory.InvalidOperation, null));
                    if (configTable.ContainsKey(ConfigFileConstants.PowerShellVersion))
                        if (!isPSVersionSpecified)
                                PSVersion = new Version(configTable[ConfigFileConstants.PowerShellVersion].ToString());
                                ThrowTerminatingError(new ErrorRecord(e, "InvalidPowerShellVersion", ErrorCategory.InvalidOperation, null));
                    if (configTable.ContainsKey(ConfigFileConstants.RunAsVirtualAccount))
                        this.RunAsVirtualAccount = LanguagePrimitives.ConvertTo<bool>(configTable[ConfigFileConstants.RunAsVirtualAccount]);
                        this.RunAsVirtualAccountSpecified = true;
                    if (configTable.ContainsKey(ConfigFileConstants.RunAsVirtualAccountGroups))
                        this.RunAsVirtualAccountGroups = PSSessionConfigurationCommandUtilities.GetRunAsVirtualAccountGroupsString(
                            DISCPowerShellConfiguration.TryGetStringArray(configTable[ConfigFileConstants.RunAsVirtualAccountGroups]));
                    if (configTable.ContainsKey(ConfigFileConstants.GMSAAccount))
                        _gmsaAccount = configTable[ConfigFileConstants.GMSAAccount] as string;
                    // Get role account and group restriction SDDL from configuration table, if any.
                    ErrorRecord error;
                    _configTableSDDL = PSSessionConfigurationCommandUtilities.ComputeSDDLFromConfiguration(
                        configTable,
                    // Update default Sddl with any group membership requirements.
                    if (string.IsNullOrEmpty(_configTableSDDL) && !this.isSddlSpecified && !string.IsNullOrEmpty(sddl))
                        string configGroupMemberShipACE = PSSessionConfigurationCommandUtilities.CreateConditionalACEFromConfig(configTable);
                        if (!string.IsNullOrEmpty(configGroupMemberShipACE))
                            sddl = PSSessionConfigurationCommandUtilities.UpdateSDDLUsersWithGroupConditional(sddl, configGroupMemberShipACE);
                        DISCUtils.ValidateAbsolutePaths(SessionState, configTable, Path);
                        ThrowTerminatingError(new ErrorRecord(e, "RelativePathsNotSupported", ErrorCategory.InvalidOperation, null));
                        DISCUtils.ValidateExtensions(configTable, Path);
                        ThrowTerminatingError(new ErrorRecord(e, "FileExtensionNotSupported", ErrorCategory.InvalidOperation, null));
                string destFolder = System.IO.Path.Combine(Utils.DefaultPowerShellAppBase, "SessionConfig");
                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);
                string destPath = System.IO.Path.Combine(destFolder,
                    shellName + "_" + sessionGuid.ToString() + StringLiterals.PowerShellDISCFileExtension);
                if (string.Equals(ProcessorArchitecture, "x86", StringComparison.OrdinalIgnoreCase))
                    if (string.Equals(procArch, "amd64", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(procArch, "ia64", StringComparison.OrdinalIgnoreCase))
                        InvalidOperationException ioe = new InvalidOperationException(RemotingErrorIdStrings.InvalidProcessorArchitecture);
                        ErrorRecord er = new ErrorRecord(ioe, "InvalidProcessorArchitecture", ErrorCategory.InvalidArgument, Path);
                        // syswow64 is applicable only on 64 bit platforms.
                        destPath = destPath.ToLowerInvariant().Replace("\\system32\\", "\\syswow64\\");
                // Return configuration file path names for later copy operation, if needed.
                // We need to copy the file again if after running Register-PSSessionConfiguration
                // removes the file which can happen if the endpoint already exists.
                srcConfigFilePath = filePath;
                destConfigFilePath = destPath;
                // Copy File.
                string destConfigFileDirectory = System.IO.Path.GetDirectoryName(destConfigFilePath);
                // The directory is not auto-created for PowerShell.
                // The call will create it or return its path if it already exists
                System.IO.Directory.CreateDirectory(destConfigFileDirectory);
                initParameters.Append(string.Format(CultureInfo.InvariantCulture,
                    initParamFormat,
                    ConfigurationDataFromXML.CONFIGFILEPATH_CamelCase,
                    destPath,
                    Environment.NewLine));
            if (!assemblyAndTypeTokensSet)
                if (!string.IsNullOrEmpty(configurationTypeName))
                        ConfigurationDataFromXML.SHELLCONFIGTYPETOKEN,
                        configurationTypeName,
                if (!string.IsNullOrEmpty(assemblyName))
                        ConfigurationDataFromXML.ASSEMBLYTOKEN,
            if (!string.IsNullOrEmpty(applicationBase))
                    ConfigurationDataFromXML.APPBASETOKEN,
                    applicationBase,
            if (!string.IsNullOrEmpty(configurationScript))
                    ConfigurationDataFromXML.STARTUPSCRIPTTOKEN,
                    configurationScript,
            if (maxCommandSizeMB.HasValue)
                    ConfigurationDataFromXML.MAXRCVDCMDSIZETOKEN,
                    maxCommandSizeMB.Value,
            if (maxObjectSizeMB.HasValue)
                    ConfigurationDataFromXML.MAXRCVDOBJSIZETOKEN,
                    maxObjectSizeMB.Value,
            if (threadAptState.HasValue)
                    ConfigurationDataFromXML.THREADAPTSTATETOKEN,
                    threadAptState.Value,
            if (threadOptions.HasValue)
                    ConfigurationDataFromXML.THREADOPTIONSTOKEN,
                    threadOptions.Value,
            // Default value for PSVersion
                psVersion = PSVersionInfo.PSVersion;
            if (psVersion != null)
                    ConfigurationDataFromXML.PSVERSIONTOKEN,
                    PSSessionConfigurationCommandUtilities.ConstructVersionFormatForConfigXml(psVersion),
                // Calculate MaxPSVersion from PSVersion
                MaxPSVersion = PSSessionConfigurationCommandUtilities.CalculateMaxPSVersion(psVersion);
                if (MaxPSVersion != null)
                        ConfigurationDataFromXML.MAXPSVERSIONTOKEN,
                        PSSessionConfigurationCommandUtilities.ConstructVersionFormatForConfigXml(MaxPSVersion),
            string securityParameters = string.Empty;
            if (!string.IsNullOrEmpty(sddl))
                securityParameters = string.Format(CultureInfo.InvariantCulture,
                    securityElementFormat,
                    WSManNativeApi.ResourceURIPrefix + shellName,
                    SecurityElement.Escape(sddl));
            string architectureParameter = string.Empty;
            if (!string.IsNullOrEmpty(ProcessorArchitecture))
                string tempValue = "32";
                switch (ProcessorArchitecture.ToLowerInvariant())
                    case "x86":
                        tempValue = "32";
                    case "amd64":
                        tempValue = "64";
                architectureParameter = string.Format(CultureInfo.InvariantCulture,
                    architectureAttribFormat,
                    tempValue);
            string sharedHostParameter = string.Empty;
            if (isUseSharedProcessSpecified)
                sharedHostParameter = string.Format(CultureInfo.InvariantCulture,
                    sharedHostAttribFormat, UseSharedProcess.ToString()
            string runAsVirtualAccountParameter = string.Empty;
            string runAsVirtualAccountGroupsParameter = string.Empty;
            if (RunAsVirtualAccount)
                runAsVirtualAccountParameter = string.Format(CultureInfo.InvariantCulture,
                    runasVirtualAccountAttribFormat, RunAsVirtualAccount.ToString()
                // Include virtual account groups if any.
                if (!string.IsNullOrEmpty(RunAsVirtualAccountGroups))
                    runAsVirtualAccountGroupsParameter = string.Format(CultureInfo.InvariantCulture,
                        runAsVirtualAccountGroupsAttribFormat, RunAsVirtualAccountGroups);
            string allowRemoteShellAccessParameter = string.Empty;
                    allowRemoteShellAccessParameter = string.Format(CultureInfo.InvariantCulture,
                        allowRemoteShellAccessFormat, false.ToString());
                        allowRemoteShellAccessFormat, true.ToString());
            StringBuilder sessionConfigurationData = new StringBuilder();
            if (modulesToImport != null && modulesToImport.Length > 0)
                sessionConfigurationData.Append(string.Format(CultureInfo.InvariantCulture,
                                                PSSessionConfigurationData.ModulesToImportToken,
                                                PSSessionConfigurationCommandUtilities.GetModulePathAsString(modulesToImport),
                                                string.Empty));
            if (sessionTypeOption != null)
                // TODO: This should probably be a terminating exception for Win8
                string privateData = this.sessionTypeOption.ConstructPrivateData();
                if (!string.IsNullOrEmpty(privateData))
                    sessionConfigurationData.Append(string.Format(CultureInfo.InvariantCulture, privateDataFormat, privateData));
            if (sessionConfigurationData.Length > 0)
                string sessionConfigData = string.Format(CultureInfo.InvariantCulture,
                                                         SessionConfigDataFormat,
                                                         sessionConfigurationData);
                string encodedSessionConfigData = SecurityElement.Escape(sessionConfigData);
                                                    ConfigurationDataFromXML.SESSIONCONFIGTOKEN,
                                                    encodedSessionConfigData,
            if (transportOption == null)
                transportOption = new WSManConfigurationOption();
                transportOption = transportOption.Clone() as PSTransportOption;
            transportOption.LoadFromDefaults(true);
            // If useSharedHost is set to false, we need to set hostIdleTimeout to 0 as well, else WS-Man throws error
            if (isUseSharedProcessSpecified && !UseSharedProcess)
                (transportOption as WSManConfigurationOption).ProcessIdleTimeoutSec = 0;
            string psPluginDllPath = PSSessionConfigurationCommandUtilities.GetWinrmPluginDllPath();
            string result = string.Format(CultureInfo.InvariantCulture,
                pluginXmlFormat,
                shellName, /* {0} */
                psPluginDllPath, /* {1} */
                architectureParameter, /* {2} */
                initParameters.ToString(), /* {3} */
                WSManNativeApi.ResourceURIPrefix + shellName, /* {4} */
                securityParameters, /* {5} */
                sharedHostParameter, /* {6} */
                runAsVirtualAccountParameter, /* {7} */
                runAsVirtualAccountGroupsParameter, /* {8} */
                allowRemoteShellAccessParameter, /* {9} */
                transportOption.ConstructOptionsAsXmlAttributes(), /* {10} */
                transportOption.ConstructQuotas(), /* {11} */
                (psVersion.Major < 3) ? 1 : 2 /* {12} - Pass in SDK version. */
    #endregion Register-PSSessionConfiguration cmdlet
    /// Utilities for Custom shell commands.
    internal static class PSSessionConfigurationCommandUtilities
        internal const string restartWSManFormat = "restart-service winrm -force -confirm:$false";
        internal const string PSCustomShellTypeName = "Microsoft.PowerShell.Commands.PSSessionConfigurationCommands#PSSessionConfiguration";
        /// Run script to restart the WinRM service. The script will write
        /// output and error into the cmdlets streams.
        /// Cmdlet's context in which the restart-service script is run.
        /// <param name="isErrorReported">
        /// if true, then this method is a no-op.
        /// if true, then the user will not be prompted.
        /// <param name="noServiceRestart">
        /// if true, we dont attempt to restart winrm service ie. this will be a no-op.
        internal static void RestartWinRMService(PSCmdlet cmdlet, bool isErrorReported, bool force, bool noServiceRestart)
            // restart the service only if there is no error running WSMan config command
            if (!(isErrorReported || noServiceRestart))
                if (force || cmdlet.ShouldProcess(restartServiceTarget, restartServiceAction))
                    cmdlet.WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.RestartWSManServiceMessageV));
                    ScriptBlock restartServiceScript = cmdlet.InvokeCommand.NewScriptBlock(restartWSManFormat);
                    restartServiceScript.InvokeUsingCmdlet(
                        contextCmdlet: cmdlet,
        internal static void MoveWinRmToIsolatedServiceHost(bool forVirtualAccount)
            string moveScript = "sc.exe config winrm type= own";
            if (forVirtualAccount)
                moveScript += @"
                    $requiredPrivileges = Get-ItemPropertyValue -Path HKLM:\SYSTEM\CurrentControlSet\Services\winrm -Name RequiredPrivileges
                    if($requiredPrivileges -notcontains 'SeTcbPrivilege')
                        $requiredPrivileges += @('SeTcbPrivilege')
                    Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\winrm -Name RequiredPrivileges -Value $requiredPrivileges
                    Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\winrm -Name ObjectName -Value 'LocalSystem'";
            using (System.Management.Automation.PowerShell invoker = System.Management.Automation.PowerShell.Create(RunspaceMode.CurrentRunspace))
                invoker.AddScript(moveScript).Invoke();
        /// Gathers WhatIf, Confirm parameter values from the cmdlet.
        /// <param name="whatIf"></param>
        /// <param name="confirm"></param>
        internal static void CollectShouldProcessParameters(PSCmdlet cmdlet, out bool whatIf, out bool confirm)
            whatIf = false;
            confirm = false;
            MshCommandRuntime cmdRuntime = cmdlet.CommandRuntime as MshCommandRuntime;
            if (cmdRuntime != null)
                whatIf = cmdRuntime.WhatIf;
                // take the value of confirm only if it is explicitly set by the user
                if (cmdRuntime.IsConfirmFlagSet)
                    confirm = cmdRuntime.Confirm;
        /// Checks if the current thread is running elevated. If not, throws an error.
        /// 1. Access is denied. You need to run this cmdlet from an elevated process.
                string message = StringUtil.Format(RemotingErrorIdStrings.EDcsRequiresElevation);
        /// Creates a Grouped Managed Service Account credential based on the passed in account name.
        /// <param name="gmsaAccount">Group Managed Service Account name.</param>
        /// <returns>PSCredential for GMS account.</returns>
        /// Invalid account name.  Must be of form 'Domain\UserName'.
        internal static PSCredential CreateGMSAAccountCredentials(string gmsaAccount)
            Dbg.Assert(!string.IsNullOrEmpty(gmsaAccount), "Should not be null or empty string.");
            // Validate account name form (must be DomainName\UserName)
            var parts = gmsaAccount.Split('\\');
            if ((parts.Length != 2) ||
                (string.IsNullOrEmpty(parts[0])) ||
                (string.IsNullOrEmpty(parts[1]))
                throw new InvalidOperationException(RemotingErrorIdStrings.InvalidGMSAName);
            // Use the provided GMSA account name (Domain\UserName$) with empty password.
            string userName = gmsaAccount + "$";
            SecureString password = new SecureString();
        /// Calculates the MaxPSVersion in the config xml from psVersion.
        /// <param name="psVersion"></param>
        internal static Version CalculateMaxPSVersion(Version psVersion)
            Version maxPSVersion = null;
            if (psVersion != null && psVersion.Major == 2)
                maxPSVersion = new Version(2, 0);
            return maxPSVersion;
        /// Converts the module path represented in the string[] into a comma separated string.
        internal static string GetModulePathAsString(object[] modulePath)
            if (modulePath != null && modulePath.Length > 0)
                foreach (object s in modulePath)
                    var module = s as string;
                        var moduleSpec = s as ModuleSpecification;
                        if (moduleSpec != null)
                            // Double escaping on ModuleSpecification string is required to treat it as a single value along with module names/paths in SessionConfigurationData
                            sb.Append(SecurityElement.Escape(SecurityElement.Escape(moduleSpec.ToString())));
        /// Converts the version number to the format "Major.Minor" which needs to be persisted in the config xml.
        internal static Version ConstructVersionFormatForConfigXml(Version psVersion)
                result = new Version(psVersion.Major, psVersion.Minor);
        /// Takes array of group name string objects and returns a semicolon delimited string.
        /// <param name="groups"></param>
        internal static string GetRunAsVirtualAccountGroupsString(string[] groups)
            if (groups == null) { return string.Empty; }
            return string.Join(';', groups);
        /// Returns the default WinRM plugin shell name for this instance of PowerShell.
        internal static string GetWinrmPluginShellName()
            // PowerShell uses a versioned directory to hold the plugin
            return string.Concat("PowerShell.", PSVersionInfo.GitCommitId);
        /// Returns the default WinRM plugin DLL file path for this instance of PowerShell.
        internal static string GetWinrmPluginDllPath()
            // PowerShell 6+ uses its versioned directory instead of system32
            string pluginDllDirectory = System.IO.Path.Combine("%windir%\\system32\\PowerShell", PSVersionInfo.GitCommitId);
            return System.IO.Path.Combine(pluginDllDirectory, RemotingConstants.PSPluginDLLName);
        #region Group Conditional SDDL
        /// Builds a session SDDL based on the provided configuration hashtable.
        /// Retrieves RequiredGroups information to add conditional group membership restrictions to SDDL.
        /// Retrieves RoleDefinitions information to include role user accounts.
        /// <param name="configTable"></param>
        /// <param name="accessMode"></param>
        /// <returns>SDDL.</returns>
        internal static string ComputeSDDLFromConfiguration(
            Hashtable configTable,
            PSSessionConfigurationAccessMode accessMode,
            Dbg.Assert(configTable != null, "configTable input parameter cannot be null.");
            string sddl = string.Empty;
            // RoleDefinitions
            if (configTable.ContainsKey(ConfigFileConstants.RoleDefinitions))
                // Start with known good security descriptor.
                if (accessMode == PSSessionConfigurationAccessMode.Local)
                    sddl = PSSessionConfigurationCommandBase.GetLocalSddl();
                else if (accessMode == PSSessionConfigurationAccessMode.Remote)
                    sddl = PSSessionConfigurationCommandBase.GetRemoteSddl();
                // Purge all existing access rules so that only role definition principals are granted access.
                List<SecurityIdentifier> sidsToRemove = new List<SecurityIdentifier>();
                    sidsToRemove.Add(ace.SecurityIdentifier);
                foreach (var sidToRemove in sidsToRemove)
                    descriptor.PurgeAccessControl(sidToRemove);
                Hashtable roleNamesHash = configTable[ConfigFileConstants.RoleDefinitions] as Hashtable;
                foreach (object roleName in roleNamesHash.Keys)
                    string roleNameValue = roleName.ToString();
                        NTAccount ntAccount = new NTAccount(roleNameValue);
                        SecurityIdentifier accountSid = (SecurityIdentifier)ntAccount.Translate(typeof(SecurityIdentifier));
                        // AccessMask = 268435456 == 0x10000000 == GR == Generic Read
                        descriptor.DiscretionaryAcl.AddAccess(AccessControlType.Allow, accountSid, 268435456, InheritanceFlags.None, PropagationFlags.None);
                    catch (IdentityNotMappedException e)
                        string message = StringUtil.Format(RemotingErrorIdStrings.CouldNotResolveRoleDefinitionPrincipal, roleNameValue, e.Message);
                        InvalidOperationException ioe = new InvalidOperationException(message, e);
                        error = new ErrorRecord(ioe, "CouldNotResolveRoleDefinitionPrincipal", ErrorCategory.ObjectNotFound, roleNameValue);
                if (descriptor.DiscretionaryAcl.Count > 0)
                    // RequiredGroups
                    string conditionalGroupACE = CreateConditionalACEFromConfig(configTable);
                    if (conditionalGroupACE != null)
                        sddl = UpdateSDDLUsersWithGroupConditional(sddl, conditionalGroupACE);
        #region SDDL Update
        private const char OpenParenChar = '(';
        private const char CloseParenChar = ')';
        private const char ACESeparatorChar = ';';
        private const string ACESeparator = ";";
        private const string ConditionalACEPrefix = "X";
        /// Update SDDL user ACEs with provided conditional group ACE fragment.
        /// SDDL:       O:NSG:BAD:P(A;;GA;;;BA)(A;;GA;;;RM)(A;;GA;;;IU)(A;;GA;;;S-1-5-21-2127438184-1604012920-1882527527)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)
        /// Cond ACE:   (Member_of {SID(2FA_GROUP_1)})
        /// Cond SDDL:  O:NSG:BAD:P(XA;;GA;;;BA)(A;;GA;;;RM)(A;;GA;;;IU)(XA;;GA;;;S-1-5-21-2127438184-1604012920-1882527527;(Member_of {SID(2FA_GROUP_1)}))S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)
        /// <param name="sddl"></param>
        /// <param name="conditionalGroupACE"></param>
        internal static string UpdateSDDLUsersWithGroupConditional(
            string sddl,
            string conditionalGroupACE)
            // Parse sddl for DACL user ACEs.
            // prologue string contains the beginning owner and primary group components
            //    O:NSG:BAD:P
            // epilogue string contains the ending (and optional) SACL components
            //    S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)
            // (https://msdn.microsoft.com/library/windows/desktop/aa379570(v=vs.85).aspx)
            string prologue;
            string epilogue;
            Collection<string> aces = ParseDACLACEs(sddl, out prologue, out epilogue);
            if (aces.Count == 0) { return sddl; }
            sb.Append(prologue);
            // Convert to conditional ACE
            // Beginning (Regular) ACE has exactly 6 required fields and one (optional) field.
            // We only manipulate ACEs that we create and we currently do not use the optional resource field,
            // so we always expect a beginning ACE with exactly 6 fields.
            // ace_type;ace_flags;rights;object_guid;inherit_object_guid;account_sid;(resource_attribute)
            // (https://msdn.microsoft.com/library/windows/desktop/aa374928(v=vs.85).aspx)
            // Converted (Conditional) ACE has exactly 7 required fields.  In addition the ACE type
            // is prepended with 'X' character.
            // AceType;AceFlags;Rights;ObjectGuid;InheritObjectGuid;AccountSid;(ConditionalExpression)
            // (https://msdn.microsoft.com/library/windows/desktop/dd981030(v=vs.85).aspx)
            // Beginning ACE: (A;;GA;;;BA)
            // Converted ACE: (XA;;GA;;;BA;(Member_of {SID(S-1-5-32-547)}))
            foreach (var ace in aces)
                // Parse ACE components.
                var components = ace.Split(ACESeparatorChar);
                if (components.Length != 6)
                        StringUtil.Format(RemotingErrorIdStrings.RequiredGroupsMalformedACE, ace));
                // Trim open and close parentheses from ACE string.
                components[0] = components[0].TrimStart(OpenParenChar);
                components[5] = components[5].TrimEnd(CloseParenChar);
                // Building new conditional ACE
                sb.Append(OpenParenChar);
                // Prepend the 'X' character
                var accessType = ConditionalACEPrefix + components[0];
                sb.Append(accessType + ACESeparator);
                for (int i = 1; i < 6; i++)
                    sb.Append(components[i] + ACESeparator);
                sb.Append(conditionalGroupACE);
                sb.Append(CloseParenChar);
            sb.Append(epilogue);
        private const string DACLPrefix = "D:";
        private static Collection<string> ParseDACLACEs(
            out string prologue,
            out string epilogue)
            // The format of the sddl is expected to be:
            // owner (O:), primary group (G:), DACL (D:), and SACL (S:).
            // O:NSG:BAD:P(A;;GA;;;BA)(XA;;GA;;;RM)(XA;;GA;;;IU)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)
            // prologue  = "O:NSG:BAD:P"
            // DACL ACEs = "(A;;GA;;;BA)(XA;;GA;;;RM)(XA;;GA;;;IU)"
            // epilogue  = "S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)"
            Collection<string> sddlACEs = new Collection<string>();
            // Find beginning of DACL ACEs
            int index = sddl.IndexOf(DACLPrefix, StringComparison.OrdinalIgnoreCase);
                prologue = string.Empty;
                epilogue = string.Empty;
                return sddlACEs;
            // Advance index to beginning of DACL ACEs and save prologue
            index = sddl.IndexOf(OpenParenChar, index);
            prologue = sddl.Substring(0, index);
            int sddlLength = sddl.Length;
                // Find ending of ACE
                int endIndex = sddl.IndexOf(CloseParenChar, index);
                if (endIndex < 0)
                        StringUtil.Format(RemotingErrorIdStrings.BadSDDLMismatchedParens, sddl));
                string ace = sddl.Substring(index, (endIndex - index + 1));
                sddlACEs.Add(ace);
                // Next ACE is indicated by an immediate open parenthesis character.
                index = endIndex + 1;
                if ((index >= sddlLength) || (sddl[index] != OpenParenChar)) { break; }
            // SACLs will be at the end of the sddl string.
            epilogue = (index < sddlLength) ? sddl.Substring(index, (sddlLength - index)) : string.Empty;
        #region Conditional ACE
        private const string AndOperator = "And";
        private const string OrOperator = "Or";
        private const string AndCondition = " && ";
        private const string OrCondition = " || ";
        private const string MemberOfFormat = "Member_of {{SID({0})}}";
        /// Parse RequiredGroups configuration and build conditional ACE string.
        // RequiredGroups:  @{ And = @{ Or = '2FA_GROUP_1', '2FA_GROUP_2' }, @{ Or = 'TRUSTEDHOSTS_1', 'TRUSTEDHOSTS_2' } }
        // User ACE:        (XA;;GA;;;S-1-5-21-2127438184-1604012920-1882527527;ConditionalPart)
        // ConditionalPart:   ((Member_of {SID(2FA_GROUP_1)} || Member_of {SID(2FA_GROUP_2)}) && (Member_of {SID(TRUSTEDHOSTS_1)} || Member_of {TRUSTEDHOSTS_2}))
        //         where:   2FA_GROUP_1, 2FA_GROUP_2, TRUSTEDHOSTS_1, TRUSTEDHOSTS_2 are resolved SIDs of the group names.
        internal static string CreateConditionalACEFromConfig(
            Hashtable configTable)
            if (!configTable.ContainsKey(ConfigFileConstants.RequiredGroups))
            StringBuilder conditionalACE = new StringBuilder();
            if (configTable[ConfigFileConstants.RequiredGroups] is not Hashtable requiredGroupsHash)
                throw new PSInvalidOperationException(RemotingErrorIdStrings.RequiredGroupsNotHashTable);
            if (requiredGroupsHash.Count == 0)
            // Recursively create group membership conditional ACE for each hash table key.
            AddCondition(conditionalACE, requiredGroupsHash);
            return conditionalACE.ToString();
        private static void AddCondition(
            Hashtable condition)
            // We currently support only 'And' and 'Or' logical operations for group membership
            // combinations.
            if (condition.ContainsKey(AndOperator))
                object keyValue = condition[AndOperator];
                ParseKeyValue(sb, keyValue, AndCondition);
            else if (condition.ContainsKey(OrOperator))
                object keyValue = condition[OrOperator];
                ParseKeyValue(sb, keyValue, OrCondition);
                throw new PSInvalidOperationException(RemotingErrorIdStrings.UnknownGroupMembershipKey);
        private static void ParseKeyValue(
            object keyValue,
            string logicalOperator)
            // Start of condition
            // Logical keyname value can contain:
            //  Single group name
            //      @{ Or = 'Group1' }
            //      (Member_of {SID(Group1)})
            //  Multiple group names
            //      @{ Or = 'Group1','Group2' }
            //      (Member_of {SID(Group2)} || Member_of {SID(Group2)})
            //  Single hash table
            //      @{ Or = @{ And = 'Group1' } }
            //  Multiple hash table
            //      @{ Or = @{ And = 'Group1','Group2' }, @{ And = 'Group3','Group4' } }
            //      ((Member_of {SID(Group1)} && Member_of {SID(Group2)}) || (Member_of {SID(Group3)} && Member_of {SID(Group4)}))
            //  Mixed
            //      @{ Or = 'Group1', @{ And = 'Group2','Group3' } }
            //      (Member_of {SID(Group1)} || (Member_of {SID(Group2)} && Member_of {SID(Group3)}))
            object[] values = keyValue as object[];
                int count = values.Length;
                for (int i = 0; i < count;)
                    ParseValues(sb, values[i++]);
                    if (i < count)
                        // Combine sub conditional ACEs with logical operator
                        sb.Append(logicalOperator);
                ParseValues(sb, keyValue);
            // End of condition
        private static void ParseValues(
            object inValue)
            // Value to parse can be a single object or an array of objects
            object[] values = inValue as object[];
                    ParseValue(sb, value);
                ParseValue(sb, inValue);
        private static void ParseValue(
            // Single value objects can be either a group name or a new logical hash table
            string groupName = value as string;
            if (groupName != null)
                // Resolve group name to SID
                NTAccount ntAccount = new NTAccount(groupName);
                sb.Append(StringUtil.Format(MemberOfFormat, accountSid.ToString()));
                Hashtable recurseCondition = value as Hashtable;
                if (recurseCondition != null)
                    // Recurse to handle logical hash table
                    AddCondition(sb, recurseCondition);
                    throw new PSInvalidOperationException(RemotingErrorIdStrings.UnknownGroupMembershipValue);
    #region PSCustomShellCommandBase
    /// Base class for PSCustomShell commands Register-PSSessionConfiguration, Set-PSSessionConfiguration.
    public class PSSessionConfigurationCommandBase : PSCmdlet
        internal const string NameParameterSetName = "NameParameterSet";
        internal const string AssemblyNameParameterSetName = "AssemblyNameParameterSet";
        internal const string SessionConfigurationFileParameterSetName = "SessionConfigurationFile";
        // Deny network access but allow local access
        private const string localSDDL = "O:NSG:BAD:P(D;;GA;;;NU)(A;;GA;;;BA)(A;;GA;;;IU)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)";
        private const string localSDDL_Win8 = "O:NSG:BAD:P(D;;GA;;;NU)(A;;GA;;;BA)(A;;GA;;;RM)(A;;GA;;;IU)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)"; // Win8+ only
        private const string remoteSDDL = "O:NSG:BAD:P(A;;GA;;;BA)(A;;GA;;;IU)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)";
        private const string remoteSDDL_Win8 = "O:NSG:BAD:P(A;;GA;;;BA)(A;;GA;;;RM)(A;;GA;;;IU)S:P(AU;FA;GA;;;WD)(AU;SA;GXGW;;;WD)"; // Win8 only
        // Win8 only SID for Remote Management Users group.
        internal const string RemoteManagementUsersSID = "S-1-5-32-580";
        // SID for Interactive Users group.
        internal const string InteractiveUsersSID = "S-1-5-4";
        internal Version MaxPSVersion = null;
        internal static string GetLocalSddl()
            return (Environment.OSVersion.Version >= new Version(6, 2)) ? localSDDL_Win8 : localSDDL;
        internal static string GetRemoteSddl()
            return (Environment.OSVersion.Version >= new Version(6, 2)) ? remoteSDDL_Win8 : remoteSDDL;
        /// This parameter enables the user to specify a shell name for the
        /// created custom shell.
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = NameParameterSetName)]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = AssemblyNameParameterSetName)]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = SessionConfigurationFileParameterSetName)]
            get { return shellName; }
            set { shellName = value; }
        internal string shellName;
        /// This parameter enables the user to load an Assembly and supply
        /// InitialSessionstate for each user connecting to the shell.
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = PSSessionConfigurationCommandBase.AssemblyNameParameterSetName)]
        public string AssemblyName
                assemblyName = value;
                isAssemblyNameSpecified = true;
        internal string assemblyName;
        internal bool isAssemblyNameSpecified;
        /// This parameter can accompany with AssemblyName. This supplies
        /// the directory to search while loading the assembly specified
        /// with parameter AssemblyName. Environment variables are accepted
        /// in the string value.
        [Parameter(ParameterSetName = NameParameterSetName)]
        [Parameter(ParameterSetName = AssemblyNameParameterSetName)]
        public string ApplicationBase
                return applicationBase;
                applicationBase = value;
                isApplicationBaseSpecified = true;
        internal string applicationBase;
        internal bool isApplicationBaseSpecified;
        /// This parameter should be specified with AssemblyName. This supplies
        /// the type to load to get the InitialSessionState. The type should
        /// be derived from <see cref="PSSessionConfiguration"/>.
        [Parameter(Position = 2, Mandatory = true, ParameterSetName = PSSessionConfigurationCommandBase.AssemblyNameParameterSetName)]
        public string ConfigurationTypeName
                return configurationTypeName;
                configurationTypeName = value;
                isConfigurationTypeNameSpecified = true;
        internal string configurationTypeName;
        internal bool isConfigurationTypeNameSpecified;
        /// Parameter used to specify the RunAs credentials.
        [Parameter, Credential]
                return runAsCredential;
                runAsCredential = value;
                isRunAsCredentialSpecified = true;
        internal PSCredential runAsCredential;
        internal bool isRunAsCredentialSpecified;
        /// ApartmentState of the Runspace created for the shell.
        public ApartmentState ThreadApartmentState
                    return threadAptState.Value;
                return ApartmentState.Unknown;
                threadAptState = value;
        internal ApartmentState? threadAptState;
        /// ThreadOptions of the Runspace created for the shell.
                    return threadOptions.Value;
                return PSThreadOptions.UseCurrentThread;
                threadOptions = value;
        internal PSThreadOptions? threadOptions;
        /// Set access mode.
        public PSSessionConfigurationAccessMode AccessMode
                accessModeSpecified = true;
        private PSSessionConfigurationAccessMode _accessMode = PSSessionConfigurationAccessMode.Remote;
        internal bool accessModeSpecified = false;
        /// Host mode.
                return _useSharedProcess;
                _useSharedProcess = value;
                isUseSharedProcessSpecified = true;
        private bool _useSharedProcess;
        internal bool isUseSharedProcessSpecified;
        /// Initialization script to run upon Runspace creation for this shell.
        public string StartupScript
                return configurationScript;
                configurationScript = value;
                isConfigurationScriptSpecified = true;
        internal string configurationScript;
        internal bool isConfigurationScriptSpecified;
        /// Total data (in MB) that can be received from a remote machine
        /// targeted towards a command.
        public double? MaximumReceivedDataSizePerCommandMB
                return maxCommandSizeMB;
                if ((value.HasValue) && (value.Value < 0))
                        PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.CSCDoubleParameterOutOfRange,
                            value.Value, "MaximumReceivedDataSizePerCommandMB")
                maxCommandSizeMB = value;
                isMaxCommandSizeMBSpecified = true;
        internal double? maxCommandSizeMB;
        internal bool isMaxCommandSizeMBSpecified;
        /// Maximum size (in MB) of a deserialized object received from a remote machine.
        public double? MaximumReceivedObjectSizeMB
                return maxObjectSizeMB;
                            value.Value, "MaximumReceivedObjectSizeMB")
                maxObjectSizeMB = value;
                isMaxObjectSizeMBSpecified = true;
        internal double? maxObjectSizeMB;
        internal bool isMaxObjectSizeMBSpecified;
        /// This enables the user to specify an SDDL on the shell.
        /// The default SDDL is the default used by Wsman.
                    // this will validate if the sddl is valid.
                    CommonSecurityDescriptor c = new CommonSecurityDescriptor(false, false, value);
                    // this will never be the case..as constructor either constructs or throws.
                    // this is used here to avoid FxCop violation.
                sddl = value;
                isSddlSpecified = true;
        internal string sddl;
        internal bool isSddlSpecified;
        /// Shows a UI to choose permissions/access rights for this session configuration.
        public SwitchParameter ShowSecurityDescriptorUI
                return _showUI;
                _showUI = value;
                showUISpecified = true;
        private bool _showUI;
        internal bool showUISpecified;
        /// restarting the WinRM service after the changes were
        internal bool force;
        /// If true, then the cmdlet will not attempt to restart
        /// WinRM service after completion. Typically WinRM service
        /// need to be restarted for changes to take place.
        public SwitchParameter NoServiceRestart
            get { return noRestart; }
            set { noRestart = value; }
        internal bool noRestart;
        /// Property corresponding to PSVersion parameter in the ConfigXML. This is treated as the minimum PowerShell version to load.
        /// This will allow existing endpoints creating during migration or upgrade that have PSVersion=2.0 to roll forward to PowerShell 3.0 automatically without changing the ConfigXML.
        [Alias("PowerShellVersion")]
        public Version PSVersion
                return psVersion;
                // PowerShell 7 remoting endpoints do not support PSVersion.
                throw new PSNotSupportedException(RemotingErrorIdStrings.PowerShellVersionNotSupported);
        internal Version psVersion;
        internal bool isPSVersionSpecified;
        /// SessionTypeOption.
        public PSSessionTypeOption SessionTypeOption
                return sessionTypeOption;
                sessionTypeOption = value;
        internal PSSessionTypeOption sessionTypeOption;
        /// TransportOption.
        public PSTransportOption TransportOption
                return transportOption;
                transportOption = value;
        internal PSTransportOption transportOption;
        /// ModulesToImport.
        public object[] ModulesToImport
                return modulesToImport;
                List<object> modulesToImportList = new List<object>();
                    foreach (var s in value)
                        var hashtable = s as Hashtable;
                            var moduleSpec = new ModuleSpecification(hashtable);
                                modulesToImportList.Add(moduleSpec);
                        // Getting the string value of the object if it is not a ModuleSpecification, this is required for the cases like PSObject is specified as ModulesToImport value.
                        string modulepath = s.ToString();
                        // Add this check after checking if it a path
                        if (!string.IsNullOrEmpty(modulepath.Trim()))
                            if ((modulepath.Contains('\\') || modulepath.Contains(':')) &&
                                !(Directory.Exists(modulepath) || File.Exists(modulepath)))
                                        RemotingErrorIdStrings.InvalidRegisterPSSessionConfigurationModulePath,
                                        modulepath));
                            modulesToImportList.Add(modulepath);
                modulesToImport = modulesToImportList.ToArray();
                modulePathSpecified = true;
        internal object[] modulesToImport;
        internal bool modulePathSpecified = false;
        /// Declaration initial session config file path.
        [Parameter(Mandatory = true, ParameterSetName = SessionConfigurationFileParameterSetName)]
        // Other helper variables that come along with the path
        protected bool RunAsVirtualAccount { get; set; } = false;
        protected bool RunAsVirtualAccountSpecified { get; set; } = false;
        /// Comma delimited string specifying groups a virtual account is associated with.
        protected string RunAsVirtualAccountGroups { get; set; }
        /// This is internal to make 3rd parties not derive from this cmdlet.
        internal PSSessionConfigurationCommandBase()
    #region Unregister-PSSessionConfiguration cmdlet
    /// Class implementing Unregister-PSSessionConfiguration.
    [Cmdlet(VerbsLifecycle.Unregister, RemotingConstants.PSSessionConfigurationNoun,
        SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096803")]
    public sealed class UnregisterPSSessionConfigurationCommand : PSCmdlet
        private const string removePluginSbFormat = @"
function Unregister-PSSessionConfiguration
    [CmdletBinding(SupportsShouldProcess=$true, ConfirmImpact=""Low"")]
       $filter,
       $action,
       $targetTemplate,
       $shellNotErrMsgFormat,
       [bool]$force)
        $shellsFound = 0
        Get-ChildItem 'WSMan:\localhost\Plugin\' -Force:$force | Where-Object {{ $_.Name -like ""$filter"" }} | ForEach-Object {{
            $pluginFileNamePath = join-path ""$($_.pspath)"" 'FileName'
            if (!(test-path ""$pluginFileNamePath""))
           $pluginFileName = get-item -literalpath ""$pluginFileNamePath""
           if ((!$pluginFileName) -or ($pluginFileName.Value -notmatch '{0}'))
                if (($pluginFileName.Value -match 'system32\\{0}') -OR
                    ($pluginFileName.Value -match 'syswow64\\{0}'))
                    # Filter out WindowsPowerShell endpoints when running as PowerShell 6+
           $shellsFound++
           $shouldProcessTargetString = $targetTemplate -f $_.Name
           $DISCConfigFilePath = [System.IO.Path]::Combine($_.PSPath, ""InitializationParameters"")
           $DISCConfigFile = get-childitem -literalpath ""$DISCConfigFilePath"" | Where-Object {{$_.Name -like ""configFilePath""}}
           if($null -ne $DISCConfigFile)
               if(test-path -LiteralPath ""$($DISCConfigFile.Value)"") {{
                       remove-item -literalpath ""$($DISCConfigFile.Value)"" -recurse -force -confirm:$false
           if($force -or $pscmdlet.ShouldProcess($shouldProcessTargetString, $action))
                remove-item -literalpath ""$($_.pspath)"" -recurse -force -confirm:$false
        if (!$shellsFound)
            $errMsg = $shellNotErrMsgFormat -f $filter
            Write-Error $errMsg
    }} # end of Process block
if ($null -eq $args[7])
    Unregister-PSSessionConfiguration -filter $args[0] -whatif:$args[1] -confirm:$args[2] -action $args[3] -targetTemplate $args[4] -shellNotErrMsgFormat $args[5] -force $args[6]
    Unregister-PSSessionConfiguration -filter $args[0] -whatif:$args[1] -confirm:$args[2] -action $args[3] -targetTemplate $args[4] -shellNotErrMsgFormat $args[5] -force $args[6] -erroraction $args[7]
        private static readonly ScriptBlock s_removePluginSb;
        /// This parameter enables the user to specify a shell name to
        /// remove.
                return _noRestart;
                _noRestart = value;
        private bool _noRestart;
        static UnregisterPSSessionConfigurationCommand()
            string removePluginScript = string.Format(CultureInfo.InvariantCulture,
                removePluginSbFormat,
                RemotingConstants.PSPluginDLLName);
            s_removePluginSb = ScriptBlock.Create(removePluginScript);
            s_removePluginSb.LanguageMode = PSLanguageMode.FullLanguage;
        /// Verifies if remoting cmdlets can be used.
            WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.RcsScriptMessageV, removePluginSbFormat));
            string targetTemplate = RemotingErrorIdStrings.CSShouldProcessTarget;
            string csNotFoundMessageFormat = RemotingErrorIdStrings.CustomShellNotFound;
            s_removePluginSb.InvokeUsingCmdlet(
                                           targetTemplate,
                                           csNotFoundMessageFormat,
            tracer.EndpointUnregistered(this.Name, WindowsIdentity.GetCurrent().Name);
    #region Get-PSSessionConfiguration cmdlet
    /// Class implementing Get-PSSessionConfiguration.
    [Cmdlet(VerbsCommon.Get, RemotingConstants.PSSessionConfigurationNoun, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096790")]
    [OutputType("Microsoft.PowerShell.Commands.PSSessionConfigurationCommands#PSSessionConfiguration")]
    public sealed class GetPSSessionConfigurationCommand : PSCmdlet
        // To Escape { -- {{
        // To Escape } -- }}
        private const string getPluginSbFormat = @"
function ExtractPluginProperties([string]$pluginDir, $objectToWriteTo)
    function Unescape-Xml($s) {{
        if ($s) {{
            $s = $s.Replace(""&lt;"", ""<"");
            $s = $s.Replace(""&gt;"", "">"");
            $s = $s.Replace(""&quot;"", '""');
            $s = $s.Replace(""&apos;"", ""'"");
            $s = $s.Replace(""&#39;"", ""'"");
            $s = $s.Replace(""&amp;"", ""&"");
        return $s;
    # The default comparer is case insensitive and it is supported on Core CLR.
    $h = new-object system.collections.hashtable
    function Get-Details([string]$path, [hashtable]$h) {{
        foreach ($o in (get-childitem -LiteralPath $path)) {{
            if ($o.PSIsContainer) {{
                Get-Details $o.PSPath $h
            }} else {{
                $h[$o.Name] = $o.Value
    Get-Details $pluginDir $h
    if (test-path -LiteralPath $pluginDir\InitializationParameters\SessionConfigurationData) {{
        $xscd = [xml](Unescape-xml (Unescape-xml (get-item -LiteralPath $pluginDir\InitializationParameters\SessionConfigurationData).Value))
        foreach ($o in $xscd.SessionConfigurationData.Param) {{
            if ($o.Name -eq ""PrivateData"") {{
                foreach($wf in $o.PrivateData.Param) {{
                    $h[$wf.Name] = $wf.Value
    ## Extract DISC related information
    if(test-path -LiteralPath $pluginDir\InitializationParameters\ConfigFilePath) {{
        $DISCFilePath = (get-item -LiteralPath $pluginDir\InitializationParameters\ConfigFilePath).Value
        if(test-path -LiteralPath $DISCFilePath) {{
            $DISCFileContent = get-content $DISCFilePath | out-string
            $DISCHash = invoke-expression $DISCFileContent
            foreach ($o in $DISCHash.Keys) {{
                if ($o -ne ""PowerShellVersion"") {{
                    $objectToWriteTo = $objectToWriteTo | add-member -membertype noteproperty -name $o -value $DISCHash[$o] -force -passthru
    if ($h[""SessionConfigurationData""]) {{
        $h[""SessionConfigurationData""] = Unescape-Xml (Unescape-Xml $h[""SessionConfigurationData""])
    foreach ($o in $h.Keys) {{
        if ($o -eq 'sddl') {{
            $objectToWriteTo = $objectToWriteTo | add-member -membertype noteproperty -name 'SecurityDescriptorSddl' -value $h[$o] -force -passthru
            $objectToWriteTo = $objectToWriteTo | add-member -membertype noteproperty -name $o -value $h[$o] -force -passthru
$shellNotErrMsgFormat = $args[1]
$force = $args[2]
$args[0] | ForEach-Object {{
    $shellsFound = 0;
    $filter = $_
    Get-ChildItem 'WSMan:\localhost\Plugin\' -Force:$force | ? {{ $_.name -like ""$filter"" }} | ForEach-Object {{
        $customPluginObject = new-object object
        $customPluginObject.pstypenames.Insert(0, '{0}')
        ExtractPluginProperties ""$($_.PSPath)"" $customPluginObject
        # This is powershell based custom shell only if its plugin dll is pwrshplugin.dll
        if (($customPluginObject.FileName) -and ($customPluginObject.FileName -match '{1}'))
            # Filter the endpoints based on the typeof PowerShell that is
            # executing the cmdlet. {1} in another location indicates that it
            # is a PowerShell 6+ endpoint
            if (!($customPluginObject.FileName -match 'system32\\{1}') -AND # WindowsPowerShell
                !($customPluginObject.FileName -match 'syswow64\\{1}'))     # WOW64 WindowsPowerShell
                # Add the PowerShell 6+ endpoint when running PowerShell 6+
                $customPluginObject
    }} # end of foreach
    if (!$shellsFound -and !([System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters($_)))
      $errMsg = $shellNotErrMsgFormat -f $_
        private const string MODULEPATH = "ModulesToImport";
        private static readonly ScriptBlock s_getPluginSb;
        static GetPSSessionConfigurationCommand()
            string scriptToRun = string.Format(CultureInfo.InvariantCulture,
                getPluginSbFormat,
                PSSessionConfigurationCommandUtilities.PSCustomShellTypeName,
            s_getPluginSb = ScriptBlock.Create(scriptToRun);
            s_getPluginSb.LanguageMode = PSLanguageMode.FullLanguage;
        [Parameter(Position = 0, Mandatory = false)]
        /// Force parameter.
            WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.GcsScriptMessageV, getPluginSbFormat));
            object arguments = "*";
                arguments = Name;
            ActionPreference backupPreference = Context.ErrorActionPreferenceVariable;
                    Context.ErrorActionPreferenceVariable = Context.CurrentCommandProcessor.CommandRuntime.ErrorAction;
                s_getPluginSb.InvokeUsingCmdlet(
                                                   _force}
                Context.ErrorActionPreferenceVariable = backupPreference;
    #region Set-PSSessionConfiguration cmdlet
    /// Class implementing Set-PSSessionConfiguration.
    [Cmdlet(VerbsCommon.Set, RemotingConstants.PSSessionConfigurationNoun,
       ConfirmImpact = ConfirmImpact.Medium, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096901")]
    public sealed class SetPSSessionConfigurationCommand : PSSessionConfigurationCommandBase
        private const string getSessionTypeFormat = @"(get-item 'WSMan::localhost\Plugin\{0}\InitializationParameters\sessiontype' -ErrorAction SilentlyContinue).Value";
        private const string getCurrentIdleTimeoutmsFormat = @"(Get-Item 'WSMan:\localhost\Plugin\{0}\Quotas\IdleTimeoutms').Value";
        private const string getAssemblyNameDataFormat = @"(Get-Item 'WSMan:\localhost\Plugin\{0}\InitializationParameters\assemblyname').Value";
        private const string getSessionConfigurationDataSbFormat = @"(Get-Item 'WSMan:\localhost\Plugin\{0}\InitializationParameters\SessionConfigurationData').Value";
        private const string setSessionConfigurationDataSbFormat = @"
function Set-SessionConfigurationData([string] $scd) {{
    if (test-path 'WSMan:\localhost\Plugin\{0}\InitializationParameters\" + ConfigurationDataFromXML.SESSIONCONFIGTOKEN + @"')
        set-item -WarningAction SilentlyContinue -Force 'WSMan:\localhost\Plugin\{0}\InitializationParameters\" + ConfigurationDataFromXML.SESSIONCONFIGTOKEN + @"' -Value $scd
        new-item -WarningAction SilentlyContinue -path 'WSMan:\localhost\Plugin\{0}\InitializationParameters' -paramname " + ConfigurationDataFromXML.SESSIONCONFIGTOKEN + @" -paramValue $scd -Force
Set-SessionConfigurationData $args[0]
        private const string setSessionConfigurationQuotaSbFormat = @"
function Set-SessionPluginQuota([hashtable] $quotas) {{
    foreach($v in $quotas.GetEnumerator()) {{
        $name = $v.Name;
        $value = $v.Value;
        if (!$value) {{
            $value = [string]::empty;
        set-item -WarningAction SilentlyContinue ('WSMan:\localhost\Plugin\{0}\Quotas\' + $name) -Value $value -confirm:$false
Set-SessionPluginQuota $args[0]
        private const string setSessionConfigurationTimeoutQuotasSbFormat = @"
function Set-SessionPluginIdleTimeoutQuotas([int] $maxIdleTimeoutms, [int] $idleTimeoutms, [bool] $setMaxIdleTimeoutFirst) {{
    if ($setMaxIdleTimeoutFirst) {{
        set-item -WarningAction SilentlyContinue 'WSMan:\localhost\Plugin\{0}\Quotas\MaxIdleTimeoutms' -Value $maxIdleTimeoutms -confirm:$false
        set-item -WarningAction SilentlyContinue 'WSMan:\localhost\Plugin\{0}\Quotas\IdleTimeoutms' -Value $idleTimeoutms -confirm:$false
    else {{
Set-SessionPluginIdleTimeoutQuotas $args[0] $args[1] $args[2]
        private const string setSessionConfigurationOptionsSbFormat = @"
function Set-SessionPluginOptions([hashtable] $options) {{
    if ($options[""UsedSharedProcess""]) {{
        $value = $options[""UseSharedProcess""];
        set-item -WarningAction SilentlyContinue 'WSMan:\localhost\Plugin\{0}\UseSharedProcess' -Value $value -confirm:$false
        $options.Remove(""UseSharedProcess"");
    foreach($v in $options.GetEnumerator()) {{
        $value = $v.Value
            $value = 0;
        set-item -WarningAction SilentlyContinue ('WSMan:\localhost\Plugin\{0}\' + $name) -Value $value -confirm:$false
Set-SessionPluginOptions $args[0]
        private const string setRunAsSbFormat = @"
function Set-RunAsCredential{{
        [string]$runAsUserName,
        [system.security.securestring]$runAsPassword
    $cred = new-object System.Management.Automation.PSCredential($runAsUserName, $runAsPassword)
    set-item -WarningAction SilentlyContinue 'WSMan:\localhost\Plugin\{0}\RunAsUser' $cred -confirm:$false
Set-RunAsCredential $args[0] $args[1]
        private const string setPluginSbFormat = @"
function Set-PSSessionConfiguration([PSObject]$customShellObject,
     [Array]$initParametersMap,
     [bool]$force,
     [string]$sddl,
     [bool]$isSddlSpecified,
     [bool]$shouldShowUI,
     [string]$resourceUri,
     [string]$pluginNotFoundErrorMsg,
     [string]$pluginNotPowerShellMsg,
     [string]$pluginForPowerShellCoreMsg,
     [string]$pluginForWindowsPowerShellMsg,
     [System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]$accessMode
   $wsmanPluginDir = 'WSMan:\localhost\Plugin'
   $pluginName = $customShellObject.Name;
   $pluginDir = Join-Path ""$wsmanPluginDir"" ""$pluginName""
   if ((!$pluginName) -or !(test-path ""$pluginDir""))
      Write-Error $pluginNotFoundErrorMsg
   # check if the plugin is a PowerShell plugin
   $pluginFileNamePath = Join-Path ""$pluginDir"" 'FileName'
      Write-Error $pluginNotPowerShellMsg
            Write-Error $pluginForWindowsPowerShellMsg
   # set Initialization Parameters
   $initParametersPath = Join-Path ""$pluginDir"" 'InitializationParameters'
   foreach($initParameterName in $initParametersMap)
        if ($customShellObject | get-member $initParameterName)
            $parampath = Join-Path ""$initParametersPath"" $initParameterName
            if (test-path $parampath)
               remove-item -path ""$parampath""
            # 0 is an accepted value for MaximumReceivedDataSizePerCommandMB and MaximumReceivedObjectSizeMB
            if (($customShellObject.$initParameterName) -or ($customShellObject.$initParameterName -eq 0))
               new-item -path ""$initParametersPath"" -paramname $initParameterName  -paramValue ""$($customShellObject.$initParameterName)"" -Force
   # sddl processing
   if ($isSddlSpecified)
       $resourcesPath = Join-Path ""$pluginDir"" 'Resources'
       Get-ChildItem -literalpath ""$resourcesPath"" | ForEach-Object {{
            $securityPath = Join-Path ""$($_.pspath)"" 'Security'
            if ((@(Get-ChildItem -literalpath ""$securityPath"")).count -gt 0)
                Get-ChildItem -literalpath ""$securityPath"" | ForEach-Object {{
                    $securityIDPath = ""$($_.pspath)""
                    remove-item -path ""$securityIDPath"" -recurse -force
                }} #end of securityPath
                if ($sddl)
                    new-item -path ""$securityPath"" -Sddl $sddl -force
       }} # end of resources
   }} #end of sddl processing
   elseif ($shouldShowUI)
        $null = winrm configsddl $resourceUri
   # If accessmode is Disabled, we do not bother to check the sddl
   if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Disabled.Equals($accessMode))
   $resPath = Join-Path ""$pluginDir"" 'Resources'
   Get-ChildItem -literalpath ""$resPath"" | ForEach-Object {{
                $sddlPath = Join-Path ""$($_.pspath)"" 'Sddl'
                $curSDDL = (get-item -path $sddlPath).value
                if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode) -and !$disableNetworkExists)
                if ([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Remote.Equals($accessMode) -and $disableNetworkExists)
                        # Built-in administrators
                        $iuSidId = new-object system.security.principal.securityidentifier ""{3}""
                        $sd.DiscretionaryAcl.AddAccess('Allow', $iuSidId, 268435456, 'none', 'none')
                if ($newSDDL)
                    set-item -WarningAction SilentlyContinue -path $sddlPath -value $newSDDL -force
            if (([System.Management.Automation.Runspaces.PSSessionConfigurationAccessMode]::Local.Equals($accessMode)))
                new-item -path ""$securityPath"" -Sddl ""{1}"" -force
Set-PSSessionConfiguration $args[0] $args[1] $args[2] $args[3] $args[4] $args[5] $args[6] $args[7] $args[8] $args[9] $args[10] $args[11]
        private const string initParamFormat = @"<Param Name='{0}' Value='{1}' />";
        private const string UseSharedProcessToken = "UseSharedProcess";
        private const string AllowRemoteAccessToken = "Enabled";
        private static readonly ScriptBlock s_setPluginSb;
        // property names used by the script to update InitParameters.
        private static readonly string[] s_initParametersMap = new string[] {
        private Hashtable _configTable;
        private string _configFilePath;
        private string _configSddl;
        static SetPSSessionConfigurationCommand()
            string setPluginScript = string.Format(CultureInfo.InvariantCulture,
                setPluginSbFormat,
                RemotingConstants.PSPluginDLLName, localSDDL, RemoteManagementUsersSID, InteractiveUsersSID);
            s_setPluginSb = ScriptBlock.Create(setPluginScript);
            s_setPluginSb.LanguageMode = PSLanguageMode.FullLanguage;
        #region Cmdlet overrides
            // Populate the configTable hash, and get its SDDL if needed
                _configFilePath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(Path, out provider, out drive);
                ExternalScriptInfo scriptInfo = DISCUtils.GetScriptInfoForFile(this.Context, _configFilePath, out scriptName);
                _configTable = DISCUtils.LoadConfigFile(this.Context, scriptInfo);
                if (!isSddlSpecified)
                    _configSddl = PSSessionConfigurationCommandUtilities.ComputeSDDLFromConfiguration(
                        _configTable,
                    if (!string.IsNullOrEmpty(_configSddl))
                        // Use Sddl from configuration.
                        SecurityDescriptorSddl = _configSddl;
                if (_configTable.ContainsKey(ConfigFileConstants.RunAsVirtualAccount))
                    this.RunAsVirtualAccount = LanguagePrimitives.ConvertTo<bool>(_configTable[ConfigFileConstants.RunAsVirtualAccount]);
                if (_configTable.ContainsKey(ConfigFileConstants.RunAsVirtualAccountGroups))
                        DISCPowerShellConfiguration.TryGetStringArray(_configTable[ConfigFileConstants.RunAsVirtualAccountGroups]));
            if (isSddlSpecified && accessModeSpecified)
            WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.ScsScriptMessageV, setPluginSbFormat));
                shouldProcessTarget = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessTarget,
                shouldProcessTarget = StringUtil.Format(RemotingErrorIdStrings.ScsShouldProcessTargetSDDL,
                    sddl);
            if (!noRestart && !force)
            if (!force && !ShouldProcess(shouldProcessTarget, shouldProcessAction))
            // Update the configuration to use a shared process
                    ps.AddScript(string.Format(CultureInfo.InvariantCulture, getSessionTypeFormat, CodeGeneration.EscapeSingleQuotedStringContent(Name)));
                    Collection<PSObject> psObjectCollection = ps.Invoke(new object[] { Name }) as Collection<PSObject>;
                    if (psObjectCollection == null || psObjectCollection.Count != 1)
                        Dbg.Assert(false, "This should never happen. ps.Invoke always return a Collection<PSObject>");
            // Update the PSSC file if there was one
            if (_configTable != null)
                // Extract the GUID for the configuration
                if (_configTable.ContainsKey(ConfigFileConstants.Guid))
                    sessionGuid = Guid.Parse(_configTable[ConfigFileConstants.Guid].ToString());
                // Extract the GMSA account name if available
                if (_configTable.ContainsKey(ConfigFileConstants.GMSAAccount))
                    _gmsaAccount = _configTable[ConfigFileConstants.GMSAAccount] as string;
                string destPath = System.IO.Path.Combine(Utils.DefaultPowerShellAppBase, "SessionConfig",
                // If the config file with the same guid name already exists then it would be overwritten.
                File.Copy(_configFilePath, destPath, true);
            string shellNotFoundErrorMsg = StringUtil.Format(RemotingErrorIdStrings.CSCmdsShellNotFound, shellName);
            string shellNotPowerShellMsg = StringUtil.Format(RemotingErrorIdStrings.CSCmdsShellNotPowerShellBased, shellName);
            string shellForPowerShellCoreMsg = StringUtil.Format(RemotingErrorIdStrings.CSCmdsPowerShellCoreShellNotModifiable, shellName);
            string shellForWindowsPowerShellMsg = StringUtil.Format(RemotingErrorIdStrings.CSCmdsWindowsPowerShellCoreNotModifiable, shellName);
            // construct object to update the properties
            PSObject propertiesToUpdate = ConstructPropertiesForUpdate();
            // hack to see if there is any error reported running the script.
            s_setPluginSb.InvokeUsingCmdlet(
                                               propertiesToUpdate,
                                               s_initParametersMap,
                                               sddl,
                                               shellNotFoundErrorMsg,
                                               shellNotPowerShellMsg,
                                               shellForPowerShellCoreMsg,
                                               shellForWindowsPowerShellMsg,
                                               accessModeSpecified ? AccessMode : PSSessionConfigurationAccessMode.Disabled,
            if (errorList.Count > errorCountBefore)
                _isErrorReported = true;
            SetSessionConfigurationTypeOptions();
            SetQuotas();
            SetRunAs();
            SetVirtualAccount();
            // Move the WinRM service to its own service host if the endpoint is given elevated credentials
            SetOptions();
        private void SetRunAs()
            if (this.runAsCredential == null)
                if (string.IsNullOrEmpty(_gmsaAccount)) { return; }
            ScriptBlock setRunAsSbFormatSb = ScriptBlock.Create(
                string.Format(CultureInfo.InvariantCulture, setRunAsSbFormat, CodeGeneration.EscapeSingleQuotedStringContent(Name)));
            setRunAsSbFormatSb.LanguageMode = PSLanguageMode.FullLanguage;
            setRunAsSbFormatSb.InvokeUsingCmdlet(
                            runAsCredential.UserName,
                            runAsCredential.Password,
        private void SetVirtualAccount()
            if (!this.RunAsVirtualAccountSpecified)
                string pluginLocation = StringUtil.Format(@"WSMAN:\localhost\plugin\{0}\RunAsVirtualAccount", Name);
                invoker.AddCommand("Set-Item").AddParameter("Path", pluginLocation).AddParameter("Value", this.RunAsVirtualAccount);
                invoker.Invoke();
        private void SetQuotas()
            if (this.transportOption != null)
                ScriptBlock setQuotasSb = ScriptBlock.Create(
                       string.Format(CultureInfo.InvariantCulture, setSessionConfigurationQuotaSbFormat, CodeGeneration.EscapeSingleQuotedStringContent(Name)));
                setQuotasSb.LanguageMode = PSLanguageMode.FullLanguage;
                Hashtable quotas = transportOption.ConstructQuotasAsHashtable();
                setQuotasSb.InvokeUsingCmdlet(
                    args: new object[] { quotas, });
        private void SetOptions()
            ScriptBlock setOptionsSb = ScriptBlock.Create(
                   string.Format(CultureInfo.InvariantCulture, setSessionConfigurationOptionsSbFormat, CodeGeneration.EscapeSingleQuotedStringContent(Name)));
            setOptionsSb.LanguageMode = PSLanguageMode.FullLanguage;
            Hashtable optionsTable
                = transportOption != null
                ? transportOption.ConstructOptionsAsHashtable()
                : new Hashtable();
            if (accessModeSpecified)
                        optionsTable[AllowRemoteAccessToken] = false.ToString();
                        optionsTable[AllowRemoteAccessToken] = true.ToString();
                optionsTable[UseSharedProcessToken] = UseSharedProcess.ToBool().ToString();
            setOptionsSb.InvokeUsingCmdlet(
                            optionsTable,
        private void SetSessionConfigurationTypeOptions()
                    ps.AddScript(string.Format(CultureInfo.InvariantCulture, getSessionConfigurationDataSbFormat, CodeGeneration.EscapeSingleQuotedStringContent(Name)));
                        Dbg.Assert(false, "This should never happen.  Plugin must exist because caller code has already checked this.");
                    PSSessionConfigurationData scd = PSSessionConfigurationData.Create(psObjectCollection[0] == null ? string.Empty : PSSessionConfigurationData.Unescape(psObjectCollection[0].BaseObject.ToString()));
                    PSSessionTypeOption original = sessionTypeOption.ConstructObjectFromPrivateData(scd.PrivateData);
                    original.CopyUpdatedValuesFrom(sessionTypeOption);
                    string modulePathParameter = null;
                    bool unsetModulePath = false;
                    if (modulePathSpecified)
                        if (modulesToImport == null ||
                            modulesToImport.Length == 0 ||
                            (modulesToImport.Length == 1 && modulesToImport[0] is string && ((string)modulesToImport[0]).Equals(string.Empty, StringComparison.OrdinalIgnoreCase)))
                            unsetModulePath = true;
                            modulePathParameter = PSSessionConfigurationCommandUtilities.GetModulePathAsString(this.modulesToImport).Trim();
                    // If the ModulesToImport parameter is not specified, or it is specified, but modulePathParameter turns out to be an empty string,
                    // we use the original module path
                    if (!unsetModulePath && string.IsNullOrEmpty(modulePathParameter))
                        modulePathParameter = (scd.ModulesToImportInternal == null) ? null : PSSessionConfigurationCommandUtilities.GetModulePathAsString(scd.ModulesToImportInternal.ToArray()).Trim();
                    // 1. unsetModulePath is true. In this case, modulePathParameter is definitely null
                    // 2. unsetModulePath is false.
                    //    a. modulePathSpecified is false. In this case, modulePathParameter will be the original module path,
                    //       and it's null or empty when the original module path is null or empty
                    //    b. modulePathSpecified is true. In this case, the user specified modulePathParameter is empty after trim(),
                    //       and it will be the original module path. It's null or empty when the original module path is null or empty.
                    if (unsetModulePath || string.IsNullOrEmpty(modulePathParameter))
                                PSSessionConfigurationData.ModulesToImportToken, string.Empty));
                    // unsetModulePath is false AND modulePathParameter is not empty.
                    // 1. modulePathSpecified is false. In this case, modulePathParameter will be the original module path.
                    // 2. modulePathSpecified is true. In this case, the user is not unsetting the module path.
                    //    a. the user specified module path is not empty.
                    //    b. the user specified module path is empty after trim(), and modulePathParameter will be the original module path.
                                modulePathParameter));
                    string privateData = original.ConstructPrivateData();
                        ScriptBlock setSessionConfigurationSb = ScriptBlock.Create(
                            string.Format(CultureInfo.InvariantCulture, setSessionConfigurationDataSbFormat, CodeGeneration.EscapeSingleQuotedStringContent(Name))
                        setSessionConfigurationSb.LanguageMode = PSLanguageMode.FullLanguage;
                        setSessionConfigurationSb.InvokeUsingCmdlet(
            PSSessionConfigurationCommandUtilities.RestartWinRMService(this, _isErrorReported, Force, noRestart);
            if (!_isErrorReported && noRestart)
                string action = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, this.CommandInfo.Name);
                WriteWarning(StringUtil.Format(RemotingErrorIdStrings.WinRMRequiresRestart, action));
            tracer.EndpointModified(this.Name, WindowsIdentity.GetCurrent().Name);
        private PSObject ConstructPropertiesForUpdate()
            result.Properties.Add(new PSNoteProperty("Name", shellName));
            if (isAssemblyNameSpecified)
                result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.ASSEMBLYTOKEN, assemblyName));
            if (isApplicationBaseSpecified)
                result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.APPBASETOKEN, applicationBase));
            if (isConfigurationTypeNameSpecified)
                result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.SHELLCONFIGTYPETOKEN, configurationTypeName));
            if (isConfigurationScriptSpecified)
                result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.STARTUPSCRIPTTOKEN, configurationScript));
            if (isMaxCommandSizeMBSpecified)
                object input = maxCommandSizeMB.HasValue ? (object)maxCommandSizeMB.Value : null;
                result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.MAXRCVDCMDSIZETOKEN, input));
            if (isMaxObjectSizeMBSpecified)
                object input = maxObjectSizeMB.HasValue ? (object)maxObjectSizeMB.Value : null;
                result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.MAXRCVDOBJSIZETOKEN, input));
                result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.THREADAPTSTATETOKEN, threadAptState.Value));
                result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.THREADOPTIONSTOKEN, threadOptions.Value));
            if (isPSVersionSpecified)
                result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.PSVERSIONTOKEN, PSSessionConfigurationCommandUtilities.ConstructVersionFormatForConfigXml(psVersion)));
                // We add MaxPSVersion to the result irrespective of whether the string is empty or not.
                // This is done to cover the following scenario
                // Register-PSSessionConfiguration -Name "blah" -PSVersion 2
                //      followed by a
                // Set-PSSessionConfiguration -Name "blah" -PSVersion 3
                // If you create an end point with version 2 and then update it to 3, then the MaxPsVersion parameter should be removed from config xml
                // So, we create a MaxPSVersion property with no value.
                // In function Set-PSSessionConfiguration, the registry item MaxPSVersion will be removed.
                // But it will not be created since the value is empty.
                result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.MAXPSVERSIONTOKEN, PSSessionConfigurationCommandUtilities.ConstructVersionFormatForConfigXml(MaxPSVersion)));
            if (modulePathSpecified && sessionTypeOption == null)
                if (modulesToImport == null || modulesToImport.Length == 0 ||
                if (unsetModulePath || !string.IsNullOrEmpty(modulePathParameter))
                        // Get the SessionConfigurationDataFormat
                        // SessionConfigurationData doesn't exist in InitializationParameters
                        if (psObjectCollection[0] == null)
                            // if unsetModulePath is false, we set the new sessionConfigurationData to contain the specified module path
                            if (!unsetModulePath)
                            // if unsetModulePath is true, we don't need to do anything because ModulesToImport doesn't exist
                            // in the original configuration. If it's a workflow config, it's not a valid one.
                        // SessionConfigurationData exists in InitializationParameters
                            PSSessionConfigurationData scd = PSSessionConfigurationData.Create(psObjectCollection[0].BaseObject.ToString());
                            string privateData = string.IsNullOrEmpty(scd.PrivateData)
                                                     : scd.PrivateData.Replace('"', '\'');
                            // The user tries to unset the module path)
                            if (unsetModulePath)
                                // ModulesToImport exist in the pssessionConfigurationData
                                if (scd.ModulesToImportInternal != null && scd.ModulesToImportInternal.Count != 0)
                                // if ModulesToImport doesn't exist in the pssessionConfigurationData, we don't need to do anything.
                                // in this case, if the current config is of type workflow, it's not a valid config.
                                // Replace the old module path
                            result.Properties.Add(new PSNoteProperty(ConfigurationDataFromXML.SESSIONCONFIGTOKEN, encodedSessionConfigData));
                    string message = StringUtil.Format(RemotingErrorIdStrings.InvalidPSSessionConfigurationFilePath, Path);
                ExternalScriptInfo scriptInfo = DISCUtils.GetScriptInfoForFile(this.Context, filePath, out scriptName);
                Hashtable configTable = DISCUtils.LoadConfigFile(this.Context, scriptInfo);
                foreach (object currentKey in configTable.Keys)
                    if (result.Properties[currentKey.ToString()] == null)
                        result.Properties.Add(new PSNoteProperty(currentKey.ToString(), configTable[currentKey]));
                        result.Properties[currentKey.ToString()].Value = configTable[currentKey];
    #region Enable/Disable-PSSessionConfiguration
    /// Class implementing Enable-PSSessionConfiguration cmdlet.
    [Cmdlet(VerbsLifecycle.Enable, RemotingConstants.PSSessionConfigurationNoun,
        SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096785")]
    public sealed class EnablePSSessionConfigurationCommand : PSCmdlet
        private const string setWSManConfigCommand = "Set-WSManQuickConfig";
        private const string enablePluginSbFormat = @"
function Test-WinRMQuickConfigNeeded
    # see issue #11005 - Function Test-WinRMQuickConfigNeeded needs to be updated:
    # 1) currently this function always returns $True
    # 2) checking for a firewall rule using Get-NetFirewallRule engages WinCompat code and has significant perf impact on Enable-PSRemoting; maybe change to Get-CimInstance -ClassName MSFT_NetFirewallRule
    return $True
# Checking the following items
#1. Starting or restarting (if already started) the WinRM service
#2. Setting the WinRM service startup type to Automatic
#3. Creating a listener to accept requests on any IP address
#4. Enabling Windows Firewall inbound rule exceptions for WS-Management traffic (for http only).
    $winrmQuickConfigNeeded = $false
    # check if WinRM service is running
    if ((Get-Service winrm).Status -ne 'Running'){{
        $winrmQuickConfigNeeded = $true
    # check if WinRM service startup is Auto
    elseif ((Get-CimInstance -Query ""Select StartMode From Win32_Service Where Name='winmgmt'"").StartMode -ne 'Auto'){{
    # check if a winrm listener is present
    elseif (!(Test-Path WSMan:\localhost\Listener) -or ($null -eq (Get-ChildItem WSMan:\localhost\Listener))){{
    # check if WinRM firewall is enabled for HTTP
    else{{
        if (Get-Command Get-NetFirewallRule -ErrorAction SilentlyContinue){{
            $winrmFirewall = Get-NetFirewallRule -Name 'WINRM-HTTP-In-TCP' -ErrorAction SilentlyContinue
            if (!$winrmFirewall -or $winrmFirewall.Enabled -ne $true){{
    $winrmQuickConfigNeeded
function Enable-PSSessionConfiguration
    [Parameter(Position=0, ValueFromPipeline=$true)]
    $sddl,
    $isSDDLSpecified,
    $queryForSet,
    $captionForSet,
    $queryForQC,
    $captionForQC,
    $shouldProcessDescForQC,
    $setEnabledTarget,
    $setEnabledAction,
    $skipNetworkProfileCheck,
    $noServiceRestart
        $winrmQuickConfigNeeded = Test-WinRMQuickConfigNeeded
        if ($winrmQuickConfigNeeded -and ($force -or $pscmdlet.ShouldProcess($shouldProcessDescForQC, $queryForQC, $captionForQC)))
            # get the status of winrm before Quick Config. if it is already
            # running..restart the service after Quick Config.
            $svc = get-service winrm
            if ($skipNetworkProfileCheck)
                {0} -force -SkipNetworkProfileCheck
                {0} -force
            if ($svc.Status -match ""Running"")
               Restart-Service winrm -force -confirm:$false
       Get-PSSessionConfiguration $name -Force:$Force | ForEach-Object {{
          if ($_.Enabled -eq $false -and ($force -or $pscmdlet.ShouldProcess($setEnabledTarget, $setEnabledAction)))
             Set-Item -WarningAction SilentlyContinue -Path ""WSMan:\localhost\Plugin\$name\Enabled"" -Value $true -confirm:$false
          if (!$isSDDLSpecified)
             $sddlTemp = $null
                 $sddlTemp = $_.psobject.members[""SecurityDescriptorSddl""].Value
             # strip out Disable-Everyone DACL from the SDDL
             if ($sddlTemp)
                # construct SID for ""EveryOne""
                [system.security.principal.wellknownsidtype]$evst = ""worldsid""
                $everyOneSID = new-object system.security.principal.securityidentifier $evst,$null
                $sd = new-object system.security.accesscontrol.commonsecuritydescriptor $false,$false,$sddlTemp
                    if (($_.acequalifier -eq ""accessdenied"") -and ($_.securityidentifier -match $everyOneSID))
                if ($securityIdentifierToPurge)
                   $sd.discretionaryacl.purge($securityIdentifierToPurge)
                          $rmSidId = new-object system.security.principal.securityidentifier ""{1}""
                      $iuSidId = new-object system.security.principal.securityidentifier ""{2}""
                   $sddl = $sd.GetSddlForm(""all"")
             }} # if ($sddlTemp)
          }} # if (!$isSDDLSpecified)
          $qMessage = $queryForSet -f $_.name,$sddl
          if (($sddl -or $isSDDLSpecified) -and ($force -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))
              $null = Set-PSSessionConfiguration -Name $_.Name -SecurityDescriptorSddl $sddl -NoServiceRestart -force -WarningAction 0
       }} #end of Get-PSSessionConfiguration | foreach
$_ | Enable-PSSessionConfiguration -force $args[0] -sddl $args[1] -isSDDLSpecified $args[2] -queryForSet $args[3] -captionForSet $args[4] -queryForQC $args[5] -captionForQC $args[6] -whatif:$args[7] -confirm:$args[8] -shouldProcessDescForQC $args[9] -setEnabledTarget $args[10] -setEnabledAction $args[11] -skipNetworkProfileCheck $args[12] -noServiceRestart $args[13]
        private static readonly ScriptBlock s_enablePluginSb;
        static EnablePSSessionConfigurationCommand()
            string enablePluginScript = string.Format(CultureInfo.InvariantCulture,
                enablePluginSbFormat, setWSManConfigCommand, PSSessionConfigurationCommandBase.RemoteManagementUsersSID, PSSessionConfigurationCommandBase.InteractiveUsersSID);
            s_enablePluginSb = ScriptBlock.Create(enablePluginScript);
            s_enablePluginSb.LanguageMode = PSLanguageMode.FullLanguage;
        /// Configurations to Enable.
        private readonly Collection<string> _shellsToEnable = new Collection<string>();
        /// configuring the WinRM and enabling the session configurations
        /// without prompting the user.
        /// This enables the user to specify an SDDL for whom the session
        /// configuration is enabled.
        /// Property that will allow configuring WinRM with Public
        /// profile exception enabled.
            get { return _skipNetworkProfileCheck; }
            set { _skipNetworkProfileCheck = value; }
        private bool _skipNetworkProfileCheck;
        /// needs to be restarted for changes to take place.
            get { return _noRestart; }
            set { _noRestart = value; }
                foreach (string shell in Name)
                    _shellsToEnable.Add(shell);
            // if user did not specify any shell, act on the default shell.
            if (_shellsToEnable.Count == 0)
                _shellsToEnable.Add(PSSessionConfigurationCommandUtilities.GetWinrmPluginShellName());
            WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.EcsScriptMessageV, enablePluginSbFormat));
            string qcCaptionMessage = StringUtil.Format(RemotingErrorIdStrings.EcsWSManQCCaption);
            string qcQueryMessage = StringUtil.Format(RemotingErrorIdStrings.EcsWSManQCQuery, setWSManConfigCommand);
            string qcShouldProcessDesc = StringUtil.Format(RemotingErrorIdStrings.EcsWSManShouldProcessDesc, setWSManConfigCommand);
            string setCaptionMessage = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction,
                "Set-PSSessionConfiguration");
            string setQueryMessage = RemotingErrorIdStrings.EcsShouldProcessTarget;
            string setEnabledAction = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, "Set-Item");
            string setEnabledTarget = RemotingErrorIdStrings.SetEnabledTrueTarget;
            s_enablePluginSb.InvokeUsingCmdlet(
                dollarUnder: _shellsToEnable,
                                               setQueryMessage,
                                               setCaptionMessage,
                                               qcQueryMessage,
                                               qcCaptionMessage,
                                               qcShouldProcessDesc,
                                               setEnabledTarget,
                                               setEnabledAction,
                                               _skipNetworkProfileCheck,
                                               _noRestart});
            foreach (string endPointName in Name ?? Array.Empty<string>())
                sb.Append(endPointName);
                sb.Remove(sb.Length - 2, 2);
            tracer.EndpointEnabled(sb.ToString(), WindowsIdentity.GetCurrent().Name);
    [Cmdlet(VerbsLifecycle.Disable, RemotingConstants.PSSessionConfigurationNoun,
        SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Low, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096692")]
    public sealed class DisablePSSessionConfigurationCommand : PSCmdlet
        private const string disablePluginSbFormat = @"
function Disable-PSSessionConfiguration
    [Parameter(Position=0, ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]
    $restartWinRMMessage,
        if ($force -or $pscmdlet.ShouldProcess($restartWinRMMessage))
            if ($svc.Status -match ""Stopped"")
           if ($_.Enabled -and ($force -or $pscmdlet.ShouldProcess($setEnabledTarget, $setEnabledAction)))
                Set-Item -WarningAction SilentlyContinue -Path ""WSMan:\localhost\Plugin\$name\Enabled"" -Value $false -Force -Confirm:$false
       }} # end of foreach block
    }} #end of process block
    # no longer necessary to restart the winrm to apply the config change
$_ | Disable-PSSessionConfiguration -force $args[0] -whatif:$args[1] -confirm:$args[2] -restartWinRMMessage $args[3] -setEnabledTarget $args[4] -setEnabledAction $args[5] -noServiceRestart $args[6]
        private static readonly ScriptBlock s_disablePluginSb;
        static DisablePSSessionConfigurationCommand()
            string disablePluginScript = string.Format(CultureInfo.InvariantCulture,
                disablePluginSbFormat);
            s_disablePluginSb = ScriptBlock.Create(disablePluginScript);
            s_disablePluginSb.LanguageMode = PSLanguageMode.FullLanguage;
        private readonly Collection<string> _shellsToDisable = new Collection<string>();
                    _shellsToDisable.Add(shell);
            if (_shellsToDisable.Count == 0)
                _shellsToDisable.Add(PSSessionConfigurationCommandUtilities.GetWinrmPluginShellName());
            // WriteWarning(StringUtil.Format(RemotingErrorIdStrings.DcsWarningMessage));
            WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.EcsScriptMessageV, disablePluginSbFormat));
            string restartWinRMMessage = RemotingErrorIdStrings.RestartWinRMMessage;
            string setEnabledTarget = RemotingErrorIdStrings.SetEnabledFalseTarget;
            s_disablePluginSb.InvokeUsingCmdlet(
                dollarUnder: _shellsToDisable,
                                               restartWinRMMessage,
            tracer.EndpointDisabled(sb.ToString(), WindowsIdentity.GetCurrent().Name);
    #region Enable-PSRemoting
    [Cmdlet(VerbsLifecycle.Enable, RemotingConstants.PSRemotingNoun,
        SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096577")]
    public sealed class EnablePSRemotingCommand : PSCmdlet
        // TODO: CLR4: Remove the logic for setting the MaxMemoryPerShellMB to 200 MB once IPMO->Get-Command->Get-Help memory usage issue is fixed.
        private const string enableRemotingSbFormat = @"
Set-StrictMode -Version Latest
function New-PluginConfigFile
    [Parameter()] [string] $pluginInstallPath
    $pluginConfigFile = Join-Path $pluginInstallPath ""RemotePowerShellConfig.txt""
    # This always overwrites the file with a new version of it (if it already exists)
    Set-Content -Path $pluginConfigFile -Value ""PSHOMEDIR=$PSHOME"" -ErrorAction Stop
    Add-Content -Path $pluginConfigFile -Value ""CORECLRDIR=$PSHOME"" -ErrorAction Stop
function Copy-PluginToEndpoint
    [Parameter()] [string] $endpointDir
    $resolvedPluginInstallPath = """"
    $pluginInstallPath = Join-Path ([System.Environment]::GetFolderPath([System.Environment+SpecialFolder]::Windows) + ""\System32\PowerShell"") $endpointDir
    if (!(Test-Path $pluginInstallPath))
        $resolvedPluginInstallPath = New-Item -Type Directory -Path $pluginInstallPath
        $resolvedPluginInstallPath = Resolve-Path $pluginInstallPath
    if (!(Test-Path $resolvedPluginInstallPath\{4}))
        Copy-Item -Path $PSHOME\{4} -Destination $resolvedPluginInstallPath -Force -ErrorAction Stop
            Write-Error ($errorMsgUnableToInstallPlugin -f ""{4}"", $resolvedPluginInstallPath)
            return $null
    return $resolvedPluginInstallPath
function Register-Endpoint
    [Parameter()] [string] $configurationName
    # Section 1:
    # Move pwrshplugin.dll from $PSHOME to the endpoint directory
    # The plugin directory pattern for endpoint configuration is:
    # '$env:WINDIR\System32\PowerShell\' + powershell_version,
    # so we call Copy-PluginToEndpoint function only with the PowerShell version argument.
    $pwshVersion = $configurationName.Replace(""PowerShell."", """")
    $resolvedPluginInstallPath = Copy-PluginToEndpoint $pwshVersion
    if (!$resolvedPluginInstallPath) {{
    # Section 2:
    # Generate the Plugin Configuration File
    New-PluginConfigFile $resolvedPluginInstallPath
    # Section 3:
    # Register the endpoint
    $null = Register-PSSessionConfiguration -Name $configurationName -force -ErrorAction Stop
    set-item -WarningAction SilentlyContinue wsman:\localhost\plugin\$configurationName\Quotas\MaxShellsPerUser -value ""25"" -confirm:$false
    set-item -WarningAction SilentlyContinue wsman:\localhost\plugin\$configurationName\Quotas\MaxIdleTimeoutms -value {3} -confirm:$false
    restart-service winrm -confirm:$false
function Register-EndpointIfNotPresent
    [Parameter()] [string] $Name,
    [Parameter()] [bool] $Force,
    [Parameter()] [string] $queryForRegisterDefault,
    [Parameter()] [string] $captionForRegisterDefault
    # This cmdlet will make sure default powershell end points exist upon successful completion.
    # Windows PowerShell:
    #   Microsoft.PowerShell
    #   Microsoft.PowerShell32 (wow64)
    # PowerShell:
    #   PowerShell.<version ID>
    $errorCount = $error.Count
    $endPoint = Get-PSSessionConfiguration $Name -Force:$Force -ErrorAction silentlycontinue 2>&1
    $newErrorCount = $error.Count
    # remove the 'No Session Configuration matches criteria' errors
    for ($index = 0; $index -lt ($newErrorCount - $errorCount); $index ++)
        $error.RemoveAt(0)
    $qMessage = $queryForRegisterDefault -f ""$Name"",""Register-PSSessionConfiguration {0} -force""
    if ((!$endpoint) -and
        ($force  -or $pscmdlet.ShouldProcess($qMessage, $captionForRegisterDefault)))
        Register-Endpoint $Name
function Enable-PSRemoting
    [Parameter()] [string] $captionForRegisterDefault,
    [Parameter()] [string] $queryForSet,
    [Parameter()] [string] $captionForSet,
    [Parameter()] [bool] $skipNetworkProfileCheck,
    [Parameter()] [string] $errorMsgUnableToInstallPlugin
        # Enable all Session Configurations
            $null = $PSBoundParameters.Remove(""queryForRegisterDefault"")
            $null = $PSBoundParameters.Remove(""captionForRegisterDefault"")
            $null = $PSBoundParameters.Remove(""queryForSet"")
            $null = $PSBoundParameters.Remove(""captionForSet"")
            $null = $PSBoundParameters.Remove(""errorMsgUnableToInstallPlugin"")
            $PSBoundParameters.Add(""Name"",""*"")
            # first try to enable all the sessions
            Enable-PSSessionConfiguration @PSBoundParameters
            Register-EndpointIfNotPresent -Name {0} $Force $queryForRegisterDefault $captionForRegisterDefault
            # Create the default PSSession configuration, not tied to specific PowerShell version
            # e. g. 'PowerShell.6'.
            $powershellVersionMajor = $PSVersionTable.PSVersion.ToString()
            $dotPos = $powershellVersionMajor.IndexOf(""."")
            if ($dotPos -ne -1) {{
                $powershellVersionMajor = $powershellVersionMajor.Substring(0, $dotPos)
            # If we are running a Preview version, we don't want to clobber the generic PowerShell.6 endpoint
            # but instead create a PowerShell.6-Preview endpoint
            if ($PSVersionTable.PSVersion.PreReleaseLabel)
                $powershellVersionMajor += ""-preview""
            Register-EndpointIfNotPresent -Name (""PowerShell."" + $powershellVersionMajor) $Force $queryForRegisterDefault $captionForRegisterDefault
            # remove the 'network deny all' tag
            Get-PSSessionConfiguration -Force:$Force | ForEach-Object {{
                            # Built-in administrators.
                            $iaSidId = new-object system.security.principal.securityidentifier ""{2}""
                }} ## end of if($sddl)
                if (($sddl) -and ($force -or $pscmdlet.ShouldProcess($qMessage, $captionForSet)))
            }} ## end of foreach-object
        catch {{
        }}  # end of catch
    }} # end of end block
}} # end of Enable-PSRemoting
Enable-PSRemoting -force $args[0] -queryForRegisterDefault $args[1] -captionForRegisterDefault $args[2] -queryForSet $args[3] -captionForSet $args[4] -whatif:$args[5] -confirm:$args[6] -skipNetworkProfileCheck $args[7] -errorMsgUnableToInstallPlugin $args[8]
        private static readonly ScriptBlock s_enableRemotingSb;
        static EnablePSRemotingCommand()
            string enableRemotingScript = string.Format(CultureInfo.InvariantCulture,
                enableRemotingSbFormat, PSSessionConfigurationCommandUtilities.GetWinrmPluginShellName(),
                PSSessionConfigurationCommandBase.RemoteManagementUsersSID, PSSessionConfigurationCommandBase.InteractiveUsersSID,
                RemotingConstants.MaxIdleTimeoutMS, RemotingConstants.PSPluginDLLName);
            s_enableRemotingSb = ScriptBlock.Create(enableRemotingScript);
            s_enableRemotingSb.LanguageMode = PSLanguageMode.FullLanguage;
            WriteWarning(RemotingErrorIdStrings.PSCoreRemotingEnableWarning);
            string captionMessage = RemotingErrorIdStrings.ERemotingCaption;
            string queryMessage = RemotingErrorIdStrings.ERemotingQuery;
            string setCaptionMessage = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, "Set-PSSessionConfiguration");
            s_enableRemotingSb.InvokeUsingCmdlet(
                                               queryMessage,
                                               captionMessage,
                                               RemotingErrorIdStrings.UnableToInstallPlugin});
    #region Disable-PSRemoting
    /// Disable-PSRemoting cmdlet
    /// Only disable the network access to the Session Configuration. The
    /// local access is still enabled.
    [Cmdlet(VerbsLifecycle.Disable, RemotingConstants.PSRemotingNoun,
        SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096482")]
    public sealed class DisablePSRemotingCommand : PSCmdlet
        private const string disablePSRemotingFormat = @"
function Disable-PSRemoting
    $force,
    $restartWinRMMessage
        if ($pscmdlet.ShouldProcess($restartWinRMMessage))
    }} # end of begin block
        # Disable the network for all Session Configurations
        Get-PSSessionConfiguration -Force:$force | ForEach-Object {{
                if (!$sddl)
                    # Disable network users from accessing this configuration
                    $sddl = ""{0}""
                    # Add disable network to the existing sddl
                    if (!$disableNetworkExists)
                        # since disable network GA already exists, we dont need to change anything.
                }} ## end of if(!$sddl)
            }} ## end of if($_.Enabled)
        }} ## end of %
    }} ## end of Process block
Disable-PSRemoting -force:$args[0] -queryForSet $args[1] -captionForSet $args[2] -restartWinRMMessage $args[3] -whatif:$args[4] -confirm:$args[5]
        private static readonly ScriptBlock s_disableRemotingSb;
        #endregion Private Data
        static DisablePSRemotingCommand()
            string localSDDL = PSSessionConfigurationCommandBase.GetLocalSddl();
            string disableRemotingScript = string.Format(CultureInfo.InvariantCulture, disablePSRemotingFormat, localSDDL);
            s_disableRemotingSb = ScriptBlock.Create(disableRemotingScript);
            s_disableRemotingSb.LanguageMode = PSLanguageMode.FullLanguage;
        #region Cmdlet Override
        /// Check for prerequisites and elevation mode.
        /// Invoke Disable-PSRemoting.
            WriteWarning(RemotingErrorIdStrings.PSCoreRemotingDisableWarning);
            WriteWarning(StringUtil.Format(RemotingErrorIdStrings.DcsWarningMessage));
            WriteVerbose(StringUtil.Format(RemotingErrorIdStrings.EcsScriptMessageV, disablePSRemotingFormat));
            string captionMessage = StringUtil.Format(RemotingErrorIdStrings.CSShouldProcessAction, "Set-PSSessionConfiguration");
            string queryMessage = RemotingErrorIdStrings.DisableRemotingShouldProcessTarget;
            s_disableRemotingSb.InvokeUsingCmdlet(
                                               confirm});
        #endregion Cmdlet Override
    #region Get-PSSessionCapability
    /// Gets the capabilities of a constrained endpoint on the local machine for a specific user.
    [Cmdlet(VerbsCommon.Get, "PSSessionCapability", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=623709")]
    [OutputType(new Type[] { typeof(System.Management.Automation.CommandInfo), typeof(System.Management.Automation.Runspaces.InitialSessionState) })]
    public sealed class GetPSSessionCapabilityCommand : PSCmdlet
        /// Gets or sets the session name that should be queried for its session capabilities.
        public string ConfigurationName { get; set; }
        /// Gets or sets the user name that should be applied to the session.
        public string Username { get; set; }
        /// Gets or sets the switch that determines whether just the commands should be returned,
        /// or the entire Initial Session State.
        public SwitchParameter Full { get; set; }
            // Validate that WinRM is OK and that the user can query system state
            // Get the configuration for the given endpoint
            Collection<PSObject> configurations = null;
            using (PowerShellApi invoker = PowerShellApi.Create(RunspaceMode.CurrentRunspace))
                invoker.AddCommand("Get-PSSessionConfiguration").AddParameter("Name", this.ConfigurationName).AddParameter("ErrorAction", "Stop");
                    // If the session name doesn't exist, this Invoke() throws
                    configurations = invoker.Invoke();
                    ThrowTerminatingError(new ErrorRecord(e.ErrorRecord.Exception, "CouldNotFindSessionConfiguration", ErrorCategory.ObjectNotFound, this.ConfigurationName));
            // The validator that will be applied to the role lookup
            Func<string, bool> validator = static (role) => true;
            if (!string.IsNullOrEmpty(this.Username))
                if (this.Username.Contains('\\'))
                    validator = null;
                    // Convert DOMAIN\user to the upn (user@DOMAIN)
                    string[] upnComponents = this.Username.Split('\\');
                    if (upnComponents.Length == 2)
                        this.Username = upnComponents[1] + "@" + upnComponents[0];
                    System.Security.Principal.WindowsPrincipal windowsPrincipal = new System.Security.Principal.WindowsPrincipal(
                        new System.Security.Principal.WindowsIdentity(this.Username));
                    validator = (role) => windowsPrincipal.IsInRole(role);
                    // Identity could not be mapped
                    string message = StringUtil.Format(RemotingErrorIdStrings.CouldNotResolveUsername, this.Username);
                    ArgumentException ioe = new ArgumentException(message, e);
                    ErrorRecord er = new ErrorRecord(ioe, "CouldNotResolveUsername", ErrorCategory.InvalidArgument, this.Username);
            foreach (PSObject foundConfiguration in configurations)
                string configFilePath = null;
                PSPropertyInfo configFilePathProperty = foundConfiguration.Properties["ConfigFilePath"];
                if (configFilePathProperty != null)
                    configFilePath = configFilePathProperty.Value as string;
                // If we could not get the config file, throw an error that it's not a configuration created with
                // config file-based session configurations.
                if (configFilePath == null)
                    string configurationName = (string)foundConfiguration.Properties["Name"].Value;
                    string message = StringUtil.Format(RemotingErrorIdStrings.SessionConfigurationMustBeFileBased, configurationName);
                    ArgumentException ioe = new ArgumentException(message);
                    ErrorRecord er = new ErrorRecord(ioe, "SessionConfigurationMustBeFileBased", ErrorCategory.InvalidArgument, foundConfiguration);
                InitialSessionState iss = InitialSessionState.CreateFromSessionConfigurationFile(configFilePath, validator);
                if (this.Full)
                    WriteObject(iss);
                    using (PowerShellApi analyzer = PowerShellApi.Create(iss))
                        analyzer.AddCommand("Get-Command").AddParameter("CommandType", "All");
                        foreach (PSObject output in analyzer.Invoke())
    /// This class is public for implementation reasons only and should not be used.
    public static class RemotingErrorResources
        public static string WinRMRestartWarning { get { return RemotingErrorIdStrings.WinRMRestartWarning; } }
        public static string CouldNotResolveRoleDefinitionPrincipal { get { return RemotingErrorIdStrings.CouldNotResolveRoleDefinitionPrincipal; } }
