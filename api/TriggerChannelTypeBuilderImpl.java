 * Implementation of {@link TriggerChannelTypeBuilder} to build {@link ChannelType}s of kind TRIGGER
public class TriggerChannelTypeBuilderImpl extends AbstractChannelTypeBuilder<TriggerChannelTypeBuilder>
        implements TriggerChannelTypeBuilder {
    private static class TriggerChannelTypeImpl extends ChannelType {
        TriggerChannelTypeImpl(ChannelTypeUID uid, boolean advanced, String label, @Nullable String description,
                @Nullable String category, @Nullable Set<String> tags, @Nullable EventDescription event,
                @Nullable URI configDescriptionURI) throws IllegalArgumentException {
            super(uid, advanced, null, null, ChannelKind.TRIGGER, label, description, category, tags, null, null, event,
                    configDescriptionURI, null);
    private @Nullable EventDescription eventDescription;
    public TriggerChannelTypeBuilderImpl(ChannelTypeUID channelTypeUID, String label) {
    public TriggerChannelTypeBuilder withEventDescription(EventDescription eventDescription) {
        this.eventDescription = eventDescription;
        return new TriggerChannelTypeImpl(channelTypeUID, advanced, label, description, category, tags,
                eventDescription, configDescriptionURI);
