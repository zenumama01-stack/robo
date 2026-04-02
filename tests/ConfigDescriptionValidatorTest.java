import static org.hamcrest.Matchers.containsInAnyOrder;
import org.mockito.stubbing.Answer;
 * Testing the {@link ConfigDescriptionValidator}.
public class ConfigDescriptionValidatorTest {
    private static final int MIN_VIOLATED = 1;
    private static final int MAX_VIOLATED = 1234;
    private static final BigDecimal DECIMAL_MIN_VIOLATED = BigDecimal.ONE;
    private static final BigDecimal DECIMAL_MAX_VIOLATED = new BigDecimal("3.5");
    private static final BigDecimal MIN = BigDecimal.valueOf(2);
    private static final BigDecimal MAX = BigDecimal.valueOf(3);
    private static final BigDecimal DECIMAL_MIN = new BigDecimal("1.3");
    private static final BigDecimal DECIMAL_MAX = new BigDecimal("3.3");
    private static final String PATTERN = "ab*c";
    private static final String UNKNOWN = "unknown";
    private static final Long INVALID = 0l;
    private static final String BOOL_PARAM_NAME = "bool-param";
    private static final String BOOL_REQUIRED_PARAM_NAME = "bool-required-param";
    private static final String TXT_PARAM_NAME = "txt-param";
    private static final String TXT_REQUIRED_PARAM_NAME = "txt-required-param";
    private static final String TXT_MIN_PARAM_NAME = "txt-min-name";
    private static final String TXT_MAX_PARAM_NAME = "txt-max-name";
    private static final String TXT_PATTERN_PARAM_NAME = "txt-pattern-name";
    private static final String TXT_MAX_PATTERN_PARAM_NAME = "txt-max-pattern-name";
    private static final String TXT_PARAM_WITH_LIMITED_OPTIONS_NAME = "txt-param-with-limited-options-name";
    private static final String TXT_PARAM_WITH_UNLIMITED_OPTIONS_NAME = "txt-param-with-unlimited-options-name";
    private static final String TXT_MULTIPLE_LIMIT_PARAM_NAME = "txt-multiple-limit-name";
    private static final String INT_PARAM_NAME = "int-param";
    private static final String INT_REQUIRED_PARAM_NAME = "int-required-param";
    private static final String INT_MIN_PARAM_NAME = "int-min-name";
    private static final String INT_MAX_PARAM_NAME = "int-max-name";
    private static final String INT_OPTION_PARAM_NAME = "int-option-param-name";
    private static final String DECIMAL_PARAM_NAME = "decimal-param";
    private static final String DECIMAL_REQUIRED_PARAM_NAME = "decimal-required-param";
    private static final String DECIMAL_WITH_LIMITED_OPTIONS_NAME = "decimal-with-limited-options-param";
    private static final String DECIMAL_MIN_PARAM_NAME = "decimal-min-name";
    private static final String DECIMAL_MAX_PARAM_NAME = "decimal-max-name";
    private static final List<ParameterOption> TEXT_PARAMETER_OPTIONS = List.of( //
            new ParameterOption("http", "HTTP"), //
            new ParameterOption("https", "HTTPS") //
    private static final List<ParameterOption> DECIMAL_PARAMETER_OPTIONS = List.of( //
            new ParameterOption("2", "Double"), //
            new ParameterOption("0.5", "Half") //
    private static final ConfigDescriptionParameter BOOL_PARAM = ConfigDescriptionParameterBuilder
            .create(BOOL_PARAM_NAME, ConfigDescriptionParameter.Type.BOOLEAN).build();
    private static final ConfigDescriptionParameter BOOL_REQUIRED_PARAM = ConfigDescriptionParameterBuilder
            .create(BOOL_REQUIRED_PARAM_NAME, ConfigDescriptionParameter.Type.BOOLEAN).withRequired(true).build();
    private static final ConfigDescriptionParameter TXT_PARAM = ConfigDescriptionParameterBuilder
            .create(TXT_PARAM_NAME, ConfigDescriptionParameter.Type.TEXT).build();
    private static final ConfigDescriptionParameter TXT_REQUIRED_PARAM = ConfigDescriptionParameterBuilder
            .create(TXT_REQUIRED_PARAM_NAME, ConfigDescriptionParameter.Type.TEXT).withRequired(true).build();
    private static final ConfigDescriptionParameter TXT_MIN_PARAM = ConfigDescriptionParameterBuilder
            .create(TXT_MIN_PARAM_NAME, ConfigDescriptionParameter.Type.TEXT).withMinimum(MIN).build();
    private static final ConfigDescriptionParameter TXT_MAX_PARAM = ConfigDescriptionParameterBuilder
            .create(TXT_MAX_PARAM_NAME, ConfigDescriptionParameter.Type.TEXT).withMaximum(MAX).build();
    private static final ConfigDescriptionParameter TXT_PATTERN_PARAM = ConfigDescriptionParameterBuilder
            .create(TXT_PATTERN_PARAM_NAME, ConfigDescriptionParameter.Type.TEXT).withPattern(PATTERN).build();
    private static final ConfigDescriptionParameter TXT_MAX_PATTERN_PARAM = ConfigDescriptionParameterBuilder
            .create(TXT_MAX_PATTERN_PARAM_NAME, ConfigDescriptionParameter.Type.TEXT).withMaximum(MAX)
            .withPattern(PATTERN).build();
    private static final ConfigDescriptionParameter TXT_PARAM_WITH_LIMITED_OPTIONS = ConfigDescriptionParameterBuilder
            .create(TXT_PARAM_WITH_LIMITED_OPTIONS_NAME, ConfigDescriptionParameter.Type.TEXT)
            .withOptions(TEXT_PARAMETER_OPTIONS).build();
    private static final ConfigDescriptionParameter TXT_PARAM_WITH_UNLIMITED_OPTIONS = ConfigDescriptionParameterBuilder
            .create(TXT_PARAM_WITH_UNLIMITED_OPTIONS_NAME, ConfigDescriptionParameter.Type.TEXT)
            .withOptions(TEXT_PARAMETER_OPTIONS).withLimitToOptions(false).build();
    private static final ConfigDescriptionParameter TXT_MULTIPLE_LIMIT_PARAM = ConfigDescriptionParameterBuilder
            .create(TXT_MULTIPLE_LIMIT_PARAM_NAME, Type.TEXT).withMultiple(true).withMultipleLimit(2).build();
    private static final ConfigDescriptionParameter INT_PARAM = ConfigDescriptionParameterBuilder
            .create(INT_PARAM_NAME, ConfigDescriptionParameter.Type.INTEGER).build();
    private static final ConfigDescriptionParameter INT_REQUIRED_PARAM = ConfigDescriptionParameterBuilder
            .create(INT_REQUIRED_PARAM_NAME, ConfigDescriptionParameter.Type.INTEGER).withRequired(true).build();
    private static final ConfigDescriptionParameter INT_MIN_PARAM = ConfigDescriptionParameterBuilder
            .create(INT_MIN_PARAM_NAME, ConfigDescriptionParameter.Type.INTEGER).withMinimum(MIN).build();
    private static final ConfigDescriptionParameter INT_MAX_PARAM = ConfigDescriptionParameterBuilder
            .create(INT_MAX_PARAM_NAME, ConfigDescriptionParameter.Type.INTEGER).withMaximum(MAX).build();
    private static final ConfigDescriptionParameter INT_OPTION_PARAM = ConfigDescriptionParameterBuilder
            .create(INT_OPTION_PARAM_NAME, Type.INTEGER).withMinimum(MIN).withMaximum(MAX)
            .withOptions(List.of(new ParameterOption("10", "10"))).build();
    private static final ConfigDescriptionParameter DECIMAL_PARAM = ConfigDescriptionParameterBuilder
            .create(DECIMAL_PARAM_NAME, ConfigDescriptionParameter.Type.DECIMAL).build();
    private static final ConfigDescriptionParameter DECIMAL_REQUIRED_PARAM = ConfigDescriptionParameterBuilder
            .create(DECIMAL_REQUIRED_PARAM_NAME, ConfigDescriptionParameter.Type.DECIMAL).withRequired(true).build();
    private static final ConfigDescriptionParameter DECIMAL_WITH_LIMITED_OPTIONS_PARAM_OPTIONS = ConfigDescriptionParameterBuilder
            .create(DECIMAL_WITH_LIMITED_OPTIONS_NAME, Type.DECIMAL).withOptions(DECIMAL_PARAMETER_OPTIONS).build();
    private static final ConfigDescriptionParameter DECIMAL_MIN_PARAM = ConfigDescriptionParameterBuilder
            .create(DECIMAL_MIN_PARAM_NAME, ConfigDescriptionParameter.Type.DECIMAL).withMinimum(DECIMAL_MIN).build();
    private static final ConfigDescriptionParameter DECIMAL_MAX_PARAM = ConfigDescriptionParameterBuilder
            .create(DECIMAL_MAX_PARAM_NAME, ConfigDescriptionParameter.Type.DECIMAL).withMaximum(DECIMAL_MAX).build();
    private static final URI CONFIG_DESCRIPTION_URI = URI.create("config:dummy");
    private static final ConfigDescription CONFIG_DESCRIPTION = ConfigDescriptionBuilder.create(CONFIG_DESCRIPTION_URI)
            .withParameters(List.of(BOOL_PARAM, BOOL_REQUIRED_PARAM, TXT_PARAM, TXT_REQUIRED_PARAM, TXT_MIN_PARAM,
                    TXT_MAX_PARAM, TXT_PATTERN_PARAM, TXT_MAX_PATTERN_PARAM, TXT_PARAM_WITH_LIMITED_OPTIONS,
                    TXT_PARAM_WITH_UNLIMITED_OPTIONS, TXT_MULTIPLE_LIMIT_PARAM, INT_PARAM, INT_REQUIRED_PARAM,
                    INT_MIN_PARAM, INT_MAX_PARAM, DECIMAL_PARAM, DECIMAL_REQUIRED_PARAM, INT_OPTION_PARAM,
                    DECIMAL_WITH_LIMITED_OPTIONS_PARAM_OPTIONS, DECIMAL_MIN_PARAM, DECIMAL_MAX_PARAM))
    private @NonNullByDefault({}) Map<String, Object> params;
    private @NonNullByDefault({}) ConfigDescriptionValidatorImpl configDescriptionValidator;
        ConfigDescriptionRegistry configDescriptionRegistry = mock(ConfigDescriptionRegistry.class);
        when(configDescriptionRegistry.getConfigDescription(any()))
                .thenAnswer((Answer<ConfigDescription>) invocation -> {
                    URI uri = invocation.getArgument(0);
                    return !CONFIG_DESCRIPTION_URI.equals(uri) ? null : CONFIG_DESCRIPTION;
        BundleContext bundleContext = mock(BundleContext.class);
        when(bundleContext.getBundle()).thenReturn(mock(Bundle.class));
        configDescriptionValidator = new ConfigDescriptionValidatorImpl(bundleContext, configDescriptionRegistry,
                mock(TranslationProvider.class));
        params = new LinkedHashMap<>();
        params.put(BOOL_PARAM_NAME, null);
        params.put(BOOL_REQUIRED_PARAM_NAME, Boolean.FALSE);
        params.put(TXT_PARAM_NAME, null);
        params.put(TXT_REQUIRED_PARAM_NAME, "");
        params.put(TXT_MIN_PARAM_NAME, String.valueOf(MAX_VIOLATED));
        params.put(TXT_MAX_PARAM_NAME, String.valueOf(MIN_VIOLATED));
        params.put(TXT_PATTERN_PARAM_NAME, "abbbc");
        params.put(TXT_MAX_PATTERN_PARAM_NAME, "abc");
        params.put(TXT_PARAM_WITH_LIMITED_OPTIONS_NAME, "http");
        params.put(TXT_MULTIPLE_LIMIT_PARAM_NAME, List.of("1", "2"));
        params.put(INT_PARAM_NAME, null);
        params.put(INT_REQUIRED_PARAM_NAME, 0);
        params.put(INT_MIN_PARAM_NAME, MIN);
        params.put(INT_MAX_PARAM_NAME, MAX);
        params.put(INT_OPTION_PARAM_NAME, new BigDecimal("10.0"));
        params.put(DECIMAL_PARAM_NAME, null);
        params.put(DECIMAL_REQUIRED_PARAM_NAME, 0f);
        params.put(DECIMAL_WITH_LIMITED_OPTIONS_NAME, new BigDecimal("2.0"));
        params.put(DECIMAL_MIN_PARAM_NAME, DECIMAL_MIN);
        params.put(DECIMAL_MAX_PARAM_NAME, DECIMAL_MAX);
    public void assertValidationThrowsNoExceptionForValidConfigParameters() {
        configDescriptionValidator.validate(params, CONFIG_DESCRIPTION_URI);
    public void assertValidationThrowsNoExceptionForValidStringConfigParameters() {
        params.put(BOOL_PARAM_NAME, "true");
        params.put(INT_PARAM_NAME, "1");
        params.put(DECIMAL_PARAM_NAME, "1.0");
    // ===========================================================================
    // REQUIRED VALIDATIONS
    public void assertValidationThrowsExceptionForMissingRequiredBooleanConfigParameter() {
        assertMissingRequired(BOOL_REQUIRED_PARAM_NAME);
        assertNonNullRequired(BOOL_REQUIRED_PARAM_NAME);
    public void assertValidationThrowsExceptionForMissingRequiredTxtConfigParameter() {
        assertMissingRequired(TXT_REQUIRED_PARAM_NAME);
        assertNonNullRequired(TXT_REQUIRED_PARAM_NAME);
    public void assertValidationThrowsExceptionForMissingRequiredIntConfigParameter() {
        assertMissingRequired(INT_REQUIRED_PARAM_NAME);
        assertNonNullRequired(INT_REQUIRED_PARAM_NAME);
    public void assertValidationThrowsExceptionForMissingRequiredDecimalConfigParameter() {
        assertMissingRequired(DECIMAL_REQUIRED_PARAM_NAME);
        assertNonNullRequired(DECIMAL_REQUIRED_PARAM_NAME);
    public void assertValidationThrowsExceptionContainingMessagesForAllRequiredConfigParameters() {
        List<ConfigValidationMessage> expected = List.of(
                new ConfigValidationMessage(BOOL_REQUIRED_PARAM_NAME, MessageKey.PARAMETER_REQUIRED.defaultMessage,
                        MessageKey.PARAMETER_REQUIRED.key),
                new ConfigValidationMessage(DECIMAL_REQUIRED_PARAM_NAME, MessageKey.PARAMETER_REQUIRED.defaultMessage,
                new ConfigValidationMessage(TXT_REQUIRED_PARAM_NAME, MessageKey.PARAMETER_REQUIRED.defaultMessage,
                new ConfigValidationMessage(INT_REQUIRED_PARAM_NAME, MessageKey.PARAMETER_REQUIRED.defaultMessage,
                        MessageKey.PARAMETER_REQUIRED.key));
        params.put(BOOL_REQUIRED_PARAM_NAME, null);
        params.put(TXT_REQUIRED_PARAM_NAME, null);
        params.put(INT_REQUIRED_PARAM_NAME, null);
        params.put(DECIMAL_REQUIRED_PARAM_NAME, null);
        ConfigValidationException exception = Assertions.assertThrows(ConfigValidationException.class,
                () -> configDescriptionValidator.validate(params, CONFIG_DESCRIPTION_URI));
        assertThat(getConfigValidationMessages(exception), containsInAnyOrder(expected.toArray()));
    void assertMissingRequired(String parameterName) {
        List<ConfigValidationMessage> expected = List.of(new ConfigValidationMessage(parameterName,
                MessageKey.PARAMETER_REQUIRED.defaultMessage, MessageKey.PARAMETER_REQUIRED.key));
        params.remove(parameterName);
        assertThat(getConfigValidationMessages(exception), is(expected));
    void assertNonNullRequired(String parameterName) {
        params.put(parameterName, null);
    // MIN MAX VALIDATIONS
    public void assertValidationThrowsExceptionForInvalidMinAttributeOfTxtConfigParameter() {
        assertMinMax(TXT_MIN_PARAM_NAME, String.valueOf(MIN_VIOLATED), MessageKey.MIN_VALUE_TXT_VIOLATED,
                MIN.toString());
    public void assertValidationThrowsExceptionForInvalidMaxAttributeOfTxtConfigParameter() {
        assertMinMax(TXT_MAX_PARAM_NAME, String.valueOf(MAX_VIOLATED), MessageKey.MAX_VALUE_TXT_VIOLATED,
                MAX.toString());
    public void assertValidationThrowsExceptionForInvalidMinAttributeOfIntConfigParameter() {
        assertMinMax(INT_MIN_PARAM_NAME, MIN_VIOLATED, MessageKey.MIN_VALUE_NUMERIC_VIOLATED, MIN.toString());
    public void assertValidationThrowsExceptionForInvalidMaxAttributeOfIntConfigParameter() {
        assertMinMax(INT_MAX_PARAM_NAME, MAX_VIOLATED, MessageKey.MAX_VALUE_NUMERIC_VIOLATED, MAX.toString());
    public void assertValidationThrowsExceptionForInvalidMinAttributeOfDecimalConfigParameter() {
        assertMinMax(DECIMAL_MIN_PARAM_NAME, DECIMAL_MIN_VIOLATED, MessageKey.MIN_VALUE_NUMERIC_VIOLATED,
                DECIMAL_MIN.toString());
    public void assertValidationThrowsExceptionForInvalidMaxAttributeOfDecimalConfigParameter() {
        assertMinMax(DECIMAL_MAX_PARAM_NAME, DECIMAL_MAX_VIOLATED, MessageKey.MAX_VALUE_NUMERIC_VIOLATED,
                DECIMAL_MAX.toString());
    public void assertValidationThrowsExceptionContainingMessagesForAllMinMaxConfigParameters() {
                new ConfigValidationMessage(TXT_MAX_PARAM_NAME, MessageKey.MAX_VALUE_TXT_VIOLATED.defaultMessage,
                        MessageKey.MAX_VALUE_TXT_VIOLATED.key, MAX.toString()),
                new ConfigValidationMessage(INT_MIN_PARAM_NAME, MessageKey.MIN_VALUE_NUMERIC_VIOLATED.defaultMessage,
                        MessageKey.MIN_VALUE_NUMERIC_VIOLATED.key, MIN.toString()),
                new ConfigValidationMessage(DECIMAL_MIN_PARAM_NAME,
                        MessageKey.MIN_VALUE_NUMERIC_VIOLATED.defaultMessage, MessageKey.MIN_VALUE_NUMERIC_VIOLATED.key,
                        DECIMAL_MIN.toString()),
                new ConfigValidationMessage(TXT_MIN_PARAM_NAME, MessageKey.MIN_VALUE_TXT_VIOLATED.defaultMessage,
                        MessageKey.MIN_VALUE_TXT_VIOLATED.key, MIN.toString()),
                new ConfigValidationMessage(INT_MAX_PARAM_NAME, MessageKey.MAX_VALUE_NUMERIC_VIOLATED.defaultMessage,
                        MessageKey.MAX_VALUE_NUMERIC_VIOLATED.key, MAX.toString()),
                new ConfigValidationMessage(DECIMAL_MAX_PARAM_NAME,
                        MessageKey.MAX_VALUE_NUMERIC_VIOLATED.defaultMessage, MessageKey.MAX_VALUE_NUMERIC_VIOLATED.key,
                        DECIMAL_MAX.toString()));
        params.put(TXT_MIN_PARAM_NAME, String.valueOf(MIN_VIOLATED));
        params.put(TXT_MAX_PARAM_NAME, String.valueOf(MAX_VIOLATED));
        params.put(INT_MIN_PARAM_NAME, MIN_VIOLATED);
        params.put(INT_MAX_PARAM_NAME, MAX_VIOLATED);
        params.put(DECIMAL_MIN_PARAM_NAME, DECIMAL_MIN_VIOLATED);
        params.put(DECIMAL_MAX_PARAM_NAME, DECIMAL_MAX_VIOLATED);
    void assertMinMax(String parameterName, Object value, MessageKey msgKey, String minMax) {
        List<ConfigValidationMessage> expected = List
                .of(new ConfigValidationMessage(parameterName, msgKey.defaultMessage, msgKey.key, minMax));
        params.put(parameterName, value);
    // TYPE VALIDATIONS
    public void assertValidationThrowsExceptionForInvalidTypeForBooleanConfigParameter() {
        assertType(BOOL_PARAM_NAME, Type.BOOLEAN);
    public void assertValidationThrowsExceptionForInvalidTypeForTxtConfigParameter() {
        assertType(TXT_PARAM_NAME, Type.TEXT);
    public void assertValidationThrowsExceptionForInvalidTypeForIntConfigParameter() {
        assertType(INT_PARAM_NAME, Type.INTEGER);
    public void assertValidationThrowsExceptionForInvalidTypeForDecimalConfigParameter() {
        assertType(DECIMAL_PARAM_NAME, Type.DECIMAL);
    public void assertValidationThrowsExceptionContainingMessagesForMultipleInvalidTypedConfigParameters() {
                new ConfigValidationMessage(BOOL_PARAM_NAME, MessageKey.DATA_TYPE_VIOLATED.defaultMessage,
                        MessageKey.DATA_TYPE_VIOLATED.key, Long.class, Type.BOOLEAN),
                new ConfigValidationMessage(INT_PARAM_NAME, MessageKey.DATA_TYPE_VIOLATED.defaultMessage,
                        MessageKey.DATA_TYPE_VIOLATED.key, Long.class, Type.INTEGER),
                new ConfigValidationMessage(TXT_PARAM_NAME, MessageKey.DATA_TYPE_VIOLATED.defaultMessage,
                        MessageKey.DATA_TYPE_VIOLATED.key, Long.class, Type.TEXT),
                new ConfigValidationMessage(DECIMAL_PARAM_NAME, MessageKey.DATA_TYPE_VIOLATED.defaultMessage,
                        MessageKey.DATA_TYPE_VIOLATED.key, Long.class, Type.DECIMAL));
        params.put(BOOL_PARAM_NAME, INVALID);
        params.put(TXT_PARAM_NAME, INVALID);
        params.put(INT_PARAM_NAME, INVALID);
        params.put(DECIMAL_PARAM_NAME, INVALID);
    void assertType(String parameterName, Type type) {
                MessageKey.DATA_TYPE_VIOLATED.defaultMessage, MessageKey.DATA_TYPE_VIOLATED.key, Long.class, type));
        params.put(parameterName, INVALID);
    // PATTERN VALIDATIONS
    public void assertValidationThrowsExceptionContainingMessagesForInvalidPatternForTxtConfigParameters() {
                .of(new ConfigValidationMessage(TXT_PATTERN_PARAM_NAME, MessageKey.PATTERN_VIOLATED.defaultMessage,
                        MessageKey.PATTERN_VIOLATED.key, String.valueOf(MAX_VIOLATED), PATTERN));
        params.put(TXT_PATTERN_PARAM_NAME, String.valueOf(MAX_VIOLATED));
    // PARAMETER OPTION VALIDATIONS
    public void assertValidationThrowsExceptionForNotAllowedLimitedParameterOption() {
        String parameterValue = "ftp";
        List<ConfigValidationMessage> expected = List.of(new ConfigValidationMessage(
                TXT_PARAM_WITH_LIMITED_OPTIONS_NAME, MessageKey.OPTIONS_VIOLATED.defaultMessage,
                MessageKey.OPTIONS_VIOLATED.key, parameterValue, TEXT_PARAMETER_OPTIONS));
        params.put(TXT_PARAM_WITH_LIMITED_OPTIONS_NAME, parameterValue);
    public void assertValidationThrowsNoExceptionForAllowedUnlimitedParameterOption() {
        params.put(TXT_PARAM_WITH_UNLIMITED_OPTIONS_NAME, "ftp");
        Assertions.assertDoesNotThrow(() -> configDescriptionValidator.validate(params, CONFIG_DESCRIPTION_URI));
    public void assertValidationThrowsNoExceptionForAllowedDecimalParameterOption() {
        params.put(DECIMAL_WITH_LIMITED_OPTIONS_NAME, new BigDecimal("0.5"));
    public void assertValidationThrowsExceptionForNotAllowedDecimalParameterOption() {
        BigDecimal parameterValue = new BigDecimal("0.1");
        params.put(DECIMAL_WITH_LIMITED_OPTIONS_NAME, parameterValue);
        List<ConfigValidationMessage> expected = List.of(new ConfigValidationMessage(DECIMAL_WITH_LIMITED_OPTIONS_NAME,
                MessageKey.OPTIONS_VIOLATED.defaultMessage, MessageKey.OPTIONS_VIOLATED.key,
                String.valueOf(parameterValue), DECIMAL_PARAMETER_OPTIONS));
    // MISC VALIDATIONS
    public void assertValidationThrowsExceptionForMultipleLimitViolated() {
        List<ConfigValidationMessage> expected = List.of(new ConfigValidationMessage(TXT_MULTIPLE_LIMIT_PARAM_NAME,
                MessageKey.MULTIPLE_LIMIT_VIOLATED.defaultMessage, MessageKey.MULTIPLE_LIMIT_VIOLATED.key, 2, 3));
        params.put(TXT_MULTIPLE_LIMIT_PARAM_NAME, List.of("1", "2", "3"));
    public void assertValidationThrowsExceptionContainingMultipleVariousViolations() {
                new ConfigValidationMessage(TXT_PATTERN_PARAM_NAME, MessageKey.PATTERN_VIOLATED.defaultMessage,
                        MessageKey.PATTERN_VIOLATED.key, String.valueOf(MAX_VIOLATED), PATTERN),
                        MessageKey.DATA_TYPE_VIOLATED.key, Long.class, Type.DECIMAL),
    public void assertValidationProvidesOnlyOneMessagePerParameterAlthoughMultipleViolationsOccur() {
        List<ConfigValidationMessage> expected = List.of(new ConfigValidationMessage(TXT_MAX_PATTERN_PARAM_NAME,
                MessageKey.MAX_VALUE_TXT_VIOLATED.defaultMessage, MessageKey.MAX_VALUE_TXT_VIOLATED.key,
                MAX.toString()));
        params.put(TXT_MAX_PATTERN_PARAM_NAME, String.valueOf(MAX_VIOLATED));
    public void assertValidationDoesNotCareAboutParameterThatIsNotSpecifiedInConfigDescription() {
        params.put(UNKNOWN, null);
        params.put(UNKNOWN, MIN_VIOLATED);
        params.put(UNKNOWN, MAX_VIOLATED);
        params.put(UNKNOWN, INVALID);
    public void assertValidateCanHandleUnknownURIs() throws Exception {
        configDescriptionValidator.validate(params, new URI(UNKNOWN));
    private static @Nullable List<ConfigValidationMessage> getConfigValidationMessages(ConfigValidationException cve) {
            Field field = cve.getClass().getDeclaredField("configValidationMessages");
            return (List<ConfigValidationMessage>) field.get(cve);
        } catch (NoSuchFieldException | SecurityException | IllegalArgumentException | IllegalAccessException e) {
            throw new IllegalStateException("Failed to get configValidationMessages: " + e.getMessage(), e);
