 * This class is responsible for tracking the bundles providing automation resources and delegating the processing to
 * the responsible providers in separate thread.
 * @param <E>
public class AutomationResourceBundlesEventQueue<@NonNull E> implements Runnable {
    protected final Logger logger = LoggerFactory.getLogger(AutomationResourceBundlesEventQueue.class);
     * This field serves for saving the BundleEvents for the bundles providing automation resources until their
     * processing completes.
    private List<BundleEvent> queue = new ArrayList<>();
     * This field is for synchronization purposes
    private boolean running = false;
    private @Nullable Thread runningThread;
    private boolean closed = false;
    private boolean shared = false;
    private final AbstractResourceBundleProvider<E> provider;
     * This constructor is responsible for initializing a queue for bundles providing automation resources.
     *            is a reference to an implementation of {@link TemplateProvider} or {@link ModuleTypeProvider} or an
     *            importer of {@link Rule}s.
    public AutomationResourceBundlesEventQueue(AbstractResourceBundleProvider<E> provider) {
        this.provider = provider;
     * When a new event for a bundle providing automation resources is received, this will causes a creation of a new
     * thread if there is no other created yet. If the thread already exists, then it will be notified for the event.
     * Starting the thread will cause the execution of this method in separate thread.
     * The general contract of this method <code>run</code> is invoking of the
     * {@link #processBundleChanged(BundleEvent)} method and executing it in separate thread.
     * @see java.lang.Thread#run()
        boolean waitForEvents = true;
            List<BundleEvent> lQueue;
                if (closed) {
                    notifyAll();
                if (queue.isEmpty()) {
                    if (waitForEvents) {
                            wait(180000);
                        waitForEvents = false;
                    running = false;
                    runningThread = null;
                lQueue = queue;
                shared = true;
            for (BundleEvent event : lQueue) {
                    processBundleChanged(event);
                    if (!closed) {
                        logger.warn("Processing bundle event {}, for automation resource bundle '{}' failed",
                                event.getType(), event.getBundle().getSymbolicName(), t);
                if (shared) {
                    queue.clear();
                shared = false;
                waitForEvents = true;
     * This method is invoked when this component is deactivated to stop the separate thread if still running.
    public void stop() {
        Thread runningThread = this.runningThread;
        if (runningThread != null) {
                runningThread.join(30000);
     * This method is called when a new event for a bundle providing automation resources is received. It causes a
     * creation of a new thread if there is no other created yet and starting the it. If the thread already exists,
     * it is waiting for events and will be notified for the event.
     * @param bundle providing automation resources
     * @param event for a bundle tracked by the {@code BundleTracker}. It has been for adding, modifying or removing the
     *            bundle.
    protected synchronized void addEvent(Bundle bundle, BundleEvent event) {
            queue = new LinkedList<>();
        if (queue.add(event)) {
            logger.debug("Process bundle event {}, for automation bundle '{}' ", event.getType(),
                    event.getBundle().getSymbolicName());
            if (running) {
                runningThread = new Thread(this, "Automation Provider Processing Queue");
                runningThread.start();
                running = true;
     * Depending on the action committed against the bundle supplier of automation resources, this method performs the
     * appropriate action - calls for it's host bundles:
     * {@link AbstractResourceBundleProvider#processAutomationProviderUninstalled(Bundle)} method
     * or
     * {@link AbstractResourceBundleProvider#processAutomationProvider(Bundle)} method
    protected void processBundleChanged(BundleEvent event) {
        Bundle bundle = event.getBundle();
        if (HostFragmentMappingUtil.isFragmentBundle(bundle)) {
            for (Bundle host : HostFragmentMappingUtil.returnHostBundles(bundle)) {
                provider.processAutomationProvider(host);
            switch (event.getType()) {
                case BundleEvent.UNINSTALLED:
                    provider.processAutomationProviderUninstalled(bundle);
                    provider.processAutomationProvider(bundle);
     * This method is responsible for initializing the queue with all already received BundleEvents and starting a
     * thread that should process them.
     * @param queue list with all already received BundleEvents
    protected synchronized void addAll(List<BundleEvent> queue) {
            this.queue = new LinkedList<>();
        if (this.queue.addAll(queue)) {
