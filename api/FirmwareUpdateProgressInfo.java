 * The {@link FirmwareUpdateProgressInfo} represents the progress indicator for a firmware update.
 * @author Christoph Knauf - Added progress and pending
public final class FirmwareUpdateProgressInfo {
    private final String firmwareVersion;
    private final ProgressStep progressStep;
    private final Collection<ProgressStep> sequence;
    private final boolean pending;
    private final @Nullable Integer progress;
    FirmwareUpdateProgressInfo() {
        firmwareVersion = "";
        progressStep = ProgressStep.WAITING;
        sequence = List.of();
        pending = false;
        progress = null;
    private FirmwareUpdateProgressInfo(ThingUID thingUID, String firmwareVersion, ProgressStep progressStep,
            Collection<ProgressStep> sequence, boolean pending, int progress) {
        Objects.requireNonNull(thingUID, "ThingUID must not be null.");
        Objects.requireNonNull(firmwareVersion, "Firmware version must not be null.");
        if (progress < 0 || progress > 100) {
            throw new IllegalArgumentException("The progress must be between 0 and 100.");
        this.progressStep = progressStep;
        this.pending = pending;
        this.progress = progress;
     * Creates a new {@link FirmwareUpdateProgressInfo}.
     * @param thingUID the thing UID of the thing that is updated (must not be null)
     * @param firmwareVersion the version of the firmware that is updated (must not be null)
     * @param progressStep the current progress step (must not be null)
     * @param sequence the collection of progress steps describing the sequence of the firmware update process
     *            (must not be null)
     * @param pending the flag indicating if the update is pending
     * @param progress the progress of the update in percent
     * @return FirmwareUpdateProgressInfo object (not null)
     * @throws IllegalArgumentException if sequence is null or empty or progress is not between 0 and 100
    public static FirmwareUpdateProgressInfo createFirmwareUpdateProgressInfo(ThingUID thingUID, String firmwareVersion,
            ProgressStep progressStep, Collection<ProgressStep> sequence, boolean pending, int progress) {
        return new FirmwareUpdateProgressInfo(thingUID, firmwareVersion, progressStep, sequence, pending, progress);
            Collection<ProgressStep> sequence, boolean pending) {
        if (sequence == null || sequence.isEmpty()) {
            throw new IllegalArgumentException("Sequence must not be null or empty.");
        Objects.requireNonNull(progressStep, "Progress step must not be null.");
        this.progress = null;
     * @throws IllegalArgumentException if sequence is null or empty
    public static FirmwareUpdateProgressInfo createFirmwareUpdateProgressInfo(ThingUID thingUID,
            ThingTypeUID thingTypeUID, String firmwareVersion, ProgressStep progressStep,
        return new FirmwareUpdateProgressInfo(thingUID, firmwareVersion, progressStep, sequence, pending);
     * Returns the firmware version of the firmware that is updated.
     * @return the firmware version of the firmware that is updated (not null)
    public String getFirmwareVersion() {
        return firmwareVersion;
     * Returns the current progress step.
     * @return the current progress step (not null)
    public ProgressStep getProgressStep() {
        return progressStep;
     * Returns the sequence of the firmware update process.
     * @return the sequence (not null)
    public Collection<ProgressStep> getSequence() {
        return sequence;
     * Returns true if the firmware update is pending, false otherwise
     * @return true if pending, false otherwise
    public boolean isPending() {
     * Returns the percentage progress of the firmware update.
     * @return the progress between 0 and 100 or null if no progress was set
    public @Nullable Integer getProgress() {
        result = prime * result + (pending ? 1231 : 1237);
        result = prime * result + progress;
        result = prime * result + ((progressStep == null) ? 0 : progressStep.hashCode());
        result = prime * result + ((sequence == null) ? 0 : sequence.hashCode());
        if (!(obj instanceof FirmwareUpdateProgressInfo)) {
        FirmwareUpdateProgressInfo other = (FirmwareUpdateProgressInfo) obj;
        if (pending != other.pending) {
        if (!progress.equals(other.progress)) {
        if (progressStep != other.progressStep) {
        if (sequence == null) {
            if (other.sequence != null) {
        } else if (!sequence.equals(other.sequence)) {
        return "FirmwareUpdateProgressInfo [thingUID=" + thingUID + ", firmwareVersion=" + firmwareVersion
                + ", progressStep=" + progressStep + ", sequence=" + sequence + ", pending=" + pending + ", progress="
                + progress + "]";
