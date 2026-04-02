 * The {@link ChannelGroupType} contains a list of {@link ChannelDefinition}s and further meta information such as label
 * and description, which are generally used by user interfaces.
 * This type can be used for Things which offers multiple functionalities which belong all together.
public class ChannelGroupType extends AbstractDescriptionType {
    private final List<ChannelDefinition> channelDefinitions;
    private final @Nullable String category;
     * @param uid the unique identifier which identifies this channel group type within the
     * @param category the category of this channel group type, e.g. Temperature
     * @param channelDefinitions the channel definitions this channel group forms
    ChannelGroupType(ChannelGroupTypeUID uid, String label, @Nullable String description, @Nullable String category,
            @Nullable List<ChannelDefinition> channelDefinitions) throws IllegalArgumentException {
        super(uid, label, description, null);
        this.channelDefinitions = channelDefinitions == null ? List.of()
                : Collections.unmodifiableList(channelDefinitions);
     * Returns the channel definitions this {@link ChannelGroupType} provides.
     * @return the channels this Thing type provides
    public List<ChannelDefinition> getChannelDefinitions() {
        return channelDefinitions;
    public @Nullable String getCategory() {
        return category;
    public ChannelGroupTypeUID getUID() {
        return (ChannelGroupTypeUID) super.getUID();
        return super.getUID().toString();
