    #region get-date
    /// Implementation for the get-date command.
    [Cmdlet(VerbsCommon.Get, "Date", DefaultParameterSetName = DateAndFormatParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096615")]
    [OutputType(typeof(DateTime), ParameterSetName = new[] { DateAndFormatParameterSet, UnixTimeSecondsAndFormatParameterSet })]
    public sealed class GetDateCommand : Cmdlet
        /// Allows user to override the date/time object that will be processed.
        [Parameter(ParameterSetName = DateAndFormatParameterSet, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = DateAndUFormatParameterSet, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Alias("LastWriteTime")]
        public DateTime Date
                return _date;
                _date = value;
                _dateSpecified = true;
        private DateTime _date;
        private bool _dateSpecified;
        // The const comes from DateTimeOffset.MinValue.ToUnixTimeSeconds()
        private const long MinimumUnixTimeSecond = -62135596800;
        // The const comes from DateTimeOffset.MaxValue.ToUnixTimeSeconds()
        private const long MaximumUnixTimeSecond = 253402300799;
        /// Gets or sets whether to treat a numeric input as ticks, or unix time.
        [Parameter(ParameterSetName = UnixTimeSecondsAndFormatParameterSet, Mandatory = true)]
        [Parameter(ParameterSetName = UnixTimeSecondsAndUFormatParameterSet, Mandatory = true)]
        [ValidateRange(MinimumUnixTimeSecond, MaximumUnixTimeSecond)]
        [Alias("UnixTime")]
        public long UnixTimeSeconds
                return _unixTimeSeconds;
                _unixTimeSeconds = value;
                _unixTimeSecondsSpecified = true;
        private long _unixTimeSeconds;
        private bool _unixTimeSecondsSpecified;
        /// Allows the user to override the year.
        [ValidateRange(1, 9999)]
        public int Year
                return _year;
                _year = value;
                _yearSpecified = true;
        private int _year;
        private bool _yearSpecified;
        /// Allows the user to override the month.
        [ValidateRange(1, 12)]
        public int Month
                return _month;
                _month = value;
                _monthSpecified = true;
        private int _month;
        private bool _monthSpecified;
        /// Allows the user to override the day.
        [ValidateRange(1, 31)]
        public int Day
                return _day;
                _day = value;
                _daySpecified = true;
        private int _day;
        private bool _daySpecified;
        /// Allows the user to override the hour.
        [ValidateRange(0, 23)]
        public int Hour
                return _hour;
                _hour = value;
                _hourSpecified = true;
        private int _hour;
        private bool _hourSpecified;
        /// Allows the user to override the minute.
        [ValidateRange(0, 59)]
        public int Minute
                return _minute;
                _minute = value;
                _minuteSpecified = true;
        private int _minute;
        private bool _minuteSpecified;
        /// Allows the user to override the second.
        public int Second
                return _second;
                _second = value;
                _secondSpecified = true;
        private int _second;
        private bool _secondSpecified;
        /// Allows the user to override the millisecond.
        [ValidateRange(0, 999)]
        public int Millisecond
                return _millisecond;
                _millisecond = value;
                _millisecondSpecified = true;
        private int _millisecond;
        private bool _millisecondSpecified;
        /// This option determines the default output format used to display the object get-date emits.
        public DisplayHintType DisplayHint { get; set; } = DisplayHintType.DateTime;
        /// Unix format string.
        [Parameter(ParameterSetName = DateAndUFormatParameterSet, Mandatory = true)]
        public string UFormat { get; set; }
        /// DotNet format string.
        [Parameter(ParameterSetName = DateAndFormatParameterSet)]
        [Parameter(ParameterSetName = UnixTimeSecondsAndFormatParameterSet)]
        [ArgumentCompletions("FileDate", "FileDateUniversal", "FileDateTime", "FileDateTimeUniversal")]
        public string Format { get; set; }
        /// Gets or sets a value that converts date to UTC before formatting.
        public SwitchParameter AsUTC { get; set; }
        #region methods
        /// Get the time.
            DateTime dateToUse = DateTime.Now;
            // use passed date object if specified
            if (_dateSpecified)
                dateToUse = Date;
            else if (_unixTimeSecondsSpecified)
                dateToUse = DateTimeOffset.FromUnixTimeSeconds(UnixTimeSeconds).LocalDateTime;
            // use passed year if specified
            if (_yearSpecified)
                offset = Year - dateToUse.Year;
                dateToUse = dateToUse.AddYears(offset);
            // use passed month if specified
            if (_monthSpecified)
                offset = Month - dateToUse.Month;
                dateToUse = dateToUse.AddMonths(offset);
            // use passed day if specified
            if (_daySpecified)
                offset = Day - dateToUse.Day;
                dateToUse = dateToUse.AddDays(offset);
            // use passed hour if specified
            if (_hourSpecified)
                offset = Hour - dateToUse.Hour;
                dateToUse = dateToUse.AddHours(offset);
            // use passed minute if specified
            if (_minuteSpecified)
                offset = Minute - dateToUse.Minute;
                dateToUse = dateToUse.AddMinutes(offset);
            // use passed second if specified
            if (_secondSpecified)
                offset = Second - dateToUse.Second;
                dateToUse = dateToUse.AddSeconds(offset);
            // use passed millisecond if specified
            if (_millisecondSpecified)
                offset = Millisecond - dateToUse.Millisecond;
                dateToUse = dateToUse.AddMilliseconds(offset);
                dateToUse = dateToUse.Subtract(TimeSpan.FromTicks(dateToUse.Ticks % 10000));
            if (AsUTC)
                dateToUse = dateToUse.ToUniversalTime();
            if (UFormat != null)
                // format according to UFormat string
                WriteObject(UFormatDateString(dateToUse));
            else if (Format != null)
                // format according to Format string
                // Special case built-in primitives: FileDate, FileDateTime.
                // These are the ISO 8601 "basic" formats, dropping dashes and colons
                // so that they can be used in file names
                if (string.Equals("FileDate", Format, StringComparison.OrdinalIgnoreCase))
                    Format = "yyyyMMdd";
                else if (string.Equals("FileDateUniversal", Format, StringComparison.OrdinalIgnoreCase))
                    Format = "yyyyMMddZ";
                else if (string.Equals("FileDateTime", Format, StringComparison.OrdinalIgnoreCase))
                    Format = "yyyyMMddTHHmmssffff";
                else if (string.Equals("FileDateTimeUniversal", Format, StringComparison.OrdinalIgnoreCase))
                    Format = "yyyyMMddTHHmmssffffZ";
                WriteObject(dateToUse.ToString(Format, CultureInfo.CurrentCulture));
                // output DateTime object wrapped in an PSObject with DisplayHint attached
                PSObject outputObj = new(dateToUse);
                PSNoteProperty note = new("DisplayHint", DisplayHint);
                outputObj.Properties.Add(note);
        /// This is more an implementation of the UNIX strftime.
        private string UFormatDateString(DateTime dateTime)
            StringBuilder sb = new();
            // folks may include the "+" as part of the format string
            if (UFormat[0] == '+')
                offset++;
            for (int i = offset; i < UFormat.Length; i++)
                if (UFormat[i] == '%')
                    switch (UFormat[i])
                            sb.Append("{0:dddd}");
                        case 'a':
                            sb.Append("{0:ddd}");
                            sb.Append("{0:MMMM}");
                        case 'b':
                            sb.Append("{0:MMM}");
                            sb.Append(dateTime.Year / 100);
                        case 'c':
                            sb.Append("{0:ddd} {0:dd} {0:MMM} {0:yyyy} {0:HH}:{0:mm}:{0:ss}");
                            sb.Append("{0:MM/dd/yy}");
                            sb.Append("{0:dd}");
                        case 'e':
                            sb.Append(StringUtil.Format("{0,2}", dateTime.Day));
                            sb.Append("{0:yyyy}-{0:MM}-{0:dd}");
                        case 'G':
                            sb.Append(StringUtil.Format("{0:0000}", ISOWeek.GetYear(dateTime)));
                        case 'g':
                            int isoYearWithoutCentury = ISOWeek.GetYear(dateTime) % 100;
                            sb.Append(StringUtil.Format("{0:00}", isoYearWithoutCentury));
                            sb.Append("{0:HH}");
                        case 'h':
                            sb.Append("{0:hh}");
                        case 'j':
                            sb.Append(StringUtil.Format("{0:000}", dateTime.DayOfYear));
                        case 'k':
                            sb.Append(StringUtil.Format("{0,2:0}", dateTime.Hour));
                        case 'l':
                            sb.Append("{0,2:%h}");
                            sb.Append("{0:mm}");
                        case 'm':
                            sb.Append("{0:MM}");
                            sb.Append('\n');
                        case 'p':
                            sb.Append("{0:tt}");
                            sb.Append("{0:HH:mm}");
                            sb.Append("{0:hh:mm:ss tt}");
                            sb.Append("{0:ss}");
                            sb.Append(StringUtil.Format("{0:0}", dateTime.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds));
                            sb.Append("{0:HH:mm:ss}");
                        case 't':
                            sb.Append('\t');
                            sb.Append(dateTime.DayOfYear / 7);
                            int dayOfWeek = dateTime.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)dateTime.DayOfWeek;
                            sb.Append(dayOfWeek);
                            sb.Append(StringUtil.Format("{0:00}", ISOWeek.GetWeekOfYear(dateTime)));
                        case 'w':
                            sb.Append((int)dateTime.DayOfWeek);
                        case 'x':
                            sb.Append("{0:yyyy}");
                        case 'y':
                            sb.Append("{0:yy}");
                            sb.Append("{0:zz}");
                            sb.Append(UFormat[i]);
                    // It's not a known format specifier, so just append it
            return StringUtil.Format(sb.ToString(), dateTime);
        private const string DateAndFormatParameterSet = "DateAndFormat";
        private const string DateAndUFormatParameterSet = "DateAndUFormat";
        private const string UnixTimeSecondsAndFormatParameterSet = "UnixTimeSecondsAndFormat";
        private const string UnixTimeSecondsAndUFormatParameterSet = "UnixTimeSecondsAndUFormat";
    #region DisplayHintType enum
    /// Display Hint type.
    public enum DisplayHintType
        /// Display preference Date-Only.
        Date,
        /// Display preference Time-Only.
        Time,
        /// Display preference Date and Time.
        DateTime
