 * <li>{@link AutomationCommands#IMPORT_MODULE_TYPES}
 * <li>{@link AutomationCommands#IMPORT_TEMPLATES}
 * <li>{@link AutomationCommands#IMPORT_RULES}
public class AutomationCommandImport extends AutomationCommand {
     * This field keeps URL of the source of automation objects that has to be imported.
    private @Nullable URL url;
    public AutomationCommandImport(String command, String[] params, int adminType,
        super(command, params, adminType, autoCommands);
        URL url = this.url;
        if (!SUCCESS.equals(parsingResult) || url == null) {
                    autoCommands.importModuleTypes(parserType, url);
                    autoCommands.importTemplates(parserType, url);
                    autoCommands.importRules(parserType, url);
        return SUCCESS + "\n";
     * This method serves to create an {@link URL} object or {@link File} object from a string that is passed as
     * a parameter of the command. From the {@link File} object the URL is constructed.
     * @param parameterValue is a string that is passed as parameter of the command and it supposed to be a URL
     * @return an {@link URL} object created from the string that is passed as parameter of the command or <b>null</b>
     *         if either no legal protocol could be found in the specified string or the string could not be parsed.
    private @Nullable URL initURL(String parameterValue) {
            return (new URI(parameterValue)).toURL();
        } catch (MalformedURLException | URISyntaxException mue) {
            if (f.isFile()) {
                    return f.toURI().toURL();
                } catch (IllegalArgumentException | MalformedURLException e) {
     * If there are redundant parameters or options or the required is missing the result will be the failure of the
     * command. This command has:
     * <li><b>PrintStackTrace</b> is common for all commands and its presence triggers printing of stack trace in case
     * of exception.
     * <li><b>url</b> is required and it points the resource of automation objects that has to be imported.
        boolean getUrl = true;
            } else if (getUrl) {
                url = initURL(parameterValues[i]);
                if (url != null) {
                    getUrl = false;
        if (getUrl) {
            return "Missing source URL parameter or its value is incorrect!";
