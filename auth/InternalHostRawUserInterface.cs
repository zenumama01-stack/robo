    class InternalHostRawUserInterface : PSHostRawUserInterface
        InternalHostRawUserInterface(PSHostRawUserInterface externalRawUI, InternalHost parentHost)
            // externalRawUI may be null
            _externalRawUI = externalRawUI;
            _parentHost = parentHost;
        ThrowNotInteractive()
            // It might be interesting to do something like
            // GetCallingMethodAndParameters here and display the name,
            // but I don't want to put that in mainline non-trace code.
            string message = HostInterfaceExceptionsStrings.HostFunctionNotImplemented;
            HostException e = new HostException(
                "HostFunctionNotImplemented",
                ErrorCategory.NotImplemented);
        /// if the RawUI property of the external host is null, possibly because the PSHostRawUserInterface is not
        ///  implemented by the external host
                if (_externalRawUI == null)
                    ThrowNotInteractive();
                ConsoleColor result = _externalRawUI.ForegroundColor;
                _externalRawUI.ForegroundColor = value;
                ConsoleColor result = _externalRawUI.BackgroundColor;
                _externalRawUI.BackgroundColor = value;
                Coordinates result = _externalRawUI.CursorPosition;
                _externalRawUI.CursorPosition = value;
                Coordinates result = _externalRawUI.WindowPosition;
                _externalRawUI.WindowPosition = value;
                int result = _externalRawUI.CursorSize;
                _externalRawUI.CursorSize = value;
                Size result = _externalRawUI.BufferSize;
                _externalRawUI.BufferSize = value;
                Size result = _externalRawUI.WindowSize;
                _externalRawUI.WindowSize = value;
                Size result = _externalRawUI.MaxWindowSize;
                Size result = _externalRawUI.MaxPhysicalWindowSize;
            KeyInfo result = new KeyInfo();
                result = _externalRawUI.ReadKey(options);
                // PipelineStoppedException is thrown by host when it wants
                // to stop the pipeline.
                LocalPipeline lpl = (LocalPipeline)((RunspaceBase)_parentHost.Context.CurrentRunspace).GetCurrentlyRunningPipeline();
                lpl.Stopper.Stop();
            _externalRawUI.FlushInputBuffer();
                bool result = _externalRawUI.KeyAvailable;
        WindowTitle
                string result = _externalRawUI.WindowTitle;
                _externalRawUI.WindowTitle = value;
            _externalRawUI.SetBufferContents(origin, contents);
        /// <param name="r">
        SetBufferContents(Rectangle r, BufferCell fill)
            _externalRawUI.SetBufferContents(r, fill);
        /// <param name="r"></param>
        BufferCell[,]
        GetBufferContents(Rectangle r)
            return _externalRawUI.GetBufferContents(r);
            _externalRawUI.ScrollBufferContents(source, destination, clip, fill);
        public override int LengthInBufferCells(string str)
            return _externalRawUI.LengthInBufferCells(str);
        public override int LengthInBufferCells(string str, int offset)
            return _externalRawUI.LengthInBufferCells(str, offset);
        LengthInBufferCells(char character)
            return _externalRawUI.LengthInBufferCells(character);
        private readonly PSHostRawUserInterface _externalRawUI;
        private readonly InternalHost _parentHost;
