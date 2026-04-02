import java.lang.reflect.Constructor;
import java.lang.reflect.Modifier;
import java.lang.reflect.ParameterizedType;
 * Map an OSGi configuration map {@code Map<String, Object>} or type-less value to an individual configuration bean or
 * typed value.
 * @author David Graeff - Initial contribution
 * @author Jan N. Klug - Extended and refactored to an exposed utility class
public final class ConfigParser {
    private static final Logger LOGGER = LoggerFactory.getLogger(ConfigParser.class);
    private static final Map<String, Class<?>> WRAPPER_CLASSES_MAP = Map.of(//
            "float", Float.class, //
            "double", Double.class, //
            "long", Long.class, //
            "int", Integer.class, //
            "short", Short.class, //
            "byte", Byte.class, //
            "boolean", Boolean.class);
    private ConfigParser() {
        // prevent instantiation
     * Use this method to automatically map a configuration collection to a Configuration holder object. A common
     * use-case would be within a service. Usage example:
     * {@code
     * public void modified(Map<String, Object> properties) {
     *     YourConfig config = ConfigParser.configurationAs(properties, YourConfig.class);
     * @param properties The configuration map.
     * @param configurationClass The configuration holder class. An instance of this will be created so make sure that
     *            a default constructor is available.
     * @return The configuration holder object. All fields that matched a configuration option are set. If a required
     *         field is not set, null is returned.
    public static <T> @Nullable T configurationAs(Map<String, @Nullable Object> properties,
            Class<T> configurationClass) {
        Constructor<T> constructor;
        T configuration = null;
            if (!configurationClass.isRecord()) {
                constructor = configurationClass.getConstructor();
                configuration = constructor.newInstance();
                Constructor<?>[] constructors = configurationClass.getConstructors();
                constructor = (Constructor<T>) constructors[0];
        } catch (NoSuchMethodException | InstantiationException | IllegalAccessException
                | InvocationTargetException e) {
        Map<Field, Object> initArgs = new LinkedHashMap<>();
        for (Field field : getAllFields(configurationClass)) {
            // Don't try to write to final fields and ignore transient fields when it's a class
            if (!configurationClass.isRecord()
                    && (Modifier.isFinal(field.getModifiers()) || Modifier.isTransient(field.getModifiers()))) {
            String fieldName = field.getName();
            Object value = properties.get(fieldName);
            // Consider RequiredField annotations
                LOGGER.trace("Skipping field '{}', because config has no entry for it", fieldName);
            Class<?> type = field.getType();
            // Allows to have List<int>, List<Double>, List<String> etc (and the corresponding Set<?>)
            if (value instanceof Collection valueCollection) {
                Collection collection = List.class.isAssignableFrom(type) ? new ArrayList<>()
                        : Set.class.isAssignableFrom(type) ? new HashSet<>() : //
                if (collection != null) {
                    Class<?> innerClass = (Class<?>) ((ParameterizedType) field.getGenericType())
                            .getActualTypeArguments()[0];
                    valueCollection.stream().map(it -> valueAs(it, innerClass)).filter(Object.class::isInstance)
                            .forEach(collection::add);
                    initArgs.put(field, collection);
                    LOGGER.warn("Skipping field '{}', only List and Set is supported as target Collection", fieldName);
                value = valueAs(value, type);
                    initArgs.put(field, value);
                    LOGGER.warn(
                            "Could not set value for field '{}' because conversion failed. Check your configuration value.",
                            fieldName);
        if (configuration instanceof T localConfiguration) {
            initArgs.forEach((field, value) -> {
                    LOGGER.trace("Setting value ({}) {} to field '{}' in configuration class {}",
                            field.getType().getSimpleName(), value, field.getName(), configurationClass.getName());
                    field.setAccessible(true);
                    field.set(localConfiguration, value);
                    LOGGER.warn("Could not set field value for field '{}': {}", field.getName(), e.getMessage(), e);
                configuration = constructor.newInstance(initArgs.values().toArray());
            } catch (InstantiationException | IllegalAccessException | InvocationTargetException e) {
                LOGGER.warn("Could invoke default record constructor '{}'", e.getMessage(), e);
     * Return fields of the given class as well as all super classes.
     * @param clazz The class
     * @return A list of Field objects
    private static List<Field> getAllFields(Class<?> clazz) {
        List<Field> fields = new ArrayList<>();
        for (Class<?> superclazz = clazz; superclazz != null; superclazz = superclazz.getSuperclass()) {
            fields.addAll(Arrays.asList(superclazz.getDeclaredFields()));
     * Convert a value to a given type or return default value
     * @param value input value or String representation of that value
     * @param type desired target class
     * @param defaultValue default value to be used if conversion fails or input value is null
     * @return the converted value or the default value if value is null or conversion fails
    public static <T> T valueAsOrElse(@Nullable Object value, Class<T> type, T defaultValue) {
        return Objects.requireNonNullElse(valueAs(value, type), defaultValue);
     * Convert a value to a given type
     * @return the converted value or null if conversion fails or input value is null
    public static <T> @Nullable T valueAs(@Nullable Object value, Class<T> type) {
        if (value == null || type.isAssignableFrom(value.getClass())) {
            // exit early if value is null or type is already compatible
            return (T) value;
        // make sure primitives are converted to their respective wrapper class
        Class<?> typeClass = WRAPPER_CLASSES_MAP.getOrDefault(type.getSimpleName(), type);
        Object result = value;
        // Handle the conversion case of Number to Float,Double,Long,Integer,Short,Byte,BigDecimal
        if (value instanceof Number number) {
            if (Float.class.equals(typeClass)) {
                result = number.floatValue();
            } else if (Double.class.equals(typeClass)) {
                result = number.doubleValue();
            } else if (Long.class.equals(typeClass)) {
                result = number.longValue();
            } else if (Integer.class.equals(typeClass)) {
                result = number.intValue();
            } else if (Short.class.equals(typeClass)) {
                result = number.shortValue();
            } else if (Byte.class.equals(typeClass)) {
                result = number.byteValue();
            } else if (BigDecimal.class.equals(typeClass)) {
                result = new BigDecimal(number.toString());
        } else if (value instanceof String strValue && !String.class.equals(typeClass)) {
            // Handle the conversion case of String to Float,Double,Long,Integer,BigDecimal,Boolean
                result = Float.valueOf(strValue);
                result = Double.valueOf(strValue);
                result = Long.valueOf(strValue);
                result = Integer.valueOf(strValue);
                result = Short.valueOf(strValue);
                result = Byte.valueOf(strValue);
                result = new BigDecimal(strValue);
            } else if (Boolean.class.equals(typeClass)) {
                result = Boolean.valueOf(strValue);
            } else if (type.isEnum()) {
                final Class<? extends Enum> enumType = (Class<? extends Enum>) typeClass;
                    result = Enum.valueOf(enumType, value.toString());
            } else if (Set.class.isAssignableFrom(typeClass)) {
                result = Set.of(value);
            } else if (Collection.class.isAssignableFrom(typeClass)) {
                result = List.of(value);
        if (result != null && typeClass.isAssignableFrom(result.getClass())) {
            return (T) result;
        LOGGER.warn("Conversion of value '{}' with type '{}' to '{}' failed. Returning null", value, value.getClass(),
