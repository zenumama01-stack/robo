import static org.openhab.core.thing.firmware.FirmwareStatusInfo.*;
import org.openhab.core.thing.binding.firmware.FirmwareUpdateBackgroundTransferHandler;
import org.openhab.core.thing.firmware.FirmwareEventFactory;
 * Default implementation of {@link FirmwareUpdateService}.
 * @author Dimitar Ivanov - update and cancel operations are run with different safe caller identifiers in order to
 *         execute asynchronously; Firmware update is done for thing
@Component(immediate = true, service = { EventSubscriber.class, FirmwareUpdateService.class })
public final class FirmwareUpdateServiceImpl implements FirmwareUpdateService, EventSubscriber {
    private static final String THREAD_POOL_NAME = FirmwareUpdateServiceImpl.class.getSimpleName();
    private static final Set<String> SUPPORTED_TIME_UNITS = Set.of(TimeUnit.SECONDS.name(), TimeUnit.MINUTES.name(),
            TimeUnit.HOURS.name(), TimeUnit.DAYS.name());
    protected static final String PERIOD_CONFIG_KEY = "period";
    protected static final String DELAY_CONFIG_KEY = "delay";
    protected static final String TIME_UNIT_CONFIG_KEY = "timeUnit";
    private static final URI CONFIG_DESC_URI = URI.create("system:firmware-status-info-job");
    private final Logger logger = LoggerFactory.getLogger(FirmwareUpdateServiceImpl.class);
    private int firmwareStatusInfoJobPeriod = 3600;
    private int firmwareStatusInfoJobDelay = 3600;
    private TimeUnit firmwareStatusInfoJobTimeUnit = TimeUnit.SECONDS;
    private @Nullable ScheduledFuture<?> firmwareStatusInfoJob;
    protected int timeout = 30 * 60 * 1000;
    private final Set<String> subscribedEventTypes = Set.of(ThingStatusInfoChangedEvent.TYPE);
    private final Map<ThingUID, FirmwareStatusInfo> firmwareStatusInfoMap = new ConcurrentHashMap<>();
    private final Map<ThingUID, ProgressCallbackImpl> progressCallbackMap = new ConcurrentHashMap<>();
    private final ConfigDescriptionValidator configDescriptionValidator;
    public FirmwareUpdateServiceImpl( //
            final @Reference BundleResolver bundleResolver,
            final @Reference FirmwareRegistry firmwareRegistry, //
            final @Reference SafeCaller safeCaller) {
    private final Runnable firmwareStatusRunnable = new Runnable() {
            logger.debug("Running firmware status check.");
                    logger.debug("Executing firmware status check for thing with UID {}.",
                            firmwareUpdateHandler.getThing().getUID());
                    Firmware latestFirmware = getLatestSuitableFirmware(firmwareUpdateHandler.getThing());
                    FirmwareStatusInfo newFirmwareStatusInfo = getFirmwareStatusInfo(firmwareUpdateHandler,
                            latestFirmware);
                    processFirmwareStatusInfo(firmwareUpdateHandler, newFirmwareStatusInfo, latestFirmware);
                    logger.debug("Exception occurred during firmware status check.", e);
        logger.debug("Modifying the configuration of the firmware update service.");
        if (!isValid(config)) {
        cancelFirmwareUpdateStatusInfoJob();
        firmwareStatusInfoJobPeriod = config.containsKey(PERIOD_CONFIG_KEY) ? (Integer) config.get(PERIOD_CONFIG_KEY)
                : firmwareStatusInfoJobPeriod;
        firmwareStatusInfoJobDelay = config.containsKey(DELAY_CONFIG_KEY) ? (Integer) config.get(DELAY_CONFIG_KEY)
                : firmwareStatusInfoJobDelay;
        firmwareStatusInfoJobTimeUnit = config.containsKey(TIME_UNIT_CONFIG_KEY)
                ? TimeUnit.valueOf((String) config.get(TIME_UNIT_CONFIG_KEY))
                : firmwareStatusInfoJobTimeUnit;
        if (!firmwareUpdateHandlers.isEmpty()) {
            createFirmwareUpdateStatusInfoJob();
        firmwareStatusInfoMap.clear();
        progressCallbackMap.clear();
    public @Nullable FirmwareStatusInfo getFirmwareStatusInfo(ThingUID thingUID) {
        ParameterChecks.checkNotNull(thingUID, "Thing UID");
            logger.trace("No firmware update handler available for thing with UID {}.", thingUID);
        FirmwareStatusInfo firmwareStatusInfo = getFirmwareStatusInfo(firmwareUpdateHandler, latestFirmware);
        processFirmwareStatusInfo(firmwareUpdateHandler, firmwareStatusInfo, latestFirmware);
    public void updateFirmware(final ThingUID thingUID, final String firmwareVersion, final @Nullable Locale locale) {
        final FirmwareUpdateHandler firmwareUpdateHandler = getFirmwareUpdateHandler(thingUID);
                    String.format("There is no firmware update handler for thing with UID %s.", thingUID));
        final Thing thing = firmwareUpdateHandler.getThing();
        final Firmware firmware = getFirmware(thing, firmwareVersion);
        validateFirmwareUpdateConditions(firmwareUpdateHandler, firmware);
        final Locale currentLocale = locale != null ? locale : localeProvider.getLocale();
        final ProgressCallbackImpl progressCallback = new ProgressCallbackImpl(firmwareUpdateHandler, eventPublisher,
                i18nProvider, bundleResolver, thingUID, firmware, currentLocale);
        progressCallbackMap.put(thingUID, progressCallback);
        logger.debug("Starting firmware update for thing with UID {} and firmware {}", thingUID, firmware);
        safeCaller.create(firmwareUpdateHandler, FirmwareUpdateHandler.class).withTimeout(timeout).withAsync()
                .onTimeout(() -> {
                    logger.error("Timeout occurred for firmware update of thing with UID {} and firmware {}.", thingUID,
                            firmware);
                    progressCallback.failedInternal("timeout-error");
                }).onException(e -> {
                            "Unexpected exception occurred for firmware update of thing with UID {} and firmware {}.",
                            thingUID, firmware, e.getCause());
                    progressCallback.failedInternal("unexpected-handler-error");
                }).build().updateFirmware(firmware, progressCallback);
    public void cancelFirmwareUpdate(final ThingUID thingUID) {
        final ProgressCallbackImpl progressCallback = getProgressCallback(thingUID);
        logger.debug("Cancelling firmware update for thing with UID {}.", thingUID);
                    logger.error("Timeout occurred while cancelling firmware update of thing with UID {}.", thingUID);
                    progressCallback.failedInternal("timeout-error-during-cancel");
                    logger.error("Unexpected exception occurred while cancelling firmware update of thing with UID {}.",
                            thingUID, e.getCause());
                    progressCallback.failedInternal("unexpected-handler-error-during-cancel");
                }).withIdentifier(new Object()).build().cancel();
        if (event instanceof ThingStatusInfoChangedEvent changedEvent) {
            if (changedEvent.getStatusInfo().getStatus() != ThingStatus.ONLINE) {
            ThingUID thingUID = changedEvent.getThingUID();
            if (firmwareUpdateHandler != null && !firmwareStatusInfoMap.containsKey(thingUID)) {
                initializeFirmwareStatus(firmwareUpdateHandler);
    protected ProgressCallbackImpl getProgressCallback(ThingUID thingUID) {
        final ProgressCallbackImpl entry = progressCallbackMap.get(thingUID);
                    String.format("No ProgressCallback available for thing with UID %s.", thingUID));
    private @Nullable Firmware getLatestSuitableFirmware(Thing thing) {
        if (firmwares != null) {
    private FirmwareStatusInfo getFirmwareStatusInfo(FirmwareUpdateHandler firmwareUpdateHandler,
            @Nullable Firmware latestFirmware) {
        String thingFirmwareVersion = getThingFirmwareVersion(firmwareUpdateHandler);
        ThingUID thingUID = firmwareUpdateHandler.getThing().getUID();
        if (latestFirmware == null || thingFirmwareVersion == null) {
            return createUnknownInfo(thingUID);
        if (latestFirmware.isSuccessorVersion(thingFirmwareVersion)) {
            if (firmwareUpdateHandler.isUpdateExecutable()) {
                return createUpdateExecutableInfo(thingUID, latestFirmware.getVersion());
            return createUpdateAvailableInfo(thingUID);
        return createUpToDateInfo(thingUID);
    private synchronized void processFirmwareStatusInfo(FirmwareUpdateHandler firmwareUpdateHandler,
            FirmwareStatusInfo newFirmwareStatusInfo, @Nullable Firmware latestFirmware) {
        FirmwareStatusInfo previousFirmwareStatusInfo = firmwareStatusInfoMap.put(newFirmwareStatusInfo.getThingUID(),
                newFirmwareStatusInfo);
        if (previousFirmwareStatusInfo == null || !previousFirmwareStatusInfo.equals(newFirmwareStatusInfo)) {
            eventPublisher.post(FirmwareEventFactory.createFirmwareStatusInfoEvent(newFirmwareStatusInfo));
            if (newFirmwareStatusInfo.getFirmwareStatus() == FirmwareStatus.UPDATE_AVAILABLE
                    && firmwareUpdateHandler instanceof FirmwareUpdateBackgroundTransferHandler handler
                    && !firmwareUpdateHandler.isUpdateExecutable()) {
                if (latestFirmware != null) {
                    transferLatestFirmware(handler, latestFirmware, previousFirmwareStatusInfo);
    private void transferLatestFirmware(final FirmwareUpdateBackgroundTransferHandler fubtHandler,
            final Firmware latestFirmware, final @Nullable FirmwareStatusInfo previousFirmwareStatusInfo) {
        getPool().submit(new Runnable() {
                    fubtHandler.transferFirmware(latestFirmware);
                    logger.error("Exception occurred during background firmware transfer.", e);
                        // restore previous firmware status info in order that transfer can be re-triggered
                        if (previousFirmwareStatusInfo == null) {
                            firmwareStatusInfoMap.remove(fubtHandler.getThing().getUID());
                            firmwareStatusInfoMap.put(fubtHandler.getThing().getUID(), previousFirmwareStatusInfo);
    private void validateFirmwareUpdateConditions(FirmwareUpdateHandler firmwareUpdateHandler, Firmware firmware) {
        if (!firmwareUpdateHandler.isUpdateExecutable()) {
            throw new IllegalStateException(String.format("The firmware update of thing with UID %s is not executable.",
                    firmwareUpdateHandler.getThing().getUID()));
        validateFirmwareSuitability(firmware, firmwareUpdateHandler);
    private void validateFirmwareSuitability(Firmware firmware, FirmwareUpdateHandler firmwareUpdateHandler) {
        Thing thing = firmwareUpdateHandler.getThing();
        if (!firmware.isSuitableFor(thing)) {
                    String.format("Firmware %s is not suitable for thing with UID %s.", firmware, thing.getUID()));
    private Firmware getFirmware(Thing thing, String firmwareVersion) {
        Firmware firmware = firmwareRegistry.getFirmware(thing, firmwareVersion);
        if (firmware == null) {
                    "Firmware with version %s for thing with UID %s was not found.", firmwareVersion, thing.getUID()));
    private @Nullable String getThingFirmwareVersion(FirmwareUpdateHandler firmwareUpdateHandler) {
        return firmwareUpdateHandler.getThing().getProperties().get(Thing.PROPERTY_FIRMWARE_VERSION);
    private void createFirmwareUpdateStatusInfoJob() {
        if (firmwareStatusInfoJob == null || firmwareStatusInfoJob.isCancelled()) {
            logger.debug("Creating firmware status info job. [delay:{}, period:{}, time unit: {}]",
                    firmwareStatusInfoJobDelay, firmwareStatusInfoJobPeriod, firmwareStatusInfoJobTimeUnit);
            firmwareStatusInfoJob = getPool().scheduleWithFixedDelay(firmwareStatusRunnable, firmwareStatusInfoJobDelay,
                    firmwareStatusInfoJobPeriod, firmwareStatusInfoJobTimeUnit);
    private void cancelFirmwareUpdateStatusInfoJob() {
        if (firmwareStatusInfoJob != null && !firmwareStatusInfoJob.isCancelled()) {
            logger.debug("Cancelling firmware status info job.");
            firmwareStatusInfoJob.cancel(true);
            firmwareStatusInfoJob = null;
    private boolean isValid(Map<String, Object> config) {
        // the config description validator does not support option value validation at the moment; so we will validate
        // the time unit here
        Object timeUnit = config.get(TIME_UNIT_CONFIG_KEY);
        if (timeUnit == null || !SUPPORTED_TIME_UNITS.contains(timeUnit)) {
            logger.debug("Given time unit {} is not supported. Will keep current configuration.", timeUnit);
            configDescriptionValidator.validate(config, CONFIG_DESC_URI);
            logger.debug("Validation of new configuration values failed. Will keep current configuration.", e);
    private void initializeFirmwareStatus(final FirmwareUpdateHandler firmwareUpdateHandler) {
                FirmwareStatusInfo info = getFirmwareStatusInfo(thingUID);
                logger.debug("Firmware status {} for thing {} initialized.", info.getFirmwareStatus(), thingUID);
                firmwareStatusInfoMap.put(thingUID, info);
    private static ScheduledExecutorService getPool() {
    public int getFirmwareStatusInfoJobPeriod() {
        return firmwareStatusInfoJobPeriod;
    public int getFirmwareStatusInfoJobDelay() {
        return firmwareStatusInfoJobDelay;
    public TimeUnit getFirmwareStatusInfoJobTimeUnit() {
        return firmwareStatusInfoJobTimeUnit;
    protected synchronized void addFirmwareUpdateHandler(FirmwareUpdateHandler firmwareUpdateHandler) {
        if (firmwareUpdateHandlers.isEmpty()) {
    protected synchronized void removeFirmwareUpdateHandler(FirmwareUpdateHandler firmwareUpdateHandler) {
        firmwareStatusInfoMap.remove(firmwareUpdateHandler.getThing().getUID());
        progressCallbackMap.remove(firmwareUpdateHandler.getThing().getUID());
