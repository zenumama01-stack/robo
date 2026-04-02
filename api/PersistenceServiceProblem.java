 * This is a representation of a persistence service configuration problem.
 * @param reason description for the persistence configuration problem, free text, but one of the public constants can
 *            be used to allow the consumer (e.g. UI) to tailor the message.
 * @param serviceId persistence service
 * @param items list of persistence item definitions
 * @param editable true if this is a managed service
 * @author Mark Herwege - Persistence health API endpoint
public record PersistenceServiceProblem(String reason, @Nullable String serviceId, @Nullable List<String> items,
    // Reasons for persistence configuration problems.
    // If one of these constants is used as reason, the UI can use these values to give a more descriptive message.
    public static final String PERSISTENCE_DUPLICATE_CONFIG = "PERSISTENCE_DUPLICATE_CONFIG";
    public static final String PERSISTENCE_NO_DEFAULT = "PERSISTENCE_NO_DEFAULT";
    public static final String PERSISTENCE_NO_CONFIG = "PERSISTENCE_SERVICE_NO_CONFIG";
    public static final String PERSISTENCE_NO_ITEMS = "PERSISTENCE_SERVICE_NO_ITEMS";
    public static final String PERSISTENCE_NO_STRATEGY = "PERSISTENCE_SERVICE_ITEMS_NO_STRATEGY";
    public static final String PERSISTENCE_NO_STORE_STRATEGY = "PERSISTENCE_SERVICE_ITEMS_NO_STORE_STRATEGY";
