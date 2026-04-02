 * This is the interface that a human language text interpreter has to implement.
public interface HumanLanguageInterpreter {
     * Interprets a human language text fragment of a given {@link Locale}
     * @param locale language of the text (given by a {@link Locale})
     * @param text the text to interpret
    String interpret(Locale locale, String text) throws InterpretationException;
     * Interprets a human language text fragment of a given {@link Locale} with optional access to the context of a
     * dialog execution.
    default String interpret(Locale locale, String text, @Nullable DialogContext dialogContext)
        return interpret(locale, text);
     * Gets the grammar of all commands of a given {@link Locale} of the interpreter
     * @param locale language of the commands (given by a {@link Locale})
     * @param format the grammar format
     * @return a grammar of the specified format
    String getGrammar(Locale locale, String format);
     * Gets all supported languages of the interpreter by their {@link Locale}s
     * @return Set of supported languages (each given by a {@link Locale}) or null, if there is no constraint
     * Gets all supported grammar format specifiers
     * @return Set of supported grammars (each given by a short name)
    Set<String> getSupportedGrammarFormats();
