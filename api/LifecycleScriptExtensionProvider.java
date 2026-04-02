 * ScriptExtensionProvider which providers a 'lifecycleTracker' object allowing scripts to register for disposal events.
public class LifecycleScriptExtensionProvider implements ScriptExtensionProvider {
    private static final String LIFECYCLE_PRESET_NAME = "lifecycle";
    private static final String LIFECYCLE_TRACKER_NAME = "lifecycleTracker";
    private final Map<String, LifecycleTracker> idToTracker = new ConcurrentHashMap<>();
        return Set.of(LIFECYCLE_PRESET_NAME);
        return Set.of(LIFECYCLE_TRACKER_NAME);
        if (LIFECYCLE_TRACKER_NAME.equals(type)) {
            return idToTracker.computeIfAbsent(scriptIdentifier, k -> new LifecycleTracker());
        if (LIFECYCLE_PRESET_NAME.equals(preset)) {
            final Object requestedType = get(scriptIdentifier, LIFECYCLE_TRACKER_NAME);
            if (requestedType != null) {
                return Map.of(LIFECYCLE_TRACKER_NAME, requestedType);
        LifecycleTracker tracker = idToTracker.remove(scriptIdentifier);
        if (tracker != null) {
            tracker.dispose();
    public static class LifecycleTracker {
        List<Disposable> disposables = new ArrayList<>();
        public void addDisposeHook(Disposable disposable) {
            disposables.add(disposable);
        void dispose() {
            for (Disposable disposable : disposables) {
                disposable.dispose();
    @FunctionalInterface
    public interface Disposable {
        void dispose();
