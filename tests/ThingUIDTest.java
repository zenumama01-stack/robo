public class ThingUIDTest {
    public void testThreeSegments() {
        ThingTypeUID thingType = new ThingTypeUID("fake", "type");
        ThingUID subject = new ThingUID(thingType, "thing");
        assertEquals("fake", subject.getBindingId());
        assertEquals("thing", subject.getId());
        assertThat(subject.getAllSegments(), hasSize(3));
        assertEquals("fake:type:thing", subject.getAsString());
    public void testTwoSegments() {
        ThingUID subject = new ThingUID("fake", "thing");
        assertEquals("fake::thing", subject.getAsString());
    public void testGetBridgeIds() {
        ThingUID subject = new ThingUID(thingType, new ThingUID("fake", "something", "bridge"), "thing");
        assertEquals("fake:type:bridge:thing", subject.getAsString());
        assertThat(subject.getBridgeIds(), hasSize(1));
        assertEquals("bridge", subject.getBridgeIds().getFirst());
