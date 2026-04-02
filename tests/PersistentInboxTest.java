import org.openhab.core.config.discovery.inbox.events.InboxAddedEvent;
import org.openhab.core.config.discovery.inbox.events.InboxUpdatedEvent;
 * @author Laurent Garnier - Added tests testApproveWithThingId and testApproveWithInvalidThingId
public class PersistentInboxTest {
    private static final String THING_OTHER_ID = "other";
    private static final ThingUID THING_OTHER_UID = new ThingUID(THING_TYPE_UID, THING_OTHER_ID);
    private @Nullable Thing lastAddedThing;
    private @Mock @NonNullByDefault({}) StorageService storageServiceMock;
    private @Mock @NonNullByDefault({}) Storage<Object> storageMock;
    private @Mock @NonNullByDefault({}) ThingType thingTypeMock;
        when(thingTypeMock.getConfigDescriptionURI()).thenReturn(null);
        when(thingTypeRegistryMock.getThingType(any())).thenReturn(thingTypeMock);
        when(storageServiceMock.getStorage(any(String.class), any(ClassLoader.class))).thenReturn(storageMock);
        doAnswer(invocation -> lastAddedThing = (Thing) invocation.getArguments()[0]).when(thingRegistryMock)
                .add(any(Thing.class));
        when(thingHandlerFactoryMock.createThing(eq(THING_TYPE_UID), any(Configuration.class), eq(THING_UID), any()))
                .then(invocation -> ThingBuilder.create(THING_TYPE_UID, "test")
        when(thingHandlerFactoryMock.createThing(eq(THING_TYPE_UID), any(Configuration.class), eq(THING_OTHER_UID),
                any()))
                .then(invocation -> ThingBuilder.create(THING_TYPE_UID, THING_OTHER_ID)
        inbox = new PersistentInbox(storageServiceMock, mock(DiscoveryServiceRegistry.class), thingRegistryMock,
                thingProviderMock, thingTypeRegistryMock, configDescriptionRegistryMock);
    public void testConfigUpdateNormalizationWithConfigDescription() throws URISyntaxException {
        Map<String, Object> props = Map.of("foo", "1");
        Configuration config = new Configuration(props);
        Thing thing = ThingBuilder.create(THING_TYPE_UID, THING_UID).withConfiguration(config).build();
        configureConfigDescriptionRegistryMock("foo", Type.TEXT);
        when(thingRegistryMock.get(eq(THING_UID))).thenReturn(thing);
        when(thingProviderMock.get(eq(THING_UID))).thenReturn(thing);
        assertInstanceOf(String.class, thing.getConfiguration().get("foo"));
        inbox.add(DiscoveryResultBuilder.create(THING_UID).withProperty("foo", 3).build());
        // thing updated if managed
        assertEquals("3", thing.getConfiguration().get("foo"));
    public void testConfigUpdateNormalizationWithConfigDescriptionUnanagedThing() {
        Configuration config = new Configuration(Map.of("foo", "1"));
        // thing not updated if unmanaged
        assertEquals("1", thing.getConfiguration().get("foo"));
    public void testApproveNormalization() {
        DiscoveryResult result = DiscoveryResultBuilder.create(THING_UID).withProperty("foo", 3).build();
        when(storageMock.getValues()).thenReturn(List.of(result));
        inbox.approve(THING_UID, "Test", null);
        Thing lastAddedThing = this.lastAddedThing;
        assertNotNull(lastAddedThing);
        assertEquals(THING_UID, lastAddedThing.getUID());
        assertInstanceOf(String.class, lastAddedThing.getConfiguration().get("foo"));
        assertEquals("3", lastAddedThing.getConfiguration().get("foo"));
    public void testApproveWithThingId() {
        inbox.approve(THING_UID, "Test", THING_OTHER_ID);
        assertEquals(THING_OTHER_UID, lastAddedThing.getUID());
    public void testApproveWithInvalidThingId() {
        Exception exception = assertThrows(IllegalArgumentException.class, () -> {
            inbox.approve(THING_UID, "Test", "invalid:id");
        assertEquals("New Thing ID invalid:id must not contain multiple segments", exception.getMessage());
        exception = assertThrows(IllegalArgumentException.class, () -> {
            inbox.approve(THING_UID, "Test", "invalid$id");
        assertEquals("Invalid thing UID test:test:invalid$id", exception.getMessage());
    public void testEmittedAddedResultIsReadFromStorage() {
        EventPublisher eventPublisher = mock(EventPublisher.class);
        inbox.setEventPublisher(eventPublisher);
        when(storageMock.get(THING_UID.toString())) //
                .thenReturn(null) //
                .thenReturn(DiscoveryResultBuilder.create(THING_UID).withProperty("foo", "bar").build());
        inbox.add(result);
        // 1st call checks existence of the result in the storage (returns null)
        // 2nd call retrieves the stored instance before the event gets emitted
        // (modified due to storage mock configuration)
        verify(storageMock, times(2)).get(THING_UID.toString());
        ArgumentCaptor<InboxAddedEvent> eventCaptor = ArgumentCaptor.forClass(InboxAddedEvent.class);
        verify(eventPublisher).post(eventCaptor.capture());
        assertThat(eventCaptor.getValue().getDiscoveryResult().properties, hasEntry("foo", "bar"));
    public void testEmittedUpdatedResultIsReadFromStorage() {
                .thenReturn(result) //
        // 1st call checks existence of the result in the storage (returns the original result)
        ArgumentCaptor<InboxUpdatedEvent> eventCaptor = ArgumentCaptor.forClass(InboxUpdatedEvent.class);
    private void configureConfigDescriptionRegistryMock(String paramName, Type type) {
        URI configDescriptionURI = URI.create("thing-type:test:test");
        ThingType thingType = ThingTypeBuilder.instance(THING_TYPE_UID, "Test")
                .withConfigDescriptionURI(configDescriptionURI).build();
        ConfigDescription configDesc = ConfigDescriptionBuilder.create(configDescriptionURI)
                .withParameter(ConfigDescriptionParameterBuilder.create(paramName, type).build()).build();
        when(thingTypeRegistryMock.getThingType(THING_TYPE_UID)).thenReturn(thingType);
        when(configDescriptionRegistryMock.getConfigDescription(eq(configDescriptionURI))).thenReturn(configDesc);
