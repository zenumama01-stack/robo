 * This enumeration is used to present the main status of a {@link Rule}.
 * <table>
 * <caption><b>Rule Status transitions</b></caption>
 * <tr>
 * </tr>
 * <td><b>From/To</b></td>
 * <td><b>{@link #UNINITIALIZED}</b></td>
 * <td><b>{@link #INITIALIZING}</b></td>
 * <td><b>{@link #IDLE}</b></td>
 * <td><b>{@link #RUNNING}</b></td>
 * <td></td>
 * <td><b>N/A</b></td>
 * <td>
 * <li><b>Add:</b> Rule, ModuleHandler, ModuleType, Template</li>
 * <li><b>Update:</b> Rule</li></td>
 * <td>Resolving fails, Disable rule</td>
 * <td>Resolving succeeds</td>
 * <li><b>Remove:</b> Rule, ModuleHandler</li>
 * <li><b>Update:</b> ModuleType</li>
 * <li><b>Disable:</b> Rule</li></td>
 * <li>Triggered</li>
 * <li><b>{@link RuleManager#runNow(String) runNow}</b></li></td>
 * <td>Execution finished</td>
 * </table>
 * @author Kai Kreuzer - Refactored to match ThingStatus implementation
 * @author Ana Dimova - add java doc
public enum RuleStatus {
    UNINITIALIZED(1),
    INITIALIZING(2),
    IDLE(3),
    RUNNING(4);
    private final int value;
    RuleStatus(final int newValue) {
        value = newValue;
     * Gets the value of a rule status.
     * @return the value
    public int getValue() {
