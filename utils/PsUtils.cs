    internal static class PsUtils
        // Cache of the current process' parentId
        private static int? s_currentParentProcessId;
        private static readonly int s_currentProcessId = Environment.ProcessId;
        /// Retrieve the parent process of a process.
        /// Previously this code used WMI, but WMI is causing a CPU spike whenever the query gets called as it results in
        /// tzres.dll and tzres.mui.dll being loaded into every process to convert the time information to local format.
        /// For perf reasons, we resort to P/Invoke.
        /// <param name="current">The process we want to find the
        /// parent of</param>
        internal static Process GetParentProcess(Process current)
            var processId = current.Id;
            // This is a common query (parent id for the current process)
            // Use cached value if available
            var parentProcessId = processId == s_currentProcessId && s_currentParentProcessId.HasValue ?
                 s_currentParentProcessId.Value :
                 Microsoft.PowerShell.ProcessCodeMethods.GetParentPid(current);
            // cache the current process parent pid if it hasn't been done yet
            if (processId == s_currentProcessId && !s_currentParentProcessId.HasValue)
                s_currentParentProcessId = parentProcessId;
            if (parentProcessId == 0)
                Process returnProcess = Process.GetProcessById(parentProcessId);
                // Ensure the process started before the current
                // process, as it could have gone away and had the
                // PID recycled.
                if (returnProcess.StartTime <= current.StartTime)
                    return returnProcess;
                // GetProcessById throws an ArgumentException when
                // you reach the top of the chain -- Explorer.exe
                // has a parent process, but you cannot retrieve it.
        /// Return true/false to indicate whether the process architecture is ARM.
        internal static bool IsRunningOnProcessArchitectureARM()
            Architecture arch = RuntimeInformation.ProcessArchitecture;
            return arch == Architecture.Arm || arch == Architecture.Arm64;
        internal static string GetHostName()
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            string hostname = ipProperties.HostName;
            string domainName = ipProperties.DomainName;
            // CoreFX on Unix calls GLibc getdomainname()
            // which returns "(none)" if a domain name is not set by setdomainname()
            if (!string.IsNullOrEmpty(domainName) && !domainName.Equals("(none)", StringComparison.Ordinal))
                hostname = hostname + "." + domainName;
            return hostname;
        internal static uint GetNativeThreadId()
            return Platform.NonWindowsGetThreadId();
            return Interop.Windows.GetCurrentThreadId();
        #region ASTUtils
        /// This method is to get the unique key for a UsingExpressionAst. The key is a base64
        /// encoded string based on the text of the UsingExpressionAst.
        /// This method is used when handling a script block that contains $using for Invoke-Command.
        /// When run Invoke-Command targeting a machine that runs PSv3 or above, we pass a dictionary
        /// to the remote end that contains the key of each UsingExpressionAst and its value. This method
        /// is used to generate the key.
        /// <param name="usingAst">A using expression.</param>
        /// <returns>Base64 encoded string as the key of the UsingExpressionAst.</returns>
        internal static string GetUsingExpressionKey(Language.UsingExpressionAst usingAst)
            Diagnostics.Assert(usingAst != null, "Caller makes sure the parameter is not null");
            // We cannot call ToLowerInvariant unconditionally, because usingAst might
            // contain IndexExpressionAst in its SubExpression, such as
            //   $using:bar["AAAA"]
            // and the index "AAAA" might not get us the same value as "aaaa".
            // But we do want a unique key to represent the same UsingExpressionAst's as much
            // as possible, so as to avoid sending redundant key-value's to remote machine.
            // As a workaround, we call ToLowerInvariant when the SubExpression of usingAst
            // is a VariableExpressionAst, because:
            //   (1) Variable name is case insensitive;
            //   (2) People use $using to refer to a variable most of the time.
            string usingAstText = usingAst.ToString();
            if (usingAst.SubExpression is Language.VariableExpressionAst)
                usingAstText = usingAstText.ToLowerInvariant();
            return StringToBase64Converter.StringToBase64String(usingAstText);
        #endregion ASTUtils
        #region EvaluatePowerShellDataFile
        /// Evaluate a powershell data file as if it's a module manifest.
        /// <param name="psDataFilePath"></param>
        /// <param name="skipPathValidation"></param>
        internal static Hashtable EvaluatePowerShellDataFileAsModuleManifest(
                                     string psDataFilePath,
                                     bool skipPathValidation)
            // Use the same capabilities as the module manifest
            // e.g. allow 'PSScriptRoot' variable
            return EvaluatePowerShellDataFile(
                      psDataFilePath,
                      Microsoft.PowerShell.Commands.ModuleCmdletBase.PermittedCmdlets,
                      new[] { "PSScriptRoot" },
                      allowEnvironmentVariables: true,
                      skipPathValidation: skipPathValidation);
        /// Get a Hashtable object out of a PowerShell data file (.psd1)
        /// Name of the parameter that takes the specified .psd1 file as a value
        /// <param name="psDataFilePath">
        /// Path to the powershell data file
        /// ExecutionContext to use
        /// <param name="allowedCommands">
        /// Set of command names that are allowed to use in the .psd1 file
        /// Set of variable names that are allowed to use in the .psd1 file
        /// <param name="allowEnvironmentVariables">
        /// If true, allow to use environment variables in the .psd1 file
        /// <param name="skipPathValidation">
        /// If true, caller guarantees the path is valid
        internal static Hashtable EvaluatePowerShellDataFile(
                                     bool allowEnvironmentVariables,
            if (!skipPathValidation && string.IsNullOrEmpty(parameterName))
            if (string.IsNullOrEmpty(psDataFilePath))
                throw PSTraceSource.NewArgumentNullException(nameof(psDataFilePath));
            string resolvedPath;
            if (skipPathValidation)
                resolvedPath = psDataFilePath;
                #region "ValidatePowerShellDataFilePath"
                bool isPathValid = true;
                // File extension needs to be .psd1
                string pathExt = Path.GetExtension(psDataFilePath);
                if (string.IsNullOrEmpty(pathExt) ||
                    !StringLiterals.PowerShellDataFileExtension.Equals(pathExt, StringComparison.OrdinalIgnoreCase))
                    isPathValid = false;
                var resolvedPaths = context.SessionState.Path.GetResolvedProviderPathFromPSPath(psDataFilePath, out provider);
                // ConfigPath should be resolved as FileSystem provider
                if (provider == null || !Microsoft.PowerShell.Commands.FileSystemProvider.ProviderName.Equals(provider.Name, StringComparison.OrdinalIgnoreCase))
                // ConfigPath should be resolved to a single path
                if (resolvedPaths.Count != 1)
                if (!isPathValid)
                             ParserStrings.CannotResolvePowerShellDataFilePath,
                             psDataFilePath);
                #endregion "ValidatePowerShellDataFilePath"
            #region "LoadAndEvaluatePowerShellDataFile"
            object evaluationResult;
                // Create the scriptInfo for the .psd1 file
                string dataFileName = Path.GetFileName(resolvedPath);
                var dataFileScriptInfo = new ExternalScriptInfo(dataFileName, resolvedPath, context);
                ScriptBlock scriptBlock = dataFileScriptInfo.ScriptBlock;
                // Validate the scriptblock
                scriptBlock.CheckRestrictedLanguage(allowedCommands, allowedVariables, allowEnvironmentVariables);
                // Evaluate the scriptblock
                object oldPsScriptRoot = context.GetVariableValue(SpecialVariables.PSScriptRootVarPath);
                    // Set the $PSScriptRoot before the evaluation
                    context.SetVariable(SpecialVariables.PSScriptRootVarPath, Path.GetDirectoryName(resolvedPath));
                    evaluationResult = PSObject.Base(scriptBlock.InvokeReturnAsIs());
                    context.SetVariable(SpecialVariables.PSScriptRootVarPath, oldPsScriptRoot);
                         ParserStrings.CannotLoadPowerShellDataFile,
            if (evaluationResult is not Hashtable retResult)
                         ParserStrings.InvalidPowerShellDataFile,
            #endregion "LoadAndEvaluatePowerShellDataFile"
            return retResult;
        #endregion EvaluatePowerShellDataFile
        internal static readonly string[] ManifestModuleVersionPropertyName = new[] { "ModuleVersion" };
        internal static readonly string[] ManifestGuidPropertyName = new[] { "GUID" };
        internal static readonly string[] ManifestPrivateDataPropertyName = new[] { "PrivateData" };
        internal static readonly string[] FastModuleManifestAnalysisPropertyNames = new[]
        internal static Hashtable GetModuleManifestProperties(string psDataFilePath, string[] keys)
            string dataFileContents = File.ReadAllText(psDataFilePath, Encoding.Default);
            var ast = (new Parser()).Parse(psDataFilePath, dataFileContents, null, out parseErrors, ParseMode.ModuleAnalysis);
            if (parseErrors.Length > 0)
                var pe = new ParseException(parseErrors);
                    pe,
                    pe.Message);
            var pipeline = ast.GetSimplePipeline(false, out _, out _);
            if (pipeline?.GetPureExpression() is HashtableAst hashtableAst)
                    if (pair.Item1 is StringConstantExpressionAst key && keys.Contains(key.Value, StringComparer.OrdinalIgnoreCase))
                            var val = pair.Item2.SafeGetValue();
                            result[key.Value] = val;
    /// This class provides helper methods for converting to/fro from
    /// string to base64string.
    internal static class StringToBase64Converter
        /// Converts string to base64 encoded string.
        /// <param name="input">String to encode.</param>
        /// <returns>Base64 encoded string.</returns>
        internal static string StringToBase64String(string input)
            // NTRAID#Windows Out Of Band Releases-926471-2005/12/27-JonN
            // shell crashes if you pass an empty script block to a native command
                throw PSTraceSource.NewArgumentNullException(nameof(input));
            string base64 = Convert.ToBase64String
                                Encoding.Unicode.GetBytes(input.ToCharArray())
            return base64;
        /// Decodes base64 encoded string.
        /// <param name="base64">Base64 string to decode.</param>
        internal static string Base64ToString(string base64)
            if (string.IsNullOrEmpty(base64))
                throw PSTraceSource.NewArgumentNullException(nameof(base64));
            string output = new string(Encoding.Unicode.GetChars(Convert.FromBase64String(base64)));
        /// Decodes base64 encoded string in to args array.
        /// <param name="base64"></param>
        internal static object[] Base64ToArgsConverter(string base64)
            string decoded = new string(Encoding.Unicode.GetChars(Convert.FromBase64String(base64)));
            // Deserialize string
            XmlReader reader = XmlReader.Create(new StringReader(decoded), InternalDeserializer.XmlReaderSettingsForCliXml);
            object dso;
            Deserializer deserializer = new Deserializer(reader);
            dso = deserializer.Deserialize();
            if (!deserializer.Done())
                // This helper function should move to host and it should provide appropriate
                // error message there.
                throw PSTraceSource.NewArgumentException(MinishellParameterBinderController.ArgsParameter);
            if (dso is not PSObject mo)
                // This helper function should move the host. Provide appropriate error message.
                // Format of args parameter is not correct.
            if (mo.BaseObject is not ArrayList argsList)
            return argsList.ToArray();
    /// A simple implementation of CRC32.
    /// See "CRC-32 algorithm" in https://en.wikipedia.org/wiki/Cyclic_redundancy_check.
    internal static class CRC32Hash
        // CRC-32C polynomial representations
        private const uint polynomial = 0x1EDC6F41;
        private static readonly uint[] table;
        static CRC32Hash()
            uint temp = 0;
            table = new uint[256];
            for (int i = 0; i < table.Length; i++)
                temp = (uint)i;
                for (int j = 0; j < 8; j++)
                    if ((temp & 1) == 1)
                        temp = (temp >> 1) ^ polynomial;
                        temp >>= 1;
                table[i] = temp;
        private static uint Compute(byte[] buffer)
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < buffer.Length; ++i)
                var index = (byte)(crc ^ buffer[i] & 0xff);
                crc = (crc >> 8) ^ table[index];
            return ~crc;
        internal static byte[] ComputeHash(byte[] buffer)
            uint crcResult = Compute(buffer);
            return BitConverter.GetBytes(crcResult);
        internal static string ComputeHash(string input)
            byte[] hashBytes = ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hashBytes);
    #region ReferenceEqualityComparer
    /// Equality comparer based on Object Identity.
    internal class ReferenceEqualityComparer : IEqualityComparer
        bool IEqualityComparer.Equals(object x, object y)
            return Object.ReferenceEquals(x, y);
        int IEqualityComparer.GetHashCode(object obj)
            // The Object.GetHashCode and RuntimeHelpers.GetHashCode methods are used in the following scenarios:
            // Object.GetHashCode is useful in scenarios where you care about object value. Two strings with identical
            // contents will return the same value for Object.GetHashCode.
            // RuntimeHelpers.GetHashCode is useful in scenarios where you care about object identity. Two strings with
            // identical contents will return different values for RuntimeHelpers.GetHashCode, because they are different
            // string objects, although their contents are the same.
            return RuntimeHelpers.GetHashCode(obj);
