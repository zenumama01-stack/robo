 * Implementation of the Console interface for Eclipse OSGi console.
 * This class wraps the Eclipse OSGi CommandInterpreter to provide a unified console interface.
 * @author Markus Rathgeb - Split to separate file
public class OSGiConsole implements Console {
    private final String baseCommand;
    private final CommandInterpreter interpreter;
     * Constructs a new OSGi console wrapper.
     * @param baseCommand the base command name (e.g., "openhab")
     * @param interpreter the Eclipse OSGi command interpreter
    public OSGiConsole(final String baseCommand, final CommandInterpreter interpreter) {
        this.baseCommand = baseCommand;
        this.interpreter = interpreter;
     * Prints a string to the console without a newline.
     * @param s the string to print
    public void print(final String s) {
        interpreter.print(s);
     * Prints a string to the console with a newline.
    public void println(final String s) {
        interpreter.println(s);
     * Prints the usage information for a command.
     * The output format is: "Usage: {baseCommand} {usageString}".
     * @param s the usage string describing command syntax
    public void printUsage(final String s) {
        interpreter.println(String.format("Usage: %s %s", baseCommand, s));
package org.openhab.core.io.console.karaf;
import java.io.PrintStream;
import org.apache.karaf.shell.api.console.Session;
import org.jline.reader.LineReader;
    private final String scope;
    private final PrintStream out;
    private final Session session;
    public OSGiConsole(final String scope, PrintStream out, Session session) {
        this.out = out;
    public void printf(String format, Object... args) {
        out.printf(format, args);
        out.print(s);
        out.println(s);
        out.println(String.format("Usage: %s:%s", scope, s));
    public String readLine(String prompt, final @Nullable Character mask) throws IOException {
        // Prevent readLine() from logging a warning
        // see:
        // https://github.com/apache/karaf/blob/ad427cd12543dc78e095bbaa4608d7ca3d5ea4d8/shell/core/src/main/java/org/apache/karaf/shell/impl/console/ConsoleSessionImpl.java#L549
        // https://github.com/jline/jline3/blob/ee4886bf24f40288a4044f9b4b74917b58103e49/reader/src/main/java/org/jline/reader/LineReaderBuilder.java#L90
        String previousSetting = System.setProperty(LineReader.PROP_SUPPORT_PARSEDLINE, "true");
            return session.readLine(prompt, mask);
            if (previousSetting != null) {
                System.setProperty(LineReader.PROP_SUPPORT_PARSEDLINE, previousSetting);
                System.clearProperty(LineReader.PROP_SUPPORT_PARSEDLINE);
    public @Nullable String getUser() {
        Object result = session.get("USER");
        return result != null ? result.toString() : null;
    public Session getSession() {
 * Implementation of the Console interface for OSGi RFC 147 console.
 * This console implementation outputs to System.out and uses a base command scope
 * for formatting usage messages.
    private final String base;
     * Constructs a new OSGi RFC 147 console.
     * @param base the base command scope (e.g., "openhab")
    public OSGiConsole(final String base) {
        this.base = base;
     * Gets the base command scope.
     * @return the base command scope string
    public String getBase() {
        System.out.printf(format, args);
        System.out.print(s);
        System.out.println(s);
        System.out.println(String.format("Usage: %s %s", base, s));
