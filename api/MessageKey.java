 * Utility class providing the {@link MessageKey}s for config description validation. The {@link MessageKey}
 * consists of a key to be used for internationalization and a general default text.
final class MessageKey {
    static final MessageKey PARAMETER_REQUIRED = new MessageKey("parameter_required", "The parameter is required.");
    static final MessageKey DATA_TYPE_VIOLATED = new MessageKey("data_type_violated",
            "The data type of the value ({0}) does not match with the type declaration ({1}) in the configuration description.");
    static final MessageKey MAX_VALUE_TXT_VIOLATED = new MessageKey("max_value_txt_violated",
            "The value must not consist of more than {0} characters.");
    static final MessageKey MAX_VALUE_NUMERIC_VIOLATED = new MessageKey("max_value_numeric_violated",
            "The value must not be greater than {0}.");
    static final MessageKey MIN_VALUE_TXT_VIOLATED = new MessageKey("min_value_txt_violated",
            "The value must not consist of less than {0} characters.");
    static final MessageKey MIN_VALUE_NUMERIC_VIOLATED = new MessageKey("min_value_numeric_violated",
            "The value must not be less than {0}.");
    static final MessageKey PATTERN_VIOLATED = new MessageKey("pattern_violated",
            "The value {0} does not match the pattern {1}.");
    static final MessageKey OPTIONS_VIOLATED = new MessageKey("options_violated",
            "The value {0} does not match allowed parameter options. Allowed options are: {1}");
    static final MessageKey MULTIPLE_LIMIT_VIOLATED = new MessageKey("multiple_limit_violated",
            "Only {0} elements are allowed but {1} are provided.");
    /** The key to be used for internationalization. */
    final String key;
    /** The default message. */
    final String defaultMessage;
    private MessageKey(String key, String defaultMessage) {
        this.defaultMessage = defaultMessage;
