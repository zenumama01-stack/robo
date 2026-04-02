import org.openhab.core.types.CommandDescriptionProvider;
 * Provides the {@link ChannelType} specific {@link CommandDescription} for the given item name and locale.
public class ChannelCommandDescriptionProvider implements CommandDescriptionProvider {
    private final Logger logger = LoggerFactory.getLogger(ChannelCommandDescriptionProvider.class);
    private final List<DynamicCommandDescriptionProvider> dynamicCommandDescriptionProviders = new CopyOnWriteArrayList<>();
    public ChannelCommandDescriptionProvider(final @Reference ItemChannelLinkRegistry itemChannelLinkRegistry,
            final @Reference ThingTypeRegistry thingTypeRegistry, final @Reference ThingRegistry thingRegistry) {
    public @Nullable CommandDescription getCommandDescription(String itemName, @Nullable Locale locale) {
        Set<ChannelUID> boundChannels = itemChannelLinkRegistry.getBoundChannels(itemName);
        if (!boundChannels.isEmpty()) {
            ChannelUID channelUID = boundChannels.iterator().next();
            Channel channel = thingRegistry.getChannel(channelUID);
                CommandDescription commandDescription = null;
                ChannelType channelType = thingTypeRegistry.getChannelType(channel, locale);
                    commandDescription = channelType.getCommandDescription();
                CommandDescription dynamicCommandDescription = getDynamicCommandDescription(channel, commandDescription,
                if (dynamicCommandDescription != null) {
                    return dynamicCommandDescription;
    private @Nullable CommandDescription getDynamicCommandDescription(Channel channel,
        for (DynamicCommandDescriptionProvider dynamicCommandDescriptionProvider : dynamicCommandDescriptionProviders) {
                CommandDescription dynamicCommandDescription = dynamicCommandDescriptionProvider
                        .getCommandDescription(channel, originalCommandDescription, locale);
                    // Compare by reference to make sure a new command description is returned
                    if (dynamicCommandDescription == originalCommandDescription) {
                                "Dynamic command description matches original command description. DynamicCommandDescriptionProvider implementations must never return the original command description. {} has to be fixed.",
                                dynamicCommandDescriptionProvider.getClass());
                logger.error("Error evaluating {}#getCommandDescription: {}",
                        dynamicCommandDescriptionProvider.getClass(), e.getLocalizedMessage(), e);
    protected void addDynamicCommandDescriptionProvider(
            DynamicCommandDescriptionProvider dynamicCommandDescriptionProvider) {
        this.dynamicCommandDescriptionProviders.add(dynamicCommandDescriptionProvider);
    protected void removeDynamicCommandDescriptionProvider(
        this.dynamicCommandDescriptionProviders.remove(dynamicCommandDescriptionProvider);
