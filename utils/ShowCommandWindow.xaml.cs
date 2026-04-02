    public partial class ShowCommandWindow : Window
        /// Initializes a new instance of the ShowCommandWindow class.
        public ShowCommandWindow()
            this.SizeChanged += this.ShowCommandWindow_SizeChanged;
            this.LocationChanged += this.ShowCommandWindow_LocationChanged;
            this.StateChanged += this.ShowCommandWindow_StateChanged;
        private void ShowCommandWindow_SizeChanged(object sender, SizeChangedEventArgs e)
            ShowCommandSettings.Default.ShowOneCommandWidth = this.Width;
            ShowCommandSettings.Default.ShowOneCommandHeight = this.Height;
        private void ShowCommandWindow_LocationChanged(object sender, System.EventArgs e)
            ShowCommandSettings.Default.ShowOneCommandTop = this.Top;
            ShowCommandSettings.Default.ShowOneCommandLeft = this.Left;
        private void ShowCommandWindow_StateChanged(object sender, System.EventArgs e)
            ShowCommandSettings.Default.ShowOneCommandWindowMaximized = this.WindowState == WindowState.Maximized;
