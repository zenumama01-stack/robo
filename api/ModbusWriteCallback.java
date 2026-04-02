 * Interface for write callbacks
public interface ModbusWriteCallback extends ModbusResultCallback {
     * @param result result of the write operation
    void handle(AsyncModbusWriteResult result);
