import java.util.concurrent.ExecutorService;
import org.openhab.core.config.core.status.events.ConfigStatusInfoEvent;
import org.openhab.core.util.BundleResolver;
 * The {@link ConfigStatusService} provides the {@link ConfigStatusInfo} for a specific entity. For this purpose
 * it loops over all registered {@link ConfigStatusProvider}s and returns the {@link ConfigStatusInfo} for the matching
 * {@link ConfigStatusProvider}.
 * @author Chris Jackson - Allow null messages
 * @author Markus Rathgeb - Add locale provider support
@Component(immediate = true, service = ConfigStatusService.class)
public final class ConfigStatusService implements ConfigStatusCallback {
    private final Logger logger = LoggerFactory.getLogger(ConfigStatusService.class);
    private final BundleResolver bundleResolver;
    private final List<ConfigStatusProvider> configStatusProviders = new CopyOnWriteArrayList<>();
    private final ExecutorService executorService = ThreadPoolManager
            .getPool(ConfigStatusService.class.getSimpleName());
    public ConfigStatusService(final @Reference EventPublisher eventPublisher, //
            final @Reference LocaleProvider localeProvider, //
            final @Reference TranslationProvider i18nProvider, //
            final @Reference BundleResolver bundleResolver) {
        this.translationProvider = i18nProvider;
        this.bundleResolver = bundleResolver;
     * Retrieves the {@link ConfigStatusInfo} of the entity by using the registered
     * {@link ConfigStatusProvider} that supports the given entity.
     * @param entityId the id of the entity whose configuration status information is to be retrieved (must not
     *            be null or empty)
     * @param locale the locale to be used for the corresponding configuration status messages; if null then the
     *            default local will be used
     * @return the {@link ConfigStatusInfo} or null if there is no {@link ConfigStatusProvider} registered that
     *         supports the given entity
     * @throws IllegalArgumentException if given entityId is null or empty
    public @Nullable ConfigStatusInfo getConfigStatus(String entityId, final @Nullable Locale locale) {
        if (entityId == null || entityId.isEmpty()) {
            throw new IllegalArgumentException("EntityId must not be null or empty");
        final Locale loc = locale != null ? locale : localeProvider.getLocale();
        for (ConfigStatusProvider configStatusProvider : configStatusProviders) {
            if (configStatusProvider.supportsEntity(entityId)) {
                return getConfigStatusInfo(configStatusProvider, entityId, loc);
        logger.debug("There is no config status provider for entity {} available.", entityId);
    public void configUpdated(final ConfigStatusSource configStatusSource) {
        executorService.submit(() -> {
            final ConfigStatusInfo info = getConfigStatus(configStatusSource.entityId, null);
                eventPublisher.post(new ConfigStatusInfoEvent(configStatusSource.getTopic(), info));
    private @Nullable ConfigStatusInfo getConfigStatusInfo(ConfigStatusProvider configStatusProvider, String entityId,
        Collection<ConfigStatusMessage> configStatusMessages = configStatusProvider.getConfigStatus();
            logger.debug("Cannot provide config status for entity {} because its config status provider returned null.",
                    entityId);
        Bundle bundle = bundleResolver.resolveBundle(configStatusProvider.getClass());
        if (!configStatusMessages.isEmpty()) {
            ConfigStatusInfo info = new ConfigStatusInfo();
                String message = null;
                if (configStatusMessage.messageKey != null) {
                    message = translationProvider.getText(bundle, configStatusMessage.messageKey, null, locale,
                            configStatusMessage.arguments);
                                "No translation found for key {} and config status provider {}. Will ignore the config status message.",
                                configStatusMessage.messageKey, configStatusProvider.getClass().getSimpleName());
                info.add(new ConfigStatusMessage(configStatusMessage.parameterName, configStatusMessage.type, message,
                        configStatusMessage.statusCode));
    protected void addConfigStatusProvider(ConfigStatusProvider configStatusProvider) {
        configStatusProvider.setConfigStatusCallback(this);
        configStatusProviders.add(configStatusProvider);
    protected void removeConfigStatusProvider(ConfigStatusProvider configStatusProvider) {
        configStatusProvider.setConfigStatusCallback(null);
        configStatusProviders.remove(configStatusProvider);
