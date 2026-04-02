import org.openhab.core.types.PrimitiveType;
 * @author Erdoan Hadzhiyusein - Refactored to use ZonedDateTime
 * @author Jan N. Klug - add ability to use time or date only
 * @author Wouter Born - increase parsing and formatting precision
 * @author Laurent Garnier - added methods toLocaleZone and toZone
 * @author Gaël L'hopital - added ability to use second and milliseconds unix time
 * @author Jimmy Tanagra - implement Comparable
 * @author Jacob Laursen - Refactored to use {@link Instant} internally
public class DateTimeType implements PrimitiveType, State, Command, Comparable<DateTimeType> {
    // external format patterns for output
    public static final String DATE_PATTERN = "yyyy-MM-dd'T'HH:mm:ss";
    public static final String DATE_PATTERN_WITH_TZ = "yyyy-MM-dd'T'HH:mm:ssz";
    // this pattern returns the time zone in RFC822 format
    public static final String DATE_PATTERN_WITH_TZ_AND_MS = "yyyy-MM-dd'T'HH:mm:ss.SSSZ";
    public static final String DATE_PATTERN_WITH_TZ_AND_MS_GENERAL = "yyyy-MM-dd'T'HH:mm:ss.SSSz";
    public static final String DATE_PATTERN_WITH_TZ_AND_MS_ISO = "yyyy-MM-dd'T'HH:mm:ss.SSSX";
    // serialization of Date, Java 17 compatible format
    public static final String DATE_PATTERN_JSON_COMPAT = "MMM d, yyyy, h:mm:ss aaa";
    // internal patterns for parsing
    private static final String DATE_PARSE_PATTERN_WITHOUT_TZ = "yyyy-MM-dd'T'HH:mm"
            + "[:ss[.SSSSSSSSS][.SSSSSSSS][.SSSSSSS][.SSSSSS][.SSSSS][.SSSS][.SSS][.SS][.S]]";
    private static final String DATE_PARSE_PATTERN_WITH_TZ = DATE_PARSE_PATTERN_WITHOUT_TZ + "z";
    private static final String DATE_PARSE_PATTERN_WITH_TZ_RFC = DATE_PARSE_PATTERN_WITHOUT_TZ + "Z";
    private static final String DATE_PARSE_PATTERN_WITH_TZ_ISO = DATE_PARSE_PATTERN_WITHOUT_TZ + "X";
    private static final DateTimeFormatter PARSER = DateTimeFormatter.ofPattern(DATE_PARSE_PATTERN_WITHOUT_TZ);
    private static final DateTimeFormatter PARSER_TZ = DateTimeFormatter.ofPattern(DATE_PARSE_PATTERN_WITH_TZ);
    private static final DateTimeFormatter PARSER_TZ_RFC = DateTimeFormatter.ofPattern(DATE_PARSE_PATTERN_WITH_TZ_RFC);
    private static final DateTimeFormatter PARSER_TZ_ISO = DateTimeFormatter.ofPattern(DATE_PARSE_PATTERN_WITH_TZ_ISO);
    private static final Pattern DATE_PARSE_PATTERN_WITH_SPACE = Pattern
            .compile("\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}.*");
    // internal patterns for formatting
    private static final String DATE_FORMAT_PATTERN_WITH_TZ_RFC = "yyyy-MM-dd'T'HH:mm[:ss[.SSSSSSSSS]]Z";
    private static final DateTimeFormatter FORMATTER_TZ_RFC = DateTimeFormatter
            .ofPattern(DATE_FORMAT_PATTERN_WITH_TZ_RFC);
    private Instant instant;
     * Creates a new {@link DateTimeType} representing the current
     * instant from the system clock.
    public DateTimeType() {
        this(Instant.now());
     * Creates a new {@link DateTimeType} with the given value.
     * @param instant
    public DateTimeType(Instant instant) {
        this.instant = instant;
     * The time-zone information will be discarded, only the
     * resulting {@link Instant} is preserved.
     * @param zoned
    public DateTimeType(ZonedDateTime zoned) {
        instant = zoned.toInstant();
    public DateTimeType(String zonedValue) {
            // direct parsing (date and time)
                if (DATE_PARSE_PATTERN_WITH_SPACE.matcher(zonedValue).matches()) {
                    instant = parse(zonedValue.substring(0, 10) + "T" + zonedValue.substring(11));
                    instant = parse(zonedValue);
            } catch (DateTimeParseException fullDtException) {
                // time only
                    instant = parse("1970-01-01T" + zonedValue);
                } catch (DateTimeParseException timeOnlyException) {
                        long epoch = Double.valueOf(zonedValue).longValue();
                        int length = (int) (Math.log10(epoch >= 0 ? epoch : epoch * -1) + 1);
                        // Assume that below 12 digits we're in seconds
                        if (length < 12) {
                            instant = Instant.ofEpochSecond(epoch);
                            instant = Instant.ofEpochMilli(epoch);
                    } catch (NumberFormatException notANumberException) {
                        // date only
                        if (zonedValue.length() == 10) {
                            instant = parse(zonedValue + "T00:00:00");
                            instant = parse(zonedValue.substring(0, 10) + "T00:00:00" + zonedValue.substring(10));
        } catch (DateTimeParseException invalidFormatException) {
            throw new IllegalArgumentException(zonedValue + " is not in a valid format.", invalidFormatException);
     *             Get object represented as a {@link ZonedDateTime} with system
     *             default time-zone applied
     * @return a {@link ZonedDateTime} representation of the object
    public ZonedDateTime getZonedDateTime() {
        return getZonedDateTime(ZoneId.systemDefault());
     * Get object represented as a {@link ZonedDateTime} with the
     * the provided time-zone applied
    public ZonedDateTime getZonedDateTime(ZoneId zoneId) {
        return instant.atZone(zoneId);
     * Get the {@link Instant} value of the object
     * @return the {@link Instant} value of the object
        return instant;
    public static DateTimeType valueOf(String value) {
        return new DateTimeType(value);
    public String format(@Nullable String pattern) {
        return format(pattern, ZoneId.systemDefault());
    public String format(@Nullable String pattern, ZoneId zoneId) {
        ZonedDateTime zonedDateTime = instant.atZone(zoneId);
            return DateTimeFormatter.ofPattern(DATE_PATTERN).format(zonedDateTime);
        return String.format(pattern, zonedDateTime);
    public String format(Locale locale, String pattern) {
        return String.format(locale, pattern, getZonedDateTime());
        return toFullString();
    public String toFullString() {
        return toFullString(ZoneId.systemDefault());
    public String toFullString(ZoneId zoneId) {
        String formatted = instant.atZone(zoneId).format(FORMATTER_TZ_RFC);
        if (formatted.contains(".")) {
            String sign = "";
            if (formatted.contains("+")) {
                sign = "+";
            } else if (formatted.contains("-")) {
                sign = "-";
            if (!sign.isEmpty()) {
                // the formatted string contains 9 fraction-of-second digits
                // truncate at most 2 trailing groups of 000s
                return formatted.replace("000" + sign, sign).replace("000" + sign, sign);
        result = prime * result + instant.hashCode();
        DateTimeType other = (DateTimeType) obj;
        return instant.compareTo(other.instant) == 0;
    public int compareTo(DateTimeType o) {
        return instant.compareTo(o.getInstant());
    private Instant parse(String value) throws DateTimeParseException {
        ZonedDateTime date;
            date = ZonedDateTime.parse(value, PARSER_TZ_RFC);
        } catch (DateTimeParseException tzMsRfcException) {
                date = ZonedDateTime.parse(value, PARSER_TZ_ISO);
            } catch (DateTimeParseException tzMsIsoException) {
                    date = ZonedDateTime.parse(value, PARSER_TZ);
                } catch (DateTimeParseException tzException) {
                        date = ZonedDateTime.parse(value);
                        LocalDateTime localDateTime = LocalDateTime.parse(value, PARSER);
                        date = ZonedDateTime.of(localDateTime, ZoneId.systemDefault());
        return date.toInstant();
