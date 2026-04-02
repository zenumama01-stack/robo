import org.openhab.core.automation.module.script.ScriptTransformationService;
import org.openhab.core.thing.profiles.Profile;
import org.openhab.core.thing.profiles.ProfileFactory;
import org.openhab.core.thing.profiles.ProfileType;
import org.openhab.core.thing.profiles.ProfileTypeBuilder;
import org.openhab.core.thing.profiles.ProfileTypeProvider;
 * The {@link ScriptProfileFactory} creates {@link ScriptProfile} instances
@Component(service = { ProfileFactory.class, ProfileTypeProvider.class })
public class ScriptProfileFactory implements ProfileFactory, ProfileTypeProvider {
    public static final String PROFILE_CONFIG_URI_PREFIX = "profile:transform:";
    private final Map<String, ServiceRecord> services = new ConcurrentHashMap<>();
    public @Nullable Profile createProfile(ProfileTypeUID profileTypeUID, ProfileCallback callback,
            ProfileContext profileContext) {
        String serviceId = profileTypeUID.getId();
        ScriptTransformationService transformationService = services.get(serviceId).service();
        return new ScriptProfile(profileTypeUID, callback, profileContext, transformationService);
    public Collection<ProfileTypeUID> getSupportedProfileTypeUIDs() {
        return services.keySet().stream()
                .map(id -> new ProfileTypeUID(TransformationService.TRANSFORM_PROFILE_SCOPE, id)).toList();
    public Collection<ProfileType> getProfileTypes(@Nullable Locale locale) {
        return getSupportedProfileTypeUIDs().stream().map(uid -> {
            String id = uid.getId();
            String label = services.get(id).serviceLabel();
            return ProfileTypeBuilder.newState(uid, label).build();
        }).collect(Collectors.toList());
    public void bindScriptTransformationService(ScriptTransformationService service, Map<String, Object> properties) {
        String serviceId = (String) properties.get(TransformationService.SERVICE_PROPERTY_NAME);
        String serviceLabel = (String) properties.get(TransformationService.SERVICE_PROPERTY_LABEL);
        if (serviceId != null && serviceLabel != null) {
            services.put(serviceId, new ServiceRecord(service, serviceLabel));
    public void unbindScriptTransformationService(ScriptTransformationService service, Map<String, Object> properties) {
        services.remove(serviceId);
    private record ServiceRecord(ScriptTransformationService service, String serviceLabel) {
