    /// Implementation for the set-date command.
    [Cmdlet(VerbsCommon.Set, "Date", DefaultParameterSetName = "Date", SupportsShouldProcess = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097133")]
    [OutputType(typeof(DateTime))]
    public sealed class SetDateCommand : PSCmdlet
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Date", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public DateTime Date { get; set; }
        /// Allows a use to specify a timespan with which to apply to the current time.
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = "Adjust", ValueFromPipelineByPropertyName = true)]
        public TimeSpan Adjust { get; set; }
        /// This option determines the default output format used to display the object set-date emits.
        /// Set the date.
            DateTime dateToUse;
                case "Adjust":
                    dateToUse = DateTime.Now.Add(Adjust);
                    goto case "Date";
            if (ShouldProcess(dateToUse.ToString()))
                // We are not validating the native call here.
                // We just want to be sure that we're using the value the user provided us.
                if (Dbg.Internal.InternalTestHooks.SetDate)
                    WriteObject(dateToUse);
                else if (!Platform.NonWindowsSetDate(dateToUse))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                // build up the SystemTime struct to pass to SetSystemTime
                NativeMethods.SystemTime systemTime = new();
                systemTime.Year = (ushort)dateToUse.Year;
                systemTime.Month = (ushort)dateToUse.Month;
                systemTime.Day = (ushort)dateToUse.Day;
                systemTime.Hour = (ushort)dateToUse.Hour;
                systemTime.Minute = (ushort)dateToUse.Minute;
                systemTime.Second = (ushort)dateToUse.Second;
                systemTime.Milliseconds = (ushort)dateToUse.Millisecond;
#pragma warning disable 56523
                    WriteObject(systemTime);
                    if (!NativeMethods.SetLocalTime(ref systemTime))
                    // MSDN says to call this twice to account for changes
                    // between DST
#pragma warning restore 56523
            // If we've turned on the SetDate test hook, don't emit the output object here because we emitted it earlier.
            if (!Dbg.Internal.InternalTestHooks.SetDate)
        #region nativemethods
                public ushort Year;
                public ushort Month;
                public ushort DayOfWeek;
                public ushort Day;
                public ushort Hour;
                public ushort Minute;
                public ushort Second;
                public ushort Milliseconds;
            [DllImport(PinvokeDllNames.SetLocalTimeDllName, SetLastError = true)]
            public static extern bool SetLocalTime(ref SystemTime systime);
