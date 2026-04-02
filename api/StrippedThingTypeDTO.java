 * This is a data transfer object that is used to serialize stripped thing types.
 * Stripped thing types exclude the parameters, configDescription and channels
 * @author Miki Jankov - Initial contribution
@Schema(name = "StrippedThingType")
public class StrippedThingTypeDTO {
    public boolean listed;
    public List<String> supportedBridgeTypeUIDs;
    public boolean bridge;
    public StrippedThingTypeDTO() {
        this("", "", null, null, true, List.of(), false, null);
    public StrippedThingTypeDTO(String uid, String label, @Nullable String description, @Nullable String category,
            boolean listed, List<String> supportedBridgeTypeUIDs, boolean bridge,
            @Nullable String semanticEquipmentTag) {
        this.listed = listed;
        this.supportedBridgeTypeUIDs = supportedBridgeTypeUIDs;
        this.bridge = bridge;
