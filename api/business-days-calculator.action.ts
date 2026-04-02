 * Action that calculates business days between two dates, accounting for weekends
 * and optionally holidays. Can also add/subtract business days from a date.
 * // Calculate business days between dates
 *   ActionName: 'Business Days Calculator',
 *     Value: 'DaysBetween'
 *     Name: 'StartDate',
 *     Value: '2024-01-01'
 *     Name: 'EndDate',
 *     Value: '2024-01-31'
 * // Add business days to a date
 *     Value: 'AddDays'
 *     Name: 'Days',
@RegisterClass(BaseAction, "__BusinessDaysCalculator")
export class BusinessDaysCalculatorAction extends BaseAction {
    // US Federal holidays (fixed dates - actual dates may vary)
    private defaultHolidays: Record<string, string[]> = {
        'US': [
            '01-01', // New Year's Day
            '07-04', // Independence Day
            '12-25', // Christmas Day
            '12-24', // Christmas Eve (many businesses closed)
            '11-11', // Veterans Day
            // Note: Some holidays like Memorial Day, Labor Day, Thanksgiving are calculated
     * Executes the business days calculation
     *   - Operation: 'DaysBetween', 'AddDays', or 'SubtractDays'
     *   - StartDate: Starting date (YYYY-MM-DD format)
     *   - EndDate: Ending date (for DaysBetween operation)
     *   - Days: Number of business days to add/subtract
     *   - ExcludeHolidays: Boolean to exclude holidays (default: true)
     *   - Country: Country code for holiday calendar (default: 'US')
     * @returns Business days calculation results
            const operationParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'operation');
            const startDateParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'startdate');
            const endDateParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'enddate');
            const daysParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'days');
            const excludeHolidaysParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'excludeholidays');
            const countryParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'country');
            if (!operationParam || !operationParam.Value) {
                    Message: "Operation parameter is required (DaysBetween, AddDays, SubtractDays)",
            if (!startDateParam || !startDateParam.Value) {
                    Message: "StartDate parameter is required",
            const operation = operationParam.Value;
            const startDate = new Date(startDateParam.Value);
            const excludeHolidays = excludeHolidaysParam?.Value !== false && excludeHolidaysParam?.Value !== 'false';
            const country = countryParam?.Value || 'US';
            if (isNaN(startDate.getTime())) {
                    Message: "Invalid StartDate format. Use YYYY-MM-DD",
                    ResultCode: "INVALID_DATE"
                case 'DaysBetween':
                    if (!endDateParam || !endDateParam.Value) {
                            Message: "EndDate parameter is required for DaysBetween operation",
                    const endDate = new Date(endDateParam.Value);
                    if (isNaN(endDate.getTime())) {
                            Message: "Invalid EndDate format. Use YYYY-MM-DD",
                    result = this.calculateDaysBetween(startDate, endDate, excludeHolidays, country);
                case 'AddDays':
                case 'SubtractDays':
                    if (!daysParam || daysParam.Value === undefined) {
                            Message: "Days parameter is required for Add/Subtract operations",
                    const days = parseInt(daysParam.Value.toString());
                    if (isNaN(days)) {
                            Message: "Days must be a valid number",
                    const daysToAdd = operation === 'AddDays' ? days : -days;
                    result = this.addBusinessDays(startDate, daysToAdd, excludeHolidays, country);
                        Message: "Invalid operation. Use: DaysBetween, AddDays, or SubtractDays",
                Message: `Failed to calculate business days: ${error instanceof Error ? error.message : String(error)}`,
    private calculateDaysBetween(startDate: Date, endDate: Date, excludeHolidays: boolean, country: string): any {
        let start = new Date(startDate);
        let end = new Date(endDate);
        const isReverse = start > end;
        if (isReverse) {
            [start, end] = [end, start];
        let businessDays = 0;
        let totalDays = 0;
        let weekends = 0;
        let holidays = 0;
        const holidayDates: string[] = [];
        const current = new Date(start);
        while (current <= end) {
            totalDays++;
            const dayOfWeek = current.getDay();
            const isWeekend = dayOfWeek === 0 || dayOfWeek === 6;
            const isHoliday = excludeHolidays && this.isHoliday(current, country);
            if (isWeekend) {
                weekends++;
            } else if (isHoliday) {
                holidays++;
                holidayDates.push(this.formatDate(current));
                businessDays++;
            current.setDate(current.getDate() + 1);
            businessDays = -businessDays;
            businessDays,
            totalDays: totalDays - 1, // Exclusive of start date
            weekends,
            holidays,
            holidayDates,
            startDate: this.formatDate(startDate),
            endDate: this.formatDate(endDate),
            includesPartialDays: false
    private addBusinessDays(startDate: Date, days: number, excludeHolidays: boolean, country: string): any {
        const result = new Date(startDate);
        const direction = days >= 0 ? 1 : -1;
        let remainingDays = Math.abs(days);
        const skippedDates: { date: string; reason: string }[] = [];
        while (remainingDays > 0) {
            result.setDate(result.getDate() + direction);
            const dayOfWeek = result.getDay();
            const isHoliday = excludeHolidays && this.isHoliday(result, country);
                skippedDates.push({
                    date: this.formatDate(result),
                    reason: dayOfWeek === 0 ? 'Sunday' : 'Saturday'
                    reason: 'Holiday'
                remainingDays--;
            businessDaysAdded: days,
            resultDate: this.formatDate(result),
            dayOfWeek: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'][result.getDay()],
            skippedDates: skippedDates.slice(-10), // Last 10 skipped dates
            totalDaysElapsed: Math.abs(Math.ceil((result.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24)))
    private isHoliday(date: Date, country: string): boolean {
        const monthDay = `${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
        // Check fixed holidays
        const holidays = this.defaultHolidays[country] || [];
        if (holidays.includes(monthDay)) {
        // Check calculated US holidays
        if (country === 'US') {
            // Memorial Day (last Monday in May)
            if (date.getMonth() === 4 && date.getDay() === 1 && date.getDate() > 24) {
            // Labor Day (first Monday in September)
            if (date.getMonth() === 8 && date.getDay() === 1 && date.getDate() <= 7) {
            // Thanksgiving (fourth Thursday in November)
            if (date.getMonth() === 10 && date.getDay() === 4 && date.getDate() >= 22 && date.getDate() <= 28) {
            // Black Friday (day after Thanksgiving)
            if (date.getMonth() === 10 && date.getDay() === 5 && date.getDate() >= 23 && date.getDate() <= 29) {
            // Martin Luther King Jr. Day (third Monday in January)
            if (date.getMonth() === 0 && date.getDay() === 1 && date.getDate() >= 15 && date.getDate() <= 21) {
            // Presidents Day (third Monday in February)
            if (date.getMonth() === 1 && date.getDay() === 1 && date.getDate() >= 15 && date.getDate() <= 21) {
    private formatDate(date: Date): string {
