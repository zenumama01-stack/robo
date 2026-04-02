import org.mockito.invocation.InvocationOnMock;
 * @param <T>
public class ResultCaptor<T> implements Answer<T> {
    private List<T> results = new ArrayList<>();
    private LongSupplier longSupplier;
    public ResultCaptor(LongSupplier longSupplier) {
        this.longSupplier = longSupplier;
    public List<T> getAllReturnValues() {
    public @Nullable T answer(InvocationOnMock invocationOnMock) throws Throwable {
        T result = (T) invocationOnMock.callRealMethod();
        synchronized (this.results) {
            results.add(result);
        long wait = longSupplier.getAsLong();
        if (wait > 0) {
            Thread.sleep(wait);
