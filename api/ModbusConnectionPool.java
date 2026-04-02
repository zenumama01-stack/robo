import org.apache.commons.pool2.KeyedPooledObjectFactory;
import org.apache.commons.pool2.impl.GenericKeyedObjectPool;
import org.apache.commons.pool2.impl.GenericKeyedObjectPoolConfig;
import net.wimpi.modbus.net.ModbusSlaveConnection;
 * Pool for modbus connections.
 * Only one connection is allowed to be active at a time.
public class ModbusConnectionPool extends GenericKeyedObjectPool<ModbusSlaveEndpoint, @Nullable ModbusSlaveConnection> {
    public ModbusConnectionPool(
            KeyedPooledObjectFactory<ModbusSlaveEndpoint, @Nullable ModbusSlaveConnection> factory) {
        super(factory, new ModbusPoolConfig());
    public void setConfig(@Nullable GenericKeyedObjectPoolConfig<@Nullable ModbusSlaveConnection> conf) {
        if (conf == null) {
        } else if (!(conf instanceof ModbusPoolConfig)) {
            throw new IllegalArgumentException("Only ModbusPoolConfig accepted!");
        super.setConfig(conf);
