package org.openhab.core.config.core.internal.metadata;
import org.openhab.core.config.core.metadata.MetadataConfigDescriptionProvider;
 * A {@link ConfigDescriptionProvider} which translated the information of {@link MetadataConfigDescriptionProvider}
 * implementations to normal {@link ConfigDescription}s.
 * It exposes the config description for the "main" value under
 *     metadata:<namespace>
 * and the config descriptions for the parameters under
 *     metadata:<namespace>:<value>
 * so that it becomes dependent of the main value and extensions can request different parameters from the user
 * depending on which main value was chosen. Implementations of course are free to ignore the {@code value} parameter
 * and always return the same set of config descriptions.
public class MetadataConfigDescriptionProviderImpl implements ConfigDescriptionProvider {
    static final String SCHEME = "metadata";
    static final String SEPARATOR = ":";
    private final List<MetadataConfigDescriptionProvider> providers = new CopyOnWriteArrayList<>();
        return new LinkedList<>(getValueConfigDescriptions(locale));
        if (!SCHEME.equals(uri.getScheme())) {
        String part = uri.getSchemeSpecificPart();
        String namespace = part.contains(SEPARATOR) ? part.substring(0, part.indexOf(SEPARATOR)) : part;
        String value = part.contains(SEPARATOR) ? part.substring(part.indexOf(SEPARATOR) + 1) : null;
        for (MetadataConfigDescriptionProvider provider : providers) {
            if (namespace.equals(provider.getNamespace())) {
                    return createValueConfigDescription(provider, locale);
                    return createParamConfigDescription(provider, value, locale);
    private List<ConfigDescription> getValueConfigDescriptions(@Nullable Locale locale) {
        List<ConfigDescription> ret = new LinkedList<>();
            ret.add(createValueConfigDescription(provider, locale));
    private ConfigDescription createValueConfigDescription(MetadataConfigDescriptionProvider provider,
        String namespace = provider.getNamespace();
        String description = provider.getDescription(locale);
        List<ParameterOption> options = provider.getParameterOptions(locale);
        URI uri = URI.create(SCHEME + SEPARATOR + namespace);
        ConfigDescriptionParameterBuilder builder = ConfigDescriptionParameterBuilder.create("value", Type.TEXT);
        if (options != null && !options.isEmpty()) {
            builder.withOptions(options);
            builder.withLimitToOptions(true);
            builder.withLimitToOptions(false);
        builder.withDescription(description != null ? description : namespace);
        ConfigDescriptionParameter parameter = builder.build();
        return ConfigDescriptionBuilder.create(uri).withParameter(parameter).build();
    private @Nullable ConfigDescription createParamConfigDescription(MetadataConfigDescriptionProvider provider,
            String value, @Nullable Locale locale) {
        URI uri = URI.create(SCHEME + SEPARATOR + namespace + SEPARATOR + value);
        List<ConfigDescriptionParameter> parameters = provider.getParameters(value, locale);
        if (parameters == null || parameters.isEmpty()) {
        return ConfigDescriptionBuilder.create(uri).withParameters(parameters).build();
    protected void addMetadataConfigDescriptionProvider(
            MetadataConfigDescriptionProvider metadataConfigDescriptionProvider) {
        providers.add(metadataConfigDescriptionProvider);
    protected void removeMetadataConfigDescriptionProvider(
        providers.remove(metadataConfigDescriptionProvider);
