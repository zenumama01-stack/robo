package org.openhab.core.io.console;
 * This interface must be implemented by consoles which want to use the {@link ConsoleInterpreter}.
 * It allows basic output commands.
public interface Console {
     * Prints formatted output to the console.
     * This method formats the given string using the specified arguments and prints it to the console
     * without appending a newline.
     * @param format the format string (following {@link String#format} syntax)
     * @param args the arguments referenced by the format specifiers in the format string
    default void printf(String format, Object... args) {
        print(String.format(format, args));
     * Prints a string to the console without appending a newline.
    void print(String s);
     * Prints a string to the console followed by a newline.
    void println(String s);
     * usage output is treated differently from other output as it might
     * differ between different kinds of consoles
     * @param s the main usage string (console independent)
    void printUsage(String s);
     * Reads a line from the console. The prompt is displayed before the line is read.
     * @param prompt the prompt to display
     * @param mask the character to use for masking input (e.g. '*'), or null if no masking is required
     * @return the line read from the console
     * @throws IOException if an I/O error occurs
    default String readLine(String prompt, final @Nullable Character mask) throws IOException {
        throw new UnsupportedOperationException("readLine not supported");
     * Returns the user name associated with the console, or null if no user is associated.
     * @return the user name, or null if no user is associated
    default @Nullable String getUser() {
