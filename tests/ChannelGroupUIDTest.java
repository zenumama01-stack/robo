 * Tests for class {@link ChannelGroupUID}.
public class ChannelGroupUIDTest {
    private static final String BINDING_ID = "binding";
    private static final String THING_TYPE_ID = "thing-type";
    private static final String THING_ID = "thing";
    private static final String GROUP_ID = "group";
    private static final ThingUID THING_UID = new ThingUID(BINDING_ID, THING_TYPE_ID, THING_ID);
    public void testInvalidCharacters() {
        assertThrows(IllegalArgumentException.class, () -> new ChannelGroupUID(THING_UID, "id_with_invalidchar%"));
    public void testNotEnoughNumberOfSegments() {
        assertThrows(IllegalArgumentException.class, () -> new ChannelUID("binding:thing-type:group"));
    public void testChannelGroupUID() {
        ChannelGroupUID channelGroupUID = new ChannelGroupUID(THING_UID, GROUP_ID);
        assertEquals("binding:thing-type:thing:group", channelGroupUID.toString());
        assertEquals(GROUP_ID, channelGroupUID.getId());
        assertEquals(THING_UID, channelGroupUID.getThingUID());
    public void testThingUIDPart() {
        assertThrows(IllegalArgumentException.class, () -> new ChannelGroupUID("binding:thing-type::group"));
        assertThrows(IllegalArgumentException.class, () -> new ChannelGroupUID("binding:thing-type:bridge::group"));
