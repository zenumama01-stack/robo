 * Interface for read callbacks
public interface ModbusReadCallback extends ModbusResultCallback {
     * Callback handling response data
     * @param result result of the read operation
    void handle(AsyncModbusReadResult result);
