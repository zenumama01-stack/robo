 * The profile's context
 * It gives access to related information like the profile's configuration or a scheduler.
public interface ProfileContext {
     * Get the profile's configuration object
     * @return the configuration
     * Returns the configuration of the profile and transforms it to the given class.
     * @return configuration of profile in form of the given class
    <T> T getConfigurationAs(Class<T> configurationClass);
     * Get a scheduler to be used within profiles (if needed at all)
     * @return the scheduler
    ScheduledExecutorService getExecutorService();
     * Get the list of accepted data types for state updates to the linked item
     * This is an optional method and will return an empty list if not implemented.
     * @return A list of all accepted data types
    default List<Class<? extends State>> getAcceptedDataTypes() {
     * Get the list of accepted command types for commands send to the linked item
     * @return A list of all accepted command types
    default List<Class<? extends Command>> getAcceptedCommandTypes() {
     * Get the list of accepted command types for commands sent to the handler
    default List<Class<? extends Command>> getHandlerAcceptedCommandTypes() {
