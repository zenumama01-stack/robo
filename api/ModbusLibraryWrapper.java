import org.openhab.core.io.transport.modbus.AsyncModbusReadResult;
import org.openhab.core.io.transport.modbus.BitArray;
import org.openhab.core.io.transport.modbus.ModbusReadFunctionCode;
import org.openhab.core.io.transport.modbus.ModbusRegisterArray;
import org.openhab.core.io.transport.modbus.ModbusWriteCoilRequestBlueprint;
import org.openhab.core.io.transport.modbus.ModbusWriteRegisterRequestBlueprint;
import org.openhab.core.io.transport.modbus.ModbusWriteRequestBlueprintVisitor;
import org.openhab.core.io.transport.modbus.endpoint.ModbusSerialSlaveEndpoint;
import org.openhab.core.io.transport.modbus.endpoint.ModbusSlaveEndpointVisitor;
import org.openhab.core.io.transport.modbus.endpoint.ModbusTCPSlaveEndpoint;
import org.openhab.core.io.transport.modbus.endpoint.ModbusUDPSlaveEndpoint;
import net.wimpi.modbus.io.ModbusSerialTransaction;
import net.wimpi.modbus.io.ModbusTCPTransaction;
import net.wimpi.modbus.io.ModbusTransaction;
import net.wimpi.modbus.io.ModbusUDPTransaction;
import net.wimpi.modbus.msg.ModbusRequest;
import net.wimpi.modbus.msg.ModbusResponse;
import net.wimpi.modbus.msg.ReadCoilsRequest;
import net.wimpi.modbus.msg.ReadCoilsResponse;
import net.wimpi.modbus.msg.ReadInputDiscretesRequest;
import net.wimpi.modbus.msg.ReadInputDiscretesResponse;
import net.wimpi.modbus.msg.ReadInputRegistersRequest;
import net.wimpi.modbus.msg.ReadInputRegistersResponse;
import net.wimpi.modbus.msg.ReadMultipleRegistersRequest;
import net.wimpi.modbus.msg.ReadMultipleRegistersResponse;
import net.wimpi.modbus.msg.WriteCoilRequest;
import net.wimpi.modbus.msg.WriteMultipleCoilsRequest;
import net.wimpi.modbus.msg.WriteMultipleRegistersRequest;
import net.wimpi.modbus.msg.WriteSingleRegisterRequest;
import net.wimpi.modbus.net.SerialConnection;
import net.wimpi.modbus.net.TCPMasterConnection;
import net.wimpi.modbus.net.UDPMasterConnection;
import net.wimpi.modbus.procimg.InputRegister;
import net.wimpi.modbus.procimg.Register;
import net.wimpi.modbus.procimg.SimpleInputRegister;
import net.wimpi.modbus.util.BitVector;
 * Conversion utilities between underlying Modbus library (net.wimpi.modbus) and this transport bundle
public class ModbusLibraryWrapper {
    private static Logger getLogger() {
        return LoggerFactory.getLogger(ModbusLibraryWrapper.class);
    private static BitArray bitArrayFromBitVector(BitVector bitVector, int count) {
        boolean[] bits = new boolean[count];
            bits[i] = bitVector.getBit(i);
        return new BitArray(bits);
    private static ModbusRegisterArray modbusRegisterArrayFromInputRegisters(InputRegister[] inputRegisters) {
        int[] registers = new int[inputRegisters.length];
        for (int i = 0; i < inputRegisters.length; i++) {
            registers[i] = inputRegisters[i].getValue();
        return new ModbusRegisterArray(registers);
     * Convert the general request to Modbus library request object
     *             1) in case function code implies coil data but we have registers
     *             2) in case function code implies register data but we have coils
     *             3) in case there is no data
     *             4) in case there is too much data in case of WRITE_COIL or WRITE_SINGLE_REGISTER
     * @throws IllegalStateException unexpected function code. Implementation is lacking and this can be considered a
     * @return MODBUS library request matching the write request
    public static ModbusRequest createRequest(ModbusWriteRequestBlueprint message) {
        // ModbusRequest[] request = new ModbusRequest[1];
        AtomicReference<ModbusRequest> request = new AtomicReference<>();
        AtomicBoolean writeSingle = new AtomicBoolean(false);
        switch (message.getFunctionCode()) {
            case WRITE_COIL:
                writeSingle.set(true);
                // fall-through on purpose
            case WRITE_MULTIPLE_COILS:
                message.accept(new ModbusWriteRequestBlueprintVisitor() {
                    public void visit(ModbusWriteRegisterRequestBlueprint blueprint) {
                    public void visit(ModbusWriteCoilRequestBlueprint blueprint) {
                        BitArray coils = blueprint.getCoils();
                        if (coils.size() == 0) {
                            throw new IllegalArgumentException("Must provide at least one coil");
                        if (writeSingle.get()) {
                            if (coils.size() != 1) {
                                throw new IllegalArgumentException("Must provide single coil with WRITE_COIL");
                            request.set(new WriteCoilRequest(message.getReference(), coils.getBit(0)));
                            request.set(new WriteMultipleCoilsRequest(message.getReference(),
                                    ModbusLibraryWrapper.convertBits(coils)));
            case WRITE_SINGLE_REGISTER:
            case WRITE_MULTIPLE_REGISTERS:
                        Register[] registers = ModbusLibraryWrapper.convertRegisters(blueprint.getRegisters());
                        if (registers.length == 0) {
                            throw new IllegalArgumentException("Must provide at least one register");
                            if (blueprint.getRegisters().size() != 1) {
                                        "Must provide single register with WRITE_SINGLE_REGISTER");
                            request.set(new WriteSingleRegisterRequest(message.getReference(), registers[0]));
                            request.set(new WriteMultipleRegistersRequest(message.getReference(), registers));
                getLogger().error("Unexpected function code {}", message.getFunctionCode());
                        String.format("Unexpected function code %s", message.getFunctionCode()));
        ModbusRequest modbusRequest = request.get();
        modbusRequest.setUnitID(message.getUnitID());
        modbusRequest.setProtocolID(message.getProtocolID());
        return modbusRequest;
     * Create a fresh transaction for the given endpoint and connection
     * The retries of the transaction will be disabled.
     * @param endpoint
     * @param connection
    public static ModbusTransaction createTransactionForEndpoint(ModbusSlaveEndpoint endpoint,
            ModbusSlaveConnection connection) {
        ModbusTransaction transaction = endpoint.accept(new ModbusSlaveEndpointVisitor<>() {
            public @NonNull ModbusTransaction visit(ModbusTCPSlaveEndpoint modbusIPSlavePoolingKey) {
                ModbusTCPTransaction transaction = new ModbusTCPTransaction();
                transaction.setReconnecting(false);
            public @NonNull ModbusTransaction visit(ModbusSerialSlaveEndpoint modbusSerialSlavePoolingKey) {
                return new ModbusSerialTransaction();
            public @NonNull ModbusTransaction visit(ModbusUDPSlaveEndpoint modbusUDPSlavePoolingKey) {
                return new ModbusUDPTransaction();
        // We disable modbus library retries and handle in the Manager implementation
        transaction.setRetries(0);
        transaction.setRetryDelayMillis(0);
        if (transaction instanceof ModbusSerialTransaction serialTransaction) {
            serialTransaction.setSerialConnection((SerialConnection) connection);
        } else if (transaction instanceof ModbusUDPTransaction pTransaction) {
            pTransaction.setTerminal(((UDPMasterConnection) connection).getTerminal());
        } else if (transaction instanceof ModbusTCPTransaction pTransaction) {
            pTransaction.setConnection((TCPMasterConnection) connection);
     * Create fresh request corresponding to {@link ModbusReadRequestBlueprint}
    public static ModbusRequest createRequest(ModbusReadRequestBlueprint message) {
        ModbusRequest request;
        if (message.getFunctionCode() == ModbusReadFunctionCode.READ_COILS) {
            request = new ReadCoilsRequest(message.getReference(), message.getDataLength());
        } else if (message.getFunctionCode() == ModbusReadFunctionCode.READ_INPUT_DISCRETES) {
            request = new ReadInputDiscretesRequest(message.getReference(), message.getDataLength());
        } else if (message.getFunctionCode() == ModbusReadFunctionCode.READ_MULTIPLE_REGISTERS) {
            request = new ReadMultipleRegistersRequest(message.getReference(), message.getDataLength());
        } else if (message.getFunctionCode() == ModbusReadFunctionCode.READ_INPUT_REGISTERS) {
            request = new ReadInputRegistersRequest(message.getReference(), message.getDataLength());
            throw new IllegalArgumentException(String.format("Unexpected function code %s", message.getFunctionCode()));
        request.setUnitID(message.getUnitID());
        request.setProtocolID(message.getProtocolID());
     * Convert {@link BitArray} to {@link BitVector}
     * @param bits
    public static BitVector convertBits(BitArray bits) {
        BitVector bitVector = new BitVector(bits.size());
        IntStream.range(0, bits.size()).forEach(i -> bitVector.setBit(i, bits.getBit(i)));
        return bitVector;
     * Convert {@link ModbusRegisterArray} to array of {@link Register}
     * @param arr
    public static Register[] convertRegisters(ModbusRegisterArray arr) {
        return IntStream.range(0, arr.size()).mapToObj(i -> new SimpleInputRegister(arr.getRegister(i))).toList()
                .toArray(new Register[0]);
     * Get number of bits/registers/discrete inputs in the request.
     * @param response
    public static int getNumberOfItemsInResponse(ModbusResponse response, ModbusReadRequestBlueprint request) {
        // jamod library seems to be a bit buggy when it comes number of coils/discrete inputs in the response. Some
        // of the methods such as ReadCoilsResponse.getBitCount() are returning wrong values.
        // This is the reason we use a bit more verbose way to get the number of items in the response.
        final int responseCount;
        if (request.getFunctionCode() == ModbusReadFunctionCode.READ_COILS) {
            responseCount = ((ReadCoilsResponse) response).getCoils().size();
        } else if (request.getFunctionCode() == ModbusReadFunctionCode.READ_INPUT_DISCRETES) {
            responseCount = ((ReadInputDiscretesResponse) response).getDiscretes().size();
        } else if (request.getFunctionCode() == ModbusReadFunctionCode.READ_MULTIPLE_REGISTERS) {
            responseCount = ((ReadMultipleRegistersResponse) response).getRegisters().length;
        } else if (request.getFunctionCode() == ModbusReadFunctionCode.READ_INPUT_REGISTERS) {
            responseCount = ((ReadInputRegistersResponse) response).getRegisters().length;
            throw new IllegalArgumentException(String.format("Unexpected function code %s", request.getFunctionCode()));
        return responseCount;
     * Invoke callback with the data received
     * @param request original request
     * @param callback callback for read
     * @param response Modbus library response object
    public static void invokeCallbackWithResponse(ModbusReadRequestBlueprint request, ModbusReadCallback callback,
            ModbusResponse response) {
            getLogger().trace("Calling read response callback {} for request {}. Response was {}", callback, request,
                    response);
            // The number of coils/discrete inputs received in response are always in the multiples of 8
            // bits.
            // So even if querying 5 bits, you will actually get 8 bits. Here we wrap the data in
            // BitArrayWrappingBitVector
            // with will validate that the consumer is not accessing the "invalid" bits of the response.
            int dataItemsInResponse = getNumberOfItemsInResponse(response, request);
                BitVector bits = ((ReadCoilsResponse) response).getCoils();
                BitArray payload = bitArrayFromBitVector(bits, Math.min(dataItemsInResponse, request.getDataLength()));
                callback.handle(new AsyncModbusReadResult(request, payload));
                BitVector bits = ((ReadInputDiscretesResponse) response).getDiscretes();
                ModbusRegisterArray payload = modbusRegisterArrayFromInputRegisters(
                        ((ReadMultipleRegistersResponse) response).getRegisters());
                        ((ReadInputRegistersResponse) response).getRegisters());
                        String.format("Unexpected function code %s", request.getFunctionCode()));
            getLogger().trace("Called read response callback {} for request {}. Response was {}", callback, request,
