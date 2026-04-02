// #define LOGENABLE // uncomment this line to enable the log,
// create c:\temp\cim.log before invoking cimcmdlets
using System.IO;
using System.Text.RegularExpressions;
    /// Global Non-localization strings
    internal static class ConstValue
        /// Default computername
        internal static readonly string[] DefaultSessionName = { @"*" };
        /// Empty computername, which will create DCOM session
        internal static readonly string NullComputerName = null;
        /// Empty computername array, which will create DCOM session
        internal static readonly string[] NullComputerNames = { NullComputerName };
        /// localhost computername, which will create WSMAN session
        internal static readonly string LocalhostComputerName = @"localhost";
        /// Default namespace
        internal static readonly string DefaultNameSpace = @"root\cimv2";
        internal static readonly string DefaultQueryDialect = @"WQL";
        /// Name of the note property that controls if "PSComputerName" column is shown.
        internal static readonly string ShowComputerNameNoteProperty = "PSShowComputerName";
        /// Whether given computername is either null or empty
        internal static bool IsDefaultComputerName(string computerName)
            return string.IsNullOrEmpty(computerName);
        /// Get computer names, if it is null then return DCOM one
        /// <param name="computerNames"></param>
        internal static IEnumerable<string> GetComputerNames(IEnumerable<string> computerNames)
            return computerNames ?? NullComputerNames;
        /// Get computer name, if it is null then return default one.
        internal static string GetComputerName(string computerName)
            return string.IsNullOrEmpty(computerName) ? NullComputerName : computerName;
        /// Get namespace, if it is null then return default one
        internal static string GetNamespace(string nameSpace)
            return nameSpace ?? DefaultNameSpace;
        /// Get queryDialect, if it is null then return default query Dialect
        internal static string GetQueryDialectWithDefault(string queryDialect)
            return queryDialect ?? DefaultQueryDialect;
    /// Debug helper class used to dump debug message to log file
    internal static class DebugHelper
        /// Flag used to control generating log message into file.
        internal static bool GenerateLog { get; set; } = true;
        /// Whether the log been initialized.
        private static bool logInitialized = false;
        internal static bool GenerateVerboseMessage { get; set; } = true;
        /// Flag used to control generating message into powershell.
        internal static readonly string logFile = @"c:\temp\Cim.log";
        /// Indent space string.
        internal static readonly string space = @"    ";
        /// Indent space strings array.
        internal static readonly string[] spaces = {
                                              space,
                                              space + space,
                                              space + space + space,
                                              space + space + space + space,
                                              space + space + space + space + space,
        /// Lock the log file.
        internal static readonly object logLock = new();
        #region internal strings
        internal static readonly string runspaceStateChanged = "Runspace {0} state changed to {1}";
        internal static readonly string classDumpInfo = @"Class type is {0}";
        internal static readonly string propertyDumpInfo = @"Property name {0} of type {1}, its value is {2}";
        internal static readonly string defaultPropertyType = @"It is a default property, default value is {0}";
        internal static readonly string propertyValueSet = @"This property value is set by user {0}";
        internal static readonly string addParameterSetName = @"Add parameter set {0} name to cache";
        internal static readonly string removeParameterSetName = @"Remove parameter set {0} name from cache";
        internal static readonly string currentParameterSetNameCount = @"Cache have {0} parameter set names";
        internal static readonly string currentParameterSetNameInCache = @"Cache have parameter set {0} valid {1}";
        internal static readonly string currentnonMandatoryParameterSetInCache = @"Cache have optional parameter set {0} valid {1}";
        internal static readonly string optionalParameterSetNameCount = @"Cache have {0} optional parameter set names";
        internal static readonly string finalParameterSetName = @"------Final parameter set name of the cmdlet is {0}";
        internal static readonly string addToOptionalParameterSet = @"Add to optional ParameterSetNames {0}";
        internal static readonly string startToResolveParameterSet = @"------Resolve ParameterSet Name";
        internal static readonly string reservedString = @"------";
        #region runtime methods
        internal static string GetSourceCodeInformation(bool withFileName, int depth)
            StackTrace trace = new();
            StackFrame frame = trace.GetFrame(depth);
            return string.Create(CultureInfo.CurrentUICulture, $"{frame.GetMethod().DeclaringType.Name}::{frame.GetMethod().Name}        ");
        /// Write message to log file named @logFile.
        internal static void WriteLog(string message)
            WriteLog(message, 0);
        /// Write blank line to log file named @logFile.
        internal static void WriteEmptyLine()
            WriteLog(string.Empty, 0);
        /// Write message to log file named @logFile with args.
        internal static void WriteLog(string message, int indent, params object[] args)
            string outMessage = string.Empty;
            FormatLogMessage(ref outMessage, message, args);
            WriteLog(outMessage, indent);
        /// Write message to log file w/o arguments.
        /// <param name="indent"></param>
        internal static void WriteLog(string message, int indent)
            WriteLogInternal(message, indent, -1);
        internal static void WriteLogEx(string message, int indent, params object[] args)
            WriteLogInternal(string.Empty, 0, -1);
            WriteLogInternal(outMessage, indent, 3);
        internal static void WriteLogEx(string message, int indent)
            WriteLogInternal(message, indent, 3);
        internal static void WriteLogEx()
            WriteLogInternal(string.Empty, 0, 3);
        /// Format the message.
        [Conditional("LOGENABLE")]
        private static void FormatLogMessage(ref string outMessage, string message, params object[] args)
            outMessage = string.Format(CultureInfo.CurrentCulture, message, args);
        /// Write message to log file named @logFile
        /// with indent space ahead of the message.
        /// <param name="nIndent"></param>
        private static void WriteLogInternal(string message, int indent, int depth)
            if (!logInitialized)
                lock (logLock)
                        DebugHelper.GenerateLog = File.Exists(logFile);
                        logInitialized = true;
            if (GenerateLog)
                if (indent < 0)
                    indent = 0;
                if (indent > 5)
                    indent = 5;
                string sourceInformation = string.Empty;
                if (depth != -1)
                    sourceInformation = string.Format(
                        CultureInfo.InvariantCulture,
                        "Thread {0}#{1}:{2}:{3} {4}",
                        Environment.CurrentManagedThreadId,
                        DateTime.Now.Hour,
                        DateTime.Now.Minute,
                        DateTime.Now.Second,
                        GetSourceCodeInformation(true, depth));
                    using (FileStream fs = new(logFile, FileMode.OpenOrCreate))
                    using (StreamWriter writer = new(fs))
                        writer.WriteLineAsync(spaces[indent] + sourceInformation + @"        " + message);
    /// Helper class used to validate given parameter
    internal static class ValidationHelper
        /// Validate the argument is not null.
        /// <param name="obj"></param>
        /// <param name="argumentName"></param>
        public static void ValidateNoNullArgument(object obj, string argumentName)
            ArgumentNullException.ThrowIfNull(obj, argumentName);
        /// Validate the argument is not null and not whitespace.
        public static void ValidateNoNullorWhiteSpaceArgument(string obj, string argumentName)
            if (string.IsNullOrWhiteSpace(obj))
                throw new ArgumentException(argumentName);
        /// Validate that given classname/propertyname is a valid name compliance with DMTF standard.
        /// Only for verifying ClassName and PropertyName argument.
        /// <exception cref="ArgumentException">Throw if the given value is not a valid name (class name or property name).</exception>
        public static string ValidateArgumentIsValidName(string parameterName, string value)
            if (value != null)
                string trimed = value.Trim();
                // The first character should be contained in set: [A-Za-z_]
                // Inner characters should be contained in set: [A-Za-z0-9_]
                Regex regex = new(@"^[a-zA-Z_][a-zA-Z0-9_]*\z");
                if (regex.IsMatch(trimed))
                    DebugHelper.WriteLogEx("A valid name: {0}={1}", 0, parameterName, value);
                    return trimed;
            DebugHelper.WriteLogEx("An invalid name: {0}={1}", 0, parameterName, value);
            throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, CimCmdletStrings.InvalidParameterValue, value, parameterName));
        /// Validate given arry argument contains all valid name (for -SelectProperties).
        /// * is valid for this case.
        /// <exception cref="ArgumentException">Throw if the given value contains any invalid name (class name or property name).</exception>
        public static string[] ValidateArgumentIsValidName(string parameterName, string[] value)
                foreach (string propertyName in value)
                    // * is wild char supported in select properties
                    if ((propertyName != null) && string.Equals(propertyName.Trim(), "*", StringComparison.OrdinalIgnoreCase))
                    ValidationHelper.ValidateArgumentIsValidName(parameterName, propertyName);
    internal static class SecurityUtils
        /// Gets the size of a file.
        /// <param name="filePath">Path to file.</param>
        /// <returns>File size.</returns>
        internal static long GetFileSize(string filePath)
            long size = 0;
            using (FileStream fs = new(filePath, FileMode.Open))
                size = fs.Length;
        /// Present a prompt for a SecureString data.
        /// <param name="hostUI">Ref to host ui interface.</param>
        /// <param name="prompt">Prompt text.</param>
        /// <returns> user input as secure string.</returns>
        internal static SecureString PromptForSecureString(PSHostUserInterface hostUI,
                                                           string prompt)
            SecureString ss = null;
            hostUI.Write(prompt);
            ss = hostUI.ReadLineAsSecureString();
            hostUI.WriteLine(string.Empty);
            return ss;
        /// <param name="resourceStr">Resource string.</param>
        /// <param name="errorId">Error identifier.</param>
        /// <param name="args">Replacement params for resource string formatting.</param>
        ErrorRecord CreateFileNotFoundErrorRecord(string resourceStr,
                    args
            FileNotFoundException e = new(message);
        /// <param name="path">Path that was not found.</param>
        /// <returns>ErrorRecord instance.</returns>
        ErrorRecord CreatePathNotFoundErrorRecord(string path,
                                                  string errorId)
            ItemNotFoundException e = new(path, "PathNotFound", SessionStateStrings.PathNotFound);
        /// Create an error record for 'operation not supported' condition.
        ErrorRecord CreateNotSupportedErrorRecord(string resourceStr,
            string message = StringUtil.Format(resourceStr, args);
            NotSupportedException e = new(message);
        /// <param name="e">Exception to include in ErrorRecord.</param>
        ErrorRecord CreateInvalidArgumentErrorRecord(Exception e,
        /// Convert the specified provider path to a provider path
        /// and make sure that all of the following is true:
        /// -- it represents a FileSystem path
        /// -- it points to a file
        /// -- the file exists.
        /// <param name="cmdlet">Cmdlet instance.</param>
        /// <param name="path">Provider path.</param>
        /// filesystem path if all conditions are true,
        /// null otherwise
        internal static string GetFilePathOfExistingFile(PSCmdlet cmdlet,
                                                         string path)
            string resolvedProviderPath = cmdlet.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
            if (File.Exists(resolvedProviderPath))
                return resolvedProviderPath;
    /// Helper fns.
    internal static class Utils
        /// Converts a given double value to BigInteger via Math.Round().
        /// <param name="d">The value to convert.</param>
        /// <returns>Returns a BigInteger value equivalent to the input value rounded to nearest integer.</returns>
        internal static BigInteger AsBigInt(this double d) => new BigInteger(Math.Round(d));
        internal static bool TryCast(BigInteger value, out byte b)
            if (value < byte.MinValue || value > byte.MaxValue)
                b = 0;
            b = (byte)value;
        internal static bool TryCast(BigInteger value, out sbyte sb)
            if (value < sbyte.MinValue || value > sbyte.MaxValue)
                sb = 0;
            sb = (sbyte)value;
        internal static bool TryCast(BigInteger value, out short s)
            if (value < short.MinValue || value > short.MaxValue)
                s = 0;
            s = (short)value;
        internal static bool TryCast(BigInteger value, out ushort us)
            if (value < ushort.MinValue || value > ushort.MaxValue)
                us = 0;
            us = (ushort)value;
        internal static bool TryCast(BigInteger value, out int i)
            if (value < int.MinValue || value > int.MaxValue)
            i = (int)value;
        internal static bool TryCast(BigInteger value, out uint u)
            if (value < uint.MinValue || value > uint.MaxValue)
                u = 0;
            u = (uint)value;
        internal static bool TryCast(BigInteger value, out long l)
            if (value < long.MinValue || value > long.MaxValue)
                l = 0;
            l = (long)value;
        internal static bool TryCast(BigInteger value, out ulong ul)
            if (value < ulong.MinValue || value > ulong.MaxValue)
                ul = 0;
            ul = (ulong)value;
        internal static bool TryCast(BigInteger value, out decimal dm)
            if (value < (BigInteger)decimal.MinValue || (BigInteger)decimal.MaxValue < value)
                dm = 0;
            dm = (decimal)value;
        internal static bool TryCast(BigInteger value, out double db)
            if (value < (BigInteger)double.MinValue || (BigInteger)double.MaxValue < value)
                db = 0;
            db = (double)value;
        /// Parses a given string or ReadOnlySpan&lt;char&gt; to calculate its value as a binary number.
        /// Assumes input has already been sanitized and only contains zeroes (0) or ones (1).
        /// <param name="digits">Span or string of binary digits. Assumes all digits are either 1 or 0.</param>
        /// <param name="unsigned">
        /// Whether to treat the number as unsigned. When false, respects established conventions
        /// with sign bits for certain input string lengths.
        /// <returns>Returns the value of the binary string as a BigInteger.</returns>
        internal static BigInteger ParseBinary(ReadOnlySpan<char> digits, bool unsigned)
            if (!unsigned)
                if (digits[0] == '0')
                    unsigned = true;
                    switch (digits.Length)
                        // Only accept sign bits at these lengths:
                        case 8: // byte
                        case 16: // short
                        case 32: // int
                        case 64: // long
                        case 96: // decimal
                        case int n when n >= 128: // BigInteger
                            // If we do not flag these as unsigned, bigint assumes a sign bit for any (8 * n) string length
            // Only use heap allocation for very large numbers
            const int MaxStackAllocation = 512;
            // Calculate number of 8-bit bytes needed to hold the input,  rounded up to next whole number.
            int outputByteCount = (digits.Length + 7) / 8;
            Span<byte> outputBytes = outputByteCount <= MaxStackAllocation ? stackalloc byte[outputByteCount] : new byte[outputByteCount];
            int outputByteIndex = outputBytes.Length - 1;
            // We need to be prepared for any partial leading bytes, (e.g., 010|00000011|00101100), or cases
            // where we only have less than 8 bits to work with from the beginning.
            // Walk bytes right to left, stepping one whole byte at a time (if there are any whole bytes).
            int byteWalker;
            for (byteWalker = digits.Length - 1; byteWalker >= 7; byteWalker -= 8)
                // Use bit shifts and binary-or to sum the values in each byte.  These calculations will
                // create values higher than a single byte, but the higher bits will be stripped out when cast
                // to byte.
                // The low bits are added in separately to allow us to strip the higher 'noise' bits before we
                // sum the values using binary-or.
                // Simplified representation of logic:     (byte)( (7)|(6)|(5)|(4) ) | ( ( (3)|(2)|(1)|(0) ) & 0b1111 )
                // N.B.: This code has been tested against a straight for loop iterating through the byte, and in no
                // circumstance was it faster or more effective than this unrolled version.
                outputBytes[outputByteIndex--] =
                    (byte)(
                        ((digits[byteWalker - 7] << 7)
                        | (digits[byteWalker - 6] << 6)
                        | (digits[byteWalker - 5] << 5)
                        | (digits[byteWalker - 4] << 4)
                    | (
                        ((digits[byteWalker - 3] << 3)
                        | (digits[byteWalker - 2] << 2)
                        | (digits[byteWalker - 1] << 1)
                        | (digits[byteWalker])
                        ) & 0b1111
            // With complete bytes parsed, byteWalker is either at the partial byte start index, or at -1
            if (byteWalker >= 0)
                int currentByteValue = 0;
                for (int i = 0; i <= byteWalker; i++)
                    currentByteValue = (currentByteValue << 1) | (digits[i] - '0');
                outputBytes[outputByteIndex] = (byte)currentByteValue;
            return new BigInteger(outputBytes, isUnsigned: unsigned, isBigEndian: true);
        // From System.Web.Util.HashCodeCombiner
        internal static int CombineHashCodes(int h1, int h2)
            return unchecked(((h1 << 5) + h1) ^ h2);
        internal static int CombineHashCodes(int h1, int h2, int h3)
            return CombineHashCodes(CombineHashCodes(h1, h2), h3);
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
            return CombineHashCodes(CombineHashCodes(h1, h2), CombineHashCodes(h3, h4));
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5)
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6)
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6));
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7)
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7));
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6, int h7, int h8)
            return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), CombineHashCodes(h5, h6, h7, h8));
        /// Allowed PowerShell Editions.
        internal static readonly string[] AllowedEditionValues = { "Desktop", "Core" };
        /// Helper fn to check byte[] arg for null.
        /// <param name="arg"> arg to check </param>
        /// <param name="argName"> name of the arg </param>
        internal static void CheckKeyArg(byte[] arg, string argName)
            if (arg == null)
                throw PSTraceSource.NewArgumentNullException(argName);
            // we use AES algorithm which supports key
            // lengths of 128, 192 and 256 bits.
            // We throw ArgumentException if the key is
            // of any other length
            else if (!((arg.Length == 16) ||
                       (arg.Length == 24) ||
                       (arg.Length == 32)))
                throw PSTraceSource.NewArgumentException(argName, Serialization.InvalidKeyLength, argName);
        /// Helper fn to check arg for empty or null.
        /// Throws ArgumentNullException on either condition.
        internal static void CheckArgForNullOrEmpty(string arg, string argName)
            else if (arg.Length == 0)
                throw PSTraceSource.NewArgumentException(argName);
        /// Helper fn to check arg for null.
        internal static void CheckArgForNull(object arg, string argName)
        internal static void CheckSecureStringArg(SecureString arg, string argName)
        internal static string GetStringFromSecureString(SecureString ss)
            IntPtr p = IntPtr.Zero;
                p = Marshal.SecureStringToCoTaskMemUnicode(ss);
                s = Marshal.PtrToStringUni(p);
                if (p != IntPtr.Zero)
                    Marshal.ZeroFreeCoTaskMemUnicode(p);
        /// Gets TypeTable by querying the ExecutionContext stored in
        /// Thread-Local-Storage. This will return null if ExecutionContext
        /// is not available.
        internal static TypeTable GetTypeTableFromExecutionContextTLS()
            ExecutionContext ecFromTLS = Runspaces.LocalPipeline.GetExecutionContextFromTLS();
            return ecFromTLS.TypeTable;
        private static string s_pshome = null;
        /// Get the application base path of the shell from registry.
        internal static string GetApplicationBaseFromRegistry(string shellId)
            bool wantPsHome = (object)shellId == (object)DefaultPowerShellShellID;
            if (wantPsHome && s_pshome != null)
                return s_pshome;
            string engineKeyPath = RegistryStrings.MonadRootKeyPath + "\\" +
                PSVersionInfo.RegistryVersionKey + "\\" + RegistryStrings.MonadEngineKey;
            using (RegistryKey engineKey = Registry.LocalMachine.OpenSubKey(engineKeyPath))
                if (engineKey != null)
                    var result = engineKey.GetValue(RegistryStrings.MonadEngine_ApplicationBase) as string;
                    result = Environment.ExpandEnvironmentVariables(result);
                    if (wantPsHome)
                        Interlocked.CompareExchange(ref s_pshome, null, result);
        private static string s_windowsPowerShellVersion = null;
        /// Get the Windows PowerShell version from registry.
        /// String of Windows PowerShell version from registry.
        internal static string GetWindowsPowerShellVersionFromRegistry()
            if (!string.IsNullOrEmpty(InternalTestHooks.TestWindowsPowerShellVersionString))
                return InternalTestHooks.TestWindowsPowerShellVersionString;
            if (s_windowsPowerShellVersion != null)
                return s_windowsPowerShellVersion;
                    s_windowsPowerShellVersion = engineKey.GetValue(RegistryStrings.MonadEngine_MonadVersion) as string;
        internal static string DefaultPowerShellAppBase => GetApplicationBase(DefaultPowerShellShellID);
        internal static string GetApplicationBase(string shellId)
            // Use the location of SMA.dll as the application base if it exists,
            // otherwise, use the base directory from `AppContext`.
            var baseDirectory = Path.GetDirectoryName(typeof(PSObject).Assembly.Location);
            if (string.IsNullOrEmpty(baseDirectory))
                // Need to remove any trailing directory separator characters
                baseDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            return baseDirectory;
        private static string[] s_productFolderDirectories;
        private static string[] GetProductFolderDirectories()
            if (s_productFolderDirectories == null)
                List<string> baseDirectories = new List<string>();
                // Retrieve the application base from the registry
                string appBase = Utils.DefaultPowerShellAppBase;
                if (!string.IsNullOrEmpty(appBase))
                    baseDirectories.Add(appBase);
                // Now add the two variations of System32
                baseDirectories.Add(Environment.GetFolderPath(Environment.SpecialFolder.System));
                string systemX86 = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
                if (!string.IsNullOrEmpty(systemX86))
                    baseDirectories.Add(systemX86);
                Interlocked.CompareExchange(ref s_productFolderDirectories, baseDirectories.ToArray(), null);
            return s_productFolderDirectories;
        /// Checks if the filePath represents a file under product folder
        /// ie., PowerShell ApplicationBase or $env:windir\system32 or
        /// $env:windir\syswow64.
        /// true: if the filePath is under product folder
        /// false: otherwise
        internal static bool IsUnderProductFolder(string filePath)
            FileInfo fileInfo = new FileInfo(filePath);
            string filename = fileInfo.FullName;
            var productFolderDirectories = GetProductFolderDirectories();
            for (int i = 0; i < productFolderDirectories.Length; i++)
                string applicationBase = productFolderDirectories[i];
                if (filename.StartsWith(applicationBase, StringComparison.OrdinalIgnoreCase))
        /// Checks if the current process is using WOW.
        internal static bool IsRunningFromSysWOW64()
            return DefaultPowerShellAppBase.Contains("SysWOW64");
        /// Checks if host machine is WinPE.
        internal static bool IsWinPEHost()
            RegistryKey winPEKey = null;
                // The existence of the following registry confirms that the host machine is a WinPE
                // HKLM\System\CurrentControlSet\Control\MiniNT
                winPEKey = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Control\MiniNT");
                return winPEKey != null;
            catch (SecurityException) { }
                winPEKey?.Dispose();
        #region Versioning related methods
        /// Returns current major version of monad ( that is running ) in a string
        /// Cannot return a Version object as minor number is a requirement for
        /// version object.
        internal static string GetCurrentMajorVersion()
            return PSVersionInfo.PSVersion.Major.ToString(CultureInfo.InvariantCulture);
        /// Coverts a string to version format.
        /// If the string is of the format x (ie., no dots), then ".0" is appended
        /// to the string.
        /// Version.TryParse will be used to convert the string to a Version
        /// <param name="versionString">String representing version.</param>
        /// <returns>A Version Object.</returns>
        internal static Version StringToVersion(string versionString)
            // max of 1 dot is allowed in version
            if (string.IsNullOrEmpty(versionString))
            int dotCount = 0;
            foreach (char c in versionString)
                    dotCount++;
                    if (dotCount > 1)
            // Version.TryParse expects the string to be in format: major.minor[.build[.revision]]
            if (dotCount == 0)
                versionString += ".0";
            Version result = null;
            if (Version.TryParse(versionString, out result))
        /// Checks whether current PowerShell session supports edition specified
        /// by checkEdition.
        /// <param name="checkEdition">Edition to check.</param>
        /// <returns>True if supported, false otherwise.</returns>
        internal static bool IsPSEditionSupported(string checkEdition)
            return PSVersionInfo.PSEditionValue.Equals(checkEdition, StringComparison.OrdinalIgnoreCase);
        /// Check whether the current PowerShell session supports any of the specified editions.
        /// <param name="editions">The PowerShell editions to check compatibility with.</param>
        /// <returns>True if the edition is supported by this runtime, false otherwise.</returns>
        internal static bool IsPSEditionSupported(IEnumerable<string> editions)
            string currentPSEdition = PSVersionInfo.PSEditionValue;
            foreach (string edition in editions)
                if (currentPSEdition.Equals(edition, StringComparison.OrdinalIgnoreCase))
        /// Checks whether the specified edition value is allowed.
        /// <param name="editionValue">Edition value to check.</param>
        /// <returns>True if allowed, false otherwise.</returns>
        internal static bool IsValidPSEditionValue(string editionValue)
            return AllowedEditionValues.Contains(editionValue, StringComparer.OrdinalIgnoreCase);
        /// String representing the Default shellID.
        internal const string DefaultPowerShellShellID = "Microsoft.PowerShell";
        /// This is used to construct the profile path.
        internal const string ProductNameForDirectory = "PowerShell";
        /// WSL introduces a new filesystem path to access the Linux filesystem from Windows, like '\\wsl$\ubuntu'.
        internal const string WslRootPath = @"\\wsl$";
        /// The subdirectory of module paths
        /// e.g. ~\Documents\WindowsPowerShell\Modules and %ProgramFiles%\WindowsPowerShell\Modules.
        internal static readonly string ModuleDirectory = Path.Combine(ProductNameForDirectory, "Modules");
        internal static readonly ConfigScope[] SystemWideOnlyConfig = new[] { ConfigScope.AllUsers };
        internal static readonly ConfigScope[] CurrentUserOnlyConfig = new[] { ConfigScope.CurrentUser };
        internal static readonly ConfigScope[] SystemWideThenCurrentUserConfig = new[] { ConfigScope.AllUsers, ConfigScope.CurrentUser };
        internal static readonly ConfigScope[] CurrentUserThenSystemWideConfig = new[] { ConfigScope.CurrentUser, ConfigScope.AllUsers };
        internal static T GetPolicySetting<T>(ConfigScope[] preferenceOrder) where T : PolicyBase, new()
            T policy = null;
            // On Windows, group policy settings from registry take precedence.
            // If the requested policy is not defined in registry, we query the configuration file.
            policy = GetPolicySettingFromGPO<T>(preferenceOrder);
            if (policy != null) { return policy; }
            policy = GetPolicySettingFromConfigFile<T>(preferenceOrder);
            return policy;
        private static readonly ConcurrentDictionary<ConfigScope, PowerShellPolicies> s_cachedPoliciesFromConfigFile =
            new ConcurrentDictionary<ConfigScope, PowerShellPolicies>();
        /// Get a specific kind of policy setting from the configuration file.
        private static T GetPolicySettingFromConfigFile<T>(ConfigScope[] preferenceOrder) where T : PolicyBase, new()
            foreach (ConfigScope scope in preferenceOrder)
                PowerShellPolicies policies;
                if (InternalTestHooks.BypassGroupPolicyCaching)
                    policies = PowerShellConfig.Instance.GetPowerShellPolicies(scope);
                else if (!s_cachedPoliciesFromConfigFile.TryGetValue(scope, out policies))
                    // Use lock here to reduce the contention on accessing the configuration file
                    lock (s_cachedPoliciesFromConfigFile)
                        policies = s_cachedPoliciesFromConfigFile.GetOrAdd(scope, PowerShellConfig.Instance.GetPowerShellPolicies);
                if (policies != null)
                    PolicyBase result = null;
                    switch (typeof(T).Name)
                        case nameof(ScriptExecution):
                            result = policies.ScriptExecution;
                        case nameof(ScriptBlockLogging):
                            result = policies.ScriptBlockLogging;
                        case nameof(ModuleLogging):
                            result = policies.ModuleLogging;
                        case nameof(ProtectedEventLogging):
                            result = policies.ProtectedEventLogging;
                        case nameof(Transcription):
                            result = policies.Transcription;
                        case nameof(UpdatableHelp):
                            result = policies.UpdatableHelp;
                        case nameof(ConsoleSessionConfiguration):
                            result = policies.ConsoleSessionConfiguration;
                            Diagnostics.Assert(false, "Should be unreachable code. Update this switch block when new PowerShell policy types are added.");
                    if (result != null) { return (T)result; }
        private static readonly Dictionary<string, string> GroupPolicyKeys = new Dictionary<string, string>
            {nameof(ScriptExecution), @"Software\Policies\Microsoft\PowerShellCore"},
            {nameof(ScriptBlockLogging), @"Software\Policies\Microsoft\PowerShellCore\ScriptBlockLogging"},
            {nameof(ModuleLogging), @"Software\Policies\Microsoft\PowerShellCore\ModuleLogging"},
            {nameof(ProtectedEventLogging), @"Software\Policies\Microsoft\Windows\EventLog\ProtectedEventLogging"},
            {nameof(Transcription), @"Software\Policies\Microsoft\PowerShellCore\Transcription"},
            {nameof(UpdatableHelp), @"Software\Policies\Microsoft\PowerShellCore\UpdatableHelp"},
            {nameof(ConsoleSessionConfiguration), @"Software\Policies\Microsoft\PowerShellCore\ConsoleSessionConfiguration"}
        private static readonly Dictionary<string, string> WindowsPowershellGroupPolicyKeys = new Dictionary<string, string>
            { nameof(ScriptExecution), @"Software\Policies\Microsoft\Windows\PowerShell" },
            { nameof(ScriptBlockLogging), @"Software\Policies\Microsoft\Windows\PowerShell\ScriptBlockLogging" },
            { nameof(ModuleLogging), @"Software\Policies\Microsoft\Windows\PowerShell\ModuleLogging" },
            { nameof(Transcription), @"Software\Policies\Microsoft\Windows\PowerShell\Transcription" },
            { nameof(UpdatableHelp), @"Software\Policies\Microsoft\Windows\PowerShell\UpdatableHelp" },
        private const string PolicySettingFallbackKey = "UseWindowsPowerShellPolicySetting";
        private static readonly ConcurrentDictionary<ConfigScope, ConcurrentDictionary<string, PolicyBase>> s_cachedPoliciesFromRegistry =
            new ConcurrentDictionary<ConfigScope, ConcurrentDictionary<string, PolicyBase>>();
        private static readonly Func<ConfigScope, ConcurrentDictionary<string, PolicyBase>> s_subCacheCreationDelegate =
            key => new ConcurrentDictionary<string, PolicyBase>(StringComparer.Ordinal);
        /// Read policy settings from a registry key into a policy object.
        /// <param name="instance">Policy object that will be filled with values from registry.</param>
        /// <param name="instanceType">Type of policy object used.</param>
        /// <param name="gpoKey">Registry key that has policy settings.</param>
        /// <returns>True if any property was successfully set on the policy object.</returns>
        private static bool TrySetPolicySettingsFromRegistryKey(object instance, Type instanceType, RegistryKey gpoKey)
            var properties = instanceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            bool isAnyPropertySet = false;
            string[] valueNames = gpoKey.GetValueNames();
            string[] subKeyNames = gpoKey.GetSubKeyNames();
            var valueNameSet = valueNames.Length > 0 ? new HashSet<string>(valueNames, StringComparer.OrdinalIgnoreCase) : null;
            var subKeyNameSet = subKeyNames.Length > 0 ? new HashSet<string>(subKeyNames, StringComparer.OrdinalIgnoreCase) : null;
            // If there are any values or subkeys in the registry key - read them into the policy instance object
            if ((valueNameSet != null) || (subKeyNameSet != null))
                foreach (var property in properties)
                    string settingName = property.Name;
                    object rawRegistryValue = null;
                    // Get the raw value from registry.
                    if (valueNameSet != null && valueNameSet.Contains(settingName))
                        rawRegistryValue = gpoKey.GetValue(settingName);
                    else if (subKeyNameSet != null && subKeyNameSet.Contains(settingName))
                        using (RegistryKey subKey = gpoKey.OpenSubKey(settingName))
                            if (subKey != null)
                                rawRegistryValue = subKey.GetValueNames();
                    // Get the actual property value based on the property type.
                    // If the final property value is not null, then set the property.
                    if (rawRegistryValue != null)
                        Type propertyType = property.PropertyType;
                        object propertyValue = null;
                        switch (propertyType)
                            case var _ when propertyType == typeof(bool?):
                                if (rawRegistryValue is int rawIntValue)
                                    if (rawIntValue == 1)
                                        propertyValue = true;
                                    else if (rawIntValue == 0)
                                        propertyValue = false;
                            case var _ when propertyType == typeof(string):
                                if (rawRegistryValue is string rawStringValue)
                                    propertyValue = rawStringValue;
                            case var _ when propertyType == typeof(string[]):
                                if (rawRegistryValue is string[] rawStringArrayValue)
                                    propertyValue = rawStringArrayValue;
                                else if (rawRegistryValue is string stringValue)
                                    propertyValue = new string[] { stringValue };
                                throw System.Management.Automation.Interpreter.Assert.Unreachable;
                        // Set the property if the value is not null
                            property.SetValue(instance, propertyValue);
                            isAnyPropertySet = true;
            return isAnyPropertySet;
        /// The implementation of fetching a specific kind of policy setting from the given configuration scope.
        private static T GetPolicySettingFromGPOImpl<T>(ConfigScope scope) where T : PolicyBase, new()
            Type tType = typeof(T);
            // SystemWide scope means 'LocalMachine' root key when query from registry
            RegistryKey rootKey = (scope == ConfigScope.AllUsers) ? Registry.LocalMachine : Registry.CurrentUser;
            GroupPolicyKeys.TryGetValue(tType.Name, out string gpoKeyPath);
            Diagnostics.Assert(gpoKeyPath != null, StringUtil.Format("The GPO registry key path should be pre-defined for {0}", tType.Name));
            using (RegistryKey gpoKey = rootKey.OpenSubKey(gpoKeyPath))
                // If the corresponding GPO key doesn't exist, return null
                if (gpoKey == null) { return null; }
                // The corresponding GPO key exists, then create an instance of T
                // and populate its properties with the settings
                object tInstance = Activator.CreateInstance(tType, nonPublic: true);
                // if PolicySettingFallbackKey is Not set - use PowerShell Core policy reg key
                if ((int)gpoKey.GetValue(PolicySettingFallbackKey, 0) == 0)
                    isAnyPropertySet = TrySetPolicySettingsFromRegistryKey(tInstance, tType, gpoKey);
                    // when PolicySettingFallbackKey flag is set (REG_DWORD "1") use Windows PS policy reg key
                    WindowsPowershellGroupPolicyKeys.TryGetValue(tType.Name, out string winPowershellGpoKeyPath);
                    Diagnostics.Assert(winPowershellGpoKeyPath != null, StringUtil.Format("The Windows PS GPO registry key path should be pre-defined for {0}", tType.Name));
                    using (RegistryKey winPowershellGpoKey = rootKey.OpenSubKey(winPowershellGpoKeyPath))
                        // If the corresponding Windows PS GPO key doesn't exist, return null
                        if (winPowershellGpoKey == null) { return null; }
                        isAnyPropertySet = TrySetPolicySettingsFromRegistryKey(tInstance, tType, winPowershellGpoKey);
                // If no property is set, then we consider this policy as undefined
                return isAnyPropertySet ? (T)tInstance : null;
        /// Get a specific kind of policy setting from the group policy registry key.
        private static T GetPolicySettingFromGPO<T>(ConfigScope[] preferenceOrder) where T : PolicyBase, new()
            PolicyBase policy = null;
            string policyName = typeof(T).Name;
                    policy = GetPolicySettingFromGPOImpl<T>(scope);
                    var subordinateCache = s_cachedPoliciesFromRegistry.GetOrAdd(scope, s_subCacheCreationDelegate);
                    if (!subordinateCache.TryGetValue(policyName, out policy))
                        policy = subordinateCache.GetOrAdd(policyName, key => GetPolicySettingFromGPOImpl<T>(scope));
                if (policy != null) { return (T)policy; }
        /// Scheduled job module name.
        internal const string ScheduledJobModuleName = "PSScheduledJob";
        internal static void EnsureModuleLoaded(string module, ExecutionContext context)
            if (context != null && !context.AutoLoadingModuleInProgress.Contains(module))
                List<PSModuleInfo> loadedModules = context.Modules.GetModules(new string[] { module }, false);
                if ((loadedModules == null) || (loadedModules.Count == 0))
                    CommandInfo commandInfo = new CmdletInfo("Import-Module", typeof(Microsoft.PowerShell.Commands.ImportModuleCommand),
                                                             null, null, context);
                    var importModuleCommand = new System.Management.Automation.Runspaces.Command(commandInfo);
                    context.AutoLoadingModuleInProgress.Add(module);
                            .AddParameter("Name", module)
                            .AddParameter("PassThru");
                        ps.Invoke<PSModuleInfo>();
                        context.AutoLoadingModuleInProgress.Remove(module);
                        ps?.Dispose();
        /// Returns modules (either loaded or in available) that match pattern <paramref name="module"/>.
        /// Uses Get-Module -ListAvailable cmdlet.
        /// List of PSModuleInfo's or Null.
        internal static List<PSModuleInfo> GetModules(string module, ExecutionContext context)
            // first look in the loaded modules and then append the modules from gmo -Listavailable
            // Reason: gmo -li looks only the PSModulepath. There may be cases where a module
            // is imported directly from a path (that is not in PSModulePath).
            List<PSModuleInfo> result = context.Modules.GetModules(new string[] { module }, false);
            CommandInfo commandInfo = new CmdletInfo("Get-Module", typeof(Microsoft.PowerShell.Commands.GetModuleCommand),
            var getModuleCommand = new System.Management.Automation.Runspaces.Command(commandInfo);
                        .AddParameter("ListAvailable");
                Collection<PSModuleInfo> gmoOutPut = ps.Invoke<PSModuleInfo>();
                if (gmoOutPut != null)
                        result = gmoOutPut.ToList<PSModuleInfo>();
                        result.AddRange(gmoOutPut);
        /// Returns modules (either loaded or in available) that match FullyQualifiedName <paramref name="fullyQualifiedName"/>.
        /// <param name="fullyQualifiedName"></param>
        internal static List<PSModuleInfo> GetModules(ModuleSpecification fullyQualifiedName, ExecutionContext context)
            List<PSModuleInfo> result = context.Modules.GetModules(new[] { fullyQualifiedName }, false);
            CommandInfo commandInfo = new CmdletInfo("Get-Module", typeof(GetModuleCommand),
            var getModuleCommand = new Runspaces.Command(commandInfo);
                        .AddParameter("FullyQualifiedName", fullyQualifiedName)
                Collection<PSModuleInfo> gmoOutput = ps.Invoke<PSModuleInfo>();
                if (gmoOutput != null)
                        result = gmoOutput.ToList();
                        // append to result
                        result.AddRange(gmoOutput);
        private static bool TryGetWindowsCurrentIdentity(out WindowsIdentity currentIdentity)
                currentIdentity = WindowsIdentity.GetCurrent();
                currentIdentity = null;
            return (currentIdentity != null);
        /// Gets the current impersonating Windows identity, if any.
        /// <param name="impersonatedIdentity">Current impersonated Windows identity or null.</param>
        /// <returns>True if current identity is impersonated.</returns>
        internal static bool TryGetWindowsImpersonatedIdentity(out WindowsIdentity impersonatedIdentity)
            WindowsIdentity currentIdentity;
            if (TryGetWindowsCurrentIdentity(out currentIdentity) && (currentIdentity.ImpersonationLevel == TokenImpersonationLevel.Impersonation))
                impersonatedIdentity = currentIdentity;
            impersonatedIdentity = null;
        internal static bool IsAdministrator()
            // Porting note: only Windows supports the SecurityPrincipal API of .NET. Due to
            // advanced privilege models, the correct approach on Unix is to assume the user has
            // permissions, attempt the task, and error gracefully if the task fails due to
            // permissions. To fit into PowerShell's existing model of preemptively checking
            // permissions (which cannot be assumed on Unix), we "assume" the user is an
            // administrator by returning true, thus nullifying this check on Unix.
            if (TryGetWindowsCurrentIdentity(out currentIdentity))
                var principal = new WindowsPrincipal(currentIdentity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
        internal static bool IsReservedDeviceName(string destinationPath)
            string[] reservedDeviceNames = { "CON", "PRN", "AUX", "CLOCK$", "NUL", "CONIN$", "CONOUT$",
                                             "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                                             "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            string compareName = Path.GetFileName(destinationPath);
            string noExtensionCompareName = Path.GetFileNameWithoutExtension(destinationPath);
            if (((compareName.Length < 3) || (compareName.Length > 7)) &&
                ((noExtensionCompareName.Length < 3) || (noExtensionCompareName.Length > 7)))
            foreach (string deviceName in reservedDeviceNames)
                    string.Equals(deviceName, compareName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(deviceName, noExtensionCompareName, StringComparison.OrdinalIgnoreCase))
        internal static bool PathIsUnc(string path, bool networkOnly = false)
            if (string.IsNullOrEmpty(path) || !path.StartsWith('\\'))
            // handle special cases like '\\wsl$\ubuntu', '\\?\', and '\\.\pipe\' which aren't a UNC path, but we can say it is so the filesystemprovider can use it
            if (!networkOnly && (path.StartsWith(WslRootPath, StringComparison.OrdinalIgnoreCase) || PathIsDevicePath(path)))
            Uri uri;
            return Uri.TryCreate(path, UriKind.Absolute, out uri) && uri.IsUnc;
        internal static bool PathIsDevicePath(string path)
            // device paths can be network paths, we would need windows to parse it.
            return path.StartsWith(@"\\.\") || path.StartsWith(@"\\?\") || path.StartsWith(@"\\;");
        internal static readonly string PowerShellAssemblyStrongNameFormat =
            "{0}, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        internal static readonly HashSet<string> PowerShellAssemblies =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    "microsoft.powershell.commands.diagnostics",
                    "microsoft.powershell.commands.management",
                    "microsoft.powershell.commands.utility",
                    "microsoft.powershell.consolehost",
                    "microsoft.powershell.scheduledjob",
                    "microsoft.powershell.security",
                    "microsoft.wsman.management",
                    "microsoft.wsman.runtime",
                    "system.management.automation"
        internal static bool IsPowerShellAssembly(string assemblyName)
            if (!string.IsNullOrWhiteSpace(assemblyName))
                var fixedName = assemblyName.EndsWith(StringLiterals.PowerShellILAssemblyExtension, StringComparison.OrdinalIgnoreCase)
                                ? Path.GetFileNameWithoutExtension(assemblyName)
                                : assemblyName;
                if ((fixedName != null) && PowerShellAssemblies.Contains(fixedName))
        internal static string GetPowerShellAssemblyStrongName(string assemblyName)
                string fixedName = assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                    return string.Format(CultureInfo.InvariantCulture, PowerShellAssemblyStrongNameFormat, fixedName);
            return assemblyName;
        /// If a mutex is abandoned, in our case, it is ok to proceed.
        /// <param name="mutex">The mutex to wait on. If it is null, a new one will be created.</param>
        /// <param name="initializer">The initializer to use to recreate the mutex.</param>
        /// <returns>A working mutex. If the mutex was abandoned, a new one is created to replace it.</returns>
        internal static Mutex SafeWaitMutex(Mutex mutex, MutexInitializer initializer)
                mutex.WaitOne();
            catch (AbandonedMutexException)
                // If the Mutex has been abandoned, then the process protecting the critical section
                // is no longer valid. We need to release to continue normal operations.
                mutex.ReleaseMutex();
                ((IDisposable)mutex).Dispose();
                // Try again, throw if it still fails
                mutex = initializer();
            return mutex;
        internal delegate Mutex MutexInitializer();
        internal static bool Succeeded(int hresult)
            return hresult >= 0;
        // BigEndianUTF32 encoding is possible, but requires creation
        internal static readonly Encoding BigEndianUTF32Encoding = new UTF32Encoding(bigEndian: true, byteOrderMark: true);
        // [System.Text.Encoding]::GetEncodings() | Where-Object { $_.GetEncoding().GetPreamble() } |
        //     Add-Member ScriptProperty Preamble { $this.GetEncoding().GetPreamble() -join "-" } -PassThru |
        //     Format-Table -Auto
        /// Queues a CLR worker thread with impersonation of provided Windows identity.
        /// <param name="identityToImpersonate">Windows identity to impersonate or null.</param>
        /// <param name="threadProc">Thread procedure for thread.</param>
        /// <param name="state">Optional state for thread procedure.</param>
        internal static void QueueWorkItemWithImpersonation(
            WindowsIdentity identityToImpersonate,
            WaitCallback threadProc,
            object state)
            object[] args = new object[3];
            args[0] = identityToImpersonate;
            args[1] = threadProc;
            args[2] = state;
            Threading.ThreadPool.QueueUserWorkItem(WorkItemCallback, args);
        private static void WorkItemCallback(object callBackArgs)
            object[] args = callBackArgs as object[];
            WindowsIdentity identityToImpersonate = args[0] as WindowsIdentity;
            WaitCallback callback = args[1] as WaitCallback;
            object state = args[2];
            if (identityToImpersonate != null)
                WindowsIdentity.RunImpersonated(
                    identityToImpersonate.AccessToken,
                    () => callback(state));
            callback(state);
        /// If the command name is fully qualified then it is split into its component parts
        /// E.g., moduleName\commandName.
        /// <returns>Command name and as appropriate Module name in out parameter.</returns>
        internal static string ParseCommandName(string commandName, out string moduleName)
            var names = commandName.Split('\\', 2);
            if (names.Length == 2)
                moduleName = names[0];
                return names[1];
            moduleName = null;
            return commandName;
        internal static ReadOnlyCollection<T> EmptyReadOnlyCollection<T>()
            return EmptyReadOnlyCollectionHolder<T>._instance;
        private static class EmptyReadOnlyCollectionHolder<T>
            internal static readonly ReadOnlyCollection<T> _instance =
                new ReadOnlyCollection<T>(Array.Empty<T>());
        internal static class Separators
            internal static readonly char[] Backslash = new char[] { '\\' };
            internal static readonly char[] Directory = new char[] { '\\', '/' };
            internal static readonly char[] DirectoryOrDrive = new char[] { '\\', '/', ':' };
            internal static readonly char[] SpaceOrTab = new char[] { ' ', '\t' };
            internal static readonly char[] StarOrQuestion = new char[] { '*', '?' };
            // (Copied from System.IO.Path so we can call TrimEnd in the same way that Directory.EnumerateFiles would on the search patterns).
            // Trim trailing white spaces, tabs etc but don't be aggressive in removing everything that has UnicodeCategory of trailing space.
            // String.WhitespaceChars will trim aggressively than what the underlying FS does (for ex, NTFS, FAT).
            internal static readonly char[] PathSearchTrimEnd = { (char)0x9, (char)0xA, (char)0xB, (char)0xC, (char)0xD, (char)0x20, (char)0x85, (char)0xA0 };
        /// A COM object could be directly of the type 'System.__ComObject', or it could be a strongly typed RWC,
        /// whose specific type derives from 'System.__ComObject'.
        /// A strongly typed RWC can be created via the 'new' operation with a Primary Interop Assembly (PIA).
        /// For example, with the PIA 'Microsoft.Office.Interop.Excel', you can write the following code:
        ///    var excelApp = new Microsoft.Office.Interop.Excel.Application();
        ///    Type type = excelApp.GetType();
        ///    Type comObjectType = typeof(object).Assembly.GetType("System.__ComObject");
        ///    Console.WriteLine("excelApp type: {0}", type.FullName);
        ///    Console.WriteLine("Is __ComObject assignable from? {0}", comObjectType.IsAssignableFrom(type));
        /// and the results are:
        ///    excelApp type: Microsoft.Office.Interop.Excel.ApplicationClass
        ///    Is __ComObject assignable from? True.
        internal static bool IsComObject(object obj)
            return obj != null && Marshal.IsComObject(obj);
        /// EnforceSystemLockDownLanguageMode
        ///     FullLangauge        ->  ConstrainedLanguage
        ///     RestrictedLanguage  ->  NoLanguage
        ///     ConstrainedLanguage ->  ConstrainedLanguage
        ///     NoLanguage          ->  NoLanguage.
        /// <param name="context">ExecutionContext.</param>
        /// <returns>The current ExecutionContext language mode.</returns>
        internal static PSLanguageMode EnforceSystemLockDownLanguageMode(ExecutionContext context)
            switch (SystemPolicy.GetSystemLockdownPolicy())
                    switch (context.LanguageMode)
                        case PSLanguageMode.FullLanguage:
                            context.LanguageMode = PSLanguageMode.NoLanguage;
                            Diagnostics.Assert(false, "Unexpected PSLanguageMode");
                            // Set to ConstrainedLanguage mode.  But no restrictions are applied in audit mode
                            // and only audit messages will be emitted to logs.
            return context.LanguageMode;
        internal static string DisplayHumanReadableFileSize(long bytes)
            return bytes switch
                < 1024 and >= 0 => $"{bytes} Bytes",
                < 1048576 and >= 1024 => $"{(bytes / 1024.0).ToString("0.0")} KB",
                < 1073741824 and >= 1048576 => $"{(bytes / 1048576.0).ToString("0.0")} MB",
                < 1099511627776 and >= 1073741824 => $"{(bytes / 1073741824.0).ToString("0.000")} GB",
                < 1125899906842624 and >= 1099511627776 => $"{(bytes / 1099511627776.0).ToString("0.00000")} TB",
                < 1152921504606847000 and >= 1125899906842624 => $"{(bytes / 1125899906842624.0).ToString("0.0000000")} PB",
                >= 1152921504606847000 => $"{(bytes / 1152921504606847000.0).ToString("0.000000000")} EB",
                _ => $"0 Bytes",
        /// Returns true if the current session is restricted (JEA or similar sessions)
        /// <returns>True if the session is restricted.</returns>
        internal static bool IsSessionRestricted(ExecutionContext context)
            // if import-module is visible, then the session is not restricted,
            // because the user can load arbitrary code.
            if (cmdletInfo != null && cmdletInfo.Visibility == SessionStateEntryVisibility.Public)
        /// Determine whether the environment variable is set and how.
        /// <param name="name">The name of the environment variable.</param>
        /// <param name="defaultValue">If the environment variable is not set, use this as the default value.</param>
        /// <returns>A boolean representing the value of the environment variable.</returns>
        internal static bool GetEnvironmentVariableAsBool(string name, bool defaultValue)
            var str = Environment.GetEnvironmentVariable(name);
            var boolStr = str.AsSpan();
            if (boolStr.Length == 1)
                if (boolStr[0] == '1')
                if (boolStr[0] == '0')
            if (boolStr.Length == 3 &&
                (boolStr[0] == 'y' || boolStr[0] == 'Y') &&
                (boolStr[1] == 'e' || boolStr[1] == 'E') &&
                (boolStr[2] == 's' || boolStr[2] == 'S'))
            if (boolStr.Length == 2 &&
                (boolStr[0] == 'n' || boolStr[0] == 'N') &&
                (boolStr[1] == 'o' || boolStr[1] == 'O'))
            if (boolStr.Length == 4 &&
                (boolStr[0] == 't' || boolStr[0] == 'T') &&
                (boolStr[1] == 'r' || boolStr[1] == 'R') &&
                (boolStr[2] == 'u' || boolStr[2] == 'U') &&
                (boolStr[3] == 'e' || boolStr[3] == 'E'))
            if (boolStr.Length == 5 &&
                (boolStr[0] == 'f' || boolStr[0] == 'F') &&
                (boolStr[1] == 'a' || boolStr[1] == 'A') &&
                (boolStr[2] == 'l' || boolStr[2] == 'L') &&
                (boolStr[3] == 's' || boolStr[3] == 'S') &&
                (boolStr[4] == 'e' || boolStr[4] == 'E'))
    /// <summary>This class is used for internal test purposes.</summary>
    [SuppressMessage("Microsoft.MSInternal", "CA903:InternalNamespaceShouldNotContainPublicTypes", Justification = "Needed Internal use only")]
    public static class InternalTestHooks
        internal static bool BypassGroupPolicyCaching;
        internal static bool ForceScriptBlockLogging;
        internal static bool UseDebugAmsiImplementation;
        internal static bool BypassAppLockerPolicyCaching;
        internal static bool BypassOnlineHelpRetrieval;
        internal static bool ForcePromptForChoiceDefaultOption;
        internal static bool NoPromptForPassword;
        internal static bool ForceFormatListFixedLabelWidth;
        // Update-Help tests
        internal static bool ThrowHelpCultureNotSupported;
        internal static CultureInfo CurrentUICulture;
        // Stop/Restart/Rename Computer tests
        internal static bool TestStopComputer;
        internal static bool TestWaitStopComputer;
        internal static bool TestRenameComputer;
        internal static int TestStopComputerResults;
        internal static int TestRenameComputerResults;
        // It's useful to test that we don't depend on the ScriptBlock and AST objects and can use a re-parsed version.
        internal static bool IgnoreScriptBlockCache;
        // Simulate 'System.Diagnostics.Stopwatch.IsHighResolution is false' to test Get-Uptime throw
        internal static bool StopwatchIsNotHighResolution;
        internal static bool DisableGACLoading;
        internal static bool SetConsoleWidthToZero;
        internal static bool SetConsoleHeightToZero;
        // Simulate 'MyDocuments' returning empty string
        internal static bool SetMyDocumentsSpecialFolderToBlank;
        internal static bool SetDate;
        // A location to test PSEdition compatibility functionality for Windows PowerShell modules with
        // since we can't manipulate the System32 directory in a test
        internal static string TestWindowsPowerShellPSHomeLocation;
        // A version of Windows PS that is installed on the system; normally this is retrieved from a reg key that is write-protected.
        internal static string TestWindowsPowerShellVersionString;
        internal static bool ShowMarkdownOutputBypass;
        internal static bool ThrowExdevErrorOnMoveDirectory;
        // To emulate OneDrive behavior we use the hard-coded symlink.
        // If OneDriveTestRecurseOn is false then the symlink works as regular symlink.
        // If OneDriveTestRecurseOn is true then we recurse into the symlink as OneDrive should work.
        // OneDriveTestSymlinkName defines the symlink name used in tests.
        internal static bool OneDriveTestOn;
        internal static bool OneDriveTestRecurseOn;
        internal static string OneDriveTestSymlinkName = "link-Beta";
        // Test out smaller connection buffer size when calling WNetGetConnection.
        internal static int WNetGetConnectionBufferSize = -1;
        /// <summary>This member is used for internal test purposes.</summary>
        public static void SetTestHook(string property, object value)
            var fieldInfo = typeof(InternalTestHooks).GetField(property, BindingFlags.Static | BindingFlags.NonPublic);
            fieldInfo?.SetValue(null, value);
        /// Constructs a custom PSSenderInfo instance that can be assigned to $PSSenderInfo
        /// in order to simulate a remoting session with respect to the $PSSenderInfo.ConnectionString (connection URL)
        /// and $PSSenderInfo.ApplicationArguments.PSVersionTable.PSVersion (the remoting client's PowerShell version).
        /// See Get-FormatDataTest.ps1.
        /// <param name="url">The connection URL to reflect in the returned instance's ConnectionString property.</param>
        /// <param name="clientVersion">The version number to report as the remoting client's PowerShell version.</param>
        /// <returns>The newly constructed custom PSSenderInfo instance.</returns>
        public static PSSenderInfo GetCustomPSSenderInfo(string url, Version clientVersion)
            var dummyPrincipal = new PSPrincipal(new PSIdentity("none", true, "someuser", null), null);
            var pssi = new PSSenderInfo(dummyPrincipal, url);
            pssi.ApplicationArguments = new PSPrimitiveDictionary();
            pssi.ApplicationArguments.Add("PSVersionTable", new PSObject(new PSPrimitiveDictionary()));
            ((PSPrimitiveDictionary)PSObject.Base(pssi.ApplicationArguments["PSVersionTable"])).Add("PSVersion", new PSObject(clientVersion));
            return pssi;
    /// Provides undo/redo functionality by using 2 instances of <seealso cref="BoundedStack{T}"/>.
    internal class HistoryStack<T>
        private readonly BoundedStack<T> _boundedUndoStack;
        private readonly BoundedStack<T> _boundedRedoStack;
        internal HistoryStack(int capacity)
            _boundedUndoStack = new BoundedStack<T>(capacity);
            _boundedRedoStack = new BoundedStack<T>(capacity);
        internal void Push(T item)
            _boundedUndoStack.Push(item);
            if (RedoCount >= 0)
                _boundedRedoStack.Clear();
        /// Handles bounded history stacks by pushing the current item to the redoStack and returning the item from the popped undoStack.
        internal T Undo(T currentItem)
            T previousItem = _boundedUndoStack.Pop();
            _boundedRedoStack.Push(currentItem);
            return previousItem;
        /// Handles bounded history stacks by pushing the current item to the undoStack and returning the item from the popped redoStack.
        internal T Redo(T currentItem)
            var nextItem = _boundedRedoStack.Pop();
            _boundedUndoStack.Push(currentItem);
            return nextItem;
        internal int UndoCount => _boundedUndoStack.Count;
        internal int RedoCount => _boundedRedoStack.Count;
    /// A bounded stack based on a linked list.
    internal class BoundedStack<T> : LinkedList<T>
        private readonly int _capacity;
        /// Lazy initialisation, i.e. it sets only its limit but does not allocate the memory for the given capacity.
        /// <param name="capacity"></param>
        internal BoundedStack(int capacity)
            _capacity = capacity;
        /// Push item.
            this.AddFirst(item);
            if (this.Count > _capacity)
                this.RemoveLast();
        /// Pop item.
        internal T Pop()
            if (this.First == null)
                throw new InvalidOperationException(SessionStateStrings.BoundedStackIsEmpty);
            var item = this.First.Value;
                this.RemoveFirst();
    /// A readonly Hashset.
    internal sealed class ReadOnlyBag<T> : IEnumerable
        private readonly HashSet<T> _hashset;
        /// Constructor for the readonly Hashset.
        internal ReadOnlyBag(HashSet<T> hashset)
            ArgumentNullException.ThrowIfNull(hashset);
            _hashset = hashset;
        /// Get the count of the Hashset.
        public int Count => _hashset.Count;
        /// Indicate if it's a readonly Hashset.
        public bool IsReadOnly => true;
        /// Check if the set contains an item.
        public bool Contains(T item) => _hashset.Contains(item);
        /// GetEnumerator method.
        public IEnumerator GetEnumerator() => _hashset.GetEnumerator();
        /// Get an empty singleton.
        internal static readonly ReadOnlyBag<T> Empty = new ReadOnlyBag<T>(new HashSet<T>(capacity: 0));
    /// Helper class for simple argument validations.
    internal static class Requires
        internal static void NotNullOrEmpty(ICollection value, string paramName)
            if (value is null || value.Count == 0)
                throw new ArgumentNullException(paramName);
