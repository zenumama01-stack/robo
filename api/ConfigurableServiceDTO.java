package org.openhab.core.io.rest.core.service;
 * {@link ConfigurableServiceDTO} is a data transfer object for configurable services.
 * @author Stefan Triller - added multiple field
@Schema(name = "ConfigurableService")
public class ConfigurableServiceDTO {
    public String category;
    public @Nullable String configDescriptionURI;
    public boolean multiple;
    public ConfigurableServiceDTO(String id, String label, String category, @Nullable String configDescriptionURI,
            boolean multiple) {
