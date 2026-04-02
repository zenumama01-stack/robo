    /// Routed event args which provide the ability to attach an
    /// arbitrary piece of data.
    /// <typeparam name="T">There are no restrictions on type T.</typeparam>
    public class DataRoutedEventArgs<T> : RoutedEventArgs
        private T data;
        /// Constructs a new instance of the DataRoutedEventArgs class.
        /// <param name="data">The data payload to be stored.</param>
        /// <param name="routedEvent">The routed event.</param>
        public DataRoutedEventArgs(T data, RoutedEvent routedEvent)
            this.data = data;
            this.RoutedEvent = routedEvent;
        /// Gets a value containing the data being stored.
        public T Data
            get { return this.data; }
