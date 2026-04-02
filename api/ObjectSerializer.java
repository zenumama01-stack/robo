 * A generic interface for serializers that serialize specific object types like Things, Items, Rules etc. into a
 * serialized representation that is written to an {@link OutputStream}.
public interface ObjectSerializer<T> {
    String getGeneratedFormat();
     * Generate the format for all data that were associated to the provided identifier.
     * @param id the identifier of the format generation.
     * @param out The {@link OutputStream} to write to.
    void generateFormat(String id, OutputStream out);
