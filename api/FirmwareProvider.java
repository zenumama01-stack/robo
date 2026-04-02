 * The {@link FirmwareProvider} is registered as an OSGi service and is responsible for providing firmwares. If a locale
 * is given to one of its operations then the following firmware attributes are to be localized:
 * <li>{@link Firmware#getDescription()}</li>
 * <li>{@link Firmware#getChangelog()}</li>
 * <li>{@link Firmware#getOnlineChangelog()}</li>
 * @author Dimitar Ivanov - Firmwares are provided for thing
public interface FirmwareProvider {
     * Returns the firmware for the given thing and provided firmware version.
     * @param thing the thing for which the firmware will be provided with the specified version
     * @param version the version of the firmware to be provided for the specified thing
     * @return the corresponding firmware or <code>null</code> if no firmware was found
    Firmware getFirmware(Thing thing, String version);
     * Returns the firmware for the given thing and version for the given locale.
     * @param thing the thing for which the firmwares are to be provided (not null)
     * @param version the version of the firmware to be provided
     * @param locale the locale to be used (if null then the default locale is to be used)
     * @return the corresponding firmware for the given locale or null if no firmware was found
    Firmware getFirmware(Thing thing, String version, @Nullable Locale locale);
     * Returns the set of available firmwares for the given thing.
     * @return the set of available firmwares for the given thing (can be null)
    Set<Firmware> getFirmwares(Thing thing);
     * Returns the set of available firmwares for the given thing and the given locale.
    Set<Firmware> getFirmwares(Thing thing, @Nullable Locale locale);
