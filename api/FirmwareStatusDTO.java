 * This is a data transfer object that is used to serialize firmware status information.
@Schema(name = "FirmwareStatus")
public class FirmwareStatusDTO {
    public final String status;
    public final @Nullable String updatableVersion;
    public FirmwareStatusDTO(String status, @Nullable String updatableVersion) {
        this.updatableVersion = updatableVersion;
