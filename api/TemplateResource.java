import org.eclipse.jdt.annotation.NonNull;
import org.openhab.core.automation.template.Template;
import org.openhab.core.automation.template.TemplateRegistry;
 * This class acts as a REST resource for templates and is registered with the Jersey servlet.
@JaxrsName(TemplateResource.PATH_TEMPLATES)
@Path(TemplateResource.PATH_TEMPLATES)
@Tag(name = TemplateResource.PATH_TEMPLATES)
public class TemplateResource implements RESTResource {
    public static final String PATH_TEMPLATES = "templates";
    private final TemplateRegistry<@NonNull RuleTemplate> templateRegistry;
    public TemplateResource( //
            final @Reference LocaleService localeService,
            final @Reference TemplateRegistry<@NonNull RuleTemplate> templateRegistry) {
        this.templateRegistry = templateRegistry;
    @Operation(operationId = "getTemplates", summary = "Get all available templates.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = Template.class)))) })
            @HeaderParam("Accept-Language") @Parameter(description = "language") @Nullable String language) {
        Collection<RuleTemplateDTO> result = templateRegistry.getAll(locale).stream()
                .map(template -> RuleTemplateDTOMapper.map(template)).toList();
        return Response.ok(result).build();
    @Path("/{templateUID}")
    @Operation(operationId = "getTemplateById", summary = "Gets a template corresponding to the given UID.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = Template.class))),
            @ApiResponse(responseCode = "404", description = "Template corresponding to the given UID does not found.") })
            @PathParam("templateUID") @Parameter(description = "templateUID") String templateUID) {
        RuleTemplate template = templateRegistry.get(templateUID, locale);
        if (template != null) {
            return Response.ok(RuleTemplateDTOMapper.map(template)).build();
