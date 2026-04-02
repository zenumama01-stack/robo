import org.openhab.core.config.discovery.addon.AddonFinderService;
 * This service is an implementation of an openHAB {@link AddonFinderService} using the Karaf features
 * service. This service allows dynamic installation/removal of add-on suggestion finders.
@Component(name = "org.openhab.core.karafaddonfinders", immediate = true)
public class KarafAddonFinderService implements AddonFinderService {
    private final FeatureInstaller featureInstaller;
    private boolean deactivated;
    public KarafAddonFinderService(final @Reference FeatureInstaller featureInstaller) {
        this.featureInstaller = featureInstaller;
        deactivated = true;
        if (!deactivated) {
            featureInstaller.addAddon(FeatureInstaller.FINDER_ADDON_TYPE, id);
            featureInstaller.removeAddon(FeatureInstaller.FINDER_ADDON_TYPE, id);
