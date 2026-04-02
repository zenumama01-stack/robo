 * The {@link RuleMetric} class implements a gauge metric for rules RUNNING events (per rule)
public class RuleMetric implements OpenhabCoreMeterBinder, EventSubscriber {
    public static final String METRIC_NAME = "openhab.rule.runs";
    public static final String RULES_TOPIC_PREFIX = "openhab/rules/";
    public static final String RULES_TOPIC_SUFFIX = "/state";
    private final Logger logger = LoggerFactory.getLogger(RuleMetric.class);
    private static final Tag CORE_RULE_METRIC_TAG = Tag.of("metric", "openhab.core.metric.rules");
    private static final String RULE_ID_TAG_NAME = "rule";
    private static final String RULE_NAME_TAG_NAME = "rulename";
    private RuleRegistry ruleRegistry;
    public RuleMetric(BundleContext bundleContext, Collection<Tag> tags, RuleRegistry ruleRegistry) {
        this.tags.add(CORE_RULE_METRIC_TAG);
        logger.debug("RuleMetric is being bound...");
            if (meter.getId().getTags().contains(CORE_RULE_METRIC_TAG)) {
        return Set.of(RuleStatusInfoEvent.TYPE);
            logger.trace("Measurement not started. Skipping rule event processing");
        String ruleId = topic.substring(RULES_TOPIC_PREFIX.length(), topic.lastIndexOf(RULES_TOPIC_SUFFIX));
        if (!event.getPayload().contains(RuleStatus.RUNNING.name())) {
            logger.trace("Skipping rule status info with status other than RUNNING {}", event.getPayload());
        logger.debug("Rule {} RUNNING - updating metric.", ruleId);
        Set<Tag> tagsWithRule = new HashSet<>(tags);
        tagsWithRule.add(Tag.of(RULE_ID_TAG_NAME, ruleId));
        String ruleName = getRuleName(ruleId);
        if (ruleName != null) {
            tagsWithRule.add(Tag.of(RULE_NAME_TAG_NAME, ruleName));
        meterRegistry.counter(METRIC_NAME, tagsWithRule).increment();
    private @Nullable String getRuleName(String ruleId) {
        Rule rule = ruleRegistry.get(ruleId);
        return rule == null ? null : rule.getName();
