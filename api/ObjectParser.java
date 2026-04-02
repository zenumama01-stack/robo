package org.openhab.core.converter;
 * A generic interface for parsers that parse strings into specific object types like Things, Items, Rules etc.
 * @param <T> The object type.
public interface ObjectParser<T> {
     * Get the name of the format.
     * @return The format name.
    String getParserFormat();
     * Parse the provided syntax in format without impacting any object registries.
     * Get the objects found when parsing the format.
     * @param modelName the model name whose objects to get.
     * @return The {@link Collection} of objects.
    Collection<T> getParsedObjects(String modelName);
     * Release the resources from a previously started format parsing.
     * @param modelName the model name whose resources to release.
    void finishParsingFormat(String modelName);
