import org.openhab.core.items.ItemStateConverter;
import org.openhab.core.items.events.AbstractItemRegistryEvent;
import org.openhab.core.thing.events.AbstractThingRegistryEvent;
import org.openhab.core.thing.internal.link.ItemChannelLinkConfigDescriptionProvider;
import org.openhab.core.thing.internal.profiles.ProfileCallbackImpl;
import org.openhab.core.thing.internal.profiles.SystemProfileFactory;
import org.openhab.core.thing.profiles.ProfileAdvisor;
import org.openhab.core.thing.profiles.StateProfile;
import org.openhab.core.thing.profiles.TriggerProfile;
 * This class manages the state related communication between bindings and the framework.
 * It mainly mediates commands, state updates and triggers from ThingHandlers to the framework and vice versa.
 * @author Simon Kaufmann - Initial contribution factored out of ThingManger
@Component(service = { EventSubscriber.class, CommunicationManager.class }, immediate = true)
public class CommunicationManager implements EventSubscriber, RegistryChangeListener<ItemChannelLink> {
    private record CacheKey(String type, Profile profile, Thing thing) {
    private static final Profile NO_OP_PROFILE = new Profile() {
        private final ProfileTypeUID noOpProfileUID = new ProfileTypeUID(ProfileTypeUID.SYSTEM_SCOPE, "noop");
            return noOpProfileUID;
    // the timeout to use for any item event processing
    public static final long THINGHANDLER_EVENT_TIMEOUT = TimeUnit.SECONDS.toMillis(30);
    private static final Set<String> SUBSCRIBED_EVENT_TYPES = Set.of(ItemStateUpdatedEvent.TYPE, ItemCommandEvent.TYPE,
            GroupStateUpdatedEvent.TYPE, ChannelTriggeredEvent.TYPE);
    private final Logger logger = LoggerFactory.getLogger(CommunicationManager.class);
    private final AutoUpdateManager autoUpdateManager;
    private final SystemProfileFactory defaultProfileFactory;
    private final ItemStateConverter itemStateConverter;
    private final ConcurrentHashMap<CacheKey, Profile> profileSafeCallCache = new ConcurrentHashMap<>();
    public CommunicationManager(final @Reference AutoUpdateManager autoUpdateManager,
            final @Reference SystemProfileFactory defaultProfileFactory,
            final @Reference ItemStateConverter itemStateConverter, //
            final @Reference SafeCaller safeCaller, //
        this.autoUpdateManager = autoUpdateManager;
        this.defaultProfileFactory = defaultProfileFactory;
        this.itemStateConverter = itemStateConverter;
    private final Set<ItemFactory> itemFactories = new CopyOnWriteArraySet<>();
    // link UID -> profile
    private final Map<String, Profile> profiles = new ConcurrentHashMap<>();
    // factory instance -> link UIDs which the factory has created profiles for
    private final Map<ProfileFactory, Set<String>> profileFactories = new ConcurrentHashMap<>();
    private final Set<ProfileAdvisor> profileAdvisors = new CopyOnWriteArraySet<>();
    private final Map<String, List<Class<? extends Command>>> acceptedCommandTypeMap = new ConcurrentHashMap<>();
    private final Map<String, List<Class<? extends State>>> acceptedStateTypeMap = new ConcurrentHashMap<>();
        return SUBSCRIBED_EVENT_TYPES;
        if (event instanceof ItemStateUpdatedEvent updatedEvent) {
            receiveUpdate(updatedEvent);
        } else if (event instanceof ItemCommandEvent commandEvent) {
            receiveCommand(commandEvent);
        } else if (event instanceof ChannelTriggeredEvent triggeredEvent) {
            receiveTrigger(triggeredEvent);
        } else if (event instanceof AbstractItemRegistryEvent registryEvent) {
            String itemName = registryEvent.getItem().name;
            profiles.entrySet().removeIf(entry -> {
                ItemChannelLink link = itemChannelLinkRegistry.get(entry.getKey());
                return link != null && itemName.equals(link.getItemName());
        } else if (event instanceof AbstractThingRegistryEvent registryEvent) {
            ThingUID thingUid = new ThingUID(registryEvent.getThing().UID);
                return link != null && thingUid.equals(link.getLinkedUID().getThingUID());
    private Profile getProfile(ItemChannelLink link, Item item, @Nullable Thing thing) {
        synchronized (profiles) {
            Profile profile = profiles.get(link.getUID());
            if (profile != null) {
                logger.trace("Using profile '{}' from cache for link '{}'", profile.getProfileTypeUID(), link);
                return profile;
            ProfileTypeUID profileTypeUID = determineProfileTypeUID(link, item, thing);
            if (profileTypeUID != null) {
                profile = getProfileFromFactories(profileTypeUID, link, createCallback(link));
                    profiles.put(link.getUID(), profile);
            logger.trace("No Profile found for link '{}', using NoOpProfile", link);
            return NO_OP_PROFILE;
    private ProfileCallback createCallback(ItemChannelLink link) {
        return new ProfileCallbackImpl(eventPublisher, safeCaller, itemStateConverter, link, thingRegistry::get,
                this::getItem, this::toAcceptedCommand);
    private @Nullable ProfileTypeUID determineProfileTypeUID(ItemChannelLink link, Item item, @Nullable Thing thing) {
        ProfileTypeUID profileTypeUID = getConfiguredProfileTypeUID(link);
        Channel channel;
        if (profileTypeUID == null) {
            channel = thing.getChannel(link.getLinkedUID());
            // ask advisors
            profileTypeUID = getAdvice(link, item, channel);
                // ask default advisor
                logger.trace("No profile advisor found for link '{}', falling back to the defaults", link);
                profileTypeUID = defaultProfileFactory.getSuggestedProfileTypeUID(channel, item.getType());
    private @Nullable ProfileTypeUID getAdvice(ItemChannelLink link, Item item, Channel channel) {
        ProfileTypeUID ret;
        for (ProfileAdvisor advisor : profileAdvisors) {
            ret = advisor.getSuggestedProfileTypeUID(channel, item.getType());
            if (ret != null) {
    private @Nullable ProfileTypeUID getConfiguredProfileTypeUID(ItemChannelLink link) {
        String profileName = (String) link.getConfiguration()
                .get(ItemChannelLinkConfigDescriptionProvider.PARAM_PROFILE);
        if (profileName != null && !profileName.trim().isEmpty()) {
            if (!profileName.contains(AbstractUID.SEPARATOR)) {
                profileName = ProfileTypeUID.SYSTEM_SCOPE + AbstractUID.SEPARATOR + profileName;
            return new ProfileTypeUID(profileName);
    private @Nullable Profile getProfileFromFactories(ProfileTypeUID profileTypeUID, ItemChannelLink link,
            ProfileCallback callback) {
        ProfileContext context = null;
        Item item = getItem(link.getItemName());
        ThingUID thingUID = link.getLinkedUID().getThingUID();
        if (item != null && thing != null) {
            Channel channel = thing.getChannel(link.getLinkedUID());
                String acceptedItemType = Objects.requireNonNullElse(channel.getAcceptedItemType(), "");
                if (acceptedItemType.startsWith("Number")) {
                    acceptedItemType = "Number";
                context = new ProfileContextImpl(link.getConfiguration(), item.getAcceptedDataTypes(),
                        item.getAcceptedCommandTypes(),
                        acceptedCommandTypeMap.getOrDefault(acceptedItemType, List.of()));
            logger.debug("Could not create full channel context, item or channel missing in registry.");
        if (supportsProfileTypeUID(defaultProfileFactory, profileTypeUID)) {
            logger.trace("Using the default ProfileFactory to create profile '{}' for link '{}'", profileTypeUID, link);
            return defaultProfileFactory.createProfile(profileTypeUID, callback, context);
        for (Entry<ProfileFactory, Set<String>> entry : profileFactories.entrySet()) {
            ProfileFactory factory = entry.getKey();
            if (supportsProfileTypeUID(factory, profileTypeUID)) {
                logger.trace("Using ProfileFactory '{}' to create profile '{}' for link '{}'", factory, profileTypeUID,
                        link);
                Profile profile = factory.createProfile(profileTypeUID, callback, context);
                if (profile == null) {
                    logger.error("ProfileFactory '{}' returned 'null' although it claimed to support profile '{}'",
                            factory, profileTypeUID);
                    entry.getValue().add(link.getUID());
        logger.warn("No ProfileFactory found which supports profile '{}' for link '{}'", profileTypeUID, link);
    private boolean supportsProfileTypeUID(ProfileFactory profileFactory, ProfileTypeUID profileTypeUID) {
        return profileFactory.getSupportedProfileTypeUIDs().contains(profileTypeUID);
    private void receiveCommand(ItemCommandEvent commandEvent) {
            autoUpdateManager.receiveCommand(commandEvent, item);
        handleEvent(itemName, command, commandEvent.getSource(), acceptedCommandTypeMap::get,
                this::applyProfileForCommand);
    private void receiveUpdate(ItemStateUpdatedEvent updateEvent) {
        final String itemName = updateEvent.getItemName();
        final State newState = updateEvent.getItemState();
        handleEvent(itemName, newState, updateEvent.getSource(), acceptedStateTypeMap::get,
                this::applyProfileForUpdate);
    private interface ProfileAction<T extends Type> {
        void applyProfile(Profile profile, Thing thing, T type, @Nullable String source);
    private void applyProfileForUpdate(Profile profile, Thing thing, State convertedState, @Nullable String source) {
        CacheKey key = new CacheKey("UPDATE", profile, thing);
        Profile p = profileSafeCallCache.computeIfAbsent(key, (k) -> safeCaller.create(k.profile, Profile.class) //
                .withAsync() //
                .withIdentifier(k.thing) //
                .withTimeout(THINGHANDLER_EVENT_TIMEOUT) //
                .build());
            p.onStateUpdateFromItem(convertedState);
            throw new IllegalStateException("ExpiringCache didn't provide a Profile instance!");
    private void applyProfileForCommand(Profile profile, Thing thing, Command convertedCommand,
        if (profile instanceof StateProfile) {
            CacheKey key = new CacheKey("COMMAND", profile, thing);
            Profile p = profileSafeCallCache.computeIfAbsent(key,
                    (k) -> safeCaller.create((StateProfile) k.profile, StateProfile.class) //
            if (p instanceof StateProfile profileP) {
                profileP.onCommandFromItem(convertedCommand, source);
                throw new IllegalStateException("ExpiringCache didn't provide a StateProfile instance!");
    private <T extends Type> void handleEvent(String itemName, T type, @Nullable String source,
            Function<@Nullable String, @Nullable List<Class<? extends T>>> acceptedTypesFunction,
            ProfileAction<T> action) {
            logger.debug("Received an event for item {} which does not exist", itemName);
        itemChannelLinkRegistry.getLinks(itemName).stream().filter(link -> {
            // make sure the command event is not sent back to its source
            return !link.getLinkedUID().toString().equals(source);
        }).forEach(link -> {
            ChannelUID channelUID = link.getLinkedUID();
                    if (thing.getHandler() != null) {
                        // fix QuantityType/DecimalType, leave others as-is
                        T uomType = fixUoM(type, channel, item);
                        Profile profile = getProfile(link, item, thing);
                        action.applyProfile(profile, thing, uomType != null ? uomType : type, source);
                    logger.debug("Received  event '{}' for non-existing channel '{}', not forwarding it to the handler",
                            type, channelUID);
                logger.debug("Received  event '{}' for non-existing thing '{}', not forwarding it to the handler", type,
                        channelUID.getThingUID());
    private <T extends Type> @Nullable T fixUoM(@Nullable T originalType, Channel channel, Item item) {
        String channelAcceptedItemType = channel.getAcceptedItemType();
        if (channelAcceptedItemType == null) {
            return originalType;
        // handle Number-Channels for backward compatibility
        if (CoreItemFactory.NUMBER.equals(channelAcceptedItemType)
                && originalType instanceof QuantityType<?> quantityType) {
            // strip unit from QuantityType for channels that accept plain number
            return (T) new DecimalType(quantityType.toBigDecimal());
        String itemDimension = ItemUtil.getItemTypeExtension(item.getType());
        String channelDimension = ItemUtil.getItemTypeExtension(channelAcceptedItemType);
        if (originalType instanceof DecimalType decimalType && channelDimension != null
                && channelDimension.equals(itemDimension)) {
            // Add unit from item to DecimalType when dimensions are equal
            Unit<?> unit = Objects.requireNonNull(((NumberItem) item).getUnit());
            return (T) new QuantityType<>(decimalType.toBigDecimal(), unit);
    public @Nullable Command toAcceptedCommand(Command originalType, @Nullable Channel channel, @Nullable Item item) {
        if (item == null || channel == null) {
            logger.warn("Trying to convert types for non-existing channel or item, discarding command.");
        Command uomCommand = fixUoM(originalType, channel, item);
        if (uomCommand != null) {
            return uomCommand;
        // handle HSBType/PercentType
        if (CoreItemFactory.DIMMER.equals(channelAcceptedItemType) && originalType instanceof HSBType hsb) {
            return hsb.as(PercentType.class);
        // check for other cases if the type is acceptable
        List<Class<? extends Command>> acceptedTypes = acceptedCommandTypeMap.get(channelAcceptedItemType);
        if (acceptedTypes == null || acceptedTypes.contains(originalType.getClass())) {
        } else if (acceptedTypes.contains(PercentType.class) && originalType instanceof State state
                && PercentType.class.isAssignableFrom(originalType.getClass())) {
            return state.as(PercentType.class);
        } else if (acceptedTypes.contains(OnOffType.class) && originalType instanceof State state
            return state.as(OnOffType.class);
            logger.debug("Received not accepted type '{}' for channel '{}'", originalType.getClass().getSimpleName(),
                    channel.getUID());
    private @Nullable Item getItem(final String itemName) {
    private void receiveTrigger(ChannelTriggeredEvent channelTriggeredEvent) {
        final ChannelUID channelUID = channelTriggeredEvent.getChannel();
        final String event = channelTriggeredEvent.getEvent();
        final Thing thing = thingRegistry.get(thingUID);
        handleCallFromHandler(channelUID, thing, profile -> {
            if (profile instanceof TriggerProfile triggerProfile) {
                triggerProfile.onTriggerFromHandler(event);
    public void stateUpdated(ChannelUID channelUID, State state) {
            if (profile instanceof StateProfile stateProfile) {
                stateProfile.onStateUpdateFromHandler(state);
    public void postCommand(ChannelUID channelUID, Command command) {
                stateProfile.onCommandFromHandler(command);
    public void sendTimeSeries(ChannelUID channelUID, TimeSeries timeSeries) {
            // TODO: check which profiles need enhancements
            if (profile instanceof TimeSeriesProfile timeSeriesProfile) {
                timeSeriesProfile.onTimeSeriesFromHandler(timeSeries);
                logger.warn("Profile '{}' on channel {} does not support time series.", profile.getProfileTypeUID(),
                        channelUID);
    private void handleCallFromHandler(ChannelUID channelUID, @Nullable Thing thing, Consumer<Profile> action) {
        itemChannelLinkRegistry.getLinks(channelUID).forEach(link -> {
            final Item item = getItem(link.getItemName());
                final Profile profile = getProfile(link, item, thing);
                action.accept(profile);
    public void channelTriggered(Thing thing, ChannelUID channelUID, String event) {
        eventPublisher.post(ThingEventFactory.createTriggerEvent(event, channelUID));
    private void cleanup(ItemChannelLink link) {
            profiles.remove(link.getUID());
        profileFactories.values().forEach(list -> list.remove(link.getUID()));
        cleanup(element);
        cleanup(oldElement);
    protected void addProfileFactory(ProfileFactory profileFactory) {
        profileFactories.put(profileFactory, ConcurrentHashMap.newKeySet());
    protected void removeProfileFactory(ProfileFactory profileFactory) {
        Set<String> links = profileFactories.remove(profileFactory);
            links.forEach(profiles::remove);
    protected void addProfileAdvisor(ProfileAdvisor profileAdvisor) {
        profileAdvisors.add(profileAdvisor);
    protected void removeProfileAdvisor(ProfileAdvisor profileAdvisor) {
        profileAdvisors.remove(profileAdvisor);
    protected void addItemFactory(ItemFactory itemFactory) {
        itemFactories.add(itemFactory);
        calculateAcceptedTypes();
    protected void removeItemFactory(ItemFactory itemFactory) {
        itemFactories.remove(itemFactory);
    private synchronized void calculateAcceptedTypes() {
        acceptedCommandTypeMap.clear();
        acceptedStateTypeMap.clear();
        for (ItemFactory itemFactory : itemFactories) {
            for (String itemTypeName : itemFactory.getSupportedItemTypes()) {
                Item item = itemFactory.createItem(itemTypeName, "tmp");
                    acceptedCommandTypeMap.put(itemTypeName, item.getAcceptedCommandTypes());
                    acceptedStateTypeMap.put(itemTypeName, item.getAcceptedDataTypes());
                    logger.error("Item factory {} suggested it can create items of type {} but returned null",
                            itemFactory, itemTypeName);
