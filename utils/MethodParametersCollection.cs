    /// Collection of method parameters and their arguments
    /// used to invoke a method in an object model wrapped by <see cref="CmdletAdapter&lt;TObjectInstance&gt;"/>
    internal sealed class MethodParametersCollection : KeyedCollection<string, MethodParameter>
        /// Creates an empty collection of method parameters.
        public MethodParametersCollection()
            : base(StringComparer.Ordinal, 5)
        /// Gets key for a method parameter.
        protected override string GetKeyForItem(MethodParameter item)
            return item.Name;
