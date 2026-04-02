 * A runtime exception to be thrown if given {@link Configuration} parameters do not match their declaration in the
 * {@link ConfigDescription}.
public final class ConfigValidationException extends RuntimeException {
    private final Logger logger = LoggerFactory.getLogger(ConfigValidationException.class);
    private final Collection<ConfigValidationMessage> configValidationMessages;
     * Creates a new {@link ConfigValidationException} for the given {@link ConfigValidationMessage}s. It
     * requires the bundle from which this exception is thrown in order to be able to provide internationalized
     * messages.
     * @param bundle the bundle from which this exception is thrown
     * @param configValidationMessages the configuration description validation messages
     * @throws NullPointerException if given bundle or configuration description validation messages are null
    public ConfigValidationException(Bundle bundle, TranslationProvider translationProvider,
            Collection<ConfigValidationMessage> configValidationMessages) {
        Objects.requireNonNull(bundle, "Bundle must not be null");
        Objects.requireNonNull(configValidationMessages, "Config validation messages must not be null");
        this.configValidationMessages = configValidationMessages;
     * Retrieves the default validation messages (cp. {@link ConfigValidationMessage#defaultMessage}) for this
     * exception.
     * @return an immutable map of validation messages having affected configuration parameter name as key and the
     *         default message as value
    public Map<String, String> getValidationMessages() {
        Map<String, String> ret = new HashMap<>();
        for (ConfigValidationMessage configValidationMessage : configValidationMessages) {
            ret.put(configValidationMessage.parameterName,
                    MessageFormat.format(configValidationMessage.defaultMessage, configValidationMessage.content));
        return Collections.unmodifiableMap(ret);
     * Retrieves the internationalized validation messages for this exception. If there is no text found to be
     * internationalized then the default message is delivered.
     * If there is no TranslationProvider available then this operation will return the default validation messages by
     * using {@link ConfigValidationException#getValidationMessages()}.
     * @param locale the locale to be used; if null then the default locale will be used
     * @return an immutable map of internationalized validation messages having affected configuration parameter name as
     *         key and the internationalized message as value (in case of there was no text found to be
     *         internationalized then the default message (cp. {@link ConfigValidationMessage#defaultMessage}) is
     *         delivered)
    public Map<String, String> getValidationMessages(Locale locale) {
            if (translationProvider == null) {
                        "TranslationProvider is not available. Will provide default validation message for parameter '{}'.",
                        configValidationMessage.parameterName);
                String text = translationProvider.getText(bundle, configValidationMessage.messageKey,
                        configValidationMessage.defaultMessage, locale, configValidationMessage.content);
                ret.put(configValidationMessage.parameterName, text);
