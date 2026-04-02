// Implementation notes: In the functions that take ConsoleHandle parameters, we only assert that the handle is valid and not
// closed, as opposed to doing a check and throwing an exception.  This is because the win32 APIs that those functions wrap will
// fail on invalid/closed handles, and the check for API failure will throw the exception.
// On the use of DangerousGetHandle: If the handle has been invalidated, then the API we pass it to will return an error.  These
// handles should not be exposed to recycling attacks (because they are not exposed at all), but if they were, the worse they
// could do is diddle with the console buffer.
using ConsoleHandle = Microsoft.Win32.SafeHandles.SafeFileHandle;
using WORD = System.UInt16;
using ULONG = System.UInt32;
using HWND = System.IntPtr;
using HDC = System.IntPtr;
    /// Class ConsoleControl is used to wrap the various win32 console APIs 1:1 (i.e. at a low level, without attempting to be a
    /// "true" object-oriented library.
    internal static class ConsoleControl
        #region structs
        internal enum InputRecordEventTypes : ushort
            // from wincon.h.  These look like bit flags, but of course they could not really be used that way, since it would
            // not make sense to have more than one of the INPUT_RECORD union members "in effect" at any one time.
            KEY_EVENT = 0x0001,
            MOUSE_EVENT = 0x0002,
            WINDOW_BUFFER_SIZE_EVENT = 0x0004,
            MENU_EVENT = 0x0008,
            FOCUS_EVENT = 0x0010
            internal WORD EventType;
        internal enum ControlKeyStates : uint
            // From wincon.h.
            RIGHT_ALT_PRESSED = 0x0001, // the right alt key is pressed.
            LEFT_ALT_PRESSED = 0x0002, // the left alt key is pressed.
            RIGHT_CTRL_PRESSED = 0x0004, // the right ctrl key is pressed.
            LEFT_CTRL_PRESSED = 0x0008, // the left ctrl key is pressed.
            SHIFT_PRESSED = 0x0010, // the shift key is pressed.
            NUMLOCK_ON = 0x0020, // the numlock light is on.
            SCROLLLOCK_ON = 0x0040, // the scrolllock light is on.
            CAPSLOCK_ON = 0x0080, // the capslock light is on.
            ENHANCED_KEY = 0x0100  // the key is enhanced.
        // LayoutKind must be Explicit
            internal bool KeyDown;
            internal WORD RepeatCount;
            internal WORD VirtualKeyCode;
            internal WORD VirtualScanCode;
            internal DWORD ControlKeyState;
        internal struct COORD
            internal short X;
            internal short Y;
                return string.Create(CultureInfo.InvariantCulture, $"{X},{Y}");
        internal struct CONSOLE_READCONSOLE_CONTROL
            // from public/internal/windows/inc/winconp.h
            internal ULONG nLength;
            internal ULONG nInitialChars;
            internal ULONG dwCtrlWakeupMask;
            internal /* out */ ULONG dwControlKeyState;
        internal struct CONSOLE_FONT_INFO_EX
            internal int cbSize;
            internal int nFont;
            internal short FontWidth;
            internal short FontHeight;
            internal int FontFamily;
            internal int FontWeight;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            internal string FontFace;
        internal struct CHAR_INFO
            internal ushort UnicodeChar;
            internal WORD Attributes;
        internal struct SMALL_RECT
            internal short Left;
            internal short Top;
            internal short Right;
            internal short Bottom;
                return string.Create(CultureInfo.InvariantCulture, $"{Left},{Top},{Right},{Bottom}");
        internal struct CONSOLE_SCREEN_BUFFER_INFO
            internal COORD BufferSize;
            internal COORD CursorPosition;
            internal SMALL_RECT WindowRect;
            internal COORD MaxWindowSize;
            // NTRAID#Windows Out Of Band Releases-938428-2006/07/17-jwh
            // Bring the total size of the struct to 24 bytes.
            internal DWORD Padding;
        internal struct CONSOLE_CURSOR_INFO
            internal DWORD Size;
            internal bool Visible;
                return string.Create(CultureInfo.InvariantCulture, $"Size: {Size}, Visible: {Visible}");
        internal struct FONTSIGNATURE
            // From public\sdk\inc\wingdi.h
            // fsUsb*: A 128-bit Unicode subset bitfield (USB) identifying up to 126 Unicode subranges
            internal DWORD fsUsb0;
            internal DWORD fsUsb1;
            internal DWORD fsUsb2;
            internal DWORD fsUsb3;
            // fsCsb*: A 64-bit, code-page bitfield (CPB) that identifies a specific character set or code page.
            internal DWORD fsCsb0;
            internal DWORD fsCsb1;
        #region SentInput Data Structures
        internal struct INPUT
            internal DWORD Type;
            internal MouseKeyboardHardwareInput Data;
        internal struct MouseKeyboardHardwareInput
            internal MouseInput Mouse;
            internal KeyboardInput Keyboard;
            internal HardwareInput Hardware;
        internal struct MouseInput
            /// The absolute position of the mouse, or the amount of motion since the last mouse event was generated, depending on the value of the dwFlags member.
            /// Absolute data is specified as the x coordinate of the mouse; relative data is specified as the number of pixels moved.
            internal int X;
            /// Absolute data is specified as the y coordinate of the mouse; relative data is specified as the number of pixels moved.
            internal int Y;
            /// If dwFlags contains MOUSEEVENTF_WHEEL, then mouseData specifies the amount of wheel movement. A positive value indicates that the wheel was rotated forward, away from the user;
            /// a negative value indicates that the wheel was rotated backward, toward the user. One wheel click is defined as WHEEL_DELTA, which is 120.
            internal DWORD MouseData;
            /// A set of bit flags that specify various aspects of mouse motion and button clicks.
            /// See (https://msdn.microsoft.com/library/ms646273(VS.85).aspx)
            internal DWORD Flags;
            /// The time stamp for the event, in milliseconds. If this parameter is 0, the system will provide its own time stamp.
            internal DWORD Time;
            /// An additional value associated with the mouse event. An application calls GetMessageExtraInfo to obtain this extra information.
            internal IntPtr ExtraInfo;
        internal struct KeyboardInput
            /// A virtual-key code. The code must be a value in the range 1 to 254.
            /// If the dwFlags member specifies KEYEVENTF_UNICODE, wVk must be 0.
            internal WORD Vk;
            /// A hardware scan code for the key. If dwFlags specifies KEYEVENTF_UNICODE,
            /// wScan specifies a Unicode character which is to be sent to the foreground application.
            internal WORD Scan;
            /// Specifies various aspects of a keystroke.
            /// This member can be certain combinations of the following values.
            /// The time stamp for the event, in milliseconds.
            /// If this parameter is zero, the system will provide its own time stamp.
            /// An additional value associated with the keystroke.
            /// Use the GetMessageExtraInfo function to obtain this information.
        internal struct HardwareInput
            /// The message generated by the input hardware.
            internal DWORD Msg;
            /// The low-order word of the lParam parameter for uMsg.
            internal WORD ParamL;
            /// The high-order word of the lParam parameter for uMsg.
            internal WORD ParamH;
        internal enum VirtualKeyCode : ushort
            /// LEFT ARROW key.
            Left = 0x25,
            /// ENTER key.
            Return = 0x0D,
        /// Specify the type of the input.
        internal enum InputType : uint
            /// INPUT_MOUSE = 0x00.
            Mouse = 0,
            /// INPUT_KEYBOARD = 0x01.
            Keyboard = 1,
            /// INPUT_HARDWARE = 0x02.
            Hardware = 2,
        internal enum KeyboardFlag : uint
            /// If specified, the scan code was preceded by a prefix byte that has the value 0xE0 (224).
            ExtendedKey = 0x0001,
            /// If specified, the key is being released. If not specified, the key is being pressed.
            KeyUp = 0x0002,
            /// If specified, wScan identifies the key and wVk is ignored.
            Unicode = 0x0004,
            /// If specified, the system synthesizes a VK_PACKET keystroke. The wVk parameter must be zero.
            /// This flag can only be combined with the KEYEVENTF_KEYUP flag.
            ScanCode = 0x0008
        #endregion SentInput Data Structures
        #endregion structs
        #region Window Visibility
        [DllImport(PinvokeDllNames.GetConsoleWindowDllName)]
        internal static extern IntPtr GetConsoleWindow();
        internal const int SW_HIDE = 0;
        internal const int SW_SHOWNORMAL = 1;
        internal const int SW_NORMAL = 1;
        internal const int SW_SHOWMINIMIZED = 2;
        internal const int SW_SHOWMAXIMIZED = 3;
        internal const int SW_MAXIMIZE = 3;
        internal const int SW_SHOWNOACTIVATE = 4;
        internal const int SW_SHOW = 5;
        internal const int SW_MINIMIZE = 6;
        internal const int SW_SHOWMINNOACTIVE = 7;
        internal const int SW_SHOWNA = 8;
        internal const int SW_RESTORE = 9;
        internal const int SW_SHOWDEFAULT = 10;
        internal const int SW_FORCEMINIMIZE = 11;
        internal const int SW_MAX = 11;
        /// Code to control the display properties of the a window...
        /// <param name="hWnd">The window to show...</param>
        /// <param name="nCmdShow">The command to do.</param>
        /// <returns>True if it was successful.</returns>
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        internal static void SetConsoleMode(ProcessWindowStyle style)
            IntPtr hwnd = GetConsoleWindow();
            Dbg.Assert(hwnd != IntPtr.Zero, "Console handle should never be zero");
            switch (style)
                    ShowWindow(hwnd, SW_HIDE);
                    ShowWindow(hwnd, SW_MAXIMIZE);
                    ShowWindow(hwnd, SW_MINIMIZE);
                    ShowWindow(hwnd, SW_NORMAL);
        #region Input break handler (Ctrl-C, Ctrl-Break)
        /// Types of control ConsoleBreakSignals received by break Win32Handler delegates.
        internal enum ConsoleBreakSignal : uint
            // These correspond to the CRTL_XXX_EVENT #defines in public/sdk/inc/wincon.h
            CtrlC = 0,
            CtrlBreak = 1,
            Close = 2,
            Logoff = 5,
            // This only gets received by services
            Shutdown = 6,
            // None is not really a signal -- it's used to indicate that no signal exists.
            None = 0xFF
        // NOTE: this delegate will be executed in its own thread
        internal delegate bool BreakHandler(ConsoleBreakSignal ConsoleBreakSignal);
        /// Set the console's break handler.
        /// <param name="handlerDelegate"></param>
        /// <exception cref="HostException">
        /// If Win32's SetConsoleCtrlHandler fails
        internal static void AddBreakHandler(BreakHandler handlerDelegate)
            bool result = NativeMethods.SetConsoleCtrlHandler(handlerDelegate, true);
                int err = Marshal.GetLastWin32Error();
                HostException e = CreateHostException(err, "AddBreakHandler",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.AddBreakHandlerExceptionMessage);
                throw e;
        /// Set the console's break handler to null.
        internal static void RemoveBreakHandler()
            bool result = NativeMethods.SetConsoleCtrlHandler(null, false);
                HostException e = CreateHostException(err, "RemoveBreakHandler",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.RemoveBreakHandlerExceptionTemplate);
        #region Win32Handles
        private static readonly Lazy<ConsoleHandle> _keyboardInputHandle = new Lazy<SafeFileHandle>(() =>
                var handle = NativeMethods.CreateFile(
                    "CONIN$",
                    (UInt32)(NativeMethods.AccessQualifiers.GenericRead | NativeMethods.AccessQualifiers.GenericWrite),
                    (UInt32)NativeMethods.ShareModes.ShareRead,
                    (IntPtr)0,
                    (UInt32)NativeMethods.CreationDisposition.OpenExisting,
                    (IntPtr)0);
                if (handle == NativeMethods.INVALID_HANDLE_VALUE)
                    HostException e = CreateHostException(err, "RetreiveInputConsoleHandle",
                                                            ConsoleControlStrings.GetInputModeExceptionTemplate);
                return new ConsoleHandle(handle, true);
        /// Returns a ConsoleHandle to the console (keyboard device)
        internal static ConsoleHandle GetConioDeviceHandle()
            return _keyboardInputHandle.Value;
        private static readonly Lazy<ConsoleHandle> _outputHandle = new Lazy<SafeFileHandle>(() =>
                // We use CreateFile here instead of GetStdWin32Handle, as GetStdWin32Handle will return redirected handles
                    "CONOUT$",
                    (UInt32)NativeMethods.ShareModes.ShareWrite,
                    HostException e = CreateHostException(err, "RetreiveActiveScreenBufferConsoleHandle",
                        ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetActiveScreenBufferHandleExceptionTemplate);
        /// Returns a ConsoleHandle to the active screen buffer, even if that output has been redirected.
        /// If Win32's CreateFile fails
        internal static ConsoleHandle GetActiveScreenBufferHandle()
            return _outputHandle.Value;
        #region Mode
        /// Flags used by ConsoleControl.GetMode and ConsoleControl.SetMode.
        internal enum ConsoleModes : uint
            // These values from wincon.h
            // input modes
            ProcessedInput = 0x001,
            LineInput = 0x002,
            EchoInput = 0x004,
            WindowInput = 0x008,
            MouseInput = 0x010,
            Insert = 0x020,
            QuickEdit = 0x040,
            Extended = 0x080,
            AutoPosition = 0x100,
            // output modes
            ProcessedOutput = 0x001,  // yes, I know they are the same values as some flags defined above.
            WrapEndOfLine = 0x002,
            VirtualTerminal = 0x004,
            // Error getting console mode
            Unknown = 0xffffffff,
        /// Returns a mask of ConsoleModes flags describing the current modality of the console.
        /// If Win32's GetConsoleMode fails
        internal static ConsoleModes GetMode(ConsoleHandle consoleHandle)
            Dbg.Assert(!consoleHandle.IsInvalid, "consoleHandle is not valid");
            Dbg.Assert(!consoleHandle.IsClosed, "ConsoleHandle is closed");
            UInt32 m = 0;
            bool result = NativeMethods.GetConsoleMode(consoleHandle.DangerousGetHandle(), out m);
                HostException e = CreateHostException(err, "GetConsoleMode",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetModeExceptionTemplate);
            return (ConsoleModes)m;
        /// Sets the current mode of the console device.
        /// <param name="consoleHandle">
        /// Handle to the console device returned by GetInputHandle
        /// <param name="mode">
        /// Mask of mode flags
        /// If Win32's SetConsoleMode fails
        internal static void SetMode(ConsoleHandle consoleHandle, ConsoleModes mode)
            bool result = NativeMethods.SetConsoleMode(consoleHandle.DangerousGetHandle(), (DWORD)mode);
                HostException e = CreateHostException(err, "SetConsoleMode",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetModeExceptionTemplate);
        /// Reads input from the console device according to the mode in effect (see GetMode, SetMode)
        /// <param name="consoleHandle"></param>
        /// <param name="initialContentLength">
        /// Length of initial content of the edit buffer. Zero if no initial content exists.
        /// Must be less than editBuffer length.
        /// <param name="editBuffer">
        /// Edit buffer with optional initial content.
        /// Caution! Last position in the edit buffer is for a null in native code.
        /// <param name="charactersToRead">
        /// Number of characters to read from the device.
        /// <param name="endOnTab">
        /// True to allow the user to terminate input by hitting the tab or shift-tab key, in addition to the enter key
        /// <param name="keyState">
        /// Bit mask indicating the state of the control/shift keys at the point input was terminated.
        /// If Win32's ReadConsole fails
        internal static string ReadConsole(
            ConsoleHandle consoleHandle,
            int initialContentLength,
            Span<char> editBuffer,
            int charactersToRead,
            bool endOnTab,
            out uint keyState)
            Dbg.Assert(!consoleHandle.IsInvalid, "ConsoleHandle is not valid");
            Dbg.Assert(initialContentLength < editBuffer.Length, "initialContentLength must be less than editBuffer.Length");
            Dbg.Assert(charactersToRead < editBuffer.Length, "charactersToRead must be less than editBuffer.Length");
            keyState = 0;
            CONSOLE_READCONSOLE_CONTROL control = new CONSOLE_READCONSOLE_CONTROL();
            control.nLength = (ULONG)Marshal.SizeOf(control);
            control.nInitialChars = (ULONG)initialContentLength;
            control.dwControlKeyState = 0;
            if (endOnTab)
                const int TAB = 0x9;
                control.dwCtrlWakeupMask = (1 << TAB);
            DWORD charsReaded = 0;
            bool result =
                NativeMethods.ReadConsole(
                    consoleHandle.DangerousGetHandle(),
                    editBuffer,
                    (DWORD)charactersToRead,
                    out charsReaded,
                    ref control);
            keyState = control.dwControlKeyState;
                HostException e = CreateHostException(
                    err,
                    "ReadConsole",
                    ConsoleControlStrings.ReadConsoleExceptionTemplate);
            if (charsReaded > (uint)charactersToRead)
                charsReaded = (uint)charactersToRead;
            return editBuffer.Slice(0, (int)charsReaded).ToString();
        /// Wraps Win32 ReadConsoleInput.
        /// Returns the number of records read in buffer.
        /// handle for the console where input is read
        /// <param name="buffer">
        /// array where data read are stored
        /// actual number of input records read
        /// If Win32's ReadConsoleInput fails
        internal static int ReadConsoleInput(ConsoleHandle consoleHandle, ref INPUT_RECORD[] buffer)
            DWORD recordsRead = 0;
                NativeMethods.ReadConsoleInput(
                    buffer,
                    (DWORD)buffer.Length,
                    out recordsRead);
                HostException e = CreateHostException(err, "ReadConsoleInput",
                    ErrorCategory.ReadError, ConsoleControlStrings.ReadConsoleInputExceptionTemplate);
            return (int)recordsRead;
        /// Wraps Win32 PeekConsoleInput.
        /// handle for the console where input is peeked
        /// actual number of input records peeked
        /// If Win32's PeekConsoleInput fails
        internal static int PeekConsoleInput
            ref INPUT_RECORD[] buffer
            DWORD recordsRead;
                NativeMethods.PeekConsoleInput(
                HostException e = CreateHostException(err, "PeekConsoleInput",
                    ErrorCategory.ReadError, ConsoleControlStrings.PeekConsoleInputExceptionTemplate);
        /// Wraps Win32 GetNumberOfConsoleInputEvents.
        /// handle for the console where the number of console input events is obtained
        /// number of console input events
        /// If Win32's GetNumberOfConsoleInputEvents fails
        internal static int GetNumberOfConsoleInputEvents(ConsoleHandle consoleHandle)
            DWORD numEvents;
            bool result = NativeMethods.GetNumberOfConsoleInputEvents(consoleHandle.DangerousGetHandle(), out numEvents);
                HostException e = CreateHostException(err, "GetNumberOfConsoleInputEvents",
                    ErrorCategory.ReadError, ConsoleControlStrings.GetNumberOfConsoleInputEventsExceptionTemplate);
            return (int)numEvents;
        /// Wraps Win32 FlushConsoleInputBuffer.
        /// handle for the console where the input buffer is flushed
        /// If Win32's FlushConsoleInputBuffer fails
        internal static void FlushConsoleInputBuffer(ConsoleHandle consoleHandle)
            NakedWin32Handle h = consoleHandle.DangerousGetHandle();
            result = NativeMethods.FlushConsoleInputBuffer(h);
                HostException e = CreateHostException(err, "FlushConsoleInputBuffer",
                    ErrorCategory.ReadError, ConsoleControlStrings.FlushConsoleInputBufferExceptionTemplate);
        #region Buffer
        /// Wraps Win32 GetConsoleScreenBufferInfo
        /// Returns Console Screen Buffer Info.
        /// Handle for the console where the screen buffer info is obtained
        /// info about the screen buffer. See the definition of CONSOLE_SCREEN_BUFFER_INFO
        /// If Win32's GetConsoleScreenBufferInfo fails
        internal static CONSOLE_SCREEN_BUFFER_INFO GetConsoleScreenBufferInfo(ConsoleHandle consoleHandle)
            CONSOLE_SCREEN_BUFFER_INFO bufferInfo;
            bool result = NativeMethods.GetConsoleScreenBufferInfo(consoleHandle.DangerousGetHandle(), out bufferInfo);
                HostException e = CreateHostException(err, "GetConsoleScreenBufferInfo",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetConsoleScreenBufferInfoExceptionTemplate);
            return bufferInfo;
        /// Set the output buffer's size.
        /// <param name="newSize"></param>
        /// If Win32's SetConsoleScreenBufferSize fails
        internal static void SetConsoleScreenBufferSize(ConsoleHandle consoleHandle, Size newSize)
            COORD s;
            s.X = (short)newSize.Width;
            s.Y = (short)newSize.Height;
            bool result = NativeMethods.SetConsoleScreenBufferSize(consoleHandle.DangerousGetHandle(), s);
                HostException e = CreateHostException(err, "SetConsoleScreenBufferSize",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleScreenBufferSizeExceptionTemplate);
        internal static bool IsConsoleColor(ConsoleColor c)
                case ConsoleColor.Black:
                case ConsoleColor.Blue:
                case ConsoleColor.Cyan:
                case ConsoleColor.DarkBlue:
                case ConsoleColor.DarkCyan:
                case ConsoleColor.DarkGray:
                case ConsoleColor.DarkGreen:
                case ConsoleColor.DarkMagenta:
                case ConsoleColor.DarkRed:
                case ConsoleColor.DarkYellow:
                case ConsoleColor.Gray:
                case ConsoleColor.Green:
                case ConsoleColor.Magenta:
                case ConsoleColor.Red:
                case ConsoleColor.White:
                case ConsoleColor.Yellow:
        internal static void WORDToColor(WORD attribute, out ConsoleColor foreground, out ConsoleColor background)
            // foreground color is the low-byte in the word, background color is the hi-byte.
            foreground = (ConsoleColor)(attribute & 0x0f);
            background = (ConsoleColor)((attribute & 0xf0) >> 4);
            Dbg.Assert(IsConsoleColor(foreground), "unknown color");
            Dbg.Assert(IsConsoleColor(background), "unknown color");
        internal static WORD ColorToWORD(ConsoleColor foreground, ConsoleColor background)
            WORD result = (WORD)(((int)background << 4) | (int)foreground);
        /// Wrap32 WriteConsoleOutput.
        /// This wrapper is not limited to 64K or 8K CHAR_INFO to which Win32's WriteConsoleOutput
        /// is constrained.
        /// handle for the console where output is written
        /// <param name="origin">
        /// location on screen buffer where writing starts
        /// <param name="contents">
        /// 2D array of cells. Caller needs to ensure that the array is 2D.
        /// If there is not enough memory to complete calls to Win32's WriteConsoleOutput
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="contents"/> is null
        /// If it is illegal to write <paramref name="contents"/> to the output buffer
        internal static void WriteConsoleOutput(ConsoleHandle consoleHandle, Coordinates origin, BufferCell[,] contents)
            if (contents == null)
                throw PSTraceSource.NewArgumentNullException(nameof(contents));
            uint codePage;
            if (IsCJKOutputCodePage(out codePage))
                // contentsRegion indicates the area in contents (declared below) in which
                // the data read from ReadConsoleOutput is stored.
                Rectangle contentsRegion = new Rectangle();
                ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO bufferInfo =
                    GetConsoleScreenBufferInfo(consoleHandle);
                int bufferWidth = bufferInfo.BufferSize.X;
                int bufferHeight = bufferInfo.BufferSize.Y;
                Rectangle screenRegion = new Rectangle(
                    origin.X, origin.Y,
                    Math.Min(origin.X + contents.GetLength(1) - 1, bufferWidth - 1),
                    Math.Min(origin.Y + contents.GetLength(0) - 1, bufferHeight - 1));
                contentsRegion.Left = contents.GetLowerBound(1);
                contentsRegion.Top = contents.GetLowerBound(0);
                contentsRegion.Right = contentsRegion.Left +
                    screenRegion.Right - screenRegion.Left;
                contentsRegion.Bottom = contentsRegion.Top +
                    screenRegion.Bottom - screenRegion.Top;
                // Check contents in contentsRegion
                CheckWriteConsoleOutputContents(contents, contentsRegion);
                // Identify edges and areas of identical contiguous edges in contentsRegion
                List<BufferCellArrayRowTypeRange> sameEdgeAreas = new List<BufferCellArrayRowTypeRange>();
                int firstLeftTrailingRow = -1, firstRightLeadingRow = -1;
                BuildEdgeTypeInfo(contentsRegion, contents,
                    sameEdgeAreas, out firstLeftTrailingRow, out firstRightLeadingRow);
                CheckWriteEdges(consoleHandle, codePage, origin, contents, contentsRegion,
                    bufferInfo, firstLeftTrailingRow, firstRightLeadingRow);
                foreach (BufferCellArrayRowTypeRange area in sameEdgeAreas)
                    Coordinates o = new Coordinates(origin.X,
                                                    origin.Y + area.Start - contentsRegion.Top);
                    Rectangle contRegion = new Rectangle(
                        contentsRegion.Left, area.Start, contentsRegion.Right, area.End);
                    if ((area.Type & BufferCellArrayRowType.LeftTrailing) != 0)
                        contRegion.Left++;
                        o.X++;
                        if (o.X >= bufferWidth || contRegion.Right < contRegion.Left)
                    WriteConsoleOutputCJK(consoleHandle, o, contRegion, contents, area.Type);
                WriteConsoleOutputPlain(consoleHandle, origin, contents);
        private static void BuildEdgeTypeInfo(
            Rectangle contentsRegion,
            BufferCell[,] contents,
            List<BufferCellArrayRowTypeRange> sameEdgeAreas,
            out int firstLeftTrailingRow,
            out int firstRightLeadingRow)
            firstLeftTrailingRow = -1;
            firstRightLeadingRow = -1;
            BufferCellArrayRowType edgeType =
                GetEdgeType(contents[contentsRegion.Top, contentsRegion.Left],
                    contents[contentsRegion.Top, contentsRegion.Right]);
            for (int r = contentsRegion.Top; r <= contentsRegion.Bottom;)
                BufferCellArrayRowTypeRange range;
                range.Start = r;
                range.Type = edgeType;
                if (firstLeftTrailingRow == -1 && ((range.Type & BufferCellArrayRowType.LeftTrailing) != 0))
                    firstLeftTrailingRow = r;
                if (firstRightLeadingRow == -1 && ((range.Type & BufferCellArrayRowType.RightLeading) != 0))
                    firstRightLeadingRow = r;
                    r++;
                    if (r > contentsRegion.Bottom)
                        range.End = r - 1;
                        sameEdgeAreas.Add(range);
                    edgeType = GetEdgeType(contents[r, contentsRegion.Left], contents[r, contentsRegion.Right]);
                    if (edgeType != range.Type)
        private static BufferCellArrayRowType GetEdgeType(BufferCell left, BufferCell right)
            BufferCellArrayRowType edgeType = 0;
            if (left.BufferCellType == BufferCellType.Trailing)
                edgeType |= BufferCellArrayRowType.LeftTrailing;
            if (right.BufferCellType == BufferCellType.Leading)
                edgeType |= BufferCellArrayRowType.RightLeading;
            return edgeType;
        private struct BufferCellArrayRowTypeRange
            internal int Start;
            internal int End;
            internal BufferCellArrayRowType Type;
        private enum BufferCellArrayRowType : uint
            LeftTrailing = 0x1,
            RightLeading = 0x2
        /// Check the existing screen columns left and right of areas to be written.
        /// <param name="codePage"></param>
        /// <param name="origin">Must be within the screen buffer.</param>
        /// <param name="contents"></param>
        /// <param name="contentsRegion"></param>
        /// <param name="bufferInfo"></param>
        /// <param name="firstLeftTrailingRow"></param>
        /// <param name="firstRightLeadingRow"></param>
        /// If it is illegal to write <paramref name="contents"/> at <paramref name="origin"/>
        /// If there is not enough memory to complete calls to Win32's ReadConsoleOutput
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called in CHK builds")]
        internal static void CheckWriteEdges(
            uint codePage, Coordinates origin,
            ConsoleControl.CONSOLE_SCREEN_BUFFER_INFO bufferInfo,
            int firstLeftTrailingRow,
            int firstRightLeadingRow)
            Rectangle existingRegion = new Rectangle(0, 0, 1, contentsRegion.Bottom - contentsRegion.Top);
            if (origin.X == 0)
                if (firstLeftTrailingRow >= 0)
                    throw PSTraceSource.NewArgumentException(string.Create(CultureInfo.InvariantCulture, $"contents[{firstLeftTrailingRow}, {contentsRegion.Left}]"));
                // use ReadConsoleOutputCJK because checking the left and right edges of the existing output
                // is NOT needed
                BufferCell[,] leftExisting = new BufferCell[existingRegion.Bottom + 1, 2];
                ReadConsoleOutputCJK(consoleHandle, codePage,
                    new Coordinates(origin.X - 1, origin.Y), existingRegion, ref leftExisting);
                for (int r = contentsRegion.Top, i = 0; r <= contentsRegion.Bottom; r++, i++)
                    if (leftExisting[r, 0].BufferCellType == BufferCellType.Leading ^
                            contents[r, contentsRegion.Left].BufferCellType == BufferCellType.Trailing)
                        throw PSTraceSource.NewArgumentException(string.Create(CultureInfo.InvariantCulture, $"contents[{r}, {contentsRegion.Left}]"));
            // Check right edge
            if (origin.X + (contentsRegion.Right - contentsRegion.Left) + 1 >= bufferInfo.BufferSize.X)
                if (firstRightLeadingRow >= 0)
                    throw PSTraceSource.NewArgumentException(string.Create(CultureInfo.InvariantCulture, $"contents[{firstRightLeadingRow}, {contentsRegion.Right}]"));
                BufferCell[,] rightExisting = new BufferCell[existingRegion.Bottom + 1, 2];
                    new Coordinates(origin.X + (contentsRegion.Right - contentsRegion.Left), origin.Y), existingRegion, ref rightExisting);
                    if (rightExisting[r, 0].BufferCellType == BufferCellType.Leading ^
                            contents[r, contentsRegion.Right].BufferCellType == BufferCellType.Leading)
                        throw PSTraceSource.NewArgumentException(string.Create(CultureInfo.InvariantCulture, $"contents[{r}, {contentsRegion.Right}]"));
        private static void CheckWriteConsoleOutputContents(BufferCell[,] contents, Rectangle contentsRegion)
            for (int r = contentsRegion.Top; r <= contentsRegion.Bottom; r++)
                for (int c = contentsRegion.Left; c <= contentsRegion.Right; c++)
                    // Changes have been made in the following code such that 2 cell characters
                    // (Chinese, Japanese or Korean) can be in a single BufferCell structure
                    // which is complete
                    if (contents[r, c].BufferCellType == BufferCellType.Trailing &&
                        contents[r, c].Character != 0)
                        // trailing character is not 0
                        throw PSTraceSource.NewArgumentException(string.Create(CultureInfo.InvariantCulture, $"contents[{r}, {c}]"));
                    if (contents[r, c].BufferCellType == BufferCellType.Leading)
                        c++;
                        if (c > contentsRegion.Right)
                        if (contents[r, c].Character != 0 || contents[r, c].BufferCellType != BufferCellType.Trailing)
                            // for a 2 cell character, either there is no trailing BufferCell or
                            // the trailing BufferCell's character is not 0
        private static void WriteConsoleOutputCJK(ConsoleHandle consoleHandle, Coordinates origin, Rectangle contentsRegion, BufferCell[,] contents, BufferCellArrayRowType rowType)
            Dbg.Assert(origin.X >= 0 && origin.Y >= 0,
                "origin must be within the output buffer");
            int rows = contentsRegion.Bottom - contentsRegion.Top + 1;
            int cols = contentsRegion.Right - contentsRegion.Left + 1;
            CONSOLE_FONT_INFO_EX fontInfo = GetConsoleFontInfo(consoleHandle);
            int fontType = fontInfo.FontFamily & NativeMethods.FontTypeMask;
            bool trueTypeInUse = (fontType & NativeMethods.TrueTypeFont) == NativeMethods.TrueTypeFont;
            int bufferLimit = 2 * 1024; // Limit is 8K bytes as each CHAR_INFO takes 4 bytes
            COORD bufferCoord;
            bufferCoord.X = 0;
            bufferCoord.Y = 0;
            // keeps track of which screen area write
            SMALL_RECT writeRegion;
            writeRegion.Top = (short)origin.Y;
            int rowsRemaining = rows;
            while (rowsRemaining > 0)
                // Iteration of columns is nested inside iteration of rows.
                // If the size of contents exceeds the buffer limit, writing is
                // done in blocks of size equal to the bufferlimit from left to right
                // then top to bottom.
                // For each iteration of rows,
                // - writeRegion.Left and bufferSize.X are reset
                // - rowsRemaining, writeRegion.Top, writeRegion.Bottom, and bufferSize.Y
                //     are updated
                //   For each iteration of columns,
                //   - writeRegion.Left, writeRegion.Right and bufferSize.X are updated
                writeRegion.Left = (short)origin.X;
                COORD bufferSize;
                bufferSize.X = (short)Math.Min(cols, bufferLimit);
                bufferSize.Y = (short)Math.Min
                                            rowsRemaining,
                                            bufferLimit / bufferSize.X
                writeRegion.Bottom = (short)(writeRegion.Top + bufferSize.Y - 1);
                // atRow is at which row of contents a particular iteration is operating
                int atRow = rows - rowsRemaining + contentsRegion.Top;
                // number of columns yet to be written
                int colsRemaining = cols;
                while (colsRemaining > 0)
                    writeRegion.Right = (short)(writeRegion.Left + bufferSize.X - 1);
                    // atCol is at which column of contents a particular iteration is operating
                    int atCol = cols - colsRemaining + contentsRegion.Left;
                    // if this is not the last column iteration &&
                    //   the leftmost BufferCell is a leading cell, don't write that cell
                    if (colsRemaining > bufferSize.X &&
                         contents[atRow, atCol + bufferSize.X - 1].BufferCellType == BufferCellType.Leading)
                        bufferSize.X--;
                        writeRegion.Right--;
                    CHAR_INFO[] characterBuffer = new CHAR_INFO[bufferSize.Y * bufferSize.X];
                    // copy characterBuffer to contents;
                    int characterBufferIndex = 0;
                    bool lastCharIsLeading = false;
                    BufferCell lastLeadingCell = new BufferCell();
                    for (int r = atRow; r < bufferSize.Y + atRow; r++)
                        for (int c = atCol; c < bufferSize.X + atCol; c++, characterBufferIndex++)
                            if (contents[r, c].BufferCellType == BufferCellType.Complete)
                                characterBuffer[characterBufferIndex].UnicodeChar =
                                    (ushort)contents[r, c].Character;
                                characterBuffer[characterBufferIndex].Attributes =
                                    (ushort)(ColorToWORD(contents[r, c].ForegroundColor, contents[r, c].BackgroundColor));
                                lastCharIsLeading = false;
                            else if (contents[r, c].BufferCellType == BufferCellType.Leading)
                                    (ushort)(ColorToWORD(contents[r, c].ForegroundColor, contents[r, c].BackgroundColor)
                                                | (ushort)NativeMethods.CHAR_INFO_Attributes.COMMON_LVB_LEADING_BYTE);
                                lastCharIsLeading = true;
                                lastLeadingCell = contents[r, c];
                            else if (contents[r, c].BufferCellType == BufferCellType.Trailing)
                                // The FontFamily is a 8-bit integer. The low-order bit (bit 0) specifies the pitch of the font.
                                // If it is 1, the font is variable pitch (or proportional). If it is 0, the font is fixed pitch
                                // (or monospace). Bits 1 and 2 specify the font type. If both bits are 0, the font is a raster font;
                                // if bit 1 is 1 and bit 2 is 0, the font is a vector font; if bit 1 is 0 and bit 2 is set, or if both
                                // bits are 1, the font is true type. Bit 3 is 1 if the font is a device font; otherwise, it is 0.
                                // We only care about the bit 1 and 2, which indicate the font type.
                                // There are only two font type defined for the Console, at
                                // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Console\.
                                //     Console\Nls           --- national language supports
                                //     Console\RasterFonts   --- raster type font
                                //     Console\TrueTypeFont  --- true type font
                                // For CJK characters, if it's TrueType, we need to output the trailing character marked with "Trailing_byte"
                                // attribute. But if it's RasterFont, we ignore the trailing character, and the "Leading_byte"/"Trailing_byte"
                                // attributes are not effective at all when reading the character from the console buffer.
                                if (lastCharIsLeading && trueTypeInUse)
                                    // For TrueType Font, we output the trailing byte with "Trailing_byte" attribute
                                    characterBuffer[characterBufferIndex].UnicodeChar = lastLeadingCell.Character;
                                                | (ushort)NativeMethods.CHAR_INFO_Attributes.COMMON_LVB_TRAILING_BYTE);
                                    // We don't output anything for this cell if Raster font is in use, or if the last cell is not a leading byte
                                    characterBufferIndex--;
                    // Now writeRegion, bufferSize and characterBuffer are updated.
                    // Call NativeMethods.WriteConsoleOutput
                    bool result;
                    if ((rowType & BufferCellArrayRowType.RightLeading) != 0 &&
                            colsRemaining == bufferSize.X)
                        COORD bSize = bufferSize;
                        bSize.X++;
                        SMALL_RECT wRegion = writeRegion;
                        wRegion.Right++;
                        result = NativeMethods.WriteConsoleOutput(
                            characterBuffer,
                            bSize,
                            bufferCoord,
                            ref wRegion);
                            ref writeRegion);
                        // When WriteConsoleOutput fails, half bufferLimit
                        if (bufferLimit < 2)
                            HostException e = CreateHostException(err, "WriteConsoleOutput",
                                ErrorCategory.WriteError, ConsoleControlStrings.WriteConsoleOutputExceptionTemplate);
                        bufferLimit /= 2;
                        if (cols == colsRemaining)
                            // if cols == colsRemaining, nothing is guaranteed written in this pass and
                            //  the unwritten area is still rectangular
                            bufferSize.Y = 0;
                            // some areas have been written. This could only happen when the number of columns
                            // to write is larger than bufferLimit. In that case, the algorithm writes one row
                            // at a time => bufferSize.Y == 1. Then, we can safely leave bufferSize.Y unchanged
                            // to retry with a smaller bufferSize.X.
                            Dbg.Assert(bufferSize.Y == 1, string.Create(CultureInfo.InvariantCulture, $"bufferSize.Y should be 1, but is {bufferSize.Y}"));
                            bufferSize.X = (short)Math.Min(colsRemaining, bufferLimit);
                    colsRemaining -= bufferSize.X;
                    writeRegion.Left += bufferSize.X;
                }  // column iteration
                rowsRemaining -= bufferSize.Y;
                writeRegion.Top += bufferSize.Y;
            }  // row iteration
        private static void WriteConsoleOutputPlain(ConsoleHandle consoleHandle, Coordinates origin, BufferCell[,] contents)
            int rows = contents.GetLength(0);
            int cols = contents.GetLength(1);
            if ((rows <= 0) || cols <= 0)
                tracer.WriteLine("contents passed in has 0 rows and columns");
                int atRow = rows - rowsRemaining + contents.GetLowerBound(0);
                    int atCol = cols - colsRemaining + contents.GetLowerBound(1);
                    for (int r = atRow, characterBufferIndex = 0;
                        r < bufferSize.Y + atRow; r++)
                                ColorToWORD(contents[r, c].ForegroundColor, contents[r, c].BackgroundColor);
                        NativeMethods.WriteConsoleOutput(
        /// Wrap32 ReadConsoleOutput
        /// This wrapper is not limited to 64K or 8K CHAR_INFO to which Win32's ReadConsoleOutput
        /// handle for the console where output is read
        /// location on screen buffer where reading begins
        /// <param name="contentsRegion">
        /// indicates the area in <paramref name="contents"/> where the data read
        /// is stored.
        /// this is ref because the bounds and size of the array are needed.
        internal static void ReadConsoleOutput
            Coordinates origin,
            ref BufferCell[,] contents
                ReadConsoleOutputCJK(consoleHandle, codePage, origin, contentsRegion, ref contents);
                // check left edge
                BufferCell[,] cellArray = null;
                Coordinates checkOrigin;
                Rectangle cellArrayRegion = new Rectangle(0, 0, 1, contentsRegion.Bottom - contentsRegion.Top);
                if (origin.X > 0 && ShouldCheck(contentsRegion.Left, contents, contentsRegion))
                    cellArray = new BufferCell[cellArrayRegion.Bottom + 1, 2];
                    checkOrigin = new Coordinates(origin.X - 1, origin.Y);
                    ReadConsoleOutputCJK(consoleHandle, codePage, checkOrigin,
                        cellArrayRegion, ref cellArray);
                    for (int i = 0; i <= cellArrayRegion.Bottom; i++)
                        if (cellArray[i, 0].BufferCellType == BufferCellType.Leading)
                            contents[contentsRegion.Top + i, 0].Character = (char)0;
                            contents[contentsRegion.Top + i, 0].BufferCellType = BufferCellType.Trailing;
                // check right edge
                if (origin.X + (contentsRegion.Right - contentsRegion.Left) + 1 < bufferInfo.BufferSize.X &&
                    ShouldCheck(contentsRegion.Right, contents, contentsRegion))
                    cellArray ??= new BufferCell[cellArrayRegion.Bottom + 1, 2];
                    checkOrigin = new Coordinates(origin.X +
                        (contentsRegion.Right - contentsRegion.Left), origin.Y);
                            contents[contentsRegion.Top + i, contentsRegion.Right] = cellArray[i, 0];
                ReadConsoleOutputPlain(consoleHandle, origin, contentsRegion, ref contents);
        #region ReadConsoleOutput CJK
        /// If an edge cell read is a blank, it is potentially part of a double width character. Hence,
        ///  at least one of the left and right edges should be checked.
        /// <param name="edge"></param>
        private static bool ShouldCheck(int edge, BufferCell[,] contents, Rectangle contentsRegion)
            for (int i = contentsRegion.Top; i <= contentsRegion.Bottom; i++)
                if (contents[i, edge].Character == ' ')
        private static bool ReadConsoleOutputCJKSmall
            uint codePage,
            bufferSize.X = (short)(contentsRegion.Right - contentsRegion.Left + 1);
            bufferSize.Y = (short)(contentsRegion.Bottom - contentsRegion.Top + 1);
            CHAR_INFO[] characterBuffer = new CHAR_INFO[bufferSize.X * bufferSize.Y];
            SMALL_RECT readRegion;
            readRegion.Left = (short)origin.X;
            readRegion.Top = (short)origin.Y;
            readRegion.Right = (short)(origin.X + bufferSize.X - 1);
            readRegion.Bottom = (short)(origin.Y + bufferSize.Y - 1);
            bool result = NativeMethods.ReadConsoleOutput(
                                        ref readRegion);
                for (int c = contentsRegion.Left; c <= contentsRegion.Right; c++, characterBufferIndex++)
                    ConsoleColor fgColor, bgColor;
                    contents[r, c].Character = (char)characterBuffer[characterBufferIndex].UnicodeChar;
                    WORDToColor(characterBuffer[characterBufferIndex].Attributes,
                                out fgColor,
                                out bgColor);
                    contents[r, c].ForegroundColor = fgColor;
                    contents[r, c].BackgroundColor = bgColor;
                    // Set the attributes of the buffercells to be the same as that of the
                    // incoming CHAR_INFO. In case where the CHAR_INFO character is a
                    // trailing byte set the Character of BufferCell to 0. This is done
                    // because at a lot of places this check is being done. Having a trailing
                    // character to be 0 is by design.
                    if ((characterBuffer[characterBufferIndex].Attributes & (ushort)NativeMethods.CHAR_INFO_Attributes.COMMON_LVB_LEADING_BYTE)
                            == (ushort)NativeMethods.CHAR_INFO_Attributes.COMMON_LVB_LEADING_BYTE)
                        contents[r, c].BufferCellType = BufferCellType.Leading;
                    else if ((characterBuffer[characterBufferIndex].Attributes & (ushort)NativeMethods.CHAR_INFO_Attributes.COMMON_LVB_TRAILING_BYTE)
                            == (ushort)NativeMethods.CHAR_INFO_Attributes.COMMON_LVB_TRAILING_BYTE)
                        contents[r, c].Character = (char)0;
                        contents[r, c].BufferCellType = BufferCellType.Trailing;
                        int charLength = LengthInBufferCells(contents[r, c].Character);
                        if (charLength == 2)
                            // When it's RasterFont, the "Leading_byte"/"Trailing_byte" are not effective, we
                            // need to decide the leading byte by checking the char length.
                            contents[r, c].BufferCellType = BufferCellType.Complete;
        /// Can handle reading CJK characters, but the left and right edges are not checked.
        /// <param name="origin"></param>
        internal static void ReadConsoleOutputCJK
                tracer.WriteLine("invalid contents region");
            // keeps track of which screen area is read
                // If the size of contents exceeds the buffer limit, reading is
                // - readRegion.Left and bufferSize.X are reset
                // - rowsRemaining, readRegion.Top, readRegion.Bottom, and bufferSize.Y
                //   - readRegion.Left, readRegion.Right and bufferSize.X are updated
                readRegion.Bottom = (short)(readRegion.Top + bufferSize.Y - 1);
                // atContentsRow is at which row of contents a particular iteration is operating
                int atContentsRow = rows - rowsRemaining + contentsRegion.Top;
                // number of columns yet to be read
                    // atContentsCol is at which column of contents a particular iteration is operating
                    int atContentsCol = cols - colsRemaining + contentsRegion.Left;
                    readRegion.Right = (short)(readRegion.Left + bufferSize.X - 1);
                    // Now readRegion and bufferSize are updated.
                    Rectangle atContents = new Rectangle(atContentsCol, atContentsRow,
                                atContentsCol + bufferSize.X - 1, atContentsRow + bufferSize.Y - 1);
                        ReadConsoleOutputCJKSmall(consoleHandle, codePage,
                            new Coordinates(readRegion.Left, readRegion.Top),
                            atContents,
                            ref contents);
                            HostException e = CreateHostException(err, "ReadConsoleOutput",
                                ErrorCategory.ReadError, ConsoleControlStrings.ReadConsoleOutputExceptionTemplate);
                            // if cols == colsRemaining, nothing is guaranteed read in this pass and
                            //  the unread area is still rectangular
                                // some areas have been read. This could only happen when the number of columns
                                // to write is larger than bufferLimit. In that case, the algorithm reads one row
                    readRegion.Left += bufferSize.X;
                    if (colsRemaining > 0 && (bufferSize.Y == 1) &&
                        (contents[atContents.Bottom, atContents.Right].Character == ' '))
                        colsRemaining++;
                        readRegion.Left--;
                readRegion.Top += bufferSize.Y;
            // The following nested loop set the value of the empty cells in contents:
            // character to ' '
            // foreground color to console's foreground color
            // background color to console's background color
            int rowIndex = contents.GetLowerBound(0);
            int rowEnd = contents.GetUpperBound(0);
            int colBegin = contents.GetLowerBound(1);
            int colEnd = contents.GetUpperBound(1);
            CONSOLE_SCREEN_BUFFER_INFO bufferInfo =
            ConsoleColor foreground = 0;
            ConsoleColor background = 0;
            WORDToColor(
                            bufferInfo.Attributes,
                            out foreground,
                            out background
            while (rowIndex <= rowEnd)
                int colIndex = colBegin;
                    // if contents[rowIndex,colIndex] is in contentsRegion, hence a non-empty cell,
                    // move colIndex to one past the right end of contentsRegion
                    if (contentsRegion.Top <= rowIndex && rowIndex <= contentsRegion.Bottom &&
                        contentsRegion.Left <= colIndex && colIndex <= contentsRegion.Right)
                        colIndex = contentsRegion.Right + 1;
                    // colIndex past contents last column
                    if (colIndex > colEnd)
                    contents[rowIndex, colIndex] = new BufferCell(
                        ' ', foreground, background, BufferCellType.Complete);
                    colIndex++;
                rowIndex++;
        #endregion ReadConsoleOutput CJK
        private static void ReadConsoleOutputPlain
            // keeps track of which screen area read
                    // Call NativeMethods.ReadConsoleOutput
                    for (int r = atContentsRow; r < bufferSize.Y + atContentsRow; r++)
                        for (int c = atContentsCol; c < bufferSize.X + atContentsCol; c++, characterBufferIndex++)
                    contents[rowIndex, colIndex].Character = ' ';
                    contents[rowIndex, colIndex].ForegroundColor = foreground;
                    contents[rowIndex, colIndex].BackgroundColor = background;
        /// Wraps Win32 FillConsoleOutputCharacter.
        /// handle for the console where output is filled
        /// <param name="character">
        /// character to fill the console output
        /// <param name="numberToWrite">
        /// number of times to write character
        /// If Win32's FillConsoleOutputCharacter fails
        internal static void FillConsoleOutputCharacter
            char character,
            int numberToWrite,
            Coordinates origin
            COORD c;
            c.X = (short)origin.X;
            c.Y = (short)origin.Y;
                NativeMethods.FillConsoleOutputCharacter(
                    character,
                    (DWORD)numberToWrite,
                    c,
                    out _);
                HostException e = CreateHostException(err, "FillConsoleOutputCharacter",
                    ErrorCategory.WriteError, ConsoleControlStrings.FillConsoleOutputCharacterExceptionTemplate);
            // we don't assert that the number actually written matches the number we asked for, as the function may clip if
            // the number of cells to write extends past the end of the screen buffer.
        /// Wraps Win32 FillConsoleOutputAttribute.
        /// <param name="attribute">
        /// attribute to fill the console output
        /// number of times to write attribute
        /// If Win32's FillConsoleOutputAttribute fails
        internal static void FillConsoleOutputAttribute
            WORD attribute,
                NativeMethods.FillConsoleOutputAttribute(
                    attribute,
                HostException e = CreateHostException(err, "FillConsoleOutputAttribute",
                    ErrorCategory.WriteError, ConsoleControlStrings.FillConsoleOutputAttributeExceptionTemplate);
        /// Wrap Win32 ScrollConsoleScreenBuffer.
        /// handle for the console where screen buffer is scrolled
        /// <param name="scrollRectangle">
        /// area to be scrolled
        /// <param name="clipRectangle">
        /// area to be updated after scrolling
        /// <param name="destOrigin">
        /// location to which the top left corner of scrollRectangle move
        /// <param name="fill">
        /// character and attribute to fill the area vacated by the scroll
        /// If Win32's ScrollConsoleScreenBuffer fails
        internal static void ScrollConsoleScreenBuffer
            SMALL_RECT scrollRectangle,
            SMALL_RECT clipRectangle,
            COORD destOrigin, CHAR_INFO fill
                NativeMethods.ScrollConsoleScreenBuffer(
                    ref scrollRectangle,
                    ref clipRectangle,
                    destOrigin,
                    ref fill);
                HostException e = CreateHostException(err, "ScrollConsoleScreenBuffer",
                    ErrorCategory.WriteError, ConsoleControlStrings.ScrollConsoleScreenBufferExceptionTemplate);
        #endregion Buffer
        #region Window
        /// Wraps Win32 SetConsoleWindowInfo.
        /// handle for the console where window info is set
        /// <param name="absolute">
        /// If this parameter is TRUE, the coordinates specify the new upper-left and
        /// lower-right corners of the window. If it is false, the coordinates are offsets
        /// to the current window-corner coordinates
        /// <param name="windowInfo">
        /// specify the size and position of the console screen buffer's window
        /// If Win32's SetConsoleWindowInfo fails
        internal static void SetConsoleWindowInfo(ConsoleHandle consoleHandle, bool absolute, SMALL_RECT windowInfo)
            bool result = NativeMethods.SetConsoleWindowInfo(consoleHandle.DangerousGetHandle(), absolute, ref windowInfo);
                HostException e = CreateHostException(err, "SetConsoleWindowInfo",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleWindowInfoExceptionTemplate);
        /// Wraps Win32 GetLargestConsoleWindowSize.
        /// handle for the console for which the largest window size is obtained
        /// the largest window size
        /// If Win32's GetLargestConsoleWindowSize fails
        internal static Size GetLargestConsoleWindowSize(ConsoleHandle consoleHandle)
            COORD result = NativeMethods.GetLargestConsoleWindowSize(consoleHandle.DangerousGetHandle());
            if ((result.X == 0) && (result.Y == 0))
                HostException e = CreateHostException(err, "GetLargestConsoleWindowSize",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetLargestConsoleWindowSizeExceptionTemplate);
            return new Size(result.X, result.Y);
        /// Wraps Win32 GetConsoleTitle. 1K is the safe limit experimentally. The 64K limit
        /// found in the docs is disregarded because it is essentially meaningless.
        /// a string for the title of the window
        /// If Win32's GetConsoleTitle fails
        internal static string GetConsoleWindowTitle()
            const int MaxWindowTitleLength = 1024;
            const DWORD bufferSize = MaxWindowTitleLength;
            DWORD result;
            StringBuilder consoleTitle = new StringBuilder((int)bufferSize);
            result = NativeMethods.GetConsoleTitle(consoleTitle, bufferSize);
            // If the result is zero, it may mean and error but it may also mean
            // that the window title has been set to null. Since we can't tell the
            // the difference, we'll just return the empty string every time.
            return consoleTitle.ToString();
        private static bool s_dontsetConsoleWindowTitle;
        /// Wraps Win32 SetConsoleTitle.
        /// <param name="consoleTitle">
        /// If Win32's SetConsoleTitle fails
        internal static void SetConsoleWindowTitle(string consoleTitle)
            if (s_dontsetConsoleWindowTitle)
            bool result = NativeMethods.SetConsoleTitle(consoleTitle);
                // ERROR_GEN_FAILURE is returned if this api can't be used with the terminal
                if (err == 0x1f)
                    tracer.WriteLine("Call to SetConsoleTitle failed: {0}", err);
                    s_dontsetConsoleWindowTitle = true;
                    // We ignore this specific error as the console can still continue to operate
                HostException e = CreateHostException(err, "SetConsoleWindowTitle",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleWindowTitleExceptionTemplate);
        #endregion Window
        /// Wrap Win32 WriteConsole.
        /// Handle for the console where the string is written.
        /// String that is written.
        /// <param name="newLine">
        /// New line is written.
        /// If the Win32's WriteConsole fails.
        internal static void WriteConsole(ConsoleHandle consoleHandle, ReadOnlySpan<char> output, bool newLine)
            if (output.Length == 0)
                    WriteConsole(consoleHandle, Environment.NewLine);
            // Native WriteConsole doesn't support output buffer longer than 64K, so we need to chop the output string if it is too long.
            // This records the chopping position in output string.
            int cursor = 0;
            // This is 64K/4 - 1 to account for possible width of each character.
            const int MaxBufferSize = 16383;
            const int MaxStackAllocSize = 512;
            ReadOnlySpan<char> outBuffer;
            // In case that a new line is required, we try to write out the last chunk and the new-line string together,
            // to avoid one extra call to 'WriteConsole' just for a new line string.
            while (cursor + MaxBufferSize < output.Length)
                outBuffer = output.Slice(cursor, MaxBufferSize);
                cursor += MaxBufferSize;
                WriteConsole(consoleHandle, outBuffer);
            outBuffer = output.Slice(cursor);
            if (!newLine)
            char[] rentedArray = null;
            string lineEnding = Environment.NewLine;
            int size = outBuffer.Length + lineEnding.Length;
            // We expect the 'size' will often be small, and thus optimize that case with 'stackalloc'.
            Span<char> buffer = size <= MaxStackAllocSize ? stackalloc char[size] : default;
                if (buffer.IsEmpty)
                    rentedArray = ArrayPool<char>.Shared.Rent(size);
                    buffer = rentedArray.AsSpan().Slice(0, size);
                outBuffer.CopyTo(buffer);
                lineEnding.CopyTo(buffer.Slice(outBuffer.Length));
                WriteConsole(consoleHandle, buffer);
                if (rentedArray is not null)
                    ArrayPool<char>.Shared.Return(rentedArray);
        private static void WriteConsole(ConsoleHandle consoleHandle, ReadOnlySpan<char> buffer)
            DWORD charsWritten;
                NativeMethods.WriteConsole(
                    out charsWritten,
                    IntPtr.Zero);
                    "WriteConsole",
                    ConsoleControlStrings.WriteConsoleExceptionTemplate);
        /// Wraps Win32 SetConsoleTextAttribute.
        /// handle for the console where text attribute is set
        /// text attribute to set the console
        /// if the Win32's SetConsoleTextAttribute fails
        internal static void SetConsoleTextAttribute(ConsoleHandle consoleHandle, WORD attribute)
            bool result = NativeMethods.SetConsoleTextAttribute(consoleHandle.DangerousGetHandle(), attribute);
                HostException e = CreateHostException(err, "SetConsoleTextAttribute",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleTextAttributeExceptionTemplate);
        #region Dealing with CJK
        // Return the length of a VT100 control sequence character in str starting
        // at the given offset.
        // This code only handles the following formatting sequences, corresponding to
        // the patterns:
        //     CSI params? 'm'               // SGR: Select Graphics Rendition
        //     CSI params? '#' [{}pq]        // XTPUSHSGR ('{'), XTPOPSGR ('}'), or their aliases ('p' and 'q')
        // Where:
        //     params: digit+ ((';' | ':') params)?
        //     CSI:     C0_CSI | C1_CSI
        //     C0_CSI:  \x001b '['            // ESC '['
        //     C1_CSI:  \x009b
        // There are many other VT100 escape sequences, but these text attribute sequences
        // (color-related, underline, etc.) are sufficient for our formatting system.  We
        // won't handle cursor movements or other attempts at animation.
        // Note that offset is adjusted past the escape sequence, or at least one
        // character forward if there is no escape sequence at the specified position.
        internal static int ControlSequenceLength(string str, ref int offset)
            var start = offset;
            // First, check for the CSI:
            if ((str[offset] == (char)0x1b) && (str.Length > (offset + 1)) && (str[offset + 1] == '['))
                // C0 CSI
            else if (str[offset] == (char)0x9b)
                // C1 CSI
                offset += 1;
                // No CSI at the current location, so we are done looking, but we still
                // need to advance offset.
            if (offset >= str.Length)
            // Next, handle possible numeric arguments:
                c = str[offset++];
            while ((offset < str.Length) && (char.IsDigit(c) || (c == ';') || (c == ':')));
            // Finally, handle the command characters for the specific sequences we
            // handle:
            if (c == 'm')
                // SGR: Select Graphics Rendition
                return offset - start;
            // Maybe XTPUSHSGR or XTPOPSGR, but we need to read another char. Offset is
            // already positioned on the next char (or past the end).
            if (c == '#')
                // '{' : XTPUSHSGR
                // '}' : XTPOPSGR
                // 'p' : alias for XTPUSHSGR
                // 'q' : alias for XTPOPSGR
                if ((c == '{') ||
                    (c == '}') ||
                    (c == 'p') ||
                    (c == 'q'))
        /// From IsConsoleFullWidth in \windows\core\ntcon\server\dbcs.c.
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            MessageId = "Microsoft.PowerShell.ConsoleControl+NativeMethods.ReleaseDC(System.IntPtr,System.IntPtr)")]
        internal static int LengthInBufferCells(string str, int offset, bool checkEscapeSequences)
            Dbg.Assert(offset >= 0, "offset >= 0");
            Dbg.Assert(string.IsNullOrEmpty(str) || (offset < str.Length), "offset < str.Length");
            var escapeSequenceAdjustment = 0;
            if (checkEscapeSequences)
                while (i < offset)
                    ControlSequenceLength(str, ref i);
                // If offset != i, we're in the middle of a sequence, which the caller should avoid,
                // but we'll tolerate.
                while (i < str.Length)
                    escapeSequenceAdjustment += ControlSequenceLength(str, ref i);
            foreach (char c in str)
                length += LengthInBufferCells(c);
            return length - offset - escapeSequenceAdjustment;
        internal static int LengthInBufferCells(char c)
            // The following is based on http://www.cl.cam.ac.uk/~mgk25/c/wcwidth.c
            // which is derived from https://www.unicode.org/Public/UCD/latest/ucd/EastAsianWidth.txt
            bool isWide = c >= 0x1100 &&
                (c <= 0x115f || /* Hangul Jamo init. consonants */
                 c == 0x2329 || c == 0x232a ||
                 ((uint)(c - 0x2e80) <= (0xa4cf - 0x2e80) &&
                  c != 0x303f) || /* CJK ... Yi */
                 ((uint)(c - 0xac00) <= (0xd7a3 - 0xac00)) || /* Hangul Syllables */
                 ((uint)(c - 0xf900) <= (0xfaff - 0xf900)) || /* CJK Compatibility Ideographs */
                 ((uint)(c - 0xfe10) <= (0xfe19 - 0xfe10)) || /* Vertical forms */
                 ((uint)(c - 0xfe30) <= (0xfe6f - 0xfe30)) || /* CJK Compatibility Forms */
                 ((uint)(c - 0xff00) <= (0xff60 - 0xff00)) || /* Fullwidth Forms */
                 ((uint)(c - 0xffe0) <= (0xffe6 - 0xffe0)));
            // We can ignore these ranges because .Net strings use surrogate pairs
            // for this range and we do not handle surrogate pairs.
            // (c >= 0x20000 && c <= 0x2fffd) ||
            // (c >= 0x30000 && c <= 0x3fffd)
            return 1 + (isWide ? 1 : 0);
        /// Check if the output buffer code page is Japanese, Simplified Chinese, Korean, or Traditional Chinese.
        internal static bool IsCJKOutputCodePage(out uint codePage)
            codePage = NativeMethods.GetConsoleOutputCP();
            return codePage == 932 || // Japanese
                codePage == 936 || // Simplified Chinese
                codePage == 949 || // Korean
                codePage == 950;  // Traditional Chinese
        #endregion Dealing with CJK
        #region Cursor
        /// Wraps Win32 GetConsoleCursorInfo.
        /// handle for the console where cursor info is obtained
        /// cursor info
        /// If Win32's GetConsoleCursorInfo fails
        internal static CONSOLE_CURSOR_INFO GetConsoleCursorInfo(ConsoleHandle consoleHandle)
            CONSOLE_CURSOR_INFO cursorInfo;
            bool result = NativeMethods.GetConsoleCursorInfo(consoleHandle.DangerousGetHandle(), out cursorInfo);
                HostException e = CreateHostException(err, "GetConsoleCursorInfo",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetConsoleCursorInfoExceptionTemplate);
            return cursorInfo;
        internal static CONSOLE_FONT_INFO_EX GetConsoleFontInfo(ConsoleHandle consoleHandle)
            CONSOLE_FONT_INFO_EX fontInfo = new CONSOLE_FONT_INFO_EX();
            fontInfo.cbSize = Marshal.SizeOf(fontInfo);
            bool result = NativeMethods.GetCurrentConsoleFontEx(consoleHandle.DangerousGetHandle(), false, ref fontInfo);
                HostException e = CreateHostException(err, "GetConsoleFontInfo",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.GetConsoleFontInfoExceptionTemplate);
            return fontInfo;
        /// Wraps Win32 SetConsoleCursorInfo.
        /// handle for the console where cursor info is set
        /// <param name="cursorInfo">
        /// cursor info to set the cursor
        /// If Win32's SetConsoleCursorInfo fails
        internal static void SetConsoleCursorInfo(ConsoleHandle consoleHandle, CONSOLE_CURSOR_INFO cursorInfo)
            bool result = NativeMethods.SetConsoleCursorInfo(consoleHandle.DangerousGetHandle(), ref cursorInfo);
                HostException e = CreateHostException(err, "SetConsoleCursorInfo",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SetConsoleCursorInfoExceptionTemplate);
        #endregion Cursor
        /// Helper function to create the proper HostException.
        /// <param name="win32Error"></param>
        /// <param name="resourceStr"></param>
        private static HostException CreateHostException(
            int win32Error, string errorId, ErrorCategory category, string resourceStr)
            Win32Exception innerException = new Win32Exception(win32Error);
            string msg = StringUtil.Format(resourceStr, innerException.Message, win32Error);
            HostException e = new HostException(msg, innerException, errorId, category);
            return e;
        #region SendInput
        internal static void MimicKeyPress(INPUT[] inputs)
            Dbg.Assert(inputs != null && inputs.Length > 0, "inputs should not be null or empty");
            var numberOfSuccessfulEvents = NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
            if (numberOfSuccessfulEvents == 0)
                HostException e = CreateHostException(err, "SendKeyPressInput",
                    ErrorCategory.ResourceUnavailable, ConsoleControlStrings.SendKeyPressInputExceptionTemplate);
        #endregion SendInput
        /// Class to hold the Native Methods used in this file enclosing class.
            internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);  // WinBase.h
            internal const int FontTypeMask = 0x06;
            internal const int TrueTypeFont = 0x04;
            #region CreateFile
            internal enum AccessQualifiers : uint
                // From winnt.h
                GenericRead = 0x80000000,
                GenericWrite = 0x40000000
            internal enum ShareModes : uint
                ShareRead = 0x00000001,
                ShareWrite = 0x00000002
            internal enum CreationDisposition : uint
                // From winbase.h
                CreateNew = 1,
                CreateAlways = 2,
                OpenExisting = 3,
                OpenAlways = 4,
                TruncateExisting = 5
            [DllImport(PinvokeDllNames.CreateFileDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern NakedWin32Handle CreateFile
                string fileName,
                DWORD desiredAccess,
                DWORD ShareModes,
                IntPtr securityAttributes,
                DWORD creationDisposition,
                DWORD flagsAndAttributes,
                NakedWin32Handle templateFileWin32Handle
            #endregion CreateFile
            #region Code Page
            [DllImport(PinvokeDllNames.GetConsoleOutputCPDllName, SetLastError = false, CharSet = CharSet.Unicode)]
            internal static extern uint GetConsoleOutputCP();
            #endregion Code Page
            [DllImport(PinvokeDllNames.GetConsoleWindowDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern HWND GetConsoleWindow();
            [DllImport(PinvokeDllNames.GetDCDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern HDC GetDC(HWND hwnd);
            [DllImport(PinvokeDllNames.ReleaseDCDllName, SetLastError = false, CharSet = CharSet.Unicode)]
            internal static extern int ReleaseDC(HWND hwnd, HDC hdc);
            [DllImport(PinvokeDllNames.FlushConsoleInputBufferDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool FlushConsoleInputBuffer(NakedWin32Handle consoleInput);
            [DllImport(PinvokeDllNames.FillConsoleOutputAttributeDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool FillConsoleOutputAttribute
                NakedWin32Handle consoleOutput,
                DWORD length,
                COORD writeCoord,
                out DWORD numberOfAttrsWritten
            [DllImport(PinvokeDllNames.FillConsoleOutputCharacterDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool FillConsoleOutputCharacter
                out DWORD numberOfCharsWritten
            [DllImport(PinvokeDllNames.WriteConsoleDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern unsafe bool WriteConsole
                char* buffer,
                DWORD numberOfCharsToWrite,
                out DWORD numberOfCharsWritten,
                IntPtr reserved
            internal static unsafe bool WriteConsole
                ReadOnlySpan<char> buffer,
                fixed (char* bufferPtr = &MemoryMarshal.GetReference(buffer))
                    return WriteConsole(consoleOutput, bufferPtr, numberOfCharsToWrite, out numberOfCharsWritten, reserved);
            [DllImport(PinvokeDllNames.GetConsoleTitleDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern DWORD GetConsoleTitle(StringBuilder consoleTitle, DWORD size);
            [DllImport(PinvokeDllNames.SetConsoleTitleDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool SetConsoleTitle(string consoleTitle);
            [DllImport(PinvokeDllNames.GetConsoleModeDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool GetConsoleMode(NakedWin32Handle consoleHandle, out UInt32 mode);
            [DllImport(PinvokeDllNames.GetConsoleScreenBufferInfoDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool GetConsoleScreenBufferInfo(NakedWin32Handle consoleHandle, out CONSOLE_SCREEN_BUFFER_INFO consoleScreenBufferInfo);
            [DllImport(PinvokeDllNames.GetLargestConsoleWindowSizeDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern COORD GetLargestConsoleWindowSize(NakedWin32Handle consoleOutput);
            [DllImport(PinvokeDllNames.ReadConsoleDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern unsafe bool ReadConsole
                NakedWin32Handle consoleInput,
                char* lpBuffer,
                DWORD numberOfCharsToRead,
                out DWORD numberOfCharsRead,
                ref CONSOLE_READCONSOLE_CONTROL controlData
            internal static unsafe bool ReadConsole
                Span<char> buffer,
                    return ReadConsole(consoleInput, bufferPtr, numberOfCharsToRead, out numberOfCharsRead, ref controlData);
            [DllImport(PinvokeDllNames.PeekConsoleInputDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool PeekConsoleInput
                [Out] INPUT_RECORD[] buffer,
                out DWORD numberOfEventsRead
            [DllImport(PinvokeDllNames.GetNumberOfConsoleInputEventsDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool GetNumberOfConsoleInputEvents(NakedWin32Handle consoleInput, out DWORD numberOfEvents);
            [DllImport(PinvokeDllNames.SetConsoleCtrlHandlerDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool SetConsoleCtrlHandler(BreakHandler handlerRoutine, bool add);
            [DllImport(PinvokeDllNames.SetConsoleModeDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool SetConsoleMode(NakedWin32Handle consoleHandle, DWORD mode);
            [DllImport(PinvokeDllNames.SetConsoleScreenBufferSizeDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool SetConsoleScreenBufferSize(NakedWin32Handle consoleOutput, COORD size);
            [DllImport(PinvokeDllNames.SetConsoleTextAttributeDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool SetConsoleTextAttribute(NakedWin32Handle consoleOutput, WORD attributes);
            [DllImport(PinvokeDllNames.SetConsoleWindowInfoDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool SetConsoleWindowInfo(NakedWin32Handle consoleHandle, bool absolute, ref SMALL_RECT windowInfo);
            [DllImport(PinvokeDllNames.WriteConsoleOutputDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool WriteConsoleOutput
                CHAR_INFO[] buffer,
                COORD bufferSize,
                COORD bufferCoord,
                ref SMALL_RECT writeRegion
            [DllImport(PinvokeDllNames.ReadConsoleOutputDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool ReadConsoleOutput
                [Out] CHAR_INFO[] buffer,
                ref SMALL_RECT readRegion
            [DllImport(PinvokeDllNames.ScrollConsoleScreenBufferDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool ScrollConsoleScreenBuffer
                ref SMALL_RECT scrollRectangle,
                ref SMALL_RECT clipRectangle,
                COORD destinationOrigin,
                ref CHAR_INFO fill
            [DllImport(PinvokeDllNames.SendInputDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern UInt32 SendInput(UInt32 inputNumbers, INPUT[] inputs, int sizeOfInput);
            // There is no GetCurrentConsoleFontEx on Core
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool GetCurrentConsoleFontEx(NakedWin32Handle consoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFO_EX consoleFontInfo);
            [DllImport(PinvokeDllNames.GetConsoleCursorInfoDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool GetConsoleCursorInfo(NakedWin32Handle consoleOutput, out CONSOLE_CURSOR_INFO consoleCursorInfo);
            [DllImport(PinvokeDllNames.SetConsoleCursorInfoDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool SetConsoleCursorInfo(NakedWin32Handle consoleOutput, ref CONSOLE_CURSOR_INFO consoleCursorInfo);
            [DllImport(PinvokeDllNames.ReadConsoleInputDllName, SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern bool ReadConsoleInput
            internal enum CHAR_INFO_Attributes : uint
                COMMON_LVB_LEADING_BYTE = 0x0100,
                COMMON_LVB_TRAILING_BYTE = 0x0200
        [TraceSourceAttribute("ConsoleControl", "Console control methods")]
        private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("ConsoleControl", "Console control methods");
