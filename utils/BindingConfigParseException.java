package org.openhab.core.model.item;
 * This exception is used by {@link BindingConfigReader} instances if parsing configurations fails
public class BindingConfigParseException extends Exception {
    private static final long serialVersionUID = 1434607160082879845L;
    public BindingConfigParseException(String msg) {
    public BindingConfigParseException(String msg, Exception e) {
        super(msg, e);
