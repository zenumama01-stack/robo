 * This class is a wrapper of {@link RuleFileProvider}s, responsible for initializing the WatchService.
@Component(immediate = true, service = RuleProvider.class)
public class RuleFileProviderWatcher extends RuleFileProvider {
     * Creates a new instance using the specified watch service.
     * @param watchService the {@link WatchService} to use.
    public RuleFileProviderWatcher(@Reference(target = WatchService.CONFIG_WATCHER_FILTER) WatchService watchService) {
    @Reference(cardinality = ReferenceCardinality.AT_LEAST_ONE, policy = ReferencePolicy.DYNAMIC, target = "(parser.type=parser.rule)")
    public void addParser(Parser<Rule> parser, Map<String, String> properties) {
    public void removeParser(Parser<Rule> parser, Map<String, String> properties) {
    protected void validateObject(Rule rule) throws ValidationException {
        if ((s = rule.getUID()) == null || s.isBlank()) {
            throw new ValidationException(ObjectType.RULE, null, "UID cannot be blank");
        if ((s = rule.getName()) == null || s.isBlank()) {
            throw new ValidationException(ObjectType.RULE, rule.getUID(), "Name cannot be blank");
