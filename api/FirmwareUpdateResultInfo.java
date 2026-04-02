 * The {@link FirmwareUpdateResultInfo} contains information about the result of a firmware update.
public final class FirmwareUpdateResultInfo {
    private final FirmwareUpdateResult result;
    private @Nullable String errorMessage;
    FirmwareUpdateResultInfo() {
        result = FirmwareUpdateResult.ERROR;
        errorMessage = null;
    private FirmwareUpdateResultInfo(ThingUID thingUID, FirmwareUpdateResult result, @Nullable String errorMessage) {
        Objects.requireNonNull(thingUID, "The thingUID must not be null.");
        Objects.requireNonNull(result, "Firmware update result must not be null");
        if (result != FirmwareUpdateResult.SUCCESS) {
            if (errorMessage == null || errorMessage.isEmpty()) {
                        "Error message must not be null or empty for erroneous firmare updates");
            this.errorMessage = errorMessage;
     * Creates a new {@link FirmwareUpdateResultInfo}.
     * @param thingUID thingUID of the thing being updated
     * @param result the result of the firmware update (must not be null)
     * @param errorMessage the error message in case of result is {@link FirmwareUpdateResult#ERROR} (must not be null
     *            or empty for erroneous firmware updates; ignored for successful firmware updates)
     * @return FirmwareUpdateResultInfo (not null)
     * @throws IllegalArgumentException if error message is null or empty for erroneous firmware updates
    public static FirmwareUpdateResultInfo createFirmwareUpdateResultInfo(ThingUID thingUID,
            FirmwareUpdateResult result, String errorMessage) {
        return new FirmwareUpdateResultInfo(thingUID, result, errorMessage);
     * Returns the result of the firmware update.
     * @return the result of the firmware update
    public FirmwareUpdateResult getResult() {
     * Returns the error message in case of result is {@link FirmwareUpdateResult#ERROR}.
     * @return the error message in case of erroneous firmware updates (is null for successful firmware updates)
    public @Nullable String getErrorMessage() {
        result = prime * result + ((errorMessage == null) ? 0 : errorMessage.hashCode());
        result = prime * result + ((this.result == null) ? 0 : this.result.hashCode());
        FirmwareUpdateResultInfo other = (FirmwareUpdateResultInfo) obj;
        if (errorMessage == null) {
            if (other.errorMessage != null) {
        } else if (!errorMessage.equals(other.errorMessage)) {
        if (result != other.result) {
        return "FirmwareUpdateResultInfo [result=" + result + ", errorMessage=" + errorMessage + "]";
