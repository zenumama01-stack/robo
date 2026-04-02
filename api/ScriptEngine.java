 * The script engine is the main entrypoint for openHAB script use. It can build {@link Script} instances from
 * simple strings. These scripts can then be executed by the caller.
public interface ScriptEngine {
     * Parses a string and returns a parsed script object.
     * @param scriptAsString script to parse
     * @return Script object, which can be executed
     * @throws ScriptParsingException
    Script newScriptFromString(final String scriptAsString) throws ScriptParsingException;
     * Wraps an Xbase XExpression in a Script instance
     * @param expression the XExpression
     * @return the Script instance containing the expression
    Script newScriptFromXExpression(final XExpression expression);
