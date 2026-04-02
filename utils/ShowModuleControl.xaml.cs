    /// Control taht shows cmdlets in a module and details for a selected cmdlet.
    public partial class ShowModuleControl : UserControl
        /// Field used for the Owner parameter.
        private Window owner;
        /// Initializes a new instance of the ShowModuleControl class.
        public ShowModuleControl()
            // See comment in method summary to understand why this event is handled
            this.CommandList.PreviewMouseMove += this.CommandList_PreviewMouseMove;
            this.CommandList.SelectionChanged += this.CommandList_SelectionChanged;
        /// Gets or sets the owner of the container.
        public Window Owner
            get { return this.owner; }
            set { this.owner = value; }
        #region Events Handlers
        /// WPF has an interesting feature in list selection where if you hold the mouse button down,
        /// it will select the item under it, but if you keep the mouse button down and move the mouse
        /// (if the list supported drag and drop, the mouse action would be the same as dragging) it
        /// will select other list items.
        /// If the first selection change causes details for the item to be displayed and resizes the list,
        /// the selection can skip to another list item that happens to be over as the list got resized.
        /// In summary, resizing the list on selection can cause a selection bug. If the user selects an
        /// item in the end of the list the next item downwards can be selected.
        /// The WPF drag-and-select feature is not a standard win32 list behavior, and we can do without it
        /// since it causes this problem.
        /// WPF sets up this behavior by using a mouse capture. We undo the behavior in the handler below
        /// which removes the behavior.
        private void CommandList_PreviewMouseMove(object sender, MouseEventArgs e)
            if (this.CommandList.IsMouseCaptured)
                this.CommandList.ReleaseMouseCapture();
        /// Ensures the selected item is scrolled into view and that the list is focused.
        /// An item could be out of the view if the selection was changed in the object model
        private void CommandList_SelectionChanged(object sender, SelectionChangedEventArgs e)
            if (this.CommandList.SelectedItem == null)
            this.CommandList.ScrollIntoView(this.CommandList.SelectedItem);
        #endregion Events Handlers
