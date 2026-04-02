    internal static partial class Clipboard
        private static bool? _clipboardSupported;
        // Used if an external clipboard is not available, e.g. if xclip is missing.
        // This is useful for testing in CI as well.
        private static string _internalClipboard;
        private static string StartProcess(
            string tool,
            string args,
            string stdin = "",
            bool readStdout = true)
            ProcessStartInfo startInfo = new();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.FileName = tool;
            startInfo.Arguments = args;
            string stdout = string.Empty;
            using (Process process = new())
                process.StartInfo = startInfo;
                    process.Start();
                catch (System.ComponentModel.Win32Exception)
                    _clipboardSupported = false;
                process.StandardInput.Write(stdin);
                process.StandardInput.Close();
                if (readStdout)
                    stdout = process.StandardOutput.ReadToEnd();
                process.WaitForExit(250);
                _clipboardSupported = process.ExitCode == 0;
            return stdout;
        public static string GetText()
            if (_clipboardSupported == false)
                return _internalClipboard ?? string.Empty;
            string tool = string.Empty;
            string args = string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                string clipboardText = string.Empty;
                ExecuteOnStaThread(() => GetTextImpl(out clipboardText));
                return clipboardText;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                tool = "xclip";
                args = "-selection clipboard -out";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                tool = "pbpaste";
            return StartProcess(tool, args);
        public static void SetText(string text)
                _internalClipboard = text;
                ExecuteOnStaThread(() => SetClipboardData(Tuple.Create(text, CF_UNICODETEXT)));
                    args = "-selection clipboard /dev/null";
                    args = "-selection clipboard -in";
                tool = "pbcopy";
            StartProcess(tool, args, text, readStdout: false);
        public static void SetRtf(string plainText, string rtfText)
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            if (s_CF_RTF == 0)
                s_CF_RTF = RegisterClipboardFormat("Rich Text Format");
            ExecuteOnStaThread(() => SetClipboardData(
                Tuple.Create(plainText, CF_UNICODETEXT),
                Tuple.Create(rtfText, s_CF_RTF)));
        private const uint GMEM_MOVEABLE = 0x0002;
        private const uint GMEM_ZEROINIT = 0x0040;
        private const uint GHND = GMEM_MOVEABLE | GMEM_ZEROINIT;
        [LibraryImport("kernel32.dll")]
        private static partial IntPtr GlobalAlloc(uint flags, UIntPtr dwBytes);
        private static partial IntPtr GlobalFree(IntPtr hMem);
        private static partial IntPtr GlobalLock(IntPtr hMem);
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GlobalUnlock(IntPtr hMem);
        [LibraryImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static partial void CopyMemory(IntPtr dest, IntPtr src, uint count);
        [LibraryImport("user32.dll")]
        private static partial bool IsClipboardFormatAvailable(uint format);
        private static partial bool OpenClipboard(IntPtr hWndNewOwner);
        private static partial bool CloseClipboard();
        private static partial bool EmptyClipboard();
        private static partial IntPtr GetClipboardData(uint format);
        private static partial IntPtr SetClipboardData(uint format, IntPtr data);
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial uint RegisterClipboardFormat(string lpszFormat);
        private const uint CF_TEXT = 1;
        private const uint CF_UNICODETEXT = 13;
        private static uint s_CF_RTF;
        private static bool GetTextImpl(out string text)
                if (IsClipboardFormatAvailable(CF_UNICODETEXT))
                    if (OpenClipboard(IntPtr.Zero))
                        var data = GetClipboardData(CF_UNICODETEXT);
                        if (data != IntPtr.Zero)
                            data = GlobalLock(data);
                            text = Marshal.PtrToStringUni(data);
                            GlobalUnlock(data);
                else if (IsClipboardFormatAvailable(CF_TEXT))
                        var data = GetClipboardData(CF_TEXT);
                            text = Marshal.PtrToStringAnsi(data);
                // Ignore exceptions
            text = string.Empty;
        private static bool SetClipboardData(params Tuple<string, uint>[] data)
                if (!OpenClipboard(IntPtr.Zero))
                EmptyClipboard();
                foreach (var d in data)
                    if (!SetSingleClipboardData(d.Item1, d.Item2))
        private static bool SetSingleClipboardData(string text, uint format)
            IntPtr hGlobal = IntPtr.Zero;
            IntPtr data = IntPtr.Zero;
                uint bytes;
                if (format == s_CF_RTF || format == CF_TEXT)
                    bytes = (uint)(text.Length + 1);
                    data = Marshal.StringToHGlobalAnsi(text);
                else if (format == CF_UNICODETEXT)
                    bytes = (uint)((text.Length + 1) * 2);
                    data = Marshal.StringToHGlobalUni(text);
                    // Not yet supported format.
                if (data == IntPtr.Zero)
                hGlobal = GlobalAlloc(GHND, (UIntPtr)bytes);
                if (hGlobal == IntPtr.Zero)
                IntPtr dataCopy = GlobalLock(hGlobal);
                if (dataCopy == IntPtr.Zero)
                CopyMemory(dataCopy, data, bytes);
                GlobalUnlock(hGlobal);
                if (SetClipboardData(format, hGlobal) != IntPtr.Zero)
                    // The clipboard owns this memory now, so don't free it.
                    hGlobal = IntPtr.Zero;
                // Ignore failures
                    Marshal.FreeHGlobal(data);
                if (hGlobal != IntPtr.Zero)
                    GlobalFree(hGlobal);
        private static void ExecuteOnStaThread(Func<bool> action)
            const int RetryCount = 5;
            int tries = 0;
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                while (tries++ < RetryCount && !action())
                    // wait until RetryCount or action
            var thread = new Thread(() =>
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (exception != null)
                throw exception;
