 * The {@link AddonInfoReader} reads XML documents, which contain the {@code addon} XML tag,
 * and converts them to {@link AddonInfoXmlResult} objects.
 * @author Alex Tugarev - Extended by options and filter criteria
 * @author Chris Jackson - Add parameter groups
public class AddonInfoReader extends XmlDocumentReader<AddonInfoXmlResult> {
    public AddonInfoReader() {
        ClassLoader classLoader = AddonInfoReader.class.getClassLoader();
