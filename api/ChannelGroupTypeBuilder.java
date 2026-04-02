 * A {@link ChannelGroupType} builder.
public class ChannelGroupTypeBuilder {
    private @Nullable List<ChannelDefinition> channelDefinitions;
    private final ChannelGroupTypeUID channelGroupTypeUID;
     * Create an instance of a ChannelGroupTypeBuilder for {@link ChannelGroupType}s
     * @param channelGroupTypeUID UID of the {@link ChannelGroupType}
     * @param label Label for the {@link ChannelGroupType}
     * @return ChannelGroupTypeBuilder for {@link ChannelGroupType}s
    public static ChannelGroupTypeBuilder instance(ChannelGroupTypeUID channelGroupTypeUID, String label) {
        if (label.isBlank()) {
            throw new IllegalArgumentException("Label for a ChannelGroupType must not be empty.");
        return new ChannelGroupTypeBuilder(channelGroupTypeUID, label);
    private ChannelGroupTypeBuilder(ChannelGroupTypeUID channelGroupTypeUID, String label) {
        this.channelGroupTypeUID = channelGroupTypeUID;
     * Build the {@link ChannelGroupType} with the given values
     * @return the created {@link ChannelGroupType}
    public ChannelGroupType build() {
        return new ChannelGroupType(channelGroupTypeUID, label, description, category, channelDefinitions);
     * Sets the description for the {@link ChannelGroupType}
     * @param description Description for the {@link ChannelGroupType}
     * @return this Builder
    public ChannelGroupTypeBuilder withDescription(String description) {
     * Sets the category for the {@link ChannelGroupType}
     * @param category Category for the {@link ChannelGroupType}
    public ChannelGroupTypeBuilder withCategory(String category) {
     * Sets the channels for the {@link ChannelGroupType}
     * @param channelDefinitions The channels this {@link ChannelGroupType} provides (could be null or empty)
    public ChannelGroupTypeBuilder withChannelDefinitions(List<ChannelDefinition> channelDefinitions) {
        this.channelDefinitions = channelDefinitions;
