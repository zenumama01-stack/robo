package org.openhab.core.io.console.rfc147.internal.extension;
import org.openhab.core.io.console.rfc147.internal.ConsoleCommandsContainer;
import org.openhab.core.io.console.rfc147.internal.ConsoleSupportRfc147;
 * Console command extension that provides help information for all available commands.
 * This command displays usage information for all registered console commands in the
 * OSGi RFC 147 console.
public class HelpConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private @Nullable ConsoleCommandsContainer commandsContainer;
     * Constructs a new help console command extension.
     * Registers the "help" command with a description.
    public HelpConsoleCommandExtension() {
        super("help", "Get help for all available commands.");
     * Sets the container that provides access to all registered console commands.
     * This is used to retrieve the list of commands to display help for.
     * @param commandsContainer the commands container, or null to clear the reference
    public void setConsoleCommandsContainer(final @Nullable ConsoleCommandsContainer commandsContainer) {
        this.commandsContainer = commandsContainer;
     * Command entry point that matches the command name "help".
     * This method is invoked by the OSGi RFC 147 command infrastructure when the help command is called.
     * @param args the command arguments
    public void help(String[] args) {
        execute(args, ConsoleSupportRfc147.CONSOLE);
        ConsoleCommandsContainer commandsContainer = this.commandsContainer;
        if (commandsContainer != null) {
            ConsoleInterpreter.printHelp(console, ConsoleSupportRfc147.CONSOLE.getBase(), ":",
                    commandsContainer.getConsoleCommandExtensions());
        return List.of(buildCommandUsage(getDescription()));
