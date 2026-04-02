    /// Derives and extends GridViewColumn to add concepts such as column visibility..
    partial class InnerListColumn
        // DataDescription dependency property
        /// Identifies the DataDescription dependency property.
        public static readonly DependencyProperty DataDescriptionProperty = DependencyProperty.Register( "DataDescription", typeof(UIPropertyGroupDescription), typeof(InnerListColumn), new PropertyMetadata( null, DataDescriptionProperty_PropertyChanged) );
        /// Gets or sets the data description.
        [Description("Gets or sets the data description.")]
        public UIPropertyGroupDescription DataDescription
                return (UIPropertyGroupDescription) GetValue(DataDescriptionProperty);
                SetValue(DataDescriptionProperty,value);
        static private void DataDescriptionProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            InnerListColumn obj = (InnerListColumn) o;
            obj.OnDataDescriptionChanged( new PropertyChangedEventArgs<UIPropertyGroupDescription>((UIPropertyGroupDescription)e.OldValue, (UIPropertyGroupDescription)e.NewValue) );
        /// Called when DataDescription property changes.
        protected virtual void OnDataDescriptionChanged(PropertyChangedEventArgs<UIPropertyGroupDescription> e)
            OnDataDescriptionChangedImplementation(e);
            this.OnPropertyChanged(new PropertyChangedEventArgs("DataDescription"));
        partial void OnDataDescriptionChangedImplementation(PropertyChangedEventArgs<UIPropertyGroupDescription> e);
        // MinWidth dependency property
        /// Identifies the MinWidth dependency property.
        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register( "MinWidth", typeof(double), typeof(InnerListColumn), new PropertyMetadata( 20.0, MinWidthProperty_PropertyChanged), MinWidthProperty_ValidateProperty );
        /// Gets or sets a value that dictates the minimum allowable width of the column.
        [Description("Gets or sets a value that dictates the minimum allowable width of the column.")]
        public double MinWidth
                return (double) GetValue(MinWidthProperty);
                SetValue(MinWidthProperty,value);
        static private void MinWidthProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnMinWidthChanged( new PropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue) );
        /// Called when MinWidth property changes.
        protected virtual void OnMinWidthChanged(PropertyChangedEventArgs<double> e)
            OnMinWidthChangedImplementation(e);
            this.OnPropertyChanged(new PropertyChangedEventArgs("MinWidth"));
        partial void OnMinWidthChangedImplementation(PropertyChangedEventArgs<double> e);
        static private bool MinWidthProperty_ValidateProperty(object value)
            bool isValid = false;
            MinWidthProperty_ValidatePropertyImplementation((double) value, ref isValid);
            return isValid;
        static partial void MinWidthProperty_ValidatePropertyImplementation(double value, ref bool isValid);
        // Required dependency property
        /// Identifies the Required dependency property.
        public static readonly DependencyProperty RequiredProperty = DependencyProperty.Register( "Required", typeof(bool), typeof(InnerListColumn), new PropertyMetadata( BooleanBoxes.FalseBox, RequiredProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether the column may not be removed.
        [Description("Gets or sets a value indicating whether the column may not be removed.")]
        public bool Required
                return (bool) GetValue(RequiredProperty);
                SetValue(RequiredProperty,BooleanBoxes.Box(value));
        static private void RequiredProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnRequiredChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Called when Required property changes.
        protected virtual void OnRequiredChanged(PropertyChangedEventArgs<bool> e)
            OnRequiredChangedImplementation(e);
            this.OnPropertyChanged(new PropertyChangedEventArgs("Required"));
        partial void OnRequiredChangedImplementation(PropertyChangedEventArgs<bool> e);
        // Visible dependency property
        /// Identifies the Visible dependency property.
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register( "Visible", typeof(bool), typeof(InnerListColumn), new PropertyMetadata( BooleanBoxes.TrueBox, VisibleProperty_PropertyChanged) );
        /// Gets or sets a value indicating whether the columns we want to have available in the list.
        /// Modifying the Visible property does not in itself make the column visible or not visible.  This should always be kept in sync with the Columns property.
        [Description("Gets or sets a value indicating whether the columns we want to have available in the list.")]
        public bool Visible
                return (bool) GetValue(VisibleProperty);
                SetValue(VisibleProperty,BooleanBoxes.Box(value));
        static private void VisibleProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnVisibleChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Called when Visible property changes.
        protected virtual void OnVisibleChanged(PropertyChangedEventArgs<bool> e)
            OnVisibleChangedImplementation(e);
            this.OnPropertyChanged(new PropertyChangedEventArgs("Visible"));
        partial void OnVisibleChangedImplementation(PropertyChangedEventArgs<bool> e);
