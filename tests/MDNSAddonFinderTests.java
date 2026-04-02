package org.openhab.core.config.discovery.addon.mdns.tests;
import static org.openhab.core.config.discovery.addon.mdns.MDNSAddonFinder.MDNS_SERVICE_TYPE;
import org.openhab.core.config.discovery.addon.AddonFinderConstants;
import org.openhab.core.config.discovery.addon.AddonSuggestionService;
import org.openhab.core.config.discovery.addon.mdns.MDNSAddonFinder;
 * JUnit tests for the {@link AddonSuggestionService}.
 * @author Mark Herwege - Adapted to finders in separate packages
public class MDNSAddonFinderTests {
    private @NonNullByDefault({}) MDNSClient mdnsClient;
    private @NonNullByDefault({}) AddonFinder addonFinder;
    private List<AddonInfo> addonInfos = new ArrayList<>();
        setupMockMdnsClient();
        setupAddonInfos();
        createAddonFinder();
    private void createAddonFinder() {
        MDNSAddonFinder mdnsAddonFinder = new MDNSAddonFinder(mdnsClient);
        assertNotNull(mdnsAddonFinder);
        for (ServiceInfo service : mdnsClient.list("_hue._tcp.local.")) {
            mdnsAddonFinder.addService(service, true);
        for (ServiceInfo service : mdnsClient.list("_printer._tcp.local.")) {
        addonFinder = mdnsAddonFinder;
    private void setupMockMdnsClient() {
        // create the mock
        mdnsClient = mock(MDNSClient.class, Mockito.RETURNS_DEEP_STUBS);
        when(mdnsClient.list(anyString())).thenReturn(new ServiceInfo[] {});
        ServiceInfo hueService = ServiceInfo.create("hue", "hue", 0, 0, 0, false, "hue service");
        when(mdnsClient.list(eq("_hue._tcp.local."))).thenReturn(new ServiceInfo[] { hueService });
        ServiceInfo hpService = ServiceInfo.create("printer", "hpprinter", 0, 0, 0, false, "hp printer service");
        hpService.setText(Map.of("ty", "hp printer", "rp", "anything"));
        when(mdnsClient.list(eq("_printer._tcp.local."))).thenReturn(new ServiceInfo[] { hpService });
        // check that it works
        assertNotNull(mdnsClient);
        ServiceInfo[] result;
        result = mdnsClient.list("_printer._tcp.local.");
        assertEquals(1, result.length);
        assertEquals("hpprinter", result[0].getName());
        assertEquals(2, Collections.list(result[0].getPropertyNames()).size());
        assertEquals("hp printer", result[0].getPropertyString("ty"));
        result = mdnsClient.list("_hue._tcp.local.");
        assertEquals("hue", result[0].getName());
        result = mdnsClient.list("aardvark");
        assertEquals(0, result.length);
    private void setupAddonInfos() {
        AddonDiscoveryMethod hp = new AddonDiscoveryMethod().setServiceType(AddonFinderConstants.SERVICE_TYPE_MDNS)
                .setMatchProperties(
                        List.of(new AddonMatchProperty("rp", ".*"), new AddonMatchProperty("ty", "hp (.*)")))
                .setParameters(List.of(new AddonParameter(MDNS_SERVICE_TYPE, "_printer._tcp.local.")));
        addonInfos.add(AddonInfo.builder("hpprinter", "binding").withName("HP").withDescription("HP Printer")
                .withDiscoveryMethods(List.of(hp)).build());
        AddonDiscoveryMethod hue = new AddonDiscoveryMethod().setServiceType(AddonFinderConstants.SERVICE_TYPE_MDNS)
                .setParameters(List.of(new AddonParameter(MDNS_SERVICE_TYPE, "_hue._tcp.local.")));
        addonInfos.add(AddonInfo.builder("hue", "binding").withName("Hue").withDescription("Hue Bridge")
                .withDiscoveryMethods(List.of(hue)).build());
    public void testGetSuggestedAddons() {
        addonFinder.setAddonCandidates(addonInfos);
        Set<AddonInfo> addons = addonFinder.getSuggestedAddons();
        assertEquals(2, addons.size());
        assertFalse(addons.stream().anyMatch(a -> "aardvark".equals(a.getUID())));
        assertTrue(addons.stream().anyMatch(a -> "binding-hue".equals(a.getUID())));
        assertTrue(addons.stream().anyMatch(a -> "binding-hpprinter".equals(a.getUID())));
