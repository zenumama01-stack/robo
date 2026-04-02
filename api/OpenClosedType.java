public enum OpenClosedType implements PrimitiveType, State, Command {
    OPEN,
    CLOSED;
            return target.cast(this == OPEN ? new DecimalType(1) : DecimalType.ZERO);
            return target.cast(this == OPEN ? PercentType.HUNDRED : PercentType.ZERO);
