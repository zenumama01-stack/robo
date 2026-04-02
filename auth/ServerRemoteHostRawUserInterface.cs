    /// The ServerRemoteHostRawUserInterface class.
    internal class ServerRemoteHostRawUserInterface : PSHostRawUserInterface
        private HostDefaultData HostDefaultData
                return _remoteHostUserInterface.ServerRemoteHost.HostInfo.HostDefaultData;
        /// Constructor for ServerRemoteHostRawUserInterface.
        internal ServerRemoteHostRawUserInterface(ServerRemoteHostUserInterface remoteHostUserInterface)
            Dbg.Assert(remoteHostUserInterface != null, "Expected remoteHostUserInterface != null");
            _remoteHostUserInterface = remoteHostUserInterface;
            Dbg.Assert(!remoteHostUserInterface.ServerRemoteHost.HostInfo.IsHostRawUINull, "Expected !remoteHostUserInterface.ServerRemoteHost.HostInfo.IsHostRawUINull");
            _serverMethodExecutor = remoteHostUserInterface.ServerRemoteHost.ServerMethodExecutor;
        /// Foreground color.
                if (this.HostDefaultData.HasValue(HostDefaultDataId.ForegroundColor))
                    return (ConsoleColor)this.HostDefaultData.GetValue(HostDefaultDataId.ForegroundColor);
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetForegroundColor);
                this.HostDefaultData.SetValue(HostDefaultDataId.ForegroundColor, value);
                _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetForegroundColor, new object[] { value });
        /// Background color.
                if (this.HostDefaultData.HasValue(HostDefaultDataId.BackgroundColor))
                    return (ConsoleColor)this.HostDefaultData.GetValue(HostDefaultDataId.BackgroundColor);
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetBackgroundColor);
                this.HostDefaultData.SetValue(HostDefaultDataId.BackgroundColor, value);
                _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBackgroundColor, new object[] { value });
        /// Cursor position.
                if (this.HostDefaultData.HasValue(HostDefaultDataId.CursorPosition))
                    return (Coordinates)this.HostDefaultData.GetValue(HostDefaultDataId.CursorPosition);
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetCursorPosition);
                this.HostDefaultData.SetValue(HostDefaultDataId.CursorPosition, value);
                _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetCursorPosition, new object[] { value });
        /// Window position.
                if (this.HostDefaultData.HasValue(HostDefaultDataId.WindowPosition))
                    return (Coordinates)this.HostDefaultData.GetValue(HostDefaultDataId.WindowPosition);
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetWindowPosition);
                this.HostDefaultData.SetValue(HostDefaultDataId.WindowPosition, value);
                _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetWindowPosition, new object[] { value });
        /// Cursor size.
                if (this.HostDefaultData.HasValue(HostDefaultDataId.CursorSize))
                    return (int)this.HostDefaultData.GetValue(HostDefaultDataId.CursorSize);
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetCursorSize);
                this.HostDefaultData.SetValue(HostDefaultDataId.CursorSize, value);
                _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetCursorSize, new object[] { value });
        /// Buffer size.
                if (this.HostDefaultData.HasValue(HostDefaultDataId.BufferSize))
                    return (Size)this.HostDefaultData.GetValue(HostDefaultDataId.BufferSize);
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetBufferSize);
                this.HostDefaultData.SetValue(HostDefaultDataId.BufferSize, value);
                _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBufferSize, new object[] { value });
        /// Window size.
                if (this.HostDefaultData.HasValue(HostDefaultDataId.WindowSize))
                    return (Size)this.HostDefaultData.GetValue(HostDefaultDataId.WindowSize);
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetWindowSize);
                this.HostDefaultData.SetValue(HostDefaultDataId.WindowSize, value);
                _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetWindowSize, new object[] { value });
        /// Window title.
                if (this.HostDefaultData.HasValue(HostDefaultDataId.WindowTitle))
                    return (string)this.HostDefaultData.GetValue(HostDefaultDataId.WindowTitle);
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetWindowTitle);
                this.HostDefaultData.SetValue(HostDefaultDataId.WindowTitle, value);
                _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetWindowTitle, new object[] { value });
        /// Max window size.
                if (this.HostDefaultData.HasValue(HostDefaultDataId.MaxWindowSize))
                    return (Size)this.HostDefaultData.GetValue(HostDefaultDataId.MaxWindowSize);
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetMaxWindowSize);
        /// Max physical window size.
                if (this.HostDefaultData.HasValue(HostDefaultDataId.MaxPhysicalWindowSize))
                    return (Size)this.HostDefaultData.GetValue(HostDefaultDataId.MaxPhysicalWindowSize);
                    throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetMaxPhysicalWindowSize);
        /// Key available.
                throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetKeyAvailable);
        /// Read key.
            return _serverMethodExecutor.ExecuteMethod<KeyInfo>(RemoteHostMethodId.ReadKey, new object[] { options });
        /// Flush input buffer.
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.FlushInputBuffer);
        /// Scroll buffer contents.
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.ScrollBufferContents, new object[] { source, destination, clip, fill });
        /// Set buffer contents.
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBufferContents1, new object[] { rectangle, fill });
        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
            _serverMethodExecutor.ExecuteVoidMethod(RemoteHostMethodId.SetBufferContents2, new object[] { origin, contents });
        /// Get buffer contents.
            // This method had an implementation earlier. However, owing
            // to a potential security risk of a malicious server scrapping
            // the screen contents of a client, this is now removed
            throw RemoteHostExceptions.NewNotImplementedException(RemoteHostMethodId.GetBufferContents);
        // same as the implementation in the base class; included here to make it easier
        // to keep the other overload in sync: LengthInBufferCells(string, int)
        public override int LengthInBufferCells(string source)
        // more performant than the default implementation provided by PSHostRawUserInterface
        public override int LengthInBufferCells(string source, int offset)
            Dbg.Assert(string.IsNullOrEmpty(source) || (offset < source.Length), "offset < source.Length");
            return source.Length - offset;
