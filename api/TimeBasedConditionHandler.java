 * {@link ConditionHandler} that evaluates, if the current time satisfies a specified condition.
public interface TimeBasedConditionHandler extends ConditionHandler {
     * Checks if this condition is satisfied for the given time.
     * @param time The time to check.
     * @return <code>true</code> if and only if the given time satisfies this condition.
    boolean isSatisfiedAt(ZonedDateTime time);
