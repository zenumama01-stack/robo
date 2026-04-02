 * The {@link YamlModelListener} interface is responsible for managing a particular model type
 * with data processed from YAML configuration files having a version that is supported by this manager.
 * Implementors are notified whenever a YAML model changed that contains elements of the given type. They MUST declare
 * at least {@link YamlModelListener} as service to automatically register them with the repository.
 * @author Laurent Garnier - Added basic version management
public interface YamlModelListener<T extends YamlElement> {
     * Method called by the model repository when elements from a model are added. Only added elements are contained in
     * the collection. In case the listener is added after a model was read, this method is also called with all
     * elements from that model.
     * @param modelName the name of the model
     * @param elements the collection of added elements
    void addedModel(String modelName, Collection<T> elements);
     * Method called by the model repository when elements from a model are updated. Only changed elements are contained
     * in the collection.
     * @param elements the collection of updated elements
    void updatedModel(String modelName, Collection<T> elements);
     * Method called by the model repository when elements from a model are removed.
     * @param elements the collection of removed elements
    void removedModel(String modelName, Collection<T> elements);
     * Get the DTO class to be used for each object of this model type. The DTO class MUST implement {@link YamlElement}
     * and fulfill all requirements defined for the interface.
     * @return the DTO element class
    Class<T> getElementClass();
     * Indicates whether a YAML file version is supported.
     * @param version the YAML file version to check
     * @return true if the version is supported
    boolean isVersionSupported(int version);
     * Indicates whether this manager is deprecated.
     * @return true if this manager is deprecated
    boolean isDeprecated();
