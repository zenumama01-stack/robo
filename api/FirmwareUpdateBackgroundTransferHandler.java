import org.openhab.core.thing.firmware.FirmwareStatus;
 * The {@link FirmwareUpdateBackgroundTransferHandler} is an extension of the {@link FirmwareUpdateHandler} and is to be
 * used if the firmware of the thing must be transferred to the actual device in the background. So the
 * {@link FirmwareUpdateService} checks if there is a newer firmware available and handles the firmware status of the
 * thing as {@link FirmwareStatus#UPDATE_AVAILABLE} in case of the handler returns false for
 * {@link FirmwareUpdateHandler#isUpdateExecutable()}. Finally it invokes the
 * {@link FirmwareUpdateBackgroundTransferHandler#transferFirmware(Firmware)} operation for this scenario.
public interface FirmwareUpdateBackgroundTransferHandler extends FirmwareUpdateHandler {
     * Transfers the firmware of the thing to its actual device in the background. After the successful transfer of the
     * firmware the operation {@link FirmwareUpdateHandler#isUpdateExecutable()} should return true.
     * @param firmware the firmware to be transferred in the background (not null)
    void transferFirmware(Firmware firmware);
