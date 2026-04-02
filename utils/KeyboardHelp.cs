    internal enum LogicalDirection
        None,
        Left,
        Right
    internal static class KeyboardHelp
        /// Gets the logical direction for a key, taking into account RTL settings.
        /// <param name="element">The element to get FlowDirection from.</param>
        /// <param name="key">The key pressed.</param>
        /// <returns>The logical direction.</returns>
        public static LogicalDirection GetLogicalDirection(DependencyObject element, Key key)
            bool rightToLeft = IsElementRightToLeft(element);
            switch (key)
                case Key.Right:
                    if (rightToLeft)
                        return LogicalDirection.Left;
                        return LogicalDirection.Right;
                case Key.Left:
                    return LogicalDirection.None;
        /// Gets the focus direction for a key, taking into account RTL settings.
        /// <returns>The focus direction.</returns>
        public static FocusNavigationDirection GetNavigationDirection(DependencyObject element, Key key)
            Debug.Assert(IsFlowDirectionKey(key));
                        return FocusNavigationDirection.Left;
                        return FocusNavigationDirection.Right;
                case Key.Down:
                    return FocusNavigationDirection.Down;
                case Key.Up:
                    return FocusNavigationDirection.Up;
                    Debug.Fail("Non-direction key specified");
                    return FocusNavigationDirection.First;
        /// Determines if the control key is pressed.
        /// <returns>True if a control is pressed.</returns>
        public static bool IsControlPressed()
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        /// Determines if the key is a navigation key.
        /// <returns>True if the key is a navigation key.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static bool IsFlowDirectionKey(Key key)
        private static bool IsElementRightToLeft(DependencyObject element)
            FlowDirection flowDirection = FrameworkElement.GetFlowDirection(element);
            bool rightToLeft = flowDirection == FlowDirection.RightToLeft;
            return rightToLeft;
