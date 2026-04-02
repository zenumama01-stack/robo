 * This class is a wrapper of multiple {@link TemplateProvider}s, responsible for initializing the WatchService.
 * @author Arne Seime - Added template validation support
@Component(immediate = true, service = RuleTemplateProvider.class)
public class TemplateFileProviderWatcher extends TemplateFileProvider {
    public TemplateFileProviderWatcher(
    protected void validateObject(RuleTemplate template) throws ValidationException {
