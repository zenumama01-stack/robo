package org.openhab.core.io.rest.core.discovery;
 * This is a data transfer object that is used to serialize the information about binding discovery.
@Schema(name = "DiscoveryInfo")
public class DiscoveryInfoDTO {
    public boolean inputSupported;
    public @Nullable String inputLabel;
    public @Nullable String inputDescription;
    public DiscoveryInfoDTO(boolean inputSupported, @Nullable String inputLabel, @Nullable String inputDescription) {
        this.inputSupported = inputSupported;
        this.inputLabel = inputLabel;
        this.inputDescription = inputDescription;
