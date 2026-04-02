 * This interface has to be implemented by all providers of {@link Template}s.
 * The {@link TemplateRegistry} uses it to get access to available {@link Template}s.
 * @author Ana Dimova - add registration property - rule.templates
public interface TemplateProvider<E extends Template> extends Provider<E> {
     * Gets the localized Templates defined by this provider. When the localization is not specified or it is not
     * supported a Template localized with default locale is returned.
     * @param uid unique identifier of the desired Template.
     * @param locale specifies the desired {@link Locale} to be used for localization of the returned element. If
     *            localization resources for this locale are not available or the passed locale is {@code null} the
     *            element is returned with the default localization.
     * @return the desired localized Template.
    E getTemplate(String uid, @Nullable Locale locale);
     * Gets the localized Templates defined by this provider. When localization is not specified or it is not supported
     * a Templates with default localization is returned.
     * @param locale specifies the desired {@link Locale} to be used for localization of the returned elements. If
     *            elements are returned with the default localization.
     * @return a collection of localized {@link Template}s provided by this provider.
    Collection<E> getTemplates(@Nullable Locale locale);
