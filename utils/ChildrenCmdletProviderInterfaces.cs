    /// Exposes the Children noun of the Cmdlet Providers to the Cmdlet base class. The methods of this class
    /// use the providers to perform operations.
    public sealed class ChildItemCmdletProviderIntrinsics
        /// Hide the default constructor since we always require an instance of SessionState.
        private ChildItemCmdletProviderIntrinsics()
                "This constructor should never be called. Only the constructor that takes an instance of SessionState should be called.");
        /// Constructs a facade over the "real" session state API.
        /// An instance of the cmdlet that this class is acting as a facade for.
        internal ChildItemCmdletProviderIntrinsics(Cmdlet cmdlet)
                throw PSTraceSource.NewArgumentNullException(nameof(cmdlet));
            _sessionState = cmdlet.Context.EngineSessionState;
        /// <param name="sessionState">
        /// An instance of the "real" session state.
        /// If <paramref name="sessionState"/> is null.
        internal ChildItemCmdletProviderIntrinsics(SessionStateInternal sessionState)
            if (sessionState == null)
                throw PSTraceSource.NewArgumentNullException(nameof(sessionState));
            _sessionState = sessionState;
        #region Public methods
        #region GetChildItems
        /// Gets the child items of the container at the given path.
        /// The path to the item to retrieve. It may be a drive or provider-qualified path and may include
        /// glob characters.
        /// If true, gets all the children in all the sub-containers of the specified
        /// container. If false, only gets the immediate children of the specified
        /// container.
        /// The children of the container at the specified path. The type of the objects returned are
        /// determined by the provider that supports the given path.
        /// If <paramref name="path"/> or <paramref name="context"/> is null.
        /// <exception cref="ProviderNotFoundException">
        /// If the <paramref name="path"/> refers to a provider that could not be found.
        /// <exception cref="DriveNotFoundException">
        /// If the <paramref name="path"/> refers to a drive that could not be found.
        /// <exception cref="ItemNotFoundException">
        /// If <paramref name="path"/> does not contain glob characters and
        /// could not be found.
        /// If the provider that the <paramref name="path"/> refers to does
        /// not support this operation.
        /// If the provider threw an exception.
        public Collection<PSObject> Get(string path, bool recurse)
                _sessionState != null,
                "The only constructor for this class should always set the sessionState field");
            // Parameter validation is done in the session state object
            return _sessionState.GetChildItems(new string[] { path }, recurse, uint.MaxValue, false, false);
        /// Gets the child items of the container at the given path(s).
        /// The path(s) to the item(s) to retrieve. They may be drive or provider-qualified paths and may include
        /// Limits the depth of recursion; uint.MaxValue performs full recursion.
        /// Passed on to providers to force operations.
        /// <param name="literalPath">
        /// If true, globbing is not done on paths.
        public Collection<PSObject> Get(string[] path, bool recurse, uint depth, bool force, bool literalPath)
            return _sessionState.GetChildItems(path, recurse, depth, force, literalPath);
        public Collection<PSObject> Get(string[] path, bool recurse, bool force, bool literalPath)
            return this.Get(path, recurse, uint.MaxValue, force, literalPath);
        /// Nothing. The children of the container at the specified path are written to the context.
        internal void Get(
            uint depth,
            CmdletProviderContext context)
            _sessionState.GetChildItems(path, recurse, depth, context);
        /// Gets the dynamic parameters for the get-childitem cmdlet.
        /// The path to the item if it was specified on the command line.
        /// The context which the core command is running.
        internal object GetChildItemsDynamicParameters(
            return _sessionState.GetChildItemsDynamicParameters(path, recurse, context);
        #endregion GetChildItems
        #region GetChildNames
        /// Gets the child names of the container at the given path.
        /// If true, gets all the relative paths of all the children
        /// in all the sub-containers of the specified
        /// container. If false, only gets the immediate child names of the specified
        /// If <paramref name="path"/> or <paramref name="propertyToClear"/> is null.
        public Collection<string> GetNames(
            return _sessionState.GetChildNames(new string[] { path }, returnContainers, recurse, uint.MaxValue, false, false);
            string[] path,
            bool literalPath)
            return _sessionState.GetChildNames(path, returnContainers, recurse, uint.MaxValue, force, literalPath);
            return _sessionState.GetChildNames(path, returnContainers, recurse, depth, force, literalPath);
        /// Nothing.  The names of the children of the specified container are written to the context.
        internal void GetNames(
            _sessionState.GetChildNames(path, returnContainers, recurse, depth, context);
        /// Gets the dynamic parameters for the get-childitem -name cmdlet.
        internal object GetChildNamesDynamicParameters(
            return _sessionState.GetChildNamesDynamicParameters(path, context);
        #endregion GetChildNames
        #region HasChildItems
        /// Determines if an item at the given path has children.
        /// The path to the item to determine if it has children. It may be a drive or provider-qualified path and may include
        /// True if the item at the specified path has children. False otherwise.
        /// If <paramref name="path"/> is null.
        public bool HasChild(string path)
            return _sessionState.HasChildItems(path, false, false);
        public bool HasChild(string path, bool force, bool literalPath)
            return _sessionState.HasChildItems(path, force, literalPath);
        internal bool HasChild(
            return _sessionState.HasChildItems(path, context);
        #endregion HasChildItems
        #endregion Public methods
        private readonly Cmdlet _cmdlet;
        private readonly SessionStateInternal _sessionState;
    /// This enum determines which types of containers are returned from some of
    /// the provider methods.
    public enum ReturnContainers
        /// Only containers that match the filter(s) are returned.
        ReturnMatchingContainers,
        /// All containers are returned even if they don't match the filter(s).
        ReturnAllContainers
