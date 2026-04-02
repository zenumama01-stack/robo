 * A simple persistence service working with cached HistoricItems used for unit tests.
 * @author Florian Binder - Initial contribution
public class TestCachedValuesPersistenceService implements ModifiablePersistenceService {
    public static final String ID = "testCachedHistoricItems";
    private final List<HistoricItem> historicItems = new ArrayList<>();
    public TestCachedValuesPersistenceService() {
        return ID;
    public void store(Item item) {
        store(item, null);
    public void store(Item item, @Nullable String alias) {
        store(item, ZonedDateTime.now(), item.getState(), alias);
    public void store(Item item, ZonedDateTime date, State state) {
        store(item, date, state, null);
    public void store(Item item, ZonedDateTime date, State state, @Nullable String alias) {
        historicItems.add(new CachedHistoricItem(date, state, alias != null ? alias : item.getName()));
    public boolean remove(FilterCriteria filter) throws IllegalArgumentException {
        return historicItems.removeAll(StreamSupport.stream(query(filter).spliterator(), false).toList());
    public Iterable<HistoricItem> query(FilterCriteria filter) {
        Stream<HistoricItem> stream = historicItems.stream();
        if (filter.getState() != null) {
            throw new UnsupportedOperationException("state filtering is not supported yet");
            stream = stream.filter(hi -> itemName.equals(hi.getName()));
        ZonedDateTime beginDate = filter.getBeginDate();
        if (beginDate != null) {
            stream = stream.filter(hi -> !beginDate.isAfter(hi.getTimestamp()));
        ZonedDateTime endDate = filter.getEndDate();
        if (endDate != null) {
            stream = stream.filter(hi -> !endDate.isBefore(hi.getTimestamp()));
        if (filter.getOrdering() == Ordering.ASCENDING) {
            stream = stream.sorted(((o1, o2) -> o1.getTimestamp().compareTo(o2.getTimestamp())));
        } else if (filter.getOrdering() == Ordering.DESCENDING) {
            stream = stream.sorted(((o1, o2) -> -o1.getTimestamp().compareTo(o2.getTimestamp())));
        if (filter.getPageNumber() > 0) {
            stream = stream.skip(filter.getPageSize() * filter.getPageNumber());
        if (filter.getPageSize() != Integer.MAX_VALUE) {
            stream = stream.limit(filter.getPageSize());
        return stream.toList();
        return "Test Label";
    public List<PersistenceStrategy> getSuggestedStrategies() {
    private static class CachedHistoricItem implements HistoricItem {
        private final State state;
        public CachedHistoricItem(ZonedDateTime timestamp, State state, String name) {
            return "CachedHistoricItem [timestamp=" + timestamp + ", state=" + state + ", name=" + name + "]";
