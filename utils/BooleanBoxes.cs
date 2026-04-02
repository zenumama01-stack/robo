    /// A class which returns the same boxed bool values.
    internal static class BooleanBoxes
        private static object trueBox = true;
        private static object falseBox = false;
        internal static object TrueBox
                return trueBox;
        internal static object FalseBox
                return falseBox;
        internal static object Box(bool value)
                return TrueBox;
                return FalseBox;
