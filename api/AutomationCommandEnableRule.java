 * This class provides functionality of command {@link AutomationCommands#ENABLE_RULE}.
public class AutomationCommandEnableRule extends AutomationCommand {
     * This field keeps the value of "enable" parameter of the command.
    private boolean enable;
     * This field indicates the presence of the "enable" parameter of the command.
    private boolean hasEnable;
     * This field keeps the specified rule UID.
    public AutomationCommandEnableRule(String command, String[] parameterValues, int providerType,
        super(command, parameterValues, providerType, autoCommands);
    public String execute() {
        String uid = this.uid;
        if (!SUCCESS.equals(parsingResult) || uid == null) {
            return parsingResult;
        if (hasEnable) {
            autoCommands.setEnabled(uid, enable);
            return SUCCESS;
            RuleStatus status = autoCommands.getRuleStatus(uid);
            if (status != null) {
                return Printer.printRuleStatus(uid, status);
    protected String parseOptionsAndParameters(String[] parameterValues) {
        for (String parameterValue : parameterValues) {
            if (null == parameterValue) {
            if (parameterValue.charAt(0) == '-') {
                if (OPTION_ST.equals(parameterValue)) {
                    st = true;
                return String.format("Unsupported option: %s", parameterValue);
            if (uid == null) {
                uid = parameterValue;
            getEnable(parameterValue);
                return "Missing required parameter: Rule UID";
            return String.format("Unsupported parameter: %s", parameterValue);
     * Utility method for parsing the command parameter - "enable".
     * @param parameterValue is the value entered from command line.
    private void getEnable(String parameterValue) {
        if ("true".equals(parameterValue)) {
            enable = true;
            hasEnable = true;
        } else if ("false".equals(parameterValue)) {
            enable = false;
            hasEnable = false;
