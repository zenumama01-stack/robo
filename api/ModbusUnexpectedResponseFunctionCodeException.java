 * Exception representing situation where function code of the response does not match request
public class ModbusUnexpectedResponseFunctionCodeException extends ModbusTransportException {
    private static final long serialVersionUID = 1109165449703638949L;
    private int requestFunctionCode;
    private int responseFunctionCode;
    public ModbusUnexpectedResponseFunctionCodeException(int requestFunctionCode, int responseFunctionCode) {
        this.requestFunctionCode = requestFunctionCode;
        this.responseFunctionCode = responseFunctionCode;
        return String.format("Function code of request (%d) does not equal response (%d)", requestFunctionCode,
                responseFunctionCode);
                "ModbusUnexpectedResponseFunctionCodeException(requestFunctionCode=%d, responseFunctionCode=%d)",
                requestFunctionCode, responseFunctionCode);
