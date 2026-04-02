package org.openhab.core.thing.internal.profiles;
import org.openhab.core.thing.internal.CommunicationManager;
 * {@link ProfileCallback} implementation.
public class ProfileCallbackImpl implements ProfileCallback {
    private static final String THING_SOURCE = "org.openhab.core.thing";
    private final Logger logger = LoggerFactory.getLogger(ProfileCallbackImpl.class);
    private final ItemChannelLink link;
    private final Function<ThingUID, @Nullable Thing> thingProvider;
    private final Function<String, @Nullable Item> itemProvider;
    private final AcceptedTypeConverter acceptedTypeConverter;
    public ProfileCallbackImpl(EventPublisher eventPublisher, SafeCaller safeCaller,
            ItemStateConverter itemStateConverter, ItemChannelLink link,
            Function<ThingUID, @Nullable Thing> thingProvider, Function<String, @Nullable Item> itemProvider,
            AcceptedTypeConverter acceptedTypeConverter) {
        this.acceptedTypeConverter = acceptedTypeConverter;
    public ItemChannelLink getItemChannelLink() {
    public void handleCommand(Command command) {
        Thing thing = thingProvider.apply(link.getLinkedUID().getThingUID());
            final ThingHandler handler = thing.getHandler();
                    logger.debug("Delegating command '{}' for item '{}' to handler for channel '{}'", command,
                            link.getItemName(), link.getLinkedUID());
                    Command convertedCommand = acceptedTypeConverter.toAcceptedCommand(command, channel,
                            itemProvider.apply(link.getItemName()));
                    if (convertedCommand != null) {
                        safeCaller.create(handler, ThingHandler.class)
                                .withTimeout(CommunicationManager.THINGHANDLER_EVENT_TIMEOUT).onTimeout(() -> {
                                    logger.warn("Handler for thing '{}' takes more than {}ms for handling a command",
                                            handler.getThing().getUID(),
                                            CommunicationManager.THINGHANDLER_EVENT_TIMEOUT);
                                }).build().handleCommand(link.getLinkedUID(), command);
                                "Not delegating command '{}' for item '{}' to handler for channel '{}', "
                                        + "because it was not possible to convert it to an accepted type).",
                                command, link.getItemName(), link.getLinkedUID());
                    logger.debug("Not delegating command '{}' for item '{}' to handler for channel '{}', "
                            + "because handler is not initialized (thing must be in status UNKNOWN, ONLINE or OFFLINE but was {}).",
                            command, link.getItemName(), link.getLinkedUID(), thing.getStatus());
                logger.warn("Cannot delegate command '{}' for item '{}' to handler for channel '{}', "
                        + "because no handler is assigned. Maybe the binding is not installed or not "
                        + "propertly initialized.", command, link.getItemName(), link.getLinkedUID());
                    "Cannot delegate command '{}' for item '{}' to handler for channel '{}', "
                            + "because no thing with the UID '{}' could be found.",
                    command, link.getItemName(), link.getLinkedUID(), link.getLinkedUID().getThingUID());
    public void sendCommand(Command command, @Nullable String source) {
        eventPublisher.post(ItemEventFactory.createCommandEvent(link.getItemName(), command, buildSource(source)));
    public void sendUpdate(State state, @Nullable String source) {
        Item item = itemProvider.apply(link.getItemName());
            logger.warn("Cannot post update event '{}' for item '{}', because no item could be found.", state,
                    link.getItemName());
        State acceptedState;
        if (state instanceof StringType && !(item instanceof StringItem)) {
            acceptedState = TypeParser.parseState(item.getAcceptedDataTypes(), state.toString());
            if (acceptedState == null) {
                acceptedState = state;
            acceptedState = itemStateConverter.convertToAcceptedState(state, item);
        eventPublisher.post(ItemEventFactory.createStateEvent(link.getItemName(), acceptedState, buildSource(source)));
    public void sendTimeSeries(TimeSeries timeSeries, @Nullable String source) {
            logger.warn("Cannot send time series event '{}' for item '{}', because no item could be found.", timeSeries,
                .post(ItemEventFactory.createTimeSeriesEvent(link.getItemName(), timeSeries, buildSource(source)));
    public interface AcceptedTypeConverter {
        Command toAcceptedCommand(Command originalType, @Nullable Channel channel, @Nullable Item item);
        return AbstractEvent.buildDelegatedSource(source, THING_SOURCE, link.getLinkedUID().toString());
