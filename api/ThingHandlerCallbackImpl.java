 * The {@link ThingHandlerCallbackImpl} implements the {@link ThingHandlerCallback} interface
class ThingHandlerCallbackImpl implements ThingHandlerCallback {
    private final Logger logger = LoggerFactory.getLogger(ThingHandlerCallbackImpl.class);
    private final ThingManagerImpl thingManager;
    public ThingHandlerCallbackImpl(ThingManagerImpl thingManager) {
        thingManager.communicationManager.stateUpdated(channelUID, state);
        thingManager.communicationManager.postCommand(channelUID, command);
        thingManager.communicationManager.sendTimeSeries(channelUID, timeSeries);
        thingManager.communicationManager.channelTriggered(thing, channelUID, event);
    public void statusUpdated(Thing thing, ThingStatusInfo statusInfo) {
        // note: all operations based on a status update should be executed asynchronously!
        ThingStatusInfo oldStatusInfo = thing.getStatusInfo();
        ensureValidStatus(oldStatusInfo.getStatus(), statusInfo.getStatus());
        if (ThingStatus.REMOVING.equals(oldStatusInfo.getStatus())
                && !ThingStatus.REMOVED.equals(statusInfo.getStatus())) {
            // if we go to ONLINE and are still in REMOVING, notify handler about required removal
            if (ThingStatus.ONLINE.equals(statusInfo.getStatus())) {
                logger.debug("Handler is initialized now and we try to remove it, because it is in REMOVING state.");
                thingManager.notifyThingHandlerAboutRemoval(thing);
            // only allow REMOVING -> REMOVED transition, all others are ignored because they are illegal
                    "Ignoring illegal status transition for thing {} from REMOVING to {}, only REMOVED would have been allowed.",
                    thing.getUID(), statusInfo.getStatus());
        // update thing status and send event about new status
        thingManager.setThingStatus(thing, statusInfo);
        // if thing is a bridge
            handleBridgeStatusUpdate(bridge, statusInfo, oldStatusInfo);
        // if thing has a bridge
        if (thing.getBridgeUID() != null) {
            handleBridgeChildStatusUpdate(thing, oldStatusInfo);
        // notify thing registry about thing removal
        if (ThingStatus.REMOVED.equals(thing.getStatus())) {
            thingManager.notifyRegistryAboutForceRemove(thing);
    private void ensureValidStatus(ThingStatus oldStatus, ThingStatus newStatus) {
        if (!(ThingStatus.UNKNOWN.equals(newStatus) || ThingStatus.ONLINE.equals(newStatus)
                || ThingStatus.OFFLINE.equals(newStatus) || ThingStatus.REMOVED.equals(newStatus))) {
                    MessageFormat.format("Illegal status {0}. Bindings only may set {1}, {2}, {3} or {4}.", newStatus,
                            ThingStatus.UNKNOWN, ThingStatus.ONLINE, ThingStatus.OFFLINE, ThingStatus.REMOVED));
        if (ThingStatus.REMOVED.equals(newStatus) && !ThingStatus.REMOVING.equals(oldStatus)) {
                    MessageFormat.format("Illegal status {0}. The thing was in state {1} and not in {2}", newStatus,
                            oldStatus, ThingStatus.REMOVING));
    private void handleBridgeStatusUpdate(Bridge bridge, ThingStatusInfo statusInfo, ThingStatusInfo oldStatusInfo) {
        if (ThingHandlerHelper.isHandlerInitialized(bridge)
                && (ThingStatus.INITIALIZING.equals(oldStatusInfo.getStatus()))) {
            // bridge has just been initialized: initialize child things as well
            thingManager.registerChildHandlers(bridge);
        } else if (!statusInfo.equals(oldStatusInfo)) {
            // bridge status has been changed: notify child things about status change
            thingManager.notifyThingsAboutBridgeStatusChange(bridge, statusInfo);
    private void handleBridgeChildStatusUpdate(Thing thing, ThingStatusInfo oldStatusInfo) {
        if (ThingHandlerHelper.isHandlerInitialized(thing)
                && ThingStatus.INITIALIZING.equals(oldStatusInfo.getStatus())) {
            // child thing has just been initialized: notify bridge about it
            thingManager.notifyBridgeAboutChildHandlerInitialization(thing);
    public void thingUpdated(final Thing thing) {
        thingManager.thingUpdated(thing);
    public void validateConfigurationParameters(Thing thing, Map<String, Object> configurationParameters) {
        ThingType thingType = thingManager.thingTypeRegistry.getThingType(thing.getThingTypeUID());
                thingManager.configDescriptionValidator.validate(configurationParameters, configDescriptionURI);
    public void validateConfigurationParameters(Channel channel, Map<String, Object> configurationParameters) {
        ChannelType channelType = thingManager.channelTypeRegistry.getChannelType(channel.getChannelTypeUID());
    public @Nullable ConfigDescription getConfigDescription(ChannelTypeUID channelTypeUID) {
        ChannelType channelType = thingManager.channelTypeRegistry.getChannelType(channelTypeUID);
            URI configDescriptionUri = channelType.getConfigDescriptionURI();
            if (configDescriptionUri != null) {
                return thingManager.configDescriptionRegistry.getConfigDescription(configDescriptionUri);
    public @Nullable ConfigDescription getConfigDescription(ThingTypeUID thingTypeUID) {
        ThingType thingType = thingManager.thingTypeRegistry.getThingType(thingTypeUID);
            URI configDescriptionUri = thingType.getConfigDescriptionURI();
    public void configurationUpdated(Thing thing) {
        if (!ThingHandlerHelper.isHandlerInitialized(thing)) {
            thingManager.initializeHandler(thing);
    public void migrateThingType(final Thing thing, final ThingTypeUID thingTypeUID,
            final Configuration configuration) {
        thingManager.migrateThingType(thing, thingTypeUID, configuration);
    public ChannelBuilder createChannelBuilder(ChannelUID channelUID, ChannelTypeUID channelTypeUID) {
            throw new IllegalArgumentException(String.format("Channel type '%s' is not known", channelTypeUID));
        return ThingFactoryHelper.createChannelBuilder(channelUID, channelType, thingManager.configDescriptionRegistry);
    public ChannelBuilder editChannel(Thing thing, ChannelUID channelUID) {
                    String.format("Channel '%s' does not exist for thing '%s'", channelUID, thing.getUID()));
        return ChannelBuilder.create(channel);
    public List<ChannelBuilder> createChannelBuilders(ChannelGroupUID channelGroupUID,
            ChannelGroupTypeUID channelGroupTypeUID) {
        ChannelGroupType channelGroupType = thingManager.channelGroupTypeRegistry
                .getChannelGroupType(channelGroupTypeUID);
                    String.format("Channel group type '%s' is not known", channelGroupTypeUID));
        List<ChannelBuilder> channelBuilders = new ArrayList<>();
        for (ChannelDefinition channelDefinition : channelGroupType.getChannelDefinitions()) {
            ChannelType channelType = thingManager.channelTypeRegistry
                    .getChannelType(channelDefinition.getChannelTypeUID());
                ChannelUID channelUID = new ChannelUID(channelGroupUID, channelDefinition.getId());
                ChannelBuilder channelBuilder = ThingFactoryHelper.createChannelBuilder(channelUID, channelDefinition,
                        thingManager.configDescriptionRegistry);
                if (channelBuilder != null) {
                    channelBuilders.add(channelBuilder);
        return channelBuilders;
    public boolean isChannelLinked(ChannelUID channelUID) {
        return thingManager.itemChannelLinkRegistry.isLinked(channelUID);
    public @Nullable Bridge getBridge(ThingUID bridgeUID) {
        Thing bridgeThing = thingManager.thingRegistry.get(bridgeUID);
        if (bridgeThing instanceof Bridge bridge) {
            return bridge;
