package org.openhab.core.model.internal.valueconverter;
import org.eclipse.xtext.common.services.DefaultTerminalConverters;
import org.eclipse.xtext.conversion.ValueConverter;
import org.openhab.core.model.core.valueconverter.ValueTypeToStringConverter;
import com.google.inject.Inject;
 * Registers {@link IValueConverter}s for the items language.
public class ItemValueConverters extends DefaultTerminalConverters {
    @Inject
    private ValueTypeToStringConverter valueTypeToStringConverter;
    @ValueConverter(rule = "ValueType")
    public IValueConverter<Object> ValueType() {
        return valueTypeToStringConverter;
