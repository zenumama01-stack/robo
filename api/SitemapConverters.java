package org.openhab.core.model.sitemap.valueconverter;
import org.eclipse.xtext.conversion.impl.AbstractNullSafeConverter;
public class SitemapConverters extends DefaultTerminalConverters {
    private static final Pattern ID_PATTERN = Pattern.compile("\\p{Alpha}\\w*");
    @ValueConverter(rule = "Icon")
    public IValueConverter<String> Icon() {
        return new IValueConverter<>() {
            public String toValue(String string, INode node) throws ValueConverterException {
                if (string != null && string.startsWith("\"")) {
                    return string.substring(1, string.length() - 1);
            public String toString(String value) throws ValueConverterException {
                if (containsWhiteSpace(value)) {
                    return "\"" + value + "\"";
    @ValueConverter(rule = "Command")
    public IValueConverter<String> Command() {
        return new AbstractNullSafeConverter<>() {
            protected String internalToValue(String string, INode node) {
                if ((string.startsWith("'") && string.endsWith("'"))
                        || (string.startsWith("\"") && string.endsWith("\""))) {
                    return STRING().toValue(string, node);
                return ID().toValue(string, node);
            protected String internalToString(String value) {
                if (ID_PATTERN.matcher(value).matches()) {
                    return ID().toString(value);
                    return STRING().toString(value);
    public static boolean containsWhiteSpace(final String string) {
            for (int i = 0; i < string.length(); i++) {
                if (Character.isWhitespace(string.charAt(i))) {
