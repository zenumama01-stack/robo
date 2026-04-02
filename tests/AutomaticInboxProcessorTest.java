import static org.openhab.core.config.discovery.inbox.InboxPredicates.withFlag;
import org.openhab.core.test.storage.VolatileStorageService;
import org.openhab.core.thing.type.ThingTypeBuilder;
 * @author Henning Sudbrock - Added tests for auto-approving inbox entries
public class AutomaticInboxProcessorTest {
    private static final String DEVICE_ID = "deviceId";
    private static final String DEVICE_ID_KEY = "deviceIdKey";
    private static final String OTHER_KEY = "otherKey";
    private static final String OTHER_VALUE = "deviceId";
    private static final String CONFIG_KEY = "configKey";
    private static final String CONFIG_VALUE = "configValue";
    private static final ThingTypeUID THING_TYPE_UID = new ThingTypeUID("test", "test");
    private static final ThingTypeUID THING_TYPE_UID2 = new ThingTypeUID("test2", "test2");
    private static final ThingTypeUID THING_TYPE_UID3 = new ThingTypeUID("test3", "test3");
    private static final ThingUID THING_UID = new ThingUID(THING_TYPE_UID, "test");
    private static final ThingUID THING_UID2 = new ThingUID(THING_TYPE_UID, "test2");
    private static final ThingUID THING_UID3 = new ThingUID(THING_TYPE_UID3, "test3");
    private static final ThingType THING_TYPE = ThingTypeBuilder.instance(THING_TYPE_UID, "label").isListed(true)
            .withRepresentationProperty(DEVICE_ID_KEY).build();
    private static final ThingType THING_TYPE2 = ThingTypeBuilder.instance(THING_TYPE_UID2, "label").isListed(true)
            .withRepresentationProperty(CONFIG_KEY).build();
    private static final ThingType THING_TYPE3 = ThingTypeBuilder.instance(THING_TYPE_UID3, "label").isListed(true)
            .withRepresentationProperty(OTHER_KEY).build();
    private static final Map<String, String> THING_PROPERTIES = Map.of(DEVICE_ID_KEY, DEVICE_ID);
    private static final Map<String, String> OTHER_THING_PROPERTIES = Map.of(OTHER_KEY, OTHER_VALUE);
    private static final Configuration CONFIG = new Configuration(Map.of(CONFIG_KEY, CONFIG_VALUE));
    private @NonNullByDefault({}) AutomaticInboxProcessor automaticInboxProcessor;
    private @NonNullByDefault({}) PersistentInbox inbox;
    private @Mock @NonNullByDefault({}) ThingRegistry thingRegistryMock;
    private @Mock @NonNullByDefault({}) ThingTypeRegistry thingTypeRegistryMock;
    private @Mock @NonNullByDefault({}) Thing thingMock;
    private @Mock @NonNullByDefault({}) Thing thing2Mock;
    private @Mock @NonNullByDefault({}) Thing thing3Mock;
    private @Mock @NonNullByDefault({}) ThingStatusInfoChangedEvent thingStatusInfoChangedEventMock;
    private @Mock @NonNullByDefault({}) ConfigDescriptionRegistry configDescriptionRegistryMock;
    private @Mock @NonNullByDefault({}) ThingHandlerFactory thingHandlerFactoryMock;
    private @Mock @NonNullByDefault({}) ManagedThingProvider thingProviderMock;
        when(thingMock.getConfiguration()).thenReturn(CONFIG);
        when(thingMock.getThingTypeUID()).thenReturn(THING_TYPE_UID);
        when(thingMock.getProperties()).thenReturn(THING_PROPERTIES);
        when(thingMock.getStatus()).thenReturn(ThingStatus.ONLINE);
        when(thingMock.getUID()).thenReturn(THING_UID);
        when(thing2Mock.getConfiguration()).thenReturn(CONFIG);
        when(thing2Mock.getThingTypeUID()).thenReturn(THING_TYPE_UID);
        when(thing2Mock.getProperties()).thenReturn(THING_PROPERTIES);
        when(thing2Mock.getStatus()).thenReturn(ThingStatus.ONLINE);
        when(thing2Mock.getUID()).thenReturn(THING_UID2);
        when(thing3Mock.getConfiguration()).thenReturn(CONFIG);
        when(thing3Mock.getThingTypeUID()).thenReturn(THING_TYPE_UID3);
        when(thing3Mock.getProperties()).thenReturn(OTHER_THING_PROPERTIES);
        when(thing3Mock.getStatus()).thenReturn(ThingStatus.ONLINE);
        when(thing3Mock.getUID()).thenReturn(THING_UID3);
        when(thingRegistryMock.stream()).thenReturn(Stream.empty());
        when(thingTypeRegistryMock.getThingType(THING_TYPE_UID)).thenReturn(THING_TYPE);
        when(thingTypeRegistryMock.getThingType(THING_TYPE_UID2)).thenReturn(THING_TYPE2);
        when(thingTypeRegistryMock.getThingType(THING_TYPE_UID3)).thenReturn(THING_TYPE3);
        when(thingHandlerFactoryMock.supportsThingType(eq(THING_TYPE_UID))).thenReturn(true);
        when(thingHandlerFactoryMock.supportsThingType(eq(THING_TYPE_UID3))).thenReturn(true);
        when(thingHandlerFactoryMock.createThing(any(ThingTypeUID.class), any(Configuration.class), any(ThingUID.class),
                nullable(ThingUID.class)))
                .then(invocation -> ThingBuilder
                        .create((ThingTypeUID) invocation.getArguments()[0], (ThingUID) invocation.getArguments()[2])
                        .withConfiguration((Configuration) invocation.getArguments()[1]).build());
        inbox = new PersistentInbox(new VolatileStorageService(), mock(DiscoveryServiceRegistry.class),
                thingRegistryMock, thingProviderMock, thingTypeRegistryMock, configDescriptionRegistryMock);
        inbox.addThingHandlerFactory(thingHandlerFactoryMock);
        inbox.activate();
        automaticInboxProcessor = new AutomaticInboxProcessor(thingTypeRegistryMock, thingRegistryMock, inbox);
        automaticInboxProcessor.activate(null);
        automaticInboxProcessor.deactivate();
     * This test is just like the test testThingWentOnline in the AutomaticInboxProcessorTest, but in contrast to the
     * above test (where a thing with the same binding ID and the same representation property value went online) here a
     * thing with another binding ID and the same representation property value goes online.
     * In this case, the discovery result should not be ignored, since it has a different thing type.
    public void testThingWithOtherBindingIDButSameRepresentationPropertyWentOnline() {
        // Add discovery result with thing type THING_TYPE_UID and representation property value DEVICE_ID
        inbox.add(DiscoveryResultBuilder.create(THING_UID).withProperty(DEVICE_ID_KEY, DEVICE_ID)
                .withRepresentationProperty(DEVICE_ID_KEY).build());
        // Then there is a discovery result which is NEW
        List<DiscoveryResult> results = inbox.stream().filter(withFlag(DiscoveryResultFlag.NEW)).toList();
        assertThat(results.size(), is(1));
        assertThat(results.getFirst().getThingUID(), is(equalTo(THING_UID)));
        // Now a thing with thing type THING_TYPE_UID3 goes online, with representation property value being also the
        // device id
        when(thingRegistryMock.get(THING_UID3)).thenReturn(thing3Mock);
        when(thingStatusInfoChangedEventMock.getStatusInfo())
                .thenReturn(new ThingStatusInfo(ThingStatus.ONLINE, ThingStatusDetail.NONE, null));
        when(thingStatusInfoChangedEventMock.getThingUID()).thenReturn(THING_UID3);
        automaticInboxProcessor.receive(thingStatusInfoChangedEventMock);
        // Then there should still be the NEW discovery result, but no IGNORED discovery result
        results = inbox.stream().filter(withFlag(DiscoveryResultFlag.NEW)).toList();
        results = inbox.stream().filter(withFlag(DiscoveryResultFlag.IGNORED)).toList();
        assertThat(results.size(), is(0));
    public void testThingWithOtherBindingIDButSameRepresentationPropertyIsDiscovered() {
        // insert thing with thing type THING_TYPE_UID3 and representation property value DEVICE_ID in registry
        when(thingRegistryMock.get(THING_UID)).thenReturn(thingMock);
        when(thingRegistryMock.stream()).thenReturn(Stream.of(thingMock));
        // Add discovery result with thing type THING_TYPE_UID3 and representation property value DEVICE_ID
        inbox.add(DiscoveryResultBuilder.create(THING_UID3).withProperty(DEVICE_ID_KEY, DEVICE_ID)
        // Do NOT ignore this discovery result because it has a different binding ID
        List<DiscoveryResult> results = inbox.stream().filter(withFlag(DiscoveryResultFlag.IGNORED)).toList();
        assertThat(results.getFirst().getThingUID(), is(equalTo(THING_UID3)));
    public void testThingWentOnline() {
        when(thingStatusInfoChangedEventMock.getThingUID()).thenReturn(THING_UID);
    public void testNoDiscoveryResultIfNoRepresentationPropertySet() {
    public void testThingWhenNoRepresentationPropertySet() {
        inbox.add(DiscoveryResultBuilder.create(THING_UID).withProperty(DEVICE_ID_KEY, DEVICE_ID).build());
        when(thingMock.getProperties()).thenReturn(Map.of());
    public void testInboxHasBeenChanged() {
        inbox.stream().map(DiscoveryResult::getThingUID).forEach(t -> inbox.remove(t));
        assertThat(inbox.getAll().size(), is(0));
        inbox.add(DiscoveryResultBuilder.create(THING_UID2).withProperty(DEVICE_ID_KEY, DEVICE_ID)
        assertThat(results.getFirst().getThingUID(), is(equalTo(THING_UID2)));
    public void testThingIsBeingRemoved() {
        inbox.setFlag(THING_UID, DiscoveryResultFlag.IGNORED);
        automaticInboxProcessor.removed(thingMock);
        results = inbox.getAll();
    public void testOneThingOutOfTwoWithSameRepresentationPropertyButDifferentBindingIdIsBeingRemoved() {
        inbox.setFlag(THING_UID3, DiscoveryResultFlag.IGNORED);
        assertThat(results.size(), is(2));
    public void testThingWithConfigWentOnline() {
        inbox.add(DiscoveryResultBuilder.create(THING_UID2).withProperty(OTHER_KEY, OTHER_VALUE)
                .withRepresentationProperty(OTHER_KEY).build());
        when(thingRegistryMock.get(THING_UID2)).thenReturn(thing2Mock);
        when(thingStatusInfoChangedEventMock.getThingUID()).thenReturn(THING_UID2);
    public void testInboxWithConfigHasBeenChanged() {
        when(thingRegistryMock.stream()).thenReturn(Stream.of(thing2Mock));
        inbox.add(DiscoveryResultBuilder.create(THING_UID).withProperty(OTHER_KEY, OTHER_VALUE)
    public void testThingWithConfigIsBeingRemoved() {
        inbox.setFlag(THING_UID2, DiscoveryResultFlag.IGNORED);
        automaticInboxProcessor.removed(thing2Mock);
    public void testAutomaticDiscoveryResultApprovalIfInboxEntriesAddedAfterApprovalPredicatesAreAdded() {
        automaticInboxProcessor.addInboxAutoApprovePredicate(
                discoveryResult -> THING_TYPE_UID.equals(discoveryResult.getThingTypeUID()));
        // The following discovery result is automatically approved, as it has matching thing type UID
        inbox.add(DiscoveryResultBuilder.create(THING_UID).build());
        verify(thingRegistryMock, times(1)).add(argThat(thing -> THING_UID.equals(thing.getUID())));
        // The following discovery result is not automatically approved, as it does not have matching thing type UID
        inbox.add(DiscoveryResultBuilder.create(THING_UID3).build());
        verify(thingRegistryMock, never()).add(argThat(thing -> THING_UID3.equals(thing.getUID())));
    public void testAutomaticDiscoveryResultApprovalIfInboxEntriesExistBeforeApprovalPredicatesAreAdded() {
        inbox.add(DiscoveryResultBuilder.create(THING_UID2).build());
        // Adding this inboxAutoApprovePredicate will auto-approve the first two discovery results as they have matching
        // thing type UID.
        verify(thingRegistryMock, times(1)).add(argThat(thing -> THING_UID2.equals(thing.getUID())));
        // Adding this inboxAutoApprovePredicate will auto-approve the third discovery results as it has matching
                discoveryResult -> THING_TYPE_UID3.equals(discoveryResult.getThingTypeUID()));
        verify(thingRegistryMock, times(1)).add(argThat(thing -> THING_UID3.equals(thing.getUID())));
    public void testAlwaysAutoApproveInboxEntries() {
        // Before setting the always auto approve property, existing inbox results are not approved
        verify(thingRegistryMock, never()).add(argThat(thing -> THING_UID.equals(thing.getUID())));
        // After setting the always auto approve property, all existing inbox results are approved.
        Map<String, Object> configProperties = Map.of(AutomaticInboxProcessor.ALWAYS_AUTO_APPROVE_CONFIG_PROPERTY,
        automaticInboxProcessor.activate(configProperties);
        // Newly added inbox results are also approved.
    @Disabled("Should this test pass? It will fail currently, as RuntimeExceptions are not explicitly caught in AutomaticInboxProcessor#isToBeAutoApproved")
    public void testRogueInboxAutoApprovePredicatesDoNoHarm() {
        automaticInboxProcessor.addInboxAutoApprovePredicate(discoveryResult -> {
            throw new IllegalArgumentException("I am an evil inboxAutoApprovePredicate");
            throw new IllegalArgumentException("I am another evil inboxAutoApprovePredicate");
        // The discovery result is auto-approved in the presence of the evil predicates
