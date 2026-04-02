package org.openhab.core.io.console.karaf.internal;
import org.apache.felix.service.command.Process;
import org.apache.karaf.shell.api.action.Action;
import org.apache.karaf.shell.api.action.lifecycle.Reference;
import org.apache.karaf.shell.api.action.lifecycle.Service;
import org.apache.karaf.shell.api.console.Command;
import org.apache.karaf.shell.api.console.Completer;
import org.apache.karaf.shell.api.console.Parser;
import org.apache.karaf.shell.api.console.Registry;
import org.openhab.core.io.console.karaf.OSGiConsole;
 * This class wraps OH ConsoleCommandExtensions to commands for Apache Karaf
 * @author Henning Treu - implement help command
@Service
@org.apache.karaf.shell.api.action.Command(name = "help", scope = "openhab", description = "Print the full usage information of the 'openhab' commands.")
public class CommandWrapper implements Command, Action {
    // Define a scope for all commands.
    public static final String SCOPE = "openhab";
    private final ConsoleCommandExtension command;
     * The registry is injected when a CommandWrapper is instantiated by Karaf (see {@link CommandWrapper} default
     * constructor).
    private Registry registry;
     * The constructor for the "help" instance of this class. This instance will be created by
     * {@code org.apache.karaf.shell.impl.action.command.ManagerImpl.instantiate(Class<? extends T>, Registry)} and
     * is used to print all usages from the {@code openhab} scope.
     * The wrapped command is unused here because the karaf infrastructure will call the {@link #execute()} method.
    public CommandWrapper() {
        this(null);
    public CommandWrapper(final ConsoleCommandExtension command) {
    public Object execute(Session session, List<Object> argList) throws Exception {
        String[] args = argList.stream().map(Object::toString).toArray(String[]::new);
        PrintStream out = Process.Utils.current().out();
        final Console console = new OSGiConsole(getScope(), out, session);
        if (args.length == 1 && "--help".equals(args[0])) {
            for (final String usage : command.getUsages()) {
                console.printUsage(usage);
            ConsoleInterpreter.execute(console, command, args);
    public Completer getCompleter(boolean scoped) {
        return new CompleterWrapper(command, scoped);
        return command.getDescription();
        return command.getCommand();
    public Parser getParser() {
    public String getScope() {
        return SCOPE;
     * {@link Action}.{@link #execute()} will be called on the CommandWrapper instance created by Karaf (see
     * {@link CommandWrapper} default constructor).
    public Object execute() throws Exception {
        List<Command> commands = registry.getCommands();
        for (Command command : commands) {
            if (SCOPE.equals(command.getScope()) && command instanceof CommandWrapper) {
                command.execute(null, List.of("--help"));
package org.openhab.core.io.console.rfc147.internal;
 * Wrapper class that bridges console command extensions to OSGi RFC 147 command interface.
 * This class wraps a ConsoleCommandExtension and exposes it as an OSGi command that can be
 * invoked through the OSGi command shell.
public class CommandWrapper {
     * Constructs a new command wrapper.
     * @param command the console command extension to wrap
     * Main entry point for command execution via OSGi RFC 147 interface.
     * This method is called when no specific function is found for the command.
     * The first argument is the command name, and remaining arguments are passed to the command.
     * @param args the command arguments, where args[0] is the command name
     * @throws Exception if command execution fails
    public void _main(/* CommandSession session, */String[] args) throws Exception {
            System.out.println("missing command");
            final String cmd = args[0];
            final String[] cmdArgs = Arrays.copyOfRange(args, 1, args.length);
            if (cmd.equals(command.getCommand())) {
                ConsoleInterpreter.execute(ConsoleSupportRfc147.CONSOLE, command, cmdArgs);
                        String.format("The command (%s) differs from expected one (%s).", cmd, command.getCommand()));
