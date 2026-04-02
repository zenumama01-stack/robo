    /// The FilterEvaluator class is responsible for allowing the registration of
    /// the FilterExpressionProviders and producing a FilterExpression composed of
    /// the FilterExpression returned from the providers.
    public abstract class FilterEvaluator : IFilterExpressionProvider, INotifyPropertyChanged
        private Collection<IFilterExpressionProvider> filterExpressionProviders = new Collection<IFilterExpressionProvider>();
        /// Gets a readonly collection of the registered FilterExpressionProviders.
        public ReadOnlyCollection<IFilterExpressionProvider> FilterExpressionProviders
                return new ReadOnlyCollection<IFilterExpressionProvider>(this.filterExpressionProviders);
        private FilterStatus filterStatus = FilterStatus.NotApplied;
        /// Gets a value indicating the status of the filter evaluation.
        public FilterStatus FilterStatus
                return this.filterStatus;
                this.filterStatus = value;
                this.NotifyPropertyChanged("FilterStatus");
        private bool startFilterOnExpressionChanged = true;
        public bool StartFilterOnExpressionChanged
                return this.startFilterOnExpressionChanged;
                this.startFilterOnExpressionChanged = value;
                this.NotifyPropertyChanged("StartFilterOnExpressionChanged");
        private bool hasFilterExpression = false;
        /// Gets a value indicating whether this provider currently has a non-empty filter expression.
        public bool HasFilterExpression
                return this.hasFilterExpression;
                this.hasFilterExpression = value;
                this.NotifyPropertyChanged("HasFilterExpression");
        #endregion Properties
        /// Notifies listeners that a property has changed.
        #region Public Methods
        /// Applies the filter.
        public abstract void StartFilter();
        /// Stops the filter.
        public abstract void StopFilter();
        /// Returns a FilterExpression composed of FilterExpressions returned from the
        /// registered providers.
        /// The FilterExpression composed of FilterExpressions returned from the
        public FilterExpressionNode FilterExpression
                FilterExpressionAndOperatorNode andNode = new FilterExpressionAndOperatorNode();
                foreach (IFilterExpressionProvider provider in this.FilterExpressionProviders)
                    FilterExpressionNode node = provider.FilterExpression;
                    if (node != null)
                        andNode.Children.Add(node);
                return (andNode.Children.Count != 0) ? andNode : null;
        /// Adds a FilterExpressionProvider to the FilterEvaluator.
        /// <param name="provider">
        /// The provider to add.
        public void AddFilterExpressionProvider(IFilterExpressionProvider provider)
            ArgumentNullException.ThrowIfNull(provider);
            this.filterExpressionProviders.Add(provider);
            provider.FilterExpressionChanged += this.FilterProvider_FilterExpressionChanged;
        /// Removes a FilterExpressionProvider from the FilterEvaluator.
        /// The provider to remove.
        public void RemoveFilterExpressionProvider(IFilterExpressionProvider provider)
            this.filterExpressionProviders.Remove(provider);
            provider.FilterExpressionChanged -= this.FilterProvider_FilterExpressionChanged;
        #region NotifyPropertyChanged
        /// <param name="propertyName">
        /// The propertyName which has changed.
        protected void NotifyPropertyChanged(string propertyName)
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "propertyName is not null");
                eh(this, new PropertyChangedEventArgs(propertyName));
        #endregion NotifyPropertyChanged
        #endregion Public Methods
        /// Occurs when the filter expression has changed.
        public event EventHandler FilterExpressionChanged;
        /// Notifies any listeners that the filter expression has changed.
        protected virtual void NotifyFilterExpressionChanged()
            EventHandler eh = this.FilterExpressionChanged;
                eh(this, new EventArgs());
        private void FilterProvider_FilterExpressionChanged(object sender, EventArgs e)
            // Update HasFilterExpression \\
            var hasFilterExpression = false;
                if (provider.HasFilterExpression)
                    hasFilterExpression = true;
            this.HasFilterExpression = hasFilterExpression;
            // Update FilterExpression \\
            this.NotifyFilterExpressionChanged();
            this.NotifyPropertyChanged("FilterExpression");
            // Start filtering if requested \\
            if (this.StartFilterOnExpressionChanged)
                this.StartFilter();
