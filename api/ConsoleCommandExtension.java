 * Client which provide a console command have to implement this interface
public interface ConsoleCommandExtension {
     * Get the command of for the extension.
     * @return command for the extension
    String getCommand();
     * Get the description for the extension.
     * @return description for the extension
     * This method called if a {@link #getCommand() command} for that extension is called.
     * Clients are not allowed to throw exceptions. They have to write corresponding messages to the given
     * {@link Console}
     * @param args array which contains all the console command arguments
     * @param console the console used to print
    void execute(String[] args, Console console);
     * @return the help texts for this extension
    List<String> getUsages();
     * This method allows a {@link ConsoleCommandExtension} to provide an object to enable
     * tab-completion functionality for the user.
     * @return a {@link ConsoleCommandCompleter} object for this command
    default @Nullable ConsoleCommandCompleter getCompleter() {
