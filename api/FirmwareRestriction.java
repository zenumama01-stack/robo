 * A function for defining specific installation restrictions for a given {@link Firmware}.
 * <b>Example:</b> Consider a device where:
 * <li>the firmware with version 5 must only be installed if the device currently
 * has firmware version 1 installed;
 * <li>the firmware with version 4 can only be installed if the device currently has firmware version 3 installed.
 * In such case the restrictions function can be defined as follows in the {@link FirmwareProvider}:
 *     &#64;code
 *     Firmware firmwareV5 = FirmwareBuilder.create(thingTypeUID, "5").withCustomRestrictions(
 *             // Hardware version A
 *             thing -> "1".equals(thing.getProperties().get(Thing.PROPERTY_FIRMWARE_VERSION))).build();
 *     Firmware firmwareV4 = FirmwareBuilder.create(thingTypeUID, "4").withCustomRestrictions(
 *             // Hardware version B
 *             thing -> "3".equals(thing.getProperties().get(Thing.PROPERTY_FIRMWARE_VERSION))).build();
public interface FirmwareRestriction extends Function<Thing, Boolean> {
