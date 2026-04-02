 * The {@link FirmwareUpdateResultInfoEvent} is sent if the firmware update has been finished. It is created by the
 * {@link FirmwareEventFactory}.
public final class FirmwareUpdateResultInfoEvent extends AbstractEvent {
    /** Constant for the firmware update result info event type. */
    public static final String TYPE = FirmwareUpdateResultInfoEvent.class.getSimpleName();
    private final FirmwareUpdateResultInfo firmwareUpdateResultInfo;
     * @param firmwareUpdateResultInfo the firmware update result info to be sent as event
    protected FirmwareUpdateResultInfoEvent(String topic, String payload,
        this.firmwareUpdateResultInfo = firmwareUpdateResultInfo;
     * Returns the firmware update result info.
     * @return the firmware update result info
    public FirmwareUpdateResultInfo getFirmwareUpdateResultInfo() {
        return firmwareUpdateResultInfo;
        result = prime * result + ((firmwareUpdateResultInfo == null) ? 0 : firmwareUpdateResultInfo.hashCode());
        FirmwareUpdateResultInfoEvent other = (FirmwareUpdateResultInfoEvent) obj;
        if (firmwareUpdateResultInfo == null) {
            if (other.firmwareUpdateResultInfo != null) {
        } else if (!firmwareUpdateResultInfo.equals(other.firmwareUpdateResultInfo)) {
        FirmwareUpdateResult result = firmwareUpdateResultInfo.getResult();
        StringBuilder sb = new StringBuilder(String.format("The result of the firmware update for thing %s is %s.",
                firmwareUpdateResultInfo.getThingUID(), result.name()));
        if (result == FirmwareUpdateResult.ERROR) {
            sb.append(String.format(" The error message is %s.", firmwareUpdateResultInfo.getErrorMessage()));
