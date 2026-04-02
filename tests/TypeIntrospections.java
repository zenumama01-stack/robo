 * The {@link TypeIntrospections} provides a corresponding {@link TypeIntrospection} for each config description
 * parameter type.
final class TypeIntrospections {
    private static final Map<Type, TypeIntrospection> INTROSPECTIONS = Map.of( //
            Type.BOOLEAN, new BooleanIntrospection(), //
            Type.TEXT, new StringIntrospection(), //
            Type.INTEGER, new IntegerIntrospection(), //
            Type.DECIMAL, new FloatIntrospection() //
    private TypeIntrospections() {
     * Returns the corresponding {@link TypeIntrospection} for the given type.
     * @param type the type for which the {@link TypeIntrospection} is to be returned
     * @return the {@link TypeIntrospection} for the given type
     * @throws IllegalArgumentException if no {@link TypeIntrospection} was found for the given type
    static TypeIntrospection get(Type type) {
        TypeIntrospection typeIntrospection = INTROSPECTIONS.get(type);
        if (typeIntrospection == null) {
            throw new IllegalArgumentException("There is no type introspection for type " + type);
        return typeIntrospection;
     * The {@link TypeIntrospection} provides operations to introspect the actual value for a configuration description
    abstract static class TypeIntrospection {
        private final Class<?> clazz;
        private final MessageKey minViolationMessageKey;
        private final MessageKey maxViolationMessageKey;
        private TypeIntrospection(Class<?> clazz) {
            this(clazz, null, null);
        private TypeIntrospection(Class<?> clazz, MessageKey minViolationMessageKey,
                MessageKey maxViolationMessageKey) {
            this.clazz = clazz;
            this.minViolationMessageKey = minViolationMessageKey;
            this.maxViolationMessageKey = maxViolationMessageKey;
         * Returns true, if the given value is less than the given min attribute, otherwise false.
         * @param value the corresponding value
         * @param min the value of the min attribute
         * @return true, if the given value is less than the given min attribute, otherwise false
        boolean isMinViolated(Object value, BigDecimal min) {
            if (min == null) {
            final BigDecimal bd;
            if (isBigDecimalInstance(value)) {
                bd = (BigDecimal) value;
                bd = new BigDecimal(value.toString());
            return bd.compareTo(min) < 0;
         * Returns true, if the given value is greater than the given max attribute, otherwise false.
         * @param max the value of the max attribute
         * @return true, if the given value is greater than the given max attribute, otherwise false
        boolean isMaxViolated(Object value, BigDecimal max) {
            if (max == null) {
            return bd.compareTo(max) > 0;
         * Returns true, if the given value can be assigned to the type of this introspection, otherwise false.
         * @return true, if the given value can be assigned to the type of this introspection, otherwise false
        boolean isAssignable(Object value) {
            return clazz.isAssignableFrom(value.getClass()) || isStringInstance(value);
         * Returns true, if the given value is a string, otherwise false.
         * @param value the value to be analyzed
         * @return true, if the given value is a string, otherwise false
        final boolean isStringInstance(Object value) {
            return value instanceof String;
         * Returns true, if the given value is a big decimal, otherwise false.
         * @return true, if the given value is a big decimal, otherwise false
        final boolean isBigDecimalInstance(Object value) {
            return value instanceof BigDecimal;
         * Returns the corresponding {@link MessageKey} for the min attribute violation.
         * @return the corresponding {@link MessageKey} for the min attribute violation
        final MessageKey getMinViolationMessageKey() {
            return minViolationMessageKey;
         * Returns the corresponding {@link MessageKey} for the max attribute violation.
         * @return the corresponding {@link MessageKey} for the max attribute violation
        final MessageKey getMaxViolationMessageKey() {
            return maxViolationMessageKey;
    private static final class BooleanIntrospection extends TypeIntrospection {
        private BooleanIntrospection() {
            super(Boolean.class);
            throw new UnsupportedOperationException("Min attribute not supported for boolean parameter.");
            throw new UnsupportedOperationException("Max attribute not supported for boolean parameter.");
    private static final class FloatIntrospection extends TypeIntrospection {
        private FloatIntrospection() {
            super(Float.class, MessageKey.MIN_VALUE_NUMERIC_VIOLATED, MessageKey.MAX_VALUE_NUMERIC_VIOLATED);
            if (!super.isAssignable(value)) {
                return isBigDecimalInstance(value);
    private static final class IntegerIntrospection extends TypeIntrospection {
        private IntegerIntrospection() {
            super(Integer.class, MessageKey.MIN_VALUE_NUMERIC_VIOLATED, MessageKey.MAX_VALUE_NUMERIC_VIOLATED);
    private static final class StringIntrospection extends TypeIntrospection {
        private StringIntrospection() {
            super(String.class, MessageKey.MIN_VALUE_TXT_VIOLATED, MessageKey.MAX_VALUE_TXT_VIOLATED);
            return ((String) value).length() < min.intValueExact();
            return ((String) value).length() > max.intValueExact();
