 * An {@link RuleUpdatedEvent} notifies subscribers that a rule has been updated.
public class RuleUpdatedEvent extends AbstractRuleRegistryEvent {
    public static final String TYPE = RuleUpdatedEvent.class.getSimpleName();
    private final RuleDTO oldRule;
     * constructs a new rule updated event
     * @param rule the rule for which is this event
     * @param oldRule the rule that has been updated
    public RuleUpdatedEvent(String topic, String payload, @Nullable String source, RuleDTO rule, RuleDTO oldRule) {
        this.oldRule = oldRule;
     * @return the oldRuleDTO
    public RuleDTO getOldRule() {
        return "Rule '" + getRule().uid + "' has been updated.";
