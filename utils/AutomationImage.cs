    /// Provides a <see cref="Image"/> control that is always visible in the automation tree.
    [Description("Provides a System.Windows.Controls.Image control that is always visible in the automation tree.")]
    public class AutomationImage : Image
        /// Initializes a new instance of the <see cref="AutomationImage" /> class.
        public AutomationImage()
            return new AutomationImageAutomationPeer(this);
    /// Provides an automation peer for AutomationImage.
    internal class AutomationImageAutomationPeer : ImageAutomationPeer
        /// Initializes a new instance of the <see cref="Microsoft.Management.UI.Internal.AutomationImageAutomationPeer" /> class.
        public AutomationImageAutomationPeer(Image owner)
