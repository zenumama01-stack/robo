import org.openhab.core.converter.ObjectParser;
 * {@link ThingParser} is the interface to implement by any {@link Thing} parser.
public interface ThingParser extends ObjectParser<Thing> {
     * Parse the provided {@code syntax} string without impacting the thing registry.
     * @param syntax the syntax in format.
     * @param errors the {@link List} to use to report errors.
     * @param warnings the {@link List} to be used to report warnings.
     * @return The model name used for parsing if the parsing succeeded without errors; {@code null} otherwise.
    String startParsingFormat(String syntax, List<String> errors, List<String> warnings);
     * Get the {@link Thing} objects found when parsing the format.
     * @param modelName the model name used when parsing.
     * @return The {@link Collection} of {@link Thing}s.
    Collection<Thing> getParsedObjects(String modelName);
     * Get the {@link ItemChannelLink} objects found when parsing the format.
     * @return The {@link Collection} of {@link ItemChannelLink}s.
    Collection<ItemChannelLink> getParsedChannelLinks(String modelName);
