import org.openhab.core.converter.ObjectSerializer;
 * {@link ThingSerializer} is the interface to implement by any file generator for {@link Thing} object.
public interface ThingSerializer extends ObjectSerializer<Thing> {
     * Specify the {@link List} of {@link Thing}s to serialize and associate them with an identifier.
     * @param id the identifier of the {@link Thing} format generation.
     * @param things the {@link List} of {@link Thing}s to serialize.
     * @param hideDefaultChannels {@code true} to hide the non extensible channels having a default configuration.
     * @param hideDefaultParameters {@code true} to hide the configuration parameters having a default value.
    void setThingsToBeSerialized(String id, List<Thing> things, boolean hideDefaultChannels,
            boolean hideDefaultParameters);
