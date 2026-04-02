 * Runnable that can be passed data and can throw a checked exception.
public interface CronJob {
     * The service property that specifies the cron schedule. The type is String+.
    String CRON = "cron";
     * Run a cron job.
     * @param data The data for the job
     * @throws Exception Exception thrown
    void run(Map<String, Object> data) throws Exception;
