 * This interface provides basic functionality for managing {@link RuleTemplate}s. It can be used for
 * <li>Get the existing {@link RuleTemplate}s with the {@link Provider#getAll()},
 * {@link TemplateProvider#getTemplates(Locale)} and {@link #getTemplate(String, Locale)} methods.</li>
 * Listers that are listening for adding removing or updating can be added with the
 * {@link #addProviderChangeListener(ProviderChangeListener)}
 * and removed with the {@link #removeProviderChangeListener(ProviderChangeListener)} methods.
public interface RuleTemplateProvider extends TemplateProvider<RuleTemplate> {
