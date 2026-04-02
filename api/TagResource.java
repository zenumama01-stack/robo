import org.openhab.core.semantics.ManagedSemanticTagProvider;
import org.openhab.core.semantics.SemanticTagImpl;
 * This class acts as a REST resource for retrieving a list of tags.
 * @author Laurent Garnier - Extend REST API to allow adding/updating/removing user tags
@JaxrsName(TagResource.PATH_TAGS)
@Path(TagResource.PATH_TAGS)
@io.swagger.v3.oas.annotations.tags.Tag(name = TagResource.PATH_TAGS)
public class TagResource implements RESTResource {
    public static final String PATH_TAGS = "tags";
    private final ManagedSemanticTagProvider managedSemanticTagProvider;
    private final RegistryChangedRunnableListener<SemanticTag> resetLastModifiedChangeListener = new RegistryChangedRunnableListener<>(
    // TODO pattern in @Path
    public TagResource(final @Reference LocaleService localeService,
            final @Reference ManagedSemanticTagProvider managedSemanticTagProvider) {
        this.managedSemanticTagProvider = managedSemanticTagProvider;
        this.semanticTagRegistry.addRegistryChangeListener(resetLastModifiedChangeListener);
        this.semanticTagRegistry.removeRegistryChangeListener(resetLastModifiedChangeListener);
    @Operation(operationId = "getSemanticTags", summary = "Get all available semantic tags.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = EnrichedSemanticTagDTO.class)))) })
    public Response getTags(final @Context Request request, final @Context UriInfo uriInfo,
            final @Context HttpHeaders httpHeaders,
        Stream<EnrichedSemanticTagDTO> tagsStream = semanticTagRegistry.getAll().stream()
                .sorted(Comparator.comparing(SemanticTag::getUID))
                .map(t -> new EnrichedSemanticTagDTO(t.localized(locale), semanticTagRegistry.isEditable(t)));
        return Response.ok(new Stream2JSONInputStream(tagsStream)).lastModified(lastModified)
    @Path("/{tagId}")
    @Operation(operationId = "getSemanticTagAndSubTags", summary = "Gets a semantic tag and its sub tags.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = EnrichedSemanticTagDTO.class)))),
            @ApiResponse(responseCode = "404", description = "Semantic tag not found.") })
    public Response getTagAndSubTags(final @Context Request request,
            @PathParam("tagId") @Parameter(description = "tag id") String tagId) {
        String uid = tagId.trim();
        SemanticTag tag = semanticTagRegistry.get(uid);
        if (tag != null) {
            Stream<EnrichedSemanticTagDTO> tagsStream = semanticTagRegistry.getSubTree(tag).stream()
            return JSONResponse.createErrorResponse(Status.NOT_FOUND, "Tag " + uid + " does not exist!");
    @Operation(operationId = "createSemanticTag", summary = "Creates a new semantic tag and adds it to the registry.", security = {
                    @ApiResponse(responseCode = "201", description = "Created", content = @Content(schema = @Schema(implementation = EnrichedSemanticTagDTO.class))),
                    @ApiResponse(responseCode = "400", description = "The tag identifier is invalid or the tag label is missing."),
                    @ApiResponse(responseCode = "409", description = "A tag with the same identifier already exists.") })
    public Response create(
            @Parameter(description = "tag data", required = true) EnrichedSemanticTagDTO data) {
        if (data.uid == null || data.uid.isBlank()) {
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "Tag identifier is required!");
        if (data.label == null || data.label.isBlank()) {
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "Tag label is required!");
        String uid = data.uid.trim();
        // check if a tag with this UID already exists
            // report a conflict
            return JSONResponse.createResponse(Status.CONFLICT,
                    new EnrichedSemanticTagDTO(tag.localized(locale), semanticTagRegistry.isEditable(tag)),
                    "Tag " + uid + " already exists!");
        tag = new SemanticTagImpl(uid, data.label, data.description, data.synonyms);
        // Check that a tag with this uid can be added to the registry
        if (!semanticTagRegistry.canBeAdded(tag)) {
            return JSONResponse.createErrorResponse(Status.BAD_REQUEST, "Invalid tag identifier " + uid);
        managedSemanticTagProvider.add(tag);
        return JSONResponse.createResponse(Status.CREATED,
                new EnrichedSemanticTagDTO(tag.localized(locale), semanticTagRegistry.isEditable(tag)), null);
    @Operation(operationId = "removeSemanticTag", summary = "Removes a semantic tag and its sub tags from the registry.", security = {
                    @ApiResponse(responseCode = "200", description = "OK, was deleted."),
                    @ApiResponse(responseCode = "404", description = "Semantic tag not found."),
                    @ApiResponse(responseCode = "405", description = "Semantic tag not removable.") })
    public Response remove(
        // check whether tag exists and throw 404 if not
        // Check that tag is removable, 405 otherwise
        if (!semanticTagRegistry.isEditable(tag)) {
            return JSONResponse.createErrorResponse(Status.METHOD_NOT_ALLOWED, "Tag " + uid + " is not removable.");
        semanticTagRegistry.removeSubTree(tag);
    @Operation(operationId = "updateSemanticTag", summary = "Updates a semantic tag.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = EnrichedSemanticTagDTO.class))),
                    @ApiResponse(responseCode = "405", description = "Semantic tag not editable.") })
    public Response update(
            @PathParam("tagId") @Parameter(description = "tag id") String tagId,
        // Check that tag is editable, 405 otherwise
            return JSONResponse.createErrorResponse(Status.METHOD_NOT_ALLOWED, "Tag " + uid + " is not editable.");
        tag = new SemanticTagImpl(uid, data.label != null ? data.label : tag.getLabel(),
                data.description != null ? data.description : tag.getDescription(),
                data.synonyms != null ? data.synonyms : tag.getSynonyms());
        managedSemanticTagProvider.update(tag);
        return JSONResponse.createResponse(Status.OK,
