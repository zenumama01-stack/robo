    class ConsoleTextWriter : TextWriter
        ConsoleTextWriter(ConsoleHostUserInterface ui)
            : base(System.Globalization.CultureInfo.CurrentCulture)
            Dbg.Assert(ui != null, "ui needs a value");
        Encoding
        Write(string value)
            _ui.WriteToConsole(value, transcribeResult: true);
        Write(ReadOnlySpan<char> value)
        WriteLine(string value)
            _ui.WriteLineToConsole(value, transcribeResult: true);
        WriteLine(ReadOnlySpan<char> value)
        Write(bool b)
            if (b)
                _ui.WriteToConsole(bool.TrueString, transcribeResult: true);
                _ui.WriteToConsole(bool.FalseString, transcribeResult: true);
        Write(char c)
        Write(char[] a)
            _ui.WriteToConsole(a, transcribeResult: true);
