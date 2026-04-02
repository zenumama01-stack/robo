package org.openhab.core.library.types;
 * This interface is only a container for functions that require the core type library
 * for its calculations.
 * @author Thomas Eichstädt-Engelen - Added "N" functions
 * @author Gaël L'hopital - Added count function
 * @author Fabian Vollmann - Added XOR function
public interface ArithmeticGroupFunction extends GroupFunction {
     * This does a logical 'and' operation. Only if all items are of 'activeState' this
     * is returned, otherwise the 'passiveState' is returned.
     * Through the getStateAs() method, it can be determined, how many
     * items actually are not in the 'activeState'.
    class And implements GroupFunction {
        protected final State activeState;
        protected final State passiveState;
        public And(@Nullable State activeValue, @Nullable State passiveValue) {
            if (activeValue == null || passiveValue == null) {
                throw new IllegalArgumentException("Parameters must not be null!");
            this.activeState = activeValue;
            this.passiveState = passiveValue;
                // if we do not have any items, we return the passive state
                return passiveState;
                    if (!activeState.equals(item.getStateAs(activeState.getClass()))) {
                return activeState;
                if (stateClass == DecimalType.class) {
                        return stateClass.cast(new DecimalType(items.size() - count(items, activeState)));
                        return stateClass.cast(DecimalType.ZERO);
        private int count(Set<Item> items, State state) {
                if (state.equals(item.getStateAs(state.getClass()))) {
            return new State[] { activeState, passiveState };
     * This does a logical 'or' operation. If at least one item is of 'activeState' this
     * items actually are in the 'activeState'.
    class Or implements GroupFunction {
        public Or(@Nullable State activeValue, @Nullable State passiveValue) {
                    if (activeState.equals(item.getStateAs(activeState.getClass()))) {
                        return stateClass.cast(new DecimalType(count(items, activeState)));
     * This does a logical 'nand' operation. The state is 'calculated' by
     * the normal 'and' operation and than negated by returning the opposite
     * value. E.g. when the 'and' operation calculates the activeValue the
     * passiveValue will be returned and vice versa.
    class NAnd extends And {
        public NAnd(@Nullable State activeValue, @Nullable State passiveValue) {
            super(activeValue, passiveValue);
            State result = super.calculate(items);
            return activeState.equals(result) ? passiveState : activeState;
     * This does a logical 'nor' operation. The state is 'calculated' by
     * the normal 'or' operation and than negated by returning the opposite
     * value. E.g. when the 'or' operation calculates the activeValue the
    class NOr extends Or {
        public NOr(@Nullable State activeValue, @Nullable State passiveValue) {
     * This does a logical 'xor' operation. If exactly one item is of 'activeState' this
    class Xor implements GroupFunction {
        public Xor(@Nullable State activeValue, @Nullable State passiveValue) {
                boolean foundOne = false;
                        if (foundOne) {
     * This calculates the numeric average over all item states of decimal type.
    class Avg implements GroupFunction {
        public Avg() {
                    DecimalType itemState = item.getStateAs(DecimalType.class);
                    if (itemState != null) {
                        sum = sum.add(itemState.toBigDecimal());
                return new DecimalType(sum.divide(BigDecimal.valueOf(count), MathContext.DECIMAL128));
     * This calculates the numeric median over all item states of decimal type.
    class Median implements GroupFunction {
        public Median() {
                List<BigDecimal> states = items.stream().map(item -> item.getStateAs(DecimalType.class))
                        .filter(Objects::nonNull).map(DecimalType::toBigDecimal).toList();
                BigDecimal median = Statistics.median(states);
     * This calculates the numeric sum over all item states of decimal type.
    class Sum implements GroupFunction {
        public Sum() {
     * This calculates the minimum value of all item states of decimal type.
    class Min implements GroupFunction {
        public Min() {
            if (items != null && !items.isEmpty()) {
                BigDecimal min = null;
                        if (min == null || min.compareTo(itemState.toBigDecimal()) > 0) {
                            min = itemState.toBigDecimal();
                    return new DecimalType(min);
     * This calculates the maximum value of all item states of decimal type.
    class Max implements GroupFunction {
        public Max() {
                BigDecimal max = null;
                        if (max == null || max.compareTo(itemState.toBigDecimal()) < 0) {
                            max = itemState.toBigDecimal();
                    return new DecimalType(max);
     * This calculates the number of items in the group matching the
     * regular expression passed in parameter
     * Group:Number:COUNT(".") will count all items having a string state of one character
     * Group:Number:COUNT("[5-9]") will count all items having a string state between 5 and 9
    class Count implements GroupFunction {
        protected final Pattern pattern;
        public Count(@Nullable State regExpr) {
            if (regExpr == null) {
                throw new IllegalArgumentException("Parameter must not be null!");
            this.pattern = Pattern.compile(regExpr.toString());
                    Matcher matcher = pattern.matcher(item.getState().toString());
            return new DecimalType(count);
            return new State[] { new StringType(pattern.pattern()) };
