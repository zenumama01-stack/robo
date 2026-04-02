    /// Base class for a variety of commandlets that take color parameters.
    public
    class ConsoleColorCmdlet : PSCmdlet
        /// Initializes a new instance of the <see cref="ConsoleColorCmdlet"/> class.
        public ConsoleColorCmdlet()
            _consoleColorEnumType = typeof(ConsoleColor);
        /// The -ForegroundColor parameter.
        ConsoleColor
        ForegroundColor
                if (!_isFgColorSet)
                    _fgColor = this.Host.UI.RawUI.ForegroundColor;
                    _isFgColorSet = true;
                return _fgColor;
                if (value >= (ConsoleColor)0 && value <= (ConsoleColor)15)
                    _fgColor = value;
                    ThrowTerminatingError(BuildOutOfRangeErrorRecord(value, "SetInvalidForegroundColor"));
        BackgroundColor
                if (!_isBgColorSet)
                    _bgColor = this.Host.UI.RawUI.BackgroundColor;
                    _isBgColorSet = true;
                return _bgColor;
                    _bgColor = value;
                    ThrowTerminatingError(BuildOutOfRangeErrorRecord(value, "SetInvalidBackgroundColor"));
        #region helper
        private static ErrorRecord BuildOutOfRangeErrorRecord(object val, string errorId)
            string msg = StringUtil.Format(HostStrings.InvalidColorErrorTemplate, val.ToString());
            ArgumentOutOfRangeException e = new("value", val, msg);
            return new ErrorRecord(e, errorId, ErrorCategory.InvalidArgument, null);
        #endregion helper
        private ConsoleColor _fgColor;
        private ConsoleColor _bgColor;
        private bool _isFgColorSet = false;
        private bool _isBgColorSet = false;
        private readonly Type _consoleColorEnumType;
