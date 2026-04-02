 * The {@link FirmwareUpdateProgressInfoEvent} is sent if there is a new progress step for a firmware update. It is
 * created by the {@link FirmwareEventFactory}.
public final class FirmwareUpdateProgressInfoEvent extends AbstractEvent {
    /** Constant for the firmware update progress info event type. */
    public static final String TYPE = FirmwareUpdateProgressInfoEvent.class.getSimpleName();
    private final FirmwareUpdateProgressInfo progressInfo;
     * @param progressInfo the progress info to be sent with the event
    protected FirmwareUpdateProgressInfoEvent(String topic, String payload, FirmwareUpdateProgressInfo progressInfo) {
        this.progressInfo = progressInfo;
     * Returns the {@link FirmwareUpdateProgressInfo}.
     * @return the firmware update progress info
    public FirmwareUpdateProgressInfo getProgressInfo() {
        return progressInfo;
        result = prime * result + ((progressInfo == null) ? 0 : progressInfo.hashCode());
        FirmwareUpdateProgressInfoEvent other = (FirmwareUpdateProgressInfoEvent) obj;
        if (progressInfo == null) {
            if (other.progressInfo != null) {
        } else if (!progressInfo.equals(other.progressInfo)) {
        String stepName = progressInfo.getProgressStep() == null ? null : progressInfo.getProgressStep().name();
        return String.format("The firmware update progress for thing %s changed. Step: %s Progress: %d.",
                progressInfo.getThingUID(), stepName, progressInfo.getProgress());
