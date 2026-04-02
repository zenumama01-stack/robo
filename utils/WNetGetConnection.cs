        private static bool s_WNetApiNotAvailable;
        [LibraryImport("mpr.dll", EntryPoint = "WNetGetConnectionW", StringMarshalling = StringMarshalling.Utf16)]
        internal static partial int WNetGetConnection(ReadOnlySpan<char> localName, Span<char> remoteName, ref int remoteNameLength);
        internal static int GetUNCForNetworkDrive(char drive, out string? uncPath)
            uncPath = null;
            ReadOnlySpan<char> driveName = [drive, ':', '\0'];
            int bufferSize = MAX_PATH;
            Span<char> uncBuffer = stackalloc char[MAX_PATH];
            if (InternalTestHooks.WNetGetConnectionBufferSize > 0 && InternalTestHooks.WNetGetConnectionBufferSize <= MAX_PATH)
                bufferSize = InternalTestHooks.WNetGetConnectionBufferSize;
                uncBuffer = uncBuffer.Slice(0, bufferSize);
                int errorCode;
                        errorCode = WNetGetConnection(driveName, uncBuffer, ref bufferSize);
                    catch (DllNotFoundException)
                    if (errorCode == ERROR_SUCCESS)
                        // Cannot rely on bufferSize as it's only set if
                        // the first call ended with ERROR_MORE_DATA,
                        // instead slice at the null terminator.
                            fixed (char* uncBufferPtr = uncBuffer)
                                uncPath = new string(uncBufferPtr);
                if (errorCode == ERROR_MORE_DATA)
                    uncBuffer = rentedArray = ArrayPool<char>.Shared.Rent(bufferSize);
