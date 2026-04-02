 * This interface has to be implemented by all providers of {@link ModuleType}s.
 * The {@link ModuleTypeRegistry} uses it to get access to available {@link ModuleType}s.
 * @author Ana Dimova - add registration property - module.types
public interface ModuleTypeProvider extends Provider<ModuleType> {
     * Gets the localized {@link ModuleType} defined by this provider. When the localization is not specified
     * or it is not supported a {@link ModuleType} with default locale is returned.
     * @param uid unique identifier of the {@link ModuleType}.
     * @param locale defines localization of label and description of the {@link ModuleType} or null.
     * @param <T> the type of the required object.
     * @return localized module type.
    <T extends ModuleType> @Nullable T getModuleType(String uid, @Nullable Locale locale);
     * Gets the localized {@link ModuleType}s defined by this provider. When localization is not specified or
     * it is not supported the {@link ModuleType}s with default localization is returned.
     * @param locale defines localization of label and description of the {@link ModuleType}s or null.
     * @return collection of localized {@link ModuleType} provided by this provider.
    <T extends ModuleType> Collection<T> getModuleTypes(@Nullable Locale locale);
