package org.openhab.core.config.core.xml.internal;
 * The {@link ConfigDescriptionReader} reads XML documents, which contain the {@code config-descriptions} XML tag, and
 * converts them to {@link List} objects consisting of {@link ConfigDescription} objects.
public class ConfigDescriptionReader extends XmlDocumentReader<List<ConfigDescription>> {
    public ConfigDescriptionReader() {
        ClassLoader classLoader = ConfigDescriptionReader.class.getClassLoader();
        xstream.alias("config-descriptions", List.class);
