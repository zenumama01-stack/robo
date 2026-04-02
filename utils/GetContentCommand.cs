    /// A command to get the content of an item at a specified path.
    [Cmdlet(VerbsCommon.Get, "Content", DefaultParameterSetName = "Path", SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096490")]
    public class GetContentCommand : ContentCommandBase
        /// The number of content items to retrieve per block.
        /// By default this value is 1 which means read one block
        /// at a time.  To read all blocks at once, set this value
        /// to a negative number.
        public long ReadCount { get; set; } = 1;
        /// The number of content items to retrieve.
        [ValidateRange(0, long.MaxValue)]
        [Alias("First", "Head")]
        public long TotalCount { get; set; } = -1;
        /// The number of content items to retrieve from the back of the file.
        [ValidateRange(0, int.MaxValue)]
        [Alias("Last")]
        public int Tail { get; set; } = -1;
                return InvokeProvider.Content.GetContentReaderDynamicParameters(Path[0], context);
            return InvokeProvider.Content.GetContentReaderDynamicParameters(".", context);
        /// Gets the content of an item at the specified path.
            // TotalCount and Tail should not be specified at the same time.
            // Throw out terminating error if this is the case.
            if (TotalCount != -1 && Tail != -1)
                string errMsg = StringUtil.Format(SessionStateStrings.GetContent_TailAndHeadCannotCoexist, "TotalCount", "Tail");
                ErrorRecord error = new(new InvalidOperationException(errMsg), "TailAndHeadCannotCoexist", ErrorCategory.InvalidOperation, null);
            if (TotalCount == 0)
                // Don't read anything
            // Get the content readers
            contentStreams = this.GetContentReaders(Path, currentContext);
                // Iterate through the content holders reading the content
                foreach (ContentHolder holder in contentStreams)
                    long countRead = 0;
                    Dbg.Diagnostics.Assert(holder.Reader != null, "All holders should have a reader assigned");
                    if (Tail != -1 && holder.Reader is not FileSystemContentReaderWriter)
                        string errMsg = SessionStateStrings.GetContent_TailNotSupported;
                        ErrorRecord error = new(new InvalidOperationException(errMsg), "TailNotSupported", ErrorCategory.InvalidOperation, Tail);
                    // If Tail is -1, we are supposed to read all content out. This is same
                    // as reading forwards. So we read forwards in this case.
                    // If Tail is positive, we seek the right position. Or, if the seek failed
                    // because of an unsupported encoding, we scan forward to get the tail content.
                    if (Tail >= 0)
                        bool seekSuccess = false;
                            seekSuccess = SeekPositionForTail(holder.Reader);
                                    "ProviderContentReadError",
                                    SessionStateStrings.ProviderContentReadError,
                        // If the seek was successful, we start to read forwards from that
                        // point. Otherwise, we need to scan forwards to get the tail content.
                        if (!seekSuccess && !ScanForwardsForTail(holder, currentContext))
                    IList results = null;
                        long countToRead = ReadCount;
                        // Make sure we only ask for the amount the user wanted
                        // I am using TotalCount - countToRead so that I don't
                        // have to worry about overflow
                        if (TotalCount > 0 && (countToRead == 0 || TotalCount - countToRead < countRead))
                            countToRead = TotalCount - countRead;
                            results = holder.Reader.Read(countToRead);
                            MshLog.LogProviderHealthEvent(this.Context, holder.PathInfo.Provider.Name, providerException, Severity.Warning);
                            WriteError(new ErrorRecord(providerException.ErrorRecord, providerException));
                        if (results != null && results.Count > 0)
                            countRead += results.Count;
                            if (ReadCount == 1)
                                // Write out the content as a single object
                                WriteContentObject(results[0], countRead, holder.PathInfo, currentContext);
                                // Write out the content as an array of objects
                                WriteContentObject(results, countRead, holder.PathInfo, currentContext);
                    } while (results != null && results.Count > 0 && (TotalCount == -1 || countRead < TotalCount));
                // Close all the content readers
                CloseContent(contentStreams, false);
                // Empty the content holder array
        /// Scan forwards to get the tail content.
        /// <param name="holder"></param>
        /// <param name="currentContext"></param>
        /// true if no error occurred
        /// false if there was an error
        private bool ScanForwardsForTail(in ContentHolder holder, CmdletProviderContext currentContext)
            var fsReader = holder.Reader as FileSystemContentReaderWriter;
            Dbg.Diagnostics.Assert(fsReader != null, "Tail is only supported for FileSystemContentReaderWriter");
            Queue<object> tailResultQueue = new();
                    results = fsReader.ReadWithoutWaitingChanges(ReadCount);
                    // Create and save the error record. The error record
                    // will be written outside the while loop.
                    // This is to make sure the accumulated results get written
                    // out before the error record when the 'scanForwardForTail' is true.
                        providerException);
                    foreach (object entry in results)
                        if (tailResultQueue.Count == Tail)
                            tailResultQueue.Dequeue();
                        tailResultQueue.Enqueue(entry);
            } while (results != null && results.Count > 0);
            if (tailResultQueue.Count > 0)
                // Respect the ReadCount parameter.
                // Output single object when ReadCount == 1; Output array otherwise
                if (ReadCount <= 0 || (ReadCount >= tailResultQueue.Count && ReadCount != 1))
                    count = tailResultQueue.Count;
                    WriteContentObject(tailResultQueue.ToArray(), count, holder.PathInfo, currentContext);
                else if (ReadCount == 1)
                    // Write out the content as single object
                    while (tailResultQueue.Count > 0)
                        WriteContentObject(tailResultQueue.Dequeue(), count++, holder.PathInfo, currentContext);
                else // ReadCount < Queue.Count
                    while (tailResultQueue.Count >= ReadCount)
                        List<object> outputList = new((int)ReadCount);
                        for (int idx = 0; idx < ReadCount; idx++, count++)
                            outputList.Add(tailResultQueue.Dequeue());
                        WriteContentObject(outputList.ToArray(), count, holder.PathInfo, currentContext);
        /// Seek position to the right place.
        /// <param name="reader">
        /// reader should be able to be casted to FileSystemContentReader
        /// true if the stream pointer is moved to the right place
        /// false if we cannot seek
        private bool SeekPositionForTail(IContentReader reader)
            var fsReader = reader as FileSystemContentReaderWriter;
                fsReader.SeekItemsBackward(Tail);
            catch (BackReaderEncodingNotSupportedException)
                // Move to the head
                fsReader.Seek(0, SeekOrigin.Begin);
        /// Be sure to clean up.
