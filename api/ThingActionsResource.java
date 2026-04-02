import java.lang.reflect.Method;
import javax.ws.rs.core.HttpHeaders;
import org.openhab.core.automation.annotation.RuleAction;
import org.openhab.core.automation.module.provider.AnnotationActionModuleTypeHelper;
import org.openhab.core.automation.type.Input;
import org.openhab.core.automation.util.ActionInputsHelper;
import org.openhab.core.config.core.dto.ConfigDescriptionDTOMapper;
import org.openhab.core.config.core.dto.ConfigDescriptionParameterDTO;
 * The {@link ThingActionsResource} allows retrieving and executing thing actions via REST API
 * @author Laurent Garnier - API enhanced to be able to run thing actions in Main UI
@JaxrsName(ThingActionsResource.PATH_THINGS)
@Path(ThingActionsResource.PATH_THINGS)
@Tag(name = ThingActionsResource.PATH_THINGS)
public class ThingActionsResource implements RESTResource {
    public static final String PATH_THINGS = "actions";
    private final ActionInputsHelper actionInputsHelper;
    private final AnnotationActionModuleTypeHelper annotationActionModuleTypeHelper;
    Map<ThingUID, Map<String, List<String>>> thingActionsMap = new ConcurrentHashMap<>();
    private List<ModuleHandlerFactory> moduleHandlerFactories = new ArrayList<>();
    public ThingActionsResource(@Reference LocaleService localeService,
            @Reference ModuleTypeRegistry moduleTypeRegistry, @Reference ActionInputsHelper actionInputsHelper,
            @Reference AnnotationActionModuleTypeHelper annotationActionModuleTypeHelper) {
        this.actionInputsHelper = actionInputsHelper;
        this.annotationActionModuleTypeHelper = annotationActionModuleTypeHelper;
    public void addThingActions(ThingActions thingActions) {
        ThingHandler handler = thingActions.getThingHandler();
        if (handler != null && scope != null) {
            ThingUID thingUID = handler.getThing().getUID();
            Method[] methods = thingActions.getClass().getDeclaredMethods();
            List<String> actionUIDs = new ArrayList<>();
            for (Method method : methods) {
                if (!method.isAnnotationPresent(RuleAction.class)) {
                actionUIDs.add(annotationActionModuleTypeHelper.getModuleIdFromMethod(scope, method));
            if (actionUIDs.isEmpty()) {
            Objects.requireNonNull(thingActionsMap.computeIfAbsent(thingUID, thingUid -> new ConcurrentHashMap<>()))
                    .put(scope, actionUIDs);
    public void removeThingActions(ThingActions thingActions) {
            Map<String, List<String>> actionMap = thingActionsMap.get(thingUID);
            if (actionMap != null) {
                actionMap.remove(scope);
                if (actionMap.isEmpty()) {
                    thingActionsMap.remove(thingUID);
    protected void addModuleHandlerFactory(ModuleHandlerFactory moduleHandlerFactory) {
        moduleHandlerFactories.add(moduleHandlerFactory);
    protected void removeModuleHandlerFactory(ModuleHandlerFactory moduleHandlerFactory) {
        moduleHandlerFactories.remove(moduleHandlerFactory);
    @Path("/{thingUID}")
    @Operation(operationId = "getAvailableActionsForThing", summary = "Get all available actions for provided thing UID", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = ThingActionDTO.class), uniqueItems = true))),
            @ApiResponse(responseCode = "204", description = "No actions found") })
    public Response getActions(@PathParam("thingUID") @Parameter(description = "thingUID") String thingUID,
            @HeaderParam(HttpHeaders.ACCEPT_LANGUAGE) @Parameter(description = "language") @Nullable String language) {
        ThingUID aThingUID = new ThingUID(thingUID);
        List<ThingActionDTO> actions = new ArrayList<>();
        Map<String, List<String>> thingActionsMap = this.thingActionsMap.get(aThingUID);
        if (thingActionsMap == null) {
            return Response.noContent().build();
        // inspect ThingActions
        for (Map.Entry<String, List<String>> thingActionsEntry : thingActionsMap.entrySet()) {
            for (String actionUID : thingActionsEntry.getValue()) {
                ActionType actionType = (ActionType) moduleTypeRegistry.get(actionUID, locale);
                if (actionType == null) {
                // Filter the configuration description parameters that correspond to inputs
                List<ConfigDescriptionParameter> inputParameters = new ArrayList<>();
                for (ConfigDescriptionParameter parameter : actionType.getConfigurationDescriptions()) {
                    if (actionType.getInputs().stream().anyMatch(i -> i.getName().equals(parameter.getName()))) {
                        inputParameters.add(parameter);
                // If the resulting list of configuration description parameters is empty while the list of
                // inputs is not empty, this is because the conversion of inputs into configuration description
                // parameters failed for at least one input
                if (inputParameters.isEmpty() && !actionType.getInputs().isEmpty()) {
                    inputParameters = null;
                ThingActionDTO actionDTO = new ThingActionDTO();
                actionDTO.actionUid = actionType.getUID();
                actionDTO.description = actionType.getDescription();
                actionDTO.label = actionType.getLabel();
                actionDTO.inputs = actionType.getInputs();
                actionDTO.inputConfigDescriptions = inputParameters == null ? null
                        : ConfigDescriptionDTOMapper.mapParameters(inputParameters);
                actionDTO.outputs = actionType.getOutputs();
                actionDTO.visibility = actionType.getVisibility();
                actions.add(actionDTO);
        return Response.ok().entity(new Stream2JSONInputStream(actions.stream())).build();
    // accept actionUid in the form of "scope.actionTypeUid" or "scope.actionTypeUid#signatureHash"
    // # is URL encoded as %23
    @Path("/{thingUID}/{actionUid: [a-zA-Z0-9]+(\\-[a-zA-Z0-9]+)?\\.[a-zA-Z0-9]+(%23[A-Fa-f0-9]+)?}")
    @Operation(operationId = "executeThingAction", summary = "Executes a thing action.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = Map.class))),
            @ApiResponse(responseCode = "404", description = "Action not found"),
            @ApiResponse(responseCode = "500", description = "Creation of action handler or execution failed") })
    public Response executeThingAction(@PathParam("thingUID") @Parameter(description = "thingUID") String thingUID,
            @PathParam("actionUid") @Parameter(description = "action type UID (including scope, separated by '.')") String actionTypeUid,
            @HeaderParam(HttpHeaders.ACCEPT_LANGUAGE) @Parameter(description = "language") @Nullable String language,
            @Parameter(description = "action inputs as map (parameter name as key / argument as value)") Map<String, Object> actionInputs) {
        ActionType actionType = (ActionType) moduleTypeRegistry.get(actionTypeUid);
            return Response.status(Response.Status.NOT_FOUND).build();
        String ruleUID = UUID.randomUUID().toString();
        Configuration configuration = new Configuration();
        configuration.put("config", thingUID);
        Action action = ModuleBuilder.createAction().withConfiguration(configuration)
                .withId(UUID.randomUUID().toString()).withTypeUID(actionTypeUid).build();
        ModuleHandlerFactory moduleHandlerFactory = moduleHandlerFactories.stream()
                .filter(f -> f.getTypes().contains(actionTypeUid)).findFirst().orElse(null);
        if (moduleHandlerFactory == null) {
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR).build();
        ActionHandler handler = (ActionHandler) moduleHandlerFactory.getHandler(action, ruleUID);
        if (handler == null) {
            Map<String, @Nullable Object> returnValue = Objects.requireNonNullElse(
                    handler.execute(actionInputsHelper.mapSerializedInputsToActionInputs(actionType, actionInputs)),
                    Map.of());
            moduleHandlerFactory.ungetHandler(action, ruleUID, handler);
            return Response.ok(returnValue).build();
            return Response.status(Response.Status.INTERNAL_SERVER_ERROR).entity(e).build();
    private @Nullable String getScope(ThingActions actions) {
        if (scopeAnnotation == null) {
    @Schema(name = "ThingAction")
    private static class ThingActionDTO {
        @Schema(requiredMode = Schema.RequiredMode.REQUIRED)
        public String actionUid = "";
        public @Nullable String label;
        public @Nullable String description;
        public @Nullable Visibility visibility;
        public List<Input> inputs = new ArrayList<>();
        public @Nullable List<ConfigDescriptionParameterDTO> inputConfigDescriptions;
        public List<Output> outputs = new ArrayList<>();
