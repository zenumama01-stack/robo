import java.util.concurrent.ThreadFactory;
 * Thread factory that applies a thread name constructed by a supplied identifier.
 * The thread name will look similar to: OH-id-counter
 * The value of "id" will be replaced with the given ID.
 * The value of "counter" will start from one and increased for every newly created thread.
public class NamedThreadFactory implements ThreadFactory {
    private final boolean daemonize;
    private final int priority;
    private final @Nullable ThreadGroup group;
    private final AtomicInteger threadNumber = new AtomicInteger(1);
    private final String namePrefix;
     * Creates a new named thread factory.
     * This constructor will create a new named thread factory using the following parameters:
     * <li>daemonize: false
     * <li>priority: normale
     * @param id the identifier used for the thread name creation
    public NamedThreadFactory(final String id) {
        this(id, false);
     * @param daemonize flag if the created thread should be daemonized
    public NamedThreadFactory(final String id, final boolean daemonize) {
        this(id, daemonize, Thread.NORM_PRIORITY);
     * @param daemonize flag if the created threads should be daemonized
     * @param priority the priority of the created threads
    public NamedThreadFactory(final String id, final boolean daemonize, final int priority) {
        this.daemonize = daemonize;
        this.priority = priority;
        this.namePrefix = "OH-" + id + "-";
        this.group = Thread.currentThread().getThreadGroup();
    public Thread newThread(@Nullable Runnable runnable) {
        final Thread thread = new Thread(group, runnable, namePrefix + threadNumber.getAndIncrement(), 0);
        if (thread.isDaemon() != daemonize) {
            thread.setDaemon(daemonize);
        if (thread.getPriority() != priority) {
            thread.setPriority(priority);
        return thread;
