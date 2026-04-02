 * Default implementation of a {@link StateProfileType}.
public class StateProfileTypeImpl implements StateProfileType {
    private final Collection<String> supportedItemTypes;
    private final Collection<String> supportedItemTypesOfChannel;
    public StateProfileTypeImpl(ProfileTypeUID profileTypeUID, String label, Collection<String> supportedItemTypes,
            Collection<String> supportedItemTypesOfChannel) {
        this.supportedItemTypes = Collections.unmodifiableCollection(supportedItemTypes);
        this.supportedItemTypesOfChannel = Collections.unmodifiableCollection(supportedItemTypesOfChannel);
    public ProfileTypeUID getUID() {
    public Collection<String> getSupportedItemTypes() {
        return supportedItemTypes;
    public Collection<String> getSupportedItemTypesOfChannel() {
        return supportedItemTypesOfChannel;
