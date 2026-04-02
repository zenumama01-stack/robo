 * This class is a {@link Rule}s importer. It extends functionality of {@link AbstractCommandProvider}.
 * It is responsible for execution of Automation Commands, corresponding to the {@link Rule}s:
 * <li>imports the {@link Rule}s from local files or from URL resources
 * <li>provides functionality for persistence of the {@link Rule}s
 * <li>removes the {@link Rule}s and their persistence
 * <li>lists the {@link Rule}s and their details
public class CommandlineRuleImporter extends AbstractCommandProvider<Rule> {
     * This constructor creates instances of this particular implementation of Rule Importer. It does not add any new
     * functionality to the constructors of the providers. Only provides consistency by invoking the parent's
     * constructor.
    public CommandlineRuleImporter(BundleContext context, RuleRegistry ruleRegistry) {
        super(context);
     * For this concrete provider, this type is a {@link Rule} {@link Parser}.
        if (reference != null && Parser.PARSER_RULE.equals(reference.getProperty(Parser.PARSER_TYPE))) {
     * This method is responsible for exporting a set of Rules in a specified file.
     * @param parserType is relevant to the format that you need for conversion of the Rules in text.
     * @param set a set of Rules to export.
     * @see AutomationCommandsPluggable#exportRules(String, Set, File)
     * This method is responsible for importing a set of Rules from a specified file or URL resource.
     * @see AutomationCommandsPluggable#importRules(String, URL)
    public Set<Rule> importRules(String parserType, URL url) throws IOException, ParsingException {
        Parser<Rule> parser = parsers.get(parserType);
            InputStreamReader inputStreamReader = new InputStreamReader(new BufferedInputStream(url.openStream()));
            throw new ParsingException(new ParsingNestedException(ParsingNestedException.RULE, null,
    protected Set<Rule> importData(URL url, Parser<Rule> parser, InputStreamReader inputStreamReader)
        Set<Rule> providedRules = parser.parse(inputStreamReader);
        if (providedRules != null && !providedRules.isEmpty()) {
            for (Rule rule : providedRules) {
                    if (ruleRegistry.get(rule.getUID()) != null) {
                        ruleRegistry.add(rule);
        return providedRules;
