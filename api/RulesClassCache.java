package org.openhab.core.model.rule.scoping;
import org.eclipse.xtext.common.types.access.impl.Primitives;
 * The class cache used by the {@link RulesClassFinder} for resolving classes in DSL rules.
 * It allows for removing and updating classes in the cache when add-ons are installed or updated.
public class RulesClassCache extends HashMap<String, Class<?>> {
    private static RulesClassCache instance;
    private final Logger logger = LoggerFactory.getLogger(RulesClassCache.class);
    public RulesClassCache() {
        super(500);
        for (Class<?> primitiveType : Primitives.ALL_PRIMITIVE_TYPES) {
            put(primitiveType.getName(), primitiveType);
            throw new IllegalStateException("RulesClassCache should only be activated once!");
        instance = this;
        clear();
    public static RulesClassCache getInstance() {
    private void updateCacheEntry(Object object) {
        String key = object.getClass().getName();
        put(key, object.getClass());
        logger.debug("Updated cache entry: {}", key);
    public void addActionService(ActionService actionService) {
        updateCacheEntry(actionService);
    public void removeActionService(ActionService actionService) {
        updateCacheEntry(thingActions);
