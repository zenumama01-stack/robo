package org.openhab.core.io.transport.modbus.internal;
 * Utility for timing operations
 * @author Sami Salonen - initial contribution
public class AggregateStopWatch {
     * ID associated with this modbus operation
    final String operationId;
     * Total operation time
    final SimpleStopWatch total = new SimpleStopWatch();
     * Time for connection related actions
    final SimpleStopWatch connection = new SimpleStopWatch();
     * Time for actual the actual transaction (read/write to slave)
    final SimpleStopWatch transaction = new SimpleStopWatch();
     * Time for calling calling the callback
    final SimpleStopWatch callback = new SimpleStopWatch();
    public AggregateStopWatch() {
        this.operationId = UUID.randomUUID().toString();
     * Suspend all running stopwatches of this aggregate
    public void suspendAllRunning() {
        for (SimpleStopWatch watch : new SimpleStopWatch[] { total, connection, transaction, callback }) {
            if (watch.isRunning()) {
                watch.suspend();
        return String.format("{total: %d ms, connection: %d, transaction=%d, callback=%d}", total.getTotalTimeMillis(),
                connection.getTotalTimeMillis(), transaction.getTotalTimeMillis(), callback.getTotalTimeMillis());
