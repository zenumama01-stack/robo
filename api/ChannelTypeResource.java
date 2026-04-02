package org.openhab.core.io.rest.core.internal.channel;
import org.openhab.core.thing.dto.ChannelTypeDTO;
import org.openhab.core.thing.profiles.ProfileTypeRegistry;
import org.openhab.core.thing.profiles.TriggerProfileType;
import org.openhab.core.thing.type.ChannelKind;
import org.openhab.core.thing.type.ChannelType;
import org.openhab.core.thing.type.ChannelTypeRegistry;
import org.openhab.core.thing.type.ChannelTypeUID;
 * Provides access to ChannelType via REST.
 * @author Yannick Schaus - Added filter to getAll
 * @author Mark Herwege - added unit hint
@JaxrsName(ChannelTypeResource.PATH_CHANNEL_TYPES)
@Path(ChannelTypeResource.PATH_CHANNEL_TYPES)
@Tag(name = ChannelTypeResource.PATH_CHANNEL_TYPES)
public class ChannelTypeResource implements RESTResource {
    public static final String PATH_CHANNEL_TYPES = "channel-types";
    private final ChannelTypeRegistry channelTypeRegistry;
    private final ProfileTypeRegistry profileTypeRegistry;
    public ChannelTypeResource( //
            final @Reference ChannelTypeRegistry channelTypeRegistry,
            final @Reference ProfileTypeRegistry profileTypeRegistry) {
        this.channelTypeRegistry = channelTypeRegistry;
        this.profileTypeRegistry = profileTypeRegistry;
    @Operation(operationId = "getChannelTypes", summary = "Gets all available channel types.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = ChannelTypeDTO.class), uniqueItems = true))) })
            @QueryParam("prefixes") @Parameter(description = "filter UIDs by prefix (multiple comma-separated prefixes allowed, for example: 'system,mqtt')") @Nullable String prefixes) {
        Stream<ChannelTypeDTO> channelStream = channelTypeRegistry.getChannelTypes(locale).stream()
                .map(c -> convertToChannelTypeDTO(c, locale));
        if (prefixes != null) {
            Predicate<ChannelTypeDTO> filter = ct -> false;
            for (String prefix : prefixes.split(",")) {
                filter = filter.or(ct -> ct.UID.startsWith(prefix + ":"));
            channelStream = channelStream.filter(filter);
        return Response.ok(new Stream2JSONInputStream(channelStream)).build();
    @Path("/{channelTypeUID}")
    @Operation(operationId = "getChannelTypeByUID", summary = "Gets channel type by UID.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = ChannelTypeDTO.class))),
            @ApiResponse(responseCode = "400", description = "Bad request"),
            @ApiResponse(responseCode = "404", description = "Channel type with provided channelTypeUID does not exist.") })
            @PathParam("channelTypeUID") @Parameter(description = "channelTypeUID") String channelTypeUID,
        ChannelType channelType = channelTypeRegistry.getChannelType(new ChannelTypeUID(channelTypeUID), locale);
        if (channelType != null) {
            return Response.ok(convertToChannelTypeDTO(channelType, locale)).build();
    @Path("/{channelTypeUID}/linkableItemTypes")
    @Operation(operationId = "getLinkableItemTypesByChannelTypeUID", summary = "Gets the item types the given trigger channel type UID can be linked to.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = String.class), uniqueItems = true))),
            @ApiResponse(responseCode = "204", description = "No content: channel type has no linkable items or is no trigger channel."),
            @ApiResponse(responseCode = "404", description = "Given channel type UID not found.") })
    public Response getLinkableItemTypes(
            @PathParam("channelTypeUID") @Parameter(description = "channelTypeUID") String channelTypeUID) {
        ChannelTypeUID ctUID = new ChannelTypeUID(channelTypeUID);
        ChannelType channelType = channelTypeRegistry.getChannelType(ctUID);
        if (channelType == null) {
        if (channelType.getKind() != ChannelKind.TRIGGER) {
        Set<String> result = new HashSet<>();
        for (ProfileType profileType : profileTypeRegistry.getProfileTypes()) {
            if (profileType instanceof TriggerProfileType type) {
                if (type.getSupportedChannelTypeUIDs().contains(ctUID)) {
                    result.addAll(profileType.getSupportedItemTypes());
        if (result.isEmpty()) {
    private ChannelTypeDTO convertToChannelTypeDTO(ChannelType channelType, Locale locale) {
        final URI descURI = channelType.getConfigDescriptionURI();
        final ConfigDescription configDescription = descURI == null ? null
                : configDescriptionRegistry.getConfigDescription(descURI, locale);
        final ConfigDescriptionDTO configDescriptionDTO = configDescription == null ? null
                : ConfigDescriptionDTOMapper.map(configDescription);
        final List<ConfigDescriptionParameterDTO> parameters = configDescriptionDTO == null ? List.of()
                : configDescriptionDTO.parameters;
        final List<ConfigDescriptionParameterGroupDTO> parameterGroups = configDescriptionDTO == null ? List.of()
                : configDescriptionDTO.parameterGroups;
        return new ChannelTypeDTO(channelType.getUID().toString(), channelType.getLabel(), channelType.getDescription(),
                channelType.getCategory(), channelType.getItemType(), channelType.getUnitHint(), channelType.getKind(),
                parameters, parameterGroups, channelType.getState(), channelType.getTags(), channelType.isAdvanced(),
                channelType.getCommandDescription());
