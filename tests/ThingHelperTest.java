public class ThingHelperTest {
    private static final ThingUID THING_UID = new ThingUID(THING_TYPE_UID, "thingId");
    public void twoTechnicalEqualThingInstancesAreDetectedAsEqual() {
        Thing thingA = ThingBuilder.create(THING_TYPE_UID, THING_UID)
                        ChannelBuilder.create(new ChannelUID("binding:type:thingId:channel1"), "itemType").build(),
                        ChannelBuilder.create(new ChannelUID("binding:type:thingId:channel2"), "itemType").build())
                .withConfiguration(new Configuration()).build();
        thingA.getConfiguration().put("prop1", "value1");
        thingA.getConfiguration().put("prop2", "value2");
        assertTrue(ThingHelper.equals(thingA, thingA));
        Thing thingB = ThingBuilder.create(THING_TYPE_UID, THING_UID)
                        ChannelBuilder.create(new ChannelUID("binding:type:thingId:channel2"), "itemType").build(),
                        ChannelBuilder.create(new ChannelUID("binding:type:thingId:channel1"), "itemType").build())
        thingB.getConfiguration().put("prop2", "value2");
        thingB.getConfiguration().put("prop1", "value1");
        assertTrue(ThingHelper.equals(thingA, thingB));
    public void twoThingsAreDifferentAfterPropertiesWereModified() {
        thingB.getConfiguration().put("prop3", "value3");
        assertFalse(ThingHelper.equals(thingA, thingB));
    public void twoThingsAreDifferentAfterChannelsWereModified() {
        Thing thingA = ThingBuilder.create(THING_TYPE_UID, THING_UID).withConfiguration(new Configuration()).build();
        Thing thingB = ThingBuilder.create(THING_TYPE_UID, THING_UID).withConfiguration(new Configuration()).build();
        ((ThingImpl) thingB).setChannels(
                List.of(ChannelBuilder.create(new ChannelUID("binding:type:thingId:channel3"), "itemType3").build()));
    public void twoThingsAreDifferentAfterLabelWasModified() {
        Thing thingA = ThingBuilder.create(THING_TYPE_UID, THING_UID).withConfiguration(new Configuration())
                .withLabel("foo").build();
        Thing thingB = ThingBuilder.create(THING_TYPE_UID, THING_UID).withConfiguration(new Configuration())
        thingB.setLabel("bar");
    public void twoThingsAreDifferentAfterLocationWasModified() {
                .withLocation("foo").build();
        thingB.setLocation("bar");
    public void assertThatNoDuplicateChannelsCanBeAdded() {
        ThingTypeUID thingTypeUID = new ThingTypeUID("test", "test");
        ThingUID thingUID = new ThingUID(thingTypeUID, "test");
        Thing thing = ThingBuilder.create(thingTypeUID, thingUID)
                .withChannels(ChannelBuilder.create(new ChannelUID(thingUID, "channel1"), "").build(),
                        ChannelBuilder.create(new ChannelUID(thingUID, "channel2"), "").build())
                () -> ThingHelper.addChannelsToThing(thing,
                        List.of(ChannelBuilder.create(new ChannelUID(thingUID, "channel2"), "").build(),
                                ChannelBuilder.create(new ChannelUID(thingUID, "channel3"), "").build())));
    public void asserThatChannelsWithDifferentConfigurationAreDetectedAsDifferent() {
                .withChannels(ChannelBuilder.create(new ChannelUID("binding:type:thingId:channel1"), "itemType")
                        .withConfiguration(new Configuration(Map.of("key", "v1"))).build())
                        .withConfiguration(new Configuration(Map.of("key", "v2"))).build())
