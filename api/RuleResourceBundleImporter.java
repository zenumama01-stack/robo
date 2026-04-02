 * This class is implementation of {@link RuleResourceBundleImporter}. It serves for providing {@link Rule}s by
 * loading
 * <li>the path to resources, corresponding to the {@link Rule}s - root directory
 * {@link AbstractResourceBundleProvider#ROOT_DIRECTORY} with sub-directory "rules".
 * <li>type of the {@link Parser}s, corresponding to the {@link Rule}s - {@link Parser#PARSER_RULE}
 * <li>specific functionality for loading the {@link Rule}s
 * <li>tracking the managing service of the {@link Rule}s.
public class RuleResourceBundleImporter extends AbstractResourceBundleProvider<Rule> {
     * This field holds the reference to the Rule Registry.
    protected @Nullable ManagedRuleProvider mProvider;
     * This constructor is responsible for initializing the path to resources and tracking the managing service of the
    public RuleResourceBundleImporter() {
        super(ROOT_DIRECTORY + "/rules/");
        this.mProvider = mProvider;
        mProvider = null;
     * This method provides functionality for processing the bundles with rule resources.
     * Checks for availability of the needed {@link Parser} and for availability of the rules managing service. If one
     * of them is not available - the bundle is added into {@link #waitingProviders} and the execution of the method
     * ends.
     * Continues with loading the rules. If a rule already exists, it is updated, otherwise it is added.
     *            it is a {@link Bundle} which has to be processed, because it provides resources for automation rules.
        logger.debug("Parse rules from bundle '{}' ", bsn);
                if (!getPreviousPortfolio(vendor).isEmpty()
                        && (waitingProviders.get(bundle) == null || !waitingProviders.get(bundle).contains(url))) {
                    Set<Rule> parsedObjects = parseData(parser, url, bundle);
                        addNewProvidedObjects(List.of(), List.of(), parsedObjects);
            putNewPortfolio(vendor, List.of());
            Set<Rule> parsedObjects) {
        if (parsedObjects != null && !parsedObjects.isEmpty()) {
            for (Rule rule : parsedObjects) {
                        mProvider.add(rule);
                        logger.debug("Not importing rule '{}' because: {}", rule.getUID(), e.getMessage(), e);
                        logger.debug("Not importing rule '{}' since the rule registry is in an invalid state: {}",
                                rule.getUID(), e.getMessage());
        List<String> portfolio = providerPortfolio.get(vendor);
                if (entry.getKey().getVendorSymbolicName().equals(vendor.getVendorSymbolicName())) {
                    return entry.getValue();
        providerPortfolio.remove(vendor);
    protected String getUID(Rule parsedObject) {
