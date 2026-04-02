 * This is the interface for a central service that provides access to {@link PersistenceService}s.
public interface PersistenceServiceRegistry {
     * Get the default persistence service.
     * @return the default {@link PersistenceService}
    PersistenceService getDefault();
     * Get the persistence service with the given id.
     * @param serviceId the service id
     * @return the {@link PersistenceService} with the given id
    PersistenceService get(@Nullable String serviceId);
     * Get the id of the default persistence service.
     * @return the id of the default {@link PersistenceService}
    String getDefaultId();
     * Returns all available persistence services.
     * @return a set of all available {@link PersistenceService}s
    Set<PersistenceService> getAll();
