package org.openhab.core.automation.events;
 * abstract class for rule events
 * @author Benedikt Niehues - Initial contribution
 * @author Markus Rathgeb - Use the DTO for the Rule representation
public abstract class AbstractRuleRegistryEvent extends AbstractEvent {
    private final RuleDTO rule;
     * Must be called in subclass constructor to create a new rule registry event.
     * @param topic the topic of the event
     * @param payload the payload of the event
     * @param source the source of the event
     * @param rule the rule for which this event is created
    protected AbstractRuleRegistryEvent(String topic, String payload, @Nullable String source, RuleDTO rule) {
        super(topic, payload, source);
     * @return the RuleDTO which caused the Event
    public RuleDTO getRule() {
        return this.rule;
