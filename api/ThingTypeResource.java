import org.openhab.core.thing.dto.ChannelDefinitionDTO;
import org.openhab.core.thing.dto.ChannelGroupDefinitionDTO;
import org.openhab.core.thing.dto.StrippedThingTypeDTO;
import org.openhab.core.thing.dto.StrippedThingTypeDTOMapper;
import org.openhab.core.thing.dto.ThingTypeDTO;
import org.openhab.core.thing.type.ChannelDefinition;
import org.openhab.core.thing.type.ChannelGroupDefinition;
import org.openhab.core.thing.type.ChannelGroupType;
import org.openhab.core.thing.type.ChannelGroupTypeRegistry;
 * ThingTypeResource provides access to ThingType via REST.
 * @author Thomas Höfer - Added thing and thing type properties
 * @author Chris Jackson - Added parameter groups, advanced, multipleLimit,
 *         limitToOptions
 * @author Miki Jankov - Introducing StrippedThingTypeDTO
@JaxrsName(ThingTypeResource.PATH_THING_TYPES)
@Path(ThingTypeResource.PATH_THING_TYPES)
@Tag(name = ThingTypeResource.PATH_THING_TYPES)
public class ThingTypeResource implements RESTResource {
    public static final String PATH_THING_TYPES = "thing-types";
    private final Logger logger = LoggerFactory.getLogger(ThingTypeResource.class);
    private final ChannelGroupTypeRegistry channelGroupTypeRegistry;
    public ThingTypeResource( //
            final @Reference ChannelGroupTypeRegistry channelGroupTypeRegistry,
        this.channelGroupTypeRegistry = channelGroupTypeRegistry;
    @Operation(operationId = "getThingTypes", summary = "Gets all available thing types without config description, channels and properties.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = StrippedThingTypeDTO.class), uniqueItems = true))) })
            @QueryParam("bindingId") @Parameter(description = "filter by binding Id") @Nullable String bindingId) {
        Stream<StrippedThingTypeDTO> typeStream = thingTypeRegistry.getThingTypes(locale).stream()
                .map(thingType -> StrippedThingTypeDTOMapper.map(thingType, locale));
        if (bindingId != null) {
            typeStream = typeStream.filter(type -> type.UID.startsWith(bindingId + ':'));
        return Response.ok(new Stream2JSONInputStream(typeStream)).build();
    @Path("/{thingTypeUID}")
    @Operation(operationId = "getThingTypeById", summary = "Gets thing type by UID.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(schema = @Schema(implementation = ThingTypeDTO.class))),
            @ApiResponse(responseCode = "404", description = "Thing type not found.") })
    public Response getByUID(@PathParam("thingTypeUID") @Parameter(description = "thingTypeUID") String thingTypeUID,
        ThingType thingType = thingTypeRegistry.getThingType(new ThingTypeUID(thingTypeUID), locale);
            return Response.ok(convertToThingTypeDTO(thingType, locale)).build();
    private ThingTypeDTO convertToThingTypeDTO(ThingType thingType, Locale locale) {
        final ConfigDescription configDescription;
        final URI descURI = thingType.getConfigDescriptionURI();
        configDescription = descURI == null ? null : configDescriptionRegistry.getConfigDescription(descURI, locale);
        List<ConfigDescriptionParameterDTO> parameters;
        List<ConfigDescriptionParameterGroupDTO> parameterGroups;
            ConfigDescriptionDTO configDescriptionDTO = ConfigDescriptionDTOMapper.map(configDescription);
            parameters = configDescriptionDTO.parameters;
            parameterGroups = configDescriptionDTO.parameterGroups;
            parameters = new ArrayList<>(0);
            parameterGroups = new ArrayList<>(0);
        final List<ChannelDefinitionDTO> channelDefinitions = convertToChannelDefinitionDTOs(
                thingType.getChannelDefinitions(), locale);
        return new ThingTypeDTO(thingType.getUID().toString(), thingType.getLabel(), thingType.getDescription(),
                thingType.getCategory(), thingType.isListed(), parameters, channelDefinitions,
                convertToChannelGroupDefinitionDTOs(thingType.getChannelGroupDefinitions(), locale),
                thingType.getSupportedBridgeTypeUIDs(), thingType.getProperties(), thingType instanceof BridgeType,
                parameterGroups, thingType.getExtensibleChannelTypeIds(), thingType.getSemanticEquipmentTag());
    private List<ChannelGroupDefinitionDTO> convertToChannelGroupDefinitionDTOs(
            List<ChannelGroupDefinition> channelGroupDefinitions, Locale locale) {
        List<ChannelGroupDefinitionDTO> channelGroupDefinitionDTOs = new ArrayList<>();
        for (ChannelGroupDefinition channelGroupDefinition : channelGroupDefinitions) {
            String id = channelGroupDefinition.getId();
            ChannelGroupType channelGroupType = channelGroupTypeRegistry
                    .getChannelGroupType(channelGroupDefinition.getTypeUID(), locale);
            // Default to the channelGroupDefinition label/description to override the channelGroupType
            String label = channelGroupDefinition.getLabel();
            String description = channelGroupDefinition.getDescription();
            List<ChannelDefinition> channelDefinitions = List.of();
            if (channelGroupType == null) {
                logger.warn("Cannot find channel group type: {}", channelGroupDefinition.getTypeUID());
                    label = channelGroupType.getLabel();
                    description = channelGroupType.getDescription();
                channelDefinitions = channelGroupType.getChannelDefinitions();
            List<ChannelDefinitionDTO> channelDefinitionDTOs = convertToChannelDefinitionDTOs(channelDefinitions,
            channelGroupDefinitionDTOs
                    .add(new ChannelGroupDefinitionDTO(id, label, description, channelDefinitionDTOs));
        return channelGroupDefinitionDTOs;
    private List<ChannelDefinitionDTO> convertToChannelDefinitionDTOs(List<ChannelDefinition> channelDefinitions,
            Locale locale) {
        List<ChannelDefinitionDTO> channelDefinitionDTOs = new ArrayList<>();
        for (ChannelDefinition channelDefinition : channelDefinitions) {
            ChannelType channelType = channelTypeRegistry.getChannelType(channelDefinition.getChannelTypeUID(), locale);
                logger.warn("Cannot find channel type: {}", channelDefinition.getChannelTypeUID());
                // Default to the channelDefinition label to override the
                // channelType
                String label = channelDefinition.getLabel();
                    label = channelType.getLabel();
                // Default to the channelDefinition description to override the
                String description = channelDefinition.getDescription();
                    description = channelType.getDescription();
                ChannelDefinitionDTO channelDefinitionDTO = new ChannelDefinitionDTO(channelDefinition.getId(),
                        channelDefinition.getChannelTypeUID().toString(), label, description, channelType.getTags(),
                        channelType.getCategory(), channelType.getState(), channelType.isAdvanced(),
                        channelDefinition.getProperties());
                channelDefinitionDTOs.add(channelDefinitionDTO);
        return channelDefinitionDTOs;
