    #region OutputRendering
    /// Defines the options for output rendering.
    public enum OutputRendering
        /// <summary>Render ANSI only to host.</summary>
        Host = 0,
        /// <summary>Render as plaintext.</summary>
        PlainText = 1,
        /// <summary>Render as ANSI.</summary>
        Ansi = 2,
    #endregion OutputRendering
    /// Defines the options for views of progress rendering.
    public enum ProgressView
        /// <summary>Render progress using minimal space.</summary>
        Minimal = 0,
        /// <summary>Classic rendering of progress.</summary>
        Classic = 1,
    #region PSStyle
    /// Contains configuration for how PowerShell renders text.
    public sealed class PSStyle
        /// Contains foreground colors.
        public sealed class ForegroundColor
            /// Gets the color black.
            public string Black { get; } = "\x1b[30m";
            /// Gets the color red.
            public string Red { get; } = "\x1b[31m";
            /// Gets the color green.
            public string Green { get; } = "\x1b[32m";
            /// Gets the color yellow.
            public string Yellow { get; } = "\x1b[33m";
            /// Gets the color blue.
            public string Blue { get; } = "\x1b[34m";
            /// Gets the color magenta.
            public string Magenta { get; } = "\x1b[35m";
            /// Gets the color cyan.
            public string Cyan { get; } = "\x1b[36m";
            /// Gets the color white.
            public string White { get; } = "\x1b[37m";
            /// Gets the color bright black.
            public string BrightBlack { get; } = "\x1b[90m";
            /// Gets the color bright red.
            public string BrightRed { get; } = "\x1b[91m";
            /// Gets the color bright green.
            public string BrightGreen { get; } = "\x1b[92m";
            /// Gets the color bright yellow.
            public string BrightYellow { get; } = "\x1b[93m";
            /// Gets the color bright blue.
            public string BrightBlue { get; } = "\x1b[94m";
            /// Gets the color bright magenta.
            public string BrightMagenta { get; } = "\x1b[95m";
            /// Gets the color bright cyan.
            public string BrightCyan { get; } = "\x1b[96m";
            /// Gets the color bright white.
            public string BrightWhite { get; } = "\x1b[97m";
            /// Set as RGB (Red, Green, Blue).
            /// <param name="red">Byte value representing red.</param>
            /// <param name="green">Byte value representing green.</param>
            /// <param name="blue">Byte value representing blue.</param>
            /// <returns>String representing ANSI code for RGB value.</returns>
            public string FromRgb(byte red, byte green, byte blue)
                return $"\x1b[38;2;{red};{green};{blue}m";
            /// The color set as RGB as a single number.
            /// <param name="rgb">RGB value specified as an integer.</param>
            public string FromRgb(int rgb)
                byte red, green, blue;
                blue = (byte)(rgb & 0xFF);
                rgb >>= 8;
                green = (byte)(rgb & 0xFF);
                red = (byte)(rgb & 0xFF);
                return FromRgb(red, green, blue);
            /// Return the VT escape sequence for a foreground color.
            /// <param name="color">The foreground color to be mapped from.</param>
            /// <returns>The VT escape sequence representing the foreground color.</returns>
            public string FromConsoleColor(ConsoleColor color)
                return MapForegroundColorToEscapeSequence(color);
        /// Contains background colors.
        public sealed class BackgroundColor
            public string Black { get; } = "\x1b[40m";
            public string Red { get; } = "\x1b[41m";
            public string Green { get; } = "\x1b[42m";
            public string Yellow { get; } = "\x1b[43m";
            public string Blue { get; } = "\x1b[44m";
            public string Magenta { get; } = "\x1b[45m";
            public string Cyan { get; } = "\x1b[46m";
            public string White { get; } = "\x1b[47m";
            public string BrightBlack { get; } = "\x1b[100m";
            public string BrightRed { get; } = "\x1b[101m";
            public string BrightGreen { get; } = "\x1b[102m";
            public string BrightYellow { get; } = "\x1b[103m";
            public string BrightBlue { get; } = "\x1b[104m";
            public string BrightMagenta { get; } = "\x1b[105m";
            public string BrightCyan { get; } = "\x1b[106m";
            public string BrightWhite { get; } = "\x1b[107m";
            /// The color set as RGB (Red, Green, Blue).
                return $"\x1b[48;2;{red};{green};{blue}m";
            /// Return the VT escape sequence for a background color.
            /// <param name="color">The background color to be mapped from.</param>
            /// <returns>The VT escape sequence representing the background color.</returns>
                return MapBackgroundColorToEscapeSequence(color);
        /// Contains configuration for the progress bar visualization.
        public sealed class ProgressConfiguration
            /// Gets or sets the style for progress bar.
            public string Style
                get => _style;
                set => _style = ValidateNoContent(value);
            private string _style = "\x1b[33;1m";
            /// Gets or sets the max width of the progress bar.
            public int MaxWidth
                get => _maxWidth;
                    // Width less than 18 does not render correctly due to the different parts of the progress bar.
                    if (value < 18)
                        throw new ArgumentOutOfRangeException(nameof(MaxWidth), PSStyleStrings.ProgressWidthTooSmall);
                    _maxWidth = value;
            private int _maxWidth = 120;
            /// Gets or sets the view for progress bar.
            public ProgressView View { get; set; } = ProgressView.Minimal;
            /// Gets or sets a value indicating whether to use Operating System Command (OSC) control sequences 'ESC ]9;4;' to show indicator in terminal.
            public bool UseOSCIndicator { get; set; } = false;
        /// Contains formatting styles for steams and objects.
        public sealed class FormattingData
            /// Gets or sets the accent style for formatting.
            public string FormatAccent
                get => _formatAccent;
                set => _formatAccent = ValidateNoContent(value);
            private string _formatAccent = "\x1b[32;1m";
            /// Gets or sets the style for table headers.
            public string TableHeader
                get => _tableHeader;
                set => _tableHeader = ValidateNoContent(value);
            private string _tableHeader = "\x1b[32;1m";
            /// Gets or sets the style for custom table headers.
            public string CustomTableHeaderLabel
                get => _customTableHeaderLabel;
                set => _customTableHeaderLabel = ValidateNoContent(value);
            private string _customTableHeaderLabel = "\x1b[32;1;3m";
            /// Gets or sets the accent style for errors.
            public string ErrorAccent
                get => _errorAccent;
                set => _errorAccent = ValidateNoContent(value);
            private string _errorAccent = "\x1b[36;1m";
            /// Gets or sets the style for error messages.
                get => _error;
                set => _error = ValidateNoContent(value);
            private string _error = "\x1b[31;1m";
            /// Gets or sets the style for warning messages.
            public string Warning
                get => _warning;
                set => _warning = ValidateNoContent(value);
            private string _warning = "\x1b[33;1m";
            /// Gets or sets the style for verbose messages.
            public string Verbose
                get => _verbose;
                set => _verbose = ValidateNoContent(value);
            private string _verbose = "\x1b[33;1m";
            /// Gets or sets the style for debug messages.
            public string Debug
                get => _debug;
                set => _debug = ValidateNoContent(value);
            private string _debug = "\x1b[33;1m";
            /// Gets or sets the style for rendering feedback provider names.
            public string FeedbackName
                get => _feedbackName;
                set => _feedbackName = ValidateNoContent(value);
            // Yellow by default.
            private string _feedbackName = "\x1b[33m";
            /// Gets or sets the style for rendering feedback message.
            public string FeedbackText
                get => _feedbackText;
                set => _feedbackText = ValidateNoContent(value);
            // BrightCyan by default.
            private string _feedbackText = "\x1b[96m";
            /// Gets or sets the style for rendering feedback actions.
            public string FeedbackAction
                get => _feedbackAction;
                set => _feedbackAction = ValidateNoContent(value);
            // BrightWhite by default.
            private string _feedbackAction = "\x1b[97m";
        /// Contains formatting styles for FileInfo objects.
        public sealed class FileInfoFormatting
            /// Gets or sets the style for directories.
            public string Directory
                get => _directory;
                set => _directory = ValidateNoContent(value);
            private string _directory = "\x1b[44;1m";
            /// Gets or sets the style for symbolic links.
            public string SymbolicLink
                get => _symbolicLink;
                set => _symbolicLink = ValidateNoContent(value);
            private string _symbolicLink = "\x1b[36;1m";
            /// Gets or sets the style for executables.
            public string Executable
                get => _executable;
                set => _executable = ValidateNoContent(value);
            private string _executable = "\x1b[32;1m";
            /// Custom dictionary handling validation of extension and content.
            public sealed class FileExtensionDictionary
                private static string ValidateExtension(string extension)
                    if (!extension.StartsWith('.'))
                        throw new ArgumentException(PSStyleStrings.ExtensionNotStartingWithPeriod);
                    return extension;
                private readonly Dictionary<string, string> _extensionDictionary = new(StringComparer.OrdinalIgnoreCase);
                /// Add new extension and decoration to dictionary.
                /// <param name="extension">Extension to add.</param>
                /// <param name="decoration">ANSI string value to add.</param>
                public void Add(string extension, string decoration)
                    _extensionDictionary.Add(ValidateExtension(extension), ValidateNoContent(decoration));
                /// Add new extension and decoration to dictionary without validation.
                internal void AddWithoutValidation(string extension, string decoration)
                    _extensionDictionary.Add(extension, decoration);
                /// Remove an extension from dictionary.
                /// <param name="extension">Extension to remove.</param>
                public void Remove(string extension)
                    _extensionDictionary.Remove(ValidateExtension(extension));
                /// Clear the dictionary.
                    _extensionDictionary.Clear();
                /// Gets or sets the decoration by specified extension.
                /// <param name="extension">Extension to get decoration for.</param>
                /// <returns>The decoration for specified extension.</returns>
                public string this[string extension]
                        return _extensionDictionary[ValidateExtension(extension)];
                        _extensionDictionary[ValidateExtension(extension)] = ValidateNoContent(value);
                /// Gets whether the dictionary contains the specified extension.
                /// <param name="extension">Extension to check for.</param>
                /// <returns>True if the dictionary contains the specified extension, otherwise false.</returns>
                public bool ContainsKey(string extension)
                    if (string.IsNullOrEmpty(extension))
                    return _extensionDictionary.ContainsKey(ValidateExtension(extension));
                /// Gets the extensions for the dictionary.
                /// <returns>The extensions for the dictionary.</returns>
                public IEnumerable<string> Keys
                        return _extensionDictionary.Keys;
            /// Gets the style for archive.
            public FileExtensionDictionary Extension { get; }
            /// Initializes a new instance of the <see cref="FileInfoFormatting"/> class.
            public FileInfoFormatting()
                Extension = new FileExtensionDictionary();
                // archives
                Extension.AddWithoutValidation(".zip", "\x1b[31;1m");
                Extension.AddWithoutValidation(".tgz", "\x1b[31;1m");
                Extension.AddWithoutValidation(".gz", "\x1b[31;1m");
                Extension.AddWithoutValidation(".tar", "\x1b[31;1m");
                Extension.AddWithoutValidation(".nupkg", "\x1b[31;1m");
                Extension.AddWithoutValidation(".cab", "\x1b[31;1m");
                Extension.AddWithoutValidation(".7z", "\x1b[31;1m");
                // powershell
                Extension.AddWithoutValidation(".ps1", "\x1b[33;1m");
                Extension.AddWithoutValidation(".psd1", "\x1b[33;1m");
                Extension.AddWithoutValidation(".psm1", "\x1b[33;1m");
                Extension.AddWithoutValidation(".ps1xml", "\x1b[33;1m");
        /// Gets or sets the rendering mode for output.
        public OutputRendering OutputRendering { get; set; } = OutputRendering.Host;
        /// Gets value to turn off all attributes.
        public string Reset { get; } = "\x1b[0m";
        /// Gets value to turn off blink.
        public string BlinkOff { get; } = "\x1b[25m";
        /// Gets value to turn on blink.
        public string Blink { get; } = "\x1b[5m";
        /// Gets value to turn off bold.
        public string BoldOff { get; } = "\x1b[22m";
        public string Bold { get; } = "\x1b[1m";
        /// Gets value to turn off dim.
        public string DimOff { get; } = "\x1b[22m";
        /// Gets value to turn on dim.
        public string Dim { get; } = "\x1b[2m";
        /// Gets value to turn on hidden.
        public string Hidden { get; } = "\x1b[8m";
        /// Gets value to turn off hidden.
        public string HiddenOff { get; } = "\x1b[28m";
        /// Gets value to turn on reverse.
        public string Reverse { get; } = "\x1b[7m";
        /// Gets value to turn off reverse.
        public string ReverseOff { get; } = "\x1b[27m";
        /// Gets value to turn off standout.
        public string ItalicOff { get; } = "\x1b[23m";
        /// Gets value to turn on standout.
        public string Italic { get; } = "\x1b[3m";
        /// Gets value to turn off underlined.
        public string UnderlineOff { get; } = "\x1b[24m";
        /// Gets value to turn on underlined.
        public string Underline { get; } = "\x1b[4m";
        /// Gets value to turn off strikethrough.
        public string StrikethroughOff { get; } = "\x1b[29m";
        /// Gets value to turn on strikethrough.
        public string Strikethrough { get; } = "\x1b[9m";
        /// Gets ANSI representation of a hyperlink.
        /// <param name="text">Text describing the link.</param>
        /// <param name="link">A valid hyperlink.</param>
        /// <returns>String representing ANSI code for the hyperlink.</returns>
        public string FormatHyperlink(string text, Uri link)
            return $"\x1b]8;;{link}\x1b\\{text}\x1b]8;;\x1b\\";
        /// Gets the formatting rendering settings.
        public FormattingData Formatting { get; }
        /// Gets the configuration for progress rendering.
        public ProgressConfiguration Progress { get; }
        /// Gets foreground colors.
        public ForegroundColor Foreground { get; }
        /// Gets background colors.
        public BackgroundColor Background { get; }
        /// Gets FileInfo colors.
        public FileInfoFormatting FileInfo { get; }
        private static readonly PSStyle s_psstyle = new PSStyle();
        private PSStyle()
            Formatting = new FormattingData();
            Progress   = new ProgressConfiguration();
            Foreground = new ForegroundColor();
            Background = new BackgroundColor();
            FileInfo = new FileInfoFormatting();
        private static string ValidateNoContent(string text)
            ArgumentNullException.ThrowIfNull(text);
            var decorartedString = new ValueStringDecorated(text);
            if (decorartedString.ContentLength > 0)
                throw new ArgumentException(string.Format(PSStyleStrings.TextContainsContent, decorartedString.ToString(OutputRendering.PlainText)));
            return text;
        /// Gets singleton instance.
        public static PSStyle Instance
                return s_psstyle;
        /// The map of background console colors to escape sequences.
        private static readonly string[] BackgroundColorMap =
                "\x1b[40m", // Black
                "\x1b[44m", // DarkBlue
                "\x1b[42m", // DarkGreen
                "\x1b[46m", // DarkCyan
                "\x1b[41m", // DarkRed
                "\x1b[45m", // DarkMagenta
                "\x1b[43m", // DarkYellow
                "\x1b[47m", // Gray
                "\x1b[100m", // DarkGray
                "\x1b[104m", // Blue
                "\x1b[102m", // Green
                "\x1b[106m", // Cyan
                "\x1b[101m", // Red
                "\x1b[105m", // Magenta
                "\x1b[103m", // Yellow
                "\x1b[107m", // White
        /// The map of foreground console colors to escape sequences.
        private static readonly string[] ForegroundColorMap =
                "\x1b[30m", // Black
                "\x1b[34m", // DarkBlue
                "\x1b[32m", // DarkGreen
                "\x1b[36m", // DarkCyan
                "\x1b[31m", // DarkRed
                "\x1b[35m", // DarkMagenta
                "\x1b[33m", // DarkYellow
                "\x1b[37m", // Gray
                "\x1b[90m", // DarkGray
                "\x1b[94m", // Blue
                "\x1b[92m", // Green
                "\x1b[96m", // Cyan
                "\x1b[91m", // Red
                "\x1b[95m", // Magenta
                "\x1b[93m", // Yellow
                "\x1b[97m", // White
        /// Return the VT escape sequence for a ConsoleColor.
        /// <param name="color">The <see cref="ConsoleColor"/> to be mapped from.</param>
        /// <param name="isBackground">Whether or not it's a background color.</param>
        /// <returns>The VT escape sequence representing the color.</returns>
        internal static string MapColorToEscapeSequence(ConsoleColor color, bool isBackground)
            int index = (int)color;
            if (index < 0 || index >= ForegroundColorMap.Length)
                throw new ArgumentOutOfRangeException(paramName: nameof(color));
            return (isBackground ? BackgroundColorMap : ForegroundColorMap)[index];
        /// <param name="foregroundColor">The foreground color to be mapped from.</param>
        public static string MapForegroundColorToEscapeSequence(ConsoleColor foregroundColor)
            => MapColorToEscapeSequence(foregroundColor, isBackground: false);
        /// <param name="backgroundColor">The background color to be mapped from.</param>
        public static string MapBackgroundColorToEscapeSequence(ConsoleColor backgroundColor)
            => MapColorToEscapeSequence(backgroundColor, isBackground: true);
        /// Return the VT escape sequence for a pair of foreground and background colors.
        /// <param name="foregroundColor">The foreground color of the color pair.</param>
        /// <param name="backgroundColor">The background color of the color pair.</param>
        /// <returns>The VT escape sequence representing the foreground and background color pair.</returns>
        public static string MapColorPairToEscapeSequence(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
            int foreIndex = (int)foregroundColor;
            int backIndex = (int)backgroundColor;
            if (foreIndex < 0 || foreIndex >= ForegroundColorMap.Length)
                throw new ArgumentOutOfRangeException(paramName: nameof(foregroundColor));
            if (backIndex < 0 || backIndex >= ForegroundColorMap.Length)
                throw new ArgumentOutOfRangeException(paramName: nameof(backgroundColor));
            string foreground = ForegroundColorMap[foreIndex];
            string background = BackgroundColorMap[backIndex];
            return string.Concat(
                foreground.AsSpan(start: 0, length: foreground.Length - 1),
                ";".AsSpan(),
                background.AsSpan(start: 2));
    #endregion PSStyle
