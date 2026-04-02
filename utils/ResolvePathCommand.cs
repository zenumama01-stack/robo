    [Cmdlet(VerbsDiagnostic.Resolve, "Path", DefaultParameterSetName = "Path", SupportsTransactions = true,
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097143")]
    public class ResolvePathCommand : CoreCommandWithCredentialsBase
        /// Gets or sets the value that determines if the resolved path should
        /// be resolved to its relative version.
        [Parameter(ParameterSetName = "Path")]
        [Parameter(ParameterSetName = "LiteralPath")]
        public SwitchParameter Relative
                return _relative;
                _relative = value;
        private SwitchParameter _relative;
        /// Gets or sets the path the resolved relative path should be based off.
        public string RelativeBasePath
                return _relativeBasePath;
                _relativeBasePath = value;
        /// The path to resolve.
        private PSDriveInfo _relativeDrive;
        private string _relativeBasePath;
        /// Finds the path and drive that should be used for relative path resolution
        /// represents.
            if (!string.IsNullOrEmpty(RelativeBasePath))
                    _relativeBasePath = SessionState.Internal.Globber.GetProviderPath(RelativeBasePath, CmdletProviderContext, out _, out _relativeDrive);
                    ThrowTerminatingError(
                catch (ProviderInvocationException providerInvocation)
                            providerInvocation.ErrorRecord,
                            providerInvocation));
                catch (NotSupportedException notSupported)
                        new ErrorRecord(notSupported, "ProviderIsNotNavigationCmdletProvider", ErrorCategory.InvalidArgument, RelativeBasePath));
                catch (InvalidOperationException invalidOperation)
                        new ErrorRecord(invalidOperation, "InvalidHomeLocation", ErrorCategory.InvalidOperation, RelativeBasePath));
            else if (_relative)
                _relativeDrive = SessionState.Path.CurrentLocation.Drive;
                _relativeBasePath = SessionState.Path.CurrentLocation.ProviderPath;
        /// Resolves the path containing glob characters to the PowerShell paths that it
                    if (MyInvocation.BoundParameters.ContainsKey("RelativeBasePath"))
                        // Pushing and popping the location is done because GetResolvedPSPathFromPSPath uses the current path to resolve relative paths.
                        // It's important that we pop the location before writing an object to the pipeline to avoid affecting downstream commands.
                            SessionState.Path.PushCurrentLocation(string.Empty);
                            _ = SessionState.Path.SetLocation(_relativeBasePath);
                            result = SessionState.Path.GetResolvedPSPathFromPSPath(path, CmdletProviderContext);
                            _ = SessionState.Path.PopLocation(string.Empty);
                    if (_relative)
                        ReadOnlySpan<char> baseCache = null;
                        ReadOnlySpan<char> adjustedBaseCache = null;
                            // When result path and base path is on different PSDrive
                            // (../)*path should not go beyond the root of base path
                            if (currentPath.Drive != _relativeDrive &&
                                _relativeDrive != null &&
                                !currentPath.ProviderPath.StartsWith(_relativeDrive.Root, StringComparison.OrdinalIgnoreCase))
                                WriteObject(currentPath.Path, enumerateCollection: false);
                            int leafIndex = currentPath.Path.LastIndexOf(currentPath.Provider.ItemSeparator);
                            var basePath = currentPath.Path.AsSpan(0, leafIndex);
                            if (basePath == baseCache)
                                WriteObject(string.Concat(adjustedBaseCache, currentPath.Path.AsSpan(leafIndex + 1)), enumerateCollection: false);
                            baseCache = basePath;
                            string adjustedPath = SessionState.Path.NormalizeRelativePath(currentPath.Path, _relativeBasePath);
                            // Do not insert './' if result path is not relative
                            if (!adjustedPath.StartsWith(
                                    currentPath.Drive?.Root ?? currentPath.Path, StringComparison.OrdinalIgnoreCase) &&
                                !adjustedPath.StartsWith('.'))
                                adjustedPath = SessionState.Path.Combine(".", adjustedPath);
                            leafIndex = adjustedPath.LastIndexOf(currentPath.Provider.ItemSeparator);
                            adjustedBaseCache = adjustedPath.AsSpan(0, leafIndex + 1);
                            WriteObject(adjustedPath, enumerateCollection: false);
                if (!_relative)
                    WriteObject(result, enumerateCollection: true);
