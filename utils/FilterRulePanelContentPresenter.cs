    /// The FilterRulePanelContentPresenter selects a template based upon the ContentConverter
    /// provided.
    public class FilterRulePanelContentPresenter : ContentPresenter
        /// Initializes a new instance of the FilterRulePanelContentPresenter class.
        public FilterRulePanelContentPresenter()
            Binding b = new Binding("FilterRuleTemplateSelector");
            b.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(FilterRulePanel), 1);
            this.SetBinding(ContentTemplateSelectorProperty, b);
        /// Gets or sets an IValueConverter used to convert the Content
        public IValueConverter ContentConverter
        /// Chooses a template based upon the provided ContentConverter.
        /// Returns a DataTemplate.
        protected override DataTemplate ChooseTemplate()
            if (this.ContentTemplateSelector == null || this.ContentConverter == null)
                return base.ChooseTemplate();
            object converterContent = this.ContentConverter.Convert(this.Content, typeof(object), null, System.Globalization.CultureInfo.CurrentCulture);
            return this.ContentTemplateSelector.SelectTemplate(converterContent, this);
