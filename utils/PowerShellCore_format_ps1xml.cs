    internal sealed class PowerShellCore_Format_Ps1Xml
            var AvailableModules_GroupingFormat = CustomControl.Create()
                            .AddScriptBlockExpressionBinding(@"Split-Path -Parent $_.Path | ForEach-Object { if([Version]::TryParse((Split-Path $_ -Leaf), [ref]$null)) { Split-Path -Parent $_} else {$_} } | Split-Path -Parent")
                AvailableModules_GroupingFormat
                "System.RuntimeType",
                ViewsOf_System_RuntimeType());
                "Microsoft.PowerShell.Commands.MemberDefinition",
                ViewsOf_Microsoft_PowerShell_Commands_MemberDefinition());
                "Microsoft.PowerShell.Commands.GroupInfo",
                ViewsOf_Microsoft_PowerShell_Commands_GroupInfo());
                "Microsoft.PowerShell.Commands.GroupInfoNoElement",
                ViewsOf_Microsoft_PowerShell_Commands_GroupInfoNoElement());
                "Microsoft.PowerShell.Commands.HistoryInfo",
                ViewsOf_Microsoft_PowerShell_Commands_HistoryInfo());
                "Microsoft.PowerShell.Commands.MatchInfo",
                ViewsOf_Microsoft_PowerShell_Commands_MatchInfo());
                "Deserialized.Microsoft.PowerShell.Commands.MatchInfo",
                ViewsOf_Deserialized_Microsoft_PowerShell_Commands_MatchInfo());
                "System.Management.Automation.PSVariable",
                ViewsOf_System_Management_Automation_PSVariable());
                "System.Management.Automation.PathInfo",
                ViewsOf_System_Management_Automation_PathInfo());
                "System.Management.Automation.CommandInfo",
                ViewsOf_System_Management_Automation_CommandInfo());
            var td10 = new ExtendedTypeDefinition(
                "System.Management.Automation.AliasInfo",
                ViewsOf_System_Management_Automation_AliasInfo_System_Management_Automation_ApplicationInfo_System_Management_Automation_CmdletInfo_System_Management_Automation_ExternalScriptInfo_System_Management_Automation_FilterInfo_System_Management_Automation_FunctionInfo_System_Management_Automation_ScriptInfo());
            td10.TypeNames.Add("System.Management.Automation.ApplicationInfo");
            td10.TypeNames.Add("System.Management.Automation.CmdletInfo");
            td10.TypeNames.Add("System.Management.Automation.ExternalScriptInfo");
            td10.TypeNames.Add("System.Management.Automation.FilterInfo");
            td10.TypeNames.Add("System.Management.Automation.FunctionInfo");
            td10.TypeNames.Add("System.Management.Automation.ScriptInfo");
            yield return td10;
                "System.Management.Automation.Runspaces.TypeData",
                ViewsOf_System_Management_Automation_Runspaces_TypeData());
                "Microsoft.PowerShell.Commands.ControlPanelItem",
                ViewsOf_Microsoft_PowerShell_Commands_ControlPanelItem());
                "System.Management.Automation.ApplicationInfo",
                ViewsOf_System_Management_Automation_ApplicationInfo());
                "System.Management.Automation.ScriptInfo",
                ViewsOf_System_Management_Automation_ScriptInfo());
                "System.Management.Automation.ExternalScriptInfo",
                ViewsOf_System_Management_Automation_ExternalScriptInfo());
                "System.Management.Automation.FunctionInfo",
                ViewsOf_System_Management_Automation_FunctionInfo());
                "System.Management.Automation.FilterInfo",
                ViewsOf_System_Management_Automation_FilterInfo());
                ViewsOf_System_Management_Automation_AliasInfo());
                "Microsoft.PowerShell.Commands.ListCommand+MemberInfo",
                ViewsOf_Microsoft_PowerShell_Commands_ListCommand_MemberInfo());
            var td20 = new ExtendedTypeDefinition(
                "Microsoft.PowerShell.Commands.ActiveDirectoryProvider+ADPSDriveInfo",
                ViewsOf_Microsoft_PowerShell_Commands_ActiveDirectoryProvider_ADPSDriveInfo_System_Management_Automation_PSDriveInfo());
            td20.TypeNames.Add("System.Management.Automation.PSDriveInfo");
            yield return td20;
                "System.Management.Automation.ProviderInfo",
                ViewsOf_System_Management_Automation_ProviderInfo());
                "System.Management.Automation.CmdletInfo",
                ViewsOf_System_Management_Automation_CmdletInfo());
            var td23 = new ExtendedTypeDefinition(
                ViewsOf_System_Management_Automation_FilterInfo_System_Management_Automation_FunctionInfo());
            td23.TypeNames.Add("System.Management.Automation.FunctionInfo");
            yield return td23;
                "System.Management.Automation.PSDriveInfo",
                ViewsOf_System_Management_Automation_PSDriveInfo());
                "System.Management.Automation.Subsystem.SubsystemInfo",
                ViewsOf_System_Management_Automation_Subsystem_SubsystemInfo());
                "System.Management.Automation.Subsystem.SubsystemInfo+ImplementationInfo",
                ViewsOf_System_Management_Automation_Subsystem_SubsystemInfo_ImplementationInfo());
                "System.Management.Automation.ShellVariable",
                ViewsOf_System_Management_Automation_ShellVariable());
                "System.Management.Automation.ScriptBlock",
                ViewsOf_System_Management_Automation_ScriptBlock());
            var extendedError = new ExtendedTypeDefinition(
                "System.Management.Automation.ErrorRecord#PSExtendedError",
                ViewsOf_System_Management_Automation_GetError());
            extendedError.TypeNames.Add("System.Exception#PSExtendedError");
            yield return extendedError;
            var errorRecord_Exception = new ExtendedTypeDefinition(
                "System.Management.Automation.ErrorRecord",
                ViewsOf_System_Management_Automation_ErrorRecord());
            yield return errorRecord_Exception;
                "System.Management.Automation.WarningRecord",
                ViewsOf_System_Management_Automation_WarningRecord());
                "Deserialized.System.Management.Automation.WarningRecord",
                ViewsOf_Deserialized_System_Management_Automation_WarningRecord());
                "System.Management.Automation.InformationRecord",
                ViewsOf_System_Management_Automation_InformationRecord());
                "System.Management.Automation.CommandParameterSetInfo",
                ViewsOf_System_Management_Automation_CommandParameterSetInfo());
                "System.Management.Automation.Runspaces.Runspace",
                ViewsOf_System_Management_Automation_Runspaces_Runspace());
                "System.Management.Automation.Runspaces.PSSession",
                ViewsOf_System_Management_Automation_Runspaces_PSSession());
                "System.Management.Automation.Job",
                ViewsOf_System_Management_Automation_Job());
                "Deserialized.Microsoft.PowerShell.Commands.TextMeasureInfo",
                ViewsOf_Deserialized_Microsoft_PowerShell_Commands_TextMeasureInfo());
                "Deserialized.Microsoft.PowerShell.Commands.GenericMeasureInfo",
                ViewsOf_Deserialized_Microsoft_PowerShell_Commands_GenericMeasureInfo());
                "System.Management.Automation.CallStackFrame",
                ViewsOf_System_Management_Automation_CallStackFrame());
            var td40 = new ExtendedTypeDefinition(
                "System.Management.Automation.CommandBreakpoint",
                ViewsOf_BreakpointTypes());
            td40.TypeNames.Add("System.Management.Automation.LineBreakpoint");
            td40.TypeNames.Add("System.Management.Automation.VariableBreakpoint");
            yield return td40;
                "Microsoft.PowerShell.Commands.PSSessionConfigurationCommands#PSSessionConfiguration",
                ViewsOf_Microsoft_PowerShell_Commands_PSSessionConfigurationCommands_PSSessionConfiguration());
                "Microsoft.PowerShell.Commands.ComputerChangeInfo",
                ViewsOf_Microsoft_PowerShell_Commands_ComputerChangeInfo());
                "Microsoft.PowerShell.Commands.RenameComputerChangeInfo",
                ViewsOf_Microsoft_PowerShell_Commands_RenameComputerChangeInfo());
                "ModuleInfoGrouping",
                ViewsOf_ModuleInfoGrouping(sharedControls));
                "System.Management.Automation.PSModuleInfo",
                ViewsOf_System_Management_Automation_PSModuleInfo());
                "System.Management.Automation.ExperimentalFeature",
                ViewsOf_System_Management_Automation_ExperimentalFeature());
            var td46 = new ExtendedTypeDefinition(
                "Microsoft.PowerShell.Commands.BasicHtmlWebResponseObject",
                ViewsOf_Microsoft_PowerShell_Commands_BasicHtmlWebResponseObject());
            yield return td46;
                "Microsoft.PowerShell.Commands.WebResponseObject",
                ViewsOf_Microsoft_PowerShell_Commands_WebResponseObject());
                "Microsoft.PowerShell.Commands.FileHashInfo",
                ViewsOf_Microsoft_Powershell_Utility_FileHashInfo());
                "Microsoft.PowerShell.Commands.PSRunspaceDebug",
                ViewsOf_Microsoft_PowerShell_Commands_PSRunspaceDebug());
                "Microsoft.PowerShell.MarkdownRender.PSMarkdownOptionInfo",
                ViewsOf_Microsoft_PowerShell_MarkdownRender_MarkdownOptionInfo());
                "Microsoft.PowerShell.Commands.TestConnectionCommand+TcpPortStatus",
                ViewsOf_Microsoft_PowerShell_Commands_TestConnectionCommand_TcpPortStatus());
                "Microsoft.PowerShell.Commands.TestConnectionCommand+PingStatus",
                ViewsOf_Microsoft_PowerShell_Commands_TestConnectionCommand_PingStatus());
                "Microsoft.PowerShell.Commands.TestConnectionCommand+PingMtuStatus",
                ViewsOf_Microsoft_PowerShell_Commands_TestConnectionCommand_PingMtuStatus());
                "Microsoft.PowerShell.Commands.TestConnectionCommand+TraceStatus",
                ViewsOf_Microsoft_PowerShell_Commands_TestConnectionCommand_TraceStatus());
                "Microsoft.PowerShell.Commands.ByteCollection",
                ViewsOf_Microsoft_PowerShell_Commands_ByteCollection());
                "System.Management.Automation.PSStyle",
                ViewsOf_System_Management_Automation_PSStyle());
                "System.Management.Automation.PSStyle+FormattingData",
                ViewsOf_System_Management_Automation_PSStyleFormattingData());
                "System.Management.Automation.PSStyle+ProgressConfiguration",
                ViewsOf_System_Management_Automation_PSStyleProgressConfiguration());
                "System.Management.Automation.PSStyle+FileInfoFormatting",
                ViewsOf_System_Management_Automation_PSStyleFileInfoFormat());
                "System.Management.Automation.PSStyle+ForegroundColor",
                ViewsOf_System_Management_Automation_PSStyleForegroundColor());
                "System.Management.Automation.PSStyle+BackgroundColor",
                ViewsOf_System_Management_Automation_PSStyleBackgroundColor());
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_RuntimeType()
            yield return new FormatViewDefinition("System.RuntimeType",
                    .AddHeader(label: "IsPublic", width: 8)
                    .AddHeader(label: "IsSerial", width: 8)
                    .AddHeader(width: 40)
                        .AddPropertyColumn("IsPublic")
                        .AddPropertyColumn("IsSerializable")
                        .AddPropertyColumn("BaseType")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_MemberDefinition()
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.MemberDefinition",
                TableControl.Create(autoSize: true)
                    .GroupByProperty("TypeName")
                    .AddHeader(label: "Name")
                    .AddHeader(label: "MemberType")
                    .AddHeader(label: "Definition")
                        .AddPropertyColumn("MemberType")
                        .AddPropertyColumn("Definition")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_GroupInfo()
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.GroupInfo",
                    .AddHeader(Alignment.Right, label: "Count", width: 5)
                    .AddHeader(width: 25)
                        .AddPropertyColumn("Count")
                        .AddItemProperty(@"Count")
                        .AddItemProperty(@"Values")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_GroupInfoNoElement()
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.GroupInfoNoElement",
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_HistoryInfo()
            yield return new FormatViewDefinition("history",
                    .AddHeader(Alignment.Right, width: 4)
                    .AddHeader(Alignment.Right, label: "Duration", width: 12)
                                if ($_.Duration.TotalHours -ge 10) {
                                    return ""{0}:{1:mm}:{1:ss}.{1:fff}"" -f [int]$_.Duration.TotalHours, $_.Duration
                                elseif ($_.Duration.TotalHours -ge 1) {
                                    $formatString = ""h\:mm\:ss\.fff""
                                elseif ($_.Duration.TotalMinutes -ge 1) {
                                    $formatString = ""m\:ss\.fff""
                                    $formatString = ""s\.fff""
                                $_.Duration.ToString($formatString)
                        .AddPropertyColumn("CommandLine")
                    .AddPropertyEntry("CommandLine")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_MatchInfo()
            yield return new FormatViewDefinition("MatchInfo",
                        .AddScriptBlockExpressionBinding(@"$_.ToEmphasizedString(((get-location).path))")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Deserialized_Microsoft_PowerShell_Commands_MatchInfo()
                        .AddScriptBlockExpressionBinding(@"$_.Line")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PSVariable()
            yield return new FormatViewDefinition("Variable",
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PathInfo()
            yield return new FormatViewDefinition("PathInfo",
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_CommandInfo()
            yield return new FormatViewDefinition("CommandInfo",
                        .AddPropertyColumn("CommandType")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_AliasInfo_System_Management_Automation_ApplicationInfo_System_Management_Automation_CmdletInfo_System_Management_Automation_ExternalScriptInfo_System_Management_Automation_FilterInfo_System_Management_Automation_FunctionInfo_System_Management_Automation_ScriptInfo()
                    .AddHeader(label: "CommandType", width: 15)
                    .AddHeader(label: "Name", width: 50)
                    .AddHeader(label: "Version", width: 10)
                    .AddHeader(label: "Source")
                                if ($_.CommandType -eq ""Alias"")
                                  $_.DisplayName
                                  $_.Name
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_Runspaces_TypeData()
            yield return new FormatViewDefinition("TypeData",
                    .AddHeader(label: "TypeName")
                    .AddHeader(label: "Members")
                        .AddPropertyColumn("TypeName")
                        .AddPropertyColumn("Members")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_ControlPanelItem()
            yield return new FormatViewDefinition("ControlPanelItem",
                    .AddHeader(label: "CanonicalName")
                    .AddHeader(label: "Category")
                        .AddPropertyColumn("CanonicalName")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_ApplicationInfo()
            yield return new FormatViewDefinition("ApplicationInfo",
            yield return new FormatViewDefinition("System.Management.Automation.ApplicationInfo",
                        .AddItemProperty(@"CommandType")
                        .AddItemProperty(@"Definition")
                        .AddItemProperty(@"Extension")
                        .AddItemProperty(@"FileVersionInfo")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_ScriptInfo()
            yield return new FormatViewDefinition("ScriptInfo",
            yield return new FormatViewDefinition("System.Management.Automation.ScriptInfo",
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_ExternalScriptInfo()
            yield return new FormatViewDefinition("ExternalScriptInfo",
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_FunctionInfo()
            yield return new FormatViewDefinition("FunctionInfo",
                        .AddPropertyColumn("Function")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_FilterInfo()
            yield return new FormatViewDefinition("FilterInfo",
                        .AddPropertyColumn("Filter")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_AliasInfo()
            yield return new FormatViewDefinition("AliasInfo",
            yield return new FormatViewDefinition("System.Management.Automation.AliasInfo",
                        .AddItemProperty(@"ReferencedCommand")
                        .AddItemProperty(@"ResolvedCommand")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_ListCommand_MemberInfo()
            yield return new FormatViewDefinition("memberinfo",
                    .AddHeader(label: "Class", width: 11)
                        .AddPropertyColumn("MemberClass")
                        .AddPropertyColumn("MemberData")
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.ListCommand+MemberInfo",
                        .AddItemProperty(@"MemberClass")
                        .AddItemProperty(@"MemberData")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_ActiveDirectoryProvider_ADPSDriveInfo_System_Management_Automation_PSDriveInfo()
            yield return new FormatViewDefinition("drive",
                    .AddHeader(label: "Used (GB)", width: 13)
                    .AddHeader(label: "Free (GB)", width: 13)
                    .AddHeader(label: "Provider", width: 13)
                    .AddHeader(label: "Root", width: 35)
                        .AddScriptBlockColumn(@"if($_.Used -or $_.Free) { ""{0:###0.00}"" -f ($_.Used / 1GB) }", alignment: Alignment.Right)
                        .AddScriptBlockColumn(@"if($_.Used -or $_.Free) { ""{0:###0.00}"" -f ($_.Free / 1GB) }", alignment: Alignment.Right)
                        .AddScriptBlockColumn("$_.Provider.Name")
                        .AddScriptBlockColumn("if($null -ne $_.DisplayRoot) { $_.DisplayRoot } else { $_.Root }")
                        .AddPropertyColumn("CurrentLocation", alignment: Alignment.Right)
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_ProviderInfo()
            yield return new FormatViewDefinition("provider",
                        .AddPropertyColumn("Capabilities")
                        .AddPropertyColumn("Drives")
                        .AddItemProperty(@"Drives")
                        .AddItemProperty(@"Home")
                        .AddItemProperty(@"Description")
                        .AddItemProperty(@"Capabilities")
                        .AddItemProperty(@"ImplementingType")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_CmdletInfo()
            yield return new FormatViewDefinition("System.Management.Automation.CmdletInfo",
                        .AddItemProperty(@"DLL")
                        .AddItemProperty(@"HelpFile")
                        .AddItemProperty(@"ParameterSets")
                        .AddItemProperty(@"Verb")
                        .AddItemProperty(@"Noun")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_FilterInfo_System_Management_Automation_FunctionInfo()
            yield return new FormatViewDefinition("System.Management.Automation.CommandInfo",
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PSDriveInfo()
            yield return new FormatViewDefinition("System.Management.Automation.PSDriveInfo",
                        .AddItemProperty(@"Provider")
                        .AddItemProperty(@"Root")
                        .AddItemProperty(@"CurrentLocation")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_Subsystem_SubsystemInfo()
                    .AddHeader(Alignment.Left, width: 17, label: "Kind")
                    .AddHeader(Alignment.Left, width: 18, label: "SubsystemType")
                    .AddHeader(Alignment.Right, width: 12, label: "IsRegistered")
                    .AddHeader(Alignment.Left, label: "Implementations")
                        .AddPropertyColumn("Kind")
                        .AddScriptBlockColumn("$_.SubsystemType.Name")
                        .AddPropertyColumn("IsRegistered")
                        .AddPropertyColumn("Implementations")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_Subsystem_SubsystemInfo_ImplementationInfo()
                        .AddItemProperty(@"Kind")
                        .AddItemProperty(@"ImplementationType")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_ShellVariable()
            yield return new FormatViewDefinition("ShellVariable",
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_ScriptBlock()
            yield return new FormatViewDefinition("ScriptBlock",
                        .AddScriptBlockExpressionBinding(@"$_")
        // This generates a custom view for ErrorRecords and Exceptions making
        // specific nested types defined in $expandTypes visible.  It also handles
        // IEnumerable types.  Nested types are indented by 4 spaces.
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_GetError()
            yield return new FormatViewDefinition("GetErrorInstance",
                    .GroupByProperty("PSErrorIndex", label: "ErrorIndex")
                            Set-StrictMode -Off
                            $maxDepth = 10
                            $ellipsis = ""`u{2026}""
                            $resetColor = ''
                            $errorColor = ''
                            $accentColor = ''
                            if ($Host.UI.SupportsVirtualTerminal -and ([string]::IsNullOrEmpty($env:__SuppressAnsiEscapeSequences))) {
                                $resetColor = $PSStyle.Reset
                                $errorColor = $psstyle.Formatting.Error
                                $accentColor = $PSStyle.Formatting.FormatAccent
                            function Show-ErrorRecord($obj, [int]$indent = 0, [int]$depth = 1) {
                                $newline = [Environment]::Newline
                                $output = [System.Text.StringBuilder]::new()
                                $prefix = ' ' * $indent
                                $expandTypes = @(
                                    'Microsoft.Rest.HttpRequestMessageWrapper'
                                    'Microsoft.Rest.HttpResponseMessageWrapper'
                                    'System.Management.Automation.InvocationInfo'
                                # if object is an Exception, add an ExceptionType property
                                if ($obj -is [Exception]) {
                                    $obj | Add-Member -NotePropertyName Type -NotePropertyValue $obj.GetType().FullName -ErrorAction Ignore
                                # first find the longest property so we can indent properly
                                $propLength = 0
                                foreach ($prop in $obj.PSObject.Properties) {
                                    if ($prop.Value -ne $null -and $prop.Value -ne [string]::Empty -and $prop.Name.Length -gt $propLength) {
                                        $propLength = $prop.Name.Length
                                $addedProperty = $false
                                    # don't show empty properties or our added property for $error[index]
                                    if ($prop.Value -ne $null -and $prop.Value -ne [string]::Empty -and $prop.Value.count -gt 0 -and $prop.Name -ne 'PSErrorIndex') {
                                        $addedProperty = $true
                                        $null = $output.Append($prefix)
                                        $null = $output.Append($accentColor)
                                        $null = $output.Append($prop.Name)
                                        $propNameIndent = ' ' * ($propLength - $prop.Name.Length)
                                        $null = $output.Append($propNameIndent)
                                        $null = $output.Append(' : ')
                                        $null = $output.Append($resetColor)
                                        $newIndent = $indent + 4
                                        # only show nested objects that are Exceptions, ErrorRecords, or types defined in $expandTypes and types not in $ignoreTypes
                                        if ($prop.Value -is [Exception] -or $prop.Value -is [System.Management.Automation.ErrorRecord] -or
                                            $expandTypes -contains $prop.TypeNameOfValue -or ($prop.TypeNames -ne $null -and $expandTypes -contains $prop.TypeNames[0])) {
                                            if ($depth -ge $maxDepth) {
                                                $null = $output.Append($ellipsis)
                                                $null = $output.Append($newline)
                                                $null = $output.Append((Show-ErrorRecord $prop.Value $newIndent ($depth + 1)))
                                        # `TargetSite` has many members that are not useful visually, so we have a reduced view of the relevant members
                                        elseif ($prop.Name -eq 'TargetSite' -and $prop.Value.GetType().Name -eq 'RuntimeMethodInfo') {
                                                $targetSite = [PSCustomObject]@{
                                                    Name = $prop.Value.Name
                                                    DeclaringType = $prop.Value.DeclaringType
                                                    MemberType = $prop.Value.MemberType
                                                    Module = $prop.Value.Module
                                                $null = $output.Append((Show-ErrorRecord $targetSite $newIndent ($depth + 1)))
                                        # `StackTrace` is handled specifically because the lines are typically long but necessary so they are left justified without additional indentation
                                        elseif ($prop.Name -eq 'StackTrace') {
                                            # for a stacktrace which is usually quite wide with info, we left justify it
                                            $null = $output.Append($prop.Value)
                                        # Dictionary and Hashtable we want to show as Key/Value pairs, we don't do the extra whitespace alignment here
                                        elseif ($prop.Value -is [System.Collections.IDictionary]) {
                                            $isFirstElement = $true
                                            foreach ($key in ($prop.Value.Keys | Sort-Object)) {
                                                if ($isFirstElement) {
                                                if ($key -eq 'Authorization') {
                                                    $null = $output.Append(""${prefix}    ${accentColor}${key} : ${resetColor}${ellipsis}${newline}"")
                                                    $null = $output.Append(""${prefix}    ${accentColor}${key} : ${resetColor}$($prop.Value[$key])${newline}"")
                                                $isFirstElement = $false
                                        # if the object implements IEnumerable and not a string, we try to show each object
                                        # We ignore the `Data` property as it can contain lots of type information by the interpreter that isn't useful here
                                        elseif (!($prop.Value -is [System.String]) -and $prop.Value.GetType().GetInterface('IEnumerable') -ne $null -and $prop.Name -ne 'Data') {
                                                foreach ($value in $prop.Value) {
                                                    $valueIndent = ' ' * ($newIndent + 2)
                                                    if ($value -is [Type]) {
                                                        # Just show the typename instead of it as an object
                                                        $null = $output.Append(""${prefix}${valueIndent}[$($value.ToString())]"")
                                                    elseif ($value -is [string] -or $value.GetType().IsPrimitive) {
                                                        $null = $output.Append(""${prefix}${valueIndent}${value}"")
                                                        if (!$isFirstElement) {
                                                        $null = $output.Append((Show-ErrorRecord $value $newIndent ($depth + 1)))
                                        elseif ($prop.Value -is [Type]) {
                                            $null = $output.Append(""[$($prop.Value.ToString())]"")
                                        # Anything else, we convert to string.
                                        # ToString() can throw so we use LanguagePrimitives.TryConvertTo() to hide a convert error
                                            $value = $null
                                            if ([System.Management.Automation.LanguagePrimitives]::TryConvertTo($prop.Value, [string], [ref]$value) -and $value -ne $null)
                                                if ($prop.Name -eq 'PositionMessage') {
                                                    $value = $value.Insert($value.IndexOf('~'), $errorColor)
                                                elseif ($prop.Name -eq 'Message') {
                                                    $value = $errorColor + $value
                                                $isFirstLine = $true
                                                if ($value.Contains($newline)) {
                                                    # the 3 is to account for ' : '
                                                    $valueIndent = ' ' * ($propLength + 3)
                                                    # need to trim any extra whitespace already in the text
                                                    foreach ($line in $value.Split($newline)) {
                                                        if (!$isFirstLine) {
                                                            $null = $output.Append(""${newline}${prefix}${valueIndent}"")
                                                        $null = $output.Append($line.Trim())
                                                        $isFirstLine = $false
                                                    $null = $output.Append($value)
                                # if we had added nested properties, we need to remove the last newline
                                if ($addedProperty) {
                                    $null = $output.Remove($output.Length - $newline.Length, $newline.Length)
                                $output.ToString()
                            # Add back original typename and remove PSExtendedError
                            if ($_.PSObject.TypeNames.Contains('System.Management.Automation.ErrorRecord#PSExtendedError')) {
                                $_.PSObject.TypeNames.Add('System.Management.Automation.ErrorRecord')
                                $null = $_.PSObject.TypeNames.Remove('System.Management.Automation.ErrorRecord#PSExtendedError')
                            elseif ($_.PSObject.TypeNames.Contains('System.Exception#PSExtendedError')) {
                                $_.PSObject.TypeNames.Add('System.Exception')
                                $null = $_.PSObject.TypeNames.Remove('System.Exception#PSExtendedError')
                            Show-ErrorRecord $_
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_ErrorRecord()
            yield return new FormatViewDefinition("ErrorInstance",
                                    $commandPrefix = ''
                                    if (@('NativeCommandErrorMessage','NativeCommandError') -notcontains $_.FullyQualifiedErrorId -and @('CategoryView','ConciseView','DetailedView') -notcontains $ErrorView)
                                        $myinv = $_.InvocationInfo
                                        if ($Host.UI.SupportsVirtualTerminal) {
                                            $errorColor = $PSStyle.Formatting.Error
                                        $commandPrefix = if ($myinv -and $myinv.MyCommand) {
                                            switch -regex ( $myinv.MyCommand.CommandType )
                                                ([System.Management.Automation.CommandTypes]::ExternalScript)
                                                    if ($myinv.MyCommand.Path)
                                                        $myinv.MyCommand.Path + ' : '
                                                ([System.Management.Automation.CommandTypes]::Script)
                                                    if ($myinv.MyCommand.ScriptBlock)
                                                        $myinv.MyCommand.ScriptBlock.ToString() + ' : '
                                                default
                                                    if ($myinv.InvocationName -match '^[&\.]?$')
                                                        if ($myinv.MyCommand.Name)
                                                            $myinv.MyCommand.Name + ' : '
                                                        $myinv.InvocationName + ' : '
                                        elseif ($myinv -and $myinv.InvocationName)
                                    $errorColor + $commandPrefix
                                    $ErrorActionPreference = 'Stop'
                                    trap { 'Error found in error view definition: ' + $_.Exception.Message }
                                        $accentColor = $PSStyle.Formatting.ErrorAccent
                                    function Get-ConciseViewPositionMessage {
                                        # returns a string cut to last whitespace
                                        function Get-TruncatedString($string, [int]$length) {
                                            if ($string.Length -le $length) {
                                                return $string
                                            return ($string.Substring(0,$length) -split '\s',-2)[0]
                                        $posmsg = ''
                                        $headerWhitespace = ''
                                        $offsetWhitespace = ''
                                        $message = ''
                                        $prefix = ''
                                        # Handle case where there is a TargetObject from a Pester `Should` assertion failure and we can show the error at the target rather than the script source
                                        # Note that in some versions, this is a Dictionary<,> and in others it's a hashtable. So we explicitly cast to a shared interface in the method invocation
                                        # to force using `IDictionary.Contains`. Hashtable does have it's own `ContainKeys` as well, but if they ever opt to use a custom `IDictionary`, that may not.
                                        $useTargetObject = $null -ne $err.TargetObject -and
                                            $err.TargetObject -is [System.Collections.IDictionary] -and
                                            ([System.Collections.IDictionary]$err.TargetObject).Contains('Line') -and
                                            ([System.Collections.IDictionary]$err.TargetObject).Contains('LineText')
                                        # The checks here determine if we show line detailed error information:
                                        # - check if `ParserError` and comes from PowerShell which eventually results in a ParseException, but during this execution it's an ErrorRecord
                                        $isParseError = $err.CategoryInfo.Category -eq 'ParserError' -and
                                            $err.Exception -is [System.Management.Automation.ParentContainsErrorRecordException]
                                        # - check if invocation is a script or multiple lines in the console
                                        $isMultiLineOrExternal = $myinv.ScriptName -or $myinv.ScriptLineNumber -gt 1
                                        # - check that it's not a script module as expectation is that users don't want to see the line of error within a module
                                        $shouldShowLineDetail = ($isParseError -or $isMultiLineOrExternal) -and
                                            $myinv.ScriptName -notmatch '\.psm1$'
                                        if ($useTargetObject -or $shouldShowLineDetail) {
                                            if ($useTargetObject) {
                                                $posmsg = "${resetcolor}$($err.TargetObject.File)${newline}"
                                            elseif ($myinv.ScriptName) {
                                                if ($env:TERM_PROGRAM -eq 'vscode') {
                                                    # If we are running in vscode, we know the file:line:col links are clickable so we use this format
                                                    $posmsg = "${resetcolor}$($myinv.ScriptName):$($myinv.ScriptLineNumber):$($myinv.OffsetInLine)${newline}"
                                                    $posmsg = "${resetcolor}$($myinv.ScriptName):$($myinv.ScriptLineNumber)${newline}"
                                                $posmsg = "${newline}"
                                                $scriptLineNumber = $err.TargetObject.Line
                                                $scriptLineNumberLength = $err.TargetObject.Line.ToString().Length
                                                $scriptLineNumber = $myinv.ScriptLineNumber
                                                $scriptLineNumberLength = $myinv.ScriptLineNumber.ToString().Length
                                            if ($scriptLineNumberLength -gt 4) {
                                                $headerWhitespace = ' ' * ($scriptLineNumberLength - 4)
                                            $lineWhitespace = ''
                                            if ($scriptLineNumberLength -lt 4) {
                                                $lineWhitespace = ' ' * (4 - $scriptLineNumberLength)
                                            $verticalBar = '|'
                                            $posmsg += "${accentColor}${headerWhitespace}Line ${verticalBar}${newline}"
                                            $highlightLine = ''
                                                $line = $_.TargetObject.LineText.Trim()
                                                $offsetLength = 0
                                                $offsetInLine = 0
                                                $startColumn = 0
                                                    ([System.Collections.IDictionary]$_.TargetObject).Contains('StartColumn') -and
                                                    [System.Management.Automation.LanguagePrimitives]::TryConvertTo[int]($_.TargetObject.StartColumn, [ref]$startColumn) -and
                                                    $null -ne $startColumn -and
                                                    $startColumn -gt 0 -and
                                                    $startColumn -le $line.Length
                                                ) {
                                                    $endColumn = 0
                                                    if (-not (
                                                        ([System.Collections.IDictionary]$_.TargetObject).Contains('EndColumn') -and
                                                        [System.Management.Automation.LanguagePrimitives]::TryConvertTo[int]($_.TargetObject.EndColumn, [ref]$endColumn) -and
                                                        $null -ne $endColumn -and
                                                        $endColumn -gt $startColumn -and
                                                        $endColumn -le ($line.Length + 1)
                                                    )) {
                                                        $endColumn = $line.Length + 1
                                                    # Input is expected to be 1-based index to match the extent positioning
                                                    # but we use 0-based indexing below.
                                                    $startColumn -= 1
                                                    $endColumn -= 1
                                                    $highlightLine = "$(" " * $startColumn)$("~" * ($endColumn - $startColumn))"
                                                    $offsetLength = $endColumn - $startColumn
                                                    $offsetInLine = $startColumn
                                                $positionMessage = $myinv.PositionMessage.Split($newline)
                                                $line = $positionMessage[1].Substring(1) # skip the '+' at the start
                                                $highlightLine = $positionMessage[$positionMessage.Count - 1].Substring(1)
                                                $offsetLength = $highlightLine.Trim().Length
                                                $offsetInLine = $highlightLine.IndexOf('~')
                                            if (-not $line.EndsWith($newline)) {
                                                $line += $newline
                                            # don't color the whole line
                                            if ($offsetLength -lt $line.Length - 1) {
                                                $line = $line.Insert($offsetInLine + $offsetLength, $resetColor).Insert($offsetInLine, $accentColor)
                                            $posmsg += "${accentColor}${lineWhitespace}${ScriptLineNumber} ${verticalBar} ${resetcolor}${line}"
                                            $offsetWhitespace = ' ' * $offsetInLine
                                            $prefix = "${accentColor}${headerWhitespace}     ${verticalBar} ${errorColor}"
                                            if ($highlightLine -ne '') {
                                                $posMsg += "${prefix}${highlightLine}${newline}"
                                            $message = "${prefix}"
                                        if (! $err.ErrorDetails -or ! $err.ErrorDetails.Message) {
                                            if ($err.CategoryInfo.Category -eq 'ParserError' -and $err.Exception.Message.Contains("~$newline")) {
                                                # need to parse out the relevant part of the pre-rendered positionmessage
                                                $message += $err.Exception.Message.split("~$newline")[1].split("${newline}${newline}")[0]
                                            elseif ($err.Exception) {
                                                $message += $err.Exception.Message
                                            elseif ($err.Message) {
                                                $message += $err.Message
                                                $message += $err.ToString()
                                            $message += $err.ErrorDetails.Message
                                        # if rendering line information, break up the message if it's wider than the console
                                        if ($myinv -and $myinv.ScriptName -or $err.CategoryInfo.Category -eq 'ParserError') {
                                            $prefixLength = [System.Management.Automation.Internal.StringDecorated]::new($prefix).ContentLength
                                            $prefixVtLength = $prefix.Length - $prefixLength
                                            # replace newlines in message so it lines up correct
                                            $message = $message.Replace($newline, ' ').Replace("`n", ' ').Replace("`t", ' ')
                                            $windowWidth = 120
                                            if ($Host.UI.RawUI -ne $null) {
                                                $windowWidth = $Host.UI.RawUI.WindowSize.Width
                                            if ($windowWidth -gt 0 -and ($message.Length - $prefixVTLength) -gt $windowWidth) {
                                                $sb = [Text.StringBuilder]::new()
                                                $substring = Get-TruncatedString -string $message -length ($windowWidth + $prefixVTLength)
                                                $null = $sb.Append($substring)
                                                $remainingMessage = $message.Substring($substring.Length).Trim()
                                                $null = $sb.Append($newline)
                                                while (($remainingMessage.Length + $prefixLength) -gt $windowWidth) {
                                                    $subMessage = $prefix + $remainingMessage
                                                    $substring = Get-TruncatedString -string $subMessage -length ($windowWidth + $prefixVtLength)
                                                    if ($substring.Length - $prefix.Length -gt 0)
                                                        $remainingMessage = $remainingMessage.Substring($substring.Length - $prefix.Length).Trim()
                                                $null = $sb.Append($prefix + $remainingMessage.Trim())
                                                $message = $sb.ToString()
                                            $message += $newline
                                        $posmsg += "${errorColor}" + $message
                                        $reason = 'Error'
                                        if ($err.Exception -and $err.Exception.WasThrownFromThrowStatement) {
                                            $reason = 'Exception'
                                        # MyCommand can be the script block, so we don't want to show that so check if it's an actual command
                                        elseif ($myinv.MyCommand -and $myinv.MyCommand.Name -and (Get-Command -Name $myinv.MyCommand -ErrorAction Ignore))
                                            $reason = $myinv.MyCommand
                                        # If it's a scriptblock, better to show the command in the scriptblock that had the error
                                        elseif ($err.CategoryInfo.Activity) {
                                            $reason = $err.CategoryInfo.Activity
                                        elseif ($myinv.MyCommand) {
                                        elseif ($myinv.InvocationName) {
                                            $reason = $myinv.InvocationName
                                        elseif ($err.CategoryInfo.Category) {
                                            $reason = $err.CategoryInfo.Category
                                        elseif ($err.CategoryInfo.Reason) {
                                            $reason = $err.CategoryInfo.Reason
                                        $errorMsg = 'Error'
                                        "${errorColor}${reason}: ${posmsg}${resetcolor}"
                                    $err = $_
                                    if (!$myinv -and $_.ErrorRecord -and $_.ErrorRecord.InvocationInfo) {
                                        $err = $_.ErrorRecord
                                        $myinv = $err.InvocationInfo
                                    if ($err.FullyQualifiedErrorId -eq 'NativeCommandErrorMessage' -or $err.FullyQualifiedErrorId -eq 'NativeCommandError') {
                                        return "${errorColor}$($err.Exception.Message)${resetcolor}"
                                    if ($ErrorView -eq 'DetailedView') {
                                        $message = Get-Error | Out-String
                                        return "${errorColor}${message}${resetcolor}"
                                    if ($ErrorView -eq 'CategoryView') {
                                        $message = $err.CategoryInfo.GetMessage()
                                    if ($ErrorView -eq 'ConciseView') {
                                        $posmsg = Get-ConciseViewPositionMessage
                                    elseif ($myinv -and ($myinv.MyCommand -or ($err.CategoryInfo.Category -ne 'ParserError'))) {
                                        $posmsg = $myinv.PositionMessage
                                        if ($posmsg -ne '') {
                                            $posmsg = $newline + $posmsg
                                    if ($err.PSMessageDetails) {
                                        $posmsg = ' : ' +  $err.PSMessageDetails + $posmsg
                                        $recommendedAction = $_.ErrorDetails.RecommendedAction
                                        if (-not [String]::IsNullOrWhiteSpace($recommendedAction)) {
                                            $recommendedAction = $newline +
                                                ${errorColor} +
                                                '  Recommendation: ' +
                                                $recommendedAction +
                                                ${resetcolor}
                                            $posmsg = "${errorColor}${posmsg}"
                                        return $posmsg + $recommendedAction
                                    $indent = 4
                                    $errorCategoryMsg = $err.ErrorCategory_Message
                                    if ($null -ne $errorCategoryMsg)
                                        $indentString = '+ CategoryInfo          : ' + $err.ErrorCategory_Message
                                        $indentString = '+ CategoryInfo          : ' + $err.CategoryInfo
                                    $posmsg += $newline + $indentString
                                    $indentString = "+ FullyQualifiedErrorId : " + $err.FullyQualifiedErrorId
                                    $originInfo = $err.OriginInfo
                                    if (($null -ne $originInfo) -and ($null -ne $originInfo.PSComputerName))
                                        $indentString = "+ PSComputerName        : " + $originInfo.PSComputerName
                                    $finalMsg = if ($err.ErrorDetails.Message) {
                                        $err.ErrorDetails.Message + $posmsg
                                        $err.Exception.Message + $posmsg
                                    "${errorColor}${finalMsg}${resetcolor}"
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_WarningRecord()
            yield return new FormatViewDefinition("WarningRecord",
        private static IEnumerable<FormatViewDefinition> ViewsOf_Deserialized_System_Management_Automation_WarningRecord()
            yield return new FormatViewDefinition("DeserializedWarningRecord",
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_InformationRecord()
            yield return new FormatViewDefinition("InformationRecord",
                        .AddScriptBlockExpressionBinding(@"$_.ToString()")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_CommandParameterSetInfo()
            var FmtParameterAttributes = CustomControl.Create()
                        .StartFrame(leftIndent: 2)
            var FmtParameterInfo = CustomControl.Create()
                            .AddText("Parameter Name: ")
                                .AddText("ParameterType = ")
                                .AddPropertyExpressionBinding(@"ParameterType")
                                .AddText("Position = ")
                                .AddPropertyExpressionBinding(@"Position")
                                .AddText("IsMandatory = ")
                                .AddPropertyExpressionBinding(@"IsMandatory")
                                .AddText("IsDynamic = ")
                                .AddPropertyExpressionBinding(@"IsDynamic")
                                .AddText("HelpMessage = ")
                                .AddPropertyExpressionBinding(@"HelpMessage")
                                .AddText("ValueFromPipeline = ")
                                .AddPropertyExpressionBinding(@"ValueFromPipeline")
                                .AddText("ValueFromPipelineByPropertyName = ")
                                .AddPropertyExpressionBinding(@"ValueFromPipelineByPropertyName")
                                .AddText("ValueFromRemainingArguments = ")
                                .AddPropertyExpressionBinding(@"ValueFromRemainingArguments")
                                .AddText("Aliases = ")
                                .AddText("Attributes =")
                                .AddPropertyExpressionBinding(@"Attributes", enumerateCollection: true, customControl: FmtParameterAttributes)
            yield return new FormatViewDefinition("CommandParameterSetInfo",
                        .AddText("Parameter Set Name: ")
                        .AddText("Is default parameter set: ")
                        .AddPropertyExpressionBinding(@"IsDefault")
                        .AddPropertyExpressionBinding(@"Parameters", enumerateCollection: true, customControl: FmtParameterInfo)
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_Runspaces_Runspace()
            yield return new FormatViewDefinition("Runspace",
                    .AddHeader(Alignment.Right, label: "Id", width: 3)
                    .AddHeader(Alignment.Left, label: "Name", width: 15)
                    .AddHeader(Alignment.Left, label: "ComputerName", width: 15)
                    .AddHeader(Alignment.Left, label: "Type", width: 13)
                    .AddHeader(Alignment.Left, label: "State", width: 13)
                    .AddHeader(Alignment.Left, label: "Availability", width: 15)
                    if ($null -ne $_.ConnectionInfo)
                      $_.ConnectionInfo.ComputerName
                      ""localhost""
                      ""Remote""
                      ""Local""
                        .AddScriptBlockColumn("$_.RunspaceStateInfo.State")
                    if (($null -ne $_.Debugger) -and ($_.Debugger.InBreakpoint))
                        ""InBreakpoint""
                        $_.RunspaceAvailability
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_Runspaces_PSSession()
            yield return new FormatViewDefinition("PSSession",
                    .AddHeader(Alignment.Left, label: "Transport", width: 9)
                    .AddHeader(Alignment.Left, label: "ComputerType", width: 15)
                    .AddHeader(Alignment.Left, label: "ConfigurationName", width: 20)
                    .AddHeader(Alignment.Right, label: "Availability", width: 13)
                        .AddPropertyColumn("Transport")
                        .AddPropertyColumn("ComputerType")
                        .AddPropertyColumn("ConfigurationName")
                        .AddPropertyColumn("Availability")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_Job()
            yield return new FormatViewDefinition("Job",
                    .AddHeader(Alignment.Left, label: "Id", width: 6)
                    .AddHeader(Alignment.Left, label: "PSJobTypeName", width: 15)
                    .AddHeader(Alignment.Left, label: "HasMoreData", width: 15)
                    .AddHeader(Alignment.Left, label: "Location", width: 20)
                    .AddHeader(Alignment.Left, label: "Command", width: 25)
                        .AddPropertyColumn("PSJobTypeName")
                        .AddPropertyColumn("HasMoreData")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Deserialized_Microsoft_PowerShell_Commands_TextMeasureInfo()
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.TextMeasureInfo",
                    .AddHeader(label: "Lines")
                    .AddHeader(label: "Words")
                    .AddHeader(label: "Characters")
                    .AddHeader(label: "Property")
                        .AddPropertyColumn("Lines")
                        .AddPropertyColumn("Words")
                        .AddPropertyColumn("Characters")
                        .AddPropertyColumn("Property")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Deserialized_Microsoft_PowerShell_Commands_GenericMeasureInfo()
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.GenericMeasureInfo",
                        .AddItemProperty(@"Average")
                        .AddItemProperty(@"Sum")
                        .AddItemProperty(@"Maximum")
                        .AddItemProperty(@"Minimum")
                        .AddItemProperty(@"Property")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_CallStackFrame()
            yield return new FormatViewDefinition("CallStackFrame",
                    .AddHeader(label: "Arguments")
                        .AddPropertyColumn("Arguments")
        private static IEnumerable<FormatViewDefinition> ViewsOf_BreakpointTypes()
            yield return new FormatViewDefinition("Breakpoint",
                    .AddHeader(Alignment.Right, label: "ID", width: 4)
                    .AddHeader(label: "Script")
                    .AddHeader(Alignment.Right, label: "Line", width: 4)
                    .AddHeader(label: "Variable")
                    .AddHeader(label: "Action")
                        .AddPropertyColumn("ID")
                        .AddScriptBlockColumn("if ($_.Script) { [System.IO.Path]::GetFileName($_.Script) }")
                        .AddPropertyColumn("Line")
                        .AddPropertyColumn("Variable")
                        .AddPropertyColumn("Action")
                    .StartEntry(entrySelectedByType: new[] { "System.Management.Automation.LineBreakpoint" })
                        .AddItemProperty(@"ID")
                        .AddItemProperty(@"Script")
                        .AddItemProperty(@"Enabled")
                        .AddItemProperty(@"HitCount")
                        .AddItemProperty(@"Action")
                    .StartEntry(entrySelectedByType: new[] { "System.Management.Automation.VariableBreakpoint" })
                        .AddItemProperty(@"Variable")
                        .AddItemProperty(@"AccessMode")
                    .StartEntry(entrySelectedByType: new[] { "System.Management.Automation.CommandBreakpoint" })
                        .AddItemProperty(@"Command")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_PSSessionConfigurationCommands_PSSessionConfiguration()
            yield return new FormatViewDefinition("PSSessionConfiguration",
                        .AddItemProperty(@"PSVersion")
                        .AddItemProperty(@"StartupScript")
                        .AddItemProperty(@"RunAsUser")
                        .AddItemProperty(@"Permission")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_ComputerChangeInfo()
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.ComputerChangeInfo",
                    .AddHeader(Alignment.Left, label: "HasSucceeded", width: 12)
                    .AddHeader(label: "ComputerName", width: 25)
                        .AddPropertyColumn("HasSucceeded")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_RenameComputerChangeInfo()
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.RenameComputerChangeInfo",
                    .AddHeader(label: "OldComputerName", width: 25)
                    .AddHeader(label: "NewComputerName", width: 25)
                        .AddPropertyColumn("OldComputerName")
                        .AddPropertyColumn("NewComputerName")
        private const string PreReleaseStringScriptBlock = @"
                            if ($_.PrivateData -and
                                $_.PrivateData.ContainsKey('PSData') -and
                                $_.PrivateData.PSData.ContainsKey('PreRelease'))
                                    $_.PrivateData.PSData.PreRelease
        private static IEnumerable<FormatViewDefinition> ViewsOf_ModuleInfoGrouping(CustomControl[] sharedControls)
            yield return new FormatViewDefinition("Module",
                    .GroupByScriptBlock("Split-Path -Parent $_.Path | ForEach-Object { if([Version]::TryParse((Split-Path $_ -Leaf), [ref]$null)) { Split-Path -Parent $_} else {$_} } | Split-Path -Parent", customControl: sharedControls[0])
                    .AddHeader(Alignment.Left, width: 10)
                    .AddHeader(Alignment.Left, label: "PreRelease", width: 10)
                    .AddHeader(Alignment.Left, width: 35)
                    .AddHeader(Alignment.Left, width: 9, label: "PSEdition")
                    .AddHeader(Alignment.Left, label: "ExportedCommands")
                        .AddPropertyColumn("ModuleType")
                        .AddScriptBlockColumn(PreReleaseStringScriptBlock)
                            $result = [System.Collections.ArrayList]::new()
                            $editions = $_.CompatiblePSEditions
                            if (-not $editions)
                                $editions = @('Desktop')
                            foreach ($edition in $editions)
                                $result += $edition.Substring(0,4)
                            ($result | Sort-Object) -join ','")
                        .AddScriptBlockColumn("$_.ExportedCommands.Keys")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PSModuleInfo()
                        .AddItemProperty(@"ModuleType")
                        .AddItemProperty(@"Version")
                            PreReleaseStringScriptBlock,
                            label: "PreRelease")
                        .AddItemProperty(@"NestedModules")
                        .AddItemScriptBlock(@"$_.ExportedFunctions.Keys", label: "ExportedFunctions")
                        .AddItemScriptBlock(@"$_.ExportedCmdlets.Keys", label: "ExportedCmdlets")
                        .AddItemScriptBlock(@"$_.ExportedVariables.Keys", label: "ExportedVariables")
                        .AddItemScriptBlock(@"$_.ExportedAliases.Keys", label: "ExportedAliases")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_ExperimentalFeature()
            yield return new FormatViewDefinition("ExperimentalFeature",
                    .AddHeader(Alignment.Left)
                        .AddItemProperty("Name")
                        .AddItemProperty("Enabled")
                        .AddItemProperty("Source")
                        .AddItemProperty("Description")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_BasicHtmlWebResponseObject()
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.BasicHtmlWebResponseObject",
                        .AddItemProperty(@"StatusCode")
                        .AddItemProperty(@"StatusDescription")
                        .AddItemScriptBlock(@"
                                  $result = $_.Content
                                  $result = $result.Substring(0, [Math]::Min($result.Length, 200) )
                                  if($result.Length -eq 200) { $result += ""`u{2026}"" }
                                ", label: "Content")
                                  $result = $_.RawContent
                                ", label: "RawContent")
                        .AddItemProperty(@"Headers")
                        .AddItemProperty(@"Images")
                        .AddItemProperty(@"InputFields")
                        .AddItemProperty(@"Links")
                        .AddItemProperty(@"RawContentLength")
                        .AddItemProperty(@"RelationLink")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_WebResponseObject()
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.WebResponseObject",
                        .AddItemProperty(@"Content")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_Powershell_Utility_FileHashInfo()
            yield return new FormatViewDefinition("Microsoft.PowerShell.Commands.FileHashInfo",
                    .AddHeader(Alignment.Left, width: 15)
                    .AddHeader(Alignment.Left, width: 70)
                        .AddPropertyColumn("Algorithm")
                        .AddPropertyColumn("Hash")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_PSRunspaceDebug()
            yield return new FormatViewDefinition("PSRunspaceDebug>",
                    .AddHeader(Alignment.Left, label: "Name", width: 20)
                    .AddHeader(Alignment.Left, label: "Enabled", width: 10)
                    .AddHeader(Alignment.Left, label: "BreakAll", width: 10)
                        .AddPropertyColumn("RunspaceId")
                        .AddPropertyColumn("RunspaceName")
                        .AddPropertyColumn("BreakAll")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_MarkdownRender_MarkdownOptionInfo()
            yield return new FormatViewDefinition("Microsoft.PowerShell.MarkdownRender.PSMarkdownOptionInfo",
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('Header1')", label: "Header1")
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('Header2')", label: "Header2")
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('Header3')", label: "Header3")
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('Header4')", label: "Header4")
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('Header5')", label: "Header5")
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('Header6')", label: "Header6")
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('Code')", label: "Code")
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('Link')", label: "Link")
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('Image')", label: "Image")
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('EmphasisBold')", label: "EmphasisBold")
                        .AddItemScriptBlock(@"$_.AsEscapeSequence('EmphasisItalics')", label: "EmphasisItalics")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_TestConnectionCommand_TcpPortStatus()
                    .AddHeader(Alignment.Right, label: "Id", width: 4)
                    .AddHeader(Alignment.Left, label: "Source", width: 16)
                    .AddHeader(Alignment.Left, label: "Address", width: 25)
                    .AddHeader(Alignment.Right, label: "Port", width: 7)
                    .AddHeader(Alignment.Right, label: "Latency(ms)", width: 7)
                    .AddHeader(Alignment.Left, label: "Connected", width: 10)
                    .AddHeader(Alignment.Left, label: "Status", width: 24)
                        .AddPropertyColumn("TargetAddress")
                        .AddPropertyColumn("Port")
                        .AddPropertyColumn("Latency")
                        .AddPropertyColumn("Connected")
                    .GroupByProperty("Target")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_TestConnectionCommand_PingStatus()
                    .AddHeader(Alignment.Right, label: "Ping", width: 4)
                    .AddHeader(Alignment.Right, label: "BufferSize(B)", width: 10)
                    .AddHeader(Alignment.Left, label: "Status", width: 16)
                        .AddPropertyColumn("Ping")
                        .AddPropertyColumn("DisplayAddress")
                            if ($_.Status -eq 'TimedOut') {
                                '*'
                                $_.Latency
                                $_.BufferSize
                    .GroupByProperty("Destination")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_TestConnectionCommand_PingMtuStatus()
                    .AddHeader(Alignment.Right, label: "MtuSize(B)", width: 7)
                        .AddPropertyColumn("MtuSize")
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_TestConnectionCommand_TraceStatus()
                    .AddHeader(Alignment.Right, label: "Hop", width: 3)
                    .AddHeader(Alignment.Left, label: "Hostname", width: 25)
                    .AddHeader(Alignment.Left, label: "Source", width: 12)
                    .AddHeader(Alignment.Left, label: "TargetAddress", width: 15)
                        .AddPropertyColumn("Hop")
                            if ($_.Hostname) {
                                $_.HostName
        private static IEnumerable<FormatViewDefinition> ViewsOf_Microsoft_PowerShell_Commands_ByteCollection()
                    .AddHeader(Alignment.Right, label: "Offset", width: 16)
                    .AddHeader(Alignment.Left, label: "Bytes\n00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F", width: 47)
                    .AddHeader(Alignment.Left, label: "Ascii", width: 16)
                        .AddPropertyColumn("HexOffset")
                        .AddPropertyColumn("HexBytes")
                        .AddPropertyColumn("Ascii")
                    .GroupByProperty("Label")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PSStyle()
            yield return new FormatViewDefinition("System.Management.Automation.PSStyle",
                        .AddItemScriptBlock(@"""$($_.Reset)$($_.Reset.Replace(""""`e"""",'`e'))""", label: "Reset")
                        .AddItemScriptBlock(@"""$($_.BlinkOff)$($_.BlinkOff.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "BlinkOff")
                        .AddItemScriptBlock(@"""$($_.Blink)$($_.Blink.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "Blink")
                        .AddItemScriptBlock(@"""$($_.BoldOff)$($_.BoldOff.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "BoldOff")
                        .AddItemScriptBlock(@"""$($_.Bold)$($_.Bold.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "Bold")
                        .AddItemScriptBlock(@"""$($_.DimOff)$($_.DimOff.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "DimOff")
                        .AddItemScriptBlock(@"""$($_.Dim)$($_.Dim.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "Dim")
                        .AddItemScriptBlock(@"""$($_.Hidden)$($_.Hidden.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "Hidden")
                        .AddItemScriptBlock(@"""$($_.HiddenOff)$($_.HiddenOff.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "HiddenOff")
                        .AddItemScriptBlock(@"""$($_.Reverse)$($_.Reverse.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "Reverse")
                        .AddItemScriptBlock(@"""$($_.ReverseOff)$($_.ReverseOff.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "ReverseOff")
                        .AddItemScriptBlock(@"""$($_.ItalicOff)$($_.ItalicOff.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "ItalicOff")
                        .AddItemScriptBlock(@"""$($_.Italic)$($_.Italic.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "Italic")
                        .AddItemScriptBlock(@"""$($_.UnderlineOff)$($_.UnderlineOff.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "UnderlineOff")
                        .AddItemScriptBlock(@"""$($_.Underline)$($_.Underline.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "Underline")
                        .AddItemScriptBlock(@"""$($_.StrikethroughOff)$($_.StrikethroughOff.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "StrikethroughOff")
                        .AddItemScriptBlock(@"""$($_.Strikethrough)$($_.Strikethrough.Replace(""""`e"""",'`e'))$($_.Reset)""", label: "Strikethrough")
                        .AddItemProperty(@"OutputRendering")
                        .AddItemScriptBlock(@"""$($_.Formatting.FormatAccent)$($_.Formatting.FormatAccent.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.FormatAccent")
                        .AddItemScriptBlock(@"""$($_.Formatting.ErrorAccent)$($_.Formatting.ErrorAccent.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.ErrorAccent")
                        .AddItemScriptBlock(@"""$($_.Formatting.Error)$($_.Formatting.Error.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.Error")
                        .AddItemScriptBlock(@"""$($_.Formatting.Warning)$($_.Formatting.Warning.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.Warning")
                        .AddItemScriptBlock(@"""$($_.Formatting.Verbose)$($_.Formatting.Verbose.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.Verbose")
                        .AddItemScriptBlock(@"""$($_.Formatting.Debug)$($_.Formatting.Debug.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.Debug")
                        .AddItemScriptBlock(@"""$($_.Formatting.TableHeader)$($_.Formatting.TableHeader.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.TableHeader")
                        .AddItemScriptBlock(@"""$($_.Formatting.CustomTableHeaderLabel)$($_.Formatting.CustomTableHeaderLabel.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.CustomTableHeaderLabel")
                        .AddItemScriptBlock(@"""$($_.Formatting.FeedbackName)$($_.Formatting.FeedbackName.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.FeedbackName")
                        .AddItemScriptBlock(@"""$($_.Formatting.FeedbackText)$($_.Formatting.FeedbackText.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.FeedbackText")
                        .AddItemScriptBlock(@"""$($_.Formatting.FeedbackAction)$($_.Formatting.FeedbackAction.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Formatting.FeedbackAction")
                        .AddItemScriptBlock(@"""$($_.Progress.Style)$($_.Progress.Style.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Progress.Style")
                        .AddItemScriptBlock(@"""$($_.Progress.MaxWidth)""", label: "Progress.MaxWidth")
                        .AddItemScriptBlock(@"""$($_.Progress.View)""", label: "Progress.View")
                        .AddItemScriptBlock(@"""$($_.Progress.UseOSCIndicator)""", label: "Progress.UseOSCIndicator")
                        .AddItemScriptBlock(@"""$($_.FileInfo.Directory)$($_.FileInfo.Directory.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "FileInfo.Directory")
                        .AddItemScriptBlock(@"""$($_.FileInfo.SymbolicLink)$($_.FileInfo.SymbolicLink.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "FileInfo.SymbolicLink")
                        .AddItemScriptBlock(@"""$($_.FileInfo.Executable)$($_.FileInfo.Executable.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "FileInfo.Executable")
                        .AddItemScriptBlock(@"""$([string]::Join(',',$_.FileInfo.Extension.Keys))""", label: "FileInfo.Extension")
                        .AddItemScriptBlock(@"""$($_.Foreground.Black)$($_.Foreground.Black.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.Black")
                        .AddItemScriptBlock(@"""$($_.Foreground.BrightBlack)$($_.Foreground.BrightBlack.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.BrightBlack")
                        .AddItemScriptBlock(@"""$($_.Foreground.White)$($_.Foreground.White.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.White")
                        .AddItemScriptBlock(@"""$($_.Foreground.BrightWhite)$($_.Foreground.BrightWhite.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.BrightWhite")
                        .AddItemScriptBlock(@"""$($_.Foreground.Red)$($_.Foreground.Red.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.Red")
                        .AddItemScriptBlock(@"""$($_.Foreground.BrightRed)$($_.Foreground.BrightRed.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.BrightRed")
                        .AddItemScriptBlock(@"""$($_.Foreground.Magenta)$($_.Foreground.Magenta.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.Magenta")
                        .AddItemScriptBlock(@"""$($_.Foreground.BrightMagenta)$($_.Foreground.BrightMagenta.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.BrightMagenta")
                        .AddItemScriptBlock(@"""$($_.Foreground.Blue)$($_.Foreground.Blue.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.Blue")
                        .AddItemScriptBlock(@"""$($_.Foreground.BrightBlue)$($_.Foreground.BrightBlue.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.BrightBlue")
                        .AddItemScriptBlock(@"""$($_.Foreground.Cyan)$($_.Foreground.Cyan.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.Cyan")
                        .AddItemScriptBlock(@"""$($_.Foreground.BrightCyan)$($_.Foreground.BrightCyan.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.BrightCyan")
                        .AddItemScriptBlock(@"""$($_.Foreground.Green)$($_.Foreground.Green.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.Green")
                        .AddItemScriptBlock(@"""$($_.Foreground.BrightGreen)$($_.Foreground.BrightGreen.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.BrightGreen")
                        .AddItemScriptBlock(@"""$($_.Foreground.Yellow)$($_.Foreground.Yellow.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.Yellow")
                        .AddItemScriptBlock(@"""$($_.Foreground.BrightYellow)$($_.Foreground.BrightYellow.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Foreground.BrightYellow")
                        .AddItemScriptBlock(@"""$($_.Background.Black)$($_.Background.Black.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.Black")
                        .AddItemScriptBlock(@"""$($_.Background.BrightBlack)$($_.Background.BrightBlack.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.BrightBlack")
                        .AddItemScriptBlock(@"""$($_.Background.White)$($_.Background.White.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.White")
                        .AddItemScriptBlock(@"""$($_.Background.BrightWhite)$($_.Background.BrightWhite.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.BrightWhite")
                        .AddItemScriptBlock(@"""$($_.Background.Red)$($_.Background.Red.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.Red")
                        .AddItemScriptBlock(@"""$($_.Background.BrightRed)$($_.Background.BrightRed.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.BrightRed")
                        .AddItemScriptBlock(@"""$($_.Background.Magenta)$($_.Background.Magenta.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.Magenta")
                        .AddItemScriptBlock(@"""$($_.Background.BrightMagenta)$($_.Background.BrightMagenta.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.BrightMagenta")
                        .AddItemScriptBlock(@"""$($_.Background.Blue)$($_.Background.Blue.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.Blue")
                        .AddItemScriptBlock(@"""$($_.Background.BrightBlue)$($_.Background.BrightBlue.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.BrightBlue")
                        .AddItemScriptBlock(@"""$($_.Background.Cyan)$($_.Background.Cyan.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.Cyan")
                        .AddItemScriptBlock(@"""$($_.Background.BrightCyan)$($_.Background.BrightCyan.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.BrightCyan")
                        .AddItemScriptBlock(@"""$($_.Background.Green)$($_.Background.Green.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.Green")
                        .AddItemScriptBlock(@"""$($_.Background.BrightGreen)$($_.Background.BrightGreen.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.BrightGreen")
                        .AddItemScriptBlock(@"""$($_.Background.Yellow)$($_.Background.Yellow.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.Yellow")
                        .AddItemScriptBlock(@"""$($_.Background.BrightYellow)$($_.Background.BrightYellow.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Background.BrightYellow")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PSStyleFormattingData()
            yield return new FormatViewDefinition("System.Management.Automation.PSStyle+FormattingData",
                        .AddItemScriptBlock(@"""$($_.FormatAccent)$($_.FormatAccent.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "FormatAccent")
                        .AddItemScriptBlock(@"""$($_.ErrorAccent)$($_.ErrorAccent.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "ErrorAccent")
                        .AddItemScriptBlock(@"""$($_.Error)$($_.Error.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Error")
                        .AddItemScriptBlock(@"""$($_.Warning)$($_.Warning.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Warning")
                        .AddItemScriptBlock(@"""$($_.Verbose)$($_.Verbose.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Verbose")
                        .AddItemScriptBlock(@"""$($_.Debug)$($_.Debug.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Debug")
                        .AddItemScriptBlock(@"""$($_.TableHeader)$($_.TableHeader.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "TableHeader")
                        .AddItemScriptBlock(@"""$($_.CustomTableHeaderLabel)$($_.CustomTableHeaderLabel.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "CustomTableHeaderLabel")
                        .AddItemScriptBlock(@"""$($_.FeedbackName)$($_.FeedbackName.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "FeedbackName")
                        .AddItemScriptBlock(@"""$($_.FeedbackText)$($_.FeedbackText.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "FeedbackText")
                        .AddItemScriptBlock(@"""$($_.FeedbackAction)$($_.FeedbackAction.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "FeedbackAction")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PSStyleProgressConfiguration()
            yield return new FormatViewDefinition("System.Management.Automation.PSStyle+ProgressConfiguration",
                        .AddItemScriptBlock(@"""$($_.Style)$($_.Style.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Style")
                        .AddItemProperty(@"MaxWidth")
                        .AddItemProperty(@"View")
                        .AddItemProperty(@"UseOSCIndicator")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PSStyleFileInfoFormat()
            yield return new FormatViewDefinition("System.Management.Automation.PSStyle+FileInfoFormatting",
                        .AddItemScriptBlock(@"""$($_.Directory)$($_.Directory.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Directory")
                        .AddItemScriptBlock(@"""$($_.SymbolicLink)$($_.SymbolicLink.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "SymbolicLink")
                        .AddItemScriptBlock(@"""$($_.Executable)$($_.Executable.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Executable")
                            $sb = [System.Text.StringBuilder]::new()
                            $maxKeyLength = 0
                            foreach ($key in $_.Extension.Keys) {
                                if ($key.Length -gt $maxKeyLength) {
                                    $maxKeyLength = $key.Length
                                $null = $sb.Append($key.PadRight($maxKeyLength))
                                $null = $sb.Append(' = ""')
                                $null = $sb.Append($_.Extension[$key])
                                $null = $sb.Append($_.Extension[$key].Replace(""`e"",'`e'))
                                $null = $sb.Append($PSStyle.Reset)
                                $null = $sb.Append('""')
                                $null = $sb.Append([Environment]::NewLine)
                            $sb.ToString()",
                            label: "Extension")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PSStyleForegroundColor()
            yield return new FormatViewDefinition("System.Management.Automation.PSStyle+ForegroundColor",
                        .AddItemScriptBlock(@"""$($_.Black)$($_.Black.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Black")
                        .AddItemScriptBlock(@"""$($_.BrightBlack)$($_.BrightBlack.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "BrightBlack")
                        .AddItemScriptBlock(@"""$($_.White)$($_.White.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "White")
                        .AddItemScriptBlock(@"""$($_.BrightWhite)$($_.BrightWhite.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "BrightWhite")
                        .AddItemScriptBlock(@"""$($_.Red)$($_.Red.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Red")
                        .AddItemScriptBlock(@"""$($_.BrightRed)$($_.BrightRed.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "BrightRed")
                        .AddItemScriptBlock(@"""$($_.Magenta)$($_.Magenta.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Magenta")
                        .AddItemScriptBlock(@"""$($_.BrightMagenta)$($_.BrightMagenta.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "BrightMagenta")
                        .AddItemScriptBlock(@"""$($_.Blue)$($_.Blue.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Blue")
                        .AddItemScriptBlock(@"""$($_.BrightBlue)$($_.BrightBlue.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "BrightBlue")
                        .AddItemScriptBlock(@"""$($_.Cyan)$($_.Cyan.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Cyan")
                        .AddItemScriptBlock(@"""$($_.BrightCyan)$($_.BrightCyan.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "BrightCyan")
                        .AddItemScriptBlock(@"""$($_.Green)$($_.Green.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Green")
                        .AddItemScriptBlock(@"""$($_.BrightGreen)$($_.BrightGreen.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "BrightGreen")
                        .AddItemScriptBlock(@"""$($_.Yellow)$($_.Yellow.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "Yellow")
                        .AddItemScriptBlock(@"""$($_.BrightYellow)$($_.BrightYellow.Replace(""""`e"""",'`e'))$($PSStyle.Reset)""", label: "BrightYellow")
        private static IEnumerable<FormatViewDefinition> ViewsOf_System_Management_Automation_PSStyleBackgroundColor()
            yield return new FormatViewDefinition("System.Management.Automation.PSStyle+BackgroundColor",
