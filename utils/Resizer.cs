    /// The resize grip possibilities.
    public enum ResizeGripLocation
        /// One grip is shown, on the right side.
        Right,
        /// One grip is shown, on the left side.
    /// Partial class implementation for Resizer control.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public partial class Resizer : ContentControl
        private AdornerLayer adornerLayer;
        private UIElementAdorner adorner;
        private ContentControl adornerContent;
        /// Creates an instance of Resizer.
        public Resizer()
        internal static Thickness CreateGripThickness(double visibleGripWidth, ResizeGripLocation gripLocation)
            Thickness thickness;
            if (visibleGripWidth < 0.0 || double.IsNaN(visibleGripWidth))
                throw new ArgumentOutOfRangeException("visibleGripWidth", "The value must be greater than or equal to 0.");
            if (double.IsInfinity(visibleGripWidth))
                throw new ArgumentOutOfRangeException("visibleGripWidth", "The value must be less than infinity.");
            switch (gripLocation)
                case ResizeGripLocation.Right:
                    thickness = new Thickness(0, 0, visibleGripWidth, 0);
                case ResizeGripLocation.Left:
                    thickness = new Thickness(visibleGripWidth, 0, 0, 0);
                    throw new InvalidEnumArgumentException("gripLocation", (int)gripLocation, typeof(ResizeGripLocation));
            return thickness;
            if (this.rightGrip != null)
                this.rightGrip.DragDelta -= this.OnRightGripDragDelta;
                this.rightGrip.DragStarted -= this.OnRightGripDragStarted;
                this.rightGrip.DragCompleted -= this.OnRightGripDragCompleted;
            if (this.leftGrip != null)
                this.leftGrip.DragDelta -= this.OnLeftGripDragDelta;
                this.leftGrip.DragStarted -= this.OnLeftGripDragStarted;
                this.leftGrip.DragCompleted -= this.OnLeftGripDragCompleted;
            this.rightGrip.DragDelta += this.OnRightGripDragDelta;
            this.rightGrip.DragStarted += this.OnRightGripDragStarted;
            this.rightGrip.DragCompleted += this.OnRightGripDragCompleted;
            this.leftGrip.DragDelta += this.OnLeftGripDragDelta;
            this.leftGrip.DragStarted += this.OnLeftGripDragStarted;
            this.leftGrip.DragCompleted += this.OnLeftGripDragCompleted;
        private void CreateAdorner()
            this.adornerLayer = AdornerLayer.GetAdornerLayer(this);
            this.adorner = new UIElementAdorner(this);
            this.adornerContent = new ContentControl();
            this.adornerContent.Name = "ResizerAdornerContent";
            this.adornerContent.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            this.adornerContent.VerticalContentAlignment = VerticalAlignment.Stretch;
            this.adornerContent.ContentTemplate = this.DraggingTemplate;
            this.adorner.Child = this.adornerContent;
        private void RemoveAdorner()
            this.adornerLayer.Remove(this.adorner);
        private void OnLeftGripDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
            this.StopDragging(ResizeGripLocation.Left, e);
        private void OnLeftGripDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
            this.StartDragging(ResizeGripLocation.Left);
        private void OnLeftGripDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
            this.PerformDrag(ResizeGripLocation.Left, e);
        private void OnRightGripDragCompleted(object sender, DragCompletedEventArgs e)
            this.StopDragging(ResizeGripLocation.Right, e);
        private void OnRightGripDragStarted(object sender, DragStartedEventArgs e)
            this.StartDragging(ResizeGripLocation.Right);
        private void OnRightGripDragDelta(object sender, DragDeltaEventArgs e)
            this.PerformDrag(ResizeGripLocation.Right, e);
        private void PerformDrag(ResizeGripLocation location, DragDeltaEventArgs e)
            double newWidth = this.GetNewWidth(location, e.HorizontalChange);
            if (this.ResizeWhileDragging)
                this.Width = newWidth;
                this.adorner.Width = newWidth;
        private void StartDragging(ResizeGripLocation location)
            if (this.ResizeWhileDragging == false)
                if (this.adornerLayer == null)
                    this.CreateAdorner();
                this.adornerContent.Content = location;
                this.adornerLayer.Add(this.adorner);
                this.adorner.Height = this.ActualHeight;
                this.adorner.Width = this.ActualWidth;
        private void StopDragging(ResizeGripLocation location, DragCompletedEventArgs e)
                this.RemoveAdorner();
        private double GetNewWidth(ResizeGripLocation location, double horzDelta)
            var realDelta = this.GetHorizontalDelta(location, horzDelta);
            double newWidth = this.ActualWidth + realDelta;
            return this.GetConstrainedValue(newWidth, this.MaxWidth, this.MinWidth);
        private double GetHorizontalDelta(ResizeGripLocation location, double horzDelta)
            double realDelta;
            if (location == ResizeGripLocation.Right)
                realDelta = horzDelta;
                Debug.Assert(location == ResizeGripLocation.Left, "location is left");
                realDelta = -horzDelta;
            return realDelta;
        private double GetConstrainedValue(double value, double max, double min)
            return Math.Min(max, Math.Max(value, min));
