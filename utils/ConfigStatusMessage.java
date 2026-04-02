 * The {@link ConfigStatusMessage} is a domain object for a configuration status message. It contains the name
 * of the configuration parameter, the {@link ConfigStatusMessage.Type} information, the internationalized message and
 * an optional status code.
 * The framework will take care of setting the corresponding internationalized message. For this purpose there must be
 * an i18n properties file inside the bundle of the {@link ConfigStatusProvider} that has a message declared for the
 * {@link ConfigStatusMessage#messageKey}. The actual message key is built by
 * {@link ConfigStatusMessage.Builder#withMessageKeySuffix(String)} in the manner that the given message key suffix is
 * appended to <code>config-status.config-status-message-type.</code>. As a result depending on the type of the message
 * the final constructed message keys are:
 * <li>config-status.information.any-suffix</li>
 * <li>config-status.warning.any-suffix</li>
 * <li>config-status.error.any-suffix</li>
 * <li>config-status.pending.any-suffix</li>
 * @author Chris Jackson - Add withMessageKey and remove message from other methods
public final class ConfigStatusMessage {
     * The {@link Type} defines an enumeration of all supported types for a configuration status message.
         * The type for an information message. It is used to provide some general information about a configuration
        INFORMATION,
         * The type for a warning message. It should be used if there might be some issue with the configuration
        WARNING,
         * The type for an error message. It should be used if there is a severe issue with the configuration parameter.
        ERROR,
         * The type for a pending message. It should be used if the transmission of the configuration parameter to the
         * entity is pending.
        PENDING
    /** The name of the configuration parameter. */
    public final String parameterName;
    /** The {@link Type} of the configuration status message. */
    public final Type type;
    /** The key for the message to be internalized. */
    final transient @Nullable String messageKey;
    /** The arguments to be injected into the internationalized message. */
    final transient Object @Nullable [] arguments;
    /** The corresponding internationalized status message. */
    public final @Nullable String message;
     * The optional status code of the configuration status message; to be used if there are additional information to
     * be provided.
    public final @Nullable Integer statusCode;
     * Package protected default constructor to allow reflective instantiation.
     * !!! DO NOT REMOVE - Gson needs it !!!
    ConfigStatusMessage() {
        this(null, null, null, null, null, null);
     * Creates a new {@link ConfigStatusMessage}.
     * @param builder the configuration status message builder
    private ConfigStatusMessage(Builder builder) {
        this(builder.parameterName, builder.type, builder.messageKey, builder.arguments, null, builder.statusCode);
     * Creates a new {@link ConfigStatusMessage} with an internationalized message.
     * @param parameterName the name of the configuration parameter
     * @param type the {@link Type} of the configuration status message
     * @param message the corresponding internationalized status message
     * @param statusCode the optional status code
    ConfigStatusMessage(String parameterName, Type type, String message, @Nullable Integer statusCode) {
        this(parameterName, type, null, null, message, statusCode);
    private ConfigStatusMessage(String parameterName, Type type, @Nullable String messageKey,
            Object @Nullable [] arguments, @Nullable String message, @Nullable Integer statusCode) {
        this.parameterName = parameterName;
        this.messageKey = messageKey;
        this.arguments = arguments;
        this.statusCode = statusCode;
     * The builder for a {@link ConfigStatusMessage} object.
        private static final String CONFIG_STATUS_MSG_KEY_PREFIX = "config-status.";
        private final String parameterName;
        private final Type type;
        private @Nullable String messageKey;
        private Object @Nullable [] arguments;
        private @Nullable Integer statusCode;
        private Builder(String parameterName, Type type) {
            Objects.requireNonNull(parameterName, "Parameter name must not be null.");
            Objects.requireNonNull(type, "Type must not be null.");
         * Creates a builder for the construction of a {@link ConfigStatusMessage} having type
         * {@link Type#INFORMATION}.
         * @param parameterName the name of the configuration parameter (must not be null)
         * @return the new builder instance
        public static Builder information(String parameterName) {
            return new Builder(parameterName, Type.INFORMATION);
         * Creates a builder for the construction of a {@link ConfigStatusMessage} having type {@link Type#WARNING}.
        public static Builder warning(String parameterName) {
            return new Builder(parameterName, Type.WARNING);
         * Creates a builder for the construction of a {@link ConfigStatusMessage} having type {@link Type#ERROR}.
        public static Builder error(String parameterName) {
            return new Builder(parameterName, Type.ERROR);
         * Creates a builder for the construction of a {@link ConfigStatusMessage} having type {@link Type#PENDING}.
        public static Builder pending(String parameterName) {
            return new Builder(parameterName, Type.PENDING);
         * Adds the given message key suffix for the creation of {@link ConfigStatusMessage#messageKey}.
         * @param messageKeySuffix the message key suffix to be added
         * @return the updated builder
        public Builder withMessageKeySuffix(String messageKeySuffix) {
            this.messageKey = CONFIG_STATUS_MSG_KEY_PREFIX + type.name().toLowerCase() + "." + messageKeySuffix;
         * Adds the given arguments (to be injected into the internationalized message) to the builder.
         * @param arguments the arguments to be added
        public Builder withArguments(Object... arguments) {
         * Adds the given status code to the builder.
         * @param statusCode the status code to be added
        public Builder withStatusCode(Integer statusCode) {
         * Builds the new {@link ConfigStatusMessage} object.
         * @return new {@link ConfigStatusMessage} object.
        public ConfigStatusMessage build() {
            return new ConfigStatusMessage(this);
        result = prime * result + ((message == null) ? 0 : message.hashCode());
        result = prime * result + ((parameterName == null) ? 0 : parameterName.hashCode());
        result = prime * result + ((statusCode == null) ? 0 : statusCode.hashCode());
        result = prime * result + ((type == null) ? 0 : type.hashCode());
        ConfigStatusMessage other = (ConfigStatusMessage) obj;
        if (message == null) {
            if (other.message != null) {
        } else if (!message.equals(other.message)) {
        if (parameterName == null) {
            if (other.parameterName != null) {
        } else if (!parameterName.equals(other.parameterName)) {
        if (statusCode == null) {
            if (other.statusCode != null) {
        } else if (!statusCode.equals(other.statusCode)) {
        if (type != other.type) {
        return "ConfigStatusMessage [parameterName=" + parameterName + ", type=" + type + ", messageKey=" + messageKey
                + ", arguments=" + Arrays.toString(arguments) + ", message=" + message + ", statusCode=" + statusCode
                + "]";
