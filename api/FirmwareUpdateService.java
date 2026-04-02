 * The firmware update service is registered as an OSGi service and is responsible for tracking all available
 * {@link FirmwareUpdateHandler}s. It provides access to the current {@link FirmwareStatusInfo} of a thing and is the
 * central instance to start a firmware update.
public interface FirmwareUpdateService {
     * Returns the {@link FirmwareStatusInfo} for the thing having the given thing UID.
     * @param thingUID the UID of the thing (must not be null)
     * @return the firmware status info (is null if there is no {@link FirmwareUpdateHandler} for the thing
     *         available)
    FirmwareStatusInfo getFirmwareStatusInfo(ThingUID thingUID);
     * Updates the firmware of the thing having the given thing UID by invoking the operation
     * {@link FirmwareUpdateHandler#updateFirmware(Firmware, ProgressCallback)} of the thing´s firmware update handler.
     * This operation is a non-blocking operation by spawning a new thread around the invocation of the firmware update
     * handler. The time out of the thread is 30 minutes.
     * @param thingUID the thing UID (must not be null)
     * @param firmwareVersion the version of the firmware to be updated (must not be null)
     * @param locale the locale to be used to internationalize error messages (if null then the locale provided by the
     *            {@link LocaleProvider} is used)
     *             <li>there is no firmware update handler for the thing</li>
     *             <li>the firmware update handler is not able to execute the firmware update</li>
     * @throws IllegalArgumentException if
     *             <li>the firmware cannot be found</li>
     *             <li>the firmware is not suitable for the thing</li>
     *             <li>the firmware requires another prerequisite firmware version</li>
    void updateFirmware(final ThingUID thingUID, final String firmwareVersion, @Nullable final Locale locale);
     * Cancels the firmware update of the thing having the given thing UID by invoking the operation
     * {@link FirmwareUpdateHandler#cancel()} of the thing´s firmware update handler.
    void cancelFirmwareUpdate(final ThingUID thingUID);
