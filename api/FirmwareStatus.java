 * The {@link FirmwareStatus} enumeration defines all possible statuses for the {@link Firmware} of a {@link Thing}
 * . The property {@link Thing#PROPERTY_FIRMWARE_VERSION} must be set for a thing in order that its firmware status can
 * be determined.
public enum FirmwareStatus {
     * The firmware status can not be determined and hence it is unknown. Either the
     * {@link Thing#PROPERTY_FIRMWARE_VERSION} is not set for the thing or there is no {@link FirmwareProvider} that
     * provides a firmware for the {@link ThingTypeUID} of the thing.
    /** The firmware of the thing is up to date. */
    UP_TO_DATE,
     * There is a newer firmware of the thing available. However the thing is not in a state where its firmware can be
     * updated, i.e. the operation {@link FirmwareUpdateHandler#isUpdateExecutable()} returned false.
    UPDATE_AVAILABLE,
    /** There is a newer firmware of the thing available and the firmware update for the thing can be executed. */
    UPDATE_EXECUTABLE
