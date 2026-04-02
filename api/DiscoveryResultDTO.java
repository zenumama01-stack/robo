package org.openhab.core.config.discovery.dto;
import org.openhab.core.config.discovery.DiscoveryResultFlag;
 * This is a data transfer object that is used to serialize discovery results.
@Schema(name = "DiscoveryResult")
public class DiscoveryResultDTO {
    public @Nullable String bridgeUID;
    public DiscoveryResultFlag flag;
    public Map<String, Object> properties;
    public @Nullable String representationProperty;
    public @NonNullByDefault({}) String thingUID;
    public @Nullable String thingTypeUID;
    // do not remove - needed by GSON
    public DiscoveryResultDTO() {
        this("", null, null, "", DiscoveryResultFlag.NEW, Map.of(), null);
    public DiscoveryResultDTO(String thingUID, @Nullable String bridgeUID, @Nullable String thingTypeUID, String label,
            DiscoveryResultFlag flag, Map<String, Object> properties, @Nullable String representationProperty) {
        this.flag = flag;
        this.properties = properties;
