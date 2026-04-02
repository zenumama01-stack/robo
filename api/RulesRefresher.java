package org.openhab.core.model.rule.jvmmodel;
import org.openhab.core.items.ItemRegistryChangeListener;
import org.openhab.core.model.script.engine.action.ActionService;
 * The {@link RulesRefresher} is responsible for reloading rules resources every time.
 * @author Kai Kreuzer - added delayed execution
 * @author Maoliang Huang - refactor
public class RulesRefresher implements ReadyTracker {
    // delay in seconds before rule resources are refreshed after items or services have changed
    private static final long REFRESH_DELAY = 30;
    public static final String RULES_REFRESH_MARKER_TYPE = "rules";
    public static final String RULES_REFRESH = "refresh";
    private final Logger logger = LoggerFactory.getLogger(RulesRefresher.class);
    private @Nullable ScheduledFuture<?> job;
    private final ScheduledExecutorService scheduler = Executors
            .newSingleThreadScheduledExecutor(new NamedThreadFactory("rulesRefresher"));
    private boolean started;
    private final ReadyMarker marker = new ReadyMarker("rules", RULES_REFRESH);
    private final ItemRegistryChangeListener itemRegistryChangeListener = new ItemRegistryChangeListener() {
        public void added(Item element) {
            logger.debug("Item \"{}\" added => rules are going to be refreshed", element.getName());
            scheduleRuleRefresh(REFRESH_DELAY);
        public void removed(Item element) {
            logger.debug("Item \"{}\" removed => rules are going to be refreshed", element.getName());
        public void updated(Item oldElement, Item element) {
        public void allItemsChanged(Collection<String> oldItemNames) {
            logger.debug("All items changed => rules are going to be refreshed");
    private final ThingRegistryChangeListener thingRegistryChangeListener = new ThingRegistryChangeListener() {
            logger.debug("Thing \"{}\" added => rules are going to be refreshed", element.getUID().getAsString());
            logger.debug("Thing \"{}\" removed => rules are going to be refreshed", element.getUID().getAsString());
    public RulesRefresher(@Reference ModelRepository modelRepository, @Reference ItemRegistry itemRegistry,
            @Reference ThingRegistry thingRegistry, @Reference ReadyService readyService) {
                .withIdentifier(Integer.toString(StartLevelService.STARTLEVEL_MODEL)));
    protected void addActionService(ActionService actionService) {
            logger.debug("Script action added => rules are going to be refreshed");
    protected void removeActionService(ActionService actionService) {
            logger.debug("Script action removed => rules are going to be refreshed");
    protected void addThingActions(ThingActions thingActions) {
            logger.debug("Thing automation action added => rules are going to be refreshed");
            logger.debug("Thing automation action removed => rules are going to be refreshed");
    protected synchronized void scheduleRuleRefresh(long delay) {
        ScheduledFuture<?> localJob = job;
        if (localJob != null && !localJob.isDone()) {
            localJob.cancel(false);
        job = scheduler.schedule(() -> {
                modelRepository.reloadAllModelsOfType("rules");
        }, delay, TimeUnit.SECONDS);
        scheduleRuleRefresh(0);
        itemRegistry.addRegistryChangeListener(itemRegistryChangeListener);
        thingRegistry.addRegistryChangeListener(thingRegistryChangeListener);
        itemRegistry.removeRegistryChangeListener(itemRegistryChangeListener);
        thingRegistry.removeRegistryChangeListener(thingRegistryChangeListener);
