using System.Windows.Automation.Provider;
    /// Provides an automation peer for <see cref="ExpanderButton"/>.
    public class ExpanderButtonAutomationPeer : ToggleButtonAutomationPeer, IExpandCollapseProvider
        private ExpanderButton expanderButton;
        /// Initializes a new instance of the <see cref="ExpanderButtonAutomationPeer" /> class.
        public ExpanderButtonAutomationPeer(ExpanderButton owner)
            this.expanderButton = owner;
        /// Gets the control pattern for the <see cref="ExpanderButton"/> that is associated with this <see cref="ExpanderButtonAutomationPeer"/>.
        /// <param name="patternInterface">Specifies the control pattern that is returned.</param>
        /// <returns>The control pattern for the <see cref="ExpanderButton"/> that is associated with this <see cref="ExpanderButtonAutomationPeer"/>.</returns>
        public override object GetPattern(PatternInterface patternInterface)
            if (patternInterface == PatternInterface.ExpandCollapse ||
                patternInterface == PatternInterface.Toggle)
                return this;
        #region IExpandCollapseProvider Implementations
        /// Gets the expand/collapse state of this <see cref="ExpanderButton"/> instance.
        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
                if (this.expanderButton.IsChecked == true)
                    return ExpandCollapseState.Expanded;
                    return ExpandCollapseState.Collapsed;
        /// Expands this instance of <see cref="ExpanderButton"/>.
        void IExpandCollapseProvider.Expand()
            if (!this.IsEnabled())
                throw new ElementNotEnabledException();
            this.expanderButton.IsChecked = true;
        /// Collapses this instance of <see cref="ExpanderButton"/>.
        void IExpandCollapseProvider.Collapse()
            this.expanderButton.IsChecked = false;
