import org.openhab.core.ui.internal.chart.ChartServlet.PeriodBeginEnd;
import org.openhab.core.ui.internal.chart.ChartServlet.PeriodPastFuture;
public class ChartServletPeriodParamTest {
    public void convertToTemporalAmountFromNull() {
        TemporalAmount period = ChartServlet.convertToTemporalAmount(null, Duration.ZERO);
        assertNotNull(period);
        assertEquals(0, period.get(ChronoUnit.SECONDS));
    public void convertToTemporalAmountFromHours() {
        TemporalAmount period = ChartServlet.convertToTemporalAmount("h", Duration.ZERO);
        assertEquals(1 * 60 * 60, period.get(ChronoUnit.SECONDS));
        period = ChartServlet.convertToTemporalAmount("12h", Duration.ZERO);
        assertEquals(12 * 60 * 60, period.get(ChronoUnit.SECONDS));
    public void convertToTemporalAmountFromDays() {
        TemporalAmount period = ChartServlet.convertToTemporalAmount("D", Duration.ZERO);
        assertEquals(1, period.get(ChronoUnit.DAYS));
        assertEquals(0, period.get(ChronoUnit.MONTHS));
        assertEquals(0, period.get(ChronoUnit.YEARS));
        period = ChartServlet.convertToTemporalAmount("4D", Duration.ZERO);
        assertEquals(4, period.get(ChronoUnit.DAYS));
    public void convertToTemporalAmountFromWeeks() {
        TemporalAmount period = ChartServlet.convertToTemporalAmount("W", Duration.ZERO);
        assertEquals(7, period.get(ChronoUnit.DAYS));
        period = ChartServlet.convertToTemporalAmount("2W", Duration.ZERO);
        assertEquals(14, period.get(ChronoUnit.DAYS));
    public void convertToTemporalAmountFromMonths() {
        TemporalAmount period = ChartServlet.convertToTemporalAmount("M", Duration.ZERO);
        assertEquals(0, period.get(ChronoUnit.DAYS));
        assertEquals(1, period.get(ChronoUnit.MONTHS));
        period = ChartServlet.convertToTemporalAmount("3M", Duration.ZERO);
        assertEquals(3, period.get(ChronoUnit.MONTHS));
    public void convertToTemporalAmountFromYears() {
        TemporalAmount period = ChartServlet.convertToTemporalAmount("Y", Duration.ZERO);
        assertEquals(1, period.get(ChronoUnit.YEARS));
        period = ChartServlet.convertToTemporalAmount("2Y", Duration.ZERO);
        assertEquals(2, period.get(ChronoUnit.YEARS));
    public void convertToTemporalAmountFromISO8601() {
        TemporalAmount period = ChartServlet.convertToTemporalAmount("P2Y3M4D", Duration.ZERO);
        period = ChartServlet.convertToTemporalAmount("P1DT12H30M15S", Duration.ZERO);
        assertEquals(36 * 60 * 60 + 30 * 60 + 15, period.get(ChronoUnit.SECONDS));
    public void getPeriodPastFutureByDefault() {
        PeriodPastFuture period = ChartServlet.getPeriodPastFuture(null);
        assertNotNull(period.past());
        assertEquals(ChartServlet.DEFAULT_PERIOD, period.past());
        assertNull(period.future());
        period = ChartServlet.getPeriodPastFuture("-");
        assertNull(period.past());
        assertNotNull(period.future());
        assertEquals(ChartServlet.DEFAULT_PERIOD, period.future());
    public void getPeriodPastFutureWithOnlyPast() {
        Period duration = Period.ofDays(2);
        PeriodPastFuture period = ChartServlet.getPeriodPastFuture("2D");
        assertEquals(duration, period.past());
        period = ChartServlet.getPeriodPastFuture("2D-");
    public void getPeriodPastFutureWithOnlyFuture() {
        Period duration = Period.ofMonths(3);
        PeriodPastFuture period = ChartServlet.getPeriodPastFuture("-3M");
        assertEquals(duration, period.future());
    public void getPeriodPastFutureWithPastAndFuture() {
        Period duration1 = Period.ofDays(2);
        Period duration2 = Period.ofMonths(3);
        PeriodPastFuture period = ChartServlet.getPeriodPastFuture("2D-3M");
        assertEquals(duration1, period.past());
        assertEquals(duration2, period.future());
    public void getPeriodBeginEndWithBeginAndEnd() {
        ZonedDateTime now = ZonedDateTime.of(2024, 4, 9, 12, 0, 0, 0, ZoneId.systemDefault());
        ZonedDateTime begin = ZonedDateTime.of(2024, 4, 9, 11, 30, 0, 0, ZoneId.systemDefault());
        ZonedDateTime end = ZonedDateTime.of(2024, 4, 9, 13, 30, 0, 0, ZoneId.systemDefault());
        PeriodBeginEnd beginEnd = ChartServlet.getPeriodBeginEnd(begin, end, ChartServlet.getPeriodPastFuture("2D-3M"),
                now);
        assertEquals(begin, beginEnd.begin());
        assertEquals(end, beginEnd.end());
    public void getPeriodBeginEndWithBeginButNotEnd() {
        PeriodBeginEnd beginEnd = ChartServlet.getPeriodBeginEnd(begin, null, ChartServlet.getPeriodPastFuture("2D-3M"),
        assertEquals(ZonedDateTime.of(2024, 7, 11, 11, 30, 0, 0, ZoneId.systemDefault()), beginEnd.end());
        beginEnd = ChartServlet.getPeriodBeginEnd(begin, null, ChartServlet.getPeriodPastFuture("2D"), now);
        assertEquals(ZonedDateTime.of(2024, 4, 11, 11, 30, 0, 0, ZoneId.systemDefault()), beginEnd.end());
        beginEnd = ChartServlet.getPeriodBeginEnd(begin, null, ChartServlet.getPeriodPastFuture("-3M"), now);
        assertEquals(ZonedDateTime.of(2024, 7, 9, 11, 30, 0, 0, ZoneId.systemDefault()), beginEnd.end());
    public void getPeriodBeginEndWithEndButNotBegin() {
        ZonedDateTime end = ZonedDateTime.of(2024, 7, 11, 11, 30, 0, 0, ZoneId.systemDefault());
        PeriodBeginEnd beginEnd = ChartServlet.getPeriodBeginEnd(null, end, ChartServlet.getPeriodPastFuture("2D-3M"),
        assertEquals(ZonedDateTime.of(2024, 4, 9, 11, 30, 0, 0, ZoneId.systemDefault()), beginEnd.begin());
        beginEnd = ChartServlet.getPeriodBeginEnd(null, end, ChartServlet.getPeriodPastFuture("2D"), now);
        assertEquals(ZonedDateTime.of(2024, 7, 9, 11, 30, 0, 0, ZoneId.systemDefault()), beginEnd.begin());
        beginEnd = ChartServlet.getPeriodBeginEnd(null, end, ChartServlet.getPeriodPastFuture("-3M"), now);
        assertEquals(ZonedDateTime.of(2024, 4, 11, 11, 30, 0, 0, ZoneId.systemDefault()), beginEnd.begin());
    public void getPeriodBeginEndWithPeriodButNotBeginEnd() {
        PeriodBeginEnd beginEnd = ChartServlet.getPeriodBeginEnd(null, null, ChartServlet.getPeriodPastFuture("2D-3M"),
        assertEquals(ZonedDateTime.of(2024, 4, 7, 12, 0, 0, 0, ZoneId.systemDefault()), beginEnd.begin());
        assertEquals(ZonedDateTime.of(2024, 7, 9, 12, 0, 0, 0, ZoneId.systemDefault()), beginEnd.end());
        beginEnd = ChartServlet.getPeriodBeginEnd(null, null, ChartServlet.getPeriodPastFuture("2D"), now);
        assertEquals(ZonedDateTime.of(2024, 4, 9, 12, 0, 0, 0, ZoneId.systemDefault()), beginEnd.end());
        beginEnd = ChartServlet.getPeriodBeginEnd(null, null, ChartServlet.getPeriodPastFuture("-3M"), now);
        assertEquals(ZonedDateTime.of(2024, 4, 9, 12, 0, 0, 0, ZoneId.systemDefault()), beginEnd.begin());
