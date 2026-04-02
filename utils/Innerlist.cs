using System.Windows.Shapes;
    /// Partial class implementation for InnerList control.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public partial class InnerList : System.Windows.Controls.ListView
        #region internal fields
        /// The current ICollectionView being displayed.
        internal ICollectionView CollectionView;
        #endregion StyleCop Suppression - generated code
        #endregion internal fields
        #region private fields
        /// The current GridView.
        private InnerListGridView innerGrid;
        private InnerListColumn sortedColumn;
        /// ContextMenu for InnerList columns.
        private ContextMenu contextMenu;
        /// Private setter for <see cref="Columns"/>.
        private ObservableCollection<InnerListColumn> columns = new ObservableCollection<InnerListColumn>();
        /// Gets or sets whether the current items source is non-null and has items.
        private bool itemsSourceIsEmpty = false;
        #endregion private fields
        /// Initializes a new instance of this control.
        public InnerList()
            // This flag is needed to dramatically increase performance of scrolling \\
            VirtualizingStackPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
            AutomationProperties.SetAutomationId(this, "InnerList"); // No localization needed
        #endregion constructors
        ///  Register PropertyChangedEventHandler ItemSourcesPropertyChanged .
        public event PropertyChangedEventHandler ItemSourcesPropertyChanged;
        #region public properties
        /// Gets ItemsSource instead.
        /// <see cref="InnerList"/> Does not support adding to Items.
        public new ItemCollection Items
                return base.Items;
        /// Gets the column that is sorted, or <c>null</c> if no column is sorted.
        public InnerListColumn SortedColumn
                return this.sortedColumn;
        /// Gets InnerListGridView.
        public InnerListGridView InnerGrid
            get { return this.innerGrid; }
            protected set { this.innerGrid = value; }
        /// Gets the collection of columns that this list should display.
        public ObservableCollection<InnerListColumn> Columns
            get { return this.columns; }
        #endregion public properties
        #region public methods
        /// Causes the object to scroll into view.
        /// <param name="item">Object to scroll.</param>
        /// <remarks>This method overrides ListBox.ScrollIntoView(), which throws NullReferenceException when VirtualizationMode is set to Recycling.
        /// This implementation uses a workaround recommended by the WPF team.</remarks>
        public new void ScrollIntoView(object item)
            Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                (DispatcherOperationCallback)((arg) =>
                    if (this.IsLoaded)
                        base.ScrollIntoView(arg);
                }),
                item);
        /// Causes the object to scroll into view from the top, so that it tends to appear at the bottom of the scroll area.
        public void ScrollIntoViewFromTop(object item)
            if (this.Items.Count > 0)
                this.ScrollIntoView(this.Items[0]);
                this.ScrollIntoView(item);
        /// Updates the InnerGrid based upon the columns collection.
        public void RefreshColumns()
            this.UpdateView(this.ItemsSource);
        /// Sorts the list by the specified column. This has no effect if the list does not have a data source.
        /// <param name="column">
        /// The column to sort
        /// <param name="shouldScrollIntoView">
        /// Indicates whether the SelectedItem should be scrolled into view.
        public void ApplySort(InnerListColumn column, bool shouldScrollIntoView)
            ArgumentNullException.ThrowIfNull(column);
            // NOTE : By setting the column here, it will be used
            // later to set the sorted column when the UI state
            // is ready.
            this.sortedColumn = column;
            // If the list hasn't been populated, don't do anything \\
            if (this.CollectionView == null)
            this.UpdatePrimarySortColumn();
            using (this.CollectionView.DeferRefresh())
                ListCollectionView lcv = (ListCollectionView)this.CollectionView;
                lcv.CustomSort = new PropertyValueComparer(this.GetDescriptionsForSorting(), true, FilterRuleCustomizationFactory.FactoryInstance.PropertyValueGetter);
            if (shouldScrollIntoView && this.SelectedIndex > 0)
                this.ScrollIntoView(this.SelectedItem);
        private void UpdatePrimarySortColumn()
            foreach (InnerListColumn column in this.InnerGrid.AvailableColumns)
                bool isPrimarySortColumn = object.ReferenceEquals(this.sortedColumn, column);
                InnerList.SetIsPrimarySortColumn(column, isPrimarySortColumn);
                InnerList.SetIsPrimarySortColumn((GridViewColumnHeader)column.Header, isPrimarySortColumn);
        /// Gets a list of data descriptions for the columns that are not the primary sort column.
        /// <returns>A list of data descriptions for the columns that are not the primary sort column.</returns>
        private List<UIPropertyGroupDescription> GetDescriptionsForSorting()
            List<UIPropertyGroupDescription> dataDescriptions = new List<UIPropertyGroupDescription>();
            dataDescriptions.Add(this.SortedColumn.DataDescription);
            foreach (InnerListColumn column in this.InnerGrid.Columns)
                if (!object.ReferenceEquals(this.SortedColumn, column))
                    dataDescriptions.Add(column.DataDescription);
            return dataDescriptions;
        /// Clears the sort order from the list.
        public void ClearSort()
                this.sortedColumn = null;
                lcv.CustomSort = null;
            // If columns are shown, update them to show none are sorted \\
            if (this.InnerGrid != null)
        #endregion public methods
        #region internal methods
        #endregion internal methods
        #region protected methods
        /// Called when the ItemsSource changes to set internal fields, subscribe to the view change
        /// and possibly autopopulate columns.
        /// <param name="oldValue">Previous ItemsSource.</param>
        /// <param name="newValue">Current ItemsSource.</param>
        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
            base.OnItemsSourceChanged(oldValue, newValue);
            this.itemsSourceIsEmpty = this.ItemsSource != null && this.ItemsSource.GetEnumerator().MoveNext() == false;
            // A view can be created if there is data to auto-generate columns, or columns are added programmatically \\
            bool canCreateView = (this.ItemsSource != null) &&
                (this.itemsSourceIsEmpty == false || this.AutoGenerateColumns == false);
            if (canCreateView)
                this.UpdateViewAndCollectionView(this.ItemsSource);
                // If there are items, select the first item now \\
                this.SelectedIndex = this.itemsSourceIsEmpty ? -1 : 0;
                // Release the current inner grid \\
                this.ReleaseInnerGridReferences();
                // clean up old state if can not set the state.
                this.SetCollectionView(null);
                this.innerGrid = null;
                this.View = null;
        /// Called when ItemsChange to throw an exception indicating we don't support
        /// changing Items directly.
        /// <param name="e">Event parameters.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
            base.OnItemsChanged(e);
                // If the items source now has items, select the first item \\
                if (this.itemsSourceIsEmpty && this.Items.Count > 0)
                    this.SelectedIndex = 0;
                    this.itemsSourceIsEmpty = false;
                if (e.Action == NotifyCollectionChangedAction.Add)
                    if (this.InnerGrid == null)
        /// Called when a key is pressed while within the InnerList scope.
            if ((e.Key == Key.Left || e.Key == Key.Right) &&
                Keyboard.Modifiers == ModifierKeys.None)
                // If pressing Left or Right on a column header, move the focus \\
                GridViewColumnHeader header = e.OriginalSource as GridViewColumnHeader;
                if (header != null)
                    header.MoveFocus(new TraversalRequest(KeyboardHelp.GetNavigationDirection(this, e.Key)));
        #endregion protected methods
        #region static private methods
        /// Called when the View property is changed.
        /// <param name="obj">InnerList whose property is being changed.</param>
        private static void InnerList_OnViewChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
            InnerList thisList = (InnerList)obj;
            GridView newGrid = e.NewValue as GridView;
            InnerListGridView innerGrid = e.NewValue as InnerListGridView;
            if (newGrid != null && innerGrid == null)
                throw new NotSupportedException(string.Format(
                   InvariantResources.ViewSetWithType,
                   nameof(GridView),
                   nameof(InnerListGridView)));
            ((InnerList)obj).innerGrid = innerGrid;
        /// Gets the exception to be thrown when using Items.
        /// <returns>The exception to be thrown when using Items.</returns>
        private static NotSupportedException GetItemsException()
            return new NotSupportedException(
                    InvariantResources.NotSupportAddingToItems,
                    nameof(InnerList),
                    ItemsControl.ItemsSourceProperty.Name));
        #endregion static private methods
        #region instance private methods
        /// Called from OnItemsSourceChanged to set the collectionView field and
        /// subscribe to the collectionView changed event.
        /// <param name="newValue">ITemsSource passed to OnItemsSourceChanged.</param>
        private void SetCollectionView(System.Collections.IEnumerable newValue)
                this.CollectionView = null;
                CollectionViewSource newValueViewSource = newValue as CollectionViewSource;
                if (newValueViewSource != null && newValueViewSource.View != null)
                    this.CollectionView = newValueViewSource.View;
                    this.CollectionView = CollectionViewSource.GetDefaultView(newValue);
        /// Update View And CollectionView.
        /// <param name="value">InnerList object.</param>
        private void UpdateViewAndCollectionView(IEnumerable value)
            Debug.Assert(value != null, "value should be non-null");
            // SetCollectionView deals with a null newEnumerable
            this.SetCollectionView(value);
            this.UpdateView(value);
            // Generate property changed event.
            if (this.ItemSourcesPropertyChanged != null)
                this.ItemSourcesPropertyChanged(this, new PropertyChangedEventArgs("ItemsSource"));
        private void UpdateView(IEnumerable value)
            // NOTE : We need to clear the SortDescription before
            // clearing the InnerGrid.Columns so that the Adorners
            // are appropriately cleared.
            InnerListColumn sortedColumn = this.SortedColumn;
            this.ClearSort();
            if (this.AutoGenerateColumns)
                this.innerGrid = new InnerListGridView();
                // PopulateColumns deals with a null newEnumerable
                this.innerGrid.PopulateColumns(value);
                this.innerGrid = new InnerListGridView(this.Columns);
            this.View = this.innerGrid;
            this.SetColumnHeaderActions();
            if (sortedColumn != null && this.Columns.Contains(sortedColumn))
                this.ApplySort(sortedColumn, false);
        /// Releases all references to the current inner grid, if one exists.
        private void ReleaseInnerGridReferences()
            if (this.innerGrid != null)
                // Tell the inner grid to release its references \\
                this.innerGrid.ReleaseReferences();
                // Release the column headers \\
                foreach (InnerListColumn column in this.innerGrid.AvailableColumns)
                    GridViewColumnHeader header = column.Header as GridViewColumnHeader;
                        header.Click -= this.Header_Click;
                        header.PreviewKeyDown -= this.Header_KeyDown;
        /// Called when the ItemsSource changes, after SetGridview to add event handlers
        /// to the column header.
        internal void SetColumnHeaderActions()
            if (this.innerGrid == null)
            // set context menu
            this.innerGrid.ColumnHeaderContextMenu = this.GetListColumnsContextMenu();
            foreach (GridViewColumn column in this.innerGrid.AvailableColumns)
                // A string header needs an explicit GridViewColumnHeader
                // so we can hook up our events
                string headerString = column.Header as string;
                if (headerString != null)
                    GridViewColumnHeader columnHeader = new GridViewColumnHeader();
                    columnHeader.Content = headerString;
                    column.Header = columnHeader;
                    // header Click
                    header.Click += this.Header_Click;
                    header.PreviewKeyDown += this.Header_KeyDown;
                // If it is a GridViewColumnHeader we will not have the same nice sorting and grouping
                // capabilities
        #region ApplicationCommands.Copy
        partial void OnCopyCanExecuteImplementation(CanExecuteRoutedEventArgs e)
            e.CanExecute = this.SelectedItems.Count > 0;
        partial void OnCopyExecutedImplementation(ExecutedRoutedEventArgs e)
            string text = this.GetClipboardTextForSelectedItems();
            this.SetClipboardWithSelectedItemsText(text);
        #region Copy Helpers
        /// Gets a tab-delimited string representing the data of the selected rows.
        /// <returns>A tab-delimited string representing the data of the selected rows.</returns>
        protected internal string GetClipboardTextForSelectedItems()
            StringBuilder text = new StringBuilder();
            foreach (object value in this.Items)
                if (this.SelectedItems.Contains(value))
                    string entry = this.GetClipboardTextLineForSelectedItem(value);
                    text.AppendLine(entry);
            return text.ToString();
        private string GetClipboardTextLineForSelectedItem(object value)
            StringBuilder entryText = new StringBuilder();
                if (!FilterRuleCustomizationFactory.FactoryInstance.PropertyValueGetter.TryGetPropertyValue(column.DataDescription.PropertyName, value, out propertyValue))
                    propertyValue = string.Empty;
                entryText.Append(CultureInfo.CurrentCulture, $"{propertyValue}\t");
            return entryText.ToString();
        private void SetClipboardWithSelectedItemsText(string text)
            DataObject data = new DataObject(DataFormats.UnicodeText, text);
            Clipboard.SetDataObject(data);
        #endregion Copy Helpers
        #endregion ApplicationCommands.Copy
        /// Called to implement sorting functionality on column header pressed by space or enter key.
        /// <param name="sender">Typically a GridViewColumnHeader.</param>
        /// <param name="e">The event information.</param>
        private void Header_KeyDown(object sender, KeyEventArgs e)
            if (e.Key != Key.Space && e.Key != Key.Enter)
            // Call HeaderActionProcess when space or enter key pressed
            this.HeaderActionProcess(sender);
        /// Called to implement sorting functionality on column header click.
        private void Header_Click(object sender, RoutedEventArgs e)
            // Call HeaderActionProcess when mouse clicked on the header
        /// Called to implement sorting functionality.
        private void HeaderActionProcess(object sender)
            GridViewColumnHeader header = (GridViewColumnHeader)sender;
            InnerListColumn column = (InnerListColumn)header.Column;
            UIPropertyGroupDescription dataDescription = column.DataDescription;
            if (dataDescription == null)
            // If the sorted column is sorted again, reverse the sort \\
            if (object.ReferenceEquals(column, this.sortedColumn))
                dataDescription.ReverseSortDirection();
            this.ApplySort(column, true);
        /// Create default Context Menu.
        /// <returns>ContextMenu of List Columns.</returns>
        private ContextMenu GetListColumnsContextMenu()
            this.contextMenu = new ContextMenu();
            // Add Context Menu item.
            this.SetColumnPickerContextMenuItem();
            return this.contextMenu;
        /// Set up context menu item for Column Picker feature.
        /// <returns>True if it is successfully set up.</returns>
        private bool SetColumnPickerContextMenuItem()
            MenuItem columnPicker = new MenuItem();
            AutomationProperties.SetAutomationId(columnPicker, "ChooseColumns");
            columnPicker.Header = UICultureResources.ColumnPicker;
            columnPicker.Click += this.innerGrid.OnColumnPicker;
            this.contextMenu.Items.Add(columnPicker);
            // Adds notification for the View changing
            ListView.ViewProperty.OverrideMetadata(
                typeof(InnerList),
                new PropertyMetadata(new PropertyChangedCallback(InnerList_OnViewChanged)));
        #endregion instance private methods
