package org.openhab.core.thing.fileconverter;
 * {@link AbstractThingSerializer} is the base class for any {@link Thing} serializer.
public abstract class AbstractThingSerializer implements ThingSerializer {
    protected final ThingTypeRegistry thingTypeRegistry;
    protected final ChannelTypeRegistry channelTypeRegistry;
    protected final ConfigDescriptionRegistry configDescRegistry;
    public AbstractThingSerializer(ThingTypeRegistry thingTypeRegistry, ChannelTypeRegistry channelTypeRegistry,
            ConfigDescriptionRegistry configDescRegistry) {
     * {@link ConfigParameter} is a container for any configuration parameter defined by a name and a value.
    protected record ConfigParameter(String name, Object value) {
     * Get the child things of a bridge thing amongst a list of things, ordered by UID.
     * @param fromThings the list of things to look for
     * @return the sorted list of child things or an empty list if the thing is not a bridge thing
    protected List<Thing> getChildThings(Thing thing, List<Thing> fromThings) {
            return fromThings.stream().filter(th -> bridge.getUID().equals(th.getBridgeUID()))
     * Get the list of configuration parameters for a thing.
     * If a configuration description is found for the thing type, the parameters are provided in the same order
     * as in this configuration description, and any parameter having the default value is ignored.
     * If not, the parameters are provided sorted by natural order of their names.
     * @param hideDefaultParameters true to hide the configuration parameters having the default value
     * @return the sorted list of configuration parameters for the thing
    protected List<ConfigParameter> getConfigurationParameters(Thing thing, boolean hideDefaultParameters) {
        return getConfigurationParameters(getConfigDescriptionParameters(thing), thing.getConfiguration(),
                hideDefaultParameters);
     * Get the list of configuration parameters for a channel.
     * If a configuration description is found for the channel type, the parameters are provided in the same order
     * @return the sorted list of configuration parameters for the channel
    protected List<ConfigParameter> getConfigurationParameters(Channel channel, boolean hideDefaultParameters) {
        return getConfigurationParameters(getConfigDescriptionParameters(channel), channel.getConfiguration(),
    private List<ConfigParameter> getConfigurationParameters(
            List<ConfigDescriptionParameter> configDescriptionParameter, Configuration configParameters,
    private List<ConfigDescriptionParameter> getConfigDescriptionParameters(Thing thing) {
        List<ConfigDescriptionParameter> configParams = null;
            configParams = getConfigDescriptionParameters(thingType.getConfigDescriptionURI());
        return configParams != null ? configParams : List.of();
    private List<ConfigDescriptionParameter> getConfigDescriptionParameters(Channel channel) {
                configParams = getConfigDescriptionParameters(channelType.getConfigDescriptionURI());
    private @Nullable List<ConfigDescriptionParameter> getConfigDescriptionParameters(@Nullable URI descURI) {
            ConfigDescription configDesc = configDescRegistry.getConfigDescription(descURI);
                return configDesc.getParameters();
     * Get non default channels.
     * It includes extensible channels and channels with a non default configuration.
     * @return the list of channels
    protected List<Channel> getNonDefaultChannels(Thing thing) {
        List<String> ids = thingType != null ? thingType.getExtensibleChannelTypeIds() : List.of();
        return thing
                .getChannels().stream().filter(ch -> ch.getChannelTypeUID() == null
                        || ids.contains(ch.getChannelTypeUID().getId()) || channelWithNonDefaultConfig(ch))
    private boolean channelWithNonDefaultConfig(Channel channel) {
        for (ConfigDescriptionParameter param : getConfigDescriptionParameters(channel)) {
            Object value = channel.getConfiguration().get(param.getName());
                value = ConfigUtil.normalizeType(value, param);
                if (!value.equals(ConfigUtil.getDefaultValueAsCorrectType(param))) {
