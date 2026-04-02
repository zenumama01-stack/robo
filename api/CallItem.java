package org.openhab.core.library.items;
 * This item identifies a telephone call by its origin and destination.
 * @author Gaël L'hopital - port to Eclipse SmartHome
public class CallItem extends GenericItem {
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(StringListType.class,
            UnDefType.class);
    private static final List<Class<? extends Command>> ACCEPTED_COMMAND_TYPES = List.of(RefreshType.class);
    public CallItem(String name) {
        super(CoreItemFactory.CALL, name);
        return ACCEPTED_DATA_TYPES;
        return ACCEPTED_COMMAND_TYPES;
        if (isAcceptedState(ACCEPTED_DATA_TYPES, state)) {
            logSetTypeError(state);
        if (timeSeries.getStates().allMatch(s -> isAcceptedState(ACCEPTED_DATA_TYPES, s.state()))) {
            logSetTypeError(timeSeries);
        return getCommandOptions(locale);
