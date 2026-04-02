using System.Windows.Automation.Peers;
using System.Windows.Controls;
    /// Provides a <see cref="Button"/> control that is always visible in the automation tree.
    [SuppressMessage("Microsoft.MSInternal", "CA903:InternalNamespaceShouldNotContainPublicTypes")]
    [Description("Provides a System.Windows.Controls.Button control that is always visible in the automation tree.")]
    public class AutomationButton : Button
        #region Constructors
        /// Initializes a new instance of the <see cref="AutomationButton" /> class.
        public AutomationButton()
            // This constructor intentionally left blank
        #region Overides
        /// Returns the <see cref="System.Windows.Automation.Peers.AutomationPeer"/> implementations for this control.
        /// <returns>The <see cref="System.Windows.Automation.Peers.AutomationPeer"/> implementations for this control.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
            return new AutomationButtonAutomationPeer(this);
    /// Provides an automation peer for AutomationButton.
    internal class AutomationButtonAutomationPeer : ButtonAutomationPeer
        /// Initializes a new instance of the <see cref="Microsoft.Management.UI.Internal.AutomationButtonAutomationPeer" /> class.
        /// <param name="owner">The owner of the automation peer.</param>
        public AutomationButtonAutomationPeer(Button owner)
            : base(owner)
        #region Overrides
        /// Gets a value that indicates whether the element is understood by the user as interactive or as contributing to the logical structure of the control in the GUI. Called by IsControlElement().
        /// <returns>This method always returns false.</returns>
        protected override bool IsControlElementCore()
            return this.Owner.Visibility != Visibility.Hidden;
