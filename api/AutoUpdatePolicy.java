 * A binding's recommendation to the framework whether a state update should be automatically sent to an item if a
 * command was received.
public enum AutoUpdatePolicy {
     * No automatic state update should be sent by the framework. The handler will make sure it sends a state update and
     * it can do it better than just converting the command to a state.
    VETO,
     * The binding does not care and the framework may do what it deems to be right. The state update which the
     * framework will send out normally will correspond the command state anyway. This is the default if no other policy
     * is set.
    DEFAULT,
     * An automatic state update should be sent by the framework because no updates will be sent by the binding.
     * This usually is the case when devices don't expose their current state to the handler.
    RECOMMEND;
     * Parses the input string into an {@link AutoUpdatePolicy}.
     * @param input the input string
     * @return the parsed AutoUpdatePolicy or null if the input was null
     * @throws IllegalArgumentException if the input couldn't be parsed.
    public static @Nullable AutoUpdatePolicy parse(@Nullable String input) {
        if (input == null) {
        for (AutoUpdatePolicy value : values()) {
            if (value.name().equalsIgnoreCase(input)) {
        throw new IllegalArgumentException(String.format("Unknown auto update policy: '%s'", input));
