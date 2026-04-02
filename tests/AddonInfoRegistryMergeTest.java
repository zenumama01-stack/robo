import static org.mockito.ArgumentMatchers.*;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.TestInstance;
import org.junit.jupiter.api.TestInstance.Lifecycle;
 * JUnit test for the {@link AddonInfoRegistry} merge function.
@TestInstance(Lifecycle.PER_CLASS)
class AddonInfoRegistryMergeTest {
    private @Nullable AddonInfoProvider addonInfoProvider0;
    private @Nullable AddonInfoProvider addonInfoProvider1;
    private @Nullable AddonInfoProvider addonInfoProvider2;
    @BeforeAll
    void beforeAll() {
        addonInfoProvider0 = createAddonInfoProvider0();
        addonInfoProvider1 = createAddonInfoProvider1();
        addonInfoProvider2 = createAddonInfoProvider2();
    private AddonInfoProvider createAddonInfoProvider0() {
        AddonInfo addonInfo = AddonInfo.builder("hue", "binding").withName("name-zero").withKeywords("the,quick")
                .withDescription("description-zero").build();
        AddonInfoProvider provider = mock(AddonInfoProvider.class);
        when(provider.getAddonInfo(anyString(), any(Locale.class))).thenReturn(null);
        when(provider.getAddonInfo(anyString(), eq(null))).thenReturn(null);
        when(provider.getAddonInfo(eq("binding-hue"), any(Locale.class))).thenReturn(addonInfo);
        when(provider.getAddonInfo(eq("binding-hue"), eq(null))).thenReturn(null);
    private AddonInfoProvider createAddonInfoProvider1() {
        AddonDiscoveryMethod discoveryMethod = new AddonDiscoveryMethod().setServiceType("mdns")
                .setParameters(List.of(new AddonParameter("mdnsServiceType", "_hue._tcp.local.")));
        AddonInfo addonInfo = AddonInfo.builder("hue", "binding").withName("name-one").withKeywords("brown,fox")
                .withDescription("description-one").withCountries("GB,NL").withConnection("local")
                .withDiscoveryMethods(List.of(discoveryMethod)).build();
    private AddonInfoProvider createAddonInfoProvider2() {
        AddonDiscoveryMethod discoveryMethod = new AddonDiscoveryMethod().setServiceType("upnp")
                .setMatchProperties(List.of(new AddonMatchProperty("modelName", "Philips hue bridge")));
        AddonInfo addonInfo = AddonInfo.builder("hue", "binding").withName("name-two")
                .withDescription("description-two").withCountries("DE,FR").withSourceBundle("source-bundle")
                .withServiceId("service-id").withConfigDescriptionURI("http://www.openhab.org")
     * Test fetching a single addon-info from the registry with no merging.
    void testGetOneAddonInfo() {
        AddonInfoRegistry registry = new AddonInfoRegistry();
        assertNotNull(addonInfoProvider0);
        registry.addAddonInfoProvider(Objects.requireNonNull(addonInfoProvider0));
        AddonInfo addonInfo;
        addonInfo = registry.getAddonInfo("aardvark", Locale.US);
        assertNull(addonInfo);
        addonInfo = registry.getAddonInfo("aardvark", null);
        addonInfo = registry.getAddonInfo("binding-hue", null);
        addonInfo = registry.getAddonInfo("binding-hue", Locale.US);
        assertNotNull(addonInfo);
        assertEquals("hue", addonInfo.getId());
        assertEquals("binding", addonInfo.getType());
        assertEquals("binding-hue", addonInfo.getUID());
        assertTrue(addonInfo.getName().startsWith("name-"));
        assertTrue(addonInfo.getDescription().startsWith("description-"));
        assertNull(addonInfo.getSourceBundle());
        assertNotEquals("local", addonInfo.getConnection());
        assertEquals(0, addonInfo.getCountries().size());
        assertNotEquals("http://www.openhab.org", addonInfo.getConfigDescriptionURI());
        assertEquals("binding.hue", addonInfo.getServiceId());
        assertEquals(0, addonInfo.getDiscoveryMethods().size());
     * Test fetching two addon-info's from the registry with merging.
    void testMergeAddonInfos2() {
        assertNotNull(addonInfoProvider1);
        registry.addAddonInfoProvider(Objects.requireNonNull(addonInfoProvider1));
        assertEquals("local", addonInfo.getConnection());
        assertEquals(2, addonInfo.getCountries().size());
        assertEquals(1, addonInfo.getDiscoveryMethods().size());
        assertTrue(addonInfo.getKeywords().contains("the"));
        assertTrue(addonInfo.getKeywords().contains("quick"));
        assertTrue(addonInfo.getKeywords().contains("brown"));
        assertTrue(addonInfo.getKeywords().contains("fox"));
     * Test fetching three addon-info's from the registry with full merging.
    void testMergeAddonInfos3() {
        assertNotNull(addonInfoProvider2);
        registry.addAddonInfoProvider(Objects.requireNonNull(addonInfoProvider2));
        assertEquals("source-bundle", addonInfo.getSourceBundle());
        assertEquals(4, addonInfo.getCountries().size());
        assertEquals("http://www.openhab.org", addonInfo.getConfigDescriptionURI());
        assertEquals("service-id", addonInfo.getServiceId());
        assertEquals(2, addonInfo.getDiscoveryMethods().size());
