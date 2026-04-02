    /// Represents a filter rule that searches for text within properties on an object.
    public class PropertiesTextContainsFilterRule : TextFilterRule
        private static readonly string TextContainsCharactersRegexPattern = "{0}";
        private static readonly string TextContainsWordsRegexPattern = WordBoundaryRegexPattern + TextContainsCharactersRegexPattern + WordBoundaryRegexPattern;
        private Regex cachedRegex;
        /// Initializes a new instance of the <see cref="PropertiesTextContainsFilterRule"/> class.
        public PropertiesTextContainsFilterRule()
            this.PropertyNames = new List<string>();
            this.EvaluationResultInvalidated += this.PropertiesTextContainsFilterRule_EvaluationResultInvalidated;
        /// Initializes a new instance of the  <see cref="PropertiesTextContainsFilterRule"/> class.
        public PropertiesTextContainsFilterRule(PropertiesTextContainsFilterRule source)
            this.PropertyNames = new List<string>(source.PropertyNames);
        /// Gets a collection of the names of properties to search in.
        public ICollection<string> PropertyNames
        /// Evaluates whether the specified properties on <paramref name="item"/> contain the current value.
        /// <returns><c>true</c> if <paramref name="item"/> is not <c>null</c>, the current value is valid, and the specified properties on <paramref name="item"/> contain the current value; otherwise, <c>false</c>.</returns>
            foreach (string propertyName in this.PropertyNames)
                object propertyValue;
                if (!FilterRuleCustomizationFactory.FactoryInstance.PropertyValueGetter.TryGetPropertyValue(propertyName, item, out propertyValue))
                if (propertyValue != null)
                    string data = propertyValue.ToString();
                    if (this.Evaluate(data))
        /// Evaluates whether the specified data contains the current value.
        /// <param name="data">The data to evaluate.</param>
        /// <returns><c>true</c> if <paramref name="data"/> contains the current value; otherwise, <c>false</c>.</returns>
        protected override bool Evaluate(string data)
            if (this.cachedRegex == null)
                this.UpdateCachedRegex();
            return this.cachedRegex.IsMatch(data);
        /// Called when the evaluation result is invalidated.
        /// Updates the cached Regex pattern.
        protected virtual void OnEvaluationResultInvalidated()
        /// Updates the cached Regex with the current value.
        /// If the current value is invalid, the Regex will not be updated because it will not be evaluated.
        private void UpdateCachedRegex()
            if (this.IsValid)
                var parsedPattern = this.GetRegexPattern(TextContainsCharactersRegexPattern, TextContainsWordsRegexPattern);
                this.cachedRegex = new Regex(parsedPattern, this.GetRegexOptions());
        private void PropertiesTextContainsFilterRule_EvaluationResultInvalidated(object sender, EventArgs e)
            this.OnEvaluationResultInvalidated();
