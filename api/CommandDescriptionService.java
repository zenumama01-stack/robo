 * An implementation of this service provides locale specific {@link CommandDescription}s for the given item.
public interface CommandDescriptionService {
     * Returns the locale specific {@link CommandDescription} for the given item name.
     * @param locale the locale for translated command labels
     * @return the locale specific {@link CommandDescription} for the given item name
    CommandDescription getCommandDescription(String itemName, @Nullable Locale locale);
