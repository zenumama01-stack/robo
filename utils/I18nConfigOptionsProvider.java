import java.util.TimeZone;
 * A {@link ConfigOptionProvider} that provides a list of config options for the i18n service
 * @author Erdoan Hadzhiyusein - Added time zone
public class I18nConfigOptionsProvider implements ConfigOptionProvider {
    private static final String LANGUAGE = "language";
    private static final String REGION = "region";
    private static final String VARIANT = "variant";
    private static final String TIMEZONE = "timezone";
    private static final String NO_OFFSET_FORMAT = "(GMT) %s";
    private static final String NEGATIVE_OFFSET_FORMAT = "(GMT%d:%02d) %s";
    private static final String POSITIVE_OFFSET_FORMAT = "(GMT+%d:%02d) %s";
        return switch (uri.toString()) {
            case "system:i18n" -> processParamType(param, locale, locale != null ? locale : Locale.getDefault());
            case "profile:system:timestamp-offset" -> TIMEZONE.equals(param) ? processTimeZoneParam() : null;
    private @Nullable Collection<ParameterOption> processParamType(String param, @Nullable Locale locale,
            Locale translation) {
        return switch (param) {
            case LANGUAGE ->
                getAvailable(locale, l -> new ParameterOption(l.getLanguage(), l.getDisplayLanguage(translation)));
            case REGION ->
                getAvailable(locale, l -> new ParameterOption(l.getCountry(), l.getDisplayCountry(translation)));
            case VARIANT ->
                getAvailable(locale, l -> new ParameterOption(l.getVariant(), l.getDisplayVariant(translation)));
            case TIMEZONE -> processTimeZoneParam();
    private Collection<ParameterOption> processTimeZoneParam() {
        Comparator<TimeZone> byOffset = Comparator.comparingInt(TimeZone::getRawOffset);
        Comparator<TimeZone> byID = Comparator.comparing(TimeZone::getID);
        return ZoneId.getAvailableZoneIds().stream().map(TimeZone::getTimeZone).sorted(byOffset.thenComparing(byID))
                .map(tz -> new ParameterOption(tz.getID(), getTimeZoneRepresentation(tz))).toList();
    private static String getTimeZoneRepresentation(TimeZone tz) {
        long hours = TimeUnit.MILLISECONDS.toHours(tz.getRawOffset());
        long minutes = TimeUnit.MILLISECONDS.toMinutes(tz.getRawOffset()) - TimeUnit.HOURS.toMinutes(hours);
        minutes = Math.abs(minutes);
        final String result;
            result = String.format(POSITIVE_OFFSET_FORMAT, hours, minutes, tz.getID());
        } else if (hours < 0) {
            result = String.format(NEGATIVE_OFFSET_FORMAT, hours, minutes, tz.getID());
            result = String.format(NO_OFFSET_FORMAT, tz.getID());
    private Collection<ParameterOption> getAvailable(@Nullable Locale locale,
            Function<Locale, ParameterOption> mapFunction) {
        return Arrays.stream(Locale.getAvailableLocales()) //
                .map(mapFunction) //
                .distinct() //
                .filter(po -> !po.getValue().isEmpty()) //
                .sorted(Comparator.comparing(ParameterOption::getLabel)) //
