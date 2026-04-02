import ch.qos.logback.classic.Level;
 * This contains tests for the {@link UsbAddonFinder} class.
class UsbAddonFinderTests {
    void testSuggestionFinder() {
        Logger root = LoggerFactory.getLogger(org.slf4j.Logger.ROOT_LOGGER_NAME);
        ((ch.qos.logback.classic.Logger) root).setLevel(Level.ERROR);
        AddonMatchProperty matchProperty = new AddonMatchProperty("product", "(?i).*zigbee.*");
        AddonDiscoveryMethod discoveryMethod = new AddonDiscoveryMethod();
        discoveryMethod.setMatchProperties(List.of(matchProperty)).setServiceType("usb");
        List<AddonInfo> addons = new ArrayList<>();
        addons.add(AddonInfo.builder("id", "binding").withName("name").withDescription("description")
                .withDiscoveryMethods(List.of(discoveryMethod)).build());
        UsbAddonFinder finder = new UsbAddonFinder();
        finder.setAddonCandidates(addons);
        finder.usbSerialDeviceDiscovered(
                new UsbSerialDeviceInformation(0x123, 0x234, null, null, null, 0, "n/a", "n/a"));
        Set<AddonInfo> suggestions = finder.getSuggestedAddons();
        assertNotNull(suggestions);
                new UsbSerialDeviceInformation(0x345, 0x456, null, null, "some zigBEE product", 0, "n/a", "n/a"));
    void testBadSyntax() {
        AddonMatchProperty matchProperty = new AddonMatchProperty("aardvark", "(?i).*zigbee.*");
