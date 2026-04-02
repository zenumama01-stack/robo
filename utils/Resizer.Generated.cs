    /// Resizer adds a resizing grip and behavior to any control.
    ///     PART_LeftGrip - A required template part which must be of type Thumb.  The grip on the left.
    ///     PART_RightGrip - A required template part which must be of type Thumb.  The grip on the right.
    [TemplatePart(Name="PART_LeftGrip", Type=typeof(Thumb))]
    [TemplatePart(Name="PART_RightGrip", Type=typeof(Thumb))]
    partial class Resizer
        private Thumb leftGrip;
        private Thumb rightGrip;
        // DraggingTemplate dependency property
        /// Identifies the DraggingTemplate dependency property.
        public static readonly DependencyProperty DraggingTemplateProperty = DependencyProperty.Register( "DraggingTemplate", typeof(DataTemplate), typeof(Resizer), new PropertyMetadata( null, DraggingTemplateProperty_PropertyChanged) );
        /// Gets or sets the template used for the dragging indicator when ResizeWhileDragging is false.
        [Description("Gets or sets the template used for the dragging indicator when ResizeWhileDragging is false.")]
        public DataTemplate DraggingTemplate
                return (DataTemplate) GetValue(DraggingTemplateProperty);
                SetValue(DraggingTemplateProperty,value);
        static private void DraggingTemplateProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            Resizer obj = (Resizer) o;
            obj.OnDraggingTemplateChanged( new PropertyChangedEventArgs<DataTemplate>((DataTemplate)e.OldValue, (DataTemplate)e.NewValue) );
        /// Occurs when DraggingTemplate property changes.
        public event EventHandler<PropertyChangedEventArgs<DataTemplate>> DraggingTemplateChanged;
        /// Called when DraggingTemplate property changes.
        protected virtual void OnDraggingTemplateChanged(PropertyChangedEventArgs<DataTemplate> e)
            OnDraggingTemplateChangedImplementation(e);
            RaisePropertyChangedEvent(DraggingTemplateChanged, e);
        partial void OnDraggingTemplateChangedImplementation(PropertyChangedEventArgs<DataTemplate> e);
        // GripBrush dependency property
        /// Identifies the GripBrush dependency property.
        public static readonly DependencyProperty GripBrushProperty = DependencyProperty.Register( "GripBrush", typeof(Brush), typeof(Resizer), new PropertyMetadata( new SolidColorBrush(Colors.Black), GripBrushProperty_PropertyChanged) );
        /// Gets or sets the color of the resize grips.
        [Description("Gets or sets the color of the resize grips.")]
        public Brush GripBrush
                return (Brush) GetValue(GripBrushProperty);
                SetValue(GripBrushProperty,value);
        static private void GripBrushProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnGripBrushChanged( new PropertyChangedEventArgs<Brush>((Brush)e.OldValue, (Brush)e.NewValue) );
        /// Occurs when GripBrush property changes.
        public event EventHandler<PropertyChangedEventArgs<Brush>> GripBrushChanged;
        /// Called when GripBrush property changes.
        protected virtual void OnGripBrushChanged(PropertyChangedEventArgs<Brush> e)
            OnGripBrushChangedImplementation(e);
            RaisePropertyChangedEvent(GripBrushChanged, e);
        partial void OnGripBrushChangedImplementation(PropertyChangedEventArgs<Brush> e);
        // GripLocation dependency property
        /// Identifies the GripLocation dependency property.
        public static readonly DependencyProperty GripLocationProperty = DependencyProperty.Register( "GripLocation", typeof(ResizeGripLocation), typeof(Resizer), new PropertyMetadata( ResizeGripLocation.Right, GripLocationProperty_PropertyChanged) );
        /// Gets or sets a value of what grips.
        [Description("Gets or sets a value of what grips.")]
        public ResizeGripLocation GripLocation
                return (ResizeGripLocation) GetValue(GripLocationProperty);
                SetValue(GripLocationProperty,value);
        static private void GripLocationProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnGripLocationChanged( new PropertyChangedEventArgs<ResizeGripLocation>((ResizeGripLocation)e.OldValue, (ResizeGripLocation)e.NewValue) );
        /// Occurs when GripLocation property changes.
        public event EventHandler<PropertyChangedEventArgs<ResizeGripLocation>> GripLocationChanged;
        /// Called when GripLocation property changes.
        protected virtual void OnGripLocationChanged(PropertyChangedEventArgs<ResizeGripLocation> e)
            OnGripLocationChangedImplementation(e);
            RaisePropertyChangedEvent(GripLocationChanged, e);
        partial void OnGripLocationChangedImplementation(PropertyChangedEventArgs<ResizeGripLocation> e);
        // GripWidth dependency property
        /// Identifies the GripWidth dependency property.
        public static readonly DependencyProperty GripWidthProperty = DependencyProperty.Register( "GripWidth", typeof(double), typeof(Resizer), new PropertyMetadata( 4.0, GripWidthProperty_PropertyChanged) );
        /// Gets or sets the width of the grips.
        [Description("Gets or sets the width of the grips.")]
        public double GripWidth
                return (double) GetValue(GripWidthProperty);
                SetValue(GripWidthProperty,value);
        static private void GripWidthProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnGripWidthChanged( new PropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue) );
        /// Occurs when GripWidth property changes.
        public event EventHandler<PropertyChangedEventArgs<double>> GripWidthChanged;
        /// Called when GripWidth property changes.
        protected virtual void OnGripWidthChanged(PropertyChangedEventArgs<double> e)
            OnGripWidthChangedImplementation(e);
            RaisePropertyChangedEvent(GripWidthChanged, e);
        partial void OnGripWidthChangedImplementation(PropertyChangedEventArgs<double> e);
        // ResizeWhileDragging dependency property
        /// Identifies the ResizeWhileDragging dependency property.
        public static readonly DependencyProperty ResizeWhileDraggingProperty = DependencyProperty.Register( "ResizeWhileDragging", typeof(bool), typeof(Resizer), new PropertyMetadata( BooleanBoxes.TrueBox, ResizeWhileDraggingProperty_PropertyChanged) );
        /// Gets or sets a value indicating if resizing occurs while dragging.
        [Description("Gets or sets a value indicating if resizing occurs while dragging.")]
        public bool ResizeWhileDragging
                return (bool) GetValue(ResizeWhileDraggingProperty);
                SetValue(ResizeWhileDraggingProperty,BooleanBoxes.Box(value));
        static private void ResizeWhileDraggingProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnResizeWhileDraggingChanged( new PropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue) );
        /// Occurs when ResizeWhileDragging property changes.
        public event EventHandler<PropertyChangedEventArgs<bool>> ResizeWhileDraggingChanged;
        /// Called when ResizeWhileDragging property changes.
        protected virtual void OnResizeWhileDraggingChanged(PropertyChangedEventArgs<bool> e)
            OnResizeWhileDraggingChangedImplementation(e);
            RaisePropertyChangedEvent(ResizeWhileDraggingChanged, e);
        partial void OnResizeWhileDraggingChangedImplementation(PropertyChangedEventArgs<bool> e);
        // ThumbGripLocation dependency property
        /// Identifies the ThumbGripLocation dependency property.
        public static readonly DependencyProperty ThumbGripLocationProperty = DependencyProperty.RegisterAttached( "ThumbGripLocation", typeof(ResizeGripLocation), typeof(Resizer), new PropertyMetadata( ResizeGripLocation.Right, ThumbGripLocationProperty_PropertyChanged) );
        /// Gets the location for a grip.
        /// The value of ThumbGripLocation that is attached to element.
        static public ResizeGripLocation GetThumbGripLocation(DependencyObject element)
            return (ResizeGripLocation) element.GetValue(ThumbGripLocationProperty);
        /// Sets the location for a grip.
        static public void SetThumbGripLocation(DependencyObject element, ResizeGripLocation value)
            element.SetValue(ThumbGripLocationProperty,value);
        static private void ThumbGripLocationProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            ThumbGripLocationProperty_PropertyChangedImplementation(o, e);
        static partial void ThumbGripLocationProperty_PropertyChangedImplementation(DependencyObject o, DependencyPropertyChangedEventArgs e);
        // VisibleGripWidth dependency property
        /// Identifies the VisibleGripWidth dependency property.
        public static readonly DependencyProperty VisibleGripWidthProperty = DependencyProperty.Register( "VisibleGripWidth", typeof(double ), typeof(Resizer), new PropertyMetadata( 1.0, VisibleGripWidthProperty_PropertyChanged) );
        /// Gets or sets the visible width of the grips.
        [Description("Gets or sets the visible width of the grips.")]
        public double  VisibleGripWidth
                return (double ) GetValue(VisibleGripWidthProperty);
                SetValue(VisibleGripWidthProperty,value);
        static private void VisibleGripWidthProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnVisibleGripWidthChanged( new PropertyChangedEventArgs<double >((double )e.OldValue, (double )e.NewValue) );
        /// Occurs when VisibleGripWidth property changes.
        public event EventHandler<PropertyChangedEventArgs<double >> VisibleGripWidthChanged;
        /// Called when VisibleGripWidth property changes.
        protected virtual void OnVisibleGripWidthChanged(PropertyChangedEventArgs<double > e)
            OnVisibleGripWidthChangedImplementation(e);
            RaisePropertyChangedEvent(VisibleGripWidthChanged, e);
        partial void OnVisibleGripWidthChangedImplementation(PropertyChangedEventArgs<double > e);
            this.leftGrip = WpfHelp.GetTemplateChild<Thumb>(this,"PART_LeftGrip");
            this.rightGrip = WpfHelp.GetTemplateChild<Thumb>(this,"PART_RightGrip");
        static Resizer()
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Resizer), new FrameworkPropertyMetadata(typeof(Resizer)));
