 * Tests for {@link BaseDynamicCommandDescriptionProvider}.
class BaseDynamicCommandDescriptionProviderTest {
    private static final ThingTypeUID THING_TYPE_UID = new ThingTypeUID("binding:type");
    private static final ChannelUID CHANNEL_UID = new ChannelUID(THING_UID, "channel");
    @Mock
    EventPublisher eventPublisherMock;
    ItemChannelLinkRegistry itemChannelLinkRegistryMock;
    class TestDynamicCommandDescriptionProvider extends BaseDynamicCommandDescriptionProvider {
        public TestDynamicCommandDescriptionProvider() {
            this.bundleContext = mock(BundleContext.class);
            this.eventPublisher = eventPublisherMock;
            this.itemChannelLinkRegistry = itemChannelLinkRegistryMock;
    private @NonNullByDefault({}) TestDynamicCommandDescriptionProvider subject;
        when(itemChannelLinkRegistryMock.getLinkedItemNames(CHANNEL_UID)).thenReturn(Set.of("item1", "item2"));
        subject = new TestDynamicCommandDescriptionProvider();
    public void setCommandOptionsPublishesEvent() {
        subject.setCommandOptions(CHANNEL_UID, List.of(new CommandOption("reboot", "Reboot")));
        ArgumentCaptor<Event> capture = ArgumentCaptor.forClass(Event.class);
        verify(eventPublisherMock, times(1)).post(capture.capture());
        Event event = capture.getValue();
        assertInstanceOf(ChannelDescriptionChangedEvent.class, event);
        ChannelDescriptionChangedEvent cdce = (ChannelDescriptionChangedEvent) event;
        assertEquals(CommonChannelDescriptionField.COMMAND_OPTIONS, cdce.getField());
        // check the event is not published again
