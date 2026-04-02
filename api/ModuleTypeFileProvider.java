 * This class is implementation of {@link ModuleTypeProvider}. It extends functionality of {@link AbstractFileProvider}
 * for importing the {@link ModuleType}s from local files.
public abstract class ModuleTypeFileProvider extends AbstractFileProvider<ModuleType> implements ModuleTypeProvider {
    public ModuleTypeFileProvider() {
        super("moduletypes");
    protected String getUID(ModuleType providedObject) {
        return providedObject.getUID();
    public <T extends ModuleType> @Nullable T getModuleType(String uid, @Nullable Locale locale) {
        return (T) providedObjectsHolder.get(uid);
        Collection<ModuleType> values = providedObjectsHolder.values();
        if (values.isEmpty()) {
        return (Collection<T>) new LinkedList<>(values);
