    /// Represents a decorator that is always visible in the automation tree, indicating that its descendents belong to a logical group.
    public class AutomationGroup : ContentControl
        /// Returns the <see cref="AutomationPeer"/> implementations for this control.
        /// <returns>The <see cref="AutomationPeer"/> implementations for this control.</returns>
            return new ExtendedFrameworkElementAutomationPeer(this, AutomationControlType.Group, true);
