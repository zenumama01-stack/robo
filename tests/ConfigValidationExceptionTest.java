 * Testing the {@link ConfigValidationException}.
public class ConfigValidationExceptionTest {
    private static final String PARAM1 = "param1";
    private static final String PARAM2 = "param2";
    private static final Locale DE = Locale.GERMAN;
    private static final Locale EN = Locale.ENGLISH;
    private static final int MAX = 3;
    private static final String TXT_DE1 = "German 1";
    private static final String TXT_DE2 = MessageFormat.format("German 2 with some {0} parameter", MAX);
    private static final String TXT_EN1 = "English 1";
    private static final String TXT_EN2 = MessageFormat.format("English 2 with some {0} parameter", MAX);
    private static final String TXT_DEFAULT1 = MessageKey.PARAMETER_REQUIRED.defaultMessage;
    private static final String TXT_DEFAULT2 = MessageFormat.format(MessageKey.MAX_VALUE_TXT_VIOLATED.defaultMessage,
            MAX);
    private static final ConfigValidationMessage MSG1 = createMessage(PARAM1, TXT_DEFAULT1,
            MessageKey.PARAMETER_REQUIRED.key, List.of());
    private static final ConfigValidationMessage MSG2 = createMessage(PARAM2, TXT_DEFAULT2,
            MessageKey.MAX_VALUE_TXT_VIOLATED.key, List.of(MAX));
    private static final List<ConfigValidationMessage> ALL = List.of(MSG1, MSG2);
    private static final Bundle BUNDLE = Mockito.mock(Bundle.class);
    private static final TranslationProvider TRANSLATION_PROVIDER = new TranslationProvider() {
        public @Nullable String getText(@Nullable Bundle bundle, @Nullable String key, @Nullable String defaultText,
                @Nullable Locale locale, @Nullable Object @Nullable... arguments) {
            return getText(bundle, key, defaultText, locale);
            if (MessageKey.PARAMETER_REQUIRED.key.equals(key)) {
                if (DE.equals(locale)) {
                    return TXT_DE1;
                    return TXT_EN1;
            } else if (MessageKey.MAX_VALUE_TXT_VIOLATED.key.equals(key)) {
                    return TXT_DE2;
                    return TXT_EN2;
    public void assertThatDefaultMessagesAreProvided() {
        ConfigValidationException configValidationException = new ConfigValidationException(BUNDLE,
                TRANSLATION_PROVIDER, ALL);
        Map<String, String> messages = configValidationException.getValidationMessages();
        assertThat(messages.size(), is(2));
        assertThat(messages.get(PARAM1), is(TXT_DEFAULT1));
        assertThat(messages.get(PARAM2), is(TXT_DEFAULT2));
    public void assertThatInternationalizedMessagesAreProvided() {
        Map<String, String> messages = configValidationException.getValidationMessages(DE);
        assertThat(messages.get(PARAM1), is(TXT_DE1));
        assertThat(messages.get(PARAM2), is(TXT_DE2));
        messages = configValidationException.getValidationMessages(EN);
        assertThat(messages.get(PARAM1), is(TXT_EN1));
        assertThat(messages.get(PARAM2), is(TXT_EN2));
    public void assertThatDefaultMessagesAreProvidedIfNoI18NproviderIsAvailable() {
        ConfigValidationException configValidationException = new ConfigValidationException(BUNDLE, null, ALL);
    public void assertThatNPEIsThrownForNullConfigValidationMessages() {
        assertThrows(NullPointerException.class,
                () -> new ConfigValidationException(BUNDLE, TRANSLATION_PROVIDER, null));
    static ConfigValidationMessage createMessage(String parameterName, String defaultMessage, String messageKey,
            Collection<Object> content) {
        return new ConfigValidationMessage(parameterName, defaultMessage, messageKey, content);
