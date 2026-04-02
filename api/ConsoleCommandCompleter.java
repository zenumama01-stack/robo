 * Implementing this interface allows a {@link org.openhab.core.io.console.extensions.ConsoleCommandExtension}
 * to provide completions for the user as they write commands.
public interface ConsoleCommandCompleter {
     * Populate possible completion candidates.
     * @param args An array of all arguments to be passed to the ConsoleCommandExtension's execute method
     * @param cursorArgumentIndex the argument index the cursor is currently in
     * @param cursorPosition the position of the cursor within the argument
     * @param candidates a list to fill with possible completion candidates
     * @return if a candidate was found
    boolean complete(String[] args, int cursorArgumentIndex, int cursorPosition, List<String> candidates);
