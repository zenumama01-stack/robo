public interface ModelRepositoryChangeListener {
     * Performs dispatch of all binding configs and
     * fires all {@link org.openhab.core.items.ItemRegistryChangeListener}s if {@code modelName} ends with "items".
    void modelChanged(String modelName, EventType type);
