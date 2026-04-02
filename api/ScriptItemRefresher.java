package org.openhab.core.model.script.jvmmodel;
 * The {@link ScriptItemRefresher} is responsible for reloading script resources every time an item is added or removed.
@Component(service = ScriptItemRefresher.class, immediate = true)
public class ScriptItemRefresher implements ItemRegistryChangeListener {
    private final Logger logger = LoggerFactory.getLogger(ScriptItemRefresher.class);
    // delay before rule resources are refreshed after items or services have changed
    private static final long REFRESH_DELAY = 2000;
    ModelRepository modelRepository;
    private ItemRegistry itemRegistry;
    private final ScheduledExecutorService scheduler = Executors.newSingleThreadScheduledExecutor();
    private ScheduledFuture<?> job;
    public void setModelRepository(ModelRepository modelRepository) {
    public void unsetModelRepository(ModelRepository modelRepository) {
        if (job != null && !job.isDone()) {
            job.cancel(false);
        this.modelRepository = null;
    public void setItemRegistry(ItemRegistry itemRegistry) {
        this.itemRegistry.addRegistryChangeListener(this);
    public void unsetItemRegistry(ItemRegistry itemRegistry) {
        this.itemRegistry.removeRegistryChangeListener(this);
        this.itemRegistry = null;
        logger.debug("Script action added => scripts are going to be refreshed");
        scheduleScriptRefresh();
        logger.debug("Script action removed => scripts are going to be refreshed");
        logger.debug("Item \"{}\" added => scripts are going to be refreshed", element.getName());
        logger.debug("Item \"{}\" removed => scripts are going to be refreshed", element.getName());
        logger.debug("All items changed => scripts are going to be refreshed");
    private synchronized void scheduleScriptRefresh() {
        job = scheduler.schedule(runnable, REFRESH_DELAY, TimeUnit.MILLISECONDS);
    Runnable runnable = new Runnable() {
                modelRepository.reloadAllModelsOfType("script");
