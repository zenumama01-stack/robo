 * An {@link PlayerItem} allows to control a player, e.g. an audio player.
public class PlayerItem extends GenericItem {
    private static final List<Class<? extends State>> ACCEPTED_DATA_TYPES = List.of(PlayPauseType.class,
            RewindFastforwardType.class, UnDefType.class);
    private static final List<Class<? extends Command>> ACCEPTED_COMMAND_TYPES = List.of(PlayPauseType.class,
            RewindFastforwardType.class, NextPreviousType.class, RefreshType.class);
    public PlayerItem(String name) {
        super(CoreItemFactory.PLAYER, name);
    /* package */ PlayerItem(String type, String name) {
     * Send a PLAY/PAUSE command to the item.
    public void send(PlayPauseType command) {
    public void send(PlayPauseType command, @Nullable String source) {
     * Send a REWIND/FASTFORWARD command to the item.
    public void send(RewindFastforwardType command) {
    public void send(RewindFastforwardType command, @Nullable String source) {
     * Send a NEXT/PREVIOUS command to the item.
    public void send(NextPreviousType command) {
    public void send(NextPreviousType command, @Nullable String source) {
        if (timeSeries.getStates()
                .allMatch(s -> s.state() instanceof PlayPauseType || s.state() instanceof RewindFastforwardType)) {
