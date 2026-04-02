 * Exception representing situation where transaction id of the response does not match request
public class ModbusUnexpectedTransactionIdException extends ModbusTransportException {
    private static final long serialVersionUID = -2453232634024813933L;
    private int requestId;
    private int responseId;
    public ModbusUnexpectedTransactionIdException(int requestId, int responseId) {
        this.requestId = requestId;
        this.responseId = responseId;
        return String.format("Transaction id of request (%d) does not equal response (%d). Slave response is invalid.",
                requestId, responseId);
                "ModbusUnexpectedTransactionIdException(requestTransactionId=%d, responseTransactionId=%d)", requestId,
                responseId);
    public int getRequestId() {
    public int getResponseId() {
        return responseId;
