    /// ArgBuilder which always produces null.
    internal sealed class NullArgBuilder : ArgBuilder
        internal NullArgBuilder() { }
            return Expression.Constant(null);
