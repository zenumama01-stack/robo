 * The {@link ChannelLinkNotifier} notifies initialized thing handlers of channels being linked or unlinked.
public class ChannelLinkNotifier implements RegistryChangeListener<ItemChannelLink> {
    private final Logger logger = LoggerFactory.getLogger(ChannelLinkNotifier.class);
    public ChannelLinkNotifier(@Reference ItemChannelLinkRegistry itemChannelLinkRegistry,
            @Reference ThingRegistry thingRegistry) {
        itemChannelLinkRegistry.addRegistryChangeListener(this);
        // registry does not dispatch notifications about existing links to listeners
        itemChannelLinkRegistry.stream().map(ItemChannelLink::getLinkedUID).distinct()
                .forEach(channelUID -> call(channelUID.getThingUID(), handler -> handler.channelLinked(channelUID),
                        "channelLinked"));
        itemChannelLinkRegistry.removeRegistryChangeListener(this);
    public void added(ItemChannelLink element) {
        ChannelUID channelUID = element.getLinkedUID();
        ThingUID thingUID = channelUID.getThingUID();
        call(thingUID, handler -> handler.channelLinked(channelUID), "channelLinked");
    public void removed(ItemChannelLink element) {
        if (!itemChannelLinkRegistry.isLinked(channelUID)) {
            call(thingUID, handler -> handler.channelUnlinked(channelUID), "channelUnlinked");
    public void updated(ItemChannelLink oldElement, ItemChannelLink element) {
        removed(oldElement);
    private void call(ThingUID thingUID, Consumer<ThingHandler> consumer, String method) {
            if (handler != null && ThingHandlerHelper.isHandlerInitialized(handler)) {
                consumer.accept(handler);
                logger.debug("Skipping notification to thing {} handler '{}' method call, as it is not initialized",
                        thingUID, method);
