 * The {@link FirmwareStatusInfoEvent} is sent if the {@link FirmwareStatusInfo} of a {@link Thing} has been changed.
 * It is created by the {@link FirmwareEventFactory}.
 * @author Dimitar Ivanov - Removed thing UID from the event
public final class FirmwareStatusInfoEvent extends AbstractEvent {
    /** Constant for the firmware status info event type. */
    public static final String TYPE = FirmwareStatusInfoEvent.class.getSimpleName();
    private final FirmwareStatusInfo firmwareStatusInfo;
     * @param firmwareStatusInfo the firmware status info to be sent with the event
    protected FirmwareStatusInfoEvent(String topic, String payload, FirmwareStatusInfo firmwareStatusInfo) {
        this.firmwareStatusInfo = firmwareStatusInfo;
     * Returns the firmware status info.
     * @return the firmware status info
    public FirmwareStatusInfo getFirmwareStatusInfo() {
        return firmwareStatusInfo;
        result = prime * result + ((firmwareStatusInfo == null) ? 0 : firmwareStatusInfo.hashCode());
        FirmwareStatusInfoEvent other = (FirmwareStatusInfoEvent) obj;
        if (firmwareStatusInfo == null) {
            if (other.firmwareStatusInfo != null) {
        } else if (!firmwareStatusInfo.equals(other.firmwareStatusInfo)) {
        FirmwareStatus status = firmwareStatusInfo.getFirmwareStatus();
        ThingUID thingUID = firmwareStatusInfo.getThingUID();
        StringBuilder sb = new StringBuilder(
                String.format("Firmware status of thing %s changed to %s.", thingUID, status.name()));
        if (status == FirmwareStatus.UPDATE_EXECUTABLE) {
            sb.append(String.format("The new updatable firmware version is %s.",
                    firmwareStatusInfo.getUpdatableFirmwareVersion()));
