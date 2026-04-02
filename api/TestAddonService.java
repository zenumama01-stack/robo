 * The {@link TestAddonService} is a
public class TestAddonService extends AbstractRemoteAddonService {
    public static final String TEST_ADDON = "testAddon";
    public static final String INSTALL_EXCEPTION_ADDON = "installException";
    public static final String UNINSTALL_EXCEPTION_ADDON = "uninstallException";
    public static final String INCOMPATIBLE_VERSION = "incompatibleVersion";
    public static final String SERVICE_PID = "testAddonService";
    public static final Set<String> REMOTE_ADDONS = Set.of(TEST_ADDON, INSTALL_EXCEPTION_ADDON,
            UNINSTALL_EXCEPTION_ADDON, INCOMPATIBLE_VERSION);
    public static final int COMPATIBLE_ADDON_COUNT = REMOTE_ADDONS.size() - 1;
    public static final int ALL_ADDON_COUNT = REMOTE_ADDONS.size();
    private int remoteCalls = 0;
    public TestAddonService(EventPublisher eventPublisher, ConfigurationAdmin configurationAdmin,
            StorageService storageService, AddonInfoRegistry addonInfoRegistry) {
        return new BundleVersion("3.2.0");
    public void addAddonHandler(MarketplaceAddonHandler handler) {
    public void removeAddonHandler(MarketplaceAddonHandler handler) {
        remoteCalls++;
        return REMOTE_ADDONS.stream()
                .map(id -> Addon.create(SERVICE_PID + ":" + id).withType("binding").withVersion("4.1.0")
                        .withId(id.substring("binding-".length()))
                        .withContentType(TestAddonHandler.TEST_ADDON_CONTENT_TYPE)
                        .withCompatible(!id.equals(INCOMPATIBLE_VERSION)).build())
        return SERVICE_PID;
        return "Test Addon Service";
        String remoteId = id.startsWith(SERVICE_PID) ? id : SERVICE_PID + ":" + id;
        return cachedAddons.stream().filter(a -> remoteId.equals(a.getUid())).findAny().orElse(null);
     * get the number of remote calls issued by the addon service
     * @return number of calls
    public int getRemoteCalls() {
        return remoteCalls;
     * this installs an addon to the service without calling the install method
     * @param id id of the addon to install
    public void setInstalled(String id) {
        Addon addon = Addon.create(SERVICE_PID + ":" + id).withType("binding").withId(id.substring("binding-".length()))
                .withVersion("4.1.0").withContentType(TestAddonHandler.TEST_ADDON_CONTENT_TYPE).build();
        addonHandlers.forEach(addonHandler -> {
                addonHandler.install(addon);
     * add to installedStorage
     * @param id id of the addon to add
    public void addToStorage(String id) {
