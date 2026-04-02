 * Runnable that can throw checked exceptions.
 * @author Hilbrand Bouwkamp - moved to it's own class and renamed
public interface SchedulerRunnable {
     * Scheduled job to run.
     * @throws Exception exception thrown
    void run() throws Exception;
