 * This class is a {@link RuleProvider} implementation that provides file-based rules. It extends the functionality
 * of {@link AbstractFileProvider} for importing the {@link Rule}s from local files.
public abstract class RuleFileProvider extends AbstractFileProvider<Rule> implements RuleProvider {
     * Creates a new instance.
    public RuleFileProvider() {
        super("rules");
    protected String getUID(Rule providedObject) {
        return List.copyOf(providedObjectsHolder.values());
