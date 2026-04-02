 * Component which takes care of calculating and sending potential auto-update event.
 * @author Kai Kreuzer - fixed issues if a linked thing is OFFLINE
@Component(immediate = true, service = {
        AutoUpdateManager.class }, configurationPid = "org.openhab.autoupdate", configurationPolicy = ConfigurationPolicy.OPTIONAL)
public class AutoUpdateManager {
    private static final String AUTOUPDATE_KEY = "autoupdate";
    protected static final String EVENT_SOURCE = "org.openhab.core.autoupdate";
    protected static final String EVENT_SOURCE_OPTIMISTIC = "org.openhab.core.autoupdate.optimistic";
    protected static final String PROPERTY_ENABLED = "enabled";
    protected static final String PROPERTY_SEND_OPTIMISTIC_UPDATES = "sendOptimisticUpdates";
    private final Logger logger = LoggerFactory.getLogger(AutoUpdateManager.class);
    private boolean sendOptimisticUpdates = false;
    private enum Recommendation {
         * An automatic state update must be sent because no channels are linked to the item.
        REQUIRED,
         * An automatic state update should be sent because none of the linked channels are capable to retrieve the
         * actual state for their device.
        RECOMMENDED,
         * An automatic state update may be sent in the optimistic anticipation of what the handler is going to send
         * soon anyway.
        OPTIMISTIC,
         * No automatic state update should be sent because at least one channel claims it can handle it better.
        DONT,
         * There are channels linked to the item which in theory would do the state update, but none of them is
         * currently able to communicate with their devices, hence no automatic state update should be done and the
         * previous item state should be sent instead in order to revert any control.
        REVERT
    public AutoUpdateManager(Map<String, Object> configuration,
            final @Reference EventPublisher eventPublisher,
            final @Reference MetadataRegistry metadataRegistry, //
            final @Reference ThingRegistry thingRegistry) {
        modified(configuration);
    protected void modified(Map<String, Object> configuration) {
        Object valueEnabled = configuration.get(PROPERTY_ENABLED);
        if (valueEnabled != null) {
            enabled = Boolean.parseBoolean(valueEnabled.toString());
        Object valueSendOptimisticUpdates = configuration.get(PROPERTY_SEND_OPTIMISTIC_UPDATES);
        if (valueSendOptimisticUpdates != null) {
            sendOptimisticUpdates = Boolean.parseBoolean(valueSendOptimisticUpdates.toString());
    public void receiveCommand(ItemCommandEvent commandEvent, Item item) {
        final String itemName = commandEvent.getItemName();
        final Command command = commandEvent.getItemCommand();
        if (command instanceof State state) {
            Recommendation autoUpdate = shouldAutoUpdate(item);
            // consider user-override via item meta-data
            MetadataKey key = new MetadataKey(AUTOUPDATE_KEY, itemName);
            if (metadata != null && !metadata.getValue().isBlank()) {
                boolean override = Boolean.parseBoolean(metadata.getValue());
                    logger.trace("Auto update strategy {} overriden by item metadata to REQUIRED", autoUpdate);
                    autoUpdate = Recommendation.REQUIRED;
                    logger.trace("Auto update strategy {} overriden by item metadata to DONT", autoUpdate);
                    autoUpdate = Recommendation.DONT;
            switch (autoUpdate) {
                case REQUIRED:
                    logger.trace("Automatically updating item '{}' because no channel is linked", itemName);
                    postUpdate(item, state, EVENT_SOURCE);
                case RECOMMENDED:
                    logger.trace("Automatically updating item '{}' because no channel does it", itemName);
                case OPTIMISTIC:
                    logger.trace("Optimistically updating item '{}'", itemName);
                    postPrediction(item, state, false);
                    if (sendOptimisticUpdates) {
                        postUpdate(item, state, EVENT_SOURCE_OPTIMISTIC);
                case DONT:
                    logger.trace("Won't update item '{}' as it was vetoed.", itemName);
                case REVERT:
                    logger.trace("Sending current item state to revert controls '{}'", itemName);
                    postPrediction(item, item.getState(), true);
    private Recommendation shouldAutoUpdate(Item item) {
        Recommendation ret = Recommendation.REQUIRED;
        // check if the item is a group item
            return Recommendation.DONT;
        List<ChannelUID> linkedChannelUIDs = new ArrayList<>();
        for (ItemChannelLink link : itemChannelLinkRegistry.getLinks(itemName)) {
            linkedChannelUIDs.add(link.getLinkedUID());
        // check if there is any channel ONLINE
        List<ChannelUID> onlineChannelUIDs = new ArrayList<>();
        for (ChannelUID channelUID : linkedChannelUIDs) {
            if (thing == null //
                    || thing.getChannel(channelUID) == null //
                    || thing.getHandler() == null //
                    || !ThingStatus.ONLINE.equals(thing.getStatus()) //
            onlineChannelUIDs.add(channelUID);
        if (!linkedChannelUIDs.isEmpty() && onlineChannelUIDs.isEmpty()) {
            // none of the linked channels is able to process the command
            return Recommendation.REVERT;
        for (ChannelUID channelUID : onlineChannelUIDs) {
            AutoUpdatePolicy policy = AutoUpdatePolicy.DEFAULT;
            Channel channel = thing.getChannel(channelUID);
                AutoUpdatePolicy channelpolicy = channel.getAutoUpdatePolicy();
                if (channelpolicy != null) {
                    policy = channelpolicy;
                    ChannelType channelType = channelTypeRegistry.getChannelType(channel.getChannelTypeUID());
                    if (channelType != null && channelType.getAutoUpdatePolicy() != null) {
                        policy = channelType.getAutoUpdatePolicy();
            switch (policy) {
                case VETO:
                    ret = Recommendation.DONT;
                case DEFAULT:
                    if (ret == Recommendation.REQUIRED || ret == Recommendation.RECOMMENDED) {
                        ret = Recommendation.OPTIMISTIC;
                case RECOMMEND:
                    if (ret == Recommendation.REQUIRED) {
                        ret = Recommendation.RECOMMENDED;
    private void postUpdate(Item item, State newState, String origin) {
        boolean isAccepted = isAcceptedState(newState, item);
        if (isAccepted) {
            eventPublisher.post(ItemEventFactory.createStateEvent(item.getName(), newState, origin));
            logger.debug("Received update of a not accepted type ({}) for item {}", newState.getClass().getSimpleName(),
                    item.getName());
    private void postPrediction(Item item, State predictedState, boolean isConfirmation) {
        boolean isAccepted = isAcceptedState(predictedState, item);
            eventPublisher
                    .post(ItemEventFactory.createStatePredictedEvent(item.getName(), predictedState, isConfirmation));
            logger.debug("Received prediction of a not accepted type ({}) for item {}",
                    predictedState.getClass().getSimpleName(), item.getName());
    private boolean isAcceptedState(State newState, Item item) {
        boolean isAccepted = false;
        if (item.getAcceptedDataTypes().contains(newState.getClass())) {
            isAccepted = true;
            // Look for class hierarchy
            for (Class<?> state : item.getAcceptedDataTypes()) {
                    if (!state.isEnum() && state.getDeclaredConstructor().newInstance().getClass()
                            .isAssignableFrom(newState.getClass())) {
                } catch (InstantiationException | IllegalAccessException | NoSuchMethodException
                    logger.warn("Exception on {}", e.getMessage(), e); // Should never happen
        return isAccepted;
