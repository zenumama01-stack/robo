 * The {@link YamlElementName} is a required annotation for the inheritors of {@link YamlElement}. It specifies the root
 * element name in a YAML model that is described by the respective class. Code review MUST ensure that element names
 * are unique.
public @interface YamlElementName {
    String value();
