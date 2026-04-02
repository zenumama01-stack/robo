    /// Provides a base automation peer for FrameworkElement controls.
    public class ExtendedFrameworkElementAutomationPeer : FrameworkElementAutomationPeer
        #region Fields
        /// Gets or sets the control type of the element that is associated with this automation peer.
        private AutomationControlType controlType = AutomationControlType.Custom;
        /// Gets or sets a value that indicates whether the control should show in the logical tree.
        private bool isControlElement = true;
        /// Initializes a new instance of the <see cref="ExtendedFrameworkElementAutomationPeer" /> class.
        public ExtendedFrameworkElementAutomationPeer(FrameworkElement owner)
        /// <param name="controlType">The control type of the element that is associated with the automation peer.</param>
        public ExtendedFrameworkElementAutomationPeer(FrameworkElement owner, AutomationControlType controlType)
            : this(owner)
            this.controlType = controlType;
        /// <param name="isControlElement">Whether the element should show in the logical tree.</param>
        public ExtendedFrameworkElementAutomationPeer(FrameworkElement owner, AutomationControlType controlType, bool isControlElement)
            : this(owner, controlType)
            this.isControlElement = isControlElement;
        /// Gets the control type of the element that is associated with the automation peer.
        /// <returns>Returns the control type of the element that is associated with the automation peer.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
            return this.controlType;
            return this.isControlElement;
