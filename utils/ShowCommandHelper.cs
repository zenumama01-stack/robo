    /// Implements the WPF window part of the show-command cmdlet.
    internal class ShowCommandHelper : IDisposable
        #region fields
        internal const string CommandTypeSegment = " -CommandType Cmdlet, Function, Script, ExternalScript";
        /// Method that will return the dialog from ShowAllModulesWindow or ShowCommandWindow.
        /// This is necessary because the PlainInvokeAndShowDialog thread starter cannot receive parameters
        private DispatcherOperationCallback methodThatReturnsDialog;
        /// Event set when the window is closed.
        private AutoResetEvent windowClosed = new AutoResetEvent(false);
        /// Event set when help is needed.
        private AutoResetEvent helpNeeded = new AutoResetEvent(false);
        /// Event set when it is necessary to import a module.
        private AutoResetEvent importModuleNeeded = new AutoResetEvent(false);
        /// Event set when the window is loaded.
        private AutoResetEvent windowLoaded = new AutoResetEvent(false);
        /// String with the command that needs help set when helpNeeded is set.
        private string commandNeedingHelp;
        /// String with the command name that needs to import a module.
        private string commandNeedingImportModule;
        /// String with the module name that needs to be imported.
        private string parentModuleNeedingImportModule;
        /// String with the selected module at the time a module needs to be imported.
        private string selectedModuleNeedingImportModule;
        /// Keeps the window for the implementation of CloseWindow.
        private Window window;
        /// host window, if any.
        private Window hostWindow;
        /// ViewModel when showing all modules.
        private AllModulesViewModel allModulesViewModel;
        /// ViewModel when showing a single command.
        private CommandViewModel commandViewModel;
        /// true when the window is closed with cancel.
        private bool dialogCanceled = true;
        #endregion fields
        #region GetSerializedCommand script
        private const string ScriptGetSerializedCommand = @"
Function PSGetSerializedShowCommandInfo
    Function GetParameterType
        param (
            [Type] $parameterType)
        $returnParameterType = new-object PSObject
        $returnParameterType | Add-Member -MemberType NoteProperty -Name ""FullName"" -Value $parameterType.FullName
        $returnParameterType | Add-Member -MemberType NoteProperty -Name ""IsEnum"" -Value $parameterType.IsEnum
        $returnParameterType | Add-Member -MemberType NoteProperty -Name ""IsArray"" -Value $parameterType.IsArray
        if ($parameterType.IsEnum)
            $enumValues = [System.Enum]::GetValues($parameterType)
            $enumValues = [string[]] @()
        $returnParameterType | Add-Member -MemberType NoteProperty -Name ""EnumValues"" -Value $enumValues
        if ($parameterType.IsArray)
            $hasFlagAttribute = ($parameterType.GetCustomAttributes([System.FlagsAttribute], $true).Length -gt 0)
            # Recurse into array elements.
            $elementType = GetParameterType($parameterType.GetElementType())
            $hasFlagAttribute = $false
            $elementType = $null
        $returnParameterType | Add-Member -MemberType NoteProperty -Name ""HasFlagAttribute"" -Value $hasFlagAttribute
        $returnParameterType | Add-Member -MemberType NoteProperty -Name ""ElementType"" -Value $elementType
        if (!($parameterType.IsEnum) -and !($parameterType.IsArray))
            $implementsDictionary = [System.Collections.IDictionary].IsAssignableFrom($parameterType)
            $implementsDictionary = $false
        $returnParameterType | Add-Member -MemberType NoteProperty -Name ""ImplementsDictionary"" -Value $implementsDictionary
        return $returnParameterType
    Function GetParameterInfo
            $parameters)
        [PSObject[]] $parameterInfos = @()
        foreach ($parameter in $parameters)
            $parameterInfo = new-object PSObject
            $parameterInfo | Add-Member -MemberType NoteProperty -Name ""Name"" -Value $parameter.Name
            $parameterInfo | Add-Member -MemberType NoteProperty -Name ""IsMandatory"" -Value $parameter.IsMandatory
            $parameterInfo | Add-Member -MemberType NoteProperty -Name ""ValueFromPipeline"" -Value $parameter.ValueFromPipeline
            $parameterInfo | Add-Member -MemberType NoteProperty -Name ""Position"" -Value $parameter.Position
            $parameterInfo | Add-Member -MemberType NoteProperty -Name ""ParameterType"" -Value (GetParameterType($parameter.ParameterType))
            $hasParameterSet = $false
            [string[]] $validValues = @()
            if ($PSVersionTable.PSVersion.Major -gt 2)
                $validateSetAttributes = $parameter.Attributes | Where {
                    [ValidateSet].IsAssignableFrom($_.GetType())
                if (($validateSetAttributes -ne $null) -and ($validateSetAttributes.Count -gt 0))
                    $hasParameterSet = $true
                    $validValues = $validateSetAttributes[0].ValidValues
            $parameterInfo | Add-Member -MemberType NoteProperty -Name ""HasParameterSet"" -Value $hasParameterSet
            $parameterInfo | Add-Member -MemberType NoteProperty -Name ""ValidParamSetValues"" -Value $validValues
            $parameterInfos += $parameterInfo
        return (,$parameterInfos)
    Function GetParameterSets
            [System.Management.Automation.CommandInfo] $cmdInfo
        $parameterSets = $null
            $parameterSets = $cmdInfo.ParameterSets
        catch [System.InvalidOperationException] { }
        catch [System.Management.Automation.PSNotSupportedException] { }
        catch [System.Management.Automation.PSNotImplementedException] { }
        if (($parameterSets -eq $null) -or ($parameterSets.Count -eq 0))
            return (,@())
        [PSObject[]] $returnParameterSets = @()
        foreach ($parameterSet in $parameterSets)
            $parameterSetInfo = new-object PSObject
            $parameterSetInfo | Add-Member -MemberType NoteProperty -Name ""Name"" -Value $parameterSet.Name
            $parameterSetInfo | Add-Member -MemberType NoteProperty -Name ""IsDefault"" -Value $parameterSet.IsDefault
            $parameterSetInfo | Add-Member -MemberType NoteProperty -Name ""Parameters"" -Value (GetParameterInfo($parameterSet.Parameters))
            $returnParameterSets += $parameterSetInfo
        return (,$returnParameterSets)
    Function GetModuleInfo
        if ($cmdInfo.ModuleName -ne $null)
            $moduleName = $cmdInfo.ModuleName
            $moduleName = """"
        $moduleInfo = new-object PSObject
        $moduleInfo | Add-Member -MemberType NoteProperty -Name ""Name"" -Value $moduleName
        return $moduleInfo
    Function ConvertToShowCommandInfo
        $showCommandInfo = new-object PSObject
        $showCommandInfo | Add-Member -MemberType NoteProperty -Name ""Name"" -Value $cmdInfo.Name
        $showCommandInfo | Add-Member -MemberType NoteProperty -Name ""ModuleName"" -Value $cmdInfo.ModuleName
        $showCommandInfo | Add-Member -MemberType NoteProperty -Name ""Module"" -Value (GetModuleInfo($cmdInfo))
        $showCommandInfo | Add-Member -MemberType NoteProperty -Name ""CommandType"" -Value $cmdInfo.CommandType
        $showCommandInfo | Add-Member -MemberType NoteProperty -Name ""Definition"" -Value $cmdInfo.Definition
        $showCommandInfo | Add-Member -MemberType NoteProperty -Name ""ParameterSets"" -Value (GetParameterSets($cmdInfo))
        return $showCommandInfo
    $commandList = @(""Cmdlet"", ""Function"", ""Script"", ""ExternalScript"")
        $commandList += ""Workflow""
    foreach ($command in @(Get-Command -CommandType $commandList))
        Write-Output (ConvertToShowCommandInfo($command))
}";
        #region constructor and destructor
        /// Prevents a default instance of the ShowCommandHelper class from being created.
        private ShowCommandHelper()
        /// Finalizes an instance of the <see cref="ShowCommandHelper"/> class.
        ~ShowCommandHelper()
            this.Dispose(false);
        #endregion constructor and destructor
        #region properties called using reflection
        /// Gets the Screen Width.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called using reflection")]
        private static double ScreenWidth
                return System.Windows.SystemParameters.PrimaryScreenWidth;
        /// Gets the Screen Height.
        private static double ScreenHeight
                return System.Windows.SystemParameters.PrimaryScreenHeight;
        /// Gets the event set when the show-command window is closed.
        private AutoResetEvent WindowClosed
                return this.windowClosed;
        /// Gets the event set when help is needed for a command.
        private AutoResetEvent HelpNeeded
                return this.helpNeeded;
        /// Gets the event set when it is necessary to import a module.
        private AutoResetEvent ImportModuleNeeded
                return this.importModuleNeeded;
        /// Gets the event set when the window is loaded.
        private AutoResetEvent WindowLoaded
                return this.windowLoaded;
        /// Gets the command needing help when HelpNeeded is set.
        private string CommandNeedingHelp
                return this.commandNeedingHelp;
        /// Gets the module we want to import.
        private string ParentModuleNeedingImportModule
                return this.parentModuleNeedingImportModule;
        /// Gets a value indicating whether there is a host window.
        private bool HasHostWindow
                return this.hostWindow != null;
        #endregion properties called using reflection
        #region public Dispose
        /// Dispose method in IDisposeable.
            this.Dispose(true);
        #endregion public Dispose
        #region internal static methods called using reflection from show-command
        /// Sets the text in the clipboard.
        /// <param name="text">Text to set the clipboard to.</param>
        internal static void SetClipboardText(string text)
                Clipboard.SetText(text);
            catch (System.Runtime.InteropServices.COMException)
                // This is the recommended way to set clipboard text
                System.Threading.Thread.Sleep(0);
        /// Gets the command to be run to get commands and imported modules.
        /// <param name="isRemoteRunspace">Boolean flag determining whether Show-Command is queried in the local or remote runspace scenario.</param>
        /// <param name="isFirstChance">Boolean flag to indicate that it is the second attempt to query Show-Command data.</param>
        /// <returns>The command to be run to get commands and imported modules.</returns>
        internal static string GetShowAllModulesCommand(bool isRemoteRunspace = false, bool isFirstChance = true)
            string scriptBase;
            if (isRemoteRunspace)
                if (isFirstChance)
                    // Return command to run.
                    scriptBase = "@(Get-Command " + ShowCommandHelper.CommandTypeSegment + @" -ShowCommandInfo)";
                    // Return script to run.
                    scriptBase = GetSerializedCommandScript();
                scriptBase = "@(Get-Command " + ShowCommandHelper.CommandTypeSegment + ")";
            scriptBase += ShowCommandHelper.GetGetModuleSuffix();
            return scriptBase;
        /// Retrieves the script for Get-SerializedCommand from local machine.
        /// <returns>String representation of the script for Get-SerializedCommand.</returns>
        private static string GetSerializedCommandScript()
                "@({0};{1};{2})",
                ScriptGetSerializedCommand,
                @"PSGetSerializedShowCommandInfo",
                @"Remove-Item -Path 'function:\PSGetSerializedShowCommandInfo' -Force");
        /// Gets the command to be run in order to show help for a command.
        /// <param name="command">Command we want to get help from.</param>
        /// <returns>The command to be run in order to show help for a command.</returns>
        internal static string GetHelpCommand(string command)
            return "Get-Help " + ShowCommandHelper.SingleQuote(command);
        /// Constructs a dictionary of imported modules based on the module names.
        /// <param name="moduleObjects">The imported modules.</param>
        /// <returns>a dictionary of imported modules based on the module names.</returns>
        internal static Dictionary<string, ShowCommandModuleInfo> GetImportedModulesDictionary(object[] moduleObjects)
            Dictionary<string, ShowCommandModuleInfo> returnValue = new Dictionary<string, ShowCommandModuleInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (PSObject rawModule in moduleObjects)
                ShowCommandModuleInfo wrappedModule = null;
                PSModuleInfo module = rawModule.BaseObject as PSModuleInfo;
                if (module != null)
                    wrappedModule = new ShowCommandModuleInfo(module);
                    wrappedModule = new ShowCommandModuleInfo(rawModule);
                // It is probably an issue somewhere else that a module would show up twice in the list, but we want to avoid
                // throwing an exception regarding that in returnValue.Add
                if (!returnValue.ContainsKey(wrappedModule.Name))
                    returnValue.Add(wrappedModule.Name, wrappedModule);
        /// Constructs a list of commands out of <paramref name="commandObjects"/>.
        /// <param name="commandObjects">The results of a get-command command.</param>
        /// <returns>a list of commands out of <paramref name="commandObjects"/>.</returns>
        internal static List<ShowCommandCommandInfo> GetCommandList(object[] commandObjects)
            List<ShowCommandCommandInfo> returnValue = new List<ShowCommandCommandInfo>();
            foreach (PSObject rawCommand in commandObjects)
                CommandInfo command = rawCommand.BaseObject as CommandInfo;
                if (command != null)
                    returnValue.Add(new ShowCommandCommandInfo(command));
                    PSObject obj = rawCommand as PSObject;
                    if (obj != null)
                        returnValue.Add(new ShowCommandCommandInfo(obj));
        /// Constructs an array of objects out of <paramref name="commandObjects"/>.
        /// <param name="commandObjects">The result of a get-command command.</param>
        /// <returns>An array of objects out of <paramref name="commandObjects"/>.</returns>
        internal static object[] ObjectArrayFromObjectCollection(object commandObjects)
            object[] objectArray = commandObjects as object[] ?? ((System.Collections.ArrayList)commandObjects).ToArray();
            return objectArray;
        /// Called after a module in <paramref name="oldViewModel"/> is imported to refresh the view model.
        /// Gets a new AllModulesViewModel populated with <paramref name="importedModules"/> and <paramref name="commands"/>.
        /// The <paramref name="oldViewModel"/> is used to cleanup event listening in the old view model and to copy NoCommonParameters.
        /// The new ViewModel will have the command selected according to <paramref name="selectedModuleNeedingImportModule"/>,
        /// <paramref name="parentModuleNeedingImportModule"/> and <paramref name="commandNeedingImportModule"/>.
        /// <param name="oldViewModel">The viewModel before the module was imported.</param>
        /// <param name="importedModules">The list of imported modules.</param>
        /// <param name="commands">The list of commands.</param>
        /// <param name="selectedModuleNeedingImportModule">The name of the module that was selected in <paramref name="oldViewModel"/>.</param>
        /// <param name="parentModuleNeedingImportModule">The name of the module that was imported.</param>
        /// <param name="commandNeedingImportModule">The name of the command that was selected in <paramref name="oldViewModel"/>.</param>
        /// <returns>The new ViewModel based on <paramref name="importedModules"/> and <paramref name="commands"/>.</returns>
        internal static AllModulesViewModel GetNewAllModulesViewModel(AllModulesViewModel oldViewModel, Dictionary<string, ShowCommandModuleInfo> importedModules, IEnumerable<ShowCommandCommandInfo> commands, string selectedModuleNeedingImportModule, string parentModuleNeedingImportModule, string commandNeedingImportModule)
            string oldFilter = null;
            if (oldViewModel.SelectedModule != null)
                // this will allow the old view model to stop listening for events before we
                // replace it with a new view model
                oldViewModel.SelectedModule.SelectedCommand = null;
                oldViewModel.SelectedModule = null;
                oldFilter = oldViewModel.CommandNameFilter;
            AllModulesViewModel returnValue = new AllModulesViewModel(importedModules, commands, oldViewModel.NoCommonParameter);
            if (!string.IsNullOrEmpty(oldFilter))
                returnValue.CommandNameFilter = oldFilter;
            if (selectedModuleNeedingImportModule == null || parentModuleNeedingImportModule == null)
            ModuleViewModel moduleToSelect = returnValue.Modules.Find(
                new Predicate<ModuleViewModel>((module) =>
                    return module.Name.Equals(selectedModuleNeedingImportModule, StringComparison.OrdinalIgnoreCase) ? true : false;
            if (moduleToSelect == null)
            returnValue.SelectedModule = moduleToSelect;
            CommandViewModel commandToSelect = moduleToSelect.Commands.Find(
                new Predicate<CommandViewModel>((command) =>
                    return command.ModuleName.Equals(parentModuleNeedingImportModule, StringComparison.OrdinalIgnoreCase) &&
                        command.Name.Equals(commandNeedingImportModule, StringComparison.OrdinalIgnoreCase) ? true : false;
            if (commandToSelect == null)
            moduleToSelect.SelectedCommand = commandToSelect;
        /// Gets an error message to be displayed when failed to import a module.
        /// <param name="command">Command belonging to the module to import.</param>
        /// <param name="module">Module to import.</param>
        /// <param name="error">Error importing the module.</param>
        /// <returns>An error message to be displayed when failed to import a module.</returns>
        internal static string GetImportModuleFailedMessage(string command, string module, string error)
                ShowCommandResources.ImportModuleFailedFormat,
                command,
                module,
                error);
        /// Single quotes <paramref name="str"/>.
        /// <param name="str">String to quote.</param>
        /// <returns><paramref name="str"/> single quoted.</returns>
        internal static string SingleQuote(string str)
                str = string.Empty;
            return "\'" + System.Management.Automation.Language.CodeGeneration.EscapeSingleQuotedStringContent(str) + "\'";
        #endregion internal static methods called using reflection from show-command
        #region internal static methods used internally in this assembly
        /// Gets the host window, if it is present or null if it is not.
        /// <returns>The host window, if it is present or null if it is not.</returns>
        internal static Window GetHostWindow(PSCmdlet cmdlet)
            // The value of 'PrivateData' property may be null for the default host or a custom host.
            PSPropertyInfo windowProperty = cmdlet.Host.PrivateData?.Properties["Window"];
            if (windowProperty == null)
                return windowProperty.Value as Window;
        #endregion internal static methods used internally in this assembly
        #region static private methods used only on this file
        /// Gets a property value using reflection.
        /// <param name="type">Type containing the property.</param>
        /// <param name="obj">Object containing the property (null for a static property).</param>
        /// <param name="propertyName">Name of property to get.</param>
        /// <param name="bindingFlags">Flags passed to reflection.</param>
        /// Property value or null if it was not able to retrieve it. This method is not suitable to return a property value that might be null.
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from a method called using reflection")]
        private static object GetPropertyValue(Type type, object obj, string propertyName, BindingFlags bindingFlags)
            PropertyInfo property = type.GetProperty(propertyName, bindingFlags);
                return property.GetValue(obj, Array.Empty<object>());
            catch (TargetException)
            catch (TargetParameterCountException)
            catch (MethodAccessException)
            catch (TargetInvocationException)
        /// Sets a property value using reflection.
        /// <param name="propertyName">Name of property to set.</param>
        /// <param name="value">Value to set the property with.</param>
        /// <returns>True if it was able to set.</returns>
        private static bool SetPropertyValue(Type type, object obj, string propertyName, object value, BindingFlags bindingFlags)
                property.SetValue(obj, value, Array.Empty<object>());
        /// Gets the suffix that adds imported modules to a command.
        /// <returns>The suffix that adds imported modules to a command.</returns>
        private static string GetGetModuleSuffix()
            return ",@(get-module)";
        #endregion static private methods used only on this file
        #region private methods called using reflection from show-command
        /// Gets the command to be run when calling show-command for a particular command.
        /// <param name="commandName">The particular command we are running show-command on.</param>
        /// <param name="includeAliasAndModules">True if we want to include aliases and retrieve modules.</param>
        /// <returns>The command to be run when calling show-command for a particular command.</returns>
        private static string GetShowCommandCommand(string commandName, bool includeAliasAndModules)
            string quotedCommandName = ShowCommandHelper.SingleQuote(commandName);
            return "@(get-command " + quotedCommandName + " " + ShowCommandHelper.CommandTypeSegment +
                (includeAliasAndModules ? ",Alias" : string.Empty) + ")" +
                (includeAliasAndModules ? ShowCommandHelper.GetGetModuleSuffix() : string.Empty);
        /// Gets a CommandViewModel of a CommandInfo.
        /// <param name="command">Command we want to get a CommandViewModel of.</param>
        /// <param name="noCommonParameter">True if we do not want common parameters.</param>
        /// <param name="moduleQualify">True to qualify command with module name in GetScript.</param>
        /// <returns>A CommandViewModel of a CommandInfo.</returns>
        private static object GetCommandViewModel(ShowCommandCommandInfo command, bool noCommonParameter, Dictionary<string, ShowCommandModuleInfo> importedModules, bool moduleQualify)
            CommandViewModel returnValue = CommandViewModel.GetCommandViewModel(new ModuleViewModel(command.ModuleName, importedModules), command, noCommonParameter);
            returnValue.ModuleQualifyCommandName = moduleQualify;
        /// Dispatches a message to the window for it to activate.
        /// <param name="window">Window to be activated.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from ActivateWindow() which is called using reflection")]
        private static void ActivateWindow(Window window)
            window.Dispatcher.Invoke(
                    (_) => window.Activate()),
        /// Shows the window listing cmdlets.
        /// <param name="commands">Commands to be listed.</param>
        /// <param name="noCommonParameter">True if we should not show common parameters.</param>
        /// <param name="windowWidth">Window width.</param>
        /// <param name="windowHeight">Window height.</param>
        /// <param name="passThrough">True if the GUI should mention ok instead of run.</param>
        private void ShowAllModulesWindow(PSCmdlet cmdlet, Dictionary<string, ShowCommandModuleInfo> importedModules, IEnumerable<ShowCommandCommandInfo> commands, bool noCommonParameter, double windowWidth, double windowHeight, bool passThrough)
            this.methodThatReturnsDialog = new DispatcherOperationCallback((object ignored) =>
                ShowAllModulesWindow allModulesWindow = new ShowAllModulesWindow();
                this.allModulesViewModel = new AllModulesViewModel(importedModules, commands, noCommonParameter);
                this.SetupButtonEvents(allModulesWindow.Run, allModulesWindow.Copy, allModulesWindow.Cancel, passThrough);
                this.SetupWindow(allModulesWindow);
                this.SetupViewModel();
                    allModulesWindow,
                    ShowCommandSettings.Default.ShowCommandsTop,
                    ShowCommandSettings.Default.ShowCommandsLeft,
                    windowWidth != 0.0 && windowWidth > allModulesWindow.MinWidth ? windowWidth : ShowCommandSettings.Default.ShowCommandsWidth,
                    windowHeight != 0.0 && windowHeight > allModulesWindow.MinHeight ? windowHeight : ShowCommandSettings.Default.ShowCommandsHeight,
                    allModulesWindow.Width,
                    allModulesWindow.Height,
                    ShowCommandSettings.Default.ShowCommandsWindowMaximized);
                return allModulesWindow;
            this.CallShowDialog(cmdlet);
        /// Calls ShowsDialog on methodThatReturnsDialog either in a separate thread or dispatched
        /// to the hostWindow thread if there is a hostWindow
        /// <param name="cmdlet">Cmdlet used to retrieve the host window.</param>
        private void CallShowDialog(PSCmdlet cmdlet)
            this.hostWindow = ShowCommandHelper.GetHostWindow(cmdlet);
            if (this.hostWindow == null)
                Thread guiThread = new Thread(new ThreadStart(this.PlainInvokeAndShowDialog));
            this.hostWindow.Dispatcher.Invoke(
                        Window childWindow = (Window)this.methodThatReturnsDialog.Invoke(null);
                        childWindow.Owner = this.hostWindow;
                        childWindow.Show();
        /// Called from CallMethodThatShowsDialog as the thtead start when there is no host window.
        private void PlainInvokeAndShowDialog()
            ((Window)this.methodThatReturnsDialog.Invoke(null)).ShowDialog();
        /// Shows the window for the cmdlet.
        /// <param name="commandViewModelObj">Command to show in the window.</param>
        private void ShowCommandWindow(PSCmdlet cmdlet, object commandViewModelObj, double windowWidth, double windowHeight, bool passThrough)
                this.commandViewModel = (CommandViewModel)commandViewModelObj;
                ShowCommandWindow showCommandWindow = new ShowCommandWindow();
                this.commandViewModel.HelpNeeded += this.CommandNeedsHelp;
                showCommandWindow.DataContext = this.commandViewModel;
                this.SetupButtonEvents(showCommandWindow.Run, showCommandWindow.Copy, showCommandWindow.Cancel, passThrough);
                this.SetupWindow(showCommandWindow);
                    showCommandWindow,
                    ShowCommandSettings.Default.ShowOneCommandTop,
                    ShowCommandSettings.Default.ShowOneCommandLeft,
                    windowWidth != 0.0 && windowWidth > showCommandWindow.MinWidth ? windowWidth : ShowCommandSettings.Default.ShowOneCommandWidth,
                    windowHeight != 0.0 && windowHeight > showCommandWindow.MinHeight ? windowHeight : ShowCommandSettings.Default.ShowOneCommandHeight,
                    showCommandWindow.Width,
                    showCommandWindow.Height,
                    ShowCommandSettings.Default.ShowOneCommandWindowMaximized);
                return showCommandWindow;
        /// Called when the module importation is done.
        /// <param name="importedModules">All modules currently imported.</param>
        /// <param name="commands">Commands to be displayed.</param>
        private void ImportModuleDone(Dictionary<string, ShowCommandModuleInfo> importedModules, IEnumerable<ShowCommandCommandInfo> commands)
            this.allModulesViewModel.WaitMessageDisplayed = false;
            if (this.window != null)
                this.window.Dispatcher.Invoke(
                        delegate(object ignored)
                            this.allModulesViewModel = ShowCommandHelper.GetNewAllModulesViewModel(
                                this.allModulesViewModel,
                                importedModules,
                                commands,
                                this.selectedModuleNeedingImportModule,
                                this.parentModuleNeedingImportModule,
                                this.commandNeedingImportModule);
        /// Called when the module importation has failed.
        /// <param name="reason">Reason why the module importation failed.</param>
        private void ImportModuleFailed(Exception reason)
                            string message = ShowCommandHelper.GetImportModuleFailedMessage(
                                this.commandNeedingImportModule,
                                reason.Message);
                            MessageBox.Show(this.window, message, ShowCommandResources.ShowCommandError, MessageBoxButton.OK, MessageBoxImage.Error);
        /// Called when the results or get-help are ready in order to display the help window for a command.
        /// <param name="getHelpResults">Results of a get-help call.</param>
        private void DisplayHelp(Collection<PSObject> getHelpResults)
            if (this.window != null && getHelpResults != null && getHelpResults.Count > 0)
                            HelpWindow help = new HelpWindow(getHelpResults[0]);
                            help.Owner = this.window;
                            help.Show();
        /// Activates this.window.
        private void ActivateWindow()
                ShowCommandHelper.ActivateWindow(this.window);
        /// Returns the script to execute if dialog has not been canceled.
        /// <returns>The script to execute if dialog has not been canceled.</returns>
        private string GetScript()
            if (this.dialogCanceled)
            return this.InternalGetScript();
        #endregion private methods called using reflection from show-command
        #region instance private methods used only on this file
        /// Sets up window settings common between the two flavors of show-command.
        /// <param name="commandWindow">The window being displayed.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from ShowAllModulesWindow and ShowCommandWindow which are called with reflection")]
        private void SetupWindow(Window commandWindow)
            this.window = commandWindow;
            this.window.Closed += this.Window_Closed;
            this.window.Loaded += this.Window_Loaded;
        /// Handles the SelectedCommandInSelectedModuleNeedsImportModule event.
        private void CommandNeedsImportModule(object sender, ImportModuleEventArgs e)
            this.commandNeedingImportModule = e.CommandName;
            this.parentModuleNeedingImportModule = e.ParentModuleName;
            this.selectedModuleNeedingImportModule = e.SelectedModuleName;
            this.allModulesViewModel.WaitMessageDisplayed = true;
            this.ImportModuleNeeded.Set();
        /// Handles the SelectedCommandInSelectedModuleNeedsHelp event.
        private void CommandNeedsHelp(object sender, HelpNeededEventArgs e)
            this.commandNeedingHelp = e.CommandName;
            this.HelpNeeded.Set();
        /// Called when the window is closed to set this.dialogCanceled.
        private void Window_Closed(object sender, EventArgs e)
            if (this.hostWindow != null)
                this.hostWindow.Focus();
            this.window = null;
            this.windowClosed.Set();
        /// Called when the window is loaded to set this.Window_Loaded.
        private void Window_Loaded(object sender, RoutedEventArgs e)
            this.window.Loaded -= this.Window_Loaded;
            this.windowLoaded.Set();
        /// Sets up event listening on the buttons.
        /// <param name="run">Button to run command.</param>
        /// <param name="copy">Button to copy command code.</param>
        /// <param name="cancel">Button to close window.</param>
        /// <param name="passThrough">True to change the text of Run to OK.</param>
        private void SetupButtonEvents(Button run, Button copy, Button cancel, bool passThrough)
            if (passThrough)
                run.Content = ShowCommandResources.ActionButtons_Button_Ok;
            run.Click += this.Buttons_RunClick;
            copy.Click += this.Buttons_CopyClick;
            cancel.Click += this.Buttons_CancelClick;
        /// Sets up event listening for a new viewModel.
        private void SetupViewModel()
            this.allModulesViewModel.SelectedCommandInSelectedModuleNeedsHelp += this.CommandNeedsHelp;
            this.allModulesViewModel.SelectedCommandInSelectedModuleNeedsImportModule += this.CommandNeedsImportModule;
            this.window.DataContext = this.allModulesViewModel;
        /// Copies the script into the clipboard.
        private void Buttons_CopyClick(object sender, RoutedEventArgs e)
            string script = this.InternalGetScript();
            if (script == null)
            this.window.Dispatcher.Invoke(new ThreadStart(delegate { ShowCommandHelper.SetClipboardText(script); }));
        /// Sets a successful dialog result and then closes the window.
        private void Buttons_RunClick(object sender, RoutedEventArgs e)
            this.dialogCanceled = false;
            this.CloseWindow();
        private void Buttons_CancelClick(object sender, RoutedEventArgs e)
        private void CloseWindow()
            if (this.window == null)
            this.window.Dispatcher.Invoke(new ThreadStart(delegate
                // This can happen if ISE is closed while show-command is up
                    this.window.Close();
        /// Showing a MessageBox when user type a invalidate command name.
        /// <param name="errorString">Error message.</param>
        private void ShowErrorString(string errorString)
            if (errorString != null && errorString.Trim().Length > 0)
                MessageBox.Show(
                        ShowCommandResources.EndProcessingErrorMessage,
                        errorString),
                     "Show-Command",
                     MessageBoxButton.OK,
                     MessageBoxImage.Error);
        /// Returns the script to execute.
        /// <returns>The script to execute.</returns>
        private string InternalGetScript()
            if (this.allModulesViewModel != null)
                return this.allModulesViewModel.GetScript();
            if (this.commandViewModel == null)
            return this.commandViewModel.GetScript();
        /// Implements IDisposable logic.
        /// <param name="isDisposing">True if being called from Dispose.</param>
        private void Dispose(bool isDisposing)
            if (isDisposing)
                this.windowClosed.Dispose();
                this.helpNeeded.Dispose();
                this.windowLoaded.Dispose();
                this.importModuleNeeded.Dispose();
        #endregion instance private methods used only on this file
