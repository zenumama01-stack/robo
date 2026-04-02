package org.openhab.core.config.discovery.addon.sddp.test;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import org.openhab.core.config.discovery.addon.sddp.SddpAddonFinder;
 * JUnit tests for the {@link SddpAddonFinder}.
public class SddpAddonFinderTests {
    private static final Map<String, String> DEVICE_FIELDS = Map.of(
            "From", "\"192.168.4.237:1902\"",
            "Host", "\"JVC_PROJECTOR-E0DADC152802\"",
            "Max-Age", "1800",
            "Type", "\"JVCKENWOOD:Projector\"",
            "Primary-Proxy", "\"projector\"",
            "Proxies", "\"projector\"",
            "Manufacturer", "\"JVCKENWOOD\"",
            "Model", "\"DLA-RS3100_NZ8\"",
            "Driver", "\"projector_JVCKENWOOD_DLA-RS3100_NZ8.c4i\"");
    private List<AddonInfo> createAddonInfos() {
        AddonDiscoveryMethod method = new AddonDiscoveryMethod().setServiceType(SddpAddonFinder.SERVICE_TYPE)
                .setMatchProperties(List.of(new AddonMatchProperty("host", "JVC.*")));
        List<AddonInfo> addonInfos = new ArrayList<>();
        addonInfos.add(AddonInfo.builder("jvc", "binding").withName("JVC").withDescription("JVC Kenwood")
                .withDiscoveryMethods(List.of(method)).build());
    public void testFinder() {
        SddpDevice device = new SddpDevice(DEVICE_FIELDS, false);
        List<AddonInfo> addonInfos = createAddonInfos();
        SddpAddonFinder finder = new SddpAddonFinder(mock(SddpDiscoveryService.class));
        finder.setAddonCandidates(addonInfos);
        Set<AddonInfo> suggestions;
        AddonInfo info;
        finder.deviceAdded(device);
        suggestions = finder.getSuggestedAddons();
        assertFalse(suggestions.isEmpty());
        info = suggestions.stream().findFirst().orElse(null);
        assertNotNull(info);
        assertEquals("JVC Kenwood", info.getDescription());
        finder.deviceRemoved(device);
        assertTrue(suggestions.isEmpty());
