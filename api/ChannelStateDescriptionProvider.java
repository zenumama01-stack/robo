 * A {@link ChannelStateDescriptionProvider} provides localized {@link StateDescription}s from the type of a
 * {@link Channel} bounded to an {@link Item}.
@Component(immediate = true, property = { "service.ranking:Integer=-1" })
public class ChannelStateDescriptionProvider implements StateDescriptionFragmentProvider {
    private final Logger logger = LoggerFactory.getLogger(ChannelStateDescriptionProvider.class);
    private final List<DynamicStateDescriptionProvider> dynamicStateDescriptionProviders = new CopyOnWriteArrayList<>();
    private Integer rank = 0;
    public ChannelStateDescriptionProvider(final @Reference ItemChannelLinkRegistry itemChannelLinkRegistry,
        StateDescription stateDescription = getStateDescription(itemName, locale);
            return StateDescriptionFragmentBuilder.create(stateDescription).build();
    private @Nullable StateDescription getStateDescription(String itemName, @Nullable Locale locale) {
        StateDescription stateDescription = null;
        for (ChannelUID channelUID : boundChannels) {
                StateDescription nextStateDescription = null;
                    nextStateDescription = channelType.getState();
                StateDescription dynamicStateDescription = getDynamicStateDescription(channel, nextStateDescription,
                if (dynamicStateDescription != null) {
                    nextStateDescription = dynamicStateDescription;
                if (nextStateDescription != null) {
                    if (stateDescription == null) {
                        stateDescription = nextStateDescription;
                        if (stateDescription.isReadOnly() && !nextStateDescription.isReadOnly()) {
    private @Nullable StateDescription getDynamicStateDescription(Channel channel,
            @Nullable StateDescription originalStateDescription, @Nullable Locale locale) {
        for (DynamicStateDescriptionProvider provider : dynamicStateDescriptionProviders) {
            StateDescription dynamicStateDescription = provider.getStateDescription(channel, originalStateDescription,
                // Compare by reference to make sure a new state description is returned
                if (dynamicStateDescription == originalStateDescription) {
                            "Dynamic state description matches original state description. DynamicStateDescriptionProvider implementations must never return the original state description. {} has to be fixed.",
                            provider.getClass());
                    return dynamicStateDescription;
    protected void addDynamicStateDescriptionProvider(DynamicStateDescriptionProvider dynamicStateDescriptionProvider) {
        this.dynamicStateDescriptionProviders.add(dynamicStateDescriptionProvider);
    protected void removeDynamicStateDescriptionProvider(
            DynamicStateDescriptionProvider dynamicStateDescriptionProvider) {
        this.dynamicStateDescriptionProviders.remove(dynamicStateDescriptionProvider);
