    /// Describes a property that has visual representation and can be sorted and grouped.
    public class UIPropertyGroupDescription : PropertyGroupDescription, INotifyPropertyChanged
        private ListSortDirection sortDirection = ListSortDirection.Ascending;
        /// Initializes a new instance of the <see cref="UIPropertyGroupDescription"/> class with the specified property name and display name.
        /// This constructor assumes that the type of data is <see cref="string"/>.
        /// <param name="propertyName">The name of the property that this instance describes.</param>
        /// <param name="displayName">The name displayed to users for this data.</param>
        public UIPropertyGroupDescription(string propertyName, string displayName)
            : this(propertyName, displayName, typeof(string))
            // This constructor just calls another constructor to default the data type to a string.
        /// Initializes a new instance of the <see cref="UIPropertyGroupDescription"/> class with the specified property name, display name and data type.
        /// <param name="dataType">The type of the data that this instance describes.</param>
        public UIPropertyGroupDescription(string propertyName, string displayName, Type dataType)
            : base(propertyName)
            this.DataType = dataType;
            this.DisplayName = displayName;
            this.DisplayContent = displayName;
            // Ignore case when sorting and grouping by default \\
            this.StringComparison = StringComparison.CurrentCultureIgnoreCase;
        /// Gets or sets the localizable display name representing the associated property.
        /// Gets or sets the display content representing the associated property.
        public object DisplayContent
        /// Gets or sets which direction the associated property should be sorted.
                return this.sortDirection;
                this.sortDirection = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs("SortDirection"));
        /// Gets or sets the type of the associated property.
        public Type DataType
        #region Methods
        /// Reverses the current sort direction.
        /// <returns>The new sort direction.</returns>
        public ListSortDirection ReverseSortDirection()
            if (this.SortDirection == ListSortDirection.Descending)
                this.SortDirection = ListSortDirection.Ascending;
                this.SortDirection = ListSortDirection.Descending;
            return this.SortDirection;
        #endregion Methods
            return this.PropertyName;
