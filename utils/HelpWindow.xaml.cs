using System.Windows.Input;
using Microsoft.Management.UI.Internal;
    /// A window displaying help content and allowing search.
    public partial class HelpWindow : Window
        /// Minimum zoom in the slider.
        public static double MinimumZoom
                return 20;
        /// Maximum zoom in the slider.
        public static double MaximumZoom
                return 300;
        /// Zoom interval.
        public static double ZoomInterval
                return 10;
        /// The ViewModel for the dialog.
        private readonly HelpViewModel viewModel;
        /// Initializes a new instance of the HelpWindow class.
        /// <param name="helpObject">The object with help information.</param>
        public HelpWindow(PSObject helpObject)
            InitializeComponent();
            this.viewModel = new HelpViewModel(helpObject, this.DocumentParagraph);
            CommonHelper.SetStartingPositionAndSize(
                this,
                HelpWindowSettings.Default.HelpWindowTop,
                HelpWindowSettings.Default.HelpWindowLeft,
                HelpWindowSettings.Default.HelpWindowWidth,
                HelpWindowSettings.Default.HelpWindowHeight,
                double.Parse((string)HelpWindowSettings.Default.Properties["HelpWindowWidth"].DefaultValue, CultureInfo.InvariantCulture.NumberFormat),
                double.Parse((string)HelpWindowSettings.Default.Properties["HelpWindowHeight"].DefaultValue, CultureInfo.InvariantCulture.NumberFormat),
                HelpWindowSettings.Default.HelpWindowMaximized);
            this.ReadZoomUserSetting();
            this.viewModel.PropertyChanged += this.ViewModel_PropertyChanged;
            this.DataContext = this.viewModel;
            this.Loaded += this.HelpDialog_Loaded;
            this.Closed += this.HelpDialog_Closed;
        /// Handles the mouse wheel to zoom in/out.
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
            if (Keyboard.Modifiers != ModifierKeys.Control)
            if (e.Delta > 0)
                this.viewModel.ZoomIn();
                e.Handled = true;
                this.viewModel.ZoomOut();
        /// Handles key down to fix the Page/Douwn going to end of help issue
        /// And to implement some additional shortcuts like Ctrl+F and ZoomIn/ZoomOut.
        protected override void OnPreviewKeyDown(KeyEventArgs e)
            if (Keyboard.Modifiers == ModifierKeys.None)
                if (e.Key == Key.PageUp)
                    this.Scroll.PageUp();
                if (e.Key == Key.PageDown)
                    this.Scroll.PageDown();
            if (Keyboard.Modifiers == ModifierKeys.Control)
                this.HandleZoomInAndZoomOut(e);
                if (e.Handled)
                if (e.Key == Key.F)
                    this.Find.Focus();
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        /// Reads the zoom part of the user settings.
        private void ReadZoomUserSetting()
            if (HelpWindowSettings.Default.HelpZoom < HelpWindow.MinimumZoom || HelpWindowSettings.Default.HelpZoom > HelpWindow.MaximumZoom)
                HelpWindowSettings.Default.HelpZoom = 100;
            this.viewModel.Zoom = HelpWindowSettings.Default.HelpZoom;
        /// Handles Zoom in and Zoom out keys.
        private void HandleZoomInAndZoomOut(KeyEventArgs e)
            if (e.Key == Key.OemPlus || e.Key == Key.Add)
            if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
        /// Listens to changes in the zoom in order to update the user settings.
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            if (e.PropertyName == "Zoom")
                HelpWindowSettings.Default.HelpZoom = this.viewModel.Zoom;
        /// Saves the user settings.
        private void HelpDialog_Closed(object sender, System.EventArgs e)
            HelpWindowSettings.Default.Save();
        /// Updates the user setting with window state.
        private void HelpDialog_StateChanged(object sender, System.EventArgs e)
            HelpWindowSettings.Default.HelpWindowMaximized = this.WindowState == WindowState.Maximized;
        /// Sets the positions from user settings and start monitoring position changes.
        private void HelpDialog_Loaded(object sender, RoutedEventArgs e)
            this.StateChanged += this.HelpDialog_StateChanged;
            this.LocationChanged += this.HelpDialog_LocationChanged;
            this.SizeChanged += this.HelpDialog_SizeChanged;
        /// Saves size changes in user settings.
        private void HelpDialog_SizeChanged(object sender, SizeChangedEventArgs e)
            HelpWindowSettings.Default.HelpWindowWidth = this.Width;
            HelpWindowSettings.Default.HelpWindowHeight = this.Height;
        /// Saves position changes in user settings.
        private void HelpDialog_LocationChanged(object sender, System.EventArgs e)
            HelpWindowSettings.Default.HelpWindowTop = this.Top;
            HelpWindowSettings.Default.HelpWindowLeft = this.Left;
        /// Called when the settings button is clicked.
        private void Settings_Click(object sender, RoutedEventArgs e)
            SettingsDialog settings = new SettingsDialog();
            settings.Owner = this;
            settings.ShowDialog();
            if (settings.DialogResult == true)
                this.viewModel.HelpBuilder.AddTextToParagraphBuilder();
                this.viewModel.Search();
        /// Called when the Previous button is clicked.
        private void PreviousMatch_Click(object sender, RoutedEventArgs e)
            this.MoveToNextMatch(false);
        /// Called when the Next button is clicked.
        private void NextMatch_Click(object sender, RoutedEventArgs e)
            this.MoveToNextMatch(true);
        /// Moves to the previous or next match.
        /// <param name="forward">True for forward false for backwards.</param>
        private void MoveToNextMatch(bool forward)
            TextPointer caretPosition = this.HelpText.CaretPosition;
            Run nextRun = this.viewModel.Searcher.MoveAndHighlightNextNextMatch(forward, caretPosition);
            this.MoveToRun(nextRun);
        /// Moves to the caret and brings the view to the <paramref name="run"/>.
        /// <param name="run">Run to move to.</param>
        private void MoveToRun(Run run)
            if (run == null)
            run.BringIntoView();
            this.HelpText.CaretPosition = run.ElementEnd;
            this.HelpText.Focus();
