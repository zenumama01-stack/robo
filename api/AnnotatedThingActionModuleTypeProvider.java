package org.openhab.core.automation.thingsupport;
 * ModuleTypeProvider that collects actions for {@link ThingHandler}s
public class AnnotatedThingActionModuleTypeProvider extends BaseModuleHandlerFactory implements ModuleTypeProvider {
    private final Logger logger = LoggerFactory.getLogger(AnnotatedThingActionModuleTypeProvider.class);
    public AnnotatedThingActionModuleTypeProvider(final @Reference ModuleTypeI18nService moduleTypeI18nService,
    public void addAnnotatedThingActions(ThingActions annotatedThingActions) {
        if (annotatedThingActions.getClass().isAnnotationPresent(ThingActionsScope.class)) {
            ThingActionsScope scope = annotatedThingActions.getClass().getAnnotation(ThingActionsScope.class);
            Collection<ModuleInformation> moduleInformations = helper.parseAnnotations(scope.name(),
                    annotatedThingActions);
            String thingUID = getThingUID(annotatedThingActions);
                mi.setThingUID(thingUID);
            logger.error("Missing 'ThingActionsScope' for '{}'. Please add it to your class definition.",
                    annotatedThingActions.getClass());
    public void removeAnnotatedThingActions(ThingActions annotatedThingActions) {
                    ModuleType oldType = helper.buildModuleType(mi.getUID(), moduleInformation);
                                l.removed(this, oldType);
    private String getThingUID(ThingActions annotatedThingActions) {
        ThingHandler handler = annotatedThingActions.getThingHandler();
                    String.format("ThingHandler for '%s' is missing.", annotatedThingActions.getClass()));
        return handler.getThing().getUID().getAsString();
