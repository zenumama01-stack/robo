 * An {@link RuleStatusInfoEvent} notifies subscribers that a rule status has been updated.
 * @author Kai Kreuzer - added toString method
public class RuleStatusInfoEvent extends AbstractEvent {
    public static final String TYPE = RuleStatusInfoEvent.class.getSimpleName();
    private RuleStatusInfo statusInfo;
    private String ruleId;
     * constructs a new rule status event
     * @param statusInfo the status info for this event
     * @param ruleId the rule for which this event is
    public RuleStatusInfoEvent(String topic, String payload, @Nullable String source, RuleStatusInfo statusInfo,
            String ruleId) {
        this.statusInfo = statusInfo;
        this.ruleId = ruleId;
     * @return the statusInfo
    public RuleStatusInfo getStatusInfo() {
        return statusInfo;
     * @return the ruleId
    public String getRuleId() {
        return ruleId;
        return ruleId + " updated: " + statusInfo.toString();
