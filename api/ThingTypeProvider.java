 * The {@link ThingTypeProvider} is responsible for providing thing types.
public interface ThingTypeProvider {
     * Provides a collection of thing types
     * @return the thing types provided by the {@link ThingTypeProvider}
    Collection<ThingType> getThingTypes(@Nullable Locale locale);
     * Provides a thing type for the given UID or null if no type for the
     * given UID exists.
     * @return thing type for the given UID or null if no type for the given
     *         UID exists
    ThingType getThingType(ThingTypeUID thingTypeUID, @Nullable Locale locale);
