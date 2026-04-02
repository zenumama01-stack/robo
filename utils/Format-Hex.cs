// Once Serialization is available on CoreCLR: using System.Runtime.Serialization.Formatters.Binary;
    /// Displays the hexadecimal equivalent of the input data.
    [Cmdlet(VerbsCommon.Format, "Hex", HelpUri = "https://go.microsoft.com/fwlink/?LinkId=2096611")]
    [OutputType(typeof(ByteCollection))]
    [Alias("fhx")]
    public sealed class FormatHex : PSCmdlet
        private const int BUFFERSIZE = 16;
        /// For cases where a homogeneous collection of bytes or other items are directly piped in, we collect all the
        /// bytes in a List&lt;byte&gt; and then output the formatted result all at once in EndProcessing().
        private readonly List<byte> _inputBuffer = new();
        /// Expect to group <see cref="InputObject"/>s by default. When receiving input that should not be grouped,
        /// e.g., arrays, strings, FileInfo objects, this flag will be disabled until the next groupable
        /// <see cref="InputObject"/> is received over the pipeline.
        private bool _groupInput = true;
        /// Keep track of prior input types to determine if we're given a heterogeneous collection.
        private Type _lastInputType;
        /// Gets or sets the path of file(s) to process.
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "Path")]
        /// Gets or sets the literal path of file to process.
        [Parameter(Mandatory = true, ParameterSetName = "LiteralPath")]
        /// Gets or sets the object to process.
        [Parameter(Mandatory = true, ParameterSetName = "ByInputObject", ValueFromPipeline = true)]
        /// Gets or sets the type of character encoding for InputObject.
        [Parameter(ParameterSetName = "ByInputObject")]
        /// Gets or sets count of bytes to read from the input stream.
        public long Count { get; set; } = long.MaxValue;
        /// Gets or sets offset of bytes to start reading the input stream from.
        public long Offset { get; set; }
        /// Gets or sets whether the file input should be swallowed as is. This parameter is no-op, deprecated.
        [Parameter(ParameterSetName = "ByInputObject", DontShow = true)]
        [Obsolete("Raw parameter is deprecated.", true)]
        /// Implements the ProcessRecord method for the FormatHex command.
            if (string.Equals(ParameterSetName, "ByInputObject", StringComparison.OrdinalIgnoreCase))
                ProcessInputObjects(InputObject);
                List<string> pathsToProcess = string.Equals(ParameterSetName, "LiteralPath", StringComparison.OrdinalIgnoreCase)
                    ? ResolvePaths(LiteralPath, true)
                    : ResolvePaths(Path, false);
                ProcessPath(pathsToProcess);
        /// Implements the EndProcessing method for the FormatHex command.
            FlushInputBuffer();
        #region Paths
        /// Validate each path provided and if valid, add to array of paths to process.
        /// If path is a literal path it is added to the array to process; we cannot validate them until we
        /// try to process file contents.
        /// <param name="path">The file path to resolve.</param>
        /// <param name="literalPath">The paths to process.</param>
        private List<string> ResolvePaths(string[] path, bool literalPath)
            foreach (string currentPath in path)
                List<string> newPaths = new();
                if (literalPath)
                    newPaths.Add(Context.SessionState.Path.GetUnresolvedProviderPathFromPSPath(currentPath, out provider, out _));
                        newPaths.AddRange(Context.SessionState.Path.GetResolvedProviderPathFromPSPath(currentPath, out provider));
                        if (!WildcardPattern.ContainsWildcardCharacters(currentPath))
                            ErrorRecord errorRecord = new(e, "FileNotFound", ErrorCategory.ObjectNotFound, path);
                    // Write a non-terminating error message indicating that path specified is not supported.
                    string errorMessage = StringUtil.Format(UtilityCommonStrings.FormatHexOnlySupportsFileSystemPaths, currentPath);
                        "FormatHexOnlySupportsFileSystemPaths",
                        currentPath);
            return pathsToProcess;
        /// Pass each valid path on to process its contents.
        /// <param name="pathsToProcess">The paths to process.</param>
        private void ProcessPath(List<string> pathsToProcess)
                ProcessFileContent(path);
        /// Creates a binary reader that reads the file content into a buffer (byte[]) 16 bytes at a time, and
        /// passes a copy of that array on to the WriteHexadecimal method to output.
        /// <param name="path">The file path to retrieve content from for processing.</param>
        private void ProcessFileContent(string path)
            Span<byte> buffer = stackalloc byte[BUFFERSIZE];
                using var reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
                long offset = Offset;
                int bytesRead = 0;
                long count = 0;
                reader.BaseStream.Position = Offset;
                while ((bytesRead = reader.Read(buffer)) > 0)
                    count += bytesRead;
                    if (count > Count)
                        bytesRead -= (int)(count - Count);
                        WriteHexadecimal(buffer.Slice(0, bytesRead), path, offset);
                    offset += bytesRead;
            catch (IOException fileException)
                // IOException takes care of FileNotFoundException, DirectoryNotFoundException, and PathTooLongException
                WriteError(new ErrorRecord(fileException, "FormatHexIOError", ErrorCategory.WriteError, path));
                WriteError(new ErrorRecord(argException, "FormatHexArgumentError", ErrorCategory.WriteError, path));
                    notSupportedException,
                    "FormatHexPathRefersToANonFileDevice",
                    path));
                    securityException,
                    "FormatHexUnauthorizedAccessError",
        #region InputObjects
        private void ProcessString(string originalString)
            Span<byte> bytes = Encoding.GetBytes(originalString);
            int offset = Math.Min(bytes.Length, Offset < int.MaxValue ? (int)Offset : int.MaxValue);
            int count = Math.Min(bytes.Length - offset, Count < int.MaxValue ? (int)Count : int.MaxValue);
            if (offset != 0 || count != bytes.Length)
                WriteHexadecimal(bytes.Slice(offset, count), offset: 0, label: GetGroupLabel(typeof(string)));
                WriteHexadecimal(bytes, offset: 0, label: GetGroupLabel(typeof(string)));
        private static readonly Random _idGenerator = new();
        private static string GetGroupLabel(Type inputType)
            => string.Format("{0} ({1}) <{2:X8}>", inputType.Name, inputType.FullName, _idGenerator.Next());
        private void FlushInputBuffer()
            if (_inputBuffer.Count == 0)
            int offset = Math.Min(_inputBuffer.Count, Offset < int.MaxValue ? (int)Offset : int.MaxValue);
            int count = Math.Min(_inputBuffer.Count - offset, Count < int.MaxValue ? (int)Count : int.MaxValue);
            if (offset != 0 || count != _inputBuffer.Count)
                WriteHexadecimal(
                    _inputBuffer.GetRange(offset, count).ToArray(),
                    offset: 0,
                    label: GetGroupLabel(_lastInputType));
                    _inputBuffer.ToArray(),
            // Reset flags so we can go back to filling up the buffer when needed.
            _lastInputType = null;
            _groupInput = true;
            _inputBuffer.Clear();
        /// Creates a byte array from the object passed to the cmdlet (based on type) and passes
        /// that array on to the WriteHexadecimal method to output.
        /// <param name="inputObject">The pipeline input object being processed.</param>
        private void ProcessInputObjects(PSObject inputObject)
            object obj = inputObject.BaseObject;
            if (obj is FileSystemInfo fsi)
                // Output already processed objects first, then process the file input.
                string[] path = { fsi.FullName };
                List<string> pathsToProcess = ResolvePaths(path, true);
            if (obj is string str)
                // Output already processed objects first, then process the string input.
                ProcessString(str);
            byte[] inputBytes = ConvertToBytes(obj);
            if (!_groupInput)
            if (inputBytes != null)
                _inputBuffer.AddRange(inputBytes);
                string errorMessage = StringUtil.Format(UtilityCommonStrings.FormatHexTypeNotSupported, obj.GetType());
                    "FormatHexTypeNotSupported",
                    obj.GetType());
        /// Converts the input object to a byte array based on the underlying type for basic value types and strings,
        /// as well as enum values or arrays.
        /// <param name="inputObject">The object to convert.</param>
        /// <returns>Returns a byte array of the input values, or null if there is no available conversion path.</returns>
        private byte[] ConvertToBytes(object inputObject)
            Type baseType = inputObject.GetType();
            byte[] result = null;
            int elements = 1;
            bool isArray = false;
            if (baseType.IsArray)
                _lastInputType = baseType;
                _groupInput = false;
                baseType = baseType.GetElementType();
                dynamic dynamicObject = inputObject;
                elements = (int)dynamicObject.Length;
                isArray = true;
            if (baseType.IsEnum)
                baseType = baseType.GetEnumUnderlyingType();
                isEnum = true;
            if (baseType.IsPrimitive && elements > 0)
                if (_groupInput)
                    if (_lastInputType != null && baseType != _lastInputType)
                var elementSize = Marshal.SizeOf(baseType);
                result = new byte[elementSize * elements];
                if (!isArray)
                    inputObject = new object[] { inputObject };
                foreach (dynamic obj in (Array)inputObject)
                    if (elementSize == 1)
                        result[index] = (byte)obj;
                        dynamic toBytes;
                            toBytes = Convert.ChangeType(obj, baseType);
                            toBytes = obj;
                        var bytes = BitConverter.GetBytes(toBytes);
                        for (int i = 0; i < bytes.Length; i++)
                            result[i + index] = bytes[i];
                    index += elementSize;
        #region Output
        /// Outputs the hexadecimal representation of the input data.
        /// <param name="inputBytes">Bytes for the hexadecimal representation.</param>
        /// <param name="path">File path.</param>
        /// <param name="offset">Offset in the file.</param>
        private void WriteHexadecimal(Span<byte> inputBytes, string path, long offset)
            const int bytesPerObject = 16;
            for (int index = 0; index < inputBytes.Length; index += bytesPerObject)
                var count = inputBytes.Length - index < bytesPerObject
                    ? inputBytes.Length - index
                    : bytesPerObject;
                var bytes = inputBytes.Slice(index, count);
                WriteObject(new ByteCollection((ulong)index + (ulong)offset, bytes.ToArray(), path));
        /// The label for the byte group. This may be a file path, a string value, or a
        /// formatted identifying string for the group.
        private void WriteHexadecimal(Span<byte> inputBytes, long offset, string label)
                WriteObject(new ByteCollection((ulong)index + (ulong)offset, label, bytes.ToArray()));
