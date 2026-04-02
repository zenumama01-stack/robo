package org.openhab.core.io.transport.modbus.internal.pooling;
import org.apache.commons.pool2.PooledObject;
import org.apache.commons.pool2.impl.EvictionConfig;
import org.openhab.core.io.transport.modbus.internal.pooling.ModbusSlaveConnectionFactoryImpl.PooledConnection;
 * Eviction policy, i.e. policy for deciding when to close idle, unused connections.
 * Connections are evicted according to {@link PooledConnection} maybeResetConnection method.
public class ModbusSlaveConnectionEvictionPolicy implements EvictionPolicy<ModbusSlaveConnection> {
    public boolean evict(EvictionConfig config, PooledObject<ModbusSlaveConnection> underTest, int idleCount) {
        return ((PooledConnection) underTest).maybeResetConnection("evict");
