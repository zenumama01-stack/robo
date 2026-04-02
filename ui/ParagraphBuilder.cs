using System.Windows.Media;
    /// Builds a paragraph based on Text + Bold + Highlight information.
    /// Bold are the segments of the text that should be bold, and Highlight are
    /// the segments of the text that should be highlighted (like search results).
    internal class ParagraphBuilder : INotifyPropertyChanged
        /// The text spans that should be bold.
        private readonly List<TextSpan> boldSpans;
        /// The text spans that should be highlighted.
        private readonly List<TextSpan> highlightedSpans;
        /// The text displayed.
        private readonly StringBuilder textBuilder;
        /// Paragraph built in BuildParagraph.
        private readonly Paragraph paragraph;
        /// Initializes a new instance of the ParagraphBuilder class.
        /// <param name="paragraph">Paragraph we will be adding lines to in BuildParagraph.</param>
        internal ParagraphBuilder(Paragraph paragraph)
            ArgumentNullException.ThrowIfNull(paragraph);
            this.paragraph = paragraph;
            this.boldSpans = new List<TextSpan>();
            this.highlightedSpans = new List<TextSpan>();
            this.textBuilder = new StringBuilder();
        /// Gets the number of highlights.
        internal int HighlightCount
            get { return this.highlightedSpans.Count; }
        /// Gets the paragraph built in BuildParagraph.
        internal Paragraph Paragraph
            get { return this.paragraph; }
        /// Called after all the AddText calls have been made to build the paragraph
        /// based on the current text.
        /// This method goes over 3 collections simultaneously:
        ///    1) characters in this.textBuilder
        ///    2) spans in this.boldSpans
        ///    3) spans in this.highlightedSpans
        /// And adds the minimal number of Inlines to the paragraph so that all
        /// characters that should be bold and/or highlighted are.
        internal void BuildParagraph()
            this.paragraph.Inlines.Clear();
            int currentBoldIndex = 0;
            TextSpan? currentBoldSpan = this.boldSpans.Count == 0 ? (TextSpan?)null : this.boldSpans[0];
            int currentHighlightedIndex = 0;
            TextSpan? currentHighlightedSpan = this.highlightedSpans.Count == 0 ? (TextSpan?)null : this.highlightedSpans[0];
            bool currentBold = false;
            bool currentHighlighted = false;
            StringBuilder sequence = new StringBuilder();
            foreach (char c in this.textBuilder.ToString())
                bool newBold = false;
                bool newHighlighted = false;
                ParagraphBuilder.MoveSpanToPosition(ref currentBoldIndex, ref currentBoldSpan, i, this.boldSpans);
                newBold = currentBoldSpan == null ? false : currentBoldSpan.Value.Contains(i);
                ParagraphBuilder.MoveSpanToPosition(ref currentHighlightedIndex, ref currentHighlightedSpan, i, this.highlightedSpans);
                newHighlighted = currentHighlightedSpan == null ? false : currentHighlightedSpan.Value.Contains(i);
                if (newBold != currentBold || newHighlighted != currentHighlighted)
                    ParagraphBuilder.AddInline(this.paragraph, currentBold, currentHighlighted, sequence);
                sequence.Append(c);
                currentHighlighted = newHighlighted;
                currentBold = newBold;
        /// Highlights all occurrences of <paramref name="search"/>.
        /// This is called after all calls to AddText have been made.
        /// <param name="search">Search string.</param>
        /// <param name="caseSensitive">True if search should be case sensitive.</param>
        /// <param name="wholeWord">True if we should search whole word only.</param>
        internal void HighlightAllInstancesOf(string search, bool caseSensitive, bool wholeWord)
            this.highlightedSpans.Clear();
            if (search == null || search.Trim().Length == 0)
                this.BuildParagraph();
                this.OnNotifyPropertyChanged("HighlightCount");
            string text = this.textBuilder.ToString();
            StringComparison comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int start = 0;
            int match;
            while ((match = text.IndexOf(search, start, comparison)) != -1)
                // false loop
                    if (wholeWord)
                        if (match > 0 && char.IsLetterOrDigit(text[match - 1]))
                        if ((match + search.Length <= text.Length - 1) && char.IsLetterOrDigit(text[match + search.Length]))
                    this.AddHighlight(match, search.Length);
                while (false);
                start = match + search.Length;
        /// Adds text to the paragraph later build with BuildParagraph.
        /// <param name="str">Text to be added.</param>
        /// <param name="bold">True if the text should be bold.</param>
        internal void AddText(string str, bool bold)
            ArgumentNullException.ThrowIfNull(str);
            if (str.Length == 0)
            if (bold)
                this.boldSpans.Add(new TextSpan(this.textBuilder.Length, str.Length));
            this.textBuilder.Append(str);
        /// Called before a derived class starts adding text
        /// to reset the current content.
        internal void ResetAllText()
            this.boldSpans.Clear();
            this.textBuilder.Clear();
        /// Adds an inline to <paramref name="currentParagraph"/> based on the remaining parameters.
        /// <param name="currentParagraph">Paragraph to add Inline to.</param>
        /// <param name="currentBold">True if text should be added in bold.</param>
        /// <param name="currentHighlighted">True if the text should be added with highlight.</param>
        /// <param name="sequence">The text to add and clear.</param>
        private static void AddInline(Paragraph currentParagraph, bool currentBold, bool currentHighlighted, StringBuilder sequence)
            if (sequence.Length == 0)
            Run run = new Run(sequence.ToString());
            if (currentHighlighted)
                run.Background = ParagraphSearcher.HighlightBrush;
            Inline inline = currentBold ? (Inline)new Bold(run) : run;
            currentParagraph.Inlines.Add(inline);
            sequence.Clear();
        /// This is an auxiliar method in BuildParagraph to move the current bold or highlighted spans
        /// according to the <paramref name="caracterPosition"/>
        /// The current bold and highlighted span should be ending ahead of the current position.
        /// Moves <paramref name="currentSpanIndex"/> and <paramref name="currentSpan"/> to the
        /// proper span in <paramref name="allSpans"/> according to the <paramref name="caracterPosition"/>
        /// This is an auxiliar method in BuildParagraph.
        /// <param name="currentSpanIndex">Current index within <paramref name="allSpans"/>.</param>
        /// <param name="currentSpan">Current span within <paramref name="allSpans"/>.</param>
        /// <param name="caracterPosition">Character position. This comes from a position within this.textBuilder.</param>
        /// <param name="allSpans">The collection of spans. This is either this.boldSpans or this.highlightedSpans.</param>
        private static void MoveSpanToPosition(ref int currentSpanIndex, ref TextSpan? currentSpan, int caracterPosition, List<TextSpan> allSpans)
            if (currentSpan == null || caracterPosition <= currentSpan.Value.End)
            for (int newBoldIndex = currentSpanIndex + 1; newBoldIndex < allSpans.Count; newBoldIndex++)
                TextSpan newBoldSpan = allSpans[newBoldIndex];
                if (caracterPosition <= newBoldSpan.End)
                    currentSpanIndex = newBoldIndex;
                    currentSpan = newBoldSpan;
            // there is no span ending ahead of current position, so
            // we set the current span to null to prevent unnecessary comparisons against the currentSpan
            currentSpan = null;
        /// Adds one individual text highlight
        /// <param name="start">Highlight start.</param>
        /// <param name="length">Highlight length.</param>
        private void AddHighlight(int start, int length)
            ArgumentOutOfRangeException.ThrowIfNegative(start);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start + length, this.textBuilder.Length, nameof(length));
            this.highlightedSpans.Add(new TextSpan(start, length));
        /// A text span used to mark bold and highlighted segments.
        internal struct TextSpan
            /// Index of the first character in the span.
            private readonly int start;
            /// Index of the last character in the span.
            private readonly int end;
            /// Initializes a new instance of the TextSpan struct.
            /// <param name="start">Index of the first character in the span.</param>
            /// <param name="length">Index of the last character in the span.</param>
            internal TextSpan(int start, int length)
                ArgumentOutOfRangeException.ThrowIfLessThan(length, 1);
                this.start = start;
                this.end = start + length - 1;
            /// Gets the index of the first character in the span.
            internal int Start
                get { return this.start; }
            internal int End
                    return this.end;
            /// Returns true if the <paramref name="position"/> is between start and end (inclusive).
            /// <param name="position">Position to verify if is in the span.</param>
            /// <returns>True if the <paramref name="position"/> is between start and end (inclusive).</returns>
            internal bool Contains(int position)
                return (position >= this.start) && (position <= this.end);
