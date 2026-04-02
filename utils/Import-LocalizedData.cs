    /// The implementation of the "import-localizeddata" cmdlet.
    [Cmdlet(VerbsData.Import, "LocalizedData", HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096710")]
    public sealed class ImportLocalizedData : PSCmdlet
        /// The path from which to import the aliases.
        [Alias("Variable")]
        public string BindingVariable
                return _bindingVariable;
                _bindingVariable = value;
        private string _bindingVariable;
        /// The scope to import the aliases to.
        public string UICulture
                return _uiculture;
                _uiculture = value;
        private string _uiculture;
        public string BaseDirectory
                return _baseDirectory;
                _baseDirectory = value;
        private string _baseDirectory;
        public string FileName
                return _fileName;
                _fileName = value;
        private string _fileName;
        /// The command allowed in the data file.  If unspecified, then ConvertFrom-StringData is allowed.
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Cmdlets use arrays for parameters.")]
        public string[] SupportedCommand
                return _commandsAllowed;
                _setSupportedCommand = true;
                _commandsAllowed = value;
        private string[] _commandsAllowed = new string[] { "ConvertFrom-StringData" };
        private bool _setSupportedCommand = false;
            string path = GetFilePath();
            if (path == null)
            if (!File.Exists(path))
                InvalidOperationException ioe =
                    PSTraceSource.NewInvalidOperationException(
                        ImportLocalizedDataStrings.FileNotExist,
                WriteError(new ErrorRecord(ioe, "ImportLocalizedData", ErrorCategory.ObjectNotFound, path));
            // Prevent additional commands in ConstrainedLanguage mode
            if (_setSupportedCommand && Context.LanguageMode == PSLanguageMode.ConstrainedLanguage)
                    NotSupportedException nse =
                        PSTraceSource.NewNotSupportedException(
                            ImportLocalizedDataStrings.CannotDefineSupportedCommand);
                        new ErrorRecord(nse, "CannotDefineSupportedCommand", ErrorCategory.PermissionDenied, null));
                    title: ImportLocalizedDataStrings.WDACLogTitle,
                    message: ImportLocalizedDataStrings.WDACLogMessage,
                    fqid: "SupportedCommandsDisabled",
            string script = GetScript(path);
                var scriptBlock = Context.Engine.ParseScriptBlock(script, false);
                scriptBlock.CheckRestrictedLanguage(SupportedCommand, null, false);
                object result;
                PSLanguageMode oldLanguageMode = Context.LanguageMode;
                Context.LanguageMode = PSLanguageMode.RestrictedLanguage;
                    result = scriptBlock.InvokeReturnAsIs();
                    if (result == AutomationNull.Value)
                    Context.LanguageMode = oldLanguageMode;
                if (_bindingVariable != null)
                    VariablePath variablePath = new(_bindingVariable);
                    if (variablePath.IsUnscopedVariable)
                        variablePath = variablePath.CloneAndSetLocal();
                    if (string.IsNullOrEmpty(variablePath.UnqualifiedPath))
                        InvalidOperationException ioe = PSTraceSource.NewInvalidOperationException(
                            ImportLocalizedDataStrings.IncorrectVariableName, _bindingVariable);
                        WriteError(new ErrorRecord(ioe, "ImportLocalizedData", ErrorCategory.InvalidArgument,
                                                   _bindingVariable));
                    SessionStateScope scope = null;
                    PSVariable variable = SessionState.Internal.GetVariableItem(variablePath, out scope);
                    if (variable == null)
                        variable = new PSVariable(variablePath.UnqualifiedPath, result, ScopedItemOptions.None);
                        Context.EngineSessionState.SetVariable(variablePath, variable, false, CommandOrigin.Internal);
                        variable.Value = result;
                        if (Context.LanguageMode == PSLanguageMode.ConstrainedLanguage)
                            // Mark untrusted values for assignments to 'Global:' variables, and 'Script:' variables in
                            // a module scope, if it's necessary.
                            ExecutionContext.MarkObjectAsUntrustedForVariableAssignment(variable, scope, Context.EngineSessionState);
                // If binding variable is null, write the object to stream
                PSInvalidOperationException ioe = PSTraceSource.NewInvalidOperationException(e,
                    ImportLocalizedDataStrings.ErrorLoadingDataFile,
                    e.Message);
                throw ioe;
        private string GetFilePath()
            if (string.IsNullOrEmpty(_fileName))
                if (InvocationExtent == null || string.IsNullOrEmpty(InvocationExtent.File))
                    throw PSTraceSource.NewInvalidOperationException(ImportLocalizedDataStrings.NotCalledFromAScriptFile);
            string dir = _baseDirectory;
            if (string.IsNullOrEmpty(dir))
                if (InvocationExtent != null && !string.IsNullOrEmpty(InvocationExtent.File))
                    dir = Path.GetDirectoryName(InvocationExtent.File);
                    dir = ".";
            dir = PathUtils.ResolveFilePath(dir, this);
            string fileName = _fileName;
            if (string.IsNullOrEmpty(fileName))
                fileName = InvocationExtent.File;
                if (!string.IsNullOrEmpty(Path.GetDirectoryName(fileName)))
                    throw PSTraceSource.NewInvalidOperationException(ImportLocalizedDataStrings.FileNameParameterCannotHavePath);
            fileName = Path.GetFileNameWithoutExtension(fileName);
            CultureInfo culture;
            if (_uiculture == null)
                culture = CultureInfo.CurrentUICulture;
                    culture = CultureInfo.GetCultureInfo(_uiculture);
                    throw PSTraceSource.NewArgumentException("Culture");
            List<CultureInfo> cultureList = new List<CultureInfo> { culture };
            if (_uiculture == null && culture.Name != "en-US")
                // .NET 4.8 presents en-US as a parent of any current culture when accessed via the CurrentUICulture
                // property.
                // This feature is not present when GetCultureInfo is called, therefore this fallback change only
                // applies when the UICulture parameter is not supplied.
                cultureList.Add(CultureInfo.GetCultureInfo("en-US"));
            string filePath;
            string fullFileName = fileName + ".psd1";
            foreach (CultureInfo cultureToTest in cultureList)
                CultureInfo currentCulture = cultureToTest;
                while (currentCulture != null && !string.IsNullOrEmpty(currentCulture.Name))
                    filePath = Path.Combine(dir, currentCulture.Name, fullFileName);
                    if (File.Exists(filePath))
                        return filePath;
                    currentCulture = currentCulture.Parent;
            filePath = Path.Combine(dir, fullFileName);
                                        ImportLocalizedDataStrings.CannotFindPsd1File,
                                        fullFileName,
                                        Path.Combine(dir, culture.Name)
            WriteError(new ErrorRecord(ioe, "ImportLocalizedData", ErrorCategory.ObjectNotFound,
                                       Path.Combine(dir, culture.Name, fullFileName)));
        private string GetScript(string filePath)
            InvalidOperationException ioe = null;
                // 197751: WR BUG BASH: Powershell: localized text display as garbage
                // leaving the encoding to be decided by the StreamReader. StreamReader
                // will read the preamble and decide proper encoding.
                using (FileStream scriptStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader scriptReader = new(scriptStream))
                    return scriptReader.ReadToEnd();
                ioe = PSTraceSource.NewInvalidOperationException(
                                            ImportLocalizedDataStrings.ErrorOpeningFile,
                                            filePath,
            catch (IOException e)
            catch (NotSupportedException e)
            catch (UnauthorizedAccessException e)
            WriteError(new ErrorRecord(ioe, "ImportLocalizedData", ErrorCategory.OpenError, filePath));
