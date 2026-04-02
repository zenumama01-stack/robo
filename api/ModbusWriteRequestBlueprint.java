 * Base interface for Modbus write requests
public abstract class ModbusWriteRequestBlueprint {
     * Returns the reference of the register/coil/discrete input to to start
     * writing with this request
     * @return the reference of the register
     *         to start reading from as <tt>int</tt>.
    public abstract int getReference();
    public abstract int getUnitID();
     * Returns the function code of this
     * The function code is a 1-byte non negative
     * integer value valid in the range of 0-127.<br>
     * Function codes are ordered in conformance
     * classes their values are specified in
     * <tt>net.wimpi.modbus.Modbus</tt>.
     * @return the function code as <tt>int</tt>.
     * @see net.wimpi.modbus.Modbus
    public abstract ModbusWriteFunctionCode getFunctionCode();
     * Get maximum number of tries, in case errors occur. Should be at least 1.
    public abstract int getMaxTries();
     * Accept visitor
     * @param visitor
    public abstract void accept(ModbusWriteRequestBlueprintVisitor visitor);
