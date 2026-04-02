    /// Represents the source of an image that can render as a vector or as a bitmap.
    partial class ScalableImageSource
        // AccessibleName dependency property
        /// Identifies the AccessibleName dependency property.
        public static readonly DependencyProperty AccessibleNameProperty = DependencyProperty.Register( "AccessibleName", typeof(string), typeof(ScalableImageSource), new PropertyMetadata( null, AccessibleNameProperty_PropertyChanged) );
        /// Gets or sets the accessible name of the image.  This is used by accessibility clients to describe the image, and must be localized.
        [Description("Gets or sets the accessible name of the image.  This is used by accessibility clients to describe the image, and must be localized.")]
        public string AccessibleName
                return (string) GetValue(AccessibleNameProperty);
                SetValue(AccessibleNameProperty,value);
        static private void AccessibleNameProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            ScalableImageSource obj = (ScalableImageSource) o;
            obj.OnAccessibleNameChanged( new PropertyChangedEventArgs<string>((string)e.OldValue, (string)e.NewValue) );
        /// Occurs when AccessibleName property changes.
        public event EventHandler<PropertyChangedEventArgs<string>> AccessibleNameChanged;
        /// Called when AccessibleName property changes.
        protected virtual void OnAccessibleNameChanged(PropertyChangedEventArgs<string> e)
            OnAccessibleNameChangedImplementation(e);
            RaisePropertyChangedEvent(AccessibleNameChanged, e);
        partial void OnAccessibleNameChangedImplementation(PropertyChangedEventArgs<string> e);
        // Brush dependency property
        /// Identifies the Brush dependency property.
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register( "Brush", typeof(Brush), typeof(ScalableImageSource), new PropertyMetadata( null, BrushProperty_PropertyChanged) );
        /// Gets or sets the source used to render the image as a vector.If this is set, the Image property will be ignored.
        [Description("Gets or sets the source used to render the image as a vector.If this is set, the Image property will be ignored.")]
        public Brush Brush
                return (Brush) GetValue(BrushProperty);
                SetValue(BrushProperty,value);
        static private void BrushProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnBrushChanged( new PropertyChangedEventArgs<Brush>((Brush)e.OldValue, (Brush)e.NewValue) );
        /// Occurs when Brush property changes.
        public event EventHandler<PropertyChangedEventArgs<Brush>> BrushChanged;
        /// Called when Brush property changes.
        protected virtual void OnBrushChanged(PropertyChangedEventArgs<Brush> e)
            OnBrushChangedImplementation(e);
            RaisePropertyChangedEvent(BrushChanged, e);
        partial void OnBrushChangedImplementation(PropertyChangedEventArgs<Brush> e);
        // Image dependency property
        /// Identifies the Image dependency property.
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register( "Image", typeof(ImageSource), typeof(ScalableImageSource), new PropertyMetadata( null, ImageProperty_PropertyChanged) );
        /// Gets or sets the source used to render the image as a bitmap. If the Brush property is set, this will be ignored.
        [Description("Gets or sets the source used to render the image as a bitmap. If the Brush property is set, this will be ignored.")]
        public ImageSource Image
                return (ImageSource) GetValue(ImageProperty);
                SetValue(ImageProperty,value);
        static private void ImageProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnImageChanged( new PropertyChangedEventArgs<ImageSource>((ImageSource)e.OldValue, (ImageSource)e.NewValue) );
        /// Occurs when Image property changes.
        public event EventHandler<PropertyChangedEventArgs<ImageSource>> ImageChanged;
        /// Called when Image property changes.
        protected virtual void OnImageChanged(PropertyChangedEventArgs<ImageSource> e)
            OnImageChangedImplementation(e);
            RaisePropertyChangedEvent(ImageChanged, e);
        partial void OnImageChangedImplementation(PropertyChangedEventArgs<ImageSource> e);
        // Size dependency property
        /// Identifies the Size dependency property.
        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register( "Size", typeof(Size), typeof(ScalableImageSource), new PropertyMetadata( new Size(double.NaN, double.NaN), SizeProperty_PropertyChanged) );
        /// Gets or sets the suggested size of the image.
        [Description("Gets or sets the suggested size of the image.")]
        public Size Size
                return (Size) GetValue(SizeProperty);
                SetValue(SizeProperty,value);
        static private void SizeProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            obj.OnSizeChanged( new PropertyChangedEventArgs<Size>((Size)e.OldValue, (Size)e.NewValue) );
        /// Occurs when Size property changes.
        public event EventHandler<PropertyChangedEventArgs<Size>> SizeChanged;
        /// Called when Size property changes.
        protected virtual void OnSizeChanged(PropertyChangedEventArgs<Size> e)
            OnSizeChangedImplementation(e);
            RaisePropertyChangedEvent(SizeChanged, e);
        partial void OnSizeChangedImplementation(PropertyChangedEventArgs<Size> e);
