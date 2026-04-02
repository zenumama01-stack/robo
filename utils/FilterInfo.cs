    /// Provides information about a filter that is stored in session state.
    public class FilterInfo : FunctionInfo
        /// Creates an instance of the FilterInfo class with the specified name and ScriptBlock.
        /// The name of the filter.
        /// <param name="filter">
        /// The ScriptBlock for the filter
        /// The ExecutionContext for the filter.
        /// If <paramref name="filter"/> is null.
        internal FilterInfo(string name, ScriptBlock filter, ExecutionContext context) : this(name, filter, context, null)
        /// The help file for the filter.
        internal FilterInfo(string name, ScriptBlock filter, ExecutionContext context, string helpFile)
            : base(name, filter, context, helpFile)
            SetCommandType(CommandTypes.Filter);
        /// The execution context for the filter.
        internal FilterInfo(string name, ScriptBlock filter, ScopedItemOptions options, ExecutionContext context) : this(name, filter, options, context, null)
        internal FilterInfo(string name, ScriptBlock filter, ScopedItemOptions options, ExecutionContext context, string helpFile)
            : base(name, filter, options, context, helpFile)
        internal FilterInfo(FilterInfo other)
        internal FilterInfo(string name, FilterInfo other)
            FilterInfo copy = new FilterInfo(this);
            get { return HelpCategory.Filter; }
