 * This is a REST resource that provides information about available icon sets.
@JaxrsName(IconSetResource.PATH_ICONSETS)
@Path(IconSetResource.PATH_ICONSETS)
@Tag(name = IconSetResource.PATH_ICONSETS)
public class IconSetResource implements RESTResource {
    public static final String PATH_ICONSETS = "iconsets";
    private final List<IconProvider> iconProviders = new ArrayList<>(5);
    public IconSetResource(final @Reference LocaleService localeService) {
    protected void addIconProvider(IconProvider iconProvider) {
        this.iconProviders.add(iconProvider);
    protected void removeIconProvider(IconProvider iconProvider) {
        this.iconProviders.remove(iconProvider);
    @Operation(operationId = "getIconSets", summary = "Gets all icon sets.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = IconSet.class)))) })
        List<IconSet> iconSets = new ArrayList<>(iconProviders.size());
        for (IconProvider iconProvider : iconProviders) {
            iconSets.addAll(iconProvider.getIconSets(locale));
        return Response.ok(iconSets).build();
