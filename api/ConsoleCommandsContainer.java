 * Container interface for accessing registered console command extensions.
 * This interface provides access to all console commands that are currently registered
 * in the OSGi RFC 147 console support.
public interface ConsoleCommandsContainer {
     * @return a collection of all console command extensions currently registered
    Collection<ConsoleCommandExtension> getConsoleCommandExtensions();
