package org.openhab.core.model.script.internal;
 * This is a {@link HumanLanguageInterpreter} implementation which is mainly meant for backward-compatibility in the way
 * that it passes the texts to interpret as a command to a specified String item, on which DSL rules can then process
 * the string and trigger some actions.
 * The implicit agreement was an item called "VoiceCommand" to do exactly this; existing apps are using this and hence
 * this service will make this work.
@Component(immediate = true, service = HumanLanguageInterpreter.class, configurationPid = "org.openhab.rulehli", //
        property = Constants.SERVICE_PID + "=org.openhab.rulehli")
@ConfigurableService(category = "system", label = "Rule Voice Interpreter", description_uri = RuleHumanLanguageInterpreter.CONFIG_URI)
public class RuleHumanLanguageInterpreter implements HumanLanguageInterpreter {
    private final Logger logger = LoggerFactory.getLogger(RuleHumanLanguageInterpreter.class);
    protected static final String CONFIG_URI = "system:rulehli";
    private String itemName = "VoiceCommand";
    public RuleHumanLanguageInterpreter(final @Reference EventPublisher eventPublisher) {
            String configItemName = (String) config.get("item");
            if (configItemName != null && ItemUtil.isValidItemName(configItemName)) {
                itemName = configItemName;
                logger.debug("Using item '{}' for passing voice commands.", itemName);
        return "rulehli";
        return "Rule-based Interpreter";
    public String interpret(Locale locale, String text) throws InterpretationException {
        Event event = ItemEventFactory.createCommandEvent(itemName, new StringType(text));
    public @Nullable String getGrammar(Locale locale, String format) {
    public Set<Locale> getSupportedLocales() {
        // we do not care about locales, so we return an empty set here to indicate this
    public Set<String> getSupportedGrammarFormats() {
