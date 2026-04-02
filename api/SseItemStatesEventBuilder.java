package org.openhab.core.io.rest.sse.internal;
import javax.ws.rs.sse.OutboundSseEvent.Builder;
import org.openhab.core.io.rest.sse.internal.dto.StateDTO;
import org.openhab.core.types.StateOption;
 * The {@link SseItemStatesEventBuilder} builds {@link OutboundSseEvent}s for connections that listen to item state
 * changes.
@Component(service = SseItemStatesEventBuilder.class)
public class SseItemStatesEventBuilder {
    private final Logger logger = LoggerFactory.getLogger(SseItemStatesEventBuilder.class);
    public SseItemStatesEventBuilder(final @Reference ItemRegistry itemRegistry,
            final @Reference LocaleService localeService, final @Reference TimeZoneProvider timeZoneProvider,
    public @Nullable OutboundSseEvent buildEvent(Builder eventBuilder, Set<String> itemNames) {
        Map<String, StateDTO> payload = new HashMap<>(itemNames.size());
        for (String itemName : itemNames) {
                StateDTO stateDto = new StateDTO();
                stateDto.state = item.getState().toString();
                stateDto.type = getStateType(item.getState());
                String displayState = getDisplayState(item, localeService.getLocale(null));
                // Only include the display state if it's different than the raw state
                if (stateDto.state != null && !stateDto.state.equals(displayState)) {
                    stateDto.displayState = displayState;
                if (item.getState() instanceof DecimalType decimalState) {
                    stateDto.numericState = decimalState.floatValue();
                if (item.getState() instanceof QuantityType quantityState) {
                    stateDto.numericState = quantityState.floatValue();
                    stateDto.unit = quantityState.getUnit().toString();
                payload.put(itemName, stateDto);
                if (startLevelService.getStartLevel() >= StartLevelService.STARTLEVEL_MODEL) {
                    logger.warn("Attempting to send a state update of an item which doesn't exist: {}", itemName);
        if (!payload.isEmpty()) {
            return eventBuilder.mediaType(MediaType.APPLICATION_JSON_TYPE).data(payload).build();
    protected @Nullable String getDisplayState(Item item, Locale locale) {
        String displayState = state.toString();
            // First check if the pattern is a transformation
                            displayState = transformation.transform(function, state.format(format));
                            if (displayState == null) {
                                displayState = state.toString();
                    logger.warn("Failed transforming the state '{}' on item '{}' with pattern '{}': {}", state,
            // If no transformation, NULL/UNDEF state is returned as "NULL"/"UNDEF" without considering anything else
            else if (!(state instanceof UnDefType)) {
                boolean optionMatched = false;
                if (!stateDescription.getOptions().isEmpty()) {
                    // Look for a state option with a value corresponding to the state
                    for (StateOption option : stateDescription.getOptions()) {
                        String label = option.getLabel();
                        if (option.getValue().equals(state.toString()) && label != null) {
                            optionMatched = true;
                                displayState = pattern == null ? label : String.format(pattern, label);
                                        "Unable to format option label '{}' of item {} using format pattern '{}': {}, displaying option label",
                                        label, item.getName(), pattern, e.getMessage());
                                displayState = label;
                if (pattern != null && !optionMatched) {
                    // if it's not a transformation pattern and there is no matching state option,
                    // then it must be a format string
                    if (state instanceof QuantityType quantityState) {
                        // sanity convert current state to the item state description unit in case it was
                        // updated in the meantime. The item state is still in the "original" unit while the
                        // state description will display the new unit:
                        Unit<?> patternUnit = UnitUtils.parseUnit(pattern);
                        if (patternUnit != null && !quantityState.getUnit().equals(patternUnit)) {
                            quantityState = quantityState.toInvertibleUnit(patternUnit);
                        if (quantityState != null) {
                            state = quantityState;
                    // The following exception handling has been added to work around a Java bug with formatting
                    // numbers. See http://bugs.sun.com/view_bug.do?bug_id=6476425
                    // This also handles IllegalFormatConversionException, which is a subclass of
                    // IllegalArgument.
                        if (state instanceof DateTimeType dateTimeState) {
                            displayState = dateTimeState.format(pattern, timeZoneProvider.getTimeZone());
                            displayState = state.format(pattern);
                                "Unable to format value '{}' of item {} using format pattern '{}': {}, displaying raw state",
                                state, item.getName(), pattern, e.getMessage());
        return displayState;
    // Taken from org.openhab.core.items.events.ItemEventFactory
    private static String getStateType(State state) {
        String stateClassName = state.getClass().getSimpleName();
        return stateClassName.substring(0, stateClassName.length() - "Type".length());
