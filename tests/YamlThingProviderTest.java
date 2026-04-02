import org.openhab.core.model.yaml.YamlModelUtils;
import org.openhab.core.thing.binding.ThingHandlerCallback;
import org.openhab.core.thing.internal.ThingFactoryHelper;
import org.openhab.core.thing.type.ChannelDefinitionBuilder;
 * The {@link YamlThingProviderTest} contains tests for the {@link YamlThingProvider} class.
public class YamlThingProviderTest {
    private static final Path SOURCE_PATH = Path.of("src/test/resources/model/things");
    private static final String NTP_BUNDLE_SYMBOLIC_NAME = "org.openhab.binding.ntp";
    private static final ThingTypeUID NTP_THING_TYPE_UID = new ThingTypeUID("ntp", "ntp");
    private static final String NTP_THING_TYPE_LABEL = "NTP Server";
    private static final String NTP_THING_TYPE_DESCRIPTION = "An NTP server that provides current date and time";
    private static final String NTP_THING_TYPE_EQUIPMENT_TAG = "WebService";
    private static final String DEFAULT_HOSTNAME = "0.pool.ntp.org";
    private static final int DEFAULT_REFRESH_INTERVAL = 60;
    private static final int DEFAULT_REFRESH_NTP = 30;
    private static final int DEFAULT_SERVER_PORT = 123;
    private static final ChannelTypeUID NTP_CHANNEL_TYPE_STRING_CHANNEL_UID = new ChannelTypeUID("ntp",
            "string-channel");
    private static final String NTP_CHANNEL_TYPE_STRING_CHANNEL_LABEL = "Date";
    private static final String NTP_CHANNEL_TYPE_STRING_CHANNEL_DESCRIPTION = "NTP refreshed date and time.";
    private static final String NTP_CHANNEL_TYPE_STRING_CHANNEL_ITEM_TYPE = "String";
    private static final String NTP_CHANNEL_TYPE_STRING_CHANNEL_CATEGORY = "Time";
    private static final String TAG_STATUS = "Status";
    private static final String TAG_TIMESTAMP = "Timestamp";
    private static final String NTP_CHANNEL_STRING_ID = "string";
    private static final String DEFAULT_DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss z";
    private static final ThingUID NTP_THING_UID = new ThingUID(NTP_THING_TYPE_UID, "local");
    private @Mock @NonNullByDefault({}) BundleResolver bundleResolver;
    private @Mock @NonNullByDefault({}) ThingTypeRegistry thingTypeRegistry;
    private @Mock @NonNullByDefault({}) ChannelTypeRegistry channelTypeRegistry;
    private @Mock @NonNullByDefault({}) ConfigDescriptionRegistry configDescriptionRegistry;
    private @Mock @NonNullByDefault({}) LocaleProvider localeProvider;
    private @Mock @NonNullByDefault({}) Bundle bundle;
    private @NonNullByDefault({}) YamlThingProvider thingProvider;
    private @NonNullByDefault({}) TestThingChangeListener thingListener;
        when(bundleResolver.resolveBundle(any())).thenReturn(bundle);
        when(bundle.getSymbolicName()).thenReturn("org.openhab.binding.ntp");
        URI uriThingType = null;
        URI uriChannelType = null;
        URI uriThing1 = null;
        URI uriChannel1 = null;
        URI uriChannel2 = null;
            uriThingType = new URI("thing-type:" + NTP_THING_TYPE_UID.getAsString());
            uriChannelType = new URI("channel-type:" + NTP_CHANNEL_TYPE_STRING_CHANNEL_UID.getAsString());
            uriThing1 = new URI("thing:" + NTP_THING_UID.getAsString());
            uriChannel1 = new URI("channel:" + NTP_THING_UID.getAsString() + ":" + NTP_CHANNEL_STRING_ID);
            uriChannel2 = new URI("channel:" + NTP_THING_UID.getAsString() + ":date-only-string");
        assertNotNull(uriThingType);
        assertNotNull(uriChannelType);
        assertNotNull(uriThing1);
        assertNotNull(uriChannel1);
        assertNotNull(uriChannel2);
        List<ChannelDefinition> channelDefinitions = new ArrayList<>();
        channelDefinitions
                .add(new ChannelDefinitionBuilder(NTP_CHANNEL_STRING_ID, NTP_CHANNEL_TYPE_STRING_CHANNEL_UID).build());
        ThingType ntpThingType = ThingTypeBuilder.instance(NTP_THING_TYPE_UID, NTP_THING_TYPE_LABEL)
                .withDescription(NTP_THING_TYPE_DESCRIPTION).withConfigDescriptionURI(uriThingType)
                .withSemanticEquipmentTag(NTP_THING_TYPE_EQUIPMENT_TAG).withChannelDefinitions(channelDefinitions)
        when(thingTypeRegistry.getThingType(eq(NTP_THING_TYPE_UID))).thenReturn(ntpThingType);
        when(thingTypeRegistry.getThingType(eq(NTP_THING_TYPE_UID), eq(Locale.FRENCH))).thenReturn(ntpThingType);
        ChannelType channelType = ChannelTypeBuilder
                .state(NTP_CHANNEL_TYPE_STRING_CHANNEL_UID, NTP_CHANNEL_TYPE_STRING_CHANNEL_LABEL,
                        NTP_CHANNEL_TYPE_STRING_CHANNEL_ITEM_TYPE)
                .withDescription(NTP_CHANNEL_TYPE_STRING_CHANNEL_DESCRIPTION)
                .withCategory(NTP_CHANNEL_TYPE_STRING_CHANNEL_CATEGORY).withConfigDescriptionURI(uriChannelType)
                .withTags(Set.of(TAG_STATUS, TAG_TIMESTAMP)).build();
        when(channelTypeRegistry.getChannelType(eq(NTP_CHANNEL_TYPE_STRING_CHANNEL_UID))).thenReturn(channelType);
        when(channelTypeRegistry.getChannelType(eq(NTP_CHANNEL_TYPE_STRING_CHANNEL_UID), eq(Locale.FRENCH)))
                .thenReturn(channelType);
        List<ConfigDescriptionParameter> params = new ArrayList<>();
        params.add(ConfigDescriptionParameterBuilder.create("hostname", Type.TEXT).withRequired(true)
                .withContext("network-address").withLabel("Hostname<").withDescription("The NTP server hostname.")
                .withDefault(DEFAULT_HOSTNAME).build());
        params.add(
                ConfigDescriptionParameterBuilder.create("refreshInterval", Type.INTEGER).withLabel("Refresh Interval")
                        .withDescription("Interval that new time updates are posted to the event bus in seconds.")
                        .withDefault(String.valueOf(DEFAULT_REFRESH_INTERVAL)).build());
                ConfigDescriptionParameterBuilder.create("refreshNtp", Type.INTEGER).withLabel("NTP Refresh Frequency<")
                        .withDescription("Number of updates before querying the NTP server.")
                        .withDefault(String.valueOf(DEFAULT_REFRESH_NTP)).build());
        params.add(ConfigDescriptionParameterBuilder.create("serverPort", Type.INTEGER).withLabel("Server Port")
                .withDescription("The port that the NTP server could use.")
                .withDefault(String.valueOf(DEFAULT_SERVER_PORT)).build());
        params.add(ConfigDescriptionParameterBuilder.create("timeZone", Type.TEXT).withLabel("Timezone")
                .withDescription("The configured timezone.").build());
        ConfigDescription configDescrThing = ConfigDescriptionBuilder.create(uriThingType).withParameters(params)
        when(configDescriptionRegistry.getConfigDescription(eq(uriThingType))).thenReturn(configDescrThing);
        ConfigDescriptionParameter param = ConfigDescriptionParameterBuilder.create("DateTimeFormat", Type.TEXT)
                .withRequired(false).withLabel("Date Time Format").withDescription("Formatting of the date and time.")
                .withDefault(DEFAULT_DATE_TIME_FORMAT).build();
        ConfigDescription configDescrChannel2 = ConfigDescriptionBuilder.create(uriChannelType).withParameter(param)
        when(configDescriptionRegistry.getConfigDescription(eq(uriChannelType))).thenReturn(configDescrChannel2);
        when(configDescriptionRegistry.getConfigDescription(eq(uriThing1))).thenReturn(null);
        when(configDescriptionRegistry.getConfigDescription(eq(uriChannel1))).thenReturn(null);
        when(configDescriptionRegistry.getConfigDescription(eq(uriChannel2))).thenReturn(null);
        when(localeProvider.getLocale()).thenReturn(Locale.FRENCH);
        NtpThingHandlerFactory thingHandlerFactory = new NtpThingHandlerFactory();
        thingProvider = new YamlThingProvider(bundleResolver, thingTypeRegistry, channelTypeRegistry,
                configDescriptionRegistry, localeProvider);
        thingProvider.onReadyMarkerAdded(new ReadyMarker(XML_THING_TYPE, NTP_BUNDLE_SYMBOLIC_NAME));
        thingListener = new TestThingChangeListener();
        thingProvider.addProviderChangeListener(thingListener);
        modelRepository.addYamlModelListener(thingProvider);
    public void testLoadModelWithThing() throws IOException {
        Files.copy(SOURCE_PATH.resolve("thing.yaml"), fullModelPath);
        assertFalse(YamlModelUtils.isIsolatedModel(MODEL_NAME));
        assertThat(thingListener.things, is(aMapWithSize(1)));
        assertThat(thingListener.things, hasKey("ntp:ntp:local"));
        assertThat(thingProvider.getAllFromModel(MODEL_NAME), hasSize(1));
        Collection<Thing> things = thingProvider.getAll();
        assertThat(things, hasSize(1));
        Thing thing = things.iterator().next();
        assertFalse(thing instanceof Bridge);
        assertEquals(NTP_THING_UID, thing.getUID());
        assertNull(thing.getBridgeUID());
        assertEquals(NTP_THING_TYPE_UID, thing.getThingTypeUID());
        assertEquals("NTP Local Server", thing.getLabel());
        assertEquals("Paris", thing.getLocation());
        assertEquals(NTP_THING_TYPE_EQUIPMENT_TAG, thing.getSemanticEquipmentTag());
        assertEquals(0, thing.getProperties().size());
        // 2 parameters injected with default value
        assertThat(thing.getConfiguration().keySet(),
                containsInAnyOrder("hostname", "refreshInterval", "refreshNtp", "serverPort", "timeZone", "other"));
        assertEquals("0.fr.pool.ntp.org", thing.getConfiguration().get("hostname"));
        assertEquals(BigDecimal.valueOf(123), thing.getConfiguration().get("serverPort"));
        assertEquals("Europe/Paris", thing.getConfiguration().get("timeZone"));
        assertEquals("A parameter that is not in the thing config description.", thing.getConfiguration().get("other"));
        // default value injected for parameter refreshInterval
        assertEquals(BigDecimal.valueOf(DEFAULT_REFRESH_INTERVAL), thing.getConfiguration().get("refreshInterval"));
        // default value injected for parameter refreshNtp
        assertEquals(BigDecimal.valueOf(DEFAULT_REFRESH_NTP), thing.getConfiguration().get("refreshNtp"));
        assertEquals(2, thing.getChannels().size());
        Iterator<Channel> it = thing.getChannels().iterator();
        Channel channel = it.next();
        assertEquals(new ChannelUID(NTP_THING_UID, "string"), channel.getUID());
        assertEquals(NTP_CHANNEL_TYPE_STRING_CHANNEL_UID, channel.getChannelTypeUID());
        assertEquals(ChannelKind.STATE, channel.getKind());
        assertEquals(NTP_CHANNEL_TYPE_STRING_CHANNEL_ITEM_TYPE, channel.getAcceptedItemType());
        // label in YAML is ignored for a channel provided by the channel type
        assertEquals(NTP_CHANNEL_TYPE_STRING_CHANNEL_LABEL, channel.getLabel());
        // description in YAML is ignored for a channel provided by the channel type
        assertEquals(NTP_CHANNEL_TYPE_STRING_CHANNEL_DESCRIPTION, channel.getDescription());
        assertNull(channel.getAutoUpdatePolicy());
        assertThat(channel.getDefaultTags(), containsInAnyOrder(TAG_STATUS, TAG_TIMESTAMP));
        assertEquals(0, channel.getProperties().size());
        assertThat(channel.getConfiguration().keySet(), containsInAnyOrder("DateTimeFormat", "other"));
        assertEquals("dd-MM-yyyy HH:mm", channel.getConfiguration().get("DateTimeFormat"));
        assertEquals("A parameter that is not in the channel config description.",
                channel.getConfiguration().get("other"));
        channel = it.next();
        assertEquals(new ChannelUID(NTP_THING_UID, "date-only-string"), channel.getUID());
        assertEquals("Date Only", channel.getLabel());
        assertEquals("Format with date only.", channel.getDescription());
        assertThat(channel.getDefaultTags(), hasSize(0));
        assertEquals("dd-MM-yyyy", channel.getConfiguration().get("DateTimeFormat"));
    public void testLoadModelWithThingEmptyConfig() throws IOException {
        Files.copy(SOURCE_PATH.resolve("thingWithEmptyConfig.yaml"), fullModelPath);
        assertEquals(NTP_THING_TYPE_LABEL, thing.getLabel());
        assertNull(thing.getLocation());
        // 4 parameters injected with default value
                containsInAnyOrder("hostname", "refreshInterval", "refreshNtp", "serverPort"));
        // default value injected for parameter hostname
        assertEquals(DEFAULT_HOSTNAME, thing.getConfiguration().get("hostname"));
        // default value injected for parameter serverPort
        assertEquals(BigDecimal.valueOf(DEFAULT_SERVER_PORT), thing.getConfiguration().get("serverPort"));
        // Parameter DateTimeFormat injected with default value
        assertThat(channel.getConfiguration().keySet(), contains("DateTimeFormat"));
        assertEquals(DEFAULT_DATE_TIME_FORMAT, channel.getConfiguration().get("DateTimeFormat"));
    public void testCreateIsolatedModelWithThing() throws IOException {
        try (FileInputStream inputStream = new FileInputStream(fullModelPath.toFile())) {
            String name = modelRepository.createIsolatedModel(inputStream, errors, warnings);
            assertNotNull(name);
            assertEquals(0, errors.size());
            assertEquals(0, warnings.size());
            assertTrue(YamlModelUtils.isIsolatedModel(name));
            assertThat(thingListener.things, is(aMapWithSize(0)));
            assertThat(thingProvider.getAll(), hasSize(0)); // No thing for the registry
            Collection<Thing> things = thingProvider.getAllFromModel(name);
            // No parameter injected
                    containsInAnyOrder("hostname", "serverPort", "timeZone", "other"));
            assertEquals("A parameter that is not in the thing config description.",
                    thing.getConfiguration().get("other"));
            // label in YAML is ignored for a channel provided by the thing type
            // description in YAML is ignored for a channel provided by the thing type
    public void testCreateIsolatedModelWithThingEmptyConfig() throws IOException {
            assertThat(thing.getConfiguration().keySet(), hasSize(0));
            // default value not injected for parameter DateTimeFormat
            assertThat(channel.getConfiguration().keySet(), hasSize(0));
    public void testConversionForConfigWithTextParam() throws IOException {
        Files.copy(SOURCE_PATH.resolve("thingWithNumberInTextParam.yaml"), fullModelPath);
        Collection<Thing> things = thingProvider.getAllFromModel(MODEL_NAME);
                containsInAnyOrder("hostname", "timeZone", "refreshInterval", "refreshNtp", "serverPort"));
        assertEquals("12345", thing.getConfiguration().get("hostname"));
        assertEquals("12.3", thing.getConfiguration().get("timeZone"));
        assertEquals("100", channel.getConfiguration().get("DateTimeFormat"));
        assertEquals("123.45", channel.getConfiguration().get("DateTimeFormat"));
    private class NtpThingHandlerFactory implements ThingHandlerFactory {
        public boolean supportsThingType(ThingTypeUID thingTypeUID) {
            return NTP_THING_TYPE_UID.equals(thingTypeUID);
        public ThingHandler registerHandler(Thing thing) {
            return new NtpThingHandler(thing);
        public void unregisterHandler(Thing thing) {
        public @Nullable Thing createThing(ThingTypeUID thingTypeUID, Configuration configuration,
                @Nullable ThingUID thingUID, @Nullable ThingUID bridgeUID) {
            ThingUID effectiveUID = thingUID != null ? thingUID : ThingFactory.generateRandomThingUID(thingTypeUID);
                ThingFactoryHelper.applyDefaultConfiguration(configuration, thingType, configDescriptionRegistry);
                List<ChannelDefinition> channelDefinitions = thingType.getChannelDefinitions();
                    Channel channel = createChannel(channelDefinition, effectiveUID, configDescriptionRegistry);
                return ThingBuilder.create(thingTypeUID, effectiveUID).withConfiguration(configuration)
                        .withChannels(channels).withProperties(thingType.getProperties()).withBridge(bridgeUID)
                        .withSemanticEquipmentTag(thingType.getSemanticEquipmentTag()).build();
        public void removeThing(ThingUID thingUID) {
        private @Nullable Channel createChannel(ChannelDefinition channelDefinition, ThingUID thingUID,
                ConfigDescriptionRegistry configDescriptionRegistry) {
            final ChannelUID channelUID = new ChannelUID(thingUID, channelDefinition.getId());
            final ChannelBuilder channelBuilder = createChannelBuilder(channelUID, channelDefinition,
                    configDescriptionRegistry);
            if (channelBuilder == null) {
            return channelBuilder.withProperties(channelDefinition.getProperties()).build();
        private @Nullable ChannelBuilder createChannelBuilder(ChannelUID channelUID,
                ChannelDefinition channelDefinition, ConfigDescriptionRegistry configDescriptionRegistry) {
            ChannelType channelType = channelTypeRegistry.getChannelType(channelDefinition.getChannelTypeUID());
            AutoUpdatePolicy autoUpdatePolicy = channelDefinition.getAutoUpdatePolicy();
            if (autoUpdatePolicy == null) {
            final ChannelBuilder channelBuilder = ChannelBuilder.create(channelUID, channelType.getItemType()) //
                    .withType(channelType.getUID()) //
                    .withDefaultTags(channelType.getTags()) //
                    .withKind(channelType.getKind()) //
                    .withAutoUpdatePolicy(autoUpdatePolicy);
                channelBuilder.withDescription(description);
            // Initialize channel configuration with default-values
            URI uri = channelType.getConfigDescriptionURI();
            if (uri != null) {
                final Configuration configuration = new Configuration();
                        configDescriptionRegistry.getConfigDescription(uri));
                channelBuilder.withConfiguration(configuration);
            return channelBuilder;
    private static class NtpThingHandler implements ThingHandler {
        NtpThingHandler(Thing thing) {
            this.thing = thing;
        public Thing getThing() {
        public void setCallback(@Nullable ThingHandlerCallback thingHandlerCallback) {
        public void handleCommand(ChannelUID channelUID, Command command) {
        public void handleConfigurationUpdate(Map<String, Object> configurationParameters) {
        public void thingUpdated(Thing thing) {
        public void channelLinked(ChannelUID channelUID) {
        public void channelUnlinked(ChannelUID channelUID) {
        public void bridgeStatusChanged(ThingStatusInfo bridgeStatusInfo) {
        public void handleRemoval() {
    private static class TestThingChangeListener implements ProviderChangeListener<Thing> {
        public final Map<String, Thing> things = new HashMap<>();
        public void added(Provider<Thing> provider, Thing element) {
            things.put(element.getUID().getAsString(), element);
        public void removed(Provider<Thing> provider, Thing element) {
            things.remove(element.getUID().getAsString());
        public void updated(Provider<Thing> provider, Thing oldelement, Thing element) {
