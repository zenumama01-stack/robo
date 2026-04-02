    /// Possible types of CompletionResults.
    public enum CompletionResultType
        /// <summary> An unknown result type, kept as text only.</summary>
        /// <summary>A history result type like the items out of get-history.</summary>
        History = 1,
        /// <summary>A command result type like the items out of get-command.</summary>
        Command = 2,
        /// <summary>A provider item.</summary>
        ProviderItem = 3,
        /// <summary>A provider container.</summary>
        ProviderContainer = 4,
        /// <summary>A property result type like the property items out of get-member.</summary>
        Property = 5,
        /// <summary>A method result type like the method items out of get-member.</summary>
        Method = 6,
        /// <summary>A parameter name result type like the Parameters property out of get-command items.</summary>
        ParameterName = 7,
        /// <summary>A parameter value result type.</summary>
        ParameterValue = 8,
        /// <summary>A variable result type like the items out of get-childitem variable.</summary>
        Variable = 9,
        /// <summary>A namespace.</summary>
        Namespace = 10,
        /// <summary>A type name.</summary>
        Type = 11,
        /// <summary>A keyword.</summary>
        Keyword = 12,
        /// <summary>A dynamic keyword.</summary>
        DynamicKeyword = 13,
        // If a new enum is added, there is a range test that uses DynamicKeyword for parameter validation
        // that needs to be updated to use the new enum.
        // We can't use a "MaxValue" enum because it's value would preclude ever adding a new enum.
    /// Class used to store a tab completion or Intellisense result.
    public class CompletionResult
        /// Text to be used as the auto completion result.
        private readonly string _completionText;
        /// Text to be displayed in a list.
        private readonly string _listItemText;
        /// The text for the tooltip with details to be displayed about the object.
        private readonly string _toolTip;
        /// Type of completion result.
        private readonly CompletionResultType _resultType;
        /// Private member for null instance.
        private static readonly CompletionResult s_nullInstance = new CompletionResult();
        /// Gets the text to be used as the auto completion result.
        public string CompletionText
                if (this == s_nullInstance)
                    throw PSTraceSource.NewInvalidOperationException(TabCompletionStrings.NoAccessToProperties);
                return _completionText;
        /// Gets the text to be displayed in a list.
        public string ListItemText
                return _listItemText;
        /// Gets the type of completion result.
        public CompletionResultType ResultType
                return _resultType;
        /// Gets the text for the tooltip with details to be displayed about the object.
                return _toolTip;
        /// Gets the null instance of type CompletionResult.
        internal static CompletionResult Null
            get { return s_nullInstance; }
        /// Initializes a new instance of the CompletionResult class.
        /// <param name="completionText">The text to be used as the auto completion result.</param>
        /// <param name="listItemText">The text to be displayed in a list.</param>
        /// <param name="resultType">The type of completion result.</param>
        /// <param name="toolTip">The text for the tooltip with details to be displayed about the object.</param>
        public CompletionResult(string completionText, string listItemText, CompletionResultType resultType, string toolTip)
            ArgumentException.ThrowIfNullOrEmpty(completionText);
            ArgumentException.ThrowIfNullOrEmpty(listItemText);
            ArgumentException.ThrowIfNullOrEmpty(toolTip);
            if (resultType < CompletionResultType.Text || resultType > CompletionResultType.DynamicKeyword)
                throw PSTraceSource.NewArgumentOutOfRangeException(nameof(resultType), resultType);
            _completionText = completionText;
            _listItemText = listItemText;
            _toolTip = toolTip;
            _resultType = resultType;
        /// Initializes a new instance of this class internally if the result out of TabExpansion is a string.
        /// <param name="completionText">Completion text.</param>
        public CompletionResult(string completionText)
            : this(completionText, completionText, CompletionResultType.Text, completionText)
        /// An null instance of CompletionResult.
        /// This can be used in argument completion, to indicate that the completion attempt has gone through the
        /// native command argument completion methods.
        private CompletionResult() { }
