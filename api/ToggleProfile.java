 * This profile reads the triggered events and uses the item's current state
 * to toggle it. Being configurable via the constructor, {@link ProfileFactory}
 * can use it for various system channel types and item types.
 * @author Patrick Fink - Initial contribution
public class ToggleProfile<T extends State & Command> implements TriggerProfile {
    public static final String EVENT_PARAM = "event";
    private final Logger logger = LoggerFactory.getLogger(ToggleProfile.class);
    private ProfileTypeUID uid;
    private ChannelType channelType;
    private T initialState;
    private T alternativeState;
    private final String triggerEvent;
    public ToggleProfile(ProfileCallback callback, ProfileContext context, ProfileTypeUID uid, ChannelType channelType,
            T initialState, T alternativeState, String defaultEvent) {
        this.channelType = channelType;
        this.initialState = initialState;
        this.alternativeState = alternativeState;
        String triggerEventParam = (String) context.getConfiguration().get(EVENT_PARAM);
        if (isValidEvent(triggerEventParam)) {
            triggerEvent = triggerEventParam;
            if (triggerEventParam != null) {
                        "'{}' is not a valid trigger event for Profile '{}'. Default trigger event '{}' is used instead.",
                        triggerEventParam, this.getProfileTypeUID().getAsString(), defaultEvent);
            triggerEvent = defaultEvent;
    public boolean isValidEvent(@Nullable String triggerEvent) {
        return channelType.getEvent().getOptions().stream().anyMatch(e -> e.getValue().equals(triggerEvent));
        if (triggerEvent.equals(event)) {
            T newState = initialState.equals(previousState) ? alternativeState : initialState;
            callback.sendCommand(newState);
            previousState = newState;
        previousState = state.as(initialState.getClass());
