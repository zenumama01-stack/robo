package org.openhab.core.addon.marketplace.internal.json.model;
 * The {@link AddonEntryDTO} is a DTO for encapsulating a single addon information.
public class AddonEntryDTO {
    public String uid = "";
    public String id = "";
    public String type = "";
    public String description = "";
    public String keywords = "";
    public String title = "";
    public String link = "";
    public String version = "";
    @SerializedName("compatible_versions")
    public String compatibleVersions = "";
    public String author = "";
    public String configDescriptionURI = "";
    public String maturity = "unstable";
    @SerializedName("content_type")
    public String contentType = "";
    public String url = "";
    @SerializedName("logger_packages")
    public List<String> loggerPackages = List.of();
    public String connection = "";
    public List<String> countries = List.of();
