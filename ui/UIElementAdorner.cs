    /// Partial class implementation for UIElementAdorner.
    internal partial class UIElementAdorner : Adorner
        private VisualCollection children;
        /// Constructs an instance of UIElementAdorner.
        /// <param name="adornedElement">The adorned element.</param>
        public UIElementAdorner(UIElement adornedElement)
            : base(adornedElement)
            this.children = new VisualCollection(this);
        /// Overrides Visual.GetVisualChild, and returns a child at the specified index from a collection of child elements.
        /// <param name="index">The zero-based index of the requested child element in the collection..</param>
        /// <returns>The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.</returns>
        protected override Visual GetVisualChild(int index)
            return this.children[index];
        /// Gets the number of visual child elements within this element.
        protected override int VisualChildrenCount
                return this.children.Count;
        /// Implements any custom measuring behavior for the popupAdorner.
        /// <param name="constraint">A size to constrain the popupAdorner to..</param>
        /// <returns>A Size object representing the amount of layout space needed by the popupAdorner.</returns>
        protected override Size MeasureOverride(Size constraint)
                this.Child.Measure(constraint);
                return this.Child.DesiredSize;
                return base.MeasureOverride(constraint);
        /// When overridden in a derived class, positions child elements and determines a size for a FrameworkElement derived class.
        /// <returns>The actual size used.</returns>
                Point location = new Point(0, 0);
                Rect rect = new Rect(location, finalSize);
                this.Child.Arrange(rect);
                return this.Child.RenderSize;
        partial void OnChildChangedImplementation(PropertyChangedEventArgs<UIElement> e)
            if (e.OldValue != null)
                this.children.Remove(e.OldValue);
                this.RemoveLogicalChild(e.OldValue);
                this.children.Add(this.Child);
                this.AddLogicalChild(this.Child);
