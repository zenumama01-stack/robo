 * The {@link FirmwareStatusInfo} represents the {@link FirmwareStatus} of a {@link Thing}. If the firmware status is
 * {@link FirmwareStatus#UPDATE_EXECUTABLE} then the information object will also provide the thing UID and the
 * version of the latest updatable firmware for the thing.
 * @author Dimitar Ivanov - Consolidated all the needed information for firmware status events
public final class FirmwareStatusInfo {
    private final FirmwareStatus firmwareStatus;
    private final @Nullable String firmwareVersion;
    FirmwareStatusInfo() {
        thingUID = new ThingUID("internal:reflective:constructor");
        firmwareStatus = FirmwareStatus.UNKNOWN;
        firmwareVersion = null;
    private FirmwareStatusInfo(ThingUID thingUID, FirmwareStatus firmwareStatus, @Nullable String firmwareVersion) {
        Objects.requireNonNull(thingUID, "Thing UID must not be null.");
        Objects.requireNonNull(firmwareStatus, "Firmware status must not be null.");
        this.firmwareVersion = firmwareVersion;
     * Creates a new {@link FirmwareStatusInfo} having {@link FirmwareStatus#UNKNOWN} as firmware status.
     * @return the firmware status info (not null)
    public static FirmwareStatusInfo createUnknownInfo(ThingUID thingUID) {
        return new FirmwareStatusInfo(thingUID, FirmwareStatus.UNKNOWN, null);
     * Creates a new {@link FirmwareStatusInfo} having {@link FirmwareStatus#UP_TO_DATE} as firmware status.
    public static FirmwareStatusInfo createUpToDateInfo(ThingUID thingUID) {
        return new FirmwareStatusInfo(thingUID, FirmwareStatus.UP_TO_DATE, null);
     * Creates a new {@link FirmwareStatusInfo} having {@link FirmwareStatus#UPDATE_AVAILABLE} as firmware status.
    public static FirmwareStatusInfo createUpdateAvailableInfo(ThingUID thingUID) {
        return new FirmwareStatusInfo(thingUID, FirmwareStatus.UPDATE_AVAILABLE, null);
     * Creates a new {@link FirmwareStatusInfo} having {@link FirmwareStatus#UPDATE_EXECUTABLE} as firmware status. The
     * given firmware version represents the version of the latest updatable firmware for the thing.
     * @param firmwareVersion the version of the latest updatable firmware for the thing (must not be null)
    public static FirmwareStatusInfo createUpdateExecutableInfo(ThingUID thingUID, @Nullable String firmwareVersion) {
        return new FirmwareStatusInfo(thingUID, FirmwareStatus.UPDATE_EXECUTABLE, firmwareVersion);
     * Returns the firmware status.
     * @return the firmware status (not null)
    public FirmwareStatus getFirmwareStatus() {
        return firmwareStatus;
     * Returns the firmware version of the latest updatable firmware for the thing.
     * @return the firmware version (only set if firmware status is {@link FirmwareStatus#UPDATE_EXECUTABLE})
    public @Nullable String getUpdatableFirmwareVersion() {
        return this.firmwareVersion;
     * Returns the thing UID.
     * @return the thing UID of the thing, whose status is updated (not null)
        result = prime * result + ((firmwareStatus == null) ? 0 : firmwareStatus.hashCode());
        result = prime * result + ((firmwareVersion == null) ? 0 : firmwareVersion.hashCode());
        FirmwareStatusInfo other = (FirmwareStatusInfo) obj;
        if (firmwareStatus != other.firmwareStatus) {
        if (firmwareVersion == null) {
            if (other.firmwareVersion != null) {
        } else if (!firmwareVersion.equals(other.firmwareVersion)) {
        return "FirmwareStatusInfo [firmwareStatus=" + firmwareStatus + ", firmwareVersion=" + firmwareVersion
                + ", thingUID=" + thingUID + "]";
