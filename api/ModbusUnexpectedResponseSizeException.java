 * Exception representing situation where data length of the response does not match request
public class ModbusUnexpectedResponseSizeException extends ModbusTransportException {
    private static final long serialVersionUID = 2460907938819984483L;
    private int requestSize;
    private int responseSize;
    public ModbusUnexpectedResponseSizeException(int requestSize, int responseSize) {
        this.requestSize = requestSize;
        this.responseSize = responseSize;
        return String.format("Data length of the request (%d) does not equal response (%d). Slave response is invalid.",
                requestSize, responseSize);
        return String.format("ModbusUnexpectedResponseSizeException(requestFunctionCode=%d, responseFunctionCode=%d)",
