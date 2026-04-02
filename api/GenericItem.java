 * The abstract base class for all items. It provides all relevant logic
 * for the infrastructure, such as publishing updates to the event bus
 * or notifying listeners.
 * @author Andre Fuechsel - Added tags
 * @author Mark Herwege - Added setState override to restore all item state information
public abstract class GenericItem implements ActiveItem {
    private final Logger logger = LoggerFactory.getLogger(GenericItem.class);
    private static final String ITEM_THREADPOOLNAME = "items";
    protected Set<StateChangeListener> listeners = new CopyOnWriteArraySet<>(
            Collections.newSetFromMap(new WeakHashMap<>()));
    protected Set<TimeSeriesListener> timeSeriesListeners = new CopyOnWriteArraySet<>(
    protected List<String> groupNames = new ArrayList<>();
    protected final String type;
    protected State state = UnDefType.NULL;
    protected @Nullable State lastState;
    protected @Nullable ZonedDateTime lastStateUpdate;
    protected @Nullable ZonedDateTime lastStateChange;
    protected @Nullable ItemStateConverter itemStateConverter;
    public GenericItem(String type, String name) {
        return state.as(typeClass);
    public @Nullable ZonedDateTime getLastStateUpdate() {
        return getName();
    public List<String> getGroupNames() {
        return List.copyOf(groupNames);
     * Adds a group name to the {@link GenericItem}.
     * @param groupItemName group item name to add
     * @throws IllegalArgumentException if groupItemName is {@code null}
    public void addGroupName(String groupItemName) {
        if (!groupNames.contains(groupItemName)) {
            groupNames.add(groupItemName);
    public void addGroupNames(String... groupItemNames) {
        for (String groupItemName : groupItemNames) {
            addGroupName(groupItemName);
    public void addGroupNames(List<String> groupItemNames) {
     * Removes a group item name from the {@link GenericItem}.
     * @param groupItemName group item name to remove
    public void removeGroupName(String groupItemName) {
        groupNames.remove(groupItemName);
     * Disposes this item. Clears all injected services and unregisters all change listeners.
     * This does not remove this item from its groups. Removing from groups should be done externally to retain the
     * member order in case this item is exchanged in a group.
        this.listeners.clear();
    public void setEventPublisher(@Nullable EventPublisher eventPublisher) {
    public void setStateDescriptionService(@Nullable StateDescriptionService stateDescriptionService) {
    public void setCommandDescriptionService(@Nullable CommandDescriptionService commandDescriptionService) {
    public void setItemStateConverter(@Nullable ItemStateConverter itemStateConverter) {
    protected void internalSend(Command command, @Nullable String source) {
        // try to send the command to the bus
        if (eventPublisher instanceof EventPublisher publisher) {
            publisher.post(ItemEventFactory.createCommandEvent(this.getName(), command, source));
     * Set a new state.
     * Subclasses may override this method in order to do necessary conversions upfront. Afterwards,
     * {@link #applyState(State, String)} should be called by classes overriding this method.
     * @param state new state of this item
     * @param source the source of the state update. See
     *            https://www.openhab.org/docs/developer/utils/events.html#the-core-events
    public void setState(State state, @Nullable String source) {
        applyState(state, source);
    public final void setState(State state) {
        setState(state, null);
     * Set a new state, lastState, lastStateUpdate and lastStateChange. This method is intended to be used for restoring
     * from persistence.
     * @param lastState last state of this item
     * @param lastStateUpdate last state update of this item
     * @param lastStateChange last state change of this item
    public void setState(State state, @Nullable State lastState, @Nullable ZonedDateTime lastStateUpdate,
            @Nullable ZonedDateTime lastStateChange, @Nullable String source) {
        State oldState = this.state;
        this.lastState = lastState != null ? lastState : this.lastState;
        this.lastStateUpdate = lastStateUpdate != null ? lastStateUpdate : this.lastStateUpdate;
        this.lastStateChange = lastStateChange != null ? lastStateChange : this.lastStateChange;
        notifyListeners(oldState, state);
        sendStateUpdatedEvent(state, lastStateUpdate, source);
        if (!oldState.equals(state)) {
            sendStateChangedEvent(state, oldState, lastStateUpdate, lastStateChange, source);
     * Sets new state, notifies listeners and sends events.
     * Classes overriding the {@link #setState(State)} method should call this method in order to actually set the
     * state, inform listeners and send the event.
    protected final void applyState(State state, @Nullable String source) {
        boolean stateChanged = !oldState.equals(state);
        if (stateChanged) {
            lastState = oldState; // update before we notify listeners
            lastStateChange = now; // update after we've notified listeners
        lastStateUpdate = now;
     * Set a new time series.
     * {@link #applyTimeSeries(TimeSeries)} should be called by classes overriding this method.
     * A time series may only contain events that are compatible with the item's internal state.
     * @param timeSeries new time series of this item
    public void setTimeSeries(TimeSeries timeSeries) {
        applyTimeSeries(timeSeries);
     * Sets new time series, notifies listeners and sends events.
     * Classes overriding the {@link #setTimeSeries(TimeSeries)} method should call this method in order to actually set
     * the time series, inform listeners and send the event.
    protected final void applyTimeSeries(TimeSeries timeSeries) {
        Set<TimeSeriesListener> clonedListeners = new CopyOnWriteArraySet<>(timeSeriesListeners);
        ExecutorService pool = ThreadPoolManager.getPool(ITEM_THREADPOOLNAME);
        clonedListeners.forEach(listener -> pool.execute(() -> {
                listener.timeSeriesUpdated(GenericItem.this, timeSeries);
                logger.warn("failed notifying listener '{}' about timeseries update of item {}: {}", listener,
                        GenericItem.this.getName(), e.getMessage(), e);
        // send event
        EventPublisher eventPublisher1 = this.eventPublisher;
        if (eventPublisher1 != null) {
            eventPublisher1.post(ItemEventFactory.createTimeSeriesUpdatedEvent(this.name, timeSeries, null));
    private void sendStateUpdatedEvent(State newState, @Nullable ZonedDateTime lastStateUpdate,
            eventPublisher1
                    .post(ItemEventFactory.createStateUpdatedEvent(this.name, newState, lastStateUpdate, source));
    private void sendStateChangedEvent(State newState, State oldState, @Nullable ZonedDateTime lastStateUpdate,
            eventPublisher1.post(ItemEventFactory.createStateChangedEvent(this.name, newState, oldState,
                    lastStateUpdate, lastStateChange, source));
     * Send a REFRESH command to the item.
    public void send(RefreshType command) {
        internalSend(command, null);
     * @param command the command to be sent
     * @param source the source of the command. See
    public void send(RefreshType command, @Nullable String source) {
        internalSend(command, source);
    protected void notifyListeners(final State oldState, final State newState) {
        // if nothing has changed, we send update notifications
        Set<StateChangeListener> clonedListeners = new CopyOnWriteArraySet<>(listeners);
            final boolean stateChanged = !newState.equals(oldState);
                    listener.stateUpdated(GenericItem.this, newState);
                        listener.stateChanged(GenericItem.this, oldState, newState);
                    logger.warn("failed notifying listener '{}' about state update of item {}: {}", listener,
            logger.warn("failed comparing oldState '{}' to newState '{}' for item {}: {}", oldState, newState,
        sb.append(getName());
        sb.append(" (");
        sb.append("Type=");
        sb.append(getClass().getSimpleName());
        sb.append("State=");
        sb.append(getState());
        sb.append("Label=");
        sb.append("Category=");
        sb.append(getCategory());
        if (!getTags().isEmpty()) {
            sb.append("Tags=[");
            sb.append(String.join(", ", getTags()));
        if (!getGroupNames().isEmpty()) {
            sb.append("Groups=[");
            sb.append(String.join(", ", getGroupNames()));
    public void addStateChangeListener(StateChangeListener listener) {
    public void removeStateChangeListener(StateChangeListener listener) {
    public void addTimeSeriesListener(TimeSeriesListener listener) {
        synchronized (timeSeriesListeners) {
            timeSeriesListeners.add(listener);
    public void removeTimeSeriesListener(TimeSeriesListener listener) {
            timeSeriesListeners.remove(listener);
        GenericItem other = (GenericItem) obj;
        if (!name.equals(other.name)) {
        return Set.copyOf(tags);
        return tags.stream().anyMatch(t -> t.equalsIgnoreCase(tag));
        tags.remove(tags.stream().filter(t -> t.equalsIgnoreCase(tag)).findFirst().orElse(tag));
    public void setCategory(@Nullable String category) {
    public @Nullable StateDescription getStateDescription() {
        return getStateDescription(null);
    public @Nullable StateDescription getStateDescription(@Nullable Locale locale) {
        if (stateDescriptionService instanceof StateDescriptionService service) {
            return service.getStateDescription(this.name, locale);
        CommandDescription commandOptions = getCommandOptions(locale);
        if (commandOptions != null) {
            return commandOptions;
        StateDescription stateDescription = getStateDescription(locale);
        if (stateDescription != null && !stateDescription.getOptions().isEmpty()) {
            return stateOptions2CommandOptions(stateDescription);
     * Tests if state is within acceptedDataTypes list or a subclass of one of them
     * @param acceptedDataTypes list of datatypes this items accepts as a state
     * @param state to be tested
     * @return true if state is an acceptedDataType or subclass thereof
    public boolean isAcceptedState(List<Class<? extends State>> acceptedDataTypes, State state) {
        return acceptedDataTypes.stream().anyMatch(clazz -> clazz.isAssignableFrom(state.getClass()));
    protected void logSetTypeError(State state) {
        logger.error("Tried to set invalid state {} ({}) on item {} of type {}, ignoring it", state,
                state.getClass().getSimpleName(), getName(), getClass().getSimpleName());
    protected void logSetTypeError(TimeSeries timeSeries) {
        logger.error("Tried to set invalid state in time series {} on item {} of type {}, ignoring it", timeSeries,
                getName(), getClass().getSimpleName());
    protected @Nullable CommandDescription getCommandOptions(@Nullable Locale locale) {
        if (commandDescriptionService instanceof CommandDescriptionService service) {
            CommandDescription commandDescription = service.getCommandDescription(this.name, locale);
    private CommandDescription stateOptions2CommandOptions(StateDescription stateDescription) {
        CommandDescriptionBuilder builder = CommandDescriptionBuilder.create();
        stateDescription.getOptions()
                .forEach(so -> builder.withCommandOption(new CommandOption(so.getValue(), so.getLabel())));
