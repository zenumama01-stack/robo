 * Kind of the channel.
public enum ChannelKind {
     * Channels which have a state.
    STATE,
     * Channels which can be triggered.
    TRIGGER;
     * Parses the input string into a {@link ChannelKind}.
     * @return the parsed ChannelKind.
    public static ChannelKind parse(@Nullable String input) {
            return STATE;
        for (ChannelKind value : values()) {
        throw new IllegalArgumentException("Unknown channel kind: '" + input + "'");
