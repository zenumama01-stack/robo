package org.openhab.core.persistence.strategy;
 * This class holds a cron expression based strategy to persist items.
public class PersistenceCronStrategy extends PersistenceStrategy {
    private final String cronExpression;
    public PersistenceCronStrategy(final String name, final String cronExpression) {
    public String getCronExpression() {
        return cronExpression;
        return String.format("%s [%s, cronExpression=%s]", getClass().getSimpleName(), super.toString(),
                cronExpression);
