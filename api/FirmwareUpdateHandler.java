 * The {@link FirmwareUpdateHandler} can be implemented and registered as an OSGi service in order to update the
 * firmware for the physical device of a {@link Thing}. The {@link FirmwareUpdateService} tracks each firmware
 * update handler and starts the firmware update process by the operation
 * {@link FirmwareUpdateService#updateFirmware(ThingUID, String, Locale)}.
public interface FirmwareUpdateHandler {
     * Returns the {@link Thing} that is handled by this firmware update handler.
     * @return the thing that is handled by this firmware update handler (not null)
     * Updates the firmware for the physical device of the thing that is handled by this firmware update handler.
     * @param firmware the new firmware to be updated (not null)
     * @param progressCallback the progress callback to send progress information of the firmware update process (not
    void updateFirmware(Firmware firmware, ProgressCallback progressCallback);
     * Cancels a previous started firmware update.
    void cancel();
     * Returns true, if this firmware update handler is in a state in which the firmware update can be executed,
     * otherwise false (e.g. the thing is {@link ThingStatus#OFFLINE} or its status detail is already
     * {@link ThingStatusDetail#FIRMWARE_UPDATING}.)
     * @return true, if this firmware update handler is in a state in which the firmware update can be executed,
     *         otherwise false
    boolean isUpdateExecutable();
