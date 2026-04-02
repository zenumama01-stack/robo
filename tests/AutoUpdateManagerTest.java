 * @author Kai Kreuzer - added tests with multiple links
public class AutoUpdateManagerTest {
    private static final ThingUID THING_UID_ONLINE = new ThingUID("test::mock-online");
    private static final ThingUID THING_UID_OFFLINE = new ThingUID("test::mock-offline");
    private static final ThingUID THING_UID_HANDLER_MISSING = new ThingUID("test::handlerMissing");
    private static final ChannelUID CHANNEL_UID_ONLINE_1 = new ChannelUID(THING_UID_ONLINE, "channel1");
    private static final ChannelUID CHANNEL_UID_ONLINE_2 = new ChannelUID(THING_UID_ONLINE, "channel2");
    private static final ChannelUID CHANNEL_UID_OFFLINE_1 = new ChannelUID(THING_UID_OFFLINE, "channel1");
    private static final ChannelUID CHANNEL_UID_ONLINE_GONE = new ChannelUID(THING_UID_ONLINE, "gone");
    private static final ChannelUID CHANNEL_UID_HANDLER_MISSING = new ChannelUID(THING_UID_HANDLER_MISSING, "channel1");
    private @NonNullByDefault({}) ItemCommandEvent event;
    private @NonNullByDefault({}) ItemCommandEvent groupEvent;
    private @NonNullByDefault({}) GroupItem groupItem;
    private @Mock @NonNullByDefault({}) EventPublisher eventPublisherMock;
    private @Mock @NonNullByDefault({}) ItemChannelLinkRegistry iclRegistryMock;
    private @Mock @NonNullByDefault({}) Thing onlineThingMock;
    private @Mock @NonNullByDefault({}) Thing offlineThingMock;
    private @Mock @NonNullByDefault({}) Thing thingMissingHandlerMock;
    private @Mock @NonNullByDefault({}) ThingHandler handlerMock;
    private final Set<ItemChannelLink> links = new HashSet<>();
    private @NonNullByDefault({}) AutoUpdateManager aum;
    private final Map<ChannelUID, AutoUpdatePolicy> policies = new HashMap<>();
        event = ItemEventFactory.createCommandEvent(ITEM_NAME, new StringType("AFTER"));
        item = new StringItem(ITEM_NAME);
        item.setState(new StringType("BEFORE"));
        groupEvent = ItemEventFactory.createCommandEvent("groupTest", new StringType("AFTER"));
        groupItem = new GroupItem("groupTest", new StringItem("test"));
        groupItem.setState(new StringType("BEFORE"));
        when(iclRegistryMock.getLinks(eq(ITEM_NAME))).then(answer -> links);
        when(thingRegistryMock.get(eq(THING_UID_ONLINE))).thenReturn(onlineThingMock);
        when(thingRegistryMock.get(eq(THING_UID_OFFLINE))).thenReturn(offlineThingMock);
        when(thingRegistryMock.get(eq(THING_UID_HANDLER_MISSING))).thenReturn(thingMissingHandlerMock);
        when(onlineThingMock.getHandler()).thenReturn(handlerMock);
        when(onlineThingMock.getStatus()).thenReturn(ThingStatus.ONLINE);
        when(onlineThingMock.getChannel(eq(CHANNEL_UID_ONLINE_1)))
                .thenAnswer(answer -> ChannelBuilder.create(CHANNEL_UID_ONLINE_1, CoreItemFactory.STRING)
                        .withAutoUpdatePolicy(policies.get(CHANNEL_UID_ONLINE_1)).build());
        when(onlineThingMock.getChannel(eq(CHANNEL_UID_ONLINE_2)))
                .thenAnswer(answer -> ChannelBuilder.create(CHANNEL_UID_ONLINE_2, CoreItemFactory.STRING)
                        .withAutoUpdatePolicy(policies.get(CHANNEL_UID_ONLINE_2)).build());
        when(offlineThingMock.getHandler()).thenReturn(handlerMock);
        when(offlineThingMock.getStatus()).thenReturn(ThingStatus.OFFLINE);
        when(offlineThingMock.getChannel(eq(CHANNEL_UID_OFFLINE_1)))
                .thenAnswer(answer -> ChannelBuilder.create(CHANNEL_UID_OFFLINE_1, CoreItemFactory.STRING)
                        .withAutoUpdatePolicy(policies.get(CHANNEL_UID_OFFLINE_1)).build());
        aum = new AutoUpdateManager(Map.of(), channelTypeRegistryMock, eventPublisherMock, iclRegistryMock,
                metadataRegistryMock, thingRegistryMock);
    private void assertStateEvent(String expectedContent, String extectedSource) {
        verify(eventPublisherMock, atLeastOnce()).post(eventCaptor.capture());
        Event event = eventCaptor.getAllValues().stream().filter(e -> e instanceof ItemStateEvent).findFirst().get();
        assertEquals(expectedContent, ((ItemStateEvent) event).getItemState().toFullString());
        assertEquals(extectedSource, event.getSource());
        assertNothingHappened();
    private void assertPredictionEvent(String expectedContent, @Nullable String expectedSource) {
        Event event = eventCaptor.getAllValues().stream().filter(e -> e instanceof ItemStatePredictedEvent).findFirst()
        assertEquals(expectedContent, ((ItemStatePredictedEvent) event).getPredictedState().toFullString());
        assertEquals(expectedSource, event.getSource());
    private void assertChangeStateTo() {
        assertPredictionEvent("AFTER", null);
    private void assertKeepCurrentState() {
        assertPredictionEvent("BEFORE", null);
    private void assertNothingHappened() {
        verifyNoMoreInteractions(eventPublisherMock);
    private void setAutoUpdatePolicy(ChannelUID channelUID, AutoUpdatePolicy policy) {
        policies.put(channelUID, policy);
    public void testAutoUpdateNoLink() {
        aum.receiveCommand(event, item);
        assertStateEvent("AFTER", AutoUpdateManager.EVENT_SOURCE);
    public void testAutoUpdateNoPolicy() {
        links.add(new ItemChannelLink(ITEM_NAME, CHANNEL_UID_ONLINE_1));
        assertChangeStateTo();
    public void testAutoUpdateNoPolicyThingOFFLINE() {
        links.add(new ItemChannelLink(ITEM_NAME, CHANNEL_UID_OFFLINE_1));
        assertKeepCurrentState();
    public void testAutoUpdateNoPolicyThingOFFLINEandThingONLINE() {
    public void testAutoUpdateNoPolicyThingONLINEandThingOFFLINE() {
    public void testAutoUpdateNoPolicyNoHandler() {
        links.add(new ItemChannelLink(ITEM_NAME, CHANNEL_UID_HANDLER_MISSING));
    public void testAutoUpdateNoPolicyNoThing() {
        links.add(new ItemChannelLink(ITEM_NAME, new ChannelUID(new ThingUID("test::missing"), "gone")));
    public void testAutoUpdateNoPolicyNoChannel() {
        links.add(new ItemChannelLink(ITEM_NAME, CHANNEL_UID_ONLINE_GONE));
    public void testAutoUpdatePolicyVETOThingONLINE() {
        setAutoUpdatePolicy(CHANNEL_UID_ONLINE_1, AutoUpdatePolicy.VETO);
    public void testAutoUpdatePolicyRECOMMEND() {
        setAutoUpdatePolicy(CHANNEL_UID_ONLINE_1, AutoUpdatePolicy.RECOMMEND);
    public void testAutoUpdatePolicyVETObeatsDEFAULT() {
        links.add(new ItemChannelLink(ITEM_NAME, CHANNEL_UID_ONLINE_2));
        setAutoUpdatePolicy(CHANNEL_UID_ONLINE_2, AutoUpdatePolicy.DEFAULT);
    public void testAutoUpdatePolicyVETObeatsRECOMMEND() {
        setAutoUpdatePolicy(CHANNEL_UID_ONLINE_2, AutoUpdatePolicy.RECOMMEND);
    public void testAutoUpdatePolicyDEFAULTbeatsRECOMMEND() {
        setAutoUpdatePolicy(CHANNEL_UID_ONLINE_1, AutoUpdatePolicy.DEFAULT);
    public void testAutoUpdateErrorInvalidatesVETO() {
        setAutoUpdatePolicy(CHANNEL_UID_ONLINE_GONE, AutoUpdatePolicy.VETO);
    public void testAutoUpdateErrorInvalidatesVETO2() {
    public void testAutoUpdateErrorInvalidatesDEFAULT() {
        setAutoUpdatePolicy(CHANNEL_UID_ONLINE_GONE, AutoUpdatePolicy.DEFAULT);
    public void testAutoUpdateMultipleErrors() {
    public void testAutoUpdateDisabled() {
        aum.modified(Map.of(AutoUpdateManager.PROPERTY_ENABLED, "false"));
    public void testAutoUpdateSendOptimisticUpdates() {
        aum.modified(Map.of(AutoUpdateManager.PROPERTY_SEND_OPTIMISTIC_UPDATES, "true"));
        assertStateEvent("AFTER", AutoUpdateManager.EVENT_SOURCE_OPTIMISTIC); // no?
    public void testAutoUpdateDisabledForGroupItems() {
        aum.receiveCommand(groupEvent, groupItem);
        groupItem.removeMember(item);
 * Tests for {@link AutoUpdateManager}.
public class AutoUpdateManagerTest extends JavaTest {
    private static final ChannelTypeUID CHANNEL_TYPE_UID = new ChannelTypeUID("binding:channelType");
    private static final ChannelUID CHANNEL_UID = new ChannelUID("binding:thingtype1:thing1:channel1");
    private static final String ITEM_NAME = "TestItem";
    private @NonNullByDefault({}) ChannelTypeRegistry channelTypeRegistry;
    private @NonNullByDefault({}) ThingRegistry thingRegistry;
    private @NonNullByDefault({}) MetadataRegistry metadataRegistry;
    private @NonNullByDefault({}) EventPublisher eventPublisher;
    private @NonNullByDefault({}) ItemChannelLinkRegistry itemChannelLinkRegistry;
    private @NonNullByDefault({}) AutoUpdateManager autoUpdateManager;
        channelTypeRegistry = mock(ChannelTypeRegistry.class);
        eventPublisher = mock(EventPublisher.class);
        itemChannelLinkRegistry = mock(ItemChannelLinkRegistry.class);
        assertNotNull(itemChannelLinkRegistry);
        thingRegistry = mock(ThingRegistry.class);
        thing = mock(Thing.class);
        metadataRegistry = mock(MetadataRegistry.class);
        Channel channel = ChannelBuilder.create(CHANNEL_UID).withType(CHANNEL_TYPE_UID).build();
        autoUpdateManager = new AutoUpdateManager(Map.of(), channelTypeRegistry, eventPublisher,
                itemChannelLinkRegistry, metadataRegistry, thingRegistry);
        item = mock(Item.class);
        when(item.getName()).thenReturn(ITEM_NAME);
        when(item.getAcceptedDataTypes()).thenReturn(List.of(OnOffType.class));
        when(itemChannelLinkRegistry.getLinks(any(String.class)))
                .thenReturn(Set.of(new ItemChannelLink(ITEM_NAME, CHANNEL_UID)));
        when(thingRegistry.get(any(ThingUID.class))).thenReturn(thing);
        when(thing.getStatus()).thenReturn(ThingStatus.ONLINE);
        when(thing.getHandler()).thenReturn(mock(ThingHandler.class));
        when(thing.getChannel(any(ChannelUID.class))).thenReturn(channel);
    public void testAutoUpdateVetoFromChannelType() {
        when(channelTypeRegistry.getChannelType(any(ChannelTypeUID.class)))
                .thenReturn(ChannelTypeBuilder.state(CHANNEL_TYPE_UID, "label", CoreItemFactory.SWITCH)
                        .withAutoUpdatePolicy(AutoUpdatePolicy.VETO).build());
        autoUpdateManager.receiveCommand(ItemEventFactory.createCommandEvent(ITEM_NAME, OnOffType.ON), item);
        // No event should have been sent
        verify(eventPublisher, never()).post(any(Event.class));
