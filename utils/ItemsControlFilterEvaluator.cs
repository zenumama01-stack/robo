    /// The ItemsControlFilterEvaluator class provides functionality to
    /// apply a filter against an ItemsControl.
    public class ItemsControlFilterEvaluator : FilterEvaluator
        private ItemsControl filterTarget;
        /// Gets or sets an ItemsControl which is
        /// the target for filtering.
        public ItemsControl FilterTarget
                return this.filterTarget;
                if (this.filterTarget != null)
                    this.StopFilter();
                this.filterTarget = value;
        private FilterExpressionNode CachedFilterExpression
        /// Used to notify listeners that an unhandled exception has occurred while
        public event EventHandler<FilterExceptionEventArgs> FilterExceptionOccurred;
        public override void StartFilter()
            if (this.FilterTarget == null)
                throw new InvalidOperationException("FilterTarget is null.");
            // Cache the expression for filtering so subsequent changes are ignored \\
            this.CachedFilterExpression = this.FilterExpression;
            if (this.CachedFilterExpression != null)
                this.FilterTarget.Items.Filter = this.FilterExpressionAdapter;
                this.FilterStatus = FilterStatus.Applied;
        public override void StopFilter()
            // Only clear the filter if necessary, since clearing it causes sorting to be re-evaluated \\
            if (this.FilterTarget.Items.Filter != null)
                this.FilterTarget.Items.Filter = null;
            this.FilterStatus = FilterStatus.NotApplied;
        private bool FilterExpressionAdapter(object item)
            Debug.Assert(this.CachedFilterExpression != null, "not null");
                return this.CachedFilterExpression.Evaluate(item);
                if (!this.TryNotifyFilterException(e))
        private bool TryNotifyFilterException(Exception e)
            EventHandler<FilterExceptionEventArgs> eh = this.FilterExceptionOccurred;
                eh(this, new FilterExceptionEventArgs(e));
