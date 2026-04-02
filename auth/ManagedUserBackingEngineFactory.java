import org.apache.karaf.jaas.modules.BackingEngineFactory;
 * A Karaf backing engine factory for the {@link UserRegistry}
@Component(service = BackingEngineFactory.class)
public class ManagedUserBackingEngineFactory implements BackingEngineFactory {
    public ManagedUserBackingEngineFactory(@Reference UserRegistry userRegistry) {
    public String getModuleClass() {
        return ManagedUserRealm.MODULE_CLASS;
    public BackingEngine build(Map<String, ?> options) {
        return new ManagedUserBackingEngine(userRegistry);
