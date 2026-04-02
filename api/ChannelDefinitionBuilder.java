 * Builder for a channel definition.
public class ChannelDefinitionBuilder {
     * Creates a new channel definition builder.
     * @param channelDefinition the channel definition the builder should be initialized by
    public ChannelDefinitionBuilder(final ChannelDefinition channelDefinition) {
        this(channelDefinition.getId(), channelDefinition.getChannelTypeUID());
        this.properties = channelDefinition.getProperties();
        this.label = channelDefinition.getLabel();
        this.description = channelDefinition.getDescription();
        this.autoUpdatePolicy = channelDefinition.getAutoUpdatePolicy();
    public ChannelDefinitionBuilder(final String id, final ChannelTypeUID channelTypeUID) {
     * Sets the properties.
     * @param properties the properties
    public ChannelDefinitionBuilder withProperties(final @Nullable Map<String, String> properties) {
     * Sets the label.
    public ChannelDefinitionBuilder withLabel(final @Nullable String label) {
     * Sets the description.
    public ChannelDefinitionBuilder withDescription(final @Nullable String description) {
     * Sets the auto update policy.
     * @param autoUpdatePolicy the auto update policy
    public ChannelDefinitionBuilder withAutoUpdatePolicy(final @Nullable AutoUpdatePolicy autoUpdatePolicy) {
     * Build a channel definition.
     * @return channel definition
    public ChannelDefinition build() {
        return new ChannelDefinition(id, channelTypeUID, label, description, properties, autoUpdatePolicy);
