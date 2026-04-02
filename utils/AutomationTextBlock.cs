    /// Provides a <see cref="TextBlock"/> control that is always visible in the automation tree.
    [Description("Provides a System.Windows.Controls.TextBlock control that is always visible in the automation tree.")]
    public class AutomationTextBlock : TextBlock
        #region Structors
        /// Initializes a new instance of the <see cref="AutomationTextBlock" /> class.
        public AutomationTextBlock()
            return new AutomationTextBlockAutomationPeer(this);
