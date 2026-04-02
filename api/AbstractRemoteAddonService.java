package org.openhab.core.addon.marketplace;
import static org.openhab.core.common.ThreadPoolManager.THREAD_POOL_NAME_COMMON;
import java.time.Duration;
import java.util.ArrayList;
import java.util.Dictionary;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Set;
import org.openhab.core.addon.AddonEventFactory;
import org.openhab.core.cache.ExpiringCache;
import org.openhab.core.config.core.ConfigParser;
import org.openhab.core.events.Event;
import org.openhab.core.events.EventPublisher;
import org.openhab.core.storage.Storage;
import org.openhab.core.storage.StorageService;
import org.osgi.framework.FrameworkUtil;
import org.osgi.service.cm.Configuration;
import org.osgi.service.cm.ConfigurationAdmin;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonSyntaxException;
 * The {@link AbstractRemoteAddonService} implements basic functionality of a remote add-on-service
 * @author Jan N. Klug - Initial contribution
public abstract class AbstractRemoteAddonService implements AddonService {
    static final String CONFIG_REMOTE_ENABLED = "remote";
    static final String CONFIG_INCLUDE_INCOMPATIBLE = "includeIncompatible";
    static final Comparator<Addon> BY_COMPATIBLE_AND_VERSION = (addon1, addon2) -> {
        // prefer compatible to incompatible
        int compatible = Boolean.compare(addon2.getCompatible(), addon1.getCompatible());
        if (compatible != 0) {
            return compatible;
            // Add-on versions often contain a dash instead of a dot as separator for the qualifier (e.g. -SNAPSHOT)
            // This is not a valid format and everything after the dash needs to be removed.
            BundleVersion version1 = new BundleVersion(addon1.getVersion().replaceAll("-.*", ".0"));
            BundleVersion version2 = new BundleVersion(addon2.getVersion().replaceAll("-.*", ".0"));
            // prefer newer version over older
            return version2.compareTo(version1);
        } catch (IllegalArgumentException e) {
            // assume they are equal (for ordering) if we can't compare the versions
    protected final BundleVersion coreVersion;
    protected final Gson gson = new GsonBuilder().setDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'").create();
    protected final Set<MarketplaceAddonHandler> addonHandlers = new HashSet<>();
    protected final Storage<String> installedAddonStorage;
    protected final EventPublisher eventPublisher;
    protected final ConfigurationAdmin configurationAdmin;
    protected final ExpiringCache<List<Addon>> cachedRemoteAddons = new ExpiringCache<>(Duration.ofMinutes(15),
            this::getRemoteAddons);
    protected final AddonInfoRegistry addonInfoRegistry;
    protected List<Addon> cachedAddons = List.of();
    protected List<String> installedAddonIds = List.of();
    private final Logger logger = LoggerFactory.getLogger(AbstractRemoteAddonService.class);
    private final ScheduledExecutorService scheduler = ThreadPoolManager.getScheduledPool(THREAD_POOL_NAME_COMMON);
    protected AbstractRemoteAddonService(EventPublisher eventPublisher, ConfigurationAdmin configurationAdmin,
            StorageService storageService, AddonInfoRegistry addonInfoRegistry, String servicePid) {
        this.eventPublisher = eventPublisher;
        this.configurationAdmin = configurationAdmin;
        this.installedAddonStorage = storageService.getStorage(servicePid);
        this.coreVersion = getCoreVersion();
    protected BundleVersion getCoreVersion() {
        return new BundleVersion(FrameworkUtil.getBundle(OpenHAB.class).getVersion().toString());
    private Addon convertFromStorage(Map.Entry<String, @Nullable String> entry) {
        Addon storedAddon = Objects.requireNonNull(gson.fromJson(entry.getValue(), Addon.class));
        AddonInfo addonInfo = addonInfoRegistry.getAddonInfo(storedAddon.getType() + "-" + storedAddon.getId());
        if (addonInfo != null && storedAddon.getConfigDescriptionURI().isBlank()) {
            return Addon.create(storedAddon).withConfigDescriptionURI(addonInfo.getConfigDescriptionURI()).build();
            return storedAddon;
        if (!addonHandlers.stream().allMatch(MarketplaceAddonHandler::isReady)) {
            logger.debug("Add-on service '{}' tried to refresh source before add-on handlers ready. Exiting.",
                    getClass());
        List<Addon> addons = new ArrayList<>();
        // retrieve add-ons that should be available from storage and check if they are really installed
        // this is safe, because the {@link AddonHandler}s only report ready when they installed everything from the
        // cache
            installedAddonStorage.stream().map(this::convertFromStorage).forEach(addon -> {
                setInstalled(addon);
                addons.add(addon);
        } catch (JsonSyntaxException e) {
            List.copyOf(installedAddonStorage.getKeys()).forEach(installedAddonStorage::remove);
                    "Failed to read JSON database, trying to purge it. You might need to re-install {} from the '{}' service.",
                    installedAddonStorage.getKeys(), getId());
            refreshSource();
        // remove not installed add-ons from the add-ons list, but remember their UIDs to re-install them
        List<String> missingAddons = addons.stream().filter(addon -> !addon.isInstalled()).map(Addon::getUid).toList();
        missingAddons.forEach(installedAddonStorage::remove);
        addons.removeIf(addon -> missingAddons.contains(addon.getUid()));
        // create lookup list to make sure installed addons take precedence
        List<String> currentAddonIds = addons.stream().map(Addon::getUid).toList();
        // get the remote addons
        if (remoteEnabled()) {
            List<Addon> remoteAddons = Objects.requireNonNullElse(cachedRemoteAddons.getValue(), List.of());
            remoteAddons.stream().filter(a -> !currentAddonIds.contains(a.getUid())).forEach(addon -> {
        // remove incompatible add-ons if not enabled
        boolean showIncompatible = includeIncompatible();
        addons.removeIf(addon -> !addon.isInstalled() && !addon.getCompatible() && !showIncompatible);
        // check and remove duplicate uids
        Map<String, List<Addon>> addonMap = new HashMap<>();
        addons.forEach(a -> addonMap.computeIfAbsent(a.getUid(), k -> new ArrayList<>()).add(a));
        for (List<Addon> partialAddonList : addonMap.values()) {
            if (partialAddonList.size() > 1) {
                partialAddonList.stream().sorted(BY_COMPATIBLE_AND_VERSION).skip(1).forEach(addons::remove);
        cachedAddons = addons;
        this.installedAddonIds = currentAddonIds;
        if (!missingAddons.isEmpty()) {
            logger.info("Re-installing missing add-ons from remote repository: {}", missingAddons);
            scheduler.execute(() -> missingAddons.forEach(this::install));
    private void setInstalled(Addon addon) {
        addon.setInstalled(addonHandlers.stream().anyMatch(h -> h.isInstalled(addon.getUid())));
     * Add a {@link MarketplaceAddonHandler} to this service
     * This needs to be implemented by the addon-services because the handlers are references to OSGi services and
     * the @Reference annotation is not inherited.
     * It is added here to make sure that implementations comply with that.
     * @param handler the handler that shall be added
    protected abstract void addAddonHandler(MarketplaceAddonHandler handler);
     * Remove a {@link MarketplaceAddonHandler} from this service
     * unbind methods can't be inherited.
     * @param handler the handler that shall be removed
    protected abstract void removeAddonHandler(MarketplaceAddonHandler handler);
     * get all addons from remote
     * @return a list of {@link Addon} that are available on the remote side
    protected abstract List<Addon> getRemoteAddons();
        return cachedAddons;
        Addon addon = getAddon(id, null);
        if (addon == null) {
            postFailureEvent(id, "Add-on can't be installed because it is not known.");
        for (MarketplaceAddonHandler handler : addonHandlers) {
            if (handler.supports(addon.getType(), addon.getContentType())) {
                if (!handler.isInstalled(addon.getUid())) {
                        handler.install(addon);
                        addon.setInstalled(true);
                        installedAddonStorage.put(id, gson.toJson(addon));
                        cachedRemoteAddons.invalidateValue();
                        postInstalledEvent(addon.getUid());
                        postFailureEvent(addon.getUid(), e.getMessage());
                    postFailureEvent(addon.getUid(), "Add-on is already installed.");
        postFailureEvent(id, "Add-on can't be installed because there is no handler for it.");
            postFailureEvent(id, "Add-on can't be uninstalled because it is not known.");
                if (handler.isInstalled(addon.getUid())) {
                        handler.uninstall(addon);
                        installedAddonStorage.remove(id);
                        postUninstalledEvent(addon.getUid());
                    postFailureEvent(addon.getUid(), "Add-on is not installed.");
        postFailureEvent(id, "Add-on can't be uninstalled because there is no handler for it.");
     * check if remote services are enabled
     * @return true if network access is allowed
    protected boolean remoteEnabled() {
            Configuration configuration = configurationAdmin.getConfiguration("org.openhab.addons", null);
            Dictionary<String, Object> properties = configuration.getProperties();
            if (properties == null) {
                // if we can't determine a set property, we use true (default is remote enabled)
            return ConfigParser.valueAsOrElse(properties.get(CONFIG_REMOTE_ENABLED), Boolean.class, true);
    protected boolean includeIncompatible() {
                // if we can't determine a set property, we use false (default is show compatible only)
            return ConfigParser.valueAsOrElse(properties.get(CONFIG_INCLUDE_INCOMPATIBLE), Boolean.class, false);
    private void postInstalledEvent(String extensionId) {
        Event event = AddonEventFactory.createAddonInstalledEvent(extensionId);
        eventPublisher.post(event);
    private void postUninstalledEvent(String extensionId) {
        Event event = AddonEventFactory.createAddonUninstalledEvent(extensionId);
    private void postFailureEvent(String extensionId, @Nullable String msg) {
        Event event = AddonEventFactory.createAddonFailureEvent(extensionId, msg);
