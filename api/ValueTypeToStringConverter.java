package org.openhab.core.model.core.valueconverter;
import org.eclipse.xtext.conversion.IValueConverter;
import org.eclipse.xtext.conversion.ValueConverterException;
import org.eclipse.xtext.nodemodel.INode;
import org.eclipse.xtext.util.Strings;
 * A {@link ValueTypeToStringConverter} is used to create a {@link String}, {@link Boolean}, or {@link BigDecimal} from
 * an input string and vice versa.
public class ValueTypeToStringConverter implements IValueConverter<@Nullable Object> {
    public @Nullable Object toValue(@Nullable String string, @Nullable INode node) throws ValueConverterException {
        if (string == null) {
        if (string.startsWith("\"") && string.endsWith("\"")) {
                return Strings.convertFromJavaString(string.substring(1, string.length() - 1), true);
                throw new ValueConverterException(e.getMessage(), node, e);
        if ("true".equals(string) || "false".equals(string)) {
            return Boolean.valueOf(string);
            return new BigDecimal(string);
            throw new ValueConverterException("Number expected.", node, e);
    public String toString(@Nullable Object value) throws ValueConverterException {
            throw new ValueConverterException("Value may not be null.", null, null);
            return toEscapedString(string);
        if (value instanceof BigDecimal decimalValue) {
            return decimalValue.toPlainString();
        if (value instanceof Boolean boolean1) {
            return boolean1.toString();
        throw new ValueConverterException("Unknown value type: " + value.getClass().getSimpleName(), null, null);
    protected String toEscapedString(String value) {
        return '"' + Strings.convertToJavaString(value, false) + '"';
