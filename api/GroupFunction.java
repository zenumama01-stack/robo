 * Group functions are used by active group items to calculate a state for the group
 * out of the states of all its member items.
public interface GroupFunction {
    String EQUALITY = "EQUALITY";
    String AND = "AND";
    String OR = "OR";
    String NAND = "NAND";
    String NOR = "NOR";
    String XOR = "XOR";
    String COUNT = "COUNT";
    String AVG = "AVG";
    String MEDIAN = "MEDIAN";
    String SUM = "SUM";
    String MIN = "MIN";
    String MAX = "MAX";
    String LATEST = "LATEST";
    String EARLIEST = "EARLIEST";
    String DEFAULT = EQUALITY;
    Set<String> VALID_FUNCTIONS = Set.of( //
            EQUALITY, AND, OR, NAND, NOR, XOR, COUNT, AVG, MEDIAN, SUM, MIN, MAX, LATEST, EARLIEST //
     * Determines the current state of a group based on a list of items
     * @param items the items to calculate a group state for
     * @return the calculated group state
    State calculate(@Nullable Set<Item> items);
     * Calculates the group state and returns it as a state of the requested type.
     * @param stateClass the type in which the state should be returned
     * @return the calculated group state of the requested type or null, if type is not supported
    <T extends State> T getStateAs(@Nullable Set<Item> items, Class<T> stateClass);
     * Returns the parameters of the function as an array.
     * @return the parameters of this function
    State[] getParameters();
     * This is the default group function that does nothing else than to check if all member items
     * have the same state. If this is the case, this state is returned, otherwise UNDEF is returned.
    class Equality implements GroupFunction {
        public State calculate(@Nullable Set<Item> items) {
                Iterator<Item> it = items.iterator();
                State state = it.next().getState();
                    if (!state.equals(it.next().getState())) {
        public @Nullable <T extends State> T getStateAs(@Nullable Set<Item> items, Class<T> stateClass) {
            State state = calculate(items);
            if (stateClass.isInstance(state)) {
                return stateClass.cast(state);
        public State[] getParameters() {
            return new State[0];
