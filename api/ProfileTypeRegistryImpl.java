 * {@link ProfileTypeRegistry} implementation.
@Component(service = ProfileTypeRegistry.class)
public class ProfileTypeRegistryImpl implements ProfileTypeRegistry {
    private final List<ProfileTypeProvider> profileTypeProviders = new CopyOnWriteArrayList<>();
    public List<ProfileType> getProfileTypes() {
        return getProfileTypes(null);
    public List<ProfileType> getProfileTypes(@Nullable Locale locale) {
        List<ProfileType> profileTypes = new ArrayList<>();
        for (ProfileTypeProvider profileTypeProvider : profileTypeProviders) {
            profileTypes.addAll(profileTypeProvider.getProfileTypes(locale));
        return Collections.unmodifiableList(profileTypes);
    protected void addProfileTypeProvider(ProfileTypeProvider profileTypeProvider) {
        profileTypeProviders.add(profileTypeProvider);
    protected void removeProfileTypeProvider(ProfileTypeProvider profileTypeProvider) {
        profileTypeProviders.remove(profileTypeProvider);
