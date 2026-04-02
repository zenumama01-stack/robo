namespace System.Management.Automation.Host
    /// Provides a description of a choice for use by <see cref="System.Management.Automation.Host.PSHostUserInterface.PromptForChoice"/>.
    /// <!--Used by the engine to describe cmdlet parameters.-->
    class ChoiceDescription
        #region DO NOT REMOVE OR RENAME THESE FIELDS - it will break remoting compatibility with Windows PowerShell compatibility with Windows PowerShell
        private readonly string label = null;
        private string helpMessage = string.Empty;
        /// Initializes an new instance of ChoiceDescription and defines the Label value.
        /// The label to identify this field description
        /// <exception cref="System.Management.Automation.PSArgumentException">
        /// <paramref name="label"/> is null or empty.
        ChoiceDescription(string label)
            // the only required parameter is label.
                // "label" is not localizable
                throw PSTraceSource.NewArgumentException(nameof(label), DescriptionsStrings.NullOrEmptyErrorTemplate, "label");
            this.label = label;
        /// Initializes an new instance of ChoiceDescription and defines the Label and HelpMessage values.
        /// The label to identify this field description.
        /// <param name="helpMessage">
        /// The help message for this field.
        /// <exception cref="System.Management.Automation.PSArgumentNullException">
        /// <paramref name="helpMessage"/> is null.
        ChoiceDescription(string label, string helpMessage)
            if (helpMessage == null)
                // "helpMessage" is not localizable
                throw PSTraceSource.NewArgumentNullException(nameof(helpMessage));
            this.helpMessage = helpMessage;
        /// Gets a short, human-presentable message to describe and identify the choice.  Think Button label.
        /// Note that the special character &amp; (ampersand) may be embedded in the label string to identify the next character in the label
        /// as a "hot key" (aka "keyboard accelerator") that the Console.PromptForChoice implementation may use to allow the user to
        /// quickly set input focus to this choice.  The implementation of <see cref="System.Management.Automation.Host.PSHostUserInterface.PromptForChoice"/>
        /// is responsible for parsing the label string for this special character and rendering it accordingly.
        /// For examples, a choice named "Yes to All" might have "Yes to &amp;All" as it's label.
        Label
                Dbg.Assert(this.label != null, "label should not be null");
                return this.label;
        /// Gets and sets the help message for this field.
        /// Set to null.
        /// This should be a few sentences to describe the field, suitable for presentation as a tool tip.
        /// Avoid placing including formatting characters such as newline and tab.
        HelpMessage
                Dbg.Assert(this.helpMessage != null, "helpMessage should not be null");
                return this.helpMessage;
                this.helpMessage = value;
