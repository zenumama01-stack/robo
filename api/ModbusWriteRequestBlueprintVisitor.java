 * ModbusWriteRequestBlueprintVisitor interface.
public interface ModbusWriteRequestBlueprintVisitor {
     * Visit request writing coil data
     * @param blueprint
    void visit(ModbusWriteCoilRequestBlueprint blueprint);
     * Visit request writing register data
    void visit(ModbusWriteRegisterRequestBlueprint blueprint);
