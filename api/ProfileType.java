 * Describes a profile type.
public interface ProfileType extends Identifiable<ProfileTypeUID> {
     * @return a collection of item types (may be empty if all are supported)
    Collection<String> getSupportedItemTypes();
     * Get a human readable description.
     * @return the label
