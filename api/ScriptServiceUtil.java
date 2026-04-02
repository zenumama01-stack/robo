 * Utility class for providing easy access to script services.
 * @author Kai Kreuzer - renamed and removed interface
@Component(immediate = true, service = ScriptServiceUtil.class)
public class ScriptServiceUtil {
    private final Logger logger = LoggerFactory.getLogger(ScriptServiceUtil.class);
    private static ScriptServiceUtil instance;
    private final AtomicReference<ScriptEngine> scriptEngine = new AtomicReference<>();
    public final List<ActionService> actionServices = new CopyOnWriteArrayList<>();
    public final List<ThingActions> thingActions = new CopyOnWriteArrayList<>();
    public ScriptServiceUtil(final @Reference ItemRegistry itemRegistry, final @Reference ThingRegistry thingRegistry,
            final @Reference EventPublisher eventPublisher, final @Reference ModelRepository modelRepository,
            final @Reference Scheduler scheduler) {
            throw new IllegalStateException("ScriptServiceUtil should only be activated once!");
        logger.debug("ScriptServiceUtil started");
        logger.debug("ScriptServiceUtil stopped");
    private static ScriptServiceUtil getInstance() {
    public static ItemRegistry getItemRegistry() {
        return getInstance().itemRegistry;
    public ItemRegistry getItemRegistryInstance() {
        return itemRegistry;
    public ThingRegistry getThingRegistryInstance() {
        return thingRegistry;
    public static EventPublisher getEventPublisher() {
        return getInstance().eventPublisher;
    public static ModelRepository getModelRepository() {
        return getInstance().modelRepository;
    public ModelRepository getModelRepositoryInstance() {
        return modelRepository;
    public static Scheduler getScheduler() {
        return getInstance().scheduler;
    public Scheduler getSchedulerInstance() {
        return scheduler;
    public static ScriptEngine getScriptEngine() {
        return getInstance().scriptEngine.get();
    public static List<ActionService> getActionServices() {
        return getInstance().actionServices;
    public static List<ThingActions> getThingActions() {
        return getInstance().thingActions;
    public List<ActionService> getActionServiceInstances() {
        return actionServices;
    public List<ThingActions> getThingActionsInstances() {
        return thingActions;
        this.actionServices.add(actionService);
        this.actionServices.remove(actionService);
        this.thingActions.add(thingActions);
        this.thingActions.remove(thingActions);
    public void setScriptEngine(ScriptEngine scriptEngine) {
        // injected as a callback from the script engine, not via DS as it is a circular dependency...
        this.scriptEngine.set(scriptEngine);
    public void unsetScriptEngine(ScriptEngine scriptEngine) {
        // uninjected as a callback from the script engine, not via DS as it is a circular dependency...
        this.scriptEngine.compareAndSet(scriptEngine, null);
