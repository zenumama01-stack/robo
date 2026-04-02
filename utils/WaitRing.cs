    /// Waiting Ring class.
    public class WaitRing : Control
        /// Static constructor for WaitRing.
        static WaitRing()
            // This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            // This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WaitRing), new FrameworkPropertyMetadata(typeof(WaitRing)));
