package org.openhab.core.io.transport.modbus.json;
import org.openhab.core.io.transport.modbus.ModbusConstants;
import org.openhab.core.io.transport.modbus.ModbusWriteFunctionCode;
 * Utilities for converting JSON to {@link ModbusWriteRequestBlueprint}
public final class WriteRequestJsonUtilities {
     * Constant for the function code key in the JSON
    public static final String JSON_FUNCTION_CODE = "functionCode";
     * Constant for the write address key in the JSON
    public static final String JSON_ADDRESS = "address";
     * Constant for the value key in the JSON
    public static final String JSON_VALUE = "value";
     * Constant for the maxTries key in the JSON
    public static final String JSON_MAX_TRIES = "maxTries";
     * Default maxTries when it has not been specified
    public static final int DEFAULT_MAX_TRIES = 3;
    private WriteRequestJsonUtilities() {
     * Parse JSON string to collection of {@link ModbusWriteRequestBlueprint}
     * JSON string should represent a JSON array, with JSON objects. Each JSON object represents a write request. The
     * JSON object must have the following keys
     * - functionCode: numeric function code
     * - address: reference or start address of the write
     * - value: array of data to be written. Use zero and one when writing coils. With registers, each number
     * corresponds to register's 16 bit data.
     * - maxTries: number of tries with the write in case of errors
     * @param unitId unit id for the constructed {@link ModbusWriteRequestBlueprint}
     * @param jsonString json to be parsed in string format
     * @return collection of {@link ModbusWriteRequestBlueprint} representing the json
     * @throws IllegalArgumentException in case of unexpected function codes, or too large payload exceeding modbus
     *             protocol specification
     * @throws IllegalStateException in case of parsing errors and unexpected json structure
     * @see WriteRequestJsonUtilities#JSON_FUNCTION_CODE
     * @see WriteRequestJsonUtilities#JSON_ADDRESS
     * @see WriteRequestJsonUtilities#JSON_VALUE
     * @see WriteRequestJsonUtilities#JSON_MAX_TRIES
    public static Collection<ModbusWriteRequestBlueprint> fromJson(int unitId, String jsonString) {
        JsonArray jsonArray = JsonParser.parseString(jsonString).getAsJsonArray();
        if (jsonArray.isEmpty()) {
            return new LinkedList<>();
        Deque<ModbusWriteRequestBlueprint> writes = new LinkedList<>();
        jsonArray.forEach(writeElem -> {
            writes.add(constructBluerint(unitId, writeElem));
        return writes;
    private static ModbusWriteRequestBlueprint constructBluerint(int unitId, JsonElement arrayElement) {
        final JsonObject writeObject;
            writeObject = arrayElement.getAsJsonObject();
            throw new IllegalStateException("JSON array contained something else than a JSON object!", e);
        JsonElement functionCode = writeObject.get(JSON_FUNCTION_CODE);
        JsonElement address = writeObject.get(JSON_ADDRESS);
        JsonElement maxTries = writeObject.get(JSON_MAX_TRIES);
        JsonArray valuesElem;
            valuesElem = writeObject.get(JSON_VALUE).getAsJsonArray();
            throw new IllegalStateException(String.format("JSON object '%s' is not an JSON array!", JSON_VALUE), e);
        return constructBluerint(unitId, functionCode, address, maxTries, valuesElem);
    private static ModbusWriteRequestBlueprint constructBluerint(int unitId, @Nullable JsonElement functionCodeElem,
            @Nullable JsonElement addressElem, @Nullable JsonElement maxTriesElem, @Nullable JsonArray valuesElem) {
        int functionCodeNumeric;
        if (functionCodeElem == null || functionCodeElem.isJsonNull()) {
            throw new IllegalStateException(String.format("Value for '%s' is invalid", JSON_FUNCTION_CODE));
            functionCodeNumeric = functionCodeElem.getAsInt();
        } catch (ClassCastException | IllegalStateException e) {
            throw new IllegalStateException(String.format("Value for '%s' is invalid", JSON_FUNCTION_CODE), e);
        ModbusWriteFunctionCode functionCode = ModbusWriteFunctionCode.fromFunctionCode(functionCodeNumeric);
        int address;
        if (addressElem == null || addressElem.isJsonNull()) {
            throw new IllegalStateException(String.format("Value for '%s' is invalid", JSON_ADDRESS));
            address = addressElem.getAsInt();
            throw new IllegalStateException(String.format("Value for '%s' is invalid", JSON_ADDRESS), e);
        int maxTries;
        if (maxTriesElem == null || maxTriesElem.isJsonNull()) {
            // Go with default
            maxTries = DEFAULT_MAX_TRIES;
                maxTries = maxTriesElem.getAsInt();
                throw new IllegalStateException(String.format("Value for '%s' is invalid", JSON_MAX_TRIES), e);
        if (valuesElem == null || valuesElem.isJsonNull()) {
            throw new IllegalArgumentException(String.format("Expecting non-null value, got: %s", valuesElem));
        switch (functionCode) {
                if (valuesElem.size() != 1) {
                            .format("Expecting single value with functionCode=%s, got: %s", functionCode, valuesElem));
                // fall-through to WRITE_MULTIPLE_COILS
                if (valuesElem.isEmpty()) {
                } else if (valuesElem.size() > ModbusConstants.MAX_BITS_WRITE_COUNT) {
                            String.format("Trying to write too many coils (%d). Maximum is %s", valuesElem.size(),
                                    ModbusConstants.MAX_BITS_WRITE_COUNT));
                BitArray bits = new BitArray(valuesElem.size());
                for (int i = 0; i < valuesElem.size(); i++) {
                    bits.setBit(i, valuesElem.get(i).getAsInt() != 0);
                return new ModbusWriteCoilRequestBlueprint(unitId, address, bits, !writeSingle.get(), maxTries);
                // fall-through to WRITE_MULTIPLE_REGISTERS
            case WRITE_MULTIPLE_REGISTERS: {
                int[] registers = new int[valuesElem.size()];
                } else if (valuesElem.size() > ModbusConstants.MAX_REGISTERS_WRITE_COUNT) {
                            String.format("Trying to write too many registers (%d). Maximum is %s", valuesElem.size(),
                                    ModbusConstants.MAX_REGISTERS_WRITE_COUNT));
                    registers[i] = valuesElem.get(i).getAsInt();
                return new ModbusWriteRegisterRequestBlueprint(unitId, address, new ModbusRegisterArray(registers),
                        !writeSingle.get(), maxTries);
                throw new IllegalArgumentException("Unknown function code");
