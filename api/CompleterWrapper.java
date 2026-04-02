import org.apache.karaf.shell.api.console.Candidate;
import org.apache.karaf.shell.api.console.CommandLine;
import org.apache.karaf.shell.support.completers.StringsCompleter;
public class CompleterWrapper implements Completer {
    private final @Nullable ConsoleCommandCompleter completer;
    private final String command;
    private final String commandDescription;
    private final @Nullable String globalCommand;
    public CompleterWrapper(final ConsoleCommandExtension command, boolean scoped) {
        this.completer = command.getCompleter();
        this.command = command.getCommand();
        this.commandDescription = command.getDescription();
        if (!scoped) {
            globalCommand = CommandWrapper.SCOPE + ":" + this.command;
            globalCommand = null;
    public int complete(Session session, CommandLine commandLine, List<String> candidates) {
        String localGlobalCommand = globalCommand;
        if (commandLine.getCursorArgumentIndex() == 0) {
            StringsCompleter stringsCompleter = new StringsCompleter();
            stringsCompleter.getStrings().add(command);
            if (localGlobalCommand != null) {
                stringsCompleter.getStrings().add(localGlobalCommand);
            return stringsCompleter.complete(session, commandLine, candidates);
        if (commandLine.getArguments().length > 1) {
            String arg = commandLine.getArguments()[0];
            if (!arg.equals(command) && !arg.equals(localGlobalCommand)) {
        if (commandLine.getCursorArgumentIndex() < 0) {
        var localCompleter = completer;
        if (localCompleter == null) {
        String[] args = commandLine.getArguments();
        boolean result = localCompleter.complete(Arrays.copyOfRange(args, 1, args.length),
                commandLine.getCursorArgumentIndex() - 1, commandLine.getArgumentPosition(), candidates);
        return result ? commandLine.getBufferPosition() - commandLine.getArgumentPosition() : -1;
    // Override this method to give command descriptions if completing the command name
    public void completeCandidates(Session session, CommandLine commandLine, List<Candidate> candidates) {
            arg = arg.substring(0, commandLine.getArgumentPosition());
            if (command.startsWith(arg)) {
                candidates.add(new Candidate(command, command, null, commandDescription, null, null, true));
            if (localGlobalCommand != null && localGlobalCommand.startsWith(arg)) {
                candidates.add(new Candidate(localGlobalCommand, localGlobalCommand, null, commandDescription, null,
                        null, true));
        org.apache.karaf.shell.api.console.Completer.super.completeCandidates(session, commandLine, candidates);
