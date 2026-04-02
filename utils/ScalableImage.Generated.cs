    /// Represents an image that can render as a vector or as a bitmap.
    partial class ScalableImage
        // Source dependency property
        /// Identifies the Source dependency property.
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register( "Source", typeof(ScalableImageSource), typeof(ScalableImage), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.AffectsRender, SourceProperty_PropertyChanged) );
        /// Gets or sets the ScalableImageSource used to render the image. This is a dependency property.
        [Description("Gets or sets the ScalableImageSource used to render the image. This is a dependency property.")]
        public ScalableImageSource Source
                return (ScalableImageSource) GetValue(SourceProperty);
                SetValue(SourceProperty,value);
        static private void SourceProperty_PropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
            ScalableImage obj = (ScalableImage) o;
            obj.OnSourceChanged( new PropertyChangedEventArgs<ScalableImageSource>((ScalableImageSource)e.OldValue, (ScalableImageSource)e.NewValue) );
        /// Occurs when Source property changes.
        public event EventHandler<PropertyChangedEventArgs<ScalableImageSource>> SourceChanged;
        /// Called when Source property changes.
        protected virtual void OnSourceChanged(PropertyChangedEventArgs<ScalableImageSource> e)
            OnSourceChangedImplementation(e);
            RaisePropertyChangedEvent(SourceChanged, e);
        partial void OnSourceChangedImplementation(PropertyChangedEventArgs<ScalableImageSource> e);
        // CreateAutomationPeer
        /// Create an instance of the AutomationPeer.
        /// An instance of the AutomationPeer.
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
            return new ExtendedFrameworkElementAutomationPeer(owner: this, controlType: AutomationControlType.Image, isControlElement: false);
