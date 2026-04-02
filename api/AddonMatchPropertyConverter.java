 * The {@link AddonMatchPropertyConverter} is a concrete implementation of the {@code XStream}
 * interface used to convert add-on discovery method match property information within an XML document into a
 * {@link AddonMatchProperty} object.
public class AddonMatchPropertyConverter extends GenericUnmarshaller<AddonMatchProperty> {
    public AddonMatchPropertyConverter() {
        super(AddonMatchProperty.class);
        String name = requireNonEmpty((String) nodeIterator.nextValue("name", true), "Name is null or empty");
        String regex = requireNonEmpty((String) nodeIterator.nextValue("regex", true), "Regex is null or empty");
        return new AddonMatchProperty(name, regex);
