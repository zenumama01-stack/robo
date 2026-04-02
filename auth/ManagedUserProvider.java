 * A {@link org.openhab.core.common.registry.ManagedProvider} for {@link org.openhab.core.auth.ManagedUser} entities
@Component(service = ManagedUserProvider.class, immediate = true)
public class ManagedUserProvider extends DefaultAbstractManagedProvider<User, String> {
    public ManagedUserProvider(final @Reference StorageService storageService) {
        return "users";
