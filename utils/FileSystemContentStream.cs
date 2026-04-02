    /// The content stream class for the file system provider. It implements both
    /// the IContentReader and IContentWriter interfaces.
    /// Note, this class does no specific error handling. All errors are allowed to
    /// propagate to the caller so that they can be written to the error pipeline
    internal class FileSystemContentReaderWriter : IContentReader, IContentWriter
        /// using "FileSystemContentStream" as the category.
            "FileSystemContentStream",
            "The provider content reader and writer for the file system")]
            Dbg.PSTraceSource.GetTracer("FileSystemContentStream",
            "The provider content reader and writer for the file system");
        private readonly string _streamName;
        private readonly FileMode _mode;
        private readonly FileAccess _access;
        private readonly FileShare _share;
        private readonly Encoding _encoding;
        private readonly CmdletProvider _provider;
        private FileStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly bool _usingByteEncoding;
        private const char DefaultDelimiter = '\n';
        private readonly string _delimiter = $"{DefaultDelimiter}";
        private readonly int[] _offsetDictionary;
        private readonly bool _usingDelimiter;
        private readonly StringBuilder _currentLineContent;
        private bool _waitForChanges;
        private readonly bool _isRawStream;
        private long _fileOffset;
        private FileAttributes _oldAttributes;
        private bool _haveOldAttributes;
        // The reader to read file content backward
        private FileStreamBackReader _backReader;
        private bool _alreadyDetectEncoding = false;
        // False to add a newline to the end of the output string, true if not.
        /// Constructor for the content stream.
        /// The path to the file to get the content from.
        /// The file mode to open the file with.
        /// <param name="access">
        /// The file access requested in the file.
        /// <param name="share">
        /// The file share to open the file with
        /// <param name="encoding">
        /// The encoding of the file to be read or written.
        /// <param name="usingByteEncoding">
        /// If true, bytes will be read from the file. If false, the specified encoding
        /// will be used to read the file.
        /// <param name="waitForChanges">
        /// If true, we will perform blocking reads on the file, waiting for new content to be appended
        /// The CmdletProvider invoking this stream
        /// <param name="isRawStream">
        /// Indicates raw stream.
        public FileSystemContentReaderWriter(
            FileMode mode,
            FileAccess access,
            FileShare share,
            bool usingByteEncoding,
            bool waitForChanges,
            CmdletProvider provider,
            bool isRawStream)
                streamName: null,
                mode,
                access,
                share,
                usingByteEncoding,
                waitForChanges,
                isRawStream)
        /// The name of the Alternate Data Stream to get the content from. If null or empty, returns
        /// the file's primary content.
            string path, string streamName, FileMode mode, FileAccess access, FileShare share,
            Encoding encoding, bool usingByteEncoding, bool waitForChanges, CmdletProvider provider,
            if (s_tracer.IsEnabled)
                s_tracer.WriteLine("path = {0}", path);
                s_tracer.WriteLine("mode = {0}", mode);
                s_tracer.WriteLine("access = {0}", access);
            _path = path;
            _streamName = streamName;
            _mode = mode;
            _access = access;
            _share = share;
            _encoding = encoding;
            _usingByteEncoding = usingByteEncoding;
            _waitForChanges = waitForChanges;
            _isRawStream = isRawStream;
            CreateStreams(path, streamName, mode, access, share, encoding);
        /// <param name="suppressNewline">
            bool isRawStream, bool suppressNewline)
                : this(path, streamName, mode, access, share, encoding, usingByteEncoding, waitForChanges, provider, isRawStream)
        ///  <param name="share">
        ///    The file share to open the file with
        /// <param name="delimiter">
        /// The delimiter to use when reading strings. Each time read is called, all contents up to an including
        /// the delimiter is read.
            string delimiter,
            : this(path, streamName, mode, access, share, encoding, false, waitForChanges, provider, isRawStream)
            // If the delimiter is default ('\n') we'll use ReadLine() method.
            // Otherwise allocate temporary structures for ReadDelimited() method.
            if (!(delimiter.Length == 1 && delimiter[0] == DefaultDelimiter))
                _usingDelimiter = true;
                // We expect that we are parsing files where line lengths can be relatively long.
                const int DefaultLineLength = 256;
                _currentLineContent = new StringBuilder(DefaultLineLength);
                // For Boyer-Moore string search algorithm.
                // Populate the offset lookups.
                // These will tell us the maximum number of characters
                // we can read to generate another possible match (safe shift).
                // If we read more characters than this, we risk consuming
                // more of the stream than we need.
                // Because an unicode character size is 2 byte we would to have use
                // very large array with 65535 size to keep this safe offsets.
                // One solution is to pack unicode character to byte.
                // The workaround is to use low byte from unicode character.
                // This allow us to use small array with size 256.
                // This workaround is the fastest and provides excellent results
                // in regular search scenarios when the file contains
                // mostly characters from the same alphabet.
                _offsetDictionary = new int[256];
                // If next char from file is not in search pattern safe shift is the search pattern length.
                for (var n = 0; n < _offsetDictionary.Length; n++)
                    _offsetDictionary[n] = _delimiter.Length;
                // If next char from file is in search pattern we should calculate a safe shift.
                char currentChar;
                byte lowByte;
                for (var i = 0; i < _delimiter.Length; i++)
                    currentChar = _delimiter[i];
                    lowByte = Unsafe.As<char, byte>(ref currentChar);
                    _offsetDictionary[lowByte] = _delimiter.Length - i - 1;
        /// Reads the specified number of characters or a lines from the file.
        /// If less than 1, then the entire file is read at once. If 1 or greater, then
        /// readCount is used to determine how many items (ie: lines, bytes, delimited tokens)
        /// to read per call.
        /// An array of strings representing the character(s) or line(s) read from
        /// the file.
        public IList Read(long readCount)
            if (_isRawStream && _waitForChanges)
                throw PSTraceSource.NewInvalidOperationException(FileSystemProviderStrings.RawAndWaitCannotCoexist);
            bool waitChanges = _waitForChanges;
            s_tracer.WriteLine("blocks requested = {0}", readCount);
            var blocks = new List<object>();
            bool readToEnd = (readCount <= 0);
            if (_alreadyDetectEncoding && _reader.BaseStream.Position == 0)
                Encoding curEncoding = _reader.CurrentEncoding;
                // Close the stream, and reopen the stream to make the BOM correctly processed.
                // The reader has already detected encoding, so if we don't reopen the stream, the BOM (if there is any)
                // will be treated as a regular character.
                CreateStreams(_path, null, _mode, _access, _share, curEncoding);
                _alreadyDetectEncoding = false;
                for (long currentBlock = 0; (currentBlock < readCount) || (readToEnd); ++currentBlock)
                    if (waitChanges && _provider.Stopping)
                        waitChanges = false;
                    if (_usingByteEncoding)
                        if (!ReadByteEncoded(waitChanges, blocks, readBackward: false))
                        if (_usingDelimiter || _isRawStream)
                            if (!ReadDelimited(waitChanges, blocks, readBackward: false, _delimiter))
                            if (!ReadByLine(waitChanges, blocks, readBackward: false))
                s_tracer.WriteLine("blocks read = {0}", blocks.Count);
                if ((e is IOException) ||
                    (e is ArgumentException) ||
                    (e is System.Security.SecurityException) ||
                    (e is UnauthorizedAccessException) ||
                    (e is ArgumentNullException))
                    // Exception contains specific message about the error occurred and so no need for errordetails.
                    _provider.WriteError(new ErrorRecord(e, "GetContentReaderIOError", ErrorCategory.ReadError, _path));
            return blocks.ToArray();
        /// Read the content regardless of the 'waitForChanges' flag.
        /// <param name="readCount"></param>
        internal IList ReadWithoutWaitingChanges(long readCount)
            bool oldWaitChanges = _waitForChanges;
            _waitForChanges = false;
                return Read(readCount);
                _waitForChanges = oldWaitChanges;
        /// Move the pointer of the stream to the position where there are 'backCount' number
        /// of items (depends on what we are using: delimiter? line? byts?) to the end of the file.
        /// <param name="backCount"></param>
        internal void SeekItemsBackward(int backCount)
            if (backCount < 0)
                // The caller needs to guarantee that 'backCount' is greater or equals to 0
                throw PSTraceSource.NewArgumentException(nameof(backCount));
            s_tracer.WriteLine("blocks seek backwards = {0}", backCount);
            if (_reader != null)
                // Make the reader automatically detect the encoding
                _reader.Peek();
                _alreadyDetectEncoding = true;
            Seek(0, SeekOrigin.End);
            if (backCount == 0)
                // If backCount is 0, we should move the position to the end of the file.
                // Maybe the "waitForChanges" is true in this case, which means that we are waiting for new inputs.
            string actualDelimiter = string.Create(
                _delimiter.Length,
                _delimiter,
                (chars, buf) =>
                    for (int i = 0, j = buf.Length - 1; i < chars.Length; i++, j--)
                        chars[i] = buf[j];
            long currentBlock = 0;
            string lastDelimiterMatch = null;
                if (_isRawStream)
                    // We always read to the end for the raw data.
                    // If it's indicated as RawStream, we move the pointer to the
                    // beginning of the file
                for (; currentBlock < backCount; ++currentBlock)
                        if (!ReadByteEncoded(waitChanges: false, blocks, readBackward: true))
                        if (_usingDelimiter)
                            if (!ReadDelimited(waitChanges: false, blocks, readBackward: true, actualDelimiter))
                            // If the delimiter is at the end of the file, we need to read one more
                            // to get to the right position. For example:
                            //      ua123ua456ua -- -Tail 1
                            // If we read backward only once, we get 'ua', and cannot get to the right position
                            // So we read one more time, get 'ua456ua', and then we can get the right position
                            lastDelimiterMatch = (string)blocks[0];
                            if (currentBlock == 0 && lastDelimiterMatch.Equals(actualDelimiter, StringComparison.Ordinal))
                                backCount++;
                            if (!ReadByLine(waitChanges: false, blocks, readBackward: true))
                    blocks.Clear();
                // If usingByteEncoding is true, we don't create the reader and _backReader
                if (!_usingByteEncoding)
                    long curStreamPosition = _backReader.GetCurrentPosition();
                        if (currentBlock == backCount)
                            Dbg.Diagnostics.Assert(lastDelimiterMatch != null, "lastDelimiterMatch should not be null when currentBlock == backCount");
                            if (lastDelimiterMatch.EndsWith(actualDelimiter, StringComparison.Ordinal))
                                curStreamPosition += _backReader.GetByteCount(_delimiter);
                    Seek(curStreamPosition, SeekOrigin.Begin);
                s_tracer.WriteLine("blocks seek position = {0}", _stream.Position);
        private bool ReadByLine(bool waitChanges, List<object> blocks, bool readBackward)
            // Reading lines as strings
            string line = readBackward ? _backReader.ReadLine() : _reader.ReadLine();
                if (waitChanges)
                    // We only wait for changes when read forwards. So here we use reader, instead of 'localReader'
                        WaitForChanges(_path, _mode, _access, _share, _reader.CurrentEncoding);
                        line = _reader.ReadLine();
                    while ((line == null) && (!_provider.Stopping));
            if (line != null)
                blocks.Add(line);
            int peekResult = readBackward ? _backReader.Peek() : _reader.Peek();
            if (peekResult == -1)
        private bool ReadDelimited(bool waitChanges, List<object> blocks, bool readBackward, string actualDelimiter)
                // when -Raw is used we want to anyway read the whole thing
                // so avoiding the while loop by reading the entire content.
                string contentRead = _reader.ReadToEnd();
                if (contentRead.Length > 0)
                    blocks.Add(contentRead);
                // We already read whole file so return EOF.
            // Since the delimiter is a string, we're essentially
            // dealing with a "find the substring" algorithm, but with
            // the additional restriction that we cannot read past the
            // end of the delimiter. If we read past the end of the delimiter,
            // then we'll eat up bytes that we need from the filestream.
            // The solution is a modified Boyer-Moore string search algorithm.
            // This version retains the sub-linear search performance (via the
            // lookup tables).
            int numRead = 0;
            int currentOffset = actualDelimiter.Length;
            Span<char> readBuffer = stackalloc char[currentOffset];
            bool delimiterNotFound = true;
            _currentLineContent.Clear();
                // Read in the required batch of characters
                numRead = readBackward
                                ? _backReader.Read(readBuffer.Slice(0, currentOffset))
                                : _reader.Read(readBuffer.Slice(0, currentOffset));
                // If we want to wait for changes, then we'll keep on attempting to read
                // until we fill the buffer.
                if (numRead == 0)
                        // But stop reading if the provider is stopping
                        while ((numRead < currentOffset) && (!_provider.Stopping))
                            // Get the change, and try to read more characters
                            // We only wait for changes when read forwards, so here we don't need to check if 'readBackward' is
                            // true or false, we only use 'reader'. The member 'reader' will be updated by WaitForChanges.
                            numRead += _reader.Read(readBuffer.Slice(0, currentOffset - numRead));
                if (numRead > 0)
                    _currentLineContent.Append(readBuffer.Slice(0, numRead));
                    // Look up the final character in our offset table.
                    // If the character doesn't exist in the lookup table, then it's not in
                    // our search key.  That means the match must happen strictly /after/ the
                    // current position.  Because of that, we can feel confident reading in the
                    // number of characters in the search key, without the risk of reading too many.
                    var currentChar = _currentLineContent[_currentLineContent.Length - 1];
                    currentOffset = _offsetDictionary[Unsafe.As<char, byte>(ref currentChar)];
                    // We want to keep reading if delimiter not found and we haven't hit the end of file
                    delimiterNotFound = true;
                    // If the final letters matched, then we will get an offset of "0".
                    // In that case, we'll either have a match (and break from the while loop,)
                    // or we need to move the scan forward one position.
                    if (currentOffset == 0)
                        currentOffset = 1;
                        if (actualDelimiter.Length <= _currentLineContent.Length)
                            delimiterNotFound = false;
                            int j = _currentLineContent.Length - actualDelimiter.Length;
                            for (; i < actualDelimiter.Length; i++, j++)
                                if (actualDelimiter[i] != _currentLineContent[j])
            while (delimiterNotFound && (numRead != 0));
            // We've reached the end of file or end of line.
            if (_currentLineContent.Length > 0)
                // Add the block read to the output array list, trimming a trailing delimiter, if present.
                // Note: If -Tail was specified, we get here in the course of 2 distinct passes:
                //  - Once while reading backward simply to determine the appropriate *start position* for later forward reading, ignoring the content of the blocks read (in reverse).
                //  - Then again during forward reading, for regular output processing; it is only then that trimming the delimiter is necessary.
                //    (Trimming it during backward reading would not only be unnecessary, but could interfere with determining the correct start position.)
                blocks.Add(
                    !readBackward && !delimiterNotFound
                        ? _currentLineContent.ToString(0, _currentLineContent.Length - actualDelimiter.Length)
                        : _currentLineContent.ToString());
            if (peekResult != -1)
                if (readBackward && _currentLineContent.Length > 0)
        private bool ReadByteEncoded(bool waitChanges, List<object> blocks, bool readBackward)
                // if RawSteam, read all bytes and return. When RawStream is used, we dont
                // support -first, -last
                byte[] bytes = new byte[_stream.Length];
                int numBytesToRead = (int)_stream.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                    // Read may return anything from 0 to numBytesToRead.
                    int n = _stream.Read(bytes, numBytesRead, numBytesToRead);
                    // Break when the end of the file is reached.
                    if (n == 0)
                    numBytesRead += n;
                    numBytesToRead -= n;
                if (numBytesRead == 0)
                    blocks.Add(bytes);
            if (readBackward)
                if (_stream.Position == 0)
                _stream.Position--;
                blocks.Add((byte)_stream.ReadByte());
            // Reading bytes not strings
            int byteRead = _stream.ReadByte();
            // We've found the end of the file.
            if (byteRead == -1)
                // If we want to tail the file, wait for
                // the changes
                    WaitForChanges(_path, _mode, _access, _share, Encoding.Default);
                    byteRead = _stream.ReadByte();
            // Add the byte we read to the list of blocks
            if (byteRead != -1)
                blocks.Add((byte)byteRead);
        private void CreateStreams(string filePath, string streamName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, Encoding fileEncoding)
            // Try to mask off the ReadOnly, and Hidden attributes
            // if they've specified Force.
            if (File.Exists(filePath) && _provider.Force)
                // Store the old attributes so that we can recover them
                // in the Close();
                _oldAttributes = File.GetAttributes(filePath);
                _haveOldAttributes = true;
                // Clear the hidden attribute, and if we're writing, also clear readonly.
                var attributesToClear = FileAttributes.Hidden;
                if ((fileAccess & (FileAccess.Write)) != 0)
                    attributesToClear |= FileAttributes.ReadOnly;
                File.SetAttributes(_path, (File.GetAttributes(filePath) & ~attributesToClear));
            // If we want to write to the stream, attempt to open it for reading as well
            // so that we can determine the file encoding as we append to it
            FileAccess requestedAccess = fileAccess;
                fileAccess = FileAccess.ReadWrite;
                if (!string.IsNullOrEmpty(streamName))
                    _stream = AlternateDataStreamUtilities.CreateFileStream(filePath, streamName, fileMode, fileAccess, fileShare);
                    _stream = new FileStream(filePath, fileMode, fileAccess, fileShare);
                    _stream = AlternateDataStreamUtilities.CreateFileStream(filePath, streamName, fileMode, requestedAccess, fileShare);
                    _stream = new FileStream(filePath, fileMode, requestedAccess, fileShare);
                // Open the reader stream
                if ((fileAccess & (FileAccess.Read)) != 0)
                    _reader = new StreamReader(_stream, fileEncoding);
                    _backReader = new FileStreamBackReader(_stream, fileEncoding);
                // Open the writer stream
                    // Ensure we are using the proper encoding
                    if ((_reader != null) &&
                        ((fileAccess & (FileAccess.Read)) != 0))
                        fileEncoding = _reader.CurrentEncoding;
                    _writer = new StreamWriter(_stream, fileEncoding);
        /// Waits for changes to the specified file.  To do this, it closes the file
        /// and then monitors for changes.  Once a change appears, it reopens the streams
        /// and seeks to the last read position.
        /// <param name="filePath">The path of the file to read / monitor.</param>
        /// <param name="fileMode">The FileMode of the file (ie: Open / Append).</param>
        /// <param name="fileAccess">The access properties of the file (ie: Read / Write).</param>
        /// <param name="fileShare">The sharing properties of the file (ie: Read / ReadWrite).</param>
        /// <param name="fileEncoding">The encoding of the file.</param>
        private void WaitForChanges(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, Encoding fileEncoding)
            // Close the old stream, and store our current position.
                _fileOffset = _stream.Position;
            // Watch for changes, as a blocking call.
            FileInfo watchFile = new FileInfo(filePath);
            long originalLength = watchFile.Length;
            using (FileSystemWatcher watcher = new FileSystemWatcher(watchFile.DirectoryName, watchFile.Name))
                ErrorEventArgs errorEventArgs = null;
                var tcs = new TaskCompletionSource<FileSystemEventArgs>();
                FileSystemEventHandler onChangedHandler = (object source, FileSystemEventArgs e) => tcs.TrySetResult(e);
                RenamedEventHandler onRenamedHandler = (object source, RenamedEventArgs e) => tcs.TrySetResult(e);
                ErrorEventHandler onErrorHandler = (object source, ErrorEventArgs e) =>
                    errorEventArgs = e;
                    tcs.TrySetResult(new FileSystemEventArgs(WatcherChangeTypes.All, watchFile.DirectoryName, watchFile.Name));
                // With WaitForChanged, we registered for all change types, so we do the same here.
                watcher.Changed += onChangedHandler;
                watcher.Created += onChangedHandler;
                watcher.Deleted += onChangedHandler;
                watcher.Renamed += onRenamedHandler;
                watcher.Error += onErrorHandler;
                    watcher.EnableRaisingEvents = true;
                    while (!_provider.Stopping)
                        bool isTaskCompleted = tcs.Task.Wait(500);
                        if (errorEventArgs != null)
                            throw errorEventArgs.GetException();
                        if (isTaskCompleted)
                        // If a process is still writing, .NET doesn't generate a change notification.
                        // So do a simple comparison on file size
                        watchFile.Refresh();
                        if (originalLength != watchFile.Length)
                    // Done here to guarantee that the handlers are removed prior
                    // to the call to ManualResetEvent's Dispose() method.
                    watcher.EnableRaisingEvents = false;
                    watcher.Changed -= onChangedHandler;
                    watcher.Created -= onChangedHandler;
                    watcher.Deleted -= onChangedHandler;
                    watcher.Renamed -= onRenamedHandler;
                    watcher.Error -= onErrorHandler;
            // Let the change complete.
            // This is a fairly arbitrary number.  Without it, though,
            // some of the filesystem streams cannot be reopened.
            // Reopen the streams.
            CreateStreams(filePath, null, fileMode, fileAccess, fileShare, fileEncoding);
            // If the file has been shortened, restart from zero.
            // That will let us catch log roll-overs.
            if (_fileOffset > _stream.Length)
                _fileOffset = 0;
            // Seek to the place we last left off.
            _stream.Seek(_fileOffset, SeekOrigin.Begin);
            _reader?.DiscardBufferedData();
            _backReader?.DiscardBufferedData();
        /// Moves the current stream position in the file.
        /// The offset from the origin to move the position to.
        /// The origin from which the offset is calculated.
        public void Seek(long offset, SeekOrigin origin)
            _writer?.Flush();
            _stream.Seek(offset, origin);
        /// Closes the file.
            bool streamClosed = false;
            if (_writer != null)
                    streamClosed = true;
                _reader.Dispose();
            if (_backReader != null)
                _backReader.Dispose();
            if (!streamClosed)
                _stream.Flush();
            // Reset the attributes
            if (_haveOldAttributes && _provider.Force)
                File.SetAttributes(_path, _oldAttributes);
        /// Writes the specified object to the file.
        /// The objects to write to the file
        /// The objects written to the file.
        public IList Write(IList content)
            foreach (object line in content)
                object[] contentArray = line as object[];
                if (contentArray != null)
                    foreach (object obj in contentArray)
        private void WriteObject(object content)
            if (content == null)
                    byte byteToWrite = (byte)content;
                    _stream.WriteByte(byteToWrite);
                    throw PSTraceSource.NewArgumentException(nameof(content), FileSystemProviderStrings.ByteEncodingError);
                    _writer.Write(content.ToString());
                    _writer.WriteLine(content.ToString());
        /// Closes the file stream.
                _stream?.Dispose();
                _reader?.Dispose();
                _backReader?.Dispose();
                _writer?.Dispose();
    internal sealed class FileStreamBackReader : StreamReader
        internal FileStreamBackReader(FileStream fileStream, Encoding encoding)
            : base(fileStream, encoding)
            _stream = fileStream;
            if (_stream.Length > 0)
                long curPosition = _stream.Position;
                _stream.Seek(0, SeekOrigin.Begin);
                base.Peek();
                _stream.Position = curPosition;
                _currentEncoding = base.CurrentEncoding;
                _currentPosition = _stream.Position;
                // Get the oem encoding and system current ANSI code page
                _oemEncoding = EncodingConversion.Convert(null, EncodingConversion.OEM);
                _defaultAnsiEncoding = EncodingConversion.Convert(null, EncodingConversion.Default);
        private readonly FileStream _stream;
        private readonly Encoding _currentEncoding;
        private readonly Encoding _oemEncoding;
        private readonly Encoding _defaultAnsiEncoding;
        private const int BuffSize = 4096;
        private readonly byte[] _byteBuff = new byte[BuffSize];
        private readonly char[] _charBuff = new char[BuffSize];
        private int _byteCount = 0;
        private int _charCount = 0;
        private long _currentPosition = 0;
        private const byte BothTopBitsSet = 0xC0;
        private const byte TopBitUnset = 0x80;
        /// We don't support this method because it is not used by the ReadBackward method in FileStreamContentReaderWriter.
        public override int ReadBlock(char[] buffer, int index, int count)
            // This method is not supposed to be used
        public override string ReadToEnd()
        /// Reset the internal character buffer. Use it only when the position of the internal buffer and
        /// the base stream do not match. These positions can become mismatch when the user read the data
        /// into the buffer and then seek a new position in the underlying stream.
        internal new void DiscardBufferedData()
            base.DiscardBufferedData();
            _charCount = 0;
            _byteCount = 0;
        /// Return the current actual stream position.
        internal long GetCurrentPosition()
            if (_charCount == 0)
                return _currentPosition;
            // _charCount > 0
            int byteCount = _currentEncoding.GetByteCount(_charBuff, 0, _charCount);
            return (_currentPosition + byteCount);
        /// Get the number of bytes the delimiter will
        /// be encoded to.
        /// <param name="delimiter"></param>
        internal int GetByteCount(string delimiter)
            char[] chars = delimiter.ToCharArray();
            return _currentEncoding.GetByteCount(chars, 0, chars.Length);
        /// Peek the next character.
        /// <returns>Return -1 if we reach the head of the file.</returns>
        public override int Peek()
                if (RefillCharBuffer() == -1)
            // Return the next available character, but DONT consume it (don't advance the _charCount)
            return (int)_charBuff[_charCount - 1];
        /// Read the next character.
        public override int Read()
            _charCount--;
            return _charBuff[_charCount];
        /// Read a specific maximum of characters from the current stream into a buffer.
        /// <param name="buffer">Output buffer.</param>
        /// <param name="index">Start position to write with.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>Return the number of characters read, or -1 if we reach the head of the file.</returns>
        public override int Read(char[] buffer, int index, int count)
            return ReadSpan(new Span<char>(buffer, index, count));
        /// Read characters from the current stream into a Span buffer.
        public override int Read(Span<char> buffer)
            return ReadSpan(buffer);
        private int ReadSpan(Span<char> buffer)
            // deal with the argument validation
            int charRead = 0;
            int count = buffer.Length;
                        return charRead;
                int toRead = _charCount > count ? count : _charCount;
                for (; toRead > 0; toRead--, count--, charRead++)
                    buffer[index++] = _charBuff[--_charCount];
            while (count > 0);
        /// Read a line from the current stream.
        /// <returns>Return null if we reach the head of the file.</returns>
            if (_charCount == 0 && RefillCharBuffer() == -1)
            int charsToRemove = 0;
            if (_charBuff[_charCount - 1] == '\r' ||
                _charBuff[_charCount - 1] == '\n')
                charsToRemove++;
                line.Insert(0, _charBuff[--_charCount]);
                if (_charBuff[_charCount] == '\n')
                    if (_charCount > 0 && _charBuff[_charCount - 1] == '\r')
                while (_charCount > 0)
                        line.Remove(line.Length - charsToRemove, charsToRemove);
                        return line.ToString();
        /// Refill the internal character buffer.
        private int RefillCharBuffer()
            if ((RefillByteBuff()) == -1)
            _charCount = _currentEncoding.GetChars(_byteBuff, 0, _byteCount, _charBuff, 0);
            return _charCount;
        /// Refill the internal byte buffer.
        private int RefillByteBuff()
            long lengthLeft = _stream.Position;
            if (lengthLeft == 0)
            int toRead = lengthLeft > BuffSize ? BuffSize : (int)lengthLeft;
            _stream.Seek(-toRead, SeekOrigin.Current);
            if (_currentEncoding is UTF8Encoding)
                // It's UTF-8, we need to detect the starting byte of a character
                    byte curByte = (byte)_stream.ReadByte();
                    if ((curByte & BothTopBitsSet) == BothTopBitsSet ||
                        (curByte & TopBitUnset) == 0x00)
                        _byteBuff[0] = curByte;
                        _byteCount = 1;
                } while (lengthLeft > _stream.Position);
                if (lengthLeft == _stream.Position)
                    // Cannot find a starting byte. The file is NOT UTF-8 format. Read 'toRead' number of bytes
                _byteCount += _stream.Read(_byteBuff, _byteCount, (int)(lengthLeft - _stream.Position));
                _stream.Position = _currentPosition;
            else if (_currentEncoding is UnicodeEncoding ||
                _currentEncoding is UTF32Encoding ||
                _currentEncoding.IsSingleByte)
                // Unicode -- two bytes per character
                // UTF-32 -- four bytes per character
                // ASCII -- one byte per character
                // The BufferSize will be a multiple of 4, so we can just read toRead number of bytes
                // if the current file is encoded by any of these formatting
                // If IsSingleByteCharacterSet() returns true, we are sure that the given encoding is OEM
                // or Default, and it is SBCS(single byte character set) code page -- one byte per character
                _byteCount = _stream.Read(_byteBuff, 0, toRead);
                // OEM and ANSI code pages include multibyte CJK code pages. If the current code page
                // is MBCS(multibyte character set), we cannot detect a starting byte.
                // UTF-7 has some characters encoded into UTF-16 and then in Modified Base64,
                // the start of these characters is indicated by a '+' sign, and the end is
                // indicated by a character that is not in Modified Base64 set.
                // For these encodings, we cannot detect a starting byte with confidence when
                // reading bytes backward. Throw out exception in these cases.
                string errMsg = StringUtil.Format(
                    FileSystemProviderStrings.ReadBackward_Encoding_NotSupport,
                    _currentEncoding.EncodingName);
                throw new BackReaderEncodingNotSupportedException(errMsg, _currentEncoding.EncodingName);
            return _byteCount;
    /// The exception that indicates the encoding is not supported when reading backward.
    internal sealed class BackReaderEncodingNotSupportedException : NotSupportedException
        internal BackReaderEncodingNotSupportedException(string message, string encodingName)
            EncodingName = encodingName;
        internal BackReaderEncodingNotSupportedException(string encodingName)
        /// Get the encoding name.
        internal string EncodingName { get; }
