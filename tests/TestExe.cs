namespace TestExe
    internal enum EnvTarget
        System = 2,
    internal sealed class TestExe
        private static int Main(string[] args)
            if (args.Length > 0)
                switch (args[0].ToLowerInvariant())
                    case "-echoargs":
                        EchoArgs(args);
                    case "-echocmdline":
                        EchoCmdLine();
                    case "-createchildprocess":
                        CreateChildProcess(args);
                    case "-returncode":
                        // Used to test functionality depending on $LASTEXITCODE, like &&/|| operators
                        Console.WriteLine(args[1]);
                        return int.Parse(args[1]);
                    case "-stderr":
                        Console.Error.WriteLine(args[1]);
                    case "-stderrandout":
                        Console.Error.WriteLine(new string(args[1].ToCharArray().Reverse().ToArray()));
                    case "-readbytes":
                        ReadBytes();
                    case "-writebytes":
                        WriteBytes(args.AsSpan()[1..]);
                    case "-updateuserpath":
                        UpdateEnvPath(EnvTarget.User);
                    case "-updatesystempath":
                        UpdateEnvPath(EnvTarget.System);
                    case "-updateuserandsystempath":
                        UpdateEnvPath(EnvTarget.User | EnvTarget.System);
                    case "--help":
                    case "-h":
                        PrintHelp();
                        exitCode = 1;
                        Console.Error.WriteLine("Unknown test {0}. Run with '-h' for help.", args[0]);
                Console.Error.WriteLine("Test not specified");
        private static void WriteBytes(ReadOnlySpan<string> args)
            using Stream stdout = Console.OpenStandardOutput();
                if (!byte.TryParse(arg, NumberStyles.AllowHexSpecifier, provider: null, out byte value))
                        nameof(args),
                        "All args after -writebytes must be single byte hex strings.");
                stdout.WriteByte(value);
        [SkipLocalsInit]
        private static void ReadBytes()
            using Stream stdin = Console.OpenStandardInput();
            Span<byte> buffer = stackalloc byte[0x200];
            Unsafe.InitBlock(ref MemoryMarshal.GetReference(buffer), 0, 0x200);
            Span<char> hex = stackalloc char[] { '\0', '\0' };
                int received = stdin.Read(buffer);
                if (received is 0)
                for (int i = 0; i < received; i++)
                    buffer[i].TryFormat(hex, out _, "X2");
                    Console.Out.WriteLine(hex);
        // <Summary>
        // Echos back to stdout the arguments passed in
        // </Summary>
        private static void EchoArgs(string[] args)
                Console.WriteLine("Arg {0} is <{1}>", i - 1, args[i]);
        // Echos the raw command line received by the process plus the arguments passed in.
        private static void EchoCmdLine()
            string rawCmdLine = "N/A";
                nint cmdLinePtr = Interop.GetCommandLineW();
                rawCmdLine = Marshal.PtrToStringUni(cmdLinePtr);
            Console.WriteLine(rawCmdLine);
        // Print help content.
        private static void PrintHelp()
            const string Content = @"
Options for echoing args are:
   -echoargs     Echos back to stdout the arguments passed in.
   -echocmdline  Echos the raw command line received by the process.
Other options are for specific tests only. Read source code for details.
            Console.WriteLine(Content);
        // First argument is the number of child processes to create which are instances of itself
        // Processes automatically exit after 100 seconds
        private static void CreateChildProcess(string[] args)
                uint num = uint.Parse(args[1]);
                for (uint i = 0; i < num; i++)
                    Process child = new Process();
                    child.StartInfo.FileName = Environment.ProcessPath;
                    child.StartInfo.Arguments = "-createchildprocess";
                    child.Start();
            // sleep is needed so the process doesn't exit before the test case kill it
            Thread.Sleep(100000);
        private static void UpdateEnvPath(EnvTarget target)
            if (!OperatingSystem.IsWindows())
            const string EnvVarName = "Path";
            const string UserEnvRegPath = "Environment";
            const string SysEnvRegPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
            if (target.HasFlag(EnvTarget.User))
                // Append to the User Path.
                using RegistryKey reg = Registry.CurrentUser.OpenSubKey(UserEnvRegPath, writable: true);
                UpdateEnvPathImpl(reg, append: true, @"X:\not-exist-user-path");
            if (target.HasFlag(EnvTarget.System))
                // Prepend to the System Path.
                using RegistryKey reg = Registry.LocalMachine.OpenSubKey(SysEnvRegPath, writable: true);
                UpdateEnvPathImpl(reg, append: false, @"X:\not-exist-sys-path");
            static void UpdateEnvPathImpl(RegistryKey regKey, bool append, string newPathItem)
                // Get the registry value kind.
                RegistryValueKind kind = regKey.GetValueKind(EnvVarName);
                // Get the literal registry value (not expanded) for the env var.
                string oldValue = (string)regKey.GetValue(
                    EnvVarName,
                    defaultValue: string.Empty,
                    RegistryValueOptions.DoNotExpandEnvironmentNames);
                string newValue;
                    // Append to the old value.
                    string separator = (oldValue is "" || oldValue.EndsWith(';')) ? string.Empty : ";";
                    newValue = $"{oldValue}{separator}{newPathItem}";
                    // Prepend to the old value.
                    string separator = (oldValue is "" || oldValue.StartsWith(';')) ? string.Empty : ";";
                    newValue = $"{newPathItem}{separator}{oldValue}";
                // Set the new value and preserve the original value kind.
                regKey.SetValue(EnvVarName, newValue, kind);
        internal static partial nint GetCommandLineW();
