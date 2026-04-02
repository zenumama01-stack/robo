    /// Implements the ImageButtonBase base class to the ImageButton and ImageToggleButton.
    public class ImageButtonBase : Grid
        /// Command associated with this button.
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(RoutedUICommand), typeof(ImageButton));
        /// Image to be used for the enabled state.
        public static readonly DependencyProperty EnabledImageSourceProperty =
            DependencyProperty.Register("EnabledImageSource", typeof(ImageSource), typeof(ImageButton));
        /// Image to be used for the disabled state.
        public static readonly DependencyProperty DisabledImageSourceProperty =
            DependencyProperty.Register("DisabledImageSource", typeof(ImageSource), typeof(ImageButton));
        /// Gets or sets the image to be used for the enabled state.
        public ImageSource EnabledImageSource
            get { return (ImageSource)GetValue(ImageButton.EnabledImageSourceProperty); }
            set { SetValue(ImageButton.EnabledImageSourceProperty, value); }
        /// Gets or sets the image to be used for the disabled state.
        public ImageSource DisabledImageSource
            get { return (ImageSource)GetValue(ImageButton.DisabledImageSourceProperty); }
            set { SetValue(ImageButton.DisabledImageSourceProperty, value); }
        /// Gets or sets the command associated with this button.
        public RoutedUICommand Command
            get { return (RoutedUICommand)GetValue(ImageButton.CommandProperty); }
            set { SetValue(ImageButton.CommandProperty, value); }
