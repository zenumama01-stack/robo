 * The {@link AddonParameterConverter} is a concrete implementation of the {@code XStream}
 * interface used to convert add-on discovery method parameter information within an XML document into a
 * {@link org.openhab.core.addon.AddonMatchProperty} object.
public class AddonParameterConverter extends GenericUnmarshaller<AddonParameter> {
    public AddonParameterConverter() {
        super(AddonParameter.class);
        String value = requireNonEmpty((String) nodeIterator.nextValue("value", true), "Value is null or empty");
        return new AddonParameter(name, value);
