package org.openhab.core.io.rest.transform.internal;
import org.openhab.core.io.rest.transform.TransformationDTO;
import org.openhab.core.transform.ManagedTransformationProvider;
 * The {@link TransformationResource} is a REST resource for handling transformations
@JaxrsName(TransformationResource.PATH_TRANSFORMATIONS)
@Path(TransformationResource.PATH_TRANSFORMATIONS)
@Tag(name = TransformationResource.PATH_TRANSFORMATIONS)
public class TransformationResource implements RESTResource {
    public static final String PATH_TRANSFORMATIONS = "transformations";
    private final Logger logger = LoggerFactory.getLogger(TransformationResource.class);
    private final ManagedTransformationProvider managedTransformationProvider;
    private final RegistryChangedRunnableListener<Transformation> resetLastModifiedChangeListener = new RegistryChangedRunnableListener<>(
    public TransformationResource(final @Reference TransformationRegistry transformationRegistry,
            final @Reference ManagedTransformationProvider managedTransformationProvider,
            final BundleContext bundleContext) {
        this.managedTransformationProvider = managedTransformationProvider;
        this.transformationRegistry.addRegistryChangeListener(resetLastModifiedChangeListener);
        this.transformationRegistry.removeRegistryChangeListener(resetLastModifiedChangeListener);
    @Operation(operationId = "getTransformations", summary = "Get a list of all transformations", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = TransformationDTO.class)))) })
    public Response getTransformations(@Context Request request) {
        Stream<TransformationDTO> stream = transformationRegistry.stream().map(TransformationDTO::new)
                .peek(c -> c.editable = isEditable(c.uid));
        return Response.ok(new Stream2JSONInputStream(stream)).lastModified(lastModified)
    @Path("services")
    @Operation(operationId = "getTransformationServices", summary = "Get all transformation services", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = String.class)))) })
    public Response getTransformationServices() {
            Collection<ServiceReference<TransformationService>> refs = bundleContext
                    .getServiceReferences(TransformationService.class, null);
            Stream<String> services = refs.stream()
                    .map(ref -> (String) ref.getProperty(TransformationService.SERVICE_PROPERTY_NAME))
                    .filter(Objects::nonNull).map(Objects::requireNonNull).sorted();
            return Response.ok(new Stream2JSONInputStream(services)).build();
    @Path("{uid}")
    @Operation(operationId = "getTransformation", summary = "Get a single transformation", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = Transformation.class))),
    public Response getTransformation(@PathParam("uid") @Parameter(description = "Transformation UID") String uid) {
        Transformation transformation = transformationRegistry.get(uid);
        if (transformation == null) {
        TransformationDTO dto = new TransformationDTO(transformation);
        dto.editable = isEditable(uid);
    @Operation(operationId = "putTransformation", summary = "Put a single transformation", responses = {
            @ApiResponse(responseCode = "400", description = "Bad Request (content missing or invalid)"),
            @ApiResponse(responseCode = "405", description = "Transformation not editable") })
    public Response putTransformation(@PathParam("uid") @Parameter(description = "Transformation UID") String uid,
            @Parameter(description = "transformation", required = true) @Nullable TransformationDTO newTransformation) {
        logger.debug("Received HTTP PUT request at '{}'", uriInfo.getPath());
        Transformation oldTransformation = transformationRegistry.get(uid);
        if (oldTransformation != null && !isEditable(uid)) {
            return Response.status(Response.Status.METHOD_NOT_ALLOWED).build();
        if (newTransformation == null) {
            return Response.status(Response.Status.BAD_REQUEST).entity("Content missing.").build();
        if (!uid.equals(newTransformation.uid)) {
            return Response.status(Response.Status.BAD_REQUEST).entity("UID of transformation and path not matching.")
        Transformation transformation = new Transformation(newTransformation.uid, newTransformation.label,
                newTransformation.type, newTransformation.configuration);
            if (oldTransformation != null) {
                managedTransformationProvider.update(transformation);
                managedTransformationProvider.add(transformation);
            return Response.status(Response.Status.BAD_REQUEST).entity(Objects.requireNonNullElse(e.getMessage(), ""))
    @Operation(operationId = "deleteTransformation", summary = "Get a single transformation", responses = {
            @ApiResponse(responseCode = "404", description = "UID not found"),
    public Response deleteTransformation(@PathParam("uid") @Parameter(description = "Transformation UID") String uid) {
        logger.debug("Received HTTP DELETE request at '{}'", uriInfo.getPath());
        if (oldTransformation == null) {
        if (!isEditable(uid)) {
        managedTransformationProvider.remove(uid);
    private boolean isEditable(String uid) {
        return managedTransformationProvider.get(uid) != null;
