 * This is a central service for accessing {@link PersistenceService}s. It is registered through DS and also provides
 * config options for the UI.
@Component(immediate = true, configurationPid = "org.openhab.persistence", //
        property = Constants.SERVICE_PID + "=org.openhab.persistence")
@ConfigurableService(category = "system", label = "Persistence", description_uri = PersistenceServiceRegistryImpl.CONFIG_URI)
public class PersistenceServiceRegistryImpl implements ConfigOptionProvider, PersistenceServiceRegistry {
    protected static final String CONFIG_URI = "system:persistence";
    private static final String CONFIG_DEFAULT = "default";
    private final Map<String, PersistenceService> persistenceServices = new HashMap<>();
    private @Nullable String defaultServiceId;
    public void addPersistenceService(PersistenceService persistenceService) {
        persistenceServices.put(persistenceService.getId(), persistenceService);
    public void removePersistenceService(PersistenceService persistenceService) {
        persistenceServices.remove(persistenceService.getId());
        defaultServiceId = (String) config.get(CONFIG_DEFAULT);
    public @Nullable PersistenceService getDefault() {
        return get(getDefaultId());
    public @Nullable PersistenceService get(@Nullable String serviceId) {
        return (serviceId != null) ? persistenceServices.get(serviceId) : null;
    public @Nullable String getDefaultId() {
        if (defaultServiceId != null) {
            return defaultServiceId;
            // if there is exactly one service available in the system, we assume that this should be used, if no
            // default is specifically configured.
            return (persistenceServices.size() == 1) ? persistenceServices.keySet().iterator().next() : null;
    public Set<PersistenceService> getAll() {
        return new HashSet<>(persistenceServices.values());
        if (CONFIG_URI.equals(uri.toString()) && CONFIG_DEFAULT.equals(param)) {
            Set<ParameterOption> options = new HashSet<>();
            for (PersistenceService service : getAll()) {
                options.add(new ParameterOption(service.getId(), service.getLabel(locale)));
