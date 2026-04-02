 * A StringItem can be used for any kind of string to either send or receive
 * from a device.
public class StringItem extends GenericItem {
    // UnDefType has to come before StringType, because otherwise every UNDEF state sent as a string would be
    // interpreted as a StringType
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(UnDefType.class, StringType.class,
            DateTimeType.class);
    private static final List<Class<? extends Command>> ACCEPTED_COMMAND_TYPES = List.of(StringType.class,
    public StringItem(String name) {
        super(CoreItemFactory.STRING, name);
     * Send a StringType command to the item.
    public void send(StringType command) {
    public void send(StringType command, @Nullable String source) {
        List<Class<? extends State>> list = new ArrayList<>();
        list.add(typeClass);
        State convertedState = TypeParser.parseState(list, state.toString());
        if (typeClass.isInstance(convertedState)) {
            return typeClass.cast(convertedState);
            return super.getStateAs(typeClass);
        if (timeSeries.getStates().allMatch(s -> s.state() instanceof StringType)) {
