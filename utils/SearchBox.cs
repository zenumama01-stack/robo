    /// Partial class implementation for SearchBox control.
    public partial class SearchBox : Control, IFilterExpressionProvider
        private SearchTextParser parser;
        /// Initializes a new instance of the <see cref="SearchBox"/> class.
        public SearchBox()
        #region IFilterExpressionProvider Implementation
        /// Gets the filter expression representing the current search text.
                return SearchBox.ConvertToFilterExpression(this.Parser.Parse(this.Text));
                return string.IsNullOrEmpty(this.Text) == false;
        /// Gets or sets the parser used to parse the search text.
        public SearchTextParser Parser
                if (this.parser == null)
                    this.parser = new SearchTextParser();
                return this.parser;
                this.parser = value;
        partial void OnTextChangedImplementation(PropertyChangedEventArgs<string> e)
        partial void OnClearTextCanExecuteImplementation(CanExecuteRoutedEventArgs e)
            e.CanExecute = this.HasFilterExpression;
        partial void OnClearTextExecutedImplementation(ExecutedRoutedEventArgs e)
            this.Text = string.Empty;
        /// Converts the specified collection of searchbox items to a filter expression.
        /// <param name="searchBoxItems">A collection of searchbox items to convert.</param>
        /// <returns>A filter expression.</returns>
        protected static FilterExpressionNode ConvertToFilterExpression(ICollection<SearchTextParseResult> searchBoxItems)
            ArgumentNullException.ThrowIfNull(searchBoxItems);
            if (searchBoxItems.Count == 0)
                FilterExpressionAndOperatorNode filterExpression = new FilterExpressionAndOperatorNode();
                foreach (SearchTextParseResult item in searchBoxItems)
                    filterExpression.Children.Add(new FilterExpressionOperandNode(item.FilterRule));
                return filterExpression;
