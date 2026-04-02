import org.openhab.core.addon.AddonInfoList;
 * The {@link AddonInfoListConverter} is a concrete implementation of the {@code XStream}
 * interface used to convert a list of add-on information within an XML document into a list of {@link AddonInfo}
 * objects.
public class AddonInfoListConverter extends GenericUnmarshaller<AddonInfoList> {
    public AddonInfoListConverter() {
        super(AddonInfoList.class);
        Object object = nodeIterator.nextList("addons", false);
        List<AddonInfo> addons = (object instanceof List<?> list)
                ? list.stream().filter(Objects::nonNull).filter(AddonInfoXmlResult.class::isInstance)
                        .map(e -> (AddonInfoXmlResult) e).map(AddonInfoXmlResult::addonInfo).toList()
        return new AddonInfoList().setAddons(addons);
