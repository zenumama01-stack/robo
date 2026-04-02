 * Expected execution of a {@link Rule}.
 * @author Sönke Küper - Initial contribution
public final class RuleExecution implements Comparable<RuleExecution> {
    private final Date date;
    private final Rule rule;
     * Creates a new {@link RuleExecution}.
     * @param date The time when the rule will be executed.
     * @param rule The rule that will be executed.
    public RuleExecution(Date date, Rule rule) {
        this.date = date;
        this.rule = rule;
     * Returns the time when the rule will be executed.
    public Date getDate() {
     * Returns the rule that will be executed.
    public Rule getRule() {
    public int compareTo(RuleExecution o) {
        return this.date.compareTo(o.getDate());
