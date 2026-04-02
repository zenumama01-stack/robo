import org.openhab.core.thing.firmware.FirmwareUpdateProgressInfo;
import org.openhab.core.thing.firmware.FirmwareUpdateResult;
import org.openhab.core.thing.firmware.FirmwareUpdateResultInfo;
 * The callback implementation for the {@link ProgressCallback}.
 * @author Christoph Knauf - Introduced pending, canceled, update and InternalState
 * @author Dimitar Ivanov - Callback contains firmware domain object
final class ProgressCallbackImpl implements ProgressCallback {
    private static final String UPDATE_CANCELED_MESSAGE_KEY = "update-canceled";
     * Handler instance is needed to retrieve the error messages from the correct bundle.
    private final FirmwareUpdateHandler firmwareUpdateHandler;
    private final Firmware firmware;
    private final Locale locale;
    private Collection<ProgressStep> sequence;
    private Iterator<ProgressStep> progressIterator;
    private ProgressStep current;
    private Integer progress;
    private enum InternalState {
        FINISHED,
        RUNNING,
        INITIALIZED
    private InternalState state;
    ProgressCallbackImpl(FirmwareUpdateHandler firmwareUpdateHandler, EventPublisher eventPublisher,
            TranslationProvider i18nProvider, BundleResolver bundleResolver, ThingUID thingUID, Firmware firmware,
        this.firmwareUpdateHandler = firmwareUpdateHandler;
        ParameterChecks.checkNotNull(eventPublisher, "Event publisher");
        ParameterChecks.checkNotNull(i18nProvider, "i18n provider");
        ParameterChecks.checkNotNull(firmware, "Firmware");
        this.firmware = firmware;
        this.locale = locale;
    public void defineSequence(ProgressStep... sequence) {
        if (sequence == null || sequence.length == 0) {
        this.sequence = List.of(sequence);
        progressIterator = this.sequence.iterator();
        this.state = InternalState.INITIALIZED;
    public void next() {
        if (this.state == InternalState.FINISHED) {
            throw new IllegalStateException("Update is finished.");
        if (this.state == InternalState.PENDING) {
            state = InternalState.RUNNING;
            postProgressInfoEvent();
        } else if (progressIterator.hasNext()) {
            this.current = progressIterator.next();
            state = InternalState.FINISHED;
            throw new IllegalStateException("There is no further progress step to be executed.");
    public void failed(String errorMessageKey, Object... arguments) {
        if (errorMessageKey == null || errorMessageKey.isEmpty()) {
            throw new IllegalArgumentException("The error message key must not be null or empty.");
        this.state = InternalState.FINISHED;
        String errorMessage = getMessage(firmwareUpdateHandler.getClass(), errorMessageKey, arguments);
        postResultInfoEvent(FirmwareUpdateResult.ERROR, errorMessage);
    public void success() {
        if ((this.progress == null || this.progress < 100)
                && (this.progressIterator == null || progressIterator.hasNext())) {
                    "Update can't be successfully finished until progress is 100% or last progress step is reached");
        postResultInfoEvent(FirmwareUpdateResult.SUCCESS, null);
    public void pending() {
        this.state = InternalState.PENDING;
    public void canceled() {
        String cancelMessage = getMessage(this.getClass(), UPDATE_CANCELED_MESSAGE_KEY);
        postResultInfoEvent(FirmwareUpdateResult.CANCELED, cancelMessage);
    public void update(int progress) {
        if (this.progress == null) {
            updateProgress(progress);
        } else if (progress < this.progress) {
            throw new IllegalArgumentException("The new progress must not be smaller than the old progress.");
        } else if (this.progress != progress) {
    private void updateProgress(int progress) {
        this.state = InternalState.RUNNING;
    void failedInternal(String errorMessageKey) {
        String errorMessage = getMessage(ProgressCallbackImpl.class, errorMessageKey);
    private String getMessage(Class<?> clazz, String errorMessageKey, Object... arguments) {
        return i18nProvider.getText(bundle, errorMessageKey, null, locale, arguments);
    private void postResultInfoEvent(FirmwareUpdateResult result, String message) {
        post(FirmwareEventFactory.createFirmwareUpdateResultInfoEvent(
                FirmwareUpdateResultInfo.createFirmwareUpdateResultInfo(thingUID, result, message)));
    private void postProgressInfoEvent() {
            post(FirmwareEventFactory.createFirmwareUpdateProgressInfoEvent(
                    FirmwareUpdateProgressInfo.createFirmwareUpdateProgressInfo(thingUID, firmware.getThingTypeUID(),
                            firmware.getVersion(), getCurrentStep(), sequence, this.state == InternalState.PENDING)));
                    FirmwareUpdateProgressInfo.createFirmwareUpdateProgressInfo(thingUID, firmware.getVersion(),
                            getCurrentStep(), sequence, this.state == InternalState.PENDING, progress)));
    private void post(Event event) {
    ProgressStep getCurrentStep() {
        if (current != null) {
        if (sequence != null && progressIterator.hasNext()) {
