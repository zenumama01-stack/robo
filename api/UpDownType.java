public enum UpDownType implements PrimitiveType, State, Command {
    UP,
    DOWN;
            return target.cast(equals(UP) ? DecimalType.ZERO : new DecimalType(new BigDecimal("1.0")));
            return target.cast(equals(UP) ? PercentType.ZERO : PercentType.HUNDRED);
