    /// Defines the exception thrown for all Metadata errors.
    public class MetadataException : RuntimeException
        internal const string MetadataMemberInitialization = "MetadataMemberInitialization";
        internal const string BaseName = "Metadata";
        /// Initializes a new instance of MetadataException with serialization parameters.
        protected MetadataException(SerializationInfo info, StreamingContext context)
        /// Initializes a new instance of MetadataException with the message set
        /// to typeof(MetadataException).FullName.
        public MetadataException() : base(typeof(MetadataException).FullName)
            SetErrorCategory(ErrorCategory.MetadataError);
        /// Initializes a new instance of MetadataException setting the message.
        public MetadataException(string message) : base(message)
        /// Initializes a new instance of MetadataException setting the message and innerException.
        public MetadataException(string message, Exception innerException) : base(message, innerException)
        internal MetadataException(
                  StringUtil.Format(resourceStr, arguments),
    /// Defines the exception thrown for all Validate attributes.
    public class ValidationMetadataException : MetadataException
        internal const string ValidateRangeElementType = "ValidateRangeElementType";
        internal const string ValidateRangePositiveFailure = "ValidateRangePositiveFailure";
        internal const string ValidateRangeNonNegativeFailure = "ValidateRangeNonNegativeFailure";
        internal const string ValidateRangeNegativeFailure = "ValidateRangeNegativeFailure";
        internal const string ValidateRangeNonPositiveFailure = "ValidateRangeNonPositiveFailure";
        internal const string ValidateRangeMinRangeMaxRangeType = "ValidateRangeMinRangeMaxRangeType";
        internal const string ValidateRangeNotIComparable = "ValidateRangeNotIComparable";
        internal const string ValidateRangeMaxRangeSmallerThanMinRange = "ValidateRangeMaxRangeSmallerThanMinRange";
        internal const string ValidateRangeGreaterThanMaxRangeFailure = "ValidateRangeGreaterThanMaxRangeFailure";
        internal const string ValidateRangeSmallerThanMinRangeFailure = "ValidateRangeSmallerThanMinRangeFailure";
        internal const string ValidateFailureResult = "ValidateFailureResult";
        internal const string ValidatePatternFailure = "ValidatePatternFailure";
        internal const string ValidateScriptFailure = "ValidateScriptFailure";
        internal const string ValidateCountNotInArray = "ValidateCountNotInArray";
        internal const string ValidateCountMaxLengthSmallerThanMinLength = "ValidateCountMaxLengthSmallerThanMinLength";
        internal const string ValidateCountMinLengthFailure = "ValidateCountMinLengthFailure";
        internal const string ValidateCountMaxLengthFailure = "ValidateCountMaxLengthFailure";
        internal const string ValidateLengthMaxLengthSmallerThanMinLength = "ValidateLengthMaxLengthSmallerThanMinLength";
        internal const string ValidateLengthNotString = "ValidateLengthNotString";
        internal const string ValidateLengthMinLengthFailure = "ValidateLengthMinLengthFailure";
        internal const string ValidateLengthMaxLengthFailure = "ValidateLengthMaxLengthFailure";
        internal const string ValidateSetFailure = "ValidateSetFailure";
        internal const string ValidateVersionFailure = "ValidateVersionFailure";
        internal const string InvalidValueFailure = "InvalidValueFailure";
        /// Initializes a new instance of ValidationMetadataException with serialization parameters.
        protected ValidationMetadataException(SerializationInfo info, StreamingContext context)
        /// Initializes a new instance of ValidationMetadataException with the message set
        /// to typeof(ValidationMetadataException).FullName.
        public ValidationMetadataException() : base(typeof(ValidationMetadataException).FullName) { }
        /// Initializes a new instance of ValidationMetadataException setting the message.
        public ValidationMetadataException(string message) : this(message, false) { }
        /// Initializes a new instance of ValidationMetadataException setting the message and innerException.
        public ValidationMetadataException(string message, Exception innerException) : base(message, innerException) { }
        internal ValidationMetadataException(
            : base(errorId, innerException, resourceStr, arguments)
        /// Initialize a new instance of ValidationMetadataException. This validation exception could be
        /// ignored in positional binding phase if the <para>swallowException</para> is set to be true.
        /// The error message</param>
        /// <param name="swallowException">
        /// Indicate whether to swallow this exception in positional binding phase
        internal ValidationMetadataException(string message, bool swallowException) : base(message)
            _swallowException = swallowException;
        /// Make the positional binding swallow this exception when it's set to true.
        /// This property is only used internally in the positional binding phase
        internal bool SwallowException
            get { return _swallowException; }
        private readonly bool _swallowException = false;
    /// Defines the exception thrown for all ArgumentTransformation attributes.
    public class ArgumentTransformationMetadataException : MetadataException
        internal const string ArgumentTransformationArgumentsShouldBeStrings = "ArgumentTransformationArgumentsShouldBeStrings";
        /// Initializes a new instance of ArgumentTransformationMetadataException with serialization parameters.
        protected ArgumentTransformationMetadataException(SerializationInfo info, StreamingContext context)
        /// Initializes a new instance of ArgumentTransformationMetadataException with the message set
        /// to typeof(ArgumentTransformationMetadataException).FullName.
        public ArgumentTransformationMetadataException()
            : base(typeof(ArgumentTransformationMetadataException).FullName) { }
        /// Initializes a new instance of ArgumentTransformationMetadataException setting the message.
        public ArgumentTransformationMetadataException(string message)
            : base(message) { }
        /// Initializes a new instance of ArgumentTransformationMetadataException setting the message and innerException.
        public ArgumentTransformationMetadataException(string message, Exception innerException)
            : base(message, innerException) { }
        internal ArgumentTransformationMetadataException(
    /// Defines the exception thrown for all parameter binding exceptions related to metadata attributes.
    public class ParsingMetadataException : MetadataException
        internal const string ParsingTooManyParameterSets = "ParsingTooManyParameterSets";
        /// Initializes a new instance of ParsingMetadataException with serialization parameters.
        protected ParsingMetadataException(SerializationInfo info, StreamingContext context)
        /// Initializes a new instance of ParsingMetadataException with the message set
        /// to typeof(ParsingMetadataException).FullName.
        public ParsingMetadataException()
            : base(typeof(ParsingMetadataException).FullName) { }
        /// Initializes a new instance of ParsingMetadataException setting the message.
        public ParsingMetadataException(string message)
        /// Initializes a new instance of ParsingMetadataException setting the message and innerException.
        public ParsingMetadataException(string message, Exception innerException)
        internal ParsingMetadataException(
