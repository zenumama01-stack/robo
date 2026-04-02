 * Provides utility methods for working with {@link ConfigurableService} so the property names can remain hidden.
 * These methods cannot be part of {@link ConfigurableService} as that introduces an annotation cycle.
public class ConfigurableServiceUtil {
    static final String SERVICE_PROPERTY_DESCRIPTION_URI = ConfigurableService.PREFIX_ + "description.uri";
    static final String SERVICE_PROPERTY_LABEL = ConfigurableService.PREFIX_ + "label";
    static final String SERVICE_PROPERTY_CATEGORY = ConfigurableService.PREFIX_ + "category";
    static final String SERVICE_PROPERTY_FACTORY_SERVICE = ConfigurableService.PREFIX_ + "factory";
    // all singleton services without multi-config services
    public static final String CONFIGURABLE_SERVICE_FILTER = "(&(" + SERVICE_PROPERTY_DESCRIPTION_URI + "=*)(!("
            + SERVICE_PROPERTY_FACTORY_SERVICE + "=*)))";
    // all multi-config services without singleton services
    public static final String CONFIGURABLE_MULTI_CONFIG_SERVICE_FILTER = "(" + SERVICE_PROPERTY_FACTORY_SERVICE
            + "=*)";
    public static ConfigurableService asConfigurableService(Function<String, @Nullable Object> propertyResolver) {
        return new ConfigurableService() {
            public Class<? extends Annotation> annotationType() {
                return ConfigurableService.class;
            public String label() {
                return resolveString(propertyResolver, SERVICE_PROPERTY_LABEL);
            public boolean factory() {
                return resolveBoolean(propertyResolver, SERVICE_PROPERTY_FACTORY_SERVICE);
            public String description_uri() {
                return resolveString(propertyResolver, SERVICE_PROPERTY_DESCRIPTION_URI);
            public String category() {
                return resolveString(propertyResolver, SERVICE_PROPERTY_CATEGORY);
    private static String resolveString(Function<String, @Nullable Object> propertyResolver, String key) {
        String value = (String) propertyResolver.apply(key);
        return value == null ? "" : value;
    private static boolean resolveBoolean(Function<String, @Nullable Object> propertyResolver, String key) {
        Boolean value = (Boolean) propertyResolver.apply(key);
        return value != null && value.booleanValue();
