 * A SwitchItem represents a normal switch that can be ON or OFF.
 * Useful for normal lights, presence detection etc.
public class SwitchItem extends GenericItem {
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(OnOffType.class, UnDefType.class);
    private static final List<Class<? extends Command>> ACCEPTED_COMMAND_TYPES = List.of(OnOffType.class,
    public SwitchItem(String name) {
        super(CoreItemFactory.SWITCH, name);
    /* package */ SwitchItem(String type, String name) {
     * Send an ON/OFF command to the item.
    public void send(OnOffType command) {
    public void send(OnOffType command, @Nullable String source) {
        if (timeSeries.getStates().allMatch(s -> s.state() instanceof OnOffType)) {
