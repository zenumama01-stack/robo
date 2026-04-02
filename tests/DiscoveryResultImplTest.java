 * The {@link DiscoveryResultImplTest} checks if any invalid input parameters
 * and the synchronization of {@link org.openhab.core.config.discovery.DiscoveryResult}s
 * work in a correct way.
public class DiscoveryResultImplTest {
    private static final int DEFAULT_TTL = 60;
    public void testInvalidConstructorForThingType() {
                () -> new DiscoveryResultImpl(null, new ThingUID("aa"), null, null, null, null, DEFAULT_TTL));
    public void testInvalidConstructorForTTL() {
        ThingTypeUID thingTypeUID = new ThingTypeUID("bindingId", "thingType");
        assertThrows(IllegalArgumentException.class, () -> new DiscoveryResultImpl(thingTypeUID,
                new ThingUID(thingTypeUID, "thingId"), null, null, null, null, -2));
    public void testValidConstructor() {
        DiscoveryResultImpl discoveryResult = new DiscoveryResultImpl(thingTypeUID,
                new ThingUID(thingTypeUID, "thingId"), null, null, null, null, DEFAULT_TTL);
        assertEquals("bindingId:thingType", discoveryResult.getThingTypeUID().toString());
        assertEquals("bindingId:thingType:thingId", discoveryResult.getThingUID().toString());
        assertEquals("bindingId", discoveryResult.getBindingId());
        assertEquals("", discoveryResult.getLabel());
        assertEquals(DiscoveryResultFlag.NEW, discoveryResult.getFlag());
        assertNotNull(discoveryResult.getProperties(), "The properties must never be null!");
        assertNull(discoveryResult.getRepresentationProperty());
    public void testInvalidSynchronize() {
        Map<String, Object> discoveryResultSourceMap = Map.of("ipAddress", "127.0.0.1");
                new ThingUID(thingTypeUID, "thingId"), null, discoveryResultSourceMap, "ipAddress", "TARGET",
                DEFAULT_TTL);
        discoveryResult.setFlag(DiscoveryResultFlag.IGNORED);
        discoveryResult.synchronize(null);
        assertEquals("127.0.0.1", discoveryResult.getProperties().get("ipAddress"));
        assertEquals("ipAddress", discoveryResult.getRepresentationProperty());
        assertEquals("TARGET", discoveryResult.getLabel());
        assertEquals(DiscoveryResultFlag.IGNORED, discoveryResult.getFlag());
    public void testIrrelevantSynchronize() {
        DiscoveryResultImpl discoveryResultSource = new DiscoveryResultImpl(thingTypeUID,
                new ThingUID(thingTypeUID, "anotherThingId"), null, null, null, null, DEFAULT_TTL);
        discoveryResult.synchronize(discoveryResultSource);
    public void testSynchronize() {
        Map<String, Object> discoveryResultMap = Map.of( //
                "ipAddress", "192.168.178.1", //
                "macAddress", "AA:BB:CC:DD:EE:FF");
                new ThingUID(thingTypeUID, "thingId"), null, discoveryResultMap, "macAddress", "SOURCE", DEFAULT_TTL);
        discoveryResultSource.setFlag(DiscoveryResultFlag.NEW);
        assertEquals("192.168.178.1", discoveryResult.getProperties().get("ipAddress"));
        assertEquals("AA:BB:CC:DD:EE:FF", discoveryResult.getProperties().get("macAddress"));
        assertEquals("macAddress", discoveryResult.getRepresentationProperty());
        assertEquals("SOURCE", discoveryResult.getLabel());
    public void testThingTypeCompatibility() {
        DiscoveryResultImpl discoveryResult = new DiscoveryResultImpl(null, new ThingUID(thingTypeUID, "thingId"), null,
                null, "nothing", "label", DEFAULT_TTL);
        assertNotNull(discoveryResult.getThingTypeUID());
        assertEquals(discoveryResult.getThingTypeUID(), thingTypeUID);
