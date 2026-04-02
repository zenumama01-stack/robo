import org.openhab.core.magic.binding.MagicBindingConstants;
import org.openhab.core.thing.binding.firmware.FirmwareUpdateHandler;
import org.openhab.core.thing.binding.firmware.ProgressCallback;
import org.openhab.core.thing.binding.firmware.ProgressStep;
 * Handler for firmware updatable magic things. Defines full progress sequence and simulates firmware update with small
 * delays between the steps.
 * @author Dimitar Ivanov - Initial contribution
public class MagicFirmwareUpdateThingHandler extends BaseThingHandler implements FirmwareUpdateHandler {
    private static final int STEP_DELAY = 100;
    public MagicFirmwareUpdateThingHandler(Thing thing) {
        String updateModel = (String) getThing().getConfiguration().get(MagicBindingConstants.UPDATE_MODEL_PROPERTY);
        switch (updateModel) {
            case MagicBindingConstants.MODEL_ALOHOMORA:
            case MagicBindingConstants.MODEL_COLLOPORTUS:
            case MagicBindingConstants.MODEL_LUMOS:
            case MagicBindingConstants.MODEL_NOX:
                getThing().setProperty(Thing.PROPERTY_MODEL_ID, updateModel);
    public void updateFirmware(Firmware firmware, ProgressCallback progressCallback) {
        progressCallback.defineSequence(ProgressStep.DOWNLOADING, ProgressStep.TRANSFERRING, ProgressStep.UPDATING,
                ProgressStep.REBOOTING, ProgressStep.WAITING);
        updateStatus(ThingStatus.OFFLINE, ThingStatusDetail.FIRMWARE_UPDATING, "Firmware is updating");
        progressCallback.next();
        for (int percent = 1; percent < 100; percent++) {
                Thread.sleep(STEP_DELAY);
                progressCallback.failed("Magic firmware update progress callback interrupted while sleeping", e);
            progressCallback.update(percent);
            if (percent % 20 == 0) {
        getThing().setProperty(Thing.PROPERTY_FIRMWARE_VERSION, firmware.getVersion());
        progressCallback.success();
    public void cancel() {
        // not needed for now
    public boolean isUpdateExecutable() {
