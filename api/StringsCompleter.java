import java.util.SortedSet;
 * Completer for a set of strings.
 * It will provide candidate completions for whichever argument the cursor is located in.
public class StringsCompleter implements ConsoleCommandCompleter {
    private final SortedSet<String> strings;
    private final boolean caseSensitive;
     * Creates a case-insensitive StringsCompleter with an empty set of strings.
     * Strings can be added later by modifying the set returned from {@link #getStrings()}.
    public StringsCompleter() {
        this(List.of(), false);
     * @param strings The set of valid strings to be completed
     * @param caseSensitive if strings must match case sensitively when the user is typing them
    public StringsCompleter(final Collection<String> strings, boolean caseSensitive) {
        this.strings = new TreeSet<>(caseSensitive ? String::compareTo : String::compareToIgnoreCase);
        this.caseSensitive = caseSensitive;
        this.strings.addAll(strings);
     * Gets the strings that are allowed for this completer, so that you can modify the set.
    public SortedSet<String> getStrings() {
        return strings;
        String argument;
        if (cursorArgumentIndex >= 0 && cursorArgumentIndex < args.length) {
            argument = args[cursorArgumentIndex].substring(0, cursorPosition);
            argument = "";
        if (!caseSensitive) {
            argument = argument.toLowerCase();
        SortedSet<String> matches = getStrings().tailSet(argument);
        for (String match : matches) {
            String s = caseSensitive ? match : match.toLowerCase();
            if (!s.startsWith(argument)) {
            candidates.add(match + " ");
        return !candidates.isEmpty();
