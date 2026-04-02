    /// An Adorner which displays a given UIElement.
    partial class UIElementAdorner
        // Child dependency property
        /// Identifies the Child dependency property.
        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register( "Child", typeof(UIElement), typeof(UIElementAdorner), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions. AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure , ChildProperty_PropertyChanged) );
        /// Gets or sets the child element.
        [Description("Gets or sets the child element.")]
        public UIElement Child
                return (UIElement) GetValue(ChildProperty);
                SetValue(ChildProperty,value);
        static private void ChildProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            UIElementAdorner obj = (UIElementAdorner) o;
            obj.OnChildChanged( new PropertyChangedEventArgs<UIElement>((UIElement)e.OldValue, (UIElement)e.NewValue) );
        /// Occurs when Child property changes.
        public event EventHandler<PropertyChangedEventArgs<UIElement>> ChildChanged;
        /// Called when Child property changes.
        private void RaiseChildChanged(PropertyChangedEventArgs<UIElement> e)
            var eh = this.ChildChanged;
        protected virtual void OnChildChanged(PropertyChangedEventArgs<UIElement> e)
            OnChildChangedImplementation(e);
            RaiseChildChanged(e);
        partial void OnChildChangedImplementation(PropertyChangedEventArgs<UIElement> e);
