 * This class is a wrapper of {@link ModuleTypeProvider}, responsible for initializing the WatchService.
 * @author Arne Seime - Added module validation support
@Component(immediate = true, service = ModuleTypeProvider.class)
public class ModuleTypeFileProviderWatcher extends ModuleTypeFileProvider {
    public ModuleTypeFileProviderWatcher(
            @Reference(target = WatchService.CONFIG_WATCHER_FILTER) WatchService watchService) {
    protected void initializeWatchService(String watchingDir) {
        WatchServiceUtil.initializeWatchService(watchingDir, this, watchService);
    protected void deactivateWatchService(String watchingDir) {
        WatchServiceUtil.deactivateWatchService(watchingDir, this);
    public void addParser(Parser<ModuleType> parser, Map<String, String> properties) {
    public void removeParser(Parser<ModuleType> parser, Map<String, String> properties) {
    protected void validateObject(ModuleType moduleType) throws ValidationException {
        if ((s = moduleType.getUID()) == null || s.isBlank()) {
            throw new ValidationException(ObjectType.MODULE_TYPE, null, "UID cannot be blank");
        if ((s = moduleType.getLabel()) == null || s.isBlank()) {
            throw new ValidationException(ObjectType.MODULE_TYPE, moduleType.getUID(), "Label cannot be blank");
