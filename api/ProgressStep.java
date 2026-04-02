 * The {@link ProgressStep} enumeration defines the possible progress steps for a firmware update. The actual sequence
 * of the firmware update is defined by the operation {@link ProgressCallback#defineSequence(ProgressStep...)}.
 * @author Chris Jackson - Add WAITING
public enum ProgressStep {
     * The {@link FirmwareUpdateHandler} is going to download / read the firmware image by reading the input stream from
     * {@link Firmware#getBytes()}.
    DOWNLOADING,
     * The {@link FirmwareUpdateHandler} is waiting for the device to initiate the transfer. For battery devices that
     * may wake up periodically, this may take some time. For mains devices this step may be very short or omitted.
    WAITING,
    /** The {@link FirmwareUpdateHandler} is going to transfer the firmware to the actual device. */
    TRANSFERRING,
    /** The {@link FirmwareUpdateHandler} is going to trigger the firmware update for the actual device. */
    UPDATING,
    /** The {@link FirmwareUpdateHandler} is going to reboot the device. */
    REBOOTING
