    /// Moves through search highlights built in a ParagraphBuilder
    /// changing the color of the current highlight.
    internal class ParagraphSearcher
        /// Highlight for all matches except the current.
        internal static readonly Brush HighlightBrush = Brushes.Yellow;
        /// Highlight for the current match.
        private static readonly Brush CurrentHighlightBrush = Brushes.Cyan;
        /// Current match being highlighted in search.
        private Run currentHighlightedMatch;
        /// Initializes a new instance of the ParagraphSearcher class.
        internal ParagraphSearcher()
        /// Move to the next highlight starting at the <paramref name="caretPosition"/>.
        /// <param name="forward">True for next false for previous.</param>
        /// <param name="caretPosition">Caret position.</param>
        /// <returns>The next highlight starting at the <paramref name="caretPosition"/>.</returns>
        internal Run MoveAndHighlightNextNextMatch(bool forward, TextPointer caretPosition)
            Debug.Assert(caretPosition != null, "a caret position is always valid");
            Debug.Assert(caretPosition.Parent != null && caretPosition.Parent is Run, "a caret Parent is always a valid Run");
            Run caretRun = (Run)caretPosition.Parent;
            Run currentRun;
            if (this.currentHighlightedMatch != null)
                // restore the curent highlighted background to plain highlighted
                this.currentHighlightedMatch.Background = ParagraphSearcher.HighlightBrush;
            // If the caret is in the end of a highlight we move to the adjacent run
            // It has to be in the end because if there is a match at the beginning of the file
            // and the caret has not been touched (so it is in the beginning of the file too)
            // we want to highlight this first match.
            // Considering the caller always set the caret to the end of the highlight
            // The condition below works well for successive searchs
            // We also need to move to the adjacent run if the caret is at the first run and we
            // are moving backwards so that a search backwards when the first run is highlighted
            // and the caret is at the beginning will wrap to the end
            if ((!forward && IsFirstRun(caretRun)) ||
                ((caretPosition.GetOffsetToPosition(caretRun.ContentEnd) == 0) && ParagraphSearcher.Ishighlighted(caretRun)))
                currentRun = ParagraphSearcher.GetNextRun(caretRun, forward);
                currentRun = caretRun;
            currentRun = ParagraphSearcher.GetNextMatch(currentRun, forward);
            if (currentRun == null)
                // if we could not find a next highlight wraparound
                currentRun = ParagraphSearcher.GetFirstOrLastRun(caretRun, forward);
            this.currentHighlightedMatch = currentRun;
                // restore the curent highlighted background to current highlighted
                this.currentHighlightedMatch.Background = ParagraphSearcher.CurrentHighlightBrush;
            return currentRun;
        /// Resets the search for fresh calls to MoveAndHighlightNextNextMatch.
        internal void ResetSearch()
            this.currentHighlightedMatch = null;
        /// Returns true if <paramref name="run"/> is highlighted.
        /// <param name="run">Run to check if is highlighted.</param>
        /// <returns>True if <paramref name="run"/> is highlighted.</returns>
        private static bool Ishighlighted(Run run)
            SolidColorBrush background = run.Background as SolidColorBrush;
            if (background != null && background == ParagraphSearcher.HighlightBrush)
        /// Get the next or previous run according to <paramref name="forward"/>.
        /// <param name="currentRun">The current run.</param>
        /// <returns>The next or previous run according to <paramref name="forward"/>.</returns>
        private static Run GetNextRun(Run currentRun, bool forward)
            Bold parentBold = currentRun.Parent as Bold;
            Inline nextInline;
            if (forward)
                nextInline = parentBold != null ? ((Inline)parentBold).NextInline : currentRun.NextInline;
                nextInline = parentBold != null ? ((Inline)parentBold).PreviousInline : currentRun.PreviousInline;
            return GetRun(nextInline);
        /// Gets the run of an inline. Inlines in a ParagrahBuilder are either a Run or a Bold
        /// which contains a Run.
        /// <param name="inline">Inline to get the run from.</param>
        /// <returns>The run of the inline.</returns>
        private static Run GetRun(Inline inline)
            Bold inlineBold = inline as Bold;
            if (inlineBold != null)
                return (Run)inlineBold.Inlines.FirstInline;
            return (Run)inline;
        /// Gets the next highlighted run starting and including <paramref name="currentRun"/>
        /// according to the direction specified in <paramref name="forward"/>.
        /// <returns>
        /// the next highlighted run starting and including <paramref name="currentRun"/>
        /// </returns>
        private static Run GetNextMatch(Run currentRun, bool forward)
            while (currentRun != null)
                if (ParagraphSearcher.Ishighlighted(currentRun))
                currentRun = ParagraphSearcher.GetNextRun(currentRun, forward);
        /// Gets the run's paragraph.
        /// <param name="run">Run to get the paragraph from.</param>
        /// <returns>The run's paragraph.</returns>
        private static Paragraph GetParagraph(Run run)
            Bold parentBold = run.Parent as Bold;
            Paragraph parentParagraph = (parentBold != null ? parentBold.Parent : run.Parent) as Paragraph;
            Debug.Assert(parentParagraph != null, "the documents we are saerching are built with ParagraphBuilder, which builds the document like this");
            return parentParagraph;
        /// Returns true if the run is the first run of the paragraph.
        /// <param name="run">Run to check.</param>
        /// <returns>True if the run is the first run of the paragraph.</returns>
        private static bool IsFirstRun(Run run)
            Paragraph paragraph = GetParagraph(run);
            Run firstRun = ParagraphSearcher.GetRun(paragraph.Inlines.FirstInline);
            return run == firstRun;
        /// Gets the first or lasr run in the paragraph containing <paramref name="caretRun"/>.
        /// <param name="caretRun">Run containing the caret.</param>
        /// <param name="forward">True for first false for last.</param>
        /// <returns>The first or last run in the paragraph containing <paramref name="caretRun"/>.</returns>
        private static Run GetFirstOrLastRun(Run caretRun, bool forward)
            Debug.Assert(caretRun != null, "a caret run is always valid");
            Paragraph paragraph = GetParagraph(caretRun);
            Inline firstOrLastInline;
                firstOrLastInline = paragraph.Inlines.FirstInline;
                firstOrLastInline = paragraph.Inlines.LastInline;
            return GetRun(firstOrLastInline);
