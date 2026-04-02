 * Minimal representation of a modbus response.
 * Only function code is exposed, which allows detecting MODBUS exception codes from normal codes.
public interface ModbusResponse {
     * Function code of the response.
     * Note that in case of Slave responding with Modbus exception response, the response
     * function code might differ from request function code
     * @return function code of the response
    int getFunctionCode();
