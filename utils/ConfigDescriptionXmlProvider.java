 * The {@link ConfigDescriptionXmlProvider} is responsible managing any created
 * objects by a {@link ConfigDescriptionReader} for a certain bundle.
 * This implementation registers each {@link ConfigDescription} object at the
 * {@link AbstractXmlConfigDescriptionProvider} which
 * is itself registered as {@link ConfigDescriptionProvider} service at the <i>OSGi</i> service registry.
public class ConfigDescriptionXmlProvider implements XmlDocumentProvider<List<ConfigDescription>> {
    public ConfigDescriptionXmlProvider(Bundle bundle, AbstractXmlConfigDescriptionProvider configDescriptionProvider)
    public synchronized void addingObject(List<ConfigDescription> configDescriptions) {
        this.configDescriptionProvider.addAll(bundle, configDescriptions);
