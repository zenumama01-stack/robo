import static org.openhab.core.automation.RulePredicates.*;
import java.util.function.Predicate;
import javax.annotation.security.RolesAllowed;
import javax.ws.rs.Consumes;
import javax.ws.rs.DELETE;
import javax.ws.rs.DefaultValue;
import javax.ws.rs.POST;
import javax.ws.rs.PUT;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.Request;
import javax.ws.rs.core.SecurityContext;
import javax.ws.rs.core.UriInfo;
import org.openhab.core.auth.Role;
import org.openhab.core.automation.ManagedRuleProvider;
import org.openhab.core.automation.RuleExecution;
import org.openhab.core.automation.RuleManager;
import org.openhab.core.automation.dto.ActionDTO;
import org.openhab.core.automation.dto.ActionDTOMapper;
import org.openhab.core.automation.dto.ConditionDTO;
import org.openhab.core.automation.dto.ConditionDTOMapper;
import org.openhab.core.automation.dto.ModuleDTO;
import org.openhab.core.automation.dto.RuleDTO;
import org.openhab.core.automation.dto.RuleDTOMapper;
import org.openhab.core.automation.dto.TriggerDTO;
import org.openhab.core.automation.dto.TriggerDTOMapper;
import org.openhab.core.automation.events.AutomationEventFactory;
import org.openhab.core.automation.rest.internal.dto.EnrichedRuleDTO;
import org.openhab.core.automation.rest.internal.dto.EnrichedRuleDTOMapper;
import org.openhab.core.common.registry.RegistryChangedRunnableListener;
import org.openhab.core.config.core.ConfigUtil;
import org.openhab.core.i18n.TimeZoneProvider;
import org.openhab.core.io.rest.DTOMapper;
import org.openhab.core.io.rest.JSONResponse;
import org.openhab.core.io.rest.Stream2JSONInputStream;
import io.swagger.v3.oas.annotations.headers.Header;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
 * This class acts as a REST resource for rules.
@JaxrsName(RuleResource.PATH_RULES)
@Path(RuleResource.PATH_RULES)
@RolesAllowed({ Role.ADMIN })
@SecurityRequirement(name = "oauth2", scopes = { "admin" })
@Tag(name = RuleResource.PATH_RULES)
public class RuleResource implements RESTResource {
    public static final String PATH_RULES = "rules";
    private final Logger logger = LoggerFactory.getLogger(RuleResource.class);
    private final DTOMapper dtoMapper;
    private final RuleManager ruleManager;
    private final ManagedRuleProvider managedRuleProvider;
    private final TimeZoneProvider timeZoneProvider;
    private final RegistryChangedRunnableListener<Rule> resetLastModifiedChangeListener = new RegistryChangedRunnableListener<>(
            () -> lastModified = null);
    private @Context @NonNullByDefault({}) UriInfo uriInfo;
    private @Nullable Date lastModified = null;
    public RuleResource( //
            final @Reference DTOMapper dtoMapper, //
            final @Reference RuleManager ruleManager, //
            final @Reference RuleRegistry ruleRegistry, //
            final @Reference ManagedRuleProvider managedRuleProvider, //
            final @Reference TimeZoneProvider timeZoneProvider) {
        this.dtoMapper = dtoMapper;
        this.ruleManager = ruleManager;
        this.managedRuleProvider = managedRuleProvider;
        this.timeZoneProvider = timeZoneProvider;
        this.ruleRegistry.addRegistryChangeListener(resetLastModifiedChangeListener);
    void deactivate() {
        this.ruleRegistry.removeRegistryChangeListener(resetLastModifiedChangeListener);
    @RolesAllowed({ Role.USER, Role.ADMIN })
    @Operation(operationId = "getRules", summary = "Get available rules, optionally filtered by tags and/or prefix.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = EnrichedRuleDTO.class)))) })
    public Response get(@Context SecurityContext securityContext, @Context Request request,
            @QueryParam("prefix") final @Nullable String prefix, @QueryParam("tags") final @Nullable List<String> tags,
            @QueryParam("summary") @Parameter(description = "summary fields only") @Nullable Boolean summary,
            @DefaultValue("false") @QueryParam("staticDataOnly") @Parameter(description = "provides a cacheable list of values not expected to change regularly and honors the If-Modified-Since header, all other parameters are ignored") boolean staticDataOnly) {
        if ((summary == null || !summary) && !securityContext.isUserInRole(Role.ADMIN)) {
            // users may only access the summary
            return JSONResponse.createErrorResponse(Status.UNAUTHORIZED, "Authentication required");
        if (staticDataOnly) {
            if (lastModified != null) {
                Response.ResponseBuilder responseBuilder = request.evaluatePreconditions(lastModified);
                if (responseBuilder != null) {
                    // send 304 Not Modified
                    return responseBuilder.build();
                lastModified = Date.from(Instant.now().truncatedTo(ChronoUnit.SECONDS));
            Stream<EnrichedRuleDTO> rules = ruleRegistry.stream()
                    .map(rule -> EnrichedRuleDTOMapper.map(rule, ruleManager, managedRuleProvider));
            rules = dtoMapper.limitToFields(rules, "uid,templateUID,name,visibility,description,tags,editable");
            return Response.ok(new Stream2JSONInputStream(rules)).lastModified(lastModified)
                    .cacheControl(RESTConstants.CACHE_CONTROL).build();
        // match all
        Predicate<Rule> p = r -> true;
        // prefix parameter has been used
        if (prefix != null) {
            // works also for null prefix
            // (empty prefix used if searching for rules without prefix)
            p = p.and(hasPrefix(prefix));
        // if tags is null or empty list returns all rules
        p = p.and(hasAllTags(tags));
        Stream<EnrichedRuleDTO> rules = ruleRegistry.stream().filter(p) // filter according to Predicates
                .map(rule -> EnrichedRuleDTOMapper.map(rule, ruleManager, managedRuleProvider)); // map matching rules
        if (summary != null && summary) {
            rules = dtoMapper.limitToFields(rules,
                    "uid,templateUID,templateState,name,visibility,description,status,tags,editable");
        return Response.ok(new Stream2JSONInputStream(rules)).build();
    @POST
    @Consumes(MediaType.APPLICATION_JSON)
    @Operation(operationId = "createRule", summary = "Creates a rule.", responses = {
            @ApiResponse(responseCode = "201", description = "Created", headers = @Header(name = "Location", description = "Newly created Rule", schema = @Schema(implementation = String.class))),
            @ApiResponse(responseCode = "409", description = "Creation of the rule is refused. Rule with the same UID already exists."),
            @ApiResponse(responseCode = "400", description = "Creation of the rule is refused. Missing required parameter.") })
    public Response create(@Parameter(description = "rule data", required = true) RuleDTO rule) throws IOException {
            final Rule newRule = ruleRegistry.add(RuleDTOMapper.map(rule));
            return Response.status(Status.CREATED)
                    .header("Location", "rules/" + URLEncoder.encode(newRule.getUID(), StandardCharsets.UTF_8)).build();
            String errMessage = "Creation of the rule is refused: " + e.getMessage();
            logException(e, errMessage);
            return JSONResponse.createErrorResponse(Status.CONFLICT, errMessage);
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, errMessage);
    private void logException(RuntimeException e, String errMessage) {
            logger.warn("{}", errMessage, e);
            logger.warn("{}", errMessage);
    @Path("/{ruleUID}")
    @Operation(operationId = "getRuleById", summary = "Gets the rule corresponding to the given UID.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = EnrichedRuleDTO.class))),
            @ApiResponse(responseCode = "404", description = "Rule not found") })
    public Response getByUID(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID) {
        Rule rule = ruleRegistry.get(ruleUID);
            return Response.ok(EnrichedRuleDTOMapper.map(rule, ruleManager, managedRuleProvider)).build();
    @DELETE
    @Operation(operationId = "deleteRule", summary = "Removes an existing rule corresponding to the given UID.", responses = {
            @ApiResponse(responseCode = "200", description = "OK"),
            @ApiResponse(responseCode = "404", description = "Rule corresponding to the given UID does not found.") })
    public Response remove(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID) {
        Rule removedRule = ruleRegistry.remove(ruleUID);
        if (removedRule == null) {
            logger.info("Received HTTP DELETE request at '{}' for the unknown rule '{}'.", uriInfo.getPath(), ruleUID);
        return Response.ok(null, MediaType.TEXT_PLAIN).build();
    @PUT
    @Operation(operationId = "updateRule", summary = "Updates an existing rule corresponding to the given UID.", responses = {
    public Response update(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID,
            @Parameter(description = "rule data", required = true) RuleDTO rule) throws IOException {
        rule.uid = ruleUID;
        final Rule oldRule = ruleRegistry.update(RuleDTOMapper.map(rule));
        if (oldRule == null) {
            logger.info("Received HTTP PUT request for update at '{}' for the unknown rule '{}'.", uriInfo.getPath(),
                    ruleUID);
    @Path("/{ruleUID}/config")
    @Operation(operationId = "getRuleConfiguration", summary = "Gets the rule configuration values.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = String.class))),
    public Response getConfiguration(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID)
        if (rule == null) {
            logger.info("Received HTTP GET request for config at '{}' for the unknown rule '{}'.", uriInfo.getPath(),
            return Response.ok(rule.getConfiguration().getProperties()).build();
    @Operation(operationId = "updateRuleConfiguration", summary = "Sets the rule configuration values.", responses = {
    public Response updateConfiguration(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID,
            @Parameter(description = "config") Map<String, @Nullable Object> configurationParameters)
        Map<String, @Nullable Object> config = ConfigUtil.normalizeTypes(configurationParameters);
            logger.info("Received HTTP PUT request for update config at '{}' for the unknown rule '{}'.",
                    uriInfo.getPath(), ruleUID);
            rule = RuleBuilder.create(rule).withConfiguration(new Configuration(config)).build();
            ruleRegistry.update(rule);
    @Path("/{ruleUID}/enable")
    @Consumes(MediaType.TEXT_PLAIN)
    @Operation(operationId = "enableRule", summary = "Sets the rule enabled status.", responses = {
            @ApiResponse(responseCode = "404", description = "Rule corresponding to the given UID was not found.") })
    public Response enableRule(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID,
            @Parameter(description = "enable", required = true) String enabled) throws IOException {
            logger.info("Received HTTP POST request for set enabled at '{}' for the unknown rule '{}'.",
            ruleManager.setEnabled(ruleUID, !"false".equalsIgnoreCase(enabled));
    @Path("/{ruleUID}/regenerate")
    @Operation(operationId = "regenerateRule", summary = "Regenerates the rule from its template.", responses = {
            @ApiResponse(responseCode = "404", description = "A template-based rule with the given UID was not found.") })
    public Response regenerateRule(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID)
            logger.info(
                    "Received HTTP POST request for regenerating rule from template at '{}' for an invalid rule UID '{}'.",
    @Path("/{ruleUID}/runnow")
    @Operation(operationId = "runRuleNow", summary = "Executes actions of the rule.", responses = {
    public Response runNow(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID,
            @Nullable @Parameter(description = "the context for running this rule", allowEmptyValue = true) Map<String, Object> context)
            logger.info("Received HTTP POST request for run now at '{}' for the unknown rule '{}'.", uriInfo.getPath(),
            if (context == null || context.isEmpty()) {
                // only add event to context if no context given, otherwise it might interfere with the intention of the
                // provided context
                Event event = AutomationEventFactory.createExecutionEvent(ruleUID, null, "manual");
                ruleManager.runNow(ruleUID, false, Map.of("event", event));
                ruleManager.runNow(ruleUID, false, context);
            return Response.ok().build();
    @Operation(deprecated = true, operationId = "runRuleNow", summary = "Executes actions of the rule.", responses = {
    public Response runNow(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID)
        return runNow(ruleUID, null);
    @Path("/{ruleUID}/triggers")
    @Operation(operationId = "getRuleTriggers", summary = "Gets the rule triggers.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = TriggerDTO.class)))),
    public Response getTriggers(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID) {
            return Response.ok(TriggerDTOMapper.map(rule.getTriggers())).build();
    @Path("/schedule/simulations")
    @Operation(operationId = "getScheduleRuleSimulations", summary = "Simulates the executions of rules filtered by tag 'Schedule' within the given times.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = RuleExecution.class)))),
            @ApiResponse(responseCode = "400", description = "The max. simulation duration of 180 days is exceeded.") })
    public Response simulateRules(
            @Parameter(description = "Start time of the simulated rule executions. Will default to the current time. ["
                    + DateTimeType.DATE_PATTERN_WITH_TZ_AND_MS + "]") @QueryParam("from") @Nullable String from,
            @Parameter(description = "End time of the simulated rule executions. Will default to 30 days after the start time. Must be less than 180 days after the given start time. ["
                    + DateTimeType.DATE_PATTERN_WITH_TZ_AND_MS + "]") @QueryParam("until") @Nullable String until) {
        final ZonedDateTime fromDate = parseTime(from, ZonedDateTime::now);
        final ZonedDateTime untilDate = parseTime(until, () -> fromDate.plusDays(31));
        if (ChronoUnit.DAYS.between(fromDate, untilDate) >= 180) {
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST,
                    "Simulated time span must be smaller than 180 days.");
        final Stream<RuleExecution> ruleExecutions = ruleManager.simulateRuleExecutions(fromDate, untilDate);
        return Response.ok(ruleExecutions.toList()).build();
    private ZonedDateTime parseTime(@Nullable String sTime, Supplier<ZonedDateTime> defaultSupplier) {
        if (sTime == null || sTime.isEmpty()) {
            return defaultSupplier.get();
        final DateTimeType dateTime = new DateTimeType(sTime);
        return dateTime.getZonedDateTime(timeZoneProvider.getTimeZone());
    @Path("/{ruleUID}/conditions")
    @Operation(operationId = "getRuleConditions", summary = "Gets the rule conditions.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = ConditionDTO.class)))),
    public Response getConditions(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID) {
            return Response.ok(ConditionDTOMapper.map(rule.getConditions())).build();
    @Path("/{ruleUID}/actions")
    @Operation(operationId = "getRuleActions", summary = "Gets the rule actions.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = ActionDTO.class)))),
    public Response getActions(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID) {
            return Response.ok(ActionDTOMapper.map(rule.getActions())).build();
    @Path("/{ruleUID}/{moduleCategory}/{id}")
    @Operation(operationId = "getRuleModuleById", summary = "Gets the rule's module corresponding to the given Category and ID.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = ModuleDTO.class))),
            @ApiResponse(responseCode = "404", description = "Rule corresponding to the given UID does not found or does not have a module with such Category and ID.") })
    public Response getModuleById(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID,
            @PathParam("moduleCategory") @Parameter(description = "moduleCategory") String moduleCategory,
            @PathParam("id") @Parameter(description = "id") String id) {
            final ModuleDTO dto = getModuleDTO(rule, moduleCategory, id);
            if (dto != null) {
                return Response.ok(dto).build();
    @Path("/{ruleUID}/{moduleCategory}/{id}/config")
    @Operation(operationId = "getRuleModuleConfig", summary = "Gets the module's configuration.", responses = {
    public Response getModuleConfig(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID,
            Module module = getModule(rule, moduleCategory, id);
            if (module != null) {
                return Response.ok(module.getConfiguration().getProperties()).build();
    @Path("/{ruleUID}/{moduleCategory}/{id}/config/{param}")
    @Produces(MediaType.TEXT_PLAIN)
    @Operation(operationId = "getRuleModuleConfigParameter", summary = "Gets the module's configuration parameter.", responses = {
    public Response getModuleConfigParam(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID,
            @PathParam("id") @Parameter(description = "id") String id,
            @PathParam("param") @Parameter(description = "param") String param) {
                return Response.ok(module.getConfiguration().getProperties().get(param)).build();
    @Operation(operationId = "setRuleModuleConfigParameter", summary = "Sets the module's configuration parameter value.", responses = {
    public Response setModuleConfigParam(@PathParam("ruleUID") @Parameter(description = "ruleUID") String ruleUID,
            @PathParam("param") @Parameter(description = "param") String param,
            @Parameter(description = "value", required = true) String value) {
                Configuration configuration = module.getConfiguration();
                configuration.put(param, ConfigUtil.normalizeType(value));
                module = ModuleBuilder.create(module).withConfiguration(configuration).build();
    protected @Nullable <T extends Module> T getModuleById(final @Nullable Collection<T> coll, final String id) {
        if (coll == null) {
        for (final T module : coll) {
            if (module.getId().equals(id)) {
    protected @Nullable Trigger getTrigger(Rule rule, String id) {
        return getModuleById(rule.getTriggers(), id);
    protected @Nullable Condition getCondition(Rule rule, String id) {
        return getModuleById(rule.getConditions(), id);
    protected @Nullable Action getAction(Rule rule, String id) {
        return getModuleById(rule.getActions(), id);
    protected @Nullable Module getModule(Rule rule, String moduleCategory, String id) {
        if ("triggers".equals(moduleCategory)) {
            return getTrigger(rule, id);
        } else if ("conditions".equals(moduleCategory)) {
            return getCondition(rule, id);
        } else if ("actions".equals(moduleCategory)) {
            return getAction(rule, id);
    protected @Nullable ModuleDTO getModuleDTO(Rule rule, String moduleCategory, String id) {
            final Trigger trigger = getTrigger(rule, id);
            return trigger == null ? null : TriggerDTOMapper.map(trigger);
            final Condition condition = getCondition(rule, id);
            return condition == null ? null : ConditionDTOMapper.map(condition);
            final Action action = getAction(rule, id);
            return action == null ? null : ActionDTOMapper.map(action);
