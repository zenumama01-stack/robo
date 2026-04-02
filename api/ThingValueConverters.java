package org.openhab.core.model.thing.valueconverter;
public class ThingValueConverters extends DefaultTerminalConverters {
    private UIDtoStringConverter uidToStringConverter;
    @ValueConverter(rule = "UID")
    public IValueConverter<String> UID() {
        return uidToStringConverter;
