 * The {@link ScriptFileWatcher} interface needs to be implemented by script file watchers. Services that implement this
 * interface can be tracked to set a {@link ReadyMarker} once all services have completed their initial loading.
public interface ScriptFileWatcher {
     * Returns a {@link CompletableFuture<Void>} that completes when the {@link ScriptFileWatcher} has completed it's
     * initial loading of files.
     * @return the {@link CompletableFuture}
    CompletableFuture<@Nullable Void> ifInitialized();
