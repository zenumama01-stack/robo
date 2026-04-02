    /// An EventArgs which holds the old and new values for a property change.
    /// <typeparam name="T">The property type.</typeparam>
    public class PropertyChangedEventArgs<T> : EventArgs
        /// Creates an instance of PropertyChangedEventArgs.
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new, current, value.</param>
        public PropertyChangedEventArgs(T oldValue, T newValue)
            this.OldValue = oldValue;
            this.NewValue = newValue;
        /// Gets the previous value for the property.
        public T OldValue
            private set;
        /// Gets the new value for the property.
        public T NewValue
