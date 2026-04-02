 * Describes a {@link StateProfile} type.
 * @author Stefan Triller - added getSupportedItemTypesOfChannel method
public interface StateProfileType extends ProfileType {
     * Get a collection of ItemType names that a Channel needs to support in order to able to use this ProfileType
     * @return a collection of supported ItemType names (an empty list means ALL types are supported)
    Collection<String> getSupportedItemTypesOfChannel();
