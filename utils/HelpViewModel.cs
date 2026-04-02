    /// ViewModel for the Help Dialog used to:
    ///     build the help document
    ///     search the help document
    ///     offer text for labels.
    internal class HelpViewModel : INotifyPropertyChanged
        /// The builder for the help FlowDocument Paragraph  used in a RichEditText control.
        private readonly HelpParagraphBuilder helpBuilder;
        /// Searcher for selecting current matches in paragraph text.
        private readonly ParagraphSearcher searcher;
        /// Title of the help window.
        private readonly string helpTitle;
        /// the zoom bound to the zoom slider value.
        private double zoom = 100;
        /// Text to be found. This is bound to the find TextBox.
        private string findText;
        /// text for the number of matches found.
        private string matchesLabel;
        /// Initializes a new instance of the HelpViewModel class.
        /// <param name="psObj">Object containing help.</param>
        /// <param name="documentParagraph">Paragraph in which help text is built/searched.</param>
        internal HelpViewModel(PSObject psObj, Paragraph documentParagraph)
            Debug.Assert(documentParagraph != null, "ensured by caller");
            this.helpBuilder = new HelpParagraphBuilder(documentParagraph, psObj);
            this.helpBuilder.BuildParagraph();
            this.searcher = new ParagraphSearcher();
            this.helpBuilder.PropertyChanged += this.HelpBuilder_PropertyChanged;
            this.helpTitle = string.Format(
                HelpWindowResources.HelpTitleFormat,
                HelpParagraphBuilder.GetPropertyString(psObj, "name"));
        #region INotifyPropertyChanged Members
        /// Used to notify of property changes.
        public event PropertyChangedEventHandler PropertyChanged;
        /// Gets or sets the Zoom bound to the zoom slider value.
        public double Zoom
                return this.zoom;
                this.zoom = value;
                this.OnNotifyPropertyChanged("Zoom");
                this.OnNotifyPropertyChanged("ZoomLabel");
                this.OnNotifyPropertyChanged("ZoomLevel");
        /// Gets the value bound to the RichTextEdit zoom, which is calculated based on the zoom.
        public double ZoomLevel
                return this.zoom / 100.0;
        /// Gets the label to be displayed for the zoom.
        public string ZoomLabel
                return string.Format(CultureInfo.CurrentCulture, HelpWindowResources.ZoomLabelTextFormat, this.zoom);
        /// Gets or sets the text to be found.
        public string FindText
                return this.findText;
                this.findText = value;
                this.Search();
                this.SetMatchesLabel();
        /// Gets the title of the window.
        public string HelpTitle
                return this.helpTitle;
        /// Gets or sets the label for current matches.
        public string MatchesLabel
                return this.matchesLabel;
                this.matchesLabel = value;
                this.OnNotifyPropertyChanged("MatchesLabel");
        /// Gets a value indicating whether there are matches to go to.
        public bool CanGoToNextOrPrevious
                return this.HelpBuilder.HighlightCount != 0;
        /// Gets the searcher for selecting current matches in paragraph text.
        internal ParagraphSearcher Searcher
            get { return this.searcher; }
        /// Gets the paragraph builder used to write help content.
        internal HelpParagraphBuilder HelpBuilder
            get { return this.helpBuilder; }
        /// Highlights all matches to this.findText
        /// Called when findText changes or whenever the search has to be refreshed
        internal void Search()
            this.HelpBuilder.HighlightAllInstancesOf(this.findText, HelpWindowSettings.Default.HelpSearchMatchCase, HelpWindowSettings.Default.HelpSearchWholeWord);
            this.searcher.ResetSearch();
        /// Increases Zoom if not above maximum.
        internal void ZoomIn()
            if (this.Zoom + HelpWindow.ZoomInterval <= HelpWindow.MaximumZoom)
                this.Zoom += HelpWindow.ZoomInterval;
        /// Decreases Zoom if not below minimum.
        internal void ZoomOut()
            if (this.Zoom - HelpWindow.ZoomInterval >= HelpWindow.MinimumZoom)
                this.Zoom -= HelpWindow.ZoomInterval;
        /// Called to update the matches label.
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void HelpBuilder_PropertyChanged(object sender, PropertyChangedEventArgs e)
            if (e.PropertyName == "HighlightCount")
                this.OnNotifyPropertyChanged("CanGoToNextOrPrevious");
        /// Sets the current matches label.
        private void SetMatchesLabel()
            if (this.findText == null || this.findText.Trim().Length == 0)
                this.MatchesLabel = string.Empty;
                if (this.HelpBuilder.HighlightCount == 0)
                    this.MatchesLabel = HelpWindowResources.NoMatches;
                    if (this.HelpBuilder.HighlightCount == 1)
                        this.MatchesLabel = HelpWindowResources.OneMatch;
                        this.MatchesLabel = string.Format(
                            HelpWindowResources.SomeMatchesFormat,
                            this.HelpBuilder.HighlightCount);
        /// Called internally to notify when a property changed.
        private void OnNotifyPropertyChanged(string propertyName)
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
