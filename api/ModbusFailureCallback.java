 * Callback used to report failure in Modbus
public interface ModbusFailureCallback<R> {
     * Callback handling response with error
     * @param failure details of the failure
    void handle(AsyncModbusFailure<R> failure);
