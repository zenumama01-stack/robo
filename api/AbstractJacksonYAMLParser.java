package org.openhab.core.automation.internal.parser.jackson;
 * Abstract class that can be used by YAML parsers for the different entity types.
 * @author Arne Seime - Initial contribution
public abstract class AbstractJacksonYAMLParser<T> implements Parser<T> {
    /** The YAML object mapper instance */
    protected static final ObjectMapper YAML_MAPPER;
        YAML_MAPPER = new ObjectMapper(new YAMLFactory());
        YAML_MAPPER.findAndRegisterModules();
        for (T dataObject : dataObjects) {
            YAML_MAPPER.writeValue(writer, dataObject);
