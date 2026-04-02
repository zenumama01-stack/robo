 * An {@link RuleRemovedEvent} notifies subscribers that a rule has been removed.
public class RuleRemovedEvent extends AbstractRuleRegistryEvent {
    public static final String TYPE = RuleRemovedEvent.class.getSimpleName();
     * Constructs a new rule removed event
     * @param rule the rule for which this event is
    public RuleRemovedEvent(String topic, String payload, @Nullable String source, RuleDTO rule) {
        return "Rule '" + getRule().uid + "' has been removed.";
