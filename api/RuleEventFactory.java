import org.openhab.core.automation.events.RuleAddedEvent;
import org.openhab.core.automation.events.RuleRemovedEvent;
import org.openhab.core.automation.events.RuleUpdatedEvent;
 * This class is a factory that creates Rule Events.
public class RuleEventFactory extends AbstractEventFactory {
    private final Logger logger = LoggerFactory.getLogger(RuleEventFactory.class);
    private static final String RULE_STATE_EVENT_TOPIC = "openhab/rules/{ruleID}/state";
    private static final String RULE_ADDED_EVENT_TOPIC = "openhab/rules/{ruleID}/added";
    private static final String RULE_REMOVED_EVENT_TOPIC = "openhab/rules/{ruleID}/removed";
    private static final String RULE_UPDATED_EVENT_TOPIC = "openhab/rules/{ruleID}/updated";
    private static final Set<String> SUPPORTED_TYPES = new HashSet<>();
        SUPPORTED_TYPES.add(RuleAddedEvent.TYPE);
        SUPPORTED_TYPES.add(RuleRemovedEvent.TYPE);
        SUPPORTED_TYPES.add(RuleStatusInfoEvent.TYPE);
        SUPPORTED_TYPES.add(RuleUpdatedEvent.TYPE);
    public RuleEventFactory() {
        if (RuleAddedEvent.TYPE.equals(eventType)) {
            return createRuleAddedEvent(topic, payload, source);
        } else if (RuleRemovedEvent.TYPE.equals(eventType)) {
            return createRuleRemovedEvent(topic, payload, source);
        } else if (RuleStatusInfoEvent.TYPE.equals(eventType)) {
            return createRuleStatusInfoEvent(topic, payload, source);
        } else if (RuleUpdatedEvent.TYPE.equals(eventType)) {
            return createRuleUpdatedEvent(topic, payload, source);
    private Event createRuleUpdatedEvent(String topic, String payload, @Nullable String source) {
        RuleDTO[] ruleDTO = deserializePayload(payload, RuleDTO[].class);
        if (ruleDTO.length != 2) {
            throw new IllegalArgumentException("Creation of RuleUpdatedEvent failed: invalid payload: " + payload);
        return new RuleUpdatedEvent(topic, payload, source, ruleDTO[0], ruleDTO[1]);
    private Event createRuleStatusInfoEvent(String topic, String payload, @Nullable String source) {
        RuleStatusInfo statusInfo = deserializePayload(payload, RuleStatusInfo.class);
        if (statusInfo.getStatus() == null || statusInfo.getStatusDetail() == null) {
            throw new IllegalArgumentException("Creation of RuleStatusInfo failed: invalid payload: " + payload);
        return new RuleStatusInfoEvent(topic, payload, source, statusInfo, getRuleId(topic));
    private Event createRuleRemovedEvent(String topic, String payload, @Nullable String source) {
        RuleDTO ruleDTO = deserializePayload(payload, RuleDTO.class);
        return new RuleRemovedEvent(topic, payload, source, ruleDTO);
    private Event createRuleAddedEvent(String topic, String payload, @Nullable String source) {
        return new RuleAddedEvent(topic, payload, source, ruleDTO);
    private String getRuleId(String topic) {
        String[] topicElements = getTopicElements(topic);
        if (topicElements.length != 4) {
            throw new IllegalArgumentException("Event creation failed, invalid topic: " + topic);
        return topicElements[2];
     * Creates a rule updated event.
     * @param rule the new rule.
     * @param oldRule the rule that has been updated.
     * @param source the source of the event.
     * @return {@link RuleUpdatedEvent} instance.
    public static RuleUpdatedEvent createRuleUpdatedEvent(Rule rule, Rule oldRule, String source) {
        String topic = buildTopic(RULE_UPDATED_EVENT_TOPIC, rule);
        final RuleDTO ruleDto = RuleDTOMapper.map(rule);
        final RuleDTO oldRuleDto = RuleDTOMapper.map(oldRule);
        List<RuleDTO> rules = new LinkedList<>();
        rules.add(ruleDto);
        rules.add(oldRuleDto);
        String payload = serializePayload(rules);
        return new RuleUpdatedEvent(topic, payload, source, ruleDto, oldRuleDto);
     * Creates a rule status info event.
     * @param statusInfo the status info of the event.
     * @param ruleUID the UID of the rule for which the event is created.
     * @return {@link RuleStatusInfoEvent} instance.
    public static RuleStatusInfoEvent createRuleStatusInfoEvent(RuleStatusInfo statusInfo, String ruleUID,
        String topic = buildTopic(RULE_STATE_EVENT_TOPIC, ruleUID);
        String payload = serializePayload(statusInfo);
        return new RuleStatusInfoEvent(topic, payload, source, statusInfo, ruleUID);
     * Creates a rule removed event.
     * @param rule the rule for which this event is created.
     * @return {@link RuleRemovedEvent} instance.
    public static RuleRemovedEvent createRuleRemovedEvent(Rule rule, String source) {
        String topic = buildTopic(RULE_REMOVED_EVENT_TOPIC, rule);
        String payload = serializePayload(ruleDto);
        return new RuleRemovedEvent(topic, payload, source, ruleDto);
     * Creates a rule added event.
     * @return {@link RuleAddedEvent} instance.
    public static RuleAddedEvent createRuleAddedEvent(Rule rule, String source) {
        String topic = buildTopic(RULE_ADDED_EVENT_TOPIC, rule);
        return new RuleAddedEvent(topic, payload, source, ruleDto);
    private static String buildTopic(String topic, String ruleUID) {
        return topic.replace("{ruleID}", ruleUID);
    private static String buildTopic(String topic, Rule rule) {
        return buildTopic(topic, rule.getUID());
