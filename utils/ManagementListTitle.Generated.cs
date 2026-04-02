    /// Provides a common control for displaying header information about a list.
    partial class ManagementListTitle
        public static readonly DependencyProperty ListProperty = DependencyProperty.Register( "List", typeof(ManagementList), typeof(ManagementListTitle), new PropertyMetadata( null, ListProperty_PropertyChanged) );
        /// Gets or sets the list this title is for. This is a dependency property.
        [Description("Gets or sets the list this title is for. This is a dependency property.")]
        public ManagementList List
                return (ManagementList) GetValue(ListProperty);
                SetValue(ListProperty,value);
            ManagementListTitle obj = (ManagementListTitle) o;
            obj.OnListChanged( new PropertyChangedEventArgs<ManagementList>((ManagementList)e.OldValue, (ManagementList)e.NewValue) );
        public event EventHandler<PropertyChangedEventArgs<ManagementList>> ListChanged;
        protected virtual void OnListChanged(PropertyChangedEventArgs<ManagementList> e)
        partial void OnListChangedImplementation(PropertyChangedEventArgs<ManagementList> e);
        // ListStatus dependency property
        /// Identifies the ListStatus dependency property.
        public static readonly DependencyProperty ListStatusProperty = DependencyProperty.Register( "ListStatus", typeof(string), typeof(ManagementListTitle), new PropertyMetadata( string.Empty, ListStatusProperty_PropertyChanged) );
        /// Gets the status of the list. This is a dependency property.
        [Description("Gets the status of the list. This is a dependency property.")]
        public string ListStatus
                return (string) GetValue(ListStatusProperty);
                SetValue(ListStatusProperty,value);
        static private void ListStatusProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnListStatusChanged( new PropertyChangedEventArgs<string>((string)e.OldValue, (string)e.NewValue) );
        /// Occurs when ListStatus property changes.
        public event EventHandler<PropertyChangedEventArgs<string>> ListStatusChanged;
        /// Called when ListStatus property changes.
        protected virtual void OnListStatusChanged(PropertyChangedEventArgs<string> e)
            OnListStatusChangedImplementation(e);
            RaisePropertyChangedEvent(ListStatusChanged, e);
        partial void OnListStatusChangedImplementation(PropertyChangedEventArgs<string> e);
        // Title dependency property
        /// Identifies the Title dependency property.
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register( "Title", typeof(string), typeof(ManagementListTitle), new PropertyMetadata( string.Empty, TitleProperty_PropertyChanged) );
        /// Gets or sets the title. This is a dependency property.
        [Description("Gets or sets the title. This is a dependency property.")]
        public string Title
                return (string) GetValue(TitleProperty);
                SetValue(TitleProperty,value);
        static private void TitleProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnTitleChanged( new PropertyChangedEventArgs<string>((string)e.OldValue, (string)e.NewValue) );
        /// Occurs when Title property changes.
        public event EventHandler<PropertyChangedEventArgs<string>> TitleChanged;
        /// Called when Title property changes.
        protected virtual void OnTitleChanged(PropertyChangedEventArgs<string> e)
            OnTitleChangedImplementation(e);
            RaisePropertyChangedEvent(TitleChanged, e);
        partial void OnTitleChangedImplementation(PropertyChangedEventArgs<string> e);
        // TotalItemCount dependency property
        /// Identifies the TotalItemCount dependency property.
        public static readonly DependencyProperty TotalItemCountProperty = DependencyProperty.Register( "TotalItemCount", typeof(int), typeof(ManagementListTitle), new PropertyMetadata( 0, TotalItemCountProperty_PropertyChanged) );
        /// Gets or sets the number of items in the list before filtering is applied. This is a dependency property.
        [Description("Gets or sets the number of items in the list before filtering is applied. This is a dependency property.")]
        public int TotalItemCount
                return (int) GetValue(TotalItemCountProperty);
                SetValue(TotalItemCountProperty,value);
        static private void TotalItemCountProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnTotalItemCountChanged( new PropertyChangedEventArgs<int>((int)e.OldValue, (int)e.NewValue) );
        /// Occurs when TotalItemCount property changes.
        public event EventHandler<PropertyChangedEventArgs<int>> TotalItemCountChanged;
        /// Called when TotalItemCount property changes.
        protected virtual void OnTotalItemCountChanged(PropertyChangedEventArgs<int> e)
            OnTotalItemCountChangedImplementation(e);
            RaisePropertyChangedEvent(TotalItemCountChanged, e);
        partial void OnTotalItemCountChangedImplementation(PropertyChangedEventArgs<int> e);
        static ManagementListTitle()
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ManagementListTitle), new FrameworkPropertyMetadata(typeof(ManagementListTitle)));
            return new ExtendedFrameworkElementAutomationPeer(this,AutomationControlType.StatusBar);
