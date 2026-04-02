    /// Picker control that displays a list with basic editing functionality.
    public partial class ListOrganizer : ContentControl
        /// Creates a new instance of the ListOrganizer class.
        public ListOrganizer()
            // empty
        /// Prevents keyboard focus from leaving the dropdown.
        /// <param name="e">The event args.</param>
        protected override void OnKeyDown(KeyEventArgs e)
            if (e.Key == Key.Up ||
                e.Key == Key.Down ||
                e.Key == Key.Left ||
                e.Key == Key.Right)
        partial void OnSelectItemExecutedImplementation(ExecutedRoutedEventArgs e)
            if (e.Parameter == null)
                throw new ArgumentException("e.Parameter is null", "e");
            this.RaiseEvent(new DataRoutedEventArgs<object>(e.Parameter, ItemSelectedEvent));
            this.picker.IsOpen = false;
        partial void OnDeleteItemExecutedImplementation(ExecutedRoutedEventArgs e)
            this.RaiseEvent(new DataRoutedEventArgs<object>(e.Parameter, ItemDeletedEvent));
