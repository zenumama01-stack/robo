import java.util.regex.PatternSyntaxException;
import org.openhab.core.addon.AddonInfoProvider;
import com.thoughtworks.xstream.XStreamException;
import com.thoughtworks.xstream.converters.ConversionException;
 * The {@link AddonInfoAddonsXmlProvider} reads the {@code runtime/etc/addons.xml} file, which
 * should contain a list of {@code addon} elements, and convert its combined contents into a list
 * of {@link AddonInfo} objects that can be accessed via the {@link AddonInfoProvider} interface.
 * @author Kai Kreuzer - Reduce it to support a single addons.xml file
@Component(service = AddonInfoProvider.class, name = AddonInfoAddonsXmlProvider.SERVICE_NAME)
public class AddonInfoAddonsXmlProvider implements AddonInfoProvider {
    private static final String ADDONS_XML_FILE = "etc" + File.separator + "addons.xml";
    public static final String SERVICE_NAME = "addons-info-provider";
    private final Logger logger = LoggerFactory.getLogger(AddonInfoAddonsXmlProvider.class);
    private final String fileName = OpenHAB.getRuntimeFolder() + File.separator + ADDONS_XML_FILE;
    private final Set<AddonInfo> addonInfos = new HashSet<>();
    public AddonInfoAddonsXmlProvider() {
        initialize();
        testAddonDeveloperRegexSyntax();
        addonInfos.clear();
    public @Nullable AddonInfo getAddonInfo(@Nullable String uid, @Nullable Locale locale) {
        return addonInfos.stream().filter(a -> a.getUID().equals(uid)).findFirst().orElse(null);
        return addonInfos;
    private void initialize() {
        File file = new File(fileName);
            if (!file.isFile()) {
                logger.debug("File '{}' does not exist.", fileName);
        } catch (SecurityException e) {
            logger.warn("File '{}' threw a security exception: {}", fileName, e.getMessage());
        AddonInfoListReader reader = new AddonInfoListReader();
            String xml = Files.readString(file.toPath());
            if (xml != null && !xml.isBlank()) {
                addonInfos.addAll(reader.readFromXML(xml).getAddons());
                logger.warn("File '{}' contents are null or empty", file.getName());
            logger.warn("File '{}' could not be read", file.getName());
        } catch (ConversionException e) {
            logger.warn("File '{}' has invalid content: {}", file.getName(), e.getMessage());
        } catch (XStreamException e) {
            logger.warn("File '{}' could not be deserialized", file.getName());
            logger.warn("File '{}' threw a security exception: {}", file, e.getMessage());
     * The openhab-addons Maven build process checks individual developer addon.xml contributions
     * against the 'addon-1.0.0.xsd' schema, but it can't check the discovery-method match-property
     * regex syntax. Invalid regexes do throw exceptions at run-time, but the log can't identify the
     * culprit addon. Ideally we need to add syntax checks to the Maven build; and this test is an
     * interim solution.
    private void testAddonDeveloperRegexSyntax() {
        List<String> patternErrors = new ArrayList<>();
        for (AddonInfo addonInfo : addonInfos) {
            for (AddonDiscoveryMethod discoveryMethod : addonInfo.getDiscoveryMethods()) {
                for (AddonMatchProperty matchProperty : discoveryMethod.getMatchProperties()) {
                        matchProperty.getPattern();
                    } catch (PatternSyntaxException e) {
                        patternErrors.add(String.format(
                                "Regex syntax error in org.openhab.%s.%s addon.xml => %s in \"%s\" position %d",
                                addonInfo.getType(), addonInfo.getId(), e.getDescription(), e.getPattern(),
                                e.getIndex()));
        if (!patternErrors.isEmpty()) {
            logger.warn("The following errors were found:\n\t{}", String.join("\n\t", patternErrors));
