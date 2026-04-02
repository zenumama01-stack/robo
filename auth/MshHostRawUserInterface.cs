    #region Ancillary types.
    // I would have preferred to make these nested types within PSHostRawUserInterface, but that
    // is evidently discouraged by the .net design guidelines.
    /// Represents an (x,y) coordinate pair.
    struct Coordinates
        private int x;
        private int y;
        /// Gets and sets the X coordinate.
        public int X
            get { return x; }
            set { x = value; }
        /// Gets and sets the Y coordinate.
        public int Y
            get { return y; }
            set { y = value; }
        /// Initializes a new instance of the Coordinates class and defines the X and Y values.
        /// <param name="x">
        /// The X coordinate
        /// <param name="y">
        /// The Y coordinate
        Coordinates(int x, int y)
            this.x = x;
            this.y = y;
        /// "a,b" where a and b are the values of the X and Y properties.
        /// Overrides <see cref="object.Equals(object)"/>
        /// object to be compared for equality.
        /// True if <paramref name="objB"/> is Coordinates and its X and Y values are the same as those of this instance,
        /// false if not.
        Equals(object obj)
            if (obj is Coordinates)
                result = this == ((Coordinates)obj);
        /// Overrides <see cref="object.GetHashCode"/>
        /// Hash code for this instance.
        GetHashCode()
            // idea: consider X the high-order part of a 64-bit in, and Y the lower order half.  Then use the int64.GetHashCode.
            UInt64 i64 = 0;
            if (X < 0)
                if (X == Int32.MinValue)
                    // add one and invert to avoid an overflow.
                    i64 = (UInt64)(-1 * (X + 1));
                    i64 = (UInt64)(-X);
                i64 = (UInt64)X;
            // rotate 32 bits to the left.
            i64 *= 0x100000000U;
            // mask in Y
            if (Y < 0)
                if (Y == Int32.MinValue)
                    i64 += (UInt64)(-1 * (Y + 1));
                    i64 += (UInt64)(-Y);
                i64 += (UInt64)Y;
            int result = i64.GetHashCode();
        /// Compares two instances for equality.
        /// The left side operand.
        /// The right side operand.
        /// true if the respective X and Y values are the same, false otherwise.
        public static
        operator ==(Coordinates first, Coordinates second)
            bool result = first.X == second.X && first.Y == second.Y;
        /// Compares two instances for inequality.
        /// true if any of the respective either X or Y field is not the same, false otherwise.
        operator !=(Coordinates first, Coordinates second)
            return !(first == second);
    /// Represents a width and height pair.
    struct Size
        private int width;
        private int height;
        /// Gets and sets the Width.
            get { return width; }
            set { width = value; }
        /// Gets and sets the Height.
        public int Height
            get { return height; }
            set { height = value; }
        /// Initialize a new instance of the Size class and defines the Width and Height values.
        /// <param name="width">
        /// The Width
        /// <param name="height">
        /// The Height
        Size(int width, int height)
            this.width = width;
            this.height = height;
        /// Overloads <see cref="object.ToString"/>
        /// "a,b" where a and b are the values of the Width and Height properties.
            return string.Create(CultureInfo.InvariantCulture, $"{Width},{Height}");
        /// True if <paramref name="obj"/> is Size and its Width and Height values are the same as those of this instance,
            if (obj is Size)
                result = this == ((Size)obj);
        /// consider Width the high-order part of a 64-bit in, and
        /// Height the lower order half.  Then use the int64.GetHashCode.-->
            // idea: consider Width the high-order part of a 64-bit in, and Height the lower order half.  Then use the int64.GetHashCode.
            if (Width < 0)
                if (Width == Int32.MinValue)
                    i64 = (UInt64)(-1 * (Width + 1));
                    i64 = (UInt64)(-Width);
                i64 = (UInt64)Width;
            // mask in Height
            if (Height < 0)
                if (Height == Int32.MinValue)
                    i64 += (UInt64)(-1 * (Height + 1));
                    i64 += (UInt64)(-Height);
                i64 += (UInt64)Height;
        /// true if the respective Width and Height fields are the same, false otherwise.
        operator ==(Size first, Size second)
            bool result = first.Width == second.Width && first.Height == second.Height;
        /// true if any of the respective Width and Height fields are not the same, false otherwise.
        operator !=(Size first, Size second)
    /// Governs the behavior of <see cref="System.Management.Automation.Host.PSHostRawUserInterface.ReadKey()"/>
    /// and <see cref="System.Management.Automation.Host.PSHostRawUserInterface.ReadKey(System.Management.Automation.Host.ReadKeyOptions)"/>
    ReadKeyOptions
        /// Allow Ctrl-C to be processed as a keystroke, as opposed to causing a break event.
        AllowCtrlC = 0x0001,
        /// Do not display the character for the key in the window when pressed.
        NoEcho = 0x0002,
        /// Include key down events.  Either one of IncludeKeyDown and IncludeKeyUp or both must be specified.
        IncludeKeyDown = 0x0004,
        /// Include key up events.  Either one of IncludeKeyDown and IncludeKeyUp or both must be specified.
        IncludeKeyUp = 0x0008
    /// Defines the states of Control Key.
    enum ControlKeyStates
        /// The right alt key is pressed.
        RightAltPressed = 0x0001,
        /// The left alt key is pressed.
        LeftAltPressed = 0x0002,
        /// The right ctrl key is pressed.
        RightCtrlPressed = 0x0004,
        /// The left ctrl key is pressed.
        LeftCtrlPressed = 0x0008,
        /// The shift key is pressed.
        ShiftPressed = 0x0010,
        /// The numlock light is on.
        NumLockOn = 0x0020,
        /// The scrolllock light is on.
        ScrollLockOn = 0x0040,
        /// The capslock light is on.
        CapsLockOn = 0x0080,
        /// The key is enhanced.
        EnhancedKey = 0x0100
    /// Represents information of a keystroke.
    struct KeyInfo
        private int virtualKeyCode;
        private char character;
        private ControlKeyStates controlKeyState;
        private bool keyDown;
        /// Gets and set device-independent key.
        public int VirtualKeyCode
            get { return virtualKeyCode; }
            set { virtualKeyCode = value; }
        /// Gets and set unicode Character of the key.
        public char Character
            get { return character; }
            set { character = value; }
        /// State of the control keys.
        public ControlKeyStates ControlKeyState
            get { return controlKeyState; }
            set { controlKeyState = value; }
        /// Gets and set the status of whether this instance is generated by a key pressed or released.
        public bool KeyDown
            get { return keyDown; }
            set { keyDown = value; }
        /// Initialize a new instance of the KeyInfo class and defines the VirtualKeyCode,
        /// Character, ControlKeyState and KeyDown values.
        /// <param name="virtualKeyCode">
        /// The virtual key code
        /// <param name="ch">
        /// The character
        /// <param name="controlKeyState">
        /// The control key state
        /// <param name="keyDown">
        /// Whether the key is pressed or released
            int virtualKeyCode,
            char ch,
            ControlKeyStates controlKeyState,
            bool keyDown
            this.virtualKeyCode = virtualKeyCode;
            this.character = ch;
            this.controlKeyState = controlKeyState;
            this.keyDown = keyDown;
        /// "a,b,c,d" where a, b, c, and d are the values of the VirtualKeyCode, Character, ControlKeyState, and KeyDown properties.
            return string.Create(CultureInfo.InvariantCulture, $"{VirtualKeyCode},{Character},{ControlKeyState},{KeyDown}");
        /// True if <paramref name="obj"/> is KeyInfo and its VirtualKeyCode, Character, ControlKeyState, and KeyDown values are the
        /// same as those of this instance, false if not.
            if (obj is KeyInfo)
                result = this == ((KeyInfo)obj);
        /// <!--consider KeyDown (true == 1, false == 0) the highest-order nibble,
        ///                ControlKeyState the second to fourth highest-order nibbles
        ///                VirtualKeyCode the lower-order nibbles of a 32-bit int,
        ///       Then use the UInt32.GetHashCode.-->
            // idea: consider KeyDown (true == 1, false == 0) the highest-order nibble,
            //                ControlKeyState the second to fourth highest-order nibbles
            //                VirtualKeyCode the lower-order nibbles of a 32-bit int,
            //       Then use the UInt32.GetHashCode.
            UInt32 i32 = KeyDown ? 0x10000000U : 0;
            // mask in ControlKeyState
            i32 |= ((uint)ControlKeyState) << 16;
            // mask in the VirtualKeyCode
            i32 |= (UInt32)VirtualKeyCode;
            return i32.GetHashCode();
        /// true if the respective Character, ControlKeyStates , KeyDown, and VirtualKeyCode fields
        /// are the same, false otherwise.
        operator ==(KeyInfo first, KeyInfo second)
            bool result = first.Character == second.Character && first.ControlKeyState == second.ControlKeyState &&
                          first.KeyDown == second.KeyDown && first.VirtualKeyCode == second.VirtualKeyCode;
        /// true if any of the respective Character, ControlKeyStates , KeyDown, or VirtualKeyCode fields
        /// are the different, false otherwise.
        operator !=(KeyInfo first, KeyInfo second)
    /// Represents a rectangular region of the screen.
    /// <!--We use this structure instead of System.Drawing.Rectangle because S.D.R
    /// is way overkill and would bring in another assembly.-->
    struct Rectangle
        private int left;
        private int top;
        private int right;
        private int bottom;
        /// Gets and sets the left side of the rectangle.
        public int Left
            get { return left; }
            set { left = value; }
        /// Gets and sets the top of the rectangle.
        public int Top
            get { return top; }
            set { top = value; }
        /// Gets and sets the right side of the rectangle.
        public int Right
            get { return right; }
            set { right = value; }
        /// Gets and sets the bottom of the rectangle.
        public int Bottom
            get { return bottom; }
            set { bottom = value; }
        /// Initialize a new instance of the Rectangle class and defines the Left, Top, Right, and Bottom values.
        /// <param name="left">
        /// The left side of the rectangle
        /// <param name="top">
        /// The top of the rectangle
        /// <param name="right">
        /// The right side of the rectangle
        /// <param name="bottom">
        /// The bottom of the rectangle
        /// <paramref name="right"/> is less than <paramref name="left"/>;
        /// <paramref name="bottom"/> is less than <paramref name="top"/>
        Rectangle(int left, int top, int right, int bottom)
            if (right < left)
                // "right" and "left" are not localizable
                throw PSTraceSource.NewArgumentException(nameof(right), MshHostRawUserInterfaceStrings.LessThanErrorTemplate, "right", "left");
            if (bottom < top)
                // "bottom" and "top" are not localizable
                throw PSTraceSource.NewArgumentException(nameof(bottom), MshHostRawUserInterfaceStrings.LessThanErrorTemplate, "bottom", "top");
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        /// Initializes a new instance of the Rectangle class and defines the Left, Top, Right, and Bottom values
        /// by <paramref name="upperLeft"/>, the upper left corner and <paramref name="lowerRight"/>, the lower
        /// right corner.
        /// Added based on feedback from review with BCL PM.
        /// <param name="upperLeft">
        /// The Coordinates of the upper left corner of the Rectangle
        /// <param name="lowerRight">
        /// The Coordinates of the lower right corner of the Rectangle
        Rectangle(Coordinates upperLeft, Coordinates lowerRight)
            : this(upperLeft.X, upperLeft.Y, lowerRight.X, lowerRight.Y)
        /// "a,b ; c,d" where a, b, c, and d are values of the Left, Top, Right, and Bottom properties.
            return string.Create(CultureInfo.InvariantCulture, $"{Left},{Top} ; {Right},{Bottom}");
        /// True if <paramref name="obj"/> is Rectangle and its Left, Top, Right, and Bottom values are the same as those of this instance,
            if (obj is Rectangle)
                result = this == ((Rectangle)obj);
        /// <!-- consider (Top XOR Bottom) the high-order part of a 64-bit int,
        ///                (Left XOR Right) the lower order half.  Then use the int64.GetHashCode.-->
            // idea: consider (Top XOR Bottom) the high-order part of a 64-bit int,
            //                (Left XOR Right) the lower order half.  Then use the int64.GetHashCode.
            int upper = Top ^ Bottom;
            if (upper < 0)
                if (upper == Int32.MinValue)
                    i64 = (UInt64)(-1 * (upper + 1));
                    i64 = (UInt64)(-upper);
                i64 = (UInt64)upper;
            // mask in lower
            int lower = Left ^ Right;
            if (lower < 0)
                if (lower == Int32.MinValue)
                    i64 += (UInt64)(-1 * (lower + 1));
                    i64 += (UInt64)(-upper);
                i64 += (UInt64)lower;
        /// true if the respective Top, Left, Bottom, and Right fields are the same, false otherwise.
        operator ==(Rectangle first, Rectangle second)
            bool result = first.Top == second.Top && first.Left == second.Left &&
             first.Bottom == second.Bottom && first.Right == second.Right;
        /// true if any of the respective Top, Left, Bottom, and Right fields are not the same, false otherwise.
        operator !=(Rectangle first, Rectangle second)
    /// Represents a character, a foregroundColor color, and background color.
    struct BufferCell
        private ConsoleColor foregroundColor;
        private ConsoleColor backgroundColor;
        private BufferCellType bufferCellType;
        /// Gets and sets the character value.
        // we reuse System.ConsoleColor - it's in the core assembly, and I think it would be confusing to create another
        // essentially identical enum
        /// Gets and sets the foreground color.
        public ConsoleColor ForegroundColor
            get { return foregroundColor; }
            set { foregroundColor = value; }
        /// Gets and sets the background color.
        public ConsoleColor BackgroundColor
            get { return backgroundColor; }
            set { backgroundColor = value; }
        /// Gets and sets the type value.
        public BufferCellType BufferCellType
            get { return bufferCellType; }
            set { bufferCellType = value; }
        /// Initializes a new instance of the BufferCell class and defines the
        /// Character, ForegroundColor, BackgroundColor and Type values.
        /// The character in this BufferCell object
        /// <param name="foreground">
        /// The foreground color of this BufferCell object
        /// <param name="background">
        /// <param name="bufferCellType">
        /// The type of this BufferCell object
        BufferCell(char character, ConsoleColor foreground, ConsoleColor background, BufferCellType bufferCellType)
            this.character = character;
            this.foregroundColor = foreground;
            this.backgroundColor = background;
            this.bufferCellType = bufferCellType;
        /// "'a' b c d" where a, b, c, and d are the values of the Character, ForegroundColor, BackgroundColor, and Type properties.
            return string.Create(CultureInfo.InvariantCulture, $"'{Character}' {ForegroundColor} {BackgroundColor} {BufferCellType}");
        /// True if <paramref name="obj"/> is BufferCell and its Character, ForegroundColor, BackgroundColor, and BufferCellType values
        /// are the same as those of this instance, false if not.
            if (obj is BufferCell)
                result = this == ((BufferCell)obj);
        /// <!-- consider (ForegroundColor XOR BackgroundColor) the high-order part of a 32-bit int,
        ///      and Character the lower order half.  Then use the int32.GetHashCode.-->
            // idea: consider (ForegroundColor XOR BackgroundColor) the high-order part of a 32-bit int,
            //                and Character the lower order half.  Then use the int32.GetHashCode.
            UInt32 i32 = ((uint)(ForegroundColor ^ BackgroundColor)) << 16;
            i32 |= (UInt16)Character;
            int result = i32.GetHashCode();
        /// true if the respective Character, ForegroundColor, BackgroundColor, and BufferCellType values are the same, false otherwise.
        operator ==(BufferCell first, BufferCell second)
            bool result = first.Character == second.Character &&
                          first.BackgroundColor == second.BackgroundColor &&
                          first.ForegroundColor == second.ForegroundColor &&
                          first.BufferCellType == second.BufferCellType;
        /// true if any of the respective Character, ForegroundColor, BackgroundColor, and BufferCellType values are not the same,
        operator !=(BufferCell first, BufferCell second)
        private const string StringsBaseName = "MshHostRawUserInterfaceStrings";
    /// Defines three types of BufferCells to accommodate for hosts that use up to two cells
    /// to display a character in some languages such as Chinese and Japanese.
    public enum
    BufferCellType
        /// Character occupies one BufferCell.
        /// Character occupies two BufferCells and this is the leading one.
        Leading,
        /// Preceded by a Leading BufferCell.
        Trailing
    #endregion Ancillary types
    /// Defines the lowest-level user interface functions that an interactive application hosting PowerShell
    /// <see cref="System.Management.Automation.Runspaces.Runspace"/> can choose to implement if it wants to
    /// support any cmdlet that does character-mode interaction with the user.
    /// It models an 2-dimensional grid of cells called a Buffer.  A buffer has a visible rectangular region, called a window.
    /// Each cell of the grid has a character, a foreground color, and a background color.  When the buffer has input focus, it
    /// shows a cursor positioned in one cell.  Keystrokes can be read from the buffer and optionally echoed at the current
    /// cursor position.
    /// <seealso cref="System.Management.Automation.Host.PSHost"/>
    public abstract
    class PSHostRawUserInterface
        PSHostRawUserInterface()
        /// Gets or sets the color used to render characters on the screen buffer. Each character cell in the screen buffer can
        /// have a separate foreground color.
        /// <!--Design note: we separate Foreground and Background colors into separate properties rather than having a single
        /// property that is a ColorAttribute.  While a single property that takes a struct is consistent with all of our
        /// other properties that take structs (e.g. -Position, -Size), I anticipate that the more common use-case for color
        /// is to just change the foreground color.-->
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.BackgroundColor"/>
        /// Gets or sets the color used to render the background behind characters on the screen buffer.  Each character cell in
        /// the screen buffer can have a separate background color.
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.ForegroundColor"/>
        /// Gets or sets the cursor position in the screen buffer.  The view window always adjusts it's location over the screen
        /// buffer such that the cursor is always visible.
        /// To write to the screen buffer without updating the cursor position, use
        /// <see cref="System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(Rectangle, BufferCell)"/> or
        /// <see cref="System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(Coordinates, BufferCell[,])"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.MaxPhysicalWindowSize"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.WindowSize"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.WindowPosition"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.MaxWindowSize"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(Rectangle, BufferCell)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.SetBufferContents(Coordinates, BufferCell[,])"/>
        /// Gets or sets position of the view window relative to the screen buffer, in characters. (0,0) is the upper left of the screen
        /// buffer.
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.CursorPosition"/>
        /// Gets or sets the cursor size as a percentage 0..100.
        /// Gets or sets the current size of the screen buffer, measured in character cells.
        /// Gets or sets the current view window size, measured in character cells.  The window size cannot be larger than the
        /// dimensions returned by <see cref="System.Management.Automation.Host.PSHostRawUserInterface.MaxPhysicalWindowSize"/>.
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.BufferSize"/>
        /// Gets the size of the largest window possible for the current buffer, current font, and current display hardware.
        /// The view window cannot be larger than the screen buffer or the current display (the display the window is rendered on).
        /// The largest dimensions the window can be resized to without resizing the screen buffer.
        /// Always returns a value less than or equal to
        /// <see cref="System.Management.Automation.Host.PSHostRawUserInterface.MaxPhysicalWindowSize"/>.
        /// Gets the largest window possible for the current font and display hardware, ignoring the current buffer dimensions.  In
        /// other words, the dimensions of the largest window that could be rendered in the current display, if the buffer was
        /// at least as large.
        /// To resize the window to this dimension, use <see cref="System.Management.Automation.Host.PSHostRawUserInterface.BufferSize"/>
        /// to first check and, if necessary, adjust, the screen buffer size.
        /// Reads a key stroke from the keyboard device, blocking until a keystroke is typed.
        /// Same as ReadKey(ReadKeyOptions.IncludeKeyDown)
        /// Key stroke when a key is pressed.
        ///         $Host.UI.RawUI.ReadKey()
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.ReadKey(ReadKeyOptions)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.FlushInputBuffer"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.KeyAvailable"/>
        ReadKey()
            return ReadKey(ReadKeyOptions.IncludeKeyDown);
        /// Either one of ReadKeyOptions.IncludeKeyDown and ReadKeyOptions.IncludeKeyUp or both must be specified.
        /// A bit mask of the options to be used to read the keyboard. Constants defined by
        /// <see cref="System.Management.Automation.Host.ReadKeyOptions"/>
        /// Key stroke depending on the value of <paramref name="options"/>.
        /// Neither ReadKeyOptions.IncludeKeyDown nor ReadKeyOptions.IncludeKeyUp is specified.
        ///         $option = [System.Management.Automation.Host.ReadKeyOptions]"IncludeKeyDown";
        ///         $host.UI.RawUI.ReadKey($option)
        /// <seealso cref="System.Management.Automation.Host.ReadKeyOptions"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.ReadKey()"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.ReadKey(System.Management.Automation.Host.ReadKeyOptions)"/>
        ReadKey(ReadKeyOptions options);
        /// Resets the keyboard input buffer.
        /// A non-blocking call to examine if a keystroke is waiting in the input buffer.
        /// True if a keystroke is waiting in the input buffer, false if not.
        /// Gets or sets the titlebar text of the current view window.
        /// Copies the <see cref="System.Management.Automation.Host.BufferCell"/> array into the screen buffer at the
        /// given origin, clipping such that cells in the array that would fall outside the screen buffer are ignored.
        /// The top left corner of the rectangular screen area to which <paramref name="contents"/> is copied.
        /// A rectangle of <see cref="System.Management.Automation.Host.BufferCell"/> objects to be copied to the
        /// screen buffer.
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(int, int, BufferCell)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(Size, BufferCell)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(string[], ConsoleColor, ConsoleColor)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(char)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(string)"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.GetBufferContents"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.ScrollBufferContents"/>
        SetBufferContents(Coordinates origin, BufferCell[,] contents);
        /// Copies a given character to all of the character cells in the screen buffer with the indicated colors.
        /// <param name="rectangle">
        /// The rectangle on the screen buffer to which <paramref name="fill"/> is copied.
        /// If all elements are -1, the entire screen buffer will be copied with <paramref name="fill"/>.
        /// The character and attributes used to fill <paramref name="rectangle"/>.
        ///         using System;
        ///         using System.Management.Automation;
        ///         using System.Management.Automation.Host;
        ///             [Cmdlet("Clear","Screen")]
        ///             public class ClearScreen : PSCmdlet
        ///                 protected override void BeginProcessing()
        ///                     Host.UI.RawUI.SetBufferContents(new Rectangle(-1, -1, -1, -1),
        ///                         new BufferCell(' ', Host.UI.RawUI.ForegroundColor, Host.UI.RawUI.BackgroundColor))
        SetBufferContents(Rectangle rectangle, BufferCell fill);
        /// Extracts a rectangular region of the screen buffer.
        /// The rectangle on the screen buffer to extract.
        /// An array of <see cref="System.Management.Automation.Host.BufferCell"/> objects extracted from
        /// the rectangular region of the screen buffer specified by <paramref name="rectangle"/>
        /// If the rectangle is completely outside of the screen buffer, a BufferCell array of zero rows and column will be
        /// If the rectangle is partially outside of the screen buffer, the area where the screen buffer and rectangle overlap
        /// will be read and returned. The size of the returned array is the same as that of r. Each BufferCell in the
        /// non-overlapping area of this array is set as follows:
        /// Character is the space (' ')
        /// ForegroundColor to the current foreground color, given by the ForegroundColor property of this class.
        /// BackgroundColor to the current background color, given by the BackgroundColor property of this class.
        /// The resulting array is organized in row-major order for performance reasons.  The screen buffer, however, is
        /// organized in column-major order -- e.g. you specify the column index first, then the row index second, as in (x, y).
        /// This means that a cell at screen buffer position (x, y) is in the array element [y, x].
        GetBufferContents(Rectangle rectangle);
        /// Scroll a region of the screen buffer.
        /// Indicates the region of the screen to be scrolled.
        /// Indicates the upper left coordinates of the region of the screen to receive the source region contents.  The target
        /// region is the same size as the source region.
        /// Indicates the region of the screen to include in the operation.  If a cell would be changed by the operation but
        /// does not fall within the clip region, it will be unchanged.
        /// The character and attributes to be used to fill any cells within the intersection of the source rectangle and
        /// clipping rectangle that are left "empty" by the move.
        /// Determines the number of BufferCells a substring of a string occupies.
        /// The string whose substring length we want to know.
        /// Offset where the substring begins in <paramref name="source"/>
        /// The default implementation calls <see cref="PSHostRawUserInterface.LengthInBufferCells(string)"/> method
        /// with the substring extracted from the <paramref name="source"/> string
        /// starting at the offset <paramref name="offset"/>
        public virtual
        LengthInBufferCells
            int offset
                throw PSTraceSource.NewArgumentNullException(nameof(source));
            // this implementation is inefficient
            // it is here to help with backcompatibility
            // it preserves the old behavior from the times
            // when there was only Length(string) overload
            string substring = offset == 0 ? source : source.Substring(offset);
            return this.LengthInBufferCells(substring);
        /// Determines the number of BufferCells a string occupies.
        /// The string whose length we want to know.
        /// The default implementation returns the length of <paramref name="source"/>
        /// <seealso cref="System.Management.Automation.Host.PSHostRawUserInterface.LengthInBufferCells(string, int)"/>
            string source
            return source.Length;
        /// Determines the number of BufferCells a character occupies.
        /// The character whose length we want to know.
        /// The default implementation returns 1.
            char source
        /// Creates a two dimensional array of BufferCells by examining each character in <paramref name="contents"/>.
        /// String array based on which the two dimensional array of BufferCells will be created.
        /// Foreground color of the buffer cells in the resulting array.
        /// Background color of the buffer cells in the resulting array.
        /// A two dimensional array of BufferCells whose characters are the same as those in <paramref name="contents"/>
        /// and whose foreground and background colors set to <paramref name="foregroundColor"/> and
        /// <paramref name="backgroundColor"/>
        /// <paramref name="contents"/> is null;
        /// Any string in <paramref name="contents"/> is null or empty
        /// If a character C takes one BufferCell to display as determined by LengthInBufferCells,
        /// one BufferCell is allocated with its Character set to C and BufferCellType to BufferCell.Complete.
        /// On the other hand, if C takes two BufferCell, two adjacent BufferCells on a row in
        /// the returned array will be allocated: the first has Character set to C and BufferCellType to
        /// <see cref="System.Management.Automation.Host.BufferCellType.Leading"/> and the second
        /// Character set to (char)0 and Type to
        /// <see cref="System.Management.Automation.Host.BufferCellType.Trailing"/>. Hence, the returned
        /// BufferCell array has <paramref name="contents"/>.Length number of rows and number of columns
        /// equal to the largest number of cells a string in <paramref name="contents"/> takes. The
        /// foreground and background colors of the cells are initialized to
        /// <paramref name="foregroundColor"/> and <paramref name="backgroundColor"/>, respectively.
        /// The resulting array is suitable for use with <see cref="PSHostRawUserInterface.SetBufferContents(Rectangle, BufferCell)"/>
        /// and <see cref="PSHostRawUserInterface.SetBufferContents(Coordinates, BufferCell[,])"/>.
        NewBufferCellArray(string[] contents, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
            byte[][] charLengths = new byte[contents.Length][];
            int maxStringLengthInBufferCells = 0;
            for (int i = 0; i < contents.Length; i++)
                if (string.IsNullOrEmpty(contents[i]))
                int lengthInBufferCells = 0;
                charLengths[i] = new byte[contents[i].Length];
                for (int j = 0; j < contents[i].Length; j++)
                    charLengths[i][j] = (byte)LengthInBufferCells(contents[i][j]);
                    lengthInBufferCells += charLengths[i][j];
                if (maxStringLengthInBufferCells < lengthInBufferCells)
                    maxStringLengthInBufferCells = lengthInBufferCells;
            if (maxStringLengthInBufferCells <= 0)
                throw PSTraceSource.NewArgumentException(nameof(contents), MshHostRawUserInterfaceStrings.AllNullOrEmptyStringsErrorTemplate);
            BufferCell[,] results = new BufferCell[contents.Length, maxStringLengthInBufferCells];
                int resultJ = 0;
                for (int j = 0; j < contents[i].Length; j++, resultJ++)
                    if (charLengths[i][j] == 1)
                        results[i, resultJ] =
                            new BufferCell(contents[i][j], foregroundColor, backgroundColor, BufferCellType.Complete);
                    else if (charLengths[i][j] == 2)
                            new BufferCell(contents[i][j], foregroundColor, backgroundColor, BufferCellType.Leading);
                        resultJ++;
                            new BufferCell((char)0, foregroundColor, backgroundColor, BufferCellType.Trailing);
                while (resultJ < maxStringLengthInBufferCells)
                    results[i, resultJ] = new BufferCell(' ', foregroundColor, backgroundColor, BufferCellType.Complete);
        /// Creates a 2D array of BufferCells by examining <paramref name="contents"/>.Character.
        /// <see cref="PSHostRawUserInterface"/>
        /// The number of columns of the resulting array
        /// The number of rows of the resulting array
        /// The cell to be copied to each of the elements of the resulting array.
        /// A <paramref name="width"/> by <paramref name="height"/> array of BufferCells where each cell's value is
        /// based on <paramref name="contents"/>
        /// <paramref name="width"/> is less than 1;
        /// <paramref name="height"/> is less than 1.
        /// If the character takes one BufferCell to display as determined by LengthInBufferCells,
        /// one BufferCell is allocated with its Character set to the character and BufferCellType to
        /// BufferCell.Complete.
        /// On the other hand, if it takes two BufferCells, two adjacent BufferCells on a row
        /// in the returned array will be allocated: the first has Character
        /// set to the character and BufferCellType to BufferCellType.Leading and the second Character
        /// set to (char)0 and BufferCellType to BufferCellType.Trailing. Moreover, if <paramref name="width"/>
        /// is odd, the last column will just contain the leading cell.
        /// <paramref name="prototype"/>.BufferCellType is not used in creating the array.
        /// The resulting array is suitable for use with the PSHostRawUserInterface.SetBufferContents method.
        NewBufferCellArray(int width, int height, BufferCell contents)
                // "width" is not localizable
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(width), width,
                    MshHostRawUserInterfaceStrings.NonPositiveNumberErrorTemplate, "width");
            if (height <= 0)
                // "height" is not localizable
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(height), height,
                    MshHostRawUserInterfaceStrings.NonPositiveNumberErrorTemplate, "height");
            BufferCell[,] buffer = new BufferCell[height, width];
            int charLength = LengthInBufferCells(contents.Character);
            if (charLength == 1)
                for (int r = 0; r < buffer.GetLength(0); ++r)
                    for (int c = 0; c < buffer.GetLength(1); ++c)
                        buffer[r, c] = contents;
                        buffer[r, c].BufferCellType = BufferCellType.Complete;
            else if (charLength == 2)
                int normalizedWidth = width % 2 == 0 ? width : width - 1;
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < normalizedWidth; j++)
                        buffer[i, j] = contents;
                        buffer[i, j].BufferCellType = BufferCellType.Leading;
                        j++;
                        buffer[i, j] = new BufferCell((char)0,
                            contents.ForegroundColor, contents.BackgroundColor,
                            BufferCellType.Trailing);
                    if (normalizedWidth < width)
                        buffer[i, normalizedWidth] = contents;
                        buffer[i, normalizedWidth].BufferCellType = BufferCellType.Leading;
            return buffer;
        /// Same as <see cref="System.Management.Automation.Host.PSHostRawUserInterface.NewBufferCellArray(int, int, BufferCell)"/>
        /// <param name="size">
        /// The width and height of the resulting array.
        /// An array of BufferCells whose size is <paramref name="size"/> and where each cell's value is
        /// If <paramref name="size"/>.Width or <paramref name="size"/>.Height is less than 1.
        NewBufferCellArray(Size size, BufferCell contents)
            return NewBufferCellArray(size.Width, size.Height, contents);
