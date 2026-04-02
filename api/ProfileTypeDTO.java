package org.openhab.core.thing.profiles.dto;
 * DTO for profile types
@Schema(name = "ProfileType")
public class ProfileTypeDTO {
    public Collection<String> supportedItemTypes;
    public ProfileTypeDTO() {
        this("", "", "", Set.of());
    public ProfileTypeDTO(String uid, String label, String kind, Collection<String> supportedItemTypes) {
        this.supportedItemTypes = supportedItemTypes;
