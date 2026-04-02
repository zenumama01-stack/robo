 * An {@link RuleAddedEvent} notifies subscribers that a rule has been added.
public class RuleAddedEvent extends AbstractRuleRegistryEvent {
    public static final String TYPE = RuleAddedEvent.class.getSimpleName();
     * constructs a new rule added event
    public RuleAddedEvent(String topic, String payload, @Nullable String source, RuleDTO rule) {
        super(topic, payload, source, rule);
        return "Rule '" + getRule().uid + "' has been added.";
