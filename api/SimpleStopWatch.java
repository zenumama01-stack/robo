import org.openhab.core.io.transport.modbus.internal.ModbusManagerImpl.PollTaskUnregistered;
 * Implementation of simple stop watch.
public class SimpleStopWatch {
    private volatile long totalMillis;
    private volatile long resumed;
    public interface SupplierWithPollTaskUnregisteredException<T> {
        T get() throws ModbusManagerImpl.PollTaskUnregistered;
    public interface RunnableWithModbusException {
        void run() throws ModbusException;
     * Resume or start the stop watch
     * @throws IllegalStateException if stop watch is running already
    public synchronized void resume() {
        if (isRunning()) {
            throw new IllegalStateException("Cannot suspend a running StopWatch");
        resumed = System.currentTimeMillis();
     * Suspend the stop watch
     * @throws IllegalStateException if stop watch has not been resumed
    public synchronized void suspend() {
        if (!isRunning()) {
            throw new IllegalStateException("Cannot suspend non-running StopWatch");
        totalMillis += System.currentTimeMillis() - resumed;
        resumed = 0;
     * Get total running time of this StopWatch in milliseconds
     * @return total running time in milliseconds
    public synchronized long getTotalTimeMillis() {
        return totalMillis;
     * Tells whether this StopWatch is now running
     * @return boolean telling whether this StopWatch is running
        return resumed > 0;
     * Time single action using this StopWatch
     * First StopWatch is resumed, then action is applied. Finally the StopWatch is suspended.
     * @param supplier action to time
     * @return return value from supplier
     * @throws PollTaskUnregistered when original supplier throws the exception
    public <R> R timeSupplierWithPollTaskUnregisteredException(SupplierWithPollTaskUnregisteredException<R> supplier)
            throws PollTaskUnregistered {
            this.resume();
            this.suspend();
    public <R> R timeSupplier(Supplier<R> supplier) {
     * @param action action to time
     * @throws ModbusException when original action throws the exception
    public void timeRunnableWithModbusException(RunnableWithModbusException action) throws ModbusException {
            action.run();
     * @param runnable action to time
    public void timeRunnable(Runnable runnable) {
     * @param consumer action to time
    public <T> void timeConsumer(Consumer<T> consumer, T parameter) {
            consumer.accept(parameter);
