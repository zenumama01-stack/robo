 * The {@link XmlDocumentProvider} is responsible managing any created objects
 * by a {@link ConfigDescriptionReader} for a certain module.
 * Each instance of this class is assigned to exactly one module.
public interface XmlDocumentProvider<@NonNull T> {
     * Adds a new result object from the XML processing for further processing.
     * @param object the result object to be processed
    void addingObject(T object);
     * Signals that all available result objects from the XML processing of the
     * certain module have been added.
    void addingFinished();
     * Releases any added result objects from the XML processing.
    void release();
