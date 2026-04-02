import org.openhab.core.config.core.xml.AbstractXmlConfigDescriptionProvider;
import org.openhab.core.config.core.xml.osgi.XmlDocumentProvider;
 * The {@link AddonInfoXmlProvider} is responsible managing any created
 * objects by a {@link AddonInfoReader} for a certain bundle.
 * This implementation registers each {@link AddonInfo} object at the {@link XmlAddonInfoProvider} which is itself
 * registered as {@link org.openhab.core.addon.AddonInfoProvider AddonInfoProvider} service at the <i>OSGi</i> service
 * registry.
 * If there is a {@link ConfigDescription} object within the {@link AddonInfoXmlResult} object, it is added to the
 * {@link AbstractXmlConfigDescriptionProvider} which is itself registered as <i>OSGi</i> service at the service
public class AddonInfoXmlProvider implements XmlDocumentProvider<AddonInfoXmlResult> {
    private Logger logger = LoggerFactory.getLogger(AddonInfoXmlProvider.class);
    private final Bundle bundle;
    private final XmlAddonInfoProvider addonInfoProvider;
    private final AbstractXmlConfigDescriptionProvider configDescriptionProvider;
    public AddonInfoXmlProvider(Bundle bundle, XmlAddonInfoProvider addonInfoProvider,
            AbstractXmlConfigDescriptionProvider configDescriptionProvider) throws IllegalArgumentException {
        this.bundle = bundle;
        this.addonInfoProvider = addonInfoProvider;
        this.configDescriptionProvider = configDescriptionProvider;
    public synchronized void addingObject(AddonInfoXmlResult addonInfoXmlResult) {
        ConfigDescription configDescription = addonInfoXmlResult.configDescription();
                configDescriptionProvider.add(bundle, configDescription);
            } catch (Exception ex) {
                logger.error("Could not register ConfigDescription!", ex);
        AddonInfo addonInfo = AddonInfo.builder(addonInfoXmlResult.addonInfo())
                .withSourceBundle(bundle.getSymbolicName()).build();
        addonInfoProvider.add(bundle, addonInfo);
    public void addingFinished() {
    public synchronized void release() {
        this.addonInfoProvider.removeAll(bundle);
        this.configDescriptionProvider.removeAll(bundle);
