package org.openhab.core.config.core.dto;
 * {@link ConfigDescriptionDTO} is a data transfer object for {@link ConfigDescription}.
@Schema(name = "ConfigDescription")
public class ConfigDescriptionDTO {
    public String uri;
    public List<ConfigDescriptionParameterDTO> parameters;
    public List<ConfigDescriptionParameterGroupDTO> parameterGroups;
    public ConfigDescriptionDTO() {
    public ConfigDescriptionDTO(String uri, List<ConfigDescriptionParameterDTO> parameters,
            List<ConfigDescriptionParameterGroupDTO> parameterGroups) {
