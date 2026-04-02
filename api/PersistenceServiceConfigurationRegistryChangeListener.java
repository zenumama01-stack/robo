 * The {@link PersistenceServiceConfigurationRegistryChangeListener} is an interface that can be implemented by services
 * that need to listen to the {@link PersistenceServiceConfigurationRegistry} when more than one registry with different
 * types is used.
public interface PersistenceServiceConfigurationRegistryChangeListener {
     * Notifies the listener that a single element has been added.
     * @param element the element that has been added
    void added(PersistenceServiceConfiguration element);
     * Notifies the listener that a single element has been removed.
     * @param element the element that has been removed
    void removed(PersistenceServiceConfiguration element);
     * Notifies the listener that a single element has been updated.
     * @param element the new element
     * @param oldElement the element that has been updated
    void updated(PersistenceServiceConfiguration oldElement, PersistenceServiceConfiguration element);
