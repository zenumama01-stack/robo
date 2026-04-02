package org.openhab.core.io.rest.core.internal.profile;
import org.openhab.core.thing.profiles.StateProfileType;
import org.openhab.core.thing.profiles.dto.ProfileTypeDTO;
import org.openhab.core.thing.profiles.dto.ProfileTypeDTOMapper;
 * REST resource to obtain profile-types
@JaxrsName(ProfileTypeResource.PATH_PROFILE_TYPES)
@Path(ProfileTypeResource.PATH_PROFILE_TYPES)
@Tag(name = ProfileTypeResource.PATH_PROFILE_TYPES)
public class ProfileTypeResource implements RESTResource {
    public static final String PATH_PROFILE_TYPES = "profile-types";
    public ProfileTypeResource( //
    @Operation(operationId = "getProfileTypes", summary = "Gets all available profile types.", responses = {
            @ApiResponse(responseCode = "200", description = "OK", content = @Content(array = @ArraySchema(schema = @Schema(implementation = ProfileTypeDTO.class), uniqueItems = true))) })
            @QueryParam("channelTypeUID") @Parameter(description = "channel type filter") @Nullable String channelTypeUID,
            @QueryParam("itemType") @Parameter(description = "item type filter") @Nullable String itemType) {
        return Response.ok(new Stream2JSONInputStream(getProfileTypes(locale, channelTypeUID, itemType))).build();
    protected Stream<ProfileTypeDTO> getProfileTypes(@Nullable Locale locale, @Nullable String channelTypeUID,
            @Nullable String itemType) {
        return profileTypeRegistry.getProfileTypes(locale).stream().filter(matchesChannelUID(channelTypeUID, locale))
                .filter(matchesItemType(itemType)).sorted(Comparator.comparing(ProfileType::getLabel))
                .map(ProfileTypeDTOMapper::map);
    private Predicate<ProfileType> matchesChannelUID(@Nullable String channelTypeUID, @Nullable Locale locale) {
        if (channelTypeUID == null) {
            return t -> true;
            // requested to filter against an unknown channel type -> do not return a ProfileType
            return t -> false;
        return switch (channelType.getKind()) {
            case STATE -> t -> stateProfileMatchesProfileType(t, channelType);
            case TRIGGER -> t -> triggerProfileMatchesProfileType(t, channelType);
    private Predicate<ProfileType> matchesItemType(@Nullable String itemType) {
        if (itemType == null) {
        return t -> profileTypeMatchesItemType(t, itemType);
    private boolean profileTypeMatchesItemType(ProfileType pt, String itemType) {
        Collection<String> supportedItemTypesOnProfileType = pt.getSupportedItemTypes();
        return supportedItemTypesOnProfileType.isEmpty()
                || supportedItemTypesOnProfileType.contains(ItemUtil.getMainItemType(itemType))
                || supportedItemTypesOnProfileType.contains(itemType);
    private boolean triggerProfileMatchesProfileType(ProfileType profileType, ChannelType channelType) {
        if (profileType instanceof TriggerProfileType triggerProfileType) {
            if (triggerProfileType.getSupportedChannelTypeUIDs().isEmpty()) {
            if (triggerProfileType.getSupportedChannelTypeUIDs().contains(channelType.getUID())) {
    private boolean stateProfileMatchesProfileType(ProfileType profileType, ChannelType channelType) {
        if (profileType instanceof StateProfileType stateProfileType) {
            if (stateProfileType.getSupportedItemTypesOfChannel().isEmpty()) {
            Collection<String> supportedItemTypesOfChannelOnProfileType = stateProfileType
                    .getSupportedItemTypesOfChannel();
            String itemType = channelType.getItemType();
            return itemType != null
                    && supportedItemTypesOfChannelOnProfileType.contains(ItemUtil.getMainItemType(itemType));
