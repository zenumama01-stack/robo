 * Utility methods for creation of Things.
 * It is supposed to contain methods that are commonly shared between {@link ThingManagerImpl} and {@link ThingFactory}.
public class ThingFactoryHelper {
    private static Logger logger = LoggerFactory.getLogger(ThingFactoryHelper.class);
     * Create {@link Channel} instances for the given Thing.
     * @param thingType the type of the Thing (must not be null)
     * @param thingUID the Thing's UID (must not be null)
     * @param configDescriptionRegistry {@link ConfigDescriptionRegistry} that will be used to initialize the
     *            {@link Channel}s with their corresponding default values, if given.
     * @return a list of {@link Channel}s
    public static List<Channel> createChannels(ThingType thingType, ThingUID thingUID,
            Channel channel = createChannel(channelDefinition, thingUID, null, configDescriptionRegistry);
        List<ChannelGroupDefinition> channelGroupDefinitions = thingType.getChannelGroupDefinitions();
        withChannelGroupTypeRegistry(channelGroupTypeRegistry -> {
                ChannelGroupType channelGroupType = null;
                if (channelGroupTypeRegistry != null) {
                    channelGroupType = channelGroupTypeRegistry
                            .getChannelGroupType(channelGroupDefinition.getTypeUID());
                if (channelGroupType != null) {
                    List<ChannelDefinition> channelGroupChannelDefinitions = channelGroupType.getChannelDefinitions();
                    for (ChannelDefinition channelDefinition : channelGroupChannelDefinitions) {
                        Channel channel = createChannel(channelDefinition, thingUID, channelGroupDefinition.getId(),
                            "Could not create channels for channel group '{}' for thing type '{}', because channel group type '{}' could not be found.",
                            channelGroupDefinition.getId(), thingUID, channelGroupDefinition.getTypeUID());
    private static <T> T withChannelGroupTypeRegistry(Function<ChannelGroupTypeRegistry, T> consumer) {
        BundleContext bundleContext = FrameworkUtil.getBundle(ThingFactoryHelper.class).getBundleContext();
        ServiceReference ref = bundleContext.getServiceReference(ChannelGroupTypeRegistry.class.getName());
            ChannelGroupTypeRegistry channelGroupTypeRegistry = null;
            if (ref != null) {
                channelGroupTypeRegistry = (ChannelGroupTypeRegistry) bundleContext.getService(ref);
            return consumer.apply(channelGroupTypeRegistry);
                bundleContext.ungetService(ref);
    private static <T> T withChannelTypeRegistry(Function<ChannelTypeRegistry, T> consumer) {
        ServiceReference ref = bundleContext.getServiceReference(ChannelTypeRegistry.class.getName());
            ChannelTypeRegistry channelTypeRegistry = null;
                channelTypeRegistry = (ChannelTypeRegistry) bundleContext.getService(ref);
            return consumer.apply(channelTypeRegistry);
    private static Channel createChannel(ChannelDefinition channelDefinition, ThingUID thingUID, String groupId,
        final ChannelUID channelUID = new ChannelUID(thingUID, groupId, channelDefinition.getId());
    public static ChannelBuilder createChannelBuilder(ChannelUID channelUID, ChannelType channelType,
                .withLabel(channelType.getLabel()) //
                .withAutoUpdatePolicy(channelType.getAutoUpdatePolicy());
        String description = channelType.getDescription();
        if (channelType.getConfigDescriptionURI() != null) {
            applyDefaultConfiguration(configuration, channelType, configDescriptionRegistry);
    public static ChannelBuilder createChannelBuilder(ChannelUID channelUID, ChannelDefinition channelDefinition,
        ChannelType channelType = withChannelTypeRegistry(channelTypeRegistry -> (channelTypeRegistry != null)
                ? channelTypeRegistry.getChannelType(channelDefinition.getChannelTypeUID())
            logger.warn("Could not create channel '{}', because channel type '{}' could not be found.",
                    channelDefinition.getId(), channelDefinition.getChannelTypeUID());
     * Apply the {@link ThingType}'s default values to the given {@link Configuration}.
     * @param thingType the {@link ThingType} where to look for the default values (must not be null)
     * @param configDescriptionRegistry the {@link ConfigDescriptionRegistry} to use (may be null, but method won't have
     *            any effect then)
    public static void applyDefaultConfiguration(Configuration configuration, ThingType thingType,
            @Nullable ConfigDescriptionRegistry configDescriptionRegistry) {
        URI configDescriptionURI = thingType.getConfigDescriptionURI();
        if (configDescriptionURI != null && configDescriptionRegistry != null) {
            // Set default values to thing configuration
                    configDescriptionRegistry.getConfigDescription(configDescriptionURI));
     * Apply the {@link ChannelType}'s default values to the given {@link Configuration}.
     * @param channelType the {@link ChannelType} where to look for the default values (must not be null)
    public static void applyDefaultConfiguration(Configuration configuration, ChannelType channelType,
            // Set default values to channel configuration
