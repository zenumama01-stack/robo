package org.openhab.core.io.transport.modbus;
 * Encapsulates result of modbus read operations
 * @author Nagy Attila Gabor - Initial contribution
public class AsyncModbusFailure<R> {
    private final R request;
    private final Exception cause;
    public AsyncModbusFailure(R request, Exception cause) {
        Objects.requireNonNull(request, "Request must not be null!");
        Objects.requireNonNull(cause, "Cause must not be null!");
        this.request = request;
        this.cause = cause;
     * Get request matching this response
     * @return request object
    public R getRequest() {
     * Get cause of error
     * @return exception representing error
    public Exception getCause() {
        return cause;
        StringBuilder builder = new StringBuilder("AsyncModbusReadResult(");
        builder.append("request = ");
        builder.append(request);
        builder.append(", error = ");
        builder.append(cause);
        builder.append(")");
        return builder.toString();
