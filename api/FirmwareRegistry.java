 * The {@link FirmwareRegistry} is registered as an OSGi service and is responsible for tracking all
 * {@link FirmwareProvider}s. For this reason it is the central instance to get access to all available firmwares. If a
 * locale is given to one of its operations then the following firmware attributes are localized:
 * @author Dimitar Ivanov - Extracted interface
public interface FirmwareRegistry {
     * Returns the firmware for the given thing and firmware version by using the locale provided by the
     * {@link LocaleProvider}.
     * @param thing the thing for which the firmwares are to be retrieved (not null)
     * @param firmwareVersion the version of the firmware to be retrieved (not null)
     * @return the corresponding firmware or null if no firmware was found
     * @throws IllegalArgumentException if the thing is null; if the firmware version is null or empty
    Firmware getFirmware(Thing thing, String firmwareVersion);
     * Returns the firmware for the given thing, firmware version and locale.
     * @param locale the locale to be used (if null then the locale provided by the {@link LocaleProvider} is used)
    Firmware getFirmware(Thing thing, String firmwareVersion, Locale locale);
     * Returns the latest firmware for the given thing, using the locale provided by the {@link LocaleProvider}.
     * @return the corresponding latest firmware or null if no firmware was found
     * @throws IllegalArgumentException if the thing is null
    Firmware getLatestFirmware(Thing thing);
     * Returns the latest firmware for the given thing and locale.
    Firmware getLatestFirmware(Thing thing, @Nullable Locale locale);
     * Returns the collection of available firmwares for the given thing using the locale provided by the
     * {@link LocaleProvider}. The collection is sorted in descending order, i.e. the latest firmware will be the first
     * element in the collection.
     * @return the collection of available firmwares for the given thing (not null)
    Collection<Firmware> getFirmwares(Thing thing);
     * Returns the collection of available firmwares for the given thing and locale. The collection is
     * sorted in descending order, i.e. the latest firmware will be the first element in the collection.
    Collection<Firmware> getFirmwares(Thing thing, @Nullable Locale locale);
