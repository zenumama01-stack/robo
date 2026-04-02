 * Interface for classes that instances provide an identifier.
public interface Identifiable<@NonNull T> {
     * Get the unique identifier.
     * @return the unique identifier
    T getUID();
