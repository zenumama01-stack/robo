package org.openhab.core.internal.scheduler;
import org.openhab.core.scheduler.CronJob;
 * Implementation of a {@link CronScheduler}.
 * @author Peter Kriens - Initial contribution
 * @author Simon Kaufmann - adapted to CompletableFutures
 * @author Hilbrand Bouwkamp - moved cron scheduling to it's own interface
@Component(service = CronScheduler.class)
public class CronSchedulerImpl implements CronScheduler {
    private final Logger logger = LoggerFactory.getLogger(CronSchedulerImpl.class);
    private final List<Cron> crons = new ArrayList<>();
    public CronSchedulerImpl(final @Reference Scheduler scheduler) {
    public ScheduledCompletableFuture<@Nullable Void> schedule(SchedulerRunnable runnable, String cronExpression) {
        return schedule(d -> runnable.run(), Map.of(), cronExpression);
    public ScheduledCompletableFuture<@Nullable Void> schedule(CronJob job, Map<String, Object> config,
            String cronExpression) {
        final CronAdjuster cronAdjuster = new CronAdjuster(cronExpression);
        final SchedulerRunnable runnable = () -> job.run(config);
        if (cronAdjuster.isReboot()) {
            return scheduler.at(runnable, Instant.ofEpochMilli(1));
            return scheduler.schedule(runnable, cronAdjuster);
    <T> void addSchedule(CronJob cronJob, Map<String, Object> map) {
        final Object scheduleConfig = map.get(CronJob.CRON);
        String[] schedules = null;
        if (scheduleConfig instanceof String[] strings) {
            schedules = strings;
        } else if (scheduleConfig instanceof String string) {
            schedules = new String[] { string };
        if (schedules == null || schedules.length == 0) {
            logger.info("No schedules in map with key '" + CronJob.CRON + "'. Nothing scheduled");
        synchronized (crons) {
            for (String schedule : schedules) {
                    final Cron cron = new Cron(cronJob, schedule(cronJob, map, schedule));
                    crons.add(cron);
                    logger.warn("Invalid cron expression {} from {}", schedule, map, e);
    void removeSchedule(CronJob s) {
            for (Iterator<Cron> cron = crons.iterator(); cron.hasNext();) {
                final Cron c = cron.next();
                if (c.target.equals(s)) {
                    cron.remove();
                    c.schedule.cancel(true);
    private static class Cron {
        private final CronJob target;
        private final ScheduledCompletableFuture<?> schedule;
        public Cron(CronJob target, ScheduledCompletableFuture<?> schedule) {
            this.schedule = schedule;
