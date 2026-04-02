package org.openhab.core.automation.dto;
 * This is a data transfer object that is used to serialize the respective class.
@Schema(name = "Action")
public class ActionDTO extends ModuleDTO {
    public Map<String, String> inputs;
