 * Identifier of a profile type.
public class ProfileTypeUID extends UID {
    public static final String SYSTEM_SCOPE = "system";
    public ProfileTypeUID(String profileType) {
        super(profileType);
    public ProfileTypeUID(String scope, String id) {
        super(scope, id);
