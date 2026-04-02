package org.openhab.core.io.rest.ui.internal;
import java.security.InvalidParameterException;
import org.openhab.core.io.rest.ui.TileDTO;
import org.openhab.core.ui.tiles.Tile;
import org.openhab.core.ui.tiles.TileProvider;
 * This class acts as a REST resource for the UI resources and is registered with the
@Component(service = { RESTResource.class, UIResource.class })
@JaxrsName(UIResource.PATH_UI)
@Path(UIResource.PATH_UI)
@Tag(name = UIResource.PATH_UI)
public class UIResource implements RESTResource {
    public static final String PATH_UI = "ui";
    private final UIComponentRegistryFactory componentRegistryFactory;
    private final TileProvider tileProvider;
    private Map<String, Date> lastModifiedDates = new HashMap<>();
    private Map<String, RegistryChangeListener<RootUIComponent>> registryChangeListeners = new HashMap<>();
    public UIResource( //
            final @Reference UIComponentRegistryFactory componentRegistryFactory,
            final @Reference TileProvider tileProvider) {
        this.componentRegistryFactory = componentRegistryFactory;
        this.tileProvider = tileProvider;
        registryChangeListeners.forEach((n, l) -> {
            UIComponentRegistry registry = componentRegistryFactory.getRegistry(n);
            registry.removeRegistryChangeListener(l);
    @Path("/tiles")
    @Operation(operationId = "getUITiles", summary = "Get all registered UI tiles.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = TileDTO.class)))) })
    public Response getAll() {
        Stream<TileDTO> tiles = tileProvider.getTiles().map(this::toTileDTO);
        return Response.ok(new Stream2JSONInputStream(tiles)).build();
    @Path("/components/{namespace}")
    @Operation(operationId = "getRegisteredUIComponentsInNamespace", summary = "Get all registered UI components in the specified namespace.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = RootUIComponent.class)))) })
    public Response getAllComponents(@Context Request request, @PathParam("namespace") String namespace,
            @QueryParam("summary") @Parameter(description = "summary fields only") @Nullable Boolean summary) {
        UIComponentRegistry registry = componentRegistryFactory.getRegistry(namespace);
        Stream<RootUIComponent> components = registry.getAll().stream();
            components = components.map(c -> {
                RootUIComponent component = new RootUIComponent(c.getUID(), c.getType());
                Set<String> tags = c.getTags();
                if (tags != null) {
                    component.addTags(c.getTags());
                Date timestamp = c.getTimestamp();
                if (timestamp != null) {
                    component.setTimestamp(timestamp);
            return Response.ok(new Stream2JSONInputStream(components)).build();
            if (!registryChangeListeners.containsKey(namespace)) {
                RegistryChangeListener<RootUIComponent> changeListener = new ResetLastModifiedChangeListener(namespace);
                registryChangeListeners.put(namespace, changeListener);
                registry.addRegistryChangeListener(changeListener);
            Date lastModifiedDate = Date.from(Instant.now());
            if (lastModifiedDates.containsKey(namespace)) {
                lastModifiedDate = lastModifiedDates.get(namespace);
                Response.ResponseBuilder responseBuilder = request.evaluatePreconditions(lastModifiedDate);
                lastModifiedDate = Date.from(Instant.now().truncatedTo(ChronoUnit.SECONDS));
                lastModifiedDates.put(namespace, lastModifiedDate);
            return Response.ok(new Stream2JSONInputStream(components)).lastModified(lastModifiedDate)
    @Path("/components/{namespace}/{componentUID}")
    @Operation(operationId = "getUIComponentInNamespace", summary = "Get a specific UI component in the specified namespace.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = RootUIComponent.class))),
            @ApiResponse(responseCode = "404", description = "Component not found") })
    public Response getComponentByUID(@PathParam("namespace") String namespace,
            @PathParam("componentUID") String componentUID) {
        RootUIComponent component = registry.get(componentUID);
        if (component == null) {
        return Response.ok(component).build();
    @Operation(operationId = "addUIComponentToNamespace", summary = "Add a UI component in the specified namespace.", security = {
                    @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = RootUIComponent.class))) })
    public Response addComponent(@PathParam("namespace") String namespace, RootUIComponent component) {
        component.updateTimestamp();
        RootUIComponent createdComponent = registry.add(component);
        return Response.ok(createdComponent).build();
    @Operation(operationId = "updateUIComponentInNamespace", summary = "Update a specific UI component in the specified namespace.", security = {
    public Response updateComponent(@PathParam("namespace") String namespace,
            @PathParam("componentUID") String componentUID, RootUIComponent component) {
        RootUIComponent existingComponent = registry.get(componentUID);
        if (existingComponent == null) {
        if (!componentUID.equals(component.getUID())) {
            throw new InvalidParameterException(
                    "The component UID in the body of the request should match the UID in the URL");
        registry.update(component);
    @Operation(operationId = "removeUIComponentFromNamespace", summary = "Remove a specific UI component in the specified namespace.", security = {
    public Response deleteComponent(@PathParam("namespace") String namespace,
        registry.remove(componentUID);
    private TileDTO toTileDTO(Tile tile) {
        return new TileDTO(tile.getName(), tile.getUrl(), tile.getOverlay(), tile.getImageUrl());
    private void resetLastModifiedDate(String namespace) {
        lastModifiedDates.remove(namespace);
    private class ResetLastModifiedChangeListener implements RegistryChangeListener<RootUIComponent> {
        private String namespace;
        ResetLastModifiedChangeListener(String namespace) {
            this.namespace = namespace;
        public void added(RootUIComponent element) {
            resetLastModifiedDate(namespace);
        public void removed(RootUIComponent element) {
        public void updated(RootUIComponent oldElement, RootUIComponent element) {
