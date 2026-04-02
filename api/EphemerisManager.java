package org.openhab.core.ephemeris;
 * This service provides functionality around days of the year and is the central
 * service to be used directly by others.
 * @author Gaël L'hopital - Initial contribution
public interface EphemerisManager {
     * Tests given day status against configured weekend days
     * @param date observed day
     * @return whether the day is on weekend
    boolean isWeekend(ZonedDateTime date);
     * Tests given day status against configured dayset
     * @param daysetName name of the requested dayset, without prefix
     * @return whether the day is in the dayset
    boolean isInDayset(String daysetName, ZonedDateTime date);
     * Tests given day status
     * @return whether the day is bank holiday or not
    boolean isBankHoliday(ZonedDateTime date);
     * Tests given day status against given userfile
     * @param resource bundle resource file containing holiday definitions
    boolean isBankHoliday(ZonedDateTime date, URL resource);
     * @param filename absolute or relative path to the file on local file system
     * @throws FileNotFoundException
    boolean isBankHoliday(ZonedDateTime date, String filename) throws FileNotFoundException;
     * Get given day name from given userfile
     * @return name of the day or null if no corresponding entry
    String getBankHolidayName(ZonedDateTime date);
    String getBankHolidayName(ZonedDateTime date, URL resource);
    String getBankHolidayName(ZonedDateTime date, String filename) throws FileNotFoundException;
     * Gets the first next to come holiday in a 1 year time window
     * @param startDate first day of the time window
     * @return next coming holiday
    String getNextBankHoliday(ZonedDateTime startDate);
    String getNextBankHoliday(ZonedDateTime startDate, URL resource);
    String getNextBankHoliday(ZonedDateTime startDate, String filename) throws FileNotFoundException;
     * Gets the localized holiday description
     * @param holiday code of searched holiday
     * @return localized holiday description
    String getHolidayDescription(@Nullable String holiday);
     * Gets the number of days until searchedHoliday
     * @param from first day of the time window
     * @param searchedHoliday name of the searched holiday
     * @return difference in days, -1 if not found
    long getDaysUntil(ZonedDateTime from, String searchedHoliday);
     * Gets the number of days until searchedHoliday in user file
    long getDaysUntil(ZonedDateTime from, String searchedHoliday, URL resource);
    long getDaysUntil(ZonedDateTime from, String searchedHoliday, String filename) throws FileNotFoundException;
