 * The {@link ConfigValidationMessage} is the result of a specific {@link ConfigDescriptionParameter}
 * validation, e.g. the validation of the required flag or of the min/max attribute. It contains the name of the
 * configuration parameter whose value does not meet its declaration in the {@link ConfigDescription}, a default
 * message, a message key to be used for internationalization and an optional content to be passed as parameters into
 * the message.
public final class ConfigValidationMessage {
    /** The name of the configuration parameter whose value does not meet its {@link ConfigDescription} declaration. */
    /** The default message describing the validation issue. */
    public final String defaultMessage;
    /** The key of the message to be used for internationalization. */
    public final String messageKey;
    /** The optional content to be passed as message parameters into the message. */
    public final Object[] content;
     * Creates a new {@link ConfigValidationMessage}
     * @param parameterName the parameter name
     * @param defaultMessage the default message
     * @param messageKey the message key to be used for internationalization
    public ConfigValidationMessage(String parameterName, String defaultMessage, String messageKey) {
        this(parameterName, defaultMessage, messageKey, List.of());
     * @param content the content to be passed as parameters into the message
    public ConfigValidationMessage(String parameterName, String defaultMessage, String messageKey, Object... content) {
        Objects.requireNonNull(parameterName, "Parameter Name must not be null");
        Objects.requireNonNull(defaultMessage, "Default message must not be null");
        Objects.requireNonNull(messageKey, "Message key must not be null");
        Objects.requireNonNull(content, "Content must not be null");
        this.content = content;
        result = prime * result + Arrays.hashCode(content);
        result = prime * result + ((defaultMessage == null) ? 0 : defaultMessage.hashCode());
        result = prime * result + ((messageKey == null) ? 0 : messageKey.hashCode());
        ConfigValidationMessage other = (ConfigValidationMessage) obj;
        if (!Arrays.equals(content, other.content)) {
        if (defaultMessage == null) {
            if (other.defaultMessage != null) {
        } else if (!defaultMessage.equals(other.defaultMessage)) {
        if (messageKey == null) {
            if (other.messageKey != null) {
        } else if (!messageKey.equals(other.messageKey)) {
        return "ConfigDescriptionValidationMessage [parameterName=" + parameterName + ", defaultMessage="
                + defaultMessage + ", messageKey=" + messageKey + ", content=" + Arrays.toString(content) + "]";
