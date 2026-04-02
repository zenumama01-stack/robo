public enum OnOffType implements PrimitiveType, State, Command {
    OFF;
     * Converts a String value "ON" or "1" to {@link OnOffType#ON} or else to {@link OnOffType#OFF}.
     * @param state String to convert to {@link OnOffType}
     * @return returns the ON or OFF state based on the String
    public static OnOffType from(String state) {
        return from("ON".equalsIgnoreCase(state) || "1".equalsIgnoreCase(state));
     * @param state boolean to convert to {@link OnOffType}
     * @return returns the ON or OFF state based on the boolean
    public static OnOffType from(boolean state) {
        return state ? ON : OFF;
        if (target == DecimalType.class) {
            return target.cast(this == ON ? new DecimalType(1) : DecimalType.ZERO);
            return target.cast(this == ON ? PercentType.HUNDRED : PercentType.ZERO);
            return target.cast(this == ON ? HSBType.WHITE : HSBType.BLACK);
