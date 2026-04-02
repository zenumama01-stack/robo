package org.openhab.core.config.discovery.addon.tests;
import org.junit.jupiter.api.AfterAll;
public class AddonSuggestionServiceTests {
    private @NonNullByDefault({}) LocaleProvider localeProvider;
    private @NonNullByDefault({}) AddonInfoProvider addonInfoProvider;
    private @NonNullByDefault({}) AddonFinder mdnsAddonFinder;
    private @NonNullByDefault({}) AddonFinder upnpAddonFinder;
    private @NonNullByDefault({}) AddonSuggestionService addonSuggestionService;
    private final HashMap<String, Object> config = new HashMap<>(
            Map.of(AddonFinderConstants.CFG_FINDER_MDNS, true, AddonFinderConstants.CFG_FINDER_UPNP, true));
    @AfterAll
    public void cleanUp() {
        assertNotNull(addonSuggestionService);
            addonSuggestionService.deactivate();
        setupMockLocaleProvider();
        setupMockAddonInfoProvider();
        setupMockMdnsAddonFinder();
        setupMockUpnpAddonFinder();
        addonSuggestionService = createAddonSuggestionService();
    private AddonSuggestionService createAddonSuggestionService() {
        AddonSuggestionService addonSuggestionService = new AddonSuggestionService(localeProvider, config);
        addonSuggestionService.addAddonFinder(mdnsAddonFinder);
        addonSuggestionService.addAddonFinder(upnpAddonFinder);
        return addonSuggestionService;
    private void setupMockLocaleProvider() {
        localeProvider = mock(LocaleProvider.class);
        when(localeProvider.getLocale()).thenReturn(Locale.US);
        assertNotNull(localeProvider);
        assertEquals(Locale.US, localeProvider.getLocale());
    private void setupMockAddonInfoProvider() {
        AddonDiscoveryMethod hue1 = new AddonDiscoveryMethod().setServiceType(AddonFinderConstants.SERVICE_TYPE_UPNP)
        AddonDiscoveryMethod hue2 = new AddonDiscoveryMethod().setServiceType(AddonFinderConstants.SERVICE_TYPE_MDNS)
        addonInfoProvider = mock(AddonInfoProvider.class);
        Set<AddonInfo> addonInfos = new HashSet<>();
                .withDiscoveryMethods(List.of(hue1, hue2)).build());
        when(addonInfoProvider.getAddonInfos(any(Locale.class))).thenReturn(addonInfos);
        assertNotNull(addonInfoProvider);
        Set<AddonInfo> addonInfos2 = addonInfoProvider.getAddonInfos(Locale.US);
        assertEquals(2, addonInfos2.size());
        assertTrue(addonInfos2.stream().anyMatch(a -> "binding-hue".equals(a.getUID())));
        assertTrue(addonInfos2.stream().anyMatch(a -> "binding-hpprinter".equals(a.getUID())));
    private void setupMockMdnsAddonFinder() {
        mdnsAddonFinder = mock(AddonFinder.class);
        Set<AddonInfo> addonInfos = addonInfoProvider.getAddonInfos(Locale.US).stream().filter(
                c -> c.getDiscoveryMethods().stream().anyMatch(m -> SERVICE_TYPE_MDNS.equals(m.getServiceType())))
        when(mdnsAddonFinder.getSuggestedAddons()).thenReturn(addonInfos);
        Set<AddonInfo> addonInfos2 = mdnsAddonFinder.getSuggestedAddons();
    private void setupMockUpnpAddonFinder() {
        upnpAddonFinder = mock(AddonFinder.class);
                c -> c.getDiscoveryMethods().stream().anyMatch(m -> SERVICE_TYPE_UPNP.equals(m.getServiceType())))
        when(upnpAddonFinder.getSuggestedAddons()).thenReturn(addonInfos);
        Set<AddonInfo> addonInfos2 = upnpAddonFinder.getSuggestedAddons();
        assertEquals(1, addonInfos2.size());
        addonSuggestionService.addAddonInfoProvider(addonInfoProvider);
        Set<AddonInfo> addons = addonSuggestionService.getSuggestedAddons(localeProvider.getLocale());
