 * The {@link PersistenceTimeFilterTest} contains tests for {@link PersistenceTimeFilter}
public class PersistenceTimeFilterTest {
    public void testTimeFilter() throws InterruptedException {
        PersistenceFilter filter = new PersistenceTimeFilter("test", 1, "s");
        StringItem item = new StringItem("testItem");
        assertThat(filter.apply(item), is(true));
        filter.persisted(item);
        // immediate store returns false
        assertThat(filter.apply(item), is(false));
        // after interval returns true
        Thread.sleep(1500);
