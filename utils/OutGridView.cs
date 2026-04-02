    /// OutGridViewWindow definition for PowerShell command out-gridview.
    internal class OutGridViewWindow
        #region private Fields
        /// Window for gridView.
        private Window gridViewWindow;
        /// Local ManagementList.
        private ManagementList managementList;
        /// A collection of PSObjects to be data bound to the local Management List.
        private ObservableCollection<PSObject> listItems;
        /// Event used for the thread gridViewWindows signaling main thread after Windows loaded.
        private AutoResetEvent gridViewWindowLoaded;
        /// <summary>Is used to store any Management list calls exceptions.</summary>
        private Exception exception = null;
        /// Is used to block thread of the pipeline.
        private AutoResetEvent closedEvent;
        /// OK Button's content.
        private static readonly string OKButtonContent = XamlLocalizableResources.OutGridView_Button_OK;
        /// Cancel Button's content.
        private static readonly string CancelButtonContent = XamlLocalizableResources.OutGridView_Button_Cancel;
        /// Used to store selected items in the ok processing.
        private List<PSObject> selectedItems;
        /// The GUI thread of Out-GridView.
        private Thread guiThread;
        #endregion private Fields
        #region internal Constructors
        /// Constructor for OutGridView.
        internal OutGridViewWindow()
            // Initialize the data source collection.
            this.listItems = new ObservableCollection<PSObject>();
        #endregion internal Constructors
        #region private delegates
        /// ThreadDelegate definition.
        /// <param name="arg">Start GridView Window delegate.</param>
        private delegate void ThreadDelegate(object arg);
        #endregion private delegates
        #region Private method that are intended to be called by the Out-GridView cmdlet.
        /// Start a new thread as STA for gridView Window.
        /// <param name="invocation">Commands of the PowerShell.</param>
        /// <param name="outputModeOptions">Selection mode of the list.</param>
        /// <param name="closedEvent">ClosedEvent.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The method is called using reflection.")]
        private void StartWindow(string invocation, string outputModeOptions, AutoResetEvent closedEvent)
            this.closedEvent = closedEvent;
            this.gridViewWindowLoaded = new AutoResetEvent(false);
            ParameterizedThreadStart threadStart = new ParameterizedThreadStart(
               new ThreadDelegate(delegate
                       this.gridViewWindow = new Window();
                       this.managementList = CreateManagementList(outputModeOptions);
                       this.gridViewWindow.Loaded += this.GridViewWindowLoaded;
                       this.gridViewWindow.Content = CreateMainGrid(outputModeOptions);
                       this.gridViewWindow.Title = invocation;
                       this.gridViewWindow.Closed += this.GridViewWindowClosed;
                           this.gridViewWindow.CommandBindings.Add(new CommandBinding(plusSettings, ZoomEventHandlerPlus));
                           this.gridViewWindow.CommandBindings.Add(new CommandBinding(minusSettings, ZoomEventHandlerMinus));
                       this.gridViewWindow.ShowDialog();
                       // Store the exception in a local variable that will be checked later.
                       if (e.InnerException != null)
                           this.exception = e.InnerException;
                           this.exception = e;
               }));
            guiThread = new Thread(threadStart);
                this.zoomLevel += ZOOM_INCREMENT;
                Grid g = this.gridViewWindow.Content as Grid;
                if (g != null)
                    g.LayoutTransform = new ScaleTransform(this.zoomLevel, this.zoomLevel, 0, 0);
                this.zoomLevel -= ZOOM_INCREMENT;
        /// Creates a new ManagementList.
        /// <param name="outputMode">Output mode of the out-gridview.</param>
        /// <returns>A new ManagementList.</returns>
        private ManagementList CreateManagementList(string outputMode)
            ManagementList newList = new ManagementList();
            newList.ViewSaverUserActionState = UserActionState.Hidden;
            newList.ViewManagerUserActionState = UserActionState.Hidden;
            newList.List.VerticalAlignment = VerticalAlignment.Stretch;
            newList.List.SetValue(Grid.RowProperty, 0);
            newList.List.SelectionMode = (outputMode == "Single") ? SelectionMode.Single : SelectionMode.Extended;
            return newList;
        /// Creates a new main grid for window.
        /// <returns>A new mainGrid.</returns>
        private Grid CreateMainGrid(string outputMode)
            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition());
            mainGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            mainGrid.Children.Add(managementList);
            if (outputMode == "None")
                return mainGrid;
            // OK and Cancel button should only be displayed if OutputMode is not None.
            mainGrid.RowDefinitions[1].Height = GridLength.Auto;
            mainGrid.Children.Add(CreateButtonGrid());
        /// Creates a OK button.
        /// <returns>A new buttonGrid.</returns>
        private Grid CreateButtonGrid()
            Grid buttonGrid = new Grid();
            //// This will allow OK and Cancel to have the same width
            buttonGrid.SetValue(Grid.IsSharedSizeScopeProperty, true);
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition());
            buttonGrid.ColumnDefinitions[0].Width = GridLength.Auto;
            buttonGrid.ColumnDefinitions[0].SharedSizeGroup = "okCancel";
            buttonGrid.ColumnDefinitions[1].Width = GridLength.Auto;
            buttonGrid.ColumnDefinitions[1].SharedSizeGroup = "okCancel";
            buttonGrid.HorizontalAlignment = HorizontalAlignment.Right;
            buttonGrid.SetValue(Grid.RowProperty, 1);
            //// This will add OK and Cancel button to buttonGrid.
            buttonGrid.Children.Add(CreateOKButton());
            buttonGrid.Children.Add(CreateCancelButton());
            return buttonGrid;
        /// <returns>A new OK button.</returns>
        private Button CreateOKButton()
            Button ok = new Button();
            ok.Content = OKButtonContent;
            ok.Margin = new Thickness(5);
            ok.Padding = new Thickness(2);
            ok.SetValue(Grid.ColumnProperty, 0);
            ok.IsDefault = true;
            ok.SetValue(AutomationProperties.AutomationIdProperty, "OGVOK");
            ok.Click += OK_Click;
            return ok;
        /// Creates a Cancel button.
        /// <returns>A new Cancel button.</returns>
        private Button CreateCancelButton()
            Button cancel = new Button();
            cancel.Content = CancelButtonContent;
            cancel.Margin = new Thickness(5);
            cancel.Padding = new Thickness(2);
            cancel.SetValue(Grid.ColumnProperty, 1);
            cancel.IsCancel = true;
            cancel.SetValue(AutomationProperties.AutomationIdProperty, "OGVCancel");
            cancel.Click += Cancel_Click;
            return cancel;
        /// Store the selected items for use in EndProcessing.
            if (this.managementList.List.SelectedItems.Count != 0)
                this.selectedItems = new List<PSObject>();
                foreach (PSObject obj in this.managementList.List.SelectedItems)
                    this.selectedItems.Add(obj);
            this.gridViewWindow.Close();
        /// Closes the window.
        private void Cancel_Click(object sender, RoutedEventArgs e)
        /// Gets selected items from List.
        /// <returns>Selected items of the list.</returns>
        private List<PSObject> SelectedItems()
            return this.selectedItems;
        public void CloseWindow()
            this.gridViewWindow.Dispatcher.Invoke(new ThreadStart(delegate { this.gridViewWindow.Close(); }));
        /// Add column definitions to the underlying management list.
        /// <param name="propertyNames">An array of property names to add.</param>
        /// <param name="displayNames">An array of display names to add.</param>
        /// <param name="types">An array of types to add.</param>
        private void AddColumns(string[] propertyNames, string[] displayNames, Type[] types)
            // Wait for the gridViewWindow thread to signal that loading of Window is done
            if (this.gridViewWindowLoaded != null)
                this.gridViewWindowLoaded.WaitOne();
                this.gridViewWindowLoaded = null;
            this.managementList.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    () =>
                        // Pick the length of the shortest incoming arrays. Normally all incoming arrays should be of the same length.
                        int length = propertyNames.Length;
                        if (length > displayNames.Length)
                            length = displayNames.Length;
                        if (length > types.Length)
                            length = types.Length;
                            // Clear all columns in case the view is changed.
                            this.managementList.List.Columns.Clear();
                            // Clear column filter rules.
                            this.managementList.AddFilterRulePicker.ColumnFilterRules.Clear();
                            // Add columns with provided names and Types.
                            for (int i = 0; i < propertyNames.Length; ++i)
                                DataTemplate dataTemplate;
                                bool haveTemplate = this.managementList.FilterRulePanel.TryGetContentTemplate(types[i], out dataTemplate);
                                InnerListColumn column = null;
                                if (haveTemplate)
                                    column = new InnerListColumn(new UIPropertyGroupDescription(propertyNames[i], displayNames[i], types[i]));
                                    column = new InnerListColumn(new UIPropertyGroupDescription(propertyNames[i], displayNames[i], typeof(string)));
                                this.managementList.AddColumn(column);
                            this.managementList.List.SetColumnHeaderActions();
                            if (this.managementList.List.ItemsSource == null)
                                // Setting ItemsSource implicitly regenerates all columns.
                                this.managementList.List.ItemsSource = this.listItems;
                            // Set focus on ListView
                            this.managementList.List.SelectedIndex = 0;
                            this.managementList.List.Focus();
        /// Add an item to ObservableCollection.
        /// <param name="value">PSObject of comlet data.</param>
        private void AddItem(PSObject value)
            if (this.GetWindowClosedStatus())
            this.managementList.Dispatcher.BeginInvoke(
                            // Remove any potential ANSI decoration
                            foreach (var property in value.Properties)
                                if (property.Value is string str)
                                    StringDecorated decoratedString = new StringDecorated(str);
                                    property.Value = decoratedString.ToString(OutputRendering.PlainText);
                            this.listItems.Add(value);
        /// Returns the state of GridView Window.
        /// <returns>The status of GridView Window close or not.</returns>
        private bool GetWindowClosedStatus()
            if (this.closedEvent == null)
            return this.closedEvent.WaitOne(0);
        /// Returns any exception that has been thrown by previous method calls.
        /// <returns>The thrown and caught exception. It returns null if no exceptions were thrown by any previous method calls.</returns>
        private Exception GetLastException()
            Exception local = this.exception;
            if (local != null)
                // Clear the caught exception.
                this.exception = null;
                return local;
            return this.exception;
        #endregion Private method that are intended to be called by the Out-GridView cmdlet.
        #region Private methods
        /// GridView Window is closing callback process.
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Event Args.</param>
        private void GridViewWindowClosed(object sender, EventArgs e)
            if (this.closedEvent != null && !this.closedEvent.SafeWaitHandle.IsClosed)
                    this.closedEvent.Set();
                catch (ObjectDisposedException)
                    // we tried to avoid this exception with "&& !this.closedEvent.SafeWaitHandle.IsClosed"
                    // but since this runs in a different thread the if condition could be evaluated and after that
                    // the handle disposed
        /// Set loaded as true when this method invoked.
        /// <param name="e">RoutedEvent Args.</param>
        private void GridViewWindowLoaded(object sender, RoutedEventArgs e)
            // signal the main thread
            this.gridViewWindowLoaded.Set();
            // Make gridview window as active window
            this.gridViewWindow.Activate();
            // Set up AutomationId and Name
            AutomationProperties.SetName(this.gridViewWindow, GraphicalHostResources.OutGridViewWindowObjectName);
            AutomationProperties.SetAutomationId(this.gridViewWindow, "OutGridViewWindow");
        #endregion Private methods
