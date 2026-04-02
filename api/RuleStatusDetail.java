 * This enumeration is used to represent a detail of a {@link RuleStatus}. It can be considered as a sub-status.
 * It shows the specific reasons why the status of the rule is like as is.
 * <caption><b>Rule Status Details</b></caption>
 * <td><b>Detail/Status</b></td>
 * <td><b>{@link RuleStatus#UNINITIALIZED UNINITIALIZED}</b></td>
 * <td><b>{@link RuleStatus#INITIALIZING INITIALIZING}</b></td>
 * <td><b>{@link RuleStatus#IDLE IDLE}</b></td>
 * <td><b>{@link RuleStatus#RUNNING RUNNING}</b></td>
 * <td><b>{@link #NONE}</b></td>
 * <td>Initial State</td>
 * <td>Resolving started</td>
 * <td>Successfully resolved</td>
 * <td>Running</td>
 * <td><b>{@link #CONFIGURATION_ERROR}</b></td>
 * <td>Resolving failed</td>
 * <td><b>{@link #HANDLER_INITIALIZING_ERROR}</b></td>
 * <td><b>{@link #HANDLER_MISSING_ERROR}</b></td>
 * <td><b>{@link #TEMPLATE_MISSING_ERROR}</b></td>
 * <td><b>{@link #TEMPLATE_PENDING}</b></td>
 * <td>Template processing pending</td>
 * <td><b>{@link #INVALID_RULE}</b></td>
 * <td><b>{@link #DISABLED}</b></td>
 * <td>Disabled</td>
 * @author Kai Kreuzer - Refactored to match ThingStatusDetail implementation
 * @author Ravi Nadahar - added {@link #TEMPLATE_PENDING}
public enum RuleStatusDetail {
    NONE(0),
    HANDLER_MISSING_ERROR(1),
    HANDLER_INITIALIZING_ERROR(2),
    CONFIGURATION_ERROR(3),
    TEMPLATE_MISSING_ERROR(4),
    TEMPLATE_PENDING(5),
    INVALID_RULE(6),
    DISABLED(7);
    RuleStatusDetail(final int newValue) {
     * Gets the value of the status detail.
     * @return the value of the status detail.
