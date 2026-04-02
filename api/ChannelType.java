 * The {@link ChannelType} describes a concrete type of {@link Channel}.
 * This description is used as template definition for the creation of the according concrete {@link Channel} object.
 * Use the {@link ChannelTypeBuilder} for building channel types.
 * @author Henning Treu - add command options
public class ChannelType extends AbstractDescriptionType {
    private final @Nullable String itemType;
    private final @Nullable StateDescription state;
    private final @Nullable CommandDescription commandDescription;
    private final @Nullable String unitHint;
    private final @Nullable EventDescription event;
     * @param uid the unique identifier which identifies this Channel type within
     * @param advanced true if this channel type contains advanced features, otherwise false
     * @param itemType the item type of this Channel type, e.g. {@code ColorItem}
     * @param kind the channel kind.
     * @param category the category of this Channel type, e.g. {@code TEMPERATURE} (could be null or empty)
     * @param tags all tags of this {@link ChannelType}, e.g. {@code Alarm} (could be null or empty)
     * @param state a {@link StateDescription} of an items state which gives information how to interpret it.
     * @param commandDescription a {@link CommandDescription} which should be rendered as push-buttons. The command
     *            values will be sent to the channel from this {@link ChannelType}.
     * @param autoUpdatePolicy the {@link AutoUpdatePolicy} to use.
     * @throws IllegalArgumentException if the UID or the item type is null or empty,
     *             or the meta information is null
    protected ChannelType(ChannelTypeUID uid, boolean advanced, @Nullable String itemType, @Nullable String unitHint,
            ChannelKind kind, String label, @Nullable String description, @Nullable String category,
            @Nullable Set<String> tags, @Nullable StateDescription state,
            @Nullable CommandDescription commandDescription, @Nullable EventDescription event,
        super(uid, label, description, configDescriptionURI);
        if (kind == ChannelKind.STATE && (itemType == null || itemType.isBlank())) {
        if (kind == ChannelKind.TRIGGER && itemType != null) {
            throw new IllegalArgumentException("If the kind is 'trigger', the item type must not be set!");
        if ((itemType == null || !itemType.startsWith("Number:")) && unitHint != null) {
                    "A unit hint must not be set if the item type is not a number with dimension!");
    public ChannelTypeUID getUID() {
        return (ChannelTypeUID) super.getUID();
     * Returns the item type of this {@link ChannelType}, e.g. {@code ColorItem}.
     * @return the item type of this Channel type, e.g. {@code ColorItem}. Can be null if the channel is a trigger
     *         channel.
     * Returns the kind of this {@link ChannelType}, e.g. {@code STATE}.
     * @return the kind of this Channel type, e.g. {@code STATE}.
     * Returns all tags of this {@link ChannelType}, e.g. {@code Alarm}.
     * @return all tags of this Channel type, e.g. {@code Alarm}
        return this.tags;
     * Returns the {@link StateDescription} of an items state which gives information how to interpret it.
     * @return the {@link StateDescription}
    public @Nullable StateDescription getState() {
     * Returns the {@link unitHint} of a channel which gives information on what unit to suggest when creating an item
     * linked to the channel.
     * @return the {@link unitHint}
    public @Nullable String getUnitHint() {
        return unitHint;
     * Returns information about the supported events.
     * @return the event information. Can be null if the channel is a state channel.
    public @Nullable EventDescription getEvent() {
     * Returns {@code true} if this channel type contains advanced functionalities
     * which should be typically not shown in the basic view of user interfaces,
     * @return true if this channel type contains advanced functionalities, otherwise false
     * Returns the category of this {@link ChannelType}, e.g. {@code TEMPERATURE}.
     * @return the category of this Channel type, e.g. {@code TEMPERATURE}
     * Returns the {@link AutoUpdatePolicy} of for channels of this type.
     * @return the {@link AutoUpdatePolicy}. Can be null if the channel is a trigger
     * Returns the {@link CommandDescription} which should be rendered as push-buttons.
     * @return the {@link CommandDescription}
    public @Nullable CommandDescription getCommandDescription() {
