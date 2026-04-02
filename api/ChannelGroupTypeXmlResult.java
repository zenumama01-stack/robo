 * The {@link ChannelGroupTypeXmlResult} is an intermediate XML conversion result object which
 * contains all parts of a {@link ChannelGroupType} object.
 * To create a concrete {@link ChannelGroupType} object, the method {@link #toChannelGroupType()} must be called.
 * @author Chris Jackson - Updated to support channel properties
public class ChannelGroupTypeXmlResult {
    private final @Nullable List<ChannelXmlResult> channelTypeReferences;
    public ChannelGroupTypeXmlResult(ChannelGroupTypeUID channelGroupTypeUID, String label,
            @Nullable String description, @Nullable String category,
            @Nullable List<ChannelXmlResult> channelTypeReferences) {
        this.channelTypeReferences = channelTypeReferences;
        return channelGroupTypeUID;
    protected @Nullable List<ChannelDefinition> toChannelDefinitions(
            @Nullable List<ChannelXmlResult> channelTypeReferences) throws ConversionException {
        List<ChannelDefinition> channelTypeDefinitions = null;
        if (channelTypeReferences != null && !channelTypeReferences.isEmpty()) {
            channelTypeDefinitions = new ArrayList<>(channelTypeReferences.size());
            for (ChannelXmlResult channelTypeReference : channelTypeReferences) {
                channelTypeDefinitions
                        .add(channelTypeReference.toChannelDefinition(channelGroupTypeUID.getBindingId()));
        return channelTypeDefinitions;
    public ChannelGroupType toChannelGroupType() throws ConversionException {
        ChannelGroupTypeBuilder builder = ChannelGroupTypeBuilder.instance(channelGroupTypeUID, label);
        String description = this.description;
        String category = this.category;
        List<ChannelDefinition> channelDefinitions = toChannelDefinitions(channelTypeReferences);
        if (channelDefinitions != null) {
            builder.withChannelDefinitions(channelDefinitions);
        return "ChannelGroupTypeXmlResult [channelGroupTypeUID=" + channelGroupTypeUID + ", label=" + label
                + ", description=" + description + ", category=" + category + ", channelTypeReferences="
                + channelTypeReferences + "]";
