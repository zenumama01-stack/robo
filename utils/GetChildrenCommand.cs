    /// The get-childitem command class.
    /// This command lists the contents of a container.
    [Cmdlet(VerbsCommon.Get, "ChildItem", DefaultParameterSetName = "Items", SupportsTransactions = true, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2096492")]
    public class GetChildItemCommand : CoreCommandBase
        /// The string declaration for the Items parameter set in this command.
        /// The "Items" parameter set includes the following parameters:
        ///     -filter
        ///     -recurse
        private const string childrenSet = "Items";
        private const string literalChildrenSet = "LiteralItems";
        #region Command parameters
        /// Gets or sets the path for the operation.
        [Parameter(Position = 0, ParameterSetName = childrenSet,
                   ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Parameter(ParameterSetName = literalChildrenSet,
                return base.Filter;
                base.Filter = value;
                return base.Include;
                base.Include = value;
                return base.Exclude;
                base.Exclude = value;
        /// Gets or sets the recurse switch.
        [Alias("s", "r")]
        public SwitchParameter Recurse
                return _recurse;
                _recurse = value;
        /// Gets or sets max depth of recursion; automatically sets Recurse parameter;
        /// Value '0' will show only contents of container specified by -Path (same result as running 'Get-ChildItem' without '-Recurse');
        /// Value '1' will show 1 level deep, etc...;
        /// Default is uint.MaxValue - it performs full recursion (this parameter has no effect).
        public uint Depth
                return _depth;
                _depth = value;
                this.Recurse = true; // Bug 2391925 - Get-ChildItem -Depth should auto-set -Recurse
                return base.Force;
                base.Force = value;
        /// Gets or sets the names switch.
        public SwitchParameter Name
                return _childNames;
                _childNames = value;
            object result = null;
            string path = string.Empty;
            if (_paths != null && _paths.Length > 0)
                path = _paths[0];
                path = ".";
                case childrenSet:
                case literalChildrenSet:
                    if (Name)
                        result = InvokeProvider.ChildItem.GetChildNamesDynamicParameters(path, context);
                        result = InvokeProvider.ChildItem.GetChildItemsDynamicParameters(path, Recurse, context);
        #endregion Command parameters
        #region command data
        /// The path for the get-location operation.
        /// Determines if the command should do recursion.
        private bool _recurse;
        /// Limits the depth of recursion; used with Recurse parameter;
        private uint _depth = uint.MaxValue;
        /// The flag that specifies whether to retrieve the child names or the child items.
        private bool _childNames = false;
        #endregion command data
        #region command code
        /// The main execution method for the get-childitem command.
            if (_paths == null || _paths.Length == 0)
                _paths = new string[] { string.Empty };
            foreach (string path in _paths)
                                // Get the names of the child items using the static namespace method.
                                // The child names should be written directly to the pipeline using the
                                // context.WriteObject method.
                                InvokeProvider.ChildItem.GetNames(path, ReturnContainers.ReturnMatchingContainers, Recurse, Depth, currentContext);
                                // Get the children using the static namespace method.
                                // The children should be written directly to the pipeline using
                                // the context.WriteObject method.
                                InvokeProvider.ChildItem.Get(path, Recurse, Depth, currentContext);
                            "Only one of the specified parameter sets should be called.");
        #endregion command code
