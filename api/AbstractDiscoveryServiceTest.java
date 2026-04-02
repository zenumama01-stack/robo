import static org.hamcrest.collection.IsMapContaining.hasEntry;
import org.openhab.core.util.SameThreadExecutorService;
 * Tests the {@link DiscoveryResultBuilder}.
 * @author Laurent Garnier - Added test for discovery with an input parameter
public class AbstractDiscoveryServiceTest implements DiscoveryListener {
    private static final String BINDING_ID = "bindingId";
    private static final ThingUID BRIDGE_UID = new ThingUID(new ThingTypeUID(BINDING_ID, "bridgeTypeId"), "bridgeId");
    private static final ThingTypeUID THING_TYPE_UID = new ThingTypeUID(BINDING_ID, "thingTypeId");
    private static final ThingUID THING_UID1 = new ThingUID(THING_TYPE_UID, BRIDGE_UID, "thingId1");
    private static final ThingUID THING_UID2 = new ThingUID(THING_TYPE_UID, "thingId2");
    private static final ThingUID THING_UID3 = new ThingUID(THING_TYPE_UID, BRIDGE_UID, "thingId3");
    private static final ThingUID THING_UID4 = new ThingUID(THING_TYPE_UID, "thingId4");
    private static final ThingUID THING_UID5 = new ThingUID(THING_TYPE_UID, BRIDGE_UID, "thingId5");
    private final Map<String, Object> properties = Map.of(KEY1, VALUE1, KEY2, VALUE2);
    private static final String DISCOVERY_THING2_INFERED_KEY = "discovery." + THING_UID2.getAsString().replace(":", ".")
            + ".label";
    private static final String DISCOVERY_THING4_INFERED_KEY = "discovery." + THING_UID4.getAsString().replace(":", ".")
    private static final String DISCOVERY_LABEL = "Result Test";
    private static final String DISCOVERY_LABEL_KEY1 = "@text/test";
    private static final String DISCOVERY_LABEL_KEY2 = "@text/test2 [ \"50\", \"number\" ]";
    private static final String DISCOVERY_LABEL_CODE = "Result Test with pairing code";
    private static final String PROPERTY_LABEL1 = "Label from property (text key)";
    private static final String PROPERTY_LABEL2 = "Label from property (infered key)";
    private static final String PROPERTY_LABEL3 = "Label from property (parameters 50 and number)";
    private static final String PAIRING_CODE_LABEL = "Pairing Code";
    private static final String PAIRING_CODE_DESCR = "The pairing code";
    private TranslationProvider i18nProvider = new TranslationProvider() {
            if (Locale.ENGLISH.equals(locale)) {
                if ("test".equals(key)) {
                    return PROPERTY_LABEL1;
                } else if ("test2".equals(key) && arguments != null && arguments.length == 2
                        && "50".equals(arguments[0]) && "number".equals(arguments[1])) {
                    return PROPERTY_LABEL3;
                } else if (DISCOVERY_THING2_INFERED_KEY.equals(key) || DISCOVERY_THING4_INFERED_KEY.equals(key)) {
                    return PROPERTY_LABEL2;
            return defaultText;
    private LocaleProvider localeProvider = new LocaleProvider() {
        public Locale getLocale() {
            return Locale.ENGLISH;
    class TestDiscoveryService extends AbstractDiscoveryService {
        int discoveryResults;
        public TestDiscoveryService(TranslationProvider i18nProvider, LocaleProvider localeProvider)
            super(new SameThreadExecutorService(), Set.of(THING_TYPE_UID), 1, false, null, null);
            // Discovered thing 1 has a hard coded label and no key based on its thing UID defined in the properties
            // file => the hard coded label should be considered
            DiscoveryResult discoveryResult = DiscoveryResultBuilder.create(THING_UID1).withThingType(THING_TYPE_UID)
                    .withProperties(properties).withRepresentationProperty(KEY1).withBridge(BRIDGE_UID)
                    .withLabel(DISCOVERY_LABEL).build();
            discoveryResults++;
            thingDiscovered(discoveryResult);
            // Discovered thing 2 has a hard coded label but with a key based on its thing UID defined in the properties
            // file => the value from the properties file should be considered
            discoveryResult = DiscoveryResultBuilder.create(THING_UID2).withThingType(THING_TYPE_UID)
                    .withProperties(properties).withRepresentationProperty(KEY1).withLabel(DISCOVERY_LABEL).build();
            // Discovered thing 3 has a label referencing an entry in the properties file and no key based on its thing
            // UID defined in the properties file => the value from the properties file should be considered
            discoveryResult = DiscoveryResultBuilder.create(THING_UID3).withThingType(THING_TYPE_UID)
                    .withLabel(DISCOVERY_LABEL_KEY1).build();
            // Discovered thing 4 has a label referencing an entry in the properties file and a key based on its thing
            // UID defined in the properties file => the value from the properties file (the one referenced by the
            // label) should be considered
            discoveryResult = DiscoveryResultBuilder.create(THING_UID4).withThingType(THING_TYPE_UID)
                    .withProperties(properties).withRepresentationProperty(KEY1).withLabel(DISCOVERY_LABEL_KEY2)
            discoveryResults--;
            thingRemoved(THING_UID3);
    class TestDiscoveryServiceWithRequiredCode extends AbstractDiscoveryService {
        public TestDiscoveryServiceWithRequiredCode(TranslationProvider i18nProvider, LocaleProvider localeProvider)
            super(new SameThreadExecutorService(), Set.of(THING_TYPE_UID), 1, false, PAIRING_CODE_LABEL,
                    PAIRING_CODE_DESCR);
            DiscoveryResult discoveryResult = DiscoveryResultBuilder.create(THING_UID5).withThingType(THING_TYPE_UID)
                    .withLabel(DISCOVERY_LABEL_CODE).build();
        assertThat(result.getThingTypeUID(), is(THING_TYPE_UID));
        assertThat(result.getBindingId(), is(BINDING_ID));
        assertThat(result.getProperties().size(), is(2));
        assertThat(result.getProperties(), hasEntry(KEY1, VALUE1));
        assertThat(result.getProperties(), hasEntry(KEY2, VALUE2));
        assertThat(result.getRepresentationProperty(), is(KEY1));
        assertThat(result.getTimeToLive(), is(DiscoveryResult.TTL_UNLIMITED));
        if (THING_UID1.equals(result.getThingUID())) {
            assertThat(result.getBridgeUID(), is(BRIDGE_UID));
            assertThat(result.getLabel(), is(DISCOVERY_LABEL));
        } else if (THING_UID2.equals(result.getThingUID())) {
            assertNull(result.getBridgeUID());
            assertThat(result.getLabel(), is(PROPERTY_LABEL2));
        } else if (THING_UID3.equals(result.getThingUID())) {
            assertThat(result.getLabel(), is(PROPERTY_LABEL1));
        } else if (THING_UID4.equals(result.getThingUID())) {
            assertThat(result.getLabel(), is(PROPERTY_LABEL3));
        } else if (THING_UID5.equals(result.getThingUID())) {
            assertThat(result.getLabel(), is(DISCOVERY_LABEL_CODE));
        assertThat(thingUID, is(THING_UID3));
    public void testDiscoveryResults() {
        TestDiscoveryService discoveryService = new TestDiscoveryService(i18nProvider, localeProvider);
        assertFalse(discoveryService.isScanInputSupported());
        assertNull(discoveryService.getScanInputLabel());
        assertNull(discoveryService.getScanInputDescription());
        discoveryService.startScan();
        assertEquals(4, discoveryService.discoveryResults);
        discoveryService.stopScan();
        assertEquals(3, discoveryService.discoveryResults);
    public void testDiscoveryResultsWhenCodeRequired() {
        TestDiscoveryServiceWithRequiredCode discoveryService = new TestDiscoveryServiceWithRequiredCode(i18nProvider,
                localeProvider);
        assertTrue(discoveryService.isScanInputSupported());
        assertThat(discoveryService.getScanInputLabel(), is(PAIRING_CODE_LABEL));
        assertThat(discoveryService.getScanInputDescription(), is(PAIRING_CODE_DESCR));
        discoveryService.startScan("code");
