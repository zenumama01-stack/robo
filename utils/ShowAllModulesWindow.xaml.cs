using Microsoft.Management.UI.Internal.ShowCommand;
    /// Interaction logic for CmdletGUI.xaml.
    public partial class ShowAllModulesWindow : Window
        /// private constants for ZoomLevel.
        /// Zoom Increments.
        private const double ZOOM_INCREMENT = 0.2;
        /// Max ZoomLevel.
        private const double ZOOM_MAX = 3.0;
        /// Min ZoomLevel.
        private const double ZOOM_MIN = 0.5;
        /// Initializes a new instance of the ShowAllModulesWindow class.
        public ShowAllModulesWindow()
            if (this.AllModulesControl != null && this.AllModulesControl.ShowModuleControl != null)
                this.AllModulesControl.ShowModuleControl.Owner = this;
            this.SizeChanged += this.ShowAllModulesWindow_SizeChanged;
            this.LocationChanged += this.ShowAllModulesWindow_LocationChanged;
            this.StateChanged += this.ShowAllModulesWindow_StateChanged;
            this.Loaded += this.ShowAllModulesWindow_Loaded;
            RoutedCommand plusSettings = new RoutedCommand();
            KeyGestureConverter keyGestureConverter = new KeyGestureConverter();
                plusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString(UICultureResources.ZoomIn1Shortcut));
                plusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString(UICultureResources.ZoomIn2Shortcut));
                plusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString(UICultureResources.ZoomIn3Shortcut));
                plusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString(UICultureResources.ZoomIn4Shortcut));
                CommandBindings.Add(new CommandBinding(plusSettings, ZoomEventHandlerPlus));
            catch (NotSupportedException)
                // localized has a problematic string - going to default
                plusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString("Ctrl+Add"));
                plusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString("Ctrl+Plus"));
            RoutedCommand minusSettings = new RoutedCommand();
                minusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString(UICultureResources.ZoomOut1Shortcut));
                minusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString(UICultureResources.ZoomOut2Shortcut));
                minusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString(UICultureResources.ZoomOut3Shortcut));
                minusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString(UICultureResources.ZoomOut4Shortcut));
                CommandBindings.Add(new CommandBinding(minusSettings, ZoomEventHandlerMinus));
                minusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString("Ctrl+Subtract"));
                minusSettings.InputGestures.Add((KeyGesture)keyGestureConverter.ConvertFromString("Ctrl+Minus"));
        protected override void OnClosed(System.EventArgs e)
            ShowCommandSettings.Default.Save();
        /// Sets the focus on the CommandName control.
        private void ShowAllModulesWindow_Loaded(object sender, RoutedEventArgs e)
            this.AllModulesControl.CommandName.Focus();
        private void ShowAllModulesWindow_SizeChanged(object sender, SizeChangedEventArgs e)
            ShowCommandSettings.Default.ShowCommandsWidth = this.Width;
            ShowCommandSettings.Default.ShowCommandsHeight = this.Height;
        private void ShowAllModulesWindow_LocationChanged(object sender, System.EventArgs e)
            ShowCommandSettings.Default.ShowCommandsTop = this.Top;
            ShowCommandSettings.Default.ShowCommandsLeft = this.Left;
        private void ShowAllModulesWindow_StateChanged(object sender, System.EventArgs e)
            ShowCommandSettings.Default.ShowCommandsWindowMaximized = this.WindowState == WindowState.Maximized;
        /// Implements ZoomIn.
        /// <param name="sender">.</param>
        /// <param name="e">.</param>
        private void ZoomEventHandlerPlus(object sender, ExecutedRoutedEventArgs e)
            if (this.zoomLevel == 0)
                this.zoomLevel = 1;
            if (this.zoomLevel < ZOOM_MAX)
                // ViewModel applies ZoomLevel after dividing it by 100, So multiply it by 100 and then later reset to normal by dividing for next zoom
                this.zoomLevel = (this.zoomLevel + ZOOM_INCREMENT) * 100;
                viewModel.ZoomLevel = this.zoomLevel;
                this.zoomLevel /= 100;
        /// Implements ZoomOut.
        private void ZoomEventHandlerMinus(object sender, ExecutedRoutedEventArgs e)
            if (this.zoomLevel >= ZOOM_MIN)
                // ViewModel applies ZoomLevel after dividing it by 100, So multiply it by 100 and then later reset to normal by dividing it for next zoom
                this.zoomLevel = (this.zoomLevel - ZOOM_INCREMENT) * 100;
