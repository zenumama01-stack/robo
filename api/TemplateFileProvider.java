 * This class is implementation of {@link TemplateProvider}. It extends functionality of {@link AbstractFileProvider}
 * for importing the {@link RuleTemplate}s from local files.
public abstract class TemplateFileProvider extends AbstractFileProvider<RuleTemplate> implements RuleTemplateProvider {
    public TemplateFileProvider() {
        super("templates");
    protected String getUID(RuleTemplate providedObject) {
        return getTemplates(null);
        Collection<RuleTemplate> values = providedObjectsHolder.values();
        return new LinkedList<>(values);
