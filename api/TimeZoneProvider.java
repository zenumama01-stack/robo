 * This interface describes a provider for time zone.
 * @author Erdoan Hadzhiyusein - Initial contribution
public interface TimeZoneProvider {
     * Gets the configured time zone as {@link ZoneId} or the system default time zone if not configured properly.
     * @return the configured time zone as {@link ZoneId} or the system default time zone if not configured properly.
    ZoneId getTimeZone();
