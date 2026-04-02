 * <li>{@link AutomationCommands#REMOVE_MODULE_TYPES}
 * <li>{@link AutomationCommands#REMOVE_TEMPLATES}
 * <li>{@link AutomationCommands#REMOVE_RULES}
 * <li>{@link AutomationCommands#REMOVE_RULE}
 * @author Kai Kreuzer - fixed feedback when deleting non-existent rule
 * @author Marin Mitev - removed prefixes in the output
public class AutomationCommandRemove extends AutomationCommand {
     * This field keeps the UID of the {@link Rule} if command is {@link AutomationCommands#REMOVE_RULE}
    private @Nullable String id;
     * This field keeps URL of the source of automation objects that has to be removed.
    public AutomationCommandRemove(String command, String[] params, int providerType,
        String id = this.id;
        if (!SUCCESS.equals(parsingResult) || id == null || url == null) {
                return autoCommands.remove(AutomationCommands.MODULE_TYPE_PROVIDER, url);
                return autoCommands.remove(AutomationCommands.TEMPLATE_PROVIDER, url);
                if (AutomationCommands.REMOVE_RULE.equals(command)) {
                    return autoCommands.removeRule(id);
                } else if (AutomationCommands.REMOVE_RULES.equals(command)) {
                    return autoCommands.removeRules(id);
     * This method is invoked from the constructor to parse all parameters and options of the command <b>REMOVE</b>.
     * If there are redundant parameters or options or the required are missing the result will be the failure of the
     * <li><b>id</b> is required for {@link AutomationCommands#REMOVE_RULE} command. If it is present for all
     * <b>REMOVE</b> commands, except {@link AutomationCommands#REMOVE_RULE}, it will be treated as redundant.
     * <li><b>url</b> is required for all <b>REMOVE</b> commands, except {@link AutomationCommands#REMOVE_RULE}.
     * If it is present for {@link AutomationCommands#REMOVE_RULE}, it will be treated as redundant.
            } else if (parameterValue.charAt(0) == '-') {
                url = initURL(parameterValue);
            } else if (getId) {
            return "Missing source URL parameter!";
            return "Missing UID parameter!";
