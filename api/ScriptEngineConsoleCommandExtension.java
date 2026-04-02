package org.openhab.core.model.script.extension;
 * This class provides the script engine as a console command
public class ScriptEngineConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private @Nullable ScriptEngine scriptEngine;
    public ScriptEngineConsoleCommandExtension() {
        super("script", "Execute scripts");
        ScriptEngine scriptEngine = this.scriptEngine;
            String scriptString = String.join(" ", args);
                Script script = scriptEngine.newScriptFromString(scriptString);
                Object result = script.execute();
                    console.println(result.toString());
                    console.println("OK");
            } catch (ScriptParsingException e) {
                        Objects.requireNonNullElse(e.getMessage(), "An error occurred while parsing the script"));
                        Objects.requireNonNullElse(e.getMessage(), "An error occurred while executing the script"));
            console.println("Script engine is not available.");
        return List.of(buildCommandUsage("<script to execute>", "Executes a script"));
        this.scriptEngine = null;
