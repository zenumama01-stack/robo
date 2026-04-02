import org.openhab.core.config.core.validation.ConfigValidationException;
 * The {@link ConfigDescriptionValidatorImpl} validates a given set of {@link Configuration} parameters against a
 * given {@link ConfigDescription} URI. So it can be used as a static pre-validation to avoid that the configuration of
 * an entity is updated with parameters which do not match with the declarations in the configuration description.
 * If the validator detects one or more mismatches then a {@link ConfigValidationException} is thrown.
 * @author Chris Jackson - Handle checks on multiple selection parameters
public final class ConfigDescriptionValidatorImpl implements ConfigDescriptionValidator {
    private static final List<ConfigDescriptionParameterValidator> VALIDATORS = List.of( //
            ConfigDescriptionParameterValidatorFactory.createRequiredValidator(), //
            ConfigDescriptionParameterValidatorFactory.createTypeValidator(), //
            ConfigDescriptionParameterValidatorFactory.createMinMaxValidator(), //
            ConfigDescriptionParameterValidatorFactory.createPatternValidator(), //
            ConfigDescriptionParameterValidatorFactory.createOptionsValidator() //
    private final Logger logger = LoggerFactory.getLogger(ConfigDescriptionValidatorImpl.class);
    private final ConfigDescriptionRegistry configDescriptionRegistry;
    private final TranslationProvider translationProvider;
    public ConfigDescriptionValidatorImpl(final BundleContext bundleContext,
            final @Reference ConfigDescriptionRegistry configDescriptionRegistry,
            final @Reference TranslationProvider translationProvider) {
        this.configDescriptionRegistry = configDescriptionRegistry;
        this.translationProvider = translationProvider;
     * Validates the given configuration parameters against the given configuration description having the given URI.
     * @param configurationParameters the configuration parameters to be validated
     * @param configDescriptionURI the URI of the configuration description against which the configuration parameters
     *            are to be validated
     * @throws ConfigValidationException if one or more configuration parameters do not match with the configuration
     *             description having the given URI
     * @throws NullPointerException if given config description URI or configuration parameters are null
    @SuppressWarnings({ "unchecked", "null" })
    public void validate(Map<String, Object> configurationParameters, URI configDescriptionURI) {
        Objects.requireNonNull(configurationParameters, "Configuration parameters must not be null");
        Objects.requireNonNull(configDescriptionURI, "Config description URI must not be null");
        ConfigDescription configDescription = getConfigDescription(configDescriptionURI);
        if (configDescription == null) {
            logger.warn("Skipping config description validation because no config description found for URI '{}'",
                    configDescriptionURI);
        Map<String, ConfigDescriptionParameter> map = configDescription.toParametersMap();
        Collection<ConfigValidationMessage> configDescriptionValidationMessages = new ArrayList<>();
        for (Entry<String, ConfigDescriptionParameter> entry : map.entrySet()) {
            ConfigDescriptionParameter configDescriptionParameter = entry.getValue();
            // If the parameter supports multiple selection, then it may be provided as an array
            if (configDescriptionParameter.isMultiple() && configurationParameters.get(key) instanceof List) {
                List<Object> values = (List<Object>) configurationParameters.get(key);
                // check if multipleLimit is obeyed
                Integer multipleLimit = configDescriptionParameter.getMultipleLimit();
                if (multipleLimit != null && values.size() > multipleLimit) {
                    MessageKey messageKey = MessageKey.MULTIPLE_LIMIT_VIOLATED;
                    ConfigValidationMessage message = new ConfigValidationMessage(configDescriptionParameter.getName(),
                            messageKey.defaultMessage, messageKey.key, multipleLimit, values.size());
                    configDescriptionValidationMessages.add(message);
                // Perform validation on each value in the list separately
                for (Object value : values) {
                    ConfigValidationMessage message = validateParameter(configDescriptionParameter, value);
                    if (message != null) {
                ConfigValidationMessage message = validateParameter(configDescriptionParameter,
                        configurationParameters.get(key));
        if (!configDescriptionValidationMessages.isEmpty()) {
            throw new ConfigValidationException(bundleContext.getBundle(), translationProvider,
                    configDescriptionValidationMessages);
     * Validates the given value against the given config description parameter.
     * @param configDescriptionParameter the corresponding config description parameter
     * @param value the actual value
     * @return the {@link ConfigValidationMessage} if the given value is not valid for the config description parameter,
    private @Nullable ConfigValidationMessage validateParameter(ConfigDescriptionParameter configDescriptionParameter,
            @Nullable Object value) {
        for (ConfigDescriptionParameterValidator validator : VALIDATORS) {
            ConfigValidationMessage message = validator.validate(configDescriptionParameter, value);
     * Retrieves the {@link ConfigDescription} for the given URI.
     * @param configDescriptionURI the URI of the configuration description to be retrieved
     * @return the requested config description or null if config description could not be found (either because of
     *         config description registry is not available or because of config description could not be found for
     *         given URI)
    private @Nullable ConfigDescription getConfigDescription(URI configDescriptionURI) {
        ConfigDescription configDescription = configDescriptionRegistry.getConfigDescription(configDescriptionURI);
            logger.warn("No config description found for URI '{}'", configDescriptionURI);
