namespace Microsoft.PowerShell.GlobalTool.Shim
    /// Shim layer to chose the appropriate runtime for PowerShell DotNet Global tool.
    public static class EntryPoint
        private const string PwshDllName = "pwsh.dll";
        private const string WinFolderName = "win";
        private const string UnixFolderName = "unix";
        /// Entry point for the global tool.
        /// <param name="args">Arguments passed to the global tool.</param>'
        /// <returns>Exit code returned by pwsh.</returns>
            var currentPath = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName;
            var isWindows = OperatingSystem.IsWindows();
            string platformFolder = isWindows ? WinFolderName : UnixFolderName;
            var arguments = new List<string>(args.Length + 1);
            var pwshPath = Path.Combine(currentPath, platformFolder, PwshDllName);
            arguments.Add(pwshPath);
            arguments.AddRange(args);
            if (File.Exists(pwshPath))
                Console.CancelKeyPress += (sender, e) =>
                    e.Cancel = true;
                var process = System.Diagnostics.Process.Start("dotnet", arguments);
                process.WaitForExit();
                return process.ExitCode;
                throw new FileNotFoundException(pwshPath);
