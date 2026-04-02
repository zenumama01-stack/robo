package org.openhab.core.io.console.extensions;
 * A base class that should be used by console command extension for better inclusion.
public abstract class AbstractConsoleCommandExtension implements ConsoleCommandExtension {
    private final String cmd;
    private final String desc;
     * Generate a new console command extension.
     * @param cmd The command the extension is used for.
     * @param desc The description what this extension is handling.
    protected AbstractConsoleCommandExtension(final String cmd, final String desc) {
        this.cmd = cmd;
        this.desc = desc;
     * You should always use that function to use a usage string that complies to a standard format.
     * @param description the description of the command
     * @param syntax the syntax format
     * Print the whole usages to the console.
     * @param console the console that should be used for output
    protected void printUsage(Console console) {
        for (final String usage : getUsages()) {
