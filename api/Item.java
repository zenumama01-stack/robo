 * This interface defines the core features of an openHAB item.
 * Item instances are used for all stateful services and are especially important for the {@link ItemRegistry}.
public interface Item extends Identifiable<String> {
     * returns the current state of the item as a specific type
     * @return the current state in the requested type or
     *         null, if state cannot be provided as the requested type
    <T extends State> @Nullable T getStateAs(Class<T> typeClass);
     * Returns the previous state of the item.
     * @return the previous state of the item, or null if the item has never been changed.
     * Returns the time the item was last updated.
     * @return the time the item was last updated, or null if the item has never been updated.
    ZonedDateTime getLastStateUpdate();
     * Returns the time the item was last changed.
     * @return the time the item was last changed, or null if the item has never been changed.
     * returns the item type as defined by {@link ItemFactory}s
     * @return the item type
     * This method provides a list of all data types that can be used to update the item state
     * Imagine e.g. a dimmer device: It's status could be 0%, 10%, 50%, 100%, but also OFF or ON and maybe
     * UNDEFINED. So the accepted data types would be in this case {@link org.openhab.core.library.types.PercentType},
     * {@linkorg.openhab.core.library.types.OnOffType} and {@link org.openhab.core.types.UnDefType}
     * The order of data types denotes the order of preference. So in case a state needs to be converted
     * in order to be accepted, it will be attempted to convert it to a type from top to bottom. Therefore
     * the type with the least information loss should be on top of the list - in the example above the
     * {@link org.openhab.core.library.types.PercentType} carries more information than the
     * {@linkorg.openhab.core.library.types.OnOffType}, hence it is listed first.
     * @return a list of data types that can be used to update the item state
    List<Class<? extends State>> getAcceptedDataTypes();
     * This method provides a list of all command types that can be used for this item
     * Imagine e.g. a dimmer device: You could ask it to dim to 0%, 10%, 50%, 100%, but also to turn OFF or ON.
     * So the accepted command types would be in this case {@link org.openhab.core.library.types.PercentType},
     * {@linkorg.openhab.core.library.types.OnOffType}
     * @return a list of all command types that can be used for this item
    List<Class<? extends Command>> getAcceptedCommandTypes();
     * Returns a list of the names of the groups this item belongs to.
     * @return list of item group names
    List<String> getGroupNames();
     * Returns a set of tags. If the item is not tagged, an empty set is returned.
     * @return set of tags.
     * Returns the label of the item or null if no label is set.
     * @return item label or null
     * Returns true if the item's tags contains the specific tag, otherwise false.
     * @param tag a tag whose presence in the item's tags is to be tested.
     * @return true if the item's tags contains the specific tag, otherwise false.
    boolean hasTag(String tag);
     * Returns the category of the item or null if no category is set.
     * @return category or null
    String getCategory();
     * Returns the first provided state description (uses the default locale).
     * If options are defined on the channel, they are included in the returned state description.
     * @return state description (can be null)
    StateDescription getStateDescription();
     * Returns the first provided state description for a given locale.
    StateDescription getStateDescription(@Nullable Locale locale);
     * Returns the {@link CommandDescription} for this item. In case no dedicated {@link CommandDescription} is
     * provided the {@link org.openhab.core.types.StateOption}s from the {@link StateDescription} will be served
     * as valid {@link org.openhab.core.types.CommandOption}s.
     * @return the {@link CommandDescription} for the default locale (can be null).
    default @Nullable CommandDescription getCommandDescription() {
        return getCommandDescription(null);
     * Returns the {@link CommandDescription} for the given locale. In case no dedicated {@link CommandDescription} is
     * provided the {@link org.openhab.core.types.StateOption}s from the {@link StateDescription} will be served as
     * valid
     * {@link org.openhab.core.types.CommandOption}s.
     * @return command description (can be null)
    CommandDescription getCommandDescription(@Nullable Locale locale);
