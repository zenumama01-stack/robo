    /// ProgressNode is an augmentation of the ProgressRecord type that adds extra fields for the purposes of tracking
    /// outstanding activities received by the host, and rendering them in the console.
    ProgressNode : ProgressRecord
        /// Indicates the various layouts for rendering a particular node.
        enum
        RenderStyle
            Invisible = 0,
            Minimal = 1,
            Compact = 2,
            /// Allocate only one line for displaying the StatusDescription or the CurrentOperation,
            /// truncate the rest if the StatusDescription or CurrentOperation doesn't fit in one line.
            Full = 3,
            /// The node will be displayed the same as Full, plus, the whole StatusDescription and CurrentOperation will be displayed (in multiple lines if needed).
            FullPlus = 4,
            /// The node will be displayed using ANSI escape sequences.
            Ansi = 5,
        /// Constructs an instance from a ProgressRecord.
        ProgressNode(long sourceId, ProgressRecord record)
            : base(record.ActivityId, record.Activity, record.StatusDescription)
            Dbg.Assert(record.RecordType == ProgressRecordType.Processing, "should only create node for Processing records");
            this.ParentActivityId = record.ParentActivityId;
            this.CurrentOperation = record.CurrentOperation;
            this.PercentComplete = Math.Min(record.PercentComplete, 100);
            this.SecondsRemaining = record.SecondsRemaining;
            this.RecordType = record.RecordType;
            this.Style = IsMinimalProgressRenderingEnabled()
                ? RenderStyle.Ansi
                : this.Style = RenderStyle.FullPlus;
            this.SourceId = sourceId;
        /// Renders a single progress node as strings of text according to that node's style. The text is appended to the
        /// supplied list of strings.
        /// <param name="strCollection">
        /// List of strings to which the node's rendering will be appended.
        /// The indentation level (in BufferCells) at which the node should be rendered.
        /// The maximum number of BufferCells that the rendering is allowed to consume.
        Render(ArrayList strCollection, int indentation, int maxWidth, PSHostRawUserInterface rawUI)
            Dbg.Assert(strCollection != null, "strCollection should not be null");
            Dbg.Assert(indentation >= 0, "indentation is negative");
            Dbg.Assert(this.RecordType != ProgressRecordType.Completed, "should never render completed records");
            switch (Style)
                case RenderStyle.FullPlus:
                    RenderFull(strCollection, indentation, maxWidth, rawUI, isFullPlus: true);
                case RenderStyle.Full:
                    RenderFull(strCollection, indentation, maxWidth, rawUI, isFullPlus: false);
                case RenderStyle.Compact:
                    RenderCompact(strCollection, indentation, maxWidth, rawUI);
                case RenderStyle.Minimal:
                    RenderMinimal(strCollection, indentation, maxWidth, rawUI);
                case RenderStyle.Ansi:
                    RenderAnsi(strCollection, indentation, maxWidth);
                case RenderStyle.Invisible:
                    // do nothing
                    Dbg.Assert(false, "unrecognized RenderStyle value");
        /// Renders a node in the "Full" style.
        /// <param name="isFullPlus">
        /// Indicate if the full StatusDescription and CurrentOperation should be displayed.
        RenderFull(ArrayList strCollection, int indentation, int maxWidth, PSHostRawUserInterface rawUI, bool isFullPlus)
            string indent = StringUtil.Padding(indentation);
            // First line: the activity
            strCollection.Add(
                StringUtil.TruncateToBufferCellWidth(
                    rawUI, StringUtil.Format(" {0}{1} ", indent, this.Activity), maxWidth));
            indentation += 3;
            indent = StringUtil.Padding(indentation);
            // Second line: the status description
            RenderFullDescription(this.StatusDescription, indent, maxWidth, rawUI, strCollection, isFullPlus);
            // Third line: the percentage thermometer. The size of this is proportional to the width we're allowed
            // to consume. -2 for the whitespace, -2 again for the brackets around thermo, -5 to not be too big
            if (PercentComplete >= 0)
                int thermoWidth = Math.Max(3, maxWidth - indentation - 2 - 2 - 5);
                int mercuryWidth = 0;
                mercuryWidth = PercentComplete * thermoWidth / 100;
                if (PercentComplete < 100 && mercuryWidth == thermoWidth)
                    // back off a tad unless we're totally complete to prevent the appearance of completion before
                    // the fact.
                    --mercuryWidth;
                        rawUI,
                            " {0}[{1}{2}] ",
                            indent,
                            new string('o', mercuryWidth),
                            StringUtil.Padding(thermoWidth - mercuryWidth)),
                        maxWidth));
            // Fourth line: the seconds remaining
            if (SecondsRemaining >= 0)
                TimeSpan span = new TimeSpan(0, 0, this.SecondsRemaining);
                            ProgressNodeStrings.SecondsRemaining,
                            span)
                        + " ",
            // Fifth and Sixth lines: The current operation
            if (!string.IsNullOrEmpty(CurrentOperation))
                strCollection.Add(" ");
                RenderFullDescription(this.CurrentOperation, indent, maxWidth, rawUI, strCollection, isFullPlus);
        private static void RenderFullDescription(string description, string indent, int maxWidth, PSHostRawUserInterface rawUi, ArrayList strCollection, bool isFullPlus)
            string oldDescription = StringUtil.Format(" {0}{1} ", indent, description);
            string newDescription;
                newDescription = StringUtil.TruncateToBufferCellWidth(rawUi, oldDescription, maxWidth);
                strCollection.Add(newDescription);
                if (oldDescription.Length == newDescription.Length)
                    oldDescription = StringUtil.Format(" {0}{1}", indent, oldDescription.Substring(newDescription.Length));
            } while (isFullPlus);
        /// Renders a node in the "Compact" style.
        RenderCompact(ArrayList strCollection, int indentation, int maxWidth, PSHostRawUserInterface rawUI)
                    StringUtil.Format(" {0}{1} ", indent, this.Activity), maxWidth));
            // Second line: the status description with percentage and time remaining, if applicable.
            string percent = string.Empty;
                percent = StringUtil.Format("{0}% ", PercentComplete);
            string secRemain = string.Empty;
                TimeSpan span = new TimeSpan(0, 0, SecondsRemaining);
                secRemain = span.ToString() + " ";
                        " {0}{1}{2}{3} ",
                        percent,
                        secRemain,
                        StatusDescription),
            // Third line: The current operation
                        StringUtil.Format(" {0}{1} ", indent, this.CurrentOperation), maxWidth));
        /// Renders a node in the "Minimal" style.
        RenderMinimal(ArrayList strCollection, int indentation, int maxWidth, PSHostRawUserInterface rawUI)
            // First line: Everything mushed into one line
                        " {0}{1} {2}{3}{4} ",
                        Activity,
        internal static bool IsMinimalProgressRenderingEnabled()
            return PSStyle.Instance.Progress.View == ProgressView.Minimal;
        /// Renders a node in the "ANSI" style.
        /// The indentation level in chars at which the node should be rendered.
        /// The maximum number of chars that the rendering is allowed to consume.
        RenderAnsi(ArrayList strCollection, int indentation, int maxWidth)
                secRemain = SecondsRemaining.ToString() + "s";
            int secRemainLength = secRemain.Length + 1;
            // limit progress bar to 120 chars as no need to render full width
            if (PSStyle.Instance.Progress.MaxWidth > 0 && maxWidth > PSStyle.Instance.Progress.MaxWidth)
                maxWidth = PSStyle.Instance.Progress.MaxWidth;
            // if the activity is really long, only use up to half the width
            string activity;
            if (Activity.Length > maxWidth / 2)
                activity = Activity.Substring(0, maxWidth / 2) + PSObjectHelper.Ellipsis;
                activity = Activity;
            // 4 is for the extra space and square brackets below and one extra space
            int barWidth = maxWidth - activity.Length - indentation - 4;
            int padding = maxWidth + PSStyle.Instance.Progress.Style.Length + PSStyle.Instance.Reverse.Length + PSStyle.Instance.ReverseOff.Length;
            sb.Append(PSStyle.Instance.Reverse);
            int maxStatusLength = barWidth - secRemainLength - 1;
            if (maxStatusLength > 0 && StatusDescription.Length > barWidth - secRemainLength)
                sb.Append(StatusDescription.AsSpan(0, barWidth - secRemainLength - 1));
                sb.Append(PSObjectHelper.Ellipsis);
                sb.Append(StatusDescription);
            int emptyPadLength = barWidth + PSStyle.Instance.Reverse.Length - sb.Length - secRemainLength;
            if (emptyPadLength > 0)
                sb.Append(string.Empty.PadRight(emptyPadLength));
            sb.Append(secRemain);
            if (PercentComplete >= 0 && PercentComplete < 100 && barWidth > 0)
                int barLength = PercentComplete * barWidth / 100;
                if (barLength >= barWidth)
                    barLength = barWidth - 1;
                if (barLength < sb.Length)
                    sb.Insert(barLength + PSStyle.Instance.Reverse.Length, PSStyle.Instance.ReverseOff);
                sb.Append(PSStyle.Instance.ReverseOff);
                    "{0}{1}{2} [{3}]{4}",
                    PSStyle.Instance.Progress.Style,
                    sb.ToString(),
                    PSStyle.Instance.Reset)
                .PadRight(padding));
        /// The nodes that have this node as their parent.
        Children;
        /// The "age" of the node.  A node's age is incremented by PendingProgress.Update each time a new ProgressRecord is
        /// received by the host. A node's age is reset when a corresponding ProgressRecord is received.  Thus, the age of
        /// a node reflects the number of ProgressRecord that have been received since the node was last updated.
        /// The age is used by PendingProgress.Render to determine which nodes should be rendered on the display, and how. As the
        /// display has finite size, it may be possible to have many more outstanding progress activities than will fit in that
        /// space. The rendering of nodes can be progressively "compressed" into a more terse format, or not rendered at all in
        /// order to fit as many nodes as possible in the available space. The oldest nodes are compressed or skipped first.
        Age;
        /// The style in which this node should be rendered.
        Style = RenderStyle.FullPlus;
        /// Identifies the source of the progress record.
        long
        SourceId;
        /// The number of vertical BufferCells that are required to render the node in its current style.
        internal int LinesRequiredMethod(PSHostRawUserInterface rawUi, int maxWidth)
                    return LinesRequiredInFullStyleMethod(rawUi, maxWidth, isFullPlus: true);
                    return LinesRequiredInFullStyleMethod(rawUi, maxWidth, isFullPlus: false);
                    return LinesRequiredInCompactStyle;
                    Dbg.Assert(false, "Unknown RenderStyle value");
        /// The number of vertical BufferCells that are required to render the node in the Full style.
        private int LinesRequiredInFullStyleMethod(PSHostRawUserInterface rawUi, int maxWidth, bool isFullPlus)
            // Since the fields of this instance could have been changed, we compute this on-the-fly.
            // NTRAID#Windows OS Bugs-1062104-2004/12/15-sburns we assume 1 line for each field.  If we ever need to
            // word-wrap text fields, then this calculation will need updating.
            // Start with 1 for the Activity
            int lines = 1;
            // Use 5 spaces as the heuristic indent. 5 spaces stand for the indent for the CurrentOperation of the first-level child node
            var indent = StringUtil.Padding(5);
            var temp = new ArrayList();
            if (isFullPlus)
                temp.Clear();
                RenderFullDescription(StatusDescription, indent, maxWidth, rawUi, temp, isFullPlus: true);
                lines += temp.Count;
                // 1 for the Status
                lines++;
                ++lines;
                    lines += 1;
                    RenderFullDescription(CurrentOperation, indent, maxWidth, rawUi, temp, isFullPlus: true);
                    lines += 2;
            return lines;
        /// The number of vertical BufferCells that are required to render the node in the Compact style.
        LinesRequiredInCompactStyle
                // Start with 1 for the Activity, and 1 for the Status.
                int lines = 2;
