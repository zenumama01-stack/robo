package org.openhab.core.io.console.eclipse.internal;
import java.util.SortedMap;
import org.eclipse.osgi.framework.console.CommandInterpreter;
import org.eclipse.osgi.framework.console.CommandProvider;
import org.openhab.core.io.console.ConsoleInterpreter;
 * This class provides access to openHAB functionality through the OSGi console
 * of Equinox. Unfortunately, there these command providers are not standardized
 * for OSGi, so we need different implementations for different OSGi containers.
 * @author Markus Rathgeb - Split console interface and specific implementation
public class ConsoleSupportEclipse implements CommandProvider {
    private static final String BASE = "openhab";
    private final SortedMap<String, ConsoleCommandExtension> consoleCommandExtensions = Collections
            .synchronizedSortedMap(new TreeMap<>());
     * Constructs a new ConsoleSupportEclipse instance.
    public ConsoleSupportEclipse() {
     * Adds a console command extension to the registry.
     * This method is called dynamically by OSGi when a new console command extension is registered.
     * @param consoleCommandExtension the console command extension to add
    public void addConsoleCommandExtension(ConsoleCommandExtension consoleCommandExtension) {
        consoleCommandExtensions.put(consoleCommandExtension.getCommand(), consoleCommandExtension);
     * Removes a console command extension from the registry.
     * This method is called dynamically by OSGi when a console command extension is unregistered.
     * @param consoleCommandExtension the console command extension to remove
    public void removeConsoleCommandExtension(ConsoleCommandExtension consoleCommandExtension) {
        consoleCommandExtensions.remove(consoleCommandExtension.getCommand());
     * Gets a console command extension by command name.
     * @param cmd the command name
     * @return the console command extension, or null if not found
    private ConsoleCommandExtension getConsoleCommandExtension(final String cmd) {
        return consoleCommandExtensions.get(cmd);
     * Gets all registered console command extensions.
     * @return a collection of all console command extensions
    private Collection<ConsoleCommandExtension> getConsoleCommandExtensions() {
        return new HashSet<>(consoleCommandExtensions.values());
     * Methods staring with "_" will be used as commands. We only define one command "openhab" to make
     * sure we do not get into conflict with other existing commands. The different functionalities
     * can then be used by the first argument.
     * @param interpreter the equinox command interpreter
     * @return null, return parameter is not used
    public Object _openhab(final CommandInterpreter interpreter) {
        final Console console = new OSGiConsole(BASE, interpreter);
        final String cmd = interpreter.nextArgument();
        if (cmd == null) {
            ConsoleInterpreter.printHelp(console, BASE, " ", getConsoleCommandExtensions());
            final ConsoleCommandExtension extension = getConsoleCommandExtension(cmd);
            if (extension == null) {
                console.println(String.format("No handler for command '%s' was found.", cmd));
                // Build argument list
                final List<String> argsList = new ArrayList<>();
                    final String narg = interpreter.nextArgument();
                    if (narg != null && !narg.isEmpty()) {
                        argsList.add(narg);
                final String[] args = new String[argsList.size()];
                argsList.toArray(args);
                ConsoleInterpreter.execute(console, extension, args);
     * Gets the help text for all registered openHAB console commands.
     * @return the help text listing all available commands and their usage
    public String getHelp() {
        return ConsoleInterpreter.getHelp(BASE, " ", getConsoleCommandExtensions());
