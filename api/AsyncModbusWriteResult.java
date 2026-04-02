 * Encapsulates result of modbus write operations
public class AsyncModbusWriteResult {
    private final ModbusWriteRequestBlueprint request;
    private final ModbusResponse response;
    public AsyncModbusWriteResult(ModbusWriteRequestBlueprint request, ModbusResponse response) {
        Objects.requireNonNull(response, "Response must not be null!");
        this.response = response;
    public ModbusWriteRequestBlueprint getRequest() {
     * Get response
     * @return response
    public ModbusResponse getResponse() {
        StringBuilder builder = new StringBuilder("AsyncModbusWriteResult(");
        builder.append(", response = ");
        builder.append(response);
