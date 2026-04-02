import static org.hamcrest.MatcherAssert.assertThat;
import static org.hamcrest.Matchers.*;
import static org.mockito.Mockito.*;
import static org.openhab.core.addon.marketplace.AbstractRemoteAddonService.CONFIG_REMOTE_ENABLED;
import static org.openhab.core.addon.marketplace.test.TestAddonService.ALL_ADDON_COUNT;
import static org.openhab.core.addon.marketplace.test.TestAddonService.COMPATIBLE_ADDON_COUNT;
import static org.openhab.core.addon.marketplace.test.TestAddonService.INCOMPATIBLE_VERSION;
import static org.openhab.core.addon.marketplace.test.TestAddonService.INSTALL_EXCEPTION_ADDON;
import static org.openhab.core.addon.marketplace.test.TestAddonService.SERVICE_PID;
import static org.openhab.core.addon.marketplace.test.TestAddonService.TEST_ADDON;
import static org.openhab.core.addon.marketplace.test.TestAddonService.UNINSTALL_EXCEPTION_ADDON;
import java.util.Hashtable;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.Mock;
import org.mockito.Mockito;
import org.mockito.junit.jupiter.MockitoExtension;
import org.mockito.junit.jupiter.MockitoSettings;
import org.mockito.quality.Strictness;
import org.openhab.core.addon.marketplace.test.TestAddonHandler;
import org.openhab.core.addon.marketplace.test.TestAddonService;
import org.openhab.core.test.storage.VolatileStorage;
 * The {@link AbstractRemoteAddonServiceTest} contains tests for the
 * {@link org.openhab.core.addon.marketplace.AbstractRemoteAddonService}
@ExtendWith(MockitoExtension.class)
@MockitoSettings(strictness = Strictness.LENIENT)
public class AbstractRemoteAddonServiceTest {
    private @Mock @NonNullByDefault({}) StorageService storageService;
    private @Mock @NonNullByDefault({}) AddonInfoRegistry addonInfoRegistry;
    private @Mock @NonNullByDefault({}) ConfigurationAdmin configurationAdmin;
    private @Mock @NonNullByDefault({}) EventPublisher eventPublisher;
    private @Mock @NonNullByDefault({}) Configuration configuration;
    private @NonNullByDefault({}) Storage<String> storage;
    private @NonNullByDefault({}) TestAddonService addonService;
    private @NonNullByDefault({}) TestAddonHandler addonHandler;
    private final Dictionary<String, Object> properties = new Hashtable<>();
    @BeforeEach
    public void initialize() throws IOException {
        storage = new VolatileStorage<>();
        Mockito.doReturn(storage).when(storageService).getStorage(SERVICE_PID);
        Mockito.doReturn(configuration).when(configurationAdmin).getConfiguration("org.openhab.addons", null);
        Mockito.doReturn(properties).when(configuration).getProperties();
        addonHandler = new TestAddonHandler();
        addonService = new TestAddonService(eventPublisher, configurationAdmin, storageService, addonInfoRegistry);
        addonService.addAddonHandler(addonHandler);
    // general tests
    @Test
    public void testSourceNotRefreshedIfAddonHandlerNotReady() {
        addonHandler.setReady(false);
        List<Addon> addons = addonService.getAddons(null);
        assertThat(addons, empty());
    public void testRemoteDisabledBlocksRemoteCalls() {
        properties.put("remote", false);
        assertThat(addonService.getRemoteCalls(), is(0));
    public void testAddonResultsAreCached() {
        assertThat(addons, hasSize(COMPATIBLE_ADDON_COUNT));
        addons = addonService.getAddons(null);
        assertThat(addonService.getRemoteCalls(), is(1));
    public void testAddonIsReportedAsInstalledIfStorageEntryMissing() {
        addonService.setInstalled(TEST_ADDON);
        Addon addon = addons.stream().filter(a -> getFullAddonId(TEST_ADDON).equals(a.getUid())).findAny().orElse(null);
        assertThat(addon, notNullValue());
        assertThat(addon.isInstalled(), is(true));
    public void testInstalledAddonIsStillPresentAfterRemoteIsDisabledOrMissing() {
        addonService.addToStorage(TEST_ADDON);
        // check all addons are present
        // disable remote repo
        properties.put(CONFIG_REMOTE_ENABLED, false);
        // check only the installed addon is present
        assertThat(addons, hasSize(1));
        assertThat(addons.getFirst().getUid(), is(getFullAddonId(TEST_ADDON)));
    public void testIncompatibleAddonsNotIncludedByDefault() {
        assertThat(addonService.getAddons(null), hasSize(COMPATIBLE_ADDON_COUNT));
    public void testIncompatibleAddonsAlwaysIncludedIfInstalled() {
        addonService.setInstalled(INCOMPATIBLE_VERSION);
        assertThat(addonService.getAddons(null), hasSize(COMPATIBLE_ADDON_COUNT + 1));
    public void testIncompatibleAddonsAreIncludedIfRequested() {
        properties.put("includeIncompatible", true);
        assertThat(addonService.getAddons(null), hasSize(ALL_ADDON_COUNT));
    // installation tests
    public void testAddonInstall() {
        addonService.getAddons(null);
        addonService.install(TEST_ADDON);
        checkResult(TEST_ADDON, getFullAddonId(TEST_ADDON) + "/installed", true, true);
    public void testAddonInstallFailsWithHandlerException() {
        addonService.install(INSTALL_EXCEPTION_ADDON);
        checkResult(INSTALL_EXCEPTION_ADDON, getFullAddonId(INSTALL_EXCEPTION_ADDON) + "/failed", false, true);
    public void testAddonInstallFailsOnInstalledAddon() {
        checkResult(TEST_ADDON, getFullAddonId(TEST_ADDON) + "/failed", true, true);
    public void testAddonInstallFailsOnUnknownAddon() {
        addonService.install("unknown");
        checkResult("unknown", "unknown/failed", false, false);
    // uninstallation tests
    public void testAddonUninstall() {
        addonService.uninstall(TEST_ADDON);
        checkResult(TEST_ADDON, getFullAddonId(TEST_ADDON) + "/uninstalled", false, true);
    public void testAddonUninstallFailsWithHandlerException() {
        addonService.setInstalled(UNINSTALL_EXCEPTION_ADDON);
        addonService.addToStorage(UNINSTALL_EXCEPTION_ADDON);
        addonService.uninstall(UNINSTALL_EXCEPTION_ADDON);
        checkResult(UNINSTALL_EXCEPTION_ADDON, getFullAddonId(UNINSTALL_EXCEPTION_ADDON) + "/failed", true, true);
    public void testAddonUninstallFailsOnUninstalledAddon() {
        checkResult(TEST_ADDON, getFullAddonId(TEST_ADDON) + "/failed", false, true);
    public void testAddonUninstallFailsOnUnknownAddon() {
        addonService.uninstall("unknown");
    public void testUninstalledAddonIsReinstalled() {
    // add-comparisons
    public void testAddonOrdering() {
        Addon addon1 = getMockedAddon("4.1.0", false);
        Addon addon2 = getMockedAddon("4.2.0", true);
        Addon addon3 = getMockedAddon("4.1.4", false);
        Addon addon4 = getMockedAddon("4.2.1", true);
        List<Addon> actual = Stream.of(addon1, addon2, addon3, addon4)
                .sorted(AbstractRemoteAddonService.BY_COMPATIBLE_AND_VERSION).toList();
        List<Addon> expected = List.of(addon4, addon2, addon3, addon1);
        assertThat(actual, is(equalTo(expected)));
    public void testSnapshotVersionsAreParsedProperly() {
        Addon addon1 = getMockedAddon("4.1.0", true);
        Addon addon2 = getMockedAddon("4.2.0-SNAPSHOT", true);
        List<Addon> actual = Stream.of(addon1, addon2).sorted(AbstractRemoteAddonService.BY_COMPATIBLE_AND_VERSION)
        List<Addon> expected = List.of(addon2, addon1);
    private Addon getMockedAddon(String version, boolean compatible) {
        Addon addon = mock(Addon.class);
        when(addon.getVersion()).thenReturn(version);
        when(addon.getCompatible()).thenReturn(compatible);
        return addon;
     * checks that a proper event is posted, the presence in storage and installation status in handler
     * @param id add-on id (without service-prefix)
     * @param expectedEventTopic the expected event (e.g. installed)
     * @param installStatus the expected installation status of the add-on
     * @param present if the addon is expected to be present after the test
    private void checkResult(String id, String expectedEventTopic, boolean installStatus, boolean present) {
        // assert expected event is posted
        ArgumentCaptor<Event> eventCaptor = ArgumentCaptor.forClass(Event.class);
        Mockito.verify(eventPublisher, timeout(5000)).post(eventCaptor.capture());
        Event event = eventCaptor.getValue();
        String topic = "openhab/addons/" + expectedEventTopic;
        assertThat(event.toString(), event.getTopic(), is(topic));
        // assert addon handler was called (by checking it's installed status)
        assertThat(addonHandler.isInstalled(getFullAddonId(id)), is(installStatus));
        // assert is present in storage if installed or missing if uninstalled
        assertThat(storage.containsKey(id), is(installStatus));
        // assert correct installation status is reported for addon
        Addon addon = addonService.getAddon(id, null);
        if (present) {
            assertThat(addon.isInstalled(), is(installStatus));
            assertThat(addon, nullValue());
    private String getFullAddonId(String id) {
        return SERVICE_PID + ":" + id;
