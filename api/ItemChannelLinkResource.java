package org.openhab.core.io.rest.core.internal.link;
import org.openhab.core.io.rest.core.link.BrokenItemChannelLinkDTO;
import org.openhab.core.io.rest.core.link.EnrichedItemChannelLinkDTO;
import org.openhab.core.io.rest.core.link.EnrichedItemChannelLinkDTOMapper;
import org.openhab.core.thing.link.AbstractLink;
import org.openhab.core.thing.link.ItemChannelLinkRegistry.ItemChannelLinkProblem;
import org.openhab.core.thing.link.ManagedItemChannelLinkProvider;
import org.openhab.core.thing.link.dto.ItemChannelLinkDTO;
 * This class acts as a REST resource for links.
 * @author Kai Kreuzer - Removed Thing links and added auto link url
 * @author Yannick Schaus - Added filters to getAll
 * @author Arne Seime - Added orphan links detection
@Component(service = { RESTResource.class, ItemChannelLinkResource.class })
@JaxrsName(ItemChannelLinkResource.PATH_LINKS)
@Path(ItemChannelLinkResource.PATH_LINKS)
@Tag(name = ItemChannelLinkResource.PATH_LINKS)
public class ItemChannelLinkResource implements RESTResource {
    public static final String PATH_LINKS = "links";
    private final ManagedItemChannelLinkProvider managedItemChannelLinkProvider;
    public ItemChannelLinkResource(final @Reference ItemRegistry itemRegistry,
            final @Reference ThingRegistry thingRegistry, final @Reference ChannelTypeRegistry channelTypeRegistry,
            final @Reference ProfileTypeRegistry profileTypeRegistry,
            final @Reference ManagedItemChannelLinkProvider managedItemChannelLinkProvider) {
        this.managedItemChannelLinkProvider = managedItemChannelLinkProvider;
    @Operation(operationId = "getItemLinks", summary = "Gets all available links.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = EnrichedItemChannelLinkDTO.class)))) })
            @QueryParam("channelUID") @Parameter(description = "filter by channel UID") @Nullable String channelUID,
            @QueryParam("itemName") @Parameter(description = "filter by item name") @Nullable String itemName) {
        Stream<EnrichedItemChannelLinkDTO> linkStream = itemChannelLinkRegistry.stream()
                .map(link -> EnrichedItemChannelLinkDTOMapper.map(link,
                        isEditable(AbstractLink.getIDFor(link.getItemName(), link.getLinkedUID()))));
        if (channelUID != null) {
            linkStream = linkStream.filter(link -> channelUID.equals(link.channelUID));
            linkStream = linkStream.filter(link -> itemName.equals(link.itemName));
        return Response.ok(new Stream2JSONInputStream(linkStream)).build();
    @Path("/{object}")
    @Operation(operationId = "removeAllLinksForObject", summary = "Delete all links that refer to an item or thing.", security = {
    public Response removeAllLinksForObject(
            @PathParam("object") @Parameter(description = "item name or thing UID") String object) {
        int removedLinks;
            ThingUID thingUID = new ThingUID(object);
            removedLinks = itemChannelLinkRegistry.removeLinksForThing(thingUID);
            removedLinks = itemChannelLinkRegistry.removeLinksForItem(object);
        return Response.ok(Map.of("count", removedLinks)).build();
    @Path("/{itemName}/{channelUID}")
    @Operation(operationId = "getItemLink", summary = "Retrieves an individual link.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = EnrichedItemChannelLinkDTO.class))),
            @ApiResponse(responseCode = "404", description = "Content does not match the path") })
    public Response getLink(@PathParam("itemName") @Parameter(description = "item name") String itemName,
            @PathParam("channelUID") @Parameter(description = "channel UID") String channelUid) {
        List<EnrichedItemChannelLinkDTO> links = itemChannelLinkRegistry.stream()
                .filter(link -> channelUid.equals(link.getLinkedUID().getAsString()))
                .filter(link -> itemName.equals(link.getItemName())).map(link -> EnrichedItemChannelLinkDTOMapper
                        .map(link, isEditable(AbstractLink.getIDFor(link.getItemName(), link.getLinkedUID()))))
        if (!links.isEmpty()) {
            return JSONResponse.createResponse(Status.OK, links.getFirst(), null);
        return JSONResponse.createErrorResponse(Status.NOT_FOUND,
                "No link found for item '" + itemName + "' + and channelUID '" + channelUid + "'");
    @Operation(operationId = "linkItemToChannel", summary = "Links an item to a channel.", security = {
                    @ApiResponse(responseCode = "400", description = "Content does not match the path"),
                    @ApiResponse(responseCode = "405", description = "Link is not editable") })
    public Response link(@PathParam("itemName") @Parameter(description = "itemName") String itemName,
            @PathParam("channelUID") @Parameter(description = "channelUID") String channelUid,
            @Parameter(description = "link data") @Nullable ItemChannelLinkDTO bean) {
            item = itemRegistry.getItem(itemName);
        ChannelUID uid;
            uid = new ChannelUID(channelUid);
        Channel channel = getChannel(uid);
        if (channel == null) {
        ChannelKind channelKind = channel.getKind();
        if (ChannelKind.TRIGGER.equals(channelKind)) {
            String itemType = ItemUtil.getMainItemType(item.getType());
            if (bean == null || bean.configuration == null) {
                // configuration is needed because a profile is mandatory
            String profileUid = (String) bean.configuration.get("profile");
            if (profileUid == null) {
                // profile is mandatory for trigger channel links
            ProfileType profileType = profileTypeRegistry.getProfileTypes().stream()
                    .filter(p -> profileUid.equals(p.getUID().getAsString())).findFirst().orElse(null);
            if (!(profileType instanceof TriggerProfileType)) {
                // only trigger profiles are allowed
            if (!(profileType.getSupportedItemTypes().isEmpty()
                    || profileType.getSupportedItemTypes().contains(itemType))
                    || !(((TriggerProfileType) profileType).getSupportedChannelTypeUIDs().isEmpty()
                            || ((TriggerProfileType) profileType).getSupportedChannelTypeUIDs()
                                    .contains(channel.getChannelTypeUID()))) {
                // item or channel type not matching
        ItemChannelLink link;
        if (bean == null) {
            link = new ItemChannelLink(itemName, uid, new Configuration());
            if (bean.channelUID != null && !bean.channelUID.equals(channelUid)) {
            if (bean.itemName != null && !bean.itemName.equals(itemName)) {
            link = new ItemChannelLink(itemName, uid, new Configuration(bean.configuration));
        if (itemChannelLinkRegistry.get(link.getUID()) == null) {
            itemChannelLinkRegistry.add(link);
            ItemChannelLink oldLink = itemChannelLinkRegistry.update(link);
            if (oldLink == null) {
    @Operation(operationId = "unlinkItemFromChannel", summary = "Unlinks an item from a channel.", security = {
                    @ApiResponse(responseCode = "400", description = "Invalid channel UID."),
                    @ApiResponse(responseCode = "404", description = "Link not found."),
                    @ApiResponse(responseCode = "405", description = "Link not editable.") })
    public Response unlink(@PathParam("itemName") @Parameter(description = "itemName") String itemName,
            @PathParam("channelUID") @Parameter(description = "channelUID") String channelUid) {
        String linkId = AbstractLink.getIDFor(itemName, uid);
        if (itemChannelLinkRegistry.get(linkId) == null) {
            String message = "Link " + linkId + " does not exist!";
        ItemChannelLink result = itemChannelLinkRegistry.remove(linkId);
    @Path("/purge")
    @Operation(operationId = "purgeDatabase", summary = "Remove unused/orphaned links.", security = {
        itemChannelLinkRegistry.purge();
    private boolean isEditable(String linkId) {
        return managedItemChannelLinkProvider.get(linkId) != null;
    private @Nullable Channel getChannel(ChannelUID channelUID) {
        Thing thing = thingRegistry.get(channelUID.getThingUID());
        return thing.getChannel(channelUID);
    @Path("/orphans")
    @Operation(operationId = "getOrphanLinks", summary = "Get orphan links between items and broken/non-existent thing channels", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = BrokenItemChannelLinkDTO.class)))) })
    public Response getOrphanLinks() {
        Map<ItemChannelLink, ItemChannelLinkProblem> orphanLinks = itemChannelLinkRegistry.getOrphanLinks();
        List<BrokenItemChannelLinkDTO> brokenLinks = orphanLinks.entrySet().stream()
                .map(e -> new BrokenItemChannelLinkDTO(EnrichedItemChannelLinkDTOMapper.map(e.getKey(),
                        managedItemChannelLinkProvider.get(e.getKey().getUID()) != null), e.getValue()))
        return JSONResponse.createResponse(Status.OK, brokenLinks, null);
