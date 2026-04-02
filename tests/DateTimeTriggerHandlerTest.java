import java.time.ZoneOffset;
import org.openhab.core.library.items.DateTimeItem;
 * Basic test cases for {@link DateTimeTriggerHandler}
public class DateTimeTriggerHandlerTest {
    private @Mock @NonNullByDefault({}) Trigger mockTrigger;
    private @Mock @NonNullByDefault({}) ItemRegistry mockItemRegistry;
    private @Mock @NonNullByDefault({}) BundleContext mockBundleContext;
    private @Mock @NonNullByDefault({}) CronScheduler mockScheduler;
    private static final String ITEM_NAME = "myItem";
    private final DateTimeItem item = new DateTimeItem(ITEM_NAME);
    public void setup() throws ItemNotFoundException {
        when(mockItemRegistry.getItem(ITEM_NAME)).thenReturn(item);
        when(mockTrigger.getConfiguration())
                .thenReturn(new Configuration(Map.ofEntries(entry(DateTimeTriggerHandler.CONFIG_ITEM_NAME, ITEM_NAME),
                        entry(DateTimeTriggerHandler.CONFIG_TIME_ONLY, false))));
    public void testSameTimeZone() {
        ZonedDateTime zdt = ZonedDateTime.of(2022, 8, 11, 0, 0, 0, 0, ZoneId.systemDefault());
        item.setState(new DateTimeType(zdt));
        DateTimeTriggerHandler handler = new DateTimeTriggerHandler(mockTrigger, mockScheduler, mockItemRegistry,
                mockBundleContext);
        verify(mockScheduler).schedule(eq(handler), eq("0 0 0 11 8 * 2022"));
    public void testDifferentTimeZone() {
        ZonedDateTime zdt = ZonedDateTime.of(2022, 8, 11, 0, 0, 0, 0, ZoneId.systemDefault())
                .withZoneSameInstant(ZoneOffset.ofTotalSeconds(12345));
    public void testOffsetPositive() {
        ZonedDateTime zdt = ZonedDateTime.of(2024, 6, 7, 0, 0, 0, 0, ZoneId.systemDefault());
                        entry(DateTimeTriggerHandler.CONFIG_OFFSET, 10))));
        verify(mockScheduler).schedule(eq(handler), eq("10 0 0 7 6 * 2024"));
    public void testOffsetNegative() {
                        entry(DateTimeTriggerHandler.CONFIG_OFFSET, -10))));
        verify(mockScheduler).schedule(eq(handler), eq("50 59 23 6 6 * 2024"));
