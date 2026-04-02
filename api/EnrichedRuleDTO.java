package org.openhab.core.automation.rest.internal.dto;
 * This is a data transfer object that is used to serialize rules with dynamic data like the status.
@Schema(name = "EnrichedRule")
public class EnrichedRuleDTO extends RuleDTO {
    public RuleStatusInfo status;
    public Boolean editable;
