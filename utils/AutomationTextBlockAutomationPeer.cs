    /// Provides an automation peer for AutomationTextBlock.
    internal class AutomationTextBlockAutomationPeer : TextBlockAutomationPeer
        /// Initializes a new instance of the <see cref="Microsoft.Management.UI.Internal.AutomationTextBlockAutomationPeer" /> class.
        public AutomationTextBlockAutomationPeer(TextBlock owner)
        /// <returns>This method always returns true.</returns>
        /// Gets the class name.
        /// <returns>The class name.</returns>
        protected override string GetClassNameCore()
            return this.Owner.GetType().Name;
