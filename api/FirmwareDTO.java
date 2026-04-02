package org.openhab.core.thing.firmware.dto;
 * This is a data transfer object that is used to serialize firmware information.
 * @author Aoun Bukhari - Initial contribution
 * @author Dimitar Ivanov - enriched with thing type UID
@Schema(name = "Firmware")
public class FirmwareDTO {
    public final String thingTypeUID;
    public final @Nullable String vendor;
    public final @Nullable String model;
    public final boolean modelRestricted;
    public final @Nullable String description;
    public final String version;
    public final @Nullable String changelog;
    public final @Nullable String prerequisiteVersion;
    public FirmwareDTO(String thingTypeUID, @Nullable String vendor, @Nullable String model, boolean modelRestricted,
            @Nullable String description, String version, @Nullable String prerequisiteVersion,
            @Nullable String changelog) {
