 * The {@link ScriptDependencyTracker} is an interface that script dependency trackers can implement to allow automatic
 * re-loading if scripts on dependency changes
public interface ScriptDependencyTracker {
     * Get the tracker for a given script identifier
     * @param scriptId the unique id of the script
     * @return a {@link Consumer<String>} that accepts a the path to a dependency
    Consumer<String> getTracker(String scriptId);
     * Remove all tracking data for a given scipt identifier
     * @param scriptId the uniwue id of the script
    void removeTracking(String scriptId);
     * The {@link ScriptDependencyTracker.Listener} is an interface that needs to be implemented by listeners that want
     * to be notified about a dependency change
    interface Listener {
         * Called by the dependency tracker when a registered dependency changes
         * @param scriptId the identifier of the script whose dependency changed
        void onDependencyChange(String scriptId);
