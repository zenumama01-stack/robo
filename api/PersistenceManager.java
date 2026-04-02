 * The {@link PersistenceManager} interface is used to communicate between external components (e.g. REST interface)
 * that modify persisted data bypassing the {@link org.openhab.core.persistence.internal.PersistenceManagerImpl}. This
 * is required because forecast jobs might need an update when the persisted data changes.
public interface PersistenceManager {
     * External code that updates persisted data, that may have an impact on the persistence logic (restoring item
     * states, forecast logic), should call this method to inform the {@link PersistenceManager} about a potential
     * impact. The {@link PersistenceManager} will query the service again to get the necessary information. This all
     * happens in the calling thread and may therefore take some time. If this is undesired, this call should be
     * performed asynchronously.
     * @param persistenceService the persistence service
     * @param item the item for which persisted data has been updated
    void handleExternalPersistenceDataChange(PersistenceService persistenceService, Item item);
