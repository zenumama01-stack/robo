 * Interface for builders for {@link ChannelType}s of kind TRIGGER
public interface TriggerChannelTypeBuilder extends ChannelTypeBuilder<TriggerChannelTypeBuilder> {
     * Sets the EventDescription for the ChannelType
     * @param eventDescription EventDescription for the ChannelType
    TriggerChannelTypeBuilder withEventDescription(EventDescription eventDescription);
