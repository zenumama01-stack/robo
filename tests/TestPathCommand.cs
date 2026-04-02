    /// The valid values for the -PathType parameter for test-path.
    public enum TestPathType
        /// If the item at the path exists, true will be returned.
        Any,
        /// If the item at the path exists and is a container, true will be returned.
        Container,
        /// If the item at the path exists and is not a container, true will be returned.
        Leaf
    /// A command to determine if an item exists at a specified path.
    [Cmdlet(VerbsDiagnostic.Test, "Path", DefaultParameterSetName = "Path", SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097057")]
    [OutputType(typeof(bool))]
    public class TestPathCommand : CoreCommandWithCredentialsBase
            get { return _paths; }
            set { _paths = value; }
        /// Gets or sets the isContainer property.
        public TestPathType PathType { get; set; } = TestPathType.Any;
        /// Gets or sets the IsValid parameter.
        public SwitchParameter IsValid { get; set; } = new SwitchParameter();
            if (!IsValid)
                if (Path != null && Path.Length > 0 && Path[0] != null)
                    result = InvokeProvider.Item.ItemExistsDynamicParameters(Path[0], context);
                    result = InvokeProvider.Item.ItemExistsDynamicParameters(".", context);
        /// The path to the item to ping.
        /// Determines if an item at the specified path exists.
                    new ArgumentNullException(TestPathResources.PathIsNullOrEmptyCollection),
                    "NullPathNotPermitted",
                    Path));
                if (string.IsNullOrWhiteSpace(path))
                    if (path is null)
                    if (IsValid)
                        result = SessionState.Path.IsValid(path, currentContext);
                        result = InvokeProvider.Item.Exists(path, currentContext);
                        if (this.PathType == TestPathType.Container)
                            result &= InvokeProvider.Item.IsContainer(path, currentContext);
                        else if (this.PathType == TestPathType.Leaf)
                            result &= !InvokeProvider.Item.IsContainer(path, currentContext);
                // Any of the known exceptions means the path does not exist.
