 * Implementation of the {@link ReadyService} interface.
public class ReadyServiceImpl implements ReadyService {
    private final Logger logger = LoggerFactory.getLogger(ReadyServiceImpl.class);
    private static final ReadyMarkerFilter ANY = new ReadyMarkerFilter();
    private final Set<ReadyMarker> markers = new CopyOnWriteArraySet<>();
    private final Map<ReadyTracker, ReadyMarkerFilter> trackers = new HashMap<>();
    private final ReentrantReadWriteLock rwlTrackers = new ReentrantReadWriteLock(true);
    public void markReady(ReadyMarker readyMarker) {
        rwlTrackers.readLock().lock();
            boolean isNew = markers.add(readyMarker);
                notifyTrackers(readyMarker, tracker -> tracker.onReadyMarkerAdded(readyMarker));
                logger.trace("Added ready marker {}", readyMarker);
            rwlTrackers.readLock().unlock();
    public void unmarkReady(ReadyMarker readyMarker) {
            boolean isRemoved = markers.remove(readyMarker);
            if (isRemoved) {
                notifyTrackers(readyMarker, tracker -> tracker.onReadyMarkerRemoved(readyMarker));
                logger.trace("Removed ready marker {}", readyMarker);
    private void notifyTrackers(ReadyMarker readyMarker, Consumer<ReadyTracker> action) {
        trackers.entrySet().stream().filter(entry -> entry.getValue().apply(readyMarker)).map(Map.Entry::getKey)
                .forEach(action);
    public boolean isReady(ReadyMarker readyMarker) {
        return markers.contains(readyMarker);
    public void registerTracker(ReadyTracker readyTracker) {
        registerTracker(readyTracker, ANY);
    public void registerTracker(ReadyTracker readyTracker, ReadyMarkerFilter filter) {
        rwlTrackers.writeLock().lock();
            if (!trackers.containsKey(readyTracker)) {
                trackers.put(readyTracker, filter);
                notifyTracker(readyTracker, readyTracker::onReadyMarkerAdded);
            logger.error("Registering tracker '{}' failed!", readyTracker, e);
            rwlTrackers.writeLock().unlock();
    public void unregisterTracker(ReadyTracker readyTracker) {
            if (trackers.containsKey(readyTracker)) {
                notifyTracker(readyTracker, readyTracker::onReadyMarkerRemoved);
            trackers.remove(readyTracker);
    private void notifyTracker(ReadyTracker readyTracker, Consumer<ReadyMarker> action) {
        ReadyMarkerFilter f = Objects.requireNonNull(trackers.get(readyTracker));
        markers.stream().filter(f::apply).forEach(action);
