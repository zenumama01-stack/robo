package org.openhab.core.magic.binding.internal.firmware;
import org.openhab.core.thing.binding.firmware.FirmwareBuilder;
import org.openhab.core.thing.firmware.FirmwareProvider;
 * Provides firmware for the magic thing type for firmware update.
@Component(service = FirmwareProvider.class)
public class MagicFirmwareProvider implements FirmwareProvider {
    private final Set<Firmware> magicFirmwares = Set
            // General firmware versions for the thing type
            .of(createFirmware(null, "0.1.0", false),
                createFirmware(null, "1.0.0", false),
             // Model restricted firmware versions
                createFirmware(MagicBindingConstants.MODEL_ALOHOMORA, "1.0.1", true),
                createFirmware(MagicBindingConstants.MODEL_ALOHOMORA, "1.1.0", true),
                createFirmware(MagicBindingConstants.MODEL_COLLOPORTUS, "1.0.1", true),
                createFirmware(MagicBindingConstants.MODEL_COLLOPORTUS, "1.2.0", true),
                createFirmware(MagicBindingConstants.MODEL_LUMOS, "2.3.1", true),
                createFirmware(MagicBindingConstants.MODEL_LUMOS, "2.5.0", true)
    public @Nullable Firmware getFirmware(Thing thing, String version) {
        return getFirmware(thing, version, null);
    public @Nullable Firmware getFirmware(Thing thing, String version, @Nullable Locale locale) {
        return getFirmwares(thing, locale).stream().filter(firmware -> firmware.getVersion().equals(version))
                .findFirst().get();
    public @Nullable Set<Firmware> getFirmwares(Thing thing) {
        return getFirmwares(thing, null);
    public @Nullable Set<Firmware> getFirmwares(Thing thing, @Nullable Locale locale) {
        return magicFirmwares.stream().filter(firmware -> firmware.isSuitableFor(thing)).collect(Collectors.toSet());
    private static Firmware createFirmware(final @Nullable String model, final String version,
            boolean modelRestricted) {
        return FirmwareBuilder.create(MagicBindingConstants.THING_TYPE_FIRMWARE_UPDATE, version).withModel(model)
                .withModelRestricted(modelRestricted).build();
