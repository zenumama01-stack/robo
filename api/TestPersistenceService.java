import java.util.stream.LongStream;
 * A simple persistence service used for unit tests
 * @author Mark Herwege - Allow future values
 * @author Mark Herwege - Adapt test expected value logic for Riemann sums
public class TestPersistenceService implements QueryablePersistenceService {
    public static final String SERVICE_ID = "test";
    static final int SWITCH_START = -15;
    static final int SWITCH_ON_1 = -15;
    static final int SWITCH_ON_INTERMEDIATE_1 = -12;
    static final int SWITCH_OFF_1 = -10;
    static final int SWITCH_OFF_INTERMEDIATE_1 = -6;
    static final int SWITCH_ON_2 = -5;
    static final int SWITCH_ON_INTERMEDIATE_21 = -1;
    static final int SWITCH_ON_INTERMEDIATE_22 = +1;
    static final int SWITCH_OFF_2 = +5;
    static final int SWITCH_OFF_INTERMEDIATE_2 = +7;
    static final int SWITCH_ON_3 = +10;
    static final int SWITCH_ON_INTERMEDIATE_3 = +12;
    static final int SWITCH_OFF_3 = +15;
    static final int SWITCH_END = +15;
    static final OnOffType SWITCH_STATE = OnOffType.ON;
    static final int BASE_VALUE = ZonedDateTime.now().getYear(); // For reference, if year is 2025
    static final int BEFORE_START = BASE_VALUE - 85; // 1940
    static final int HISTORIC_START = BASE_VALUE - 75; // 1950
    static final int HISTORIC_INTERMEDIATE_VALUE_1 = BASE_VALUE - 20; // 2005
    static final int HISTORIC_INTERMEDIATE_VALUE_2 = BASE_VALUE - 14; // 2011
    static final int HISTORIC_END = BASE_VALUE - 13; // 2012
    static final int HISTORIC_INTERMEDIATE_NOVALUE_3 = BASE_VALUE - 6; // 2019
    static final int HISTORIC_INTERMEDIATE_NOVALUE_4 = BASE_VALUE - 4; // 2021
    static final int FUTURE_INTERMEDIATE_NOVALUE_1 = BASE_VALUE + 21; // 2051
    static final int FUTURE_INTERMEDIATE_NOVALUE_2 = BASE_VALUE + 31; // 2056
    static final int FUTURE_START = BASE_VALUE + 35; // 2060
    static final int FUTURE_INTERMEDIATE_VALUE_3 = BASE_VALUE + 45; // 2070
    static final int FUTURE_INTERMEDIATE_VALUE_4 = BASE_VALUE + 52; // 2077
    static final int FUTURE_END = BASE_VALUE + 75; // 2100
    static final int AFTER_END = BASE_VALUE + 85; // 2110
    static final DecimalType STATE = new DecimalType(HISTORIC_END);
    static final double KELVIN_OFFSET = 273.15;
    public TestPersistenceService(ItemRegistry itemRegistry) {
        if (PersistenceExtensionsTest.TEST_SWITCH.equals(filter.getItemName())) {
            ZonedDateTime nowMinusHours = now.plusHours(SWITCH_START);
            endDate = endDate != null ? endDate : now;
            beginDate = beginDate != null ? beginDate : endDate.isAfter(now) ? now : endDate.minusHours(1);
            List<HistoricItem> results = new ArrayList<>(31);
            for (int i = SWITCH_START; i <= SWITCH_END; i++) {
                final int hour = i;
                final ZonedDateTime theDate = nowMinusHours.plusHours(i - SWITCH_START);
                if (!theDate.isBefore(beginDate) && !theDate.isAfter(endDate)) {
                    results.add(new HistoricItem() {
                            return OnOffType.from(hour < SWITCH_OFF_1 || (hour >= SWITCH_ON_2 && hour < SWITCH_OFF_2)
                                    || hour >= SWITCH_ON_3);
                            return Objects.requireNonNull(filter.getItemName());
            if (filter.getOrdering() == Ordering.DESCENDING) {
                Collections.reverse(results);
            Stream<HistoricItem> stream = results.stream();
            int startValue = HISTORIC_START;
            int endValue = FUTURE_END;
            if (beginDate != null && beginDate.getYear() >= startValue) {
                startValue = beginDate.getYear();
            if (endDate != null && endDate.getYear() <= endValue) {
                endValue = endDate.getYear();
            if (endValue <= startValue) {
            List<HistoricItem> results = new ArrayList<>(endValue - startValue);
            for (int i = startValue; i <= endValue; i++) {
                if (i > HISTORIC_END && i < FUTURE_START) {
                        Item item = itemRegistry.get(Objects.requireNonNull(filter.getItemName()));
                        Item baseItem = item;
                        if (baseItem instanceof GroupItem groupItem) {
                            baseItem = groupItem.getBaseItem();
                        Unit<?> unit = baseItem instanceof NumberItem ni ? ni.getUnit() : null;
                        return unit == null ? new DecimalType(year) : QuantityType.valueOf(year, unit);
    static OnOffType switchValue(int hour) {
        return (hour >= SWITCH_ON_1 && hour < SWITCH_OFF_1) || (hour >= SWITCH_ON_2 && hour < SWITCH_OFF_2)
                || (hour >= SWITCH_ON_3 && hour < SWITCH_OFF_3) ? OnOffType.ON : OnOffType.OFF;
    static DecimalType value(long year) {
        return value(year, false);
    private static DecimalType value(long year, boolean kelvinOffset) {
        if (year < HISTORIC_START) {
            return DecimalType.ZERO;
        } else if (year <= HISTORIC_END) {
            return new DecimalType(year + (kelvinOffset ? KELVIN_OFFSET : 0));
        } else if (year < FUTURE_START) {
            return new DecimalType(HISTORIC_END + (kelvinOffset ? KELVIN_OFFSET : 0));
        } else if (year <= FUTURE_END) {
            return new DecimalType(FUTURE_END + (kelvinOffset ? KELVIN_OFFSET : 0));
    static double testRiemannSum(@Nullable Integer beginYear, @Nullable Integer endYear, RiemannType type) {
        return testRiemannSum(beginYear, endYear, type, false);
    static double testRiemannSumCelsius(@Nullable Integer beginYear, @Nullable Integer endYear, RiemannType type) {
        return testRiemannSum(beginYear, endYear, type, true);
    private static double testRiemannSum(@Nullable Integer beginYear, @Nullable Integer endYear, RiemannType type,
            boolean kelvinOffset) {
        int begin = beginYear != null ? (beginYear < HISTORIC_START ? HISTORIC_START : beginYear) : now.getYear() + 1;
        int end = endYear != null ? endYear : now.getYear();
        double sum = 0;
        int index = begin;
        long duration = 0;
        long nextDuration = 0;
                if (beginYear == null) {
                    duration = Duration
                            .between(now, ZonedDateTime.of(now.getYear() + 1, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()))
                            .toSeconds();
                while (index < end) {
                    int bucketStart = index;
                    double value = value(index, kelvinOffset).doubleValue();
                    while ((index < end - 1) && (value(index).longValue() == value(index + 1).longValue())) {
                    duration += Duration
                            .between(ZonedDateTime.of(bucketStart, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()),
                                    ZonedDateTime.of(index, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()))
                    if (endYear == null && index == end) {
                                .between(ZonedDateTime.of(now.getYear(), 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()), now)
                    sum += value * duration;
                    duration = 0;
                    value = (value + value(index, kelvinOffset).doubleValue()) / 2.0;
                int nextIndex = begin;
                boolean startBucket = true;
                double startValue = value(begin, kelvinOffset).doubleValue();
                    duration = Duration.between(now, ZonedDateTime.of(begin, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()))
                while (index < end - 1 && nextIndex < end) {
                    if (startBucket) {
                        sum += startValue * duration / 2.0;
                        startBucket = false;
                    bucketStart = index;
                    nextIndex = index;
                    while ((nextIndex < end - 1)
                            && (value(nextIndex).longValue() == value(nextIndex + 1).longValue())) {
                        nextIndex++;
                    nextDuration = Duration
                                    ZonedDateTime.of(nextIndex, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault()))
                    if (endYear == null && nextIndex == end) {
                        nextDuration += Duration
                    sum += value * (duration + nextDuration) / 2.0;
                double endValue = value(end, kelvinOffset).doubleValue();
                long endDuration = nextDuration;
                sum += endValue * endDuration / 2.0;
    static double testAverage(@Nullable Integer beginYear, @Nullable Integer endYear) {
        ZonedDateTime beginDate = beginYear != null
                ? ZonedDateTime.of(beginYear >= HISTORIC_START ? beginYear : HISTORIC_START, 1, 1, 0, 0, 0, 0,
                        ZoneId.systemDefault())
                : now;
        ZonedDateTime endDate = endYear != null ? ZonedDateTime.of(endYear, 1, 1, 0, 0, 0, 0, ZoneId.systemDefault())
        double sum = testRiemannSum(beginYear, endYear, RiemannType.LEFT);
        long duration = Duration.between(beginDate, endDate).toSeconds();
        return 1.0 * sum / duration;
    static double testMedian(@Nullable Integer beginYear, @Nullable Integer endYear) {
        int begin = beginYear != null ? beginYear : now.getYear() + 1;
        long[] values = LongStream.range(begin, end + 1)
                .filter(v -> ((v >= HISTORIC_START && v <= HISTORIC_END) || (v >= FUTURE_START && v <= FUTURE_END)))
                .sorted().toArray();
        int length = values.length;
        if (length % 2 == 1) {
            return values[values.length / 2];
            return 0.5 * (values[values.length / 2] + values[values.length / 2 - 1]);
