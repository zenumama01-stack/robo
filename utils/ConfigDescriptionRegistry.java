 * {@link ConfigDescriptionRegistry} provides access to {@link ConfigDescription}s.
 * It tracks {@link ConfigDescriptionProvider} OSGi services to collect all {@link ConfigDescription}s.
 * @see ConfigDescriptionProvider
 * @author Chris Jackson - Added compatibility with multiple ConfigDescriptionProviders. Added Config OptionProvider.
@Component(immediate = true, service = { ConfigDescriptionRegistry.class })
public class ConfigDescriptionRegistry {
    private final Logger logger = LoggerFactory.getLogger(ConfigDescriptionRegistry.class);
    private final List<ConfigOptionProvider> configOptionProviders = new CopyOnWriteArrayList<>();
    private final List<ConfigDescriptionProvider> configDescriptionProviders = new CopyOnWriteArrayList<>();
    private final List<ConfigDescriptionAliasProvider> configDescriptionAliasProviders = new CopyOnWriteArrayList<>();
    protected void addConfigOptionProvider(ConfigOptionProvider configOptionProvider) {
        configOptionProviders.add(configOptionProvider);
    protected void removeConfigOptionProvider(ConfigOptionProvider configOptionProvider) {
        configOptionProviders.remove(configOptionProvider);
    protected void addConfigDescriptionProvider(ConfigDescriptionProvider configDescriptionProvider) {
        configDescriptionProviders.add(configDescriptionProvider);
    protected void removeConfigDescriptionProvider(ConfigDescriptionProvider configDescriptionProvider) {
        configDescriptionProviders.remove(configDescriptionProvider);
    protected void addConfigDescriptionAliasProvider(ConfigDescriptionAliasProvider configDescriptionAliasProvider) {
        configDescriptionAliasProviders.add(configDescriptionAliasProvider);
    protected void removeConfigDescriptionAliasProvider(ConfigDescriptionAliasProvider configDescriptionAliasProvider) {
        configDescriptionAliasProviders.remove(configDescriptionAliasProvider);
     * Returns all config descriptions.
     * If more than one {@link ConfigDescriptionProvider} is registered for a specific URI, then the returned
     * {@link ConfigDescription} collection will contain the data from all providers.
     * No checking is performed to ensure that multiple providers don't provide the same configuration data. It is up to
     * the binding to ensure that multiple sources (eg static XML and dynamic binding data) do not contain overlapping
     * @return all config descriptions or an empty collection if no config
     *         description exists
        Map<URI, ConfigDescription> configMap = new HashMap<>();
        // Loop over all providers
        for (ConfigDescriptionProvider configDescriptionProvider : configDescriptionProviders) {
            // And for each provider, loop over all their config descriptions
            for (ConfigDescription configDescription : configDescriptionProvider.getConfigDescriptions(locale)) {
                // See if there already exists a configuration for this URI in the map
                ConfigDescription configFromMap = configMap.get(configDescription.getUID());
                if (configFromMap != null) {
                    // Yes - Merge the groups and parameters
                    List<ConfigDescriptionParameter> parameters = new ArrayList<>();
                    parameters.addAll(configFromMap.getParameters());
                    parameters.addAll(configDescription.getParameters());
                    List<ConfigDescriptionParameterGroup> parameterGroups = new ArrayList<>();
                    parameterGroups.addAll(configFromMap.getParameterGroups());
                    parameterGroups.addAll(configDescription.getParameterGroups());
                    // And add the combined configuration to the map
                    configMap.put(configDescription.getUID(),
                            ConfigDescriptionBuilder.create(configDescription.getUID()).withParameters(parameters)
                                    .withParameterGroups(parameterGroups).build());
                    // No - Just add the new configuration to the map
                    configMap.put(configDescription.getUID(), configDescription);
        // Now convert the map into the collection
        return Collections.unmodifiableCollection(new ArrayList<>(configMap.values()));
    public Collection<ConfigDescription> getConfigDescriptions() {
        return getConfigDescriptions(null);
     * Returns a config description for a given URI.
     * If more than one {@link ConfigDescriptionProvider} is registered for the requested URI, then the returned
     * {@link ConfigDescription} will contain the data from all providers.
     * @param uri the URI to which the config description to be returned (must
     *            not be null)
     * @return config description or null if no config description exists for
     *         the given name
        Set<URI> aliases = getAliases(uri);
        for (URI alias : aliases) {
            logger.debug("No config description found for '{}', using alias '{}' instead", uri, alias);
            found |= fillFromProviders(alias, locale, parameters, parameterGroups);
        found |= fillFromProviders(uri, locale, parameters, parameterGroups);
            List<ConfigDescriptionParameter> parametersWithOptions = new ArrayList<>(parameters.size());
                parametersWithOptions.add(getConfigOptions(uri, aliases, parameter, locale));
            // Return the new configuration description
            return ConfigDescriptionBuilder.create(uri).withParameters(parametersWithOptions)
                    .withParameterGroups(parameterGroups).build();
            // Otherwise null
    private Set<URI> getAliases(URI original) {
        Set<URI> ret = new LinkedHashSet<>();
        for (ConfigDescriptionAliasProvider aliasProvider : configDescriptionAliasProviders) {
            URI alias = aliasProvider.getAlias(original);
            if (alias != null) {
                ret.add(alias);
    private boolean fillFromProviders(URI uri, @Nullable Locale locale, List<ConfigDescriptionParameter> parameters,
            List<ConfigDescriptionParameterGroup> parameterGroups) {
            ConfigDescription config = configDescriptionProvider.getConfigDescription(uri, locale);
                // Simply merge the groups and parameters
                parameters.addAll(config.getParameters());
                parameterGroups.addAll(config.getParameterGroups());
    public @Nullable ConfigDescription getConfigDescription(URI uri) {
        return getConfigDescription(uri, null);
     * Updates the config parameter options for a given URI and parameter
     * If more than one {@link ConfigOptionProvider} is registered for the requested URI, then the returned
     * {@link ConfigDescriptionParameter} will contain the data from all providers.
     * No checking is performed to ensure that multiple providers don't provide the same options. It is up to
     * @param uri the URI to which the options to be returned
     * @param parameter the parameter requiring options to be updated
     * @return config description
    private ConfigDescriptionParameter getConfigOptions(URI uri, Set<URI> aliases, ConfigDescriptionParameter parameter,
        // Add all the existing options that may be provided by the initial config description provider
        List<ParameterOption> options = new ArrayList<>(parameter.getOptions());
        boolean found = fillFromProviders(uri, parameter, locale, options);
                found = fillFromProviders(alias, parameter, locale, options);
            // Return the new parameter
            return ConfigDescriptionParameterBuilder.create(parameter.getName(), parameter.getType()) //
                    .withMinimum(parameter.getMinimum()) //
                    .withMaximum(parameter.getMaximum()) //
                    .withStepSize(parameter.getStepSize()) //
                    .withPattern(parameter.getPattern()) //
                    .withRequired(parameter.isRequired()) //
                    .withReadOnly(parameter.isReadOnly()) //
                    .withMultiple(parameter.isMultiple()) //
                    .withContext(parameter.getContext()) //
                    .withDefault(parameter.getDefault()) //
                    .withLabel(parameter.getLabel()) //
                    .withDescription(parameter.getDescription()) //
                    .withOptions(options) //
                    .withFilterCriteria(parameter.getFilterCriteria()) //
                    .withGroupName(parameter.getGroupName()) //
                    .withAdvanced(parameter.isAdvanced()) //
                    .withLimitToOptions(parameter.getLimitToOptions()) //
                    .withMultipleLimit(parameter.getMultipleLimit()) //
                    .withUnit(parameter.getUnit()) //
                    .withUnitLabel(parameter.getUnitLabel()) //
                    .withVerify(parameter.isVerifyable()) //
            // Otherwise return the original parameter
    private boolean fillFromProviders(URI alias, ConfigDescriptionParameter parameter, @Nullable Locale locale,
            List<ParameterOption> options) {
        for (ConfigOptionProvider configOptionProvider : configOptionProviders) {
            Collection<ParameterOption> newOptions = configOptionProvider.getParameterOptions(alias,
                    parameter.getName(), parameter.getContext(), locale);
            if (newOptions != null) {
                // Simply merge the options
                options.addAll(newOptions);
