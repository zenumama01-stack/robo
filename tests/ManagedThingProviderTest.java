 * The {@link ManagedThingProviderTest} contains tests for the {@link ManagedThingProvider}
public class ManagedThingProviderTest {
    private static final String FIRST_CHANNEL_ID = "firstgroup#channel1";
    private static final ChannelUID FIRST_CHANNEL_UID = new ChannelUID(THING_UID, FIRST_CHANNEL_ID);
    public void testThingImplConversion() {
        Thing thing = ThingBuilder.create(THING_TYPE_UID, THING_UID)
                .withChannel(ChannelBuilder.create(FIRST_CHANNEL_UID, CoreItemFactory.STRING).build()).build();
        ManagedThingProvider managedThingProvider = new ManagedThingProvider(storageService);
        ThingStorageEntity persistableElement = managedThingProvider.toPersistableElement(thing);
        assertThat(persistableElement.isBridge, is(false));
        Thing thing1 = managedThingProvider.toElement("", persistableElement);
        assertThat(thing1, is(thing));
    public void testBridgeImplConversion() {
        Bridge thing = BridgeBuilder.create(THING_TYPE_UID, THING_UID)
        assertThat(persistableElement.isBridge, is(true));
