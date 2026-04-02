    /// The FilterRuleTemplateSelector class selects a template based upon the type of
    /// the item and the corresponding template that is registered in the TemplateDictionary.
    public class FilterRuleTemplateSelector : DataTemplateSelector
        private Dictionary<Type, DataTemplate> templateDictionary = new Dictionary<Type, DataTemplate>();
        /// Gets the dictionary containing the type-template values.
        public IDictionary<Type, DataTemplate> TemplateDictionary
            get { return this.templateDictionary; }
        /// Selects a template based upon the type of the item and the
        /// corresponding template that is registered in the TemplateDictionary.
        /// The item to return a template for.
        /// <param name="container">
        /// The parameter is not used.
        /// Returns a DataTemplate for item.
        public override DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
                return base.SelectTemplate(item, container);
            Type type = item as Type ?? item.GetType();
            DataTemplate template;
                if (type.IsGenericType)
                    type = type.GetGenericTypeDefinition();
                if (this.TemplateDictionary.TryGetValue(type, out template))
                    return template;
                type = type.BaseType;
            while (type != null);
