import org.openhab.core.io.transport.modbus.ModbusFailureCallback;
import org.openhab.core.io.transport.modbus.ModbusReadCallback;
import org.openhab.core.io.transport.modbus.ModbusReadRequestBlueprint;
import org.openhab.core.io.transport.modbus.PollTask;
 * Implementation of {@link PollTask} that differentiates tasks using endpoint, request and callbacks.
 * Note: Two differentiate poll tasks are considered unequal if their callbacks are unequal.
 * HashCode and equals should be defined such that two poll tasks considered the same only if their request,
 * maxTries, endpoint and callback are the same.
public class BasicPollTask implements PollTask {
    private ModbusReadRequestBlueprint request;
    private ModbusReadCallback resultCallback;
    private ModbusFailureCallback<ModbusReadRequestBlueprint> failureCallback;
    public BasicPollTask(ModbusSlaveEndpoint endpoint, ModbusReadRequestBlueprint request,
            ModbusReadCallback resultCallback, ModbusFailureCallback<ModbusReadRequestBlueprint> failureCallback) {
        this.resultCallback = resultCallback;
        this.failureCallback = failureCallback;
    public ModbusReadCallback getResultCallback() {
        return resultCallback;
    public ModbusFailureCallback<ModbusReadRequestBlueprint> getFailureCallback() {
        return failureCallback;
        return Objects.hash(request, getEndpoint(), getResultCallback(), getFailureCallback());
        return "BasicPollTask [getEndpoint=" + getEndpoint() + ", request=" + request + ", getResultCallback()="
                + getResultCallback() + ", getFailureCallback()=" + getFailureCallback() + "]";
        BasicPollTask rhs = (BasicPollTask) obj;
        return Objects.equals(request, rhs.request) && Objects.equals(getEndpoint(), rhs.getEndpoint())
                && Objects.equals(getResultCallback(), rhs.getResultCallback())
                && Objects.equals(getFailureCallback(), rhs.getFailureCallback());
