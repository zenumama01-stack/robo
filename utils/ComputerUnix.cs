#if UNIX
#nullable enable
    /// Cmdlet to restart computer.
    [Cmdlet(VerbsLifecycle.Restart, "Computer", SupportsShouldProcess = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097060", RemotingCapability = RemotingCapability.SupportedByCommand)]
    public sealed class RestartComputerCommand : CommandLineCmdletBase
        // TODO: Support remote computers?
            if (InternalTestHooks.TestStopComputer)
                var retVal = InternalTestHooks.TestStopComputerResults;
                    string errMsg = StringUtil.Format("Command returned 0x{0:X}", retVal);
                    ErrorRecord error = new ErrorRecord(
                        new InvalidOperationException(errMsg), "CommandFailed", ErrorCategory.OperationStopped, "localhost");
            RunShutdown("-r now");
    public sealed class StopComputerCommand : CommandLineCmdletBase
            var args = "-P now";
            if (Platform.IsMacOS)
                args = "now";
            RunShutdown(args);
    /// A base class for cmdlets that can run shell commands.
    public class CommandLineCmdletBase : PSCmdlet, IDisposable
        private Process? _process = null;
        /// Releases all resources used by the <see cref="CommandLineCmdletBase"/>.
        /// Releases the unmanaged resources used by the <see cref="CommandLineCmdletBase"/>
        /// and optionally releases the managed resources.
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
                _process?.Dispose();
            if (_process == null) {
            try {
                if (!_process.HasExited) {
                    _process.Kill();
                WriteObject(_process.ExitCode);
            catch (InvalidOperationException) {}
            catch (NotSupportedException) {}
#region "Internals"
        private static string? shutdownPath;
        /// Run shutdown command.
        protected void RunShutdown(string args)
            if (shutdownPath is null)
                CommandInfo cmdinfo = CommandDiscovery.LookupCommandInfo(
                    "shutdown", CommandTypes.Application,
                    SearchResolutionOptions.None, CommandOrigin.Internal, this.Context);
                if (cmdinfo is not null)
                    shutdownPath = cmdinfo.Definition;
                        new InvalidOperationException(ComputerResources.ShutdownCommandNotFound), "CommandNotFound", ErrorCategory.ObjectNotFound, targetObject: null);
            _process = new Process()
                StartInfo = new ProcessStartInfo
                    FileName = shutdownPath,
                    Arguments = string.Empty,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
            _process.Start();
            _process.WaitForExit();
            if (_process.ExitCode != 0)
                string stderr = _process.StandardError.ReadToEnd();
                    new InvalidOperationException(stderr), "CommandFailed", ErrorCategory.OperationStopped, null);
